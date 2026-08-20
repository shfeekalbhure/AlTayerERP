using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office.Services;

public sealed class VoucherAttachmentService(HttpClient httpClient, SessionStorageService sessionStorage)
{
    public async Task<List<VoucherAttachmentDto>> GetAsync(long voucherId, CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync();
        using var request = CreateRequest(HttpMethod.Get, $"api/vouchers/{voucherId}/attachments", session.AccessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "تعذر تحميل مرفقات سند القبض.", cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<VoucherAttachmentDto>>(cancellationToken: cancellationToken) ?? [];
    }

    public async Task UploadAsync(long voucherId, FileResult file, string? notes, CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync();
        await using var stream = await file.OpenReadAsync();
        using var content = new MultipartFormDataContent();
        using var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
        content.Add(fileContent, "file", file.FileName);
        if (!string.IsNullOrWhiteSpace(notes)) content.Add(new StringContent(notes.Trim()), "notes");

        using var request = CreateRequest(HttpMethod.Post, $"api/vouchers/{voucherId}/attachments", session.AccessToken);
        request.Content = content;
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "تعذر رفع المرفق.", cancellationToken);
    }

    public async Task<byte[]> DownloadAsync(long voucherId, string attachmentId, CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync();
        using var request = CreateRequest(HttpMethod.Get, $"api/vouchers/{voucherId}/attachments/{Uri.EscapeDataString(attachmentId)}/download", session.AccessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "تعذر تنزيل المرفق.", cancellationToken);
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    public async Task DeleteAsync(long voucherId, string attachmentId, string reason, CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync();
        using var request = CreateRequest(HttpMethod.Delete, $"api/vouchers/{voucherId}/attachments/{Uri.EscapeDataString(attachmentId)}", session.AccessToken);
        request.Content = JsonContent.Create(new { Reason = reason.Trim() });
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "تعذر حذف المرفق.", cancellationToken);
    }

    private async Task<StoredSessionDto> GetSessionAsync() =>
        await sessionStorage.GetAsync() ?? throw new InvalidOperationException("لا توجد جلسة دخول محفوظة.");

    private static HttpRequestMessage CreateRequest(HttpMethod method, string url, string token)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
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
                if (json.RootElement.TryGetProperty("detail", out var detail))
                    throw new InvalidOperationException(detail.GetString() ?? fallback);
                if (json.RootElement.TryGetProperty("title", out var title))
                    throw new InvalidOperationException(title.GetString() ?? fallback);
            }
            catch (JsonException)
            {
                // تستخدم الرسالة الافتراضية عندما لا تكون الاستجابة JSON.
            }
        }
        throw new InvalidOperationException(fallback);
    }
}
