using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AlTayerERP.Mobile.Office.DTOs;
using Microsoft.Extensions.DependencyInjection;

namespace AlTayerERP.Mobile.Office.Services;

public sealed class PaymentRequestService(
    HttpClient httpClient,
    SessionStorageService sessionStorage,
    IServiceProvider serviceProvider)
{
    public async Task<List<PaymentRequestListItemDto>> GetAsync(string? status = null, string? requestNo = null, CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync();
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(status)) query.Add($"status={Uri.EscapeDataString(status)}");
        if (!string.IsNullOrWhiteSpace(requestNo)) query.Add($"requestNo={Uri.EscapeDataString(requestNo.Trim())}");
        var url = "api/payment-requests" + (query.Count > 0 ? "?" + string.Join("&", query) : string.Empty);
        using var request = CreateRequest(HttpMethod.Get, url, session.AccessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "تعذر تحميل طلبات الصرف.", cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<PaymentRequestListItemDto>>(cancellationToken: cancellationToken) ?? [];
    }

    public async Task<PaymentRequestListItemDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync();
        using var request = CreateRequest(HttpMethod.Get, $"api/payment-requests/{id}", session.AccessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "تعذر تحميل تفاصيل طلب الصرف.", cancellationToken);
        return await response.Content.ReadFromJsonAsync<PaymentRequestListItemDto>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("استجابة تفاصيل طلب الصرف غير صالحة.");
    }

    public Task<PaymentRequestListItemDto> CreateAsync(CreatePaymentRequestDto dto, CancellationToken cancellationToken = default) =>
        SaveAsync(HttpMethod.Post, "api/payment-requests", dto, "تعذر حفظ طلب الصرف.", cancellationToken);

    public Task<PaymentRequestListItemDto> UpdateAsync(long id, CreatePaymentRequestDto dto, CancellationToken cancellationToken = default) =>
        SaveAsync(HttpMethod.Put, $"api/payment-requests/{id}", dto, "تعذر تحديث طلب الصرف.", cancellationToken);

    public async Task<List<PaymentVoucherSourceDto>> GetPaymentVoucherSourcesAsync(CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync();
        using var request = CreateRequest(HttpMethod.Get, "api/mobile/payment-voucher-sources", session.AccessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "تعذر تحميل الصناديق والحسابات البنكية.", cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<PaymentVoucherSourceDto>>(cancellationToken: cancellationToken) ?? [];
    }

    public async Task<CreatePaymentVoucherResponseDto> CreatePaymentVoucherAsync(long requestId, string cashAccountId, CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync();
        using var request = CreateRequest(HttpMethod.Post, $"api/payment-requests/{requestId}/create-payment-voucher", session.AccessToken);
        request.Content = JsonContent.Create(new { Cash_Account_ID = cashAccountId });
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "تعذر إنشاء سند الصرف.", cancellationToken);
        return await response.Content.ReadFromJsonAsync<CreatePaymentVoucherResponseDto>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("استجابة إنشاء سند الصرف غير صالحة.");
    }

    private async Task<PaymentRequestListItemDto> SaveAsync(HttpMethod method, string url, CreatePaymentRequestDto dto, string fallback, CancellationToken cancellationToken)
    {
        var session = await GetSessionAsync();
        using var request = CreateRequest(method, url, session.AccessToken);
        request.Content = JsonContent.Create(dto);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, fallback, cancellationToken);
        return await response.Content.ReadFromJsonAsync<PaymentRequestListItemDto>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("استجابة حفظ طلب الصرف غير صالحة.");
    }

    public Task SubmitAsync(long id, CancellationToken cancellationToken = default) =>
        PostAsync($"api/payment-requests/{id}/submit", null, "تعذر إرسال طلب الصرف للمراجعة.", cancellationToken);
    public Task ReviewAsync(long id, string reason, CancellationToken cancellationToken = default) =>
        PostAsync($"api/payment-requests/{id}/review", new { reason }, "تعذر تحويل الطلب للاعتماد.", cancellationToken);
    public Task ApproveAsync(long id, string reason, CancellationToken cancellationToken = default) =>
        PostAsync($"api/payment-requests/{id}/approve", new { reason }, "تعذر اعتماد طلب الصرف.", cancellationToken);
    public Task RejectAsync(long id, string reason, CancellationToken cancellationToken = default) =>
        PostAsync($"api/payment-requests/{id}/reject", new { reason }, "تعذر رفض طلب الصرف.", cancellationToken);
    public Task ReturnAsync(long id, string reason, CancellationToken cancellationToken = default) =>
        PostAsync($"api/payment-requests/{id}/return", new { reason }, "تعذر إعادة طلب الصرف.", cancellationToken);

    private async Task PostAsync(string url, object? body, string fallback, CancellationToken cancellationToken)
    {
        var session = await GetSessionAsync();
        using var request = CreateRequest(HttpMethod.Post, url, session.AccessToken);
        if (body != null) request.Content = JsonContent.Create(body);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, fallback, cancellationToken);
    }

    private async Task<StoredSessionDto> GetSessionAsync() =>
        await sessionStorage.GetAsync() ?? throw new PaymentRequestSessionExpiredException("انتهت جلسة الدخول. سجل الدخول من جديد.");

    private static HttpRequestMessage CreateRequest(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response, string fallback, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode) return;

        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = ToUserMessage(ExtractMessage(raw, fallback), fallback);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            sessionStorage.Clear();
            await RedirectToLoginAsync();
            throw new PaymentRequestSessionExpiredException("انتهت جلسة الدخول. تم إعادتك إلى شاشة الدخول.");
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
            throw new PaymentRequestForbiddenException(string.IsNullOrWhiteSpace(message)
                ? "لا تملك الصلاحية المطلوبة لتنفيذ هذه العملية."
                : message);

        if (response.StatusCode == HttpStatusCode.Conflict)
            throw new PaymentRequestConflictException(string.IsNullOrWhiteSpace(message)
                ? "حدث تعارض في حالة طلب الصرف. حدّث البيانات ثم أعد المحاولة."
                : message);

        throw new InvalidOperationException(message);
    }

    private async Task RedirectToLoginAsync()
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            var mainPage = serviceProvider.GetRequiredService<MainPage>();
            var navigationPage = new NavigationPage(mainPage)
            {
                FlowDirection = FlowDirection.RightToLeft,
                BarBackgroundColor = Color.FromArgb("#17324D"),
                BarTextColor = Colors.White
            };

            var window = Application.Current?.Windows.FirstOrDefault();
            if (window != null)
                window.Page = navigationPage;
        });
    }

    private static string ToUserMessage(string message, string fallback)
    {
        if (string.IsNullOrWhiteSpace(message))
            return fallback;

        if (message.Contains("PAYMENT_VOUCHER", StringComparison.OrdinalIgnoreCase) &&
            message.Contains("إعداد ترقيم", StringComparison.OrdinalIgnoreCase))
        {
            return "لا يوجد إعداد ترقيم فعال لسند الصرف. راجع إعدادات الترقيم.";
        }

        if (message.Contains("RECEIPT_VOUCHER", StringComparison.OrdinalIgnoreCase) &&
            message.Contains("إعداد ترقيم", StringComparison.OrdinalIgnoreCase))
        {
            return "لا يوجد إعداد ترقيم فعال لسند القبض. راجع إعدادات الترقيم.";
        }

        if (message.Contains("HTTP 400", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("Bad Request", StringComparison.OrdinalIgnoreCase))
        {
            return fallback;
        }

        return message;
    }

    private static string ExtractMessage(string? raw, string fallback)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return fallback;

        var text = raw.Trim();
        try
        {
            using var document = JsonDocument.Parse(text);
            if (document.RootElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var propertyName in new[] { "message", "detail", "title" })
                {
                    if (document.RootElement.TryGetProperty(propertyName, out var value) &&
                        value.ValueKind == JsonValueKind.String &&
                        !string.IsNullOrWhiteSpace(value.GetString()))
                    {
                        return value.GetString()!.Trim();
                    }
                }
            }

            if (document.RootElement.ValueKind == JsonValueKind.String)
                return document.RootElement.GetString()?.Trim() ?? fallback;
        }
        catch (JsonException)
        {
            // ليست استجابة JSON؛ نستخدم النص المختصر إن كان آمناً.
        }

        if (text.Contains("MySqlConnector", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("System.", StringComparison.OrdinalIgnoreCase) ||
            text.Contains(" at ", StringComparison.OrdinalIgnoreCase))
        {
            return fallback;
        }

        return text.Trim('"');
    }
}

public sealed class PaymentRequestSessionExpiredException(string message) : InvalidOperationException(message);
public sealed class PaymentRequestForbiddenException(string message) : InvalidOperationException(message);
public sealed class PaymentRequestConflictException(string message) : InvalidOperationException(message);
