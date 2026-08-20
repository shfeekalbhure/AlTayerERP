using System.Net.Http.Headers;
using System.Net.Http.Json;
using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office.Services;

public sealed class ApprovalRequestsService(HttpClient httpClient, SessionStorageService sessionStorage)
{
    public async Task<List<ApprovalRequestListItemDto>> GetAsync(
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync();
        var url = string.IsNullOrWhiteSpace(status)
            ? "api/approval-requests"
            : $"api/approval-requests?status={Uri.EscapeDataString(status)}";
        using var request = CreateRequest(HttpMethod.Get, url, session.AccessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "تعذر تحميل طلبات الاعتماد.", cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<ApprovalRequestListItemDto>>(cancellationToken: cancellationToken) ?? [];
    }

    public async Task<ApprovalRequestListItemDto> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync();
        using var request = CreateRequest(HttpMethod.Get, $"api/approval-requests/{id}", session.AccessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "تعذر تحميل تفاصيل طلب الاعتماد.", cancellationToken);
        return await response.Content.ReadFromJsonAsync<ApprovalRequestListItemDto>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("استجابة تفاصيل طلب الاعتماد غير صالحة.");
    }

    public Task ReviewAsync(int id, string? reason = null, CancellationToken cancellationToken = default) =>
        PostAsync($"api/approval-requests/{id}/review", reason, "تعذر تحويل طلب الاعتماد إلى قيد المراجعة.", cancellationToken);

    public Task ApproveAsync(int id, string reason, CancellationToken cancellationToken = default) =>
        PostAsync($"api/approval-requests/{id}/approve", reason, "تعذر اعتماد الطلب.", cancellationToken);

    public Task RejectAsync(int id, string reason, CancellationToken cancellationToken = default) =>
        PostAsync($"api/approval-requests/{id}/reject", reason, "تعذر رفض الطلب.", cancellationToken);

    public Task ReturnAsync(int id, string reason, CancellationToken cancellationToken = default) =>
        PostAsync($"api/approval-requests/{id}/return", reason, "تعذر إعادة الطلب للتعديل.", cancellationToken);

    private async Task PostAsync(
        string url,
        string? reason,
        string fallback,
        CancellationToken cancellationToken)
    {
        var session = await GetSessionAsync();
        using var request = CreateRequest(HttpMethod.Post, url, session.AccessToken);
        request.Content = JsonContent.Create(new ApprovalDecisionDto { Reason = reason });
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, fallback, cancellationToken);
    }

    private async Task<StoredSessionDto> GetSessionAsync() =>
        await sessionStorage.GetAsync() ?? throw new InvalidOperationException("لا توجد جلسة دخول محفوظة.");

    private static HttpRequestMessage CreateRequest(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string fallback,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode) return;
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(MobileApiErrorHandler.FromPayload(raw, fallback));
    }
}
