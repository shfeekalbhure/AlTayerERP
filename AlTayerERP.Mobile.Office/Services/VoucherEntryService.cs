using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office.Services;

public sealed class VoucherEntryService(HttpClient httpClient, SessionStorageService sessionStorage)
{
    public async Task<VoucherEntryReferencesDto> GetReferencesAsync(string type, CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync();
        using var request = CreateRequest(HttpMethod.Get,
            $"api/mobile/voucher-entry-references?type={Uri.EscapeDataString(type)}",
            session.AccessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "تعذر تحميل بيانات السند.", cancellationToken);
        return await response.Content.ReadFromJsonAsync<VoucherEntryReferencesDto>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("استجابة بيانات السند غير صالحة.");
    }

    public async Task<CreateMobileVoucherResultDto> CreateAsync(CreateMobileVoucherDto dto, CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync();
        dto.Branch_ID = session.BranchId.ToString();
        dto.Fiscal_Year_ID = session.YearId;

        using var request = CreateRequest(HttpMethod.Post, "api/FinancialVoucher", session.AccessToken);
        request.Content = JsonContent.Create(dto);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "تعذر حفظ السند.", cancellationToken);

        return await response.Content.ReadFromJsonAsync<CreateMobileVoucherResultDto>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("استجابة حفظ السند غير صالحة.");
    }

    public async Task<StoredSessionDto> GetSessionAsync() =>
        await sessionStorage.GetAsync() ?? throw new InvalidOperationException("لا توجد جلسة دخول محفوظة.");

    private static HttpRequestMessage CreateRequest(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, string fallback, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode) return;
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(raw))
        {
            try
            {
                using var json = JsonDocument.Parse(raw);
                if (json.RootElement.TryGetProperty("message", out var message))
                    throw new InvalidOperationException(message.GetString() ?? fallback);
                if (json.RootElement.TryGetProperty("title", out var title))
                    throw new InvalidOperationException(title.GetString() ?? fallback);
            }
            catch (JsonException)
            {
                // نستخدم الرسالة الافتراضية.
            }
        }
        throw new InvalidOperationException(fallback);
    }
}
