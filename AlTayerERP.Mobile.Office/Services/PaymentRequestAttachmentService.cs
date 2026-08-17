using System.Net.Http.Headers;
using System.Net.Http.Json;
using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office.Services;

public sealed class PaymentRequestAttachmentService(HttpClient httpClient, SessionStorageService sessionStorage)
{
    public async Task<List<PaymentRequestAttachmentDto>> GetAsync(long requestId, CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync();
        using var request = CreateRequest(HttpMethod.Get, $"api/payment-requests/{requestId}/attachments", session.AccessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "تعذر تحميل مرفقات طلب الصرف.", cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<PaymentRequestAttachmentDto>>(cancellationToken: cancellationToken) ?? [];
    }

    public async Task UploadAsync(long requestId, FileResult file, CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync();
        await using var stream = await file.OpenReadAsync();
        using var content = new MultipartFormDataContent();
        using var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(file.ContentType)
            ? "application/octet-stream"
            : file.ContentType);
        content.Add(fileContent, "file", file.FileName);

        using var request = CreateRequest(HttpMethod.Post, $"api/payment-requests/{requestId}/attachments", session.AccessToken);
        request.Content = content;
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "تعذر رفع المرفق.", cancellationToken);
    }

    public async Task<string> DownloadAsync(long requestId, PaymentRequestAttachmentDto attachment, CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync();
        using var request = CreateRequest(HttpMethod.Get,
            $"api/payment-requests/{requestId}/attachments/{attachment.Payment_Request_Attachment_ID}/download",
            session.AccessToken);
        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        await EnsureSuccessAsync(response, "تعذر تنزيل المرفق.", cancellationToken);

        var safeName = Path.GetFileName(attachment.Original_File_Name);
        var targetPath = Path.Combine(FileSystem.CacheDirectory,
            $"{attachment.Payment_Request_Attachment_ID}_{safeName}");
        await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var target = File.Create(targetPath);
        await source.CopyToAsync(target, cancellationToken);
        return targetPath;
    }

    public async Task DeleteAsync(long requestId, long attachmentId, string reason, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new InvalidOperationException("سبب حذف المرفق إلزامي.");

        var session = await GetSessionAsync();
        var url = $"api/payment-requests/{requestId}/attachments/{attachmentId}?reason={Uri.EscapeDataString(reason.Trim())}";
        using var request = CreateRequest(HttpMethod.Delete, url, session.AccessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "تعذر حذف المرفق.", cancellationToken);
    }

    private async Task<StoredSessionDto> GetSessionAsync() =>
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
        var message = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(string.IsNullOrWhiteSpace(message) ? fallback : message.Trim().Trim('"'));
    }
}
