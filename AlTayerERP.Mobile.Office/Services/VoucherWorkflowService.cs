using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace AlTayerERP.Mobile.Office.Services;

public sealed class VoucherWorkflowService(HttpClient httpClient, SessionStorageService sessionStorage)
{
    public Task ReviewAsync(long voucherId, string? notes = null, CancellationToken cancellationToken = default) =>
        PostAsync(voucherId, "review", new
        {
            Action_Channel = "MOBILE",
            Device_Name = DeviceInfo.Name,
            Notes = Clean(notes)
        }, "تعذر مراجعة السند.", cancellationToken);

    public Task ApproveAsync(long voucherId, string? notes = null, CancellationToken cancellationToken = default) =>
        PostAsync(voucherId, "approve", new
        {
            Action_Channel = "MOBILE",
            Device_Name = DeviceInfo.Name,
            Notes = Clean(notes)
        }, "تعذر اعتماد السند.", cancellationToken);

    public Task ReturnForCorrectionAsync(long voucherId, string reason, CancellationToken cancellationToken = default) =>
        PostAsync(voucherId, "return-for-correction", new
        {
            Reason = reason.Trim(),
            Action_Channel = "MOBILE",
            Device_Name = DeviceInfo.Name
        }, "تعذر إعادة السند للتصحيح.", cancellationToken);

    public Task CancelApprovalAsync(long voucherId, string reason, CancellationToken cancellationToken = default) =>
        PostAsync(voucherId, "cancel-approval", new
        {
            Reason = reason.Trim(),
            Action_Channel = "MOBILE",
            Device_Name = DeviceInfo.Name
        }, "تعذر إلغاء اعتماد السند.", cancellationToken);

    public Task PostVoucherAsync(long voucherId, string? notes = null, CancellationToken cancellationToken = default) =>
        PostAsync(voucherId, "post", new
        {
            Action_Channel = "MOBILE",
            Device_Name = DeviceInfo.Name,
            Notes = Clean(notes)
        }, "تعذر ترحيل السند.", cancellationToken);

    public Task UnpostAsync(long voucherId, string reason, CancellationToken cancellationToken = default) =>
        PostAsync(voucherId, "unpost", new
        {
            Reason = reason.Trim(),
            Action_Channel = "MOBILE",
            Device_Name = DeviceInfo.Name
        }, "تعذر فك ترحيل السند.", cancellationToken);

    private async Task PostAsync(long voucherId, string action, object body, string fallback, CancellationToken cancellationToken)
    {
        var session = await sessionStorage.GetAsync()
            ?? throw new InvalidOperationException("لا توجد جلسة دخول محفوظة.");

        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/FinancialVoucher/{voucherId}/{action}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        request.Content = JsonContent.Create(body);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode) return;

        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(raw))
        {
            try
            {
                using var json = JsonDocument.Parse(raw);
                if (json.RootElement.TryGetProperty("message", out var message))
                    throw new InvalidOperationException(message.GetString() ?? fallback);
                if (json.RootElement.TryGetProperty("detail", out var detail))
                    throw new InvalidOperationException(detail.GetString() ?? fallback);
                if (json.RootElement.TryGetProperty("title", out var title))
                    throw new InvalidOperationException(title.GetString() ?? fallback);
            }
            catch (JsonException)
            {
                // نستخدم الرسالة الافتراضية عندما لا تكون الاستجابة JSON.
            }
        }

        throw new InvalidOperationException(fallback);
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
