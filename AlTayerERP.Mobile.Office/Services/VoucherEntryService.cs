using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office.Services;

public sealed class VoucherEntryService(HttpClient httpClient, SessionStorageService sessionStorage)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<VoucherEntryReferencesDto> GetReferencesAsync(string type, CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync();
        using var request = CreateRequest(HttpMethod.Get,
            $"api/mobile/voucher-entry-references?type={Uri.EscapeDataString(type)}",
            session.AccessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "تعذر تحميل بيانات السند.", cancellationToken);

        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(raw))
            throw new InvalidOperationException("استجابة منسدلات السند فارغة من الخادم.");

        VoucherEntryReferencesDto? result;
        try
        {
            result = JsonSerializer.Deserialize<VoucherEntryReferencesDto>(raw, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"تعذر قراءة بيانات منسدلات السند: {ex.Message}");
        }

        if (result == null)
            throw new InvalidOperationException("استجابة بيانات السند غير صالحة.");

        result.Sources ??= [];
        result.Accounts ??= [];
        result.CostCenters ??= [];
        result.Currencies ??= [];
        result.Parties ??= [];
        result.PaymentMethods ??= [];
        result.OpenPeriods ??= [];

        var allCoreListsEmpty = result.Sources.Count == 0 &&
                                result.Accounts.Count == 0 &&
                                result.Currencies.Count == 0 &&
                                result.Parties.Count == 0 &&
                                result.PaymentMethods.Count == 0;

        if (allCoreListsEmpty)
        {
            throw new InvalidOperationException(
                $"لم تصل أي بيانات للمنسدلات. الشركة: {session.CompanyId}، الفرع: {session.BranchId}، السنة: {session.YearId}. " +
                $"الصناديق/البنوك: {result.Sources.Count}، الحسابات: {result.Accounts.Count}، العملات: {result.Currencies.Count}، " +
                $"الأطراف: {result.Parties.Count}، طرق السداد: {result.PaymentMethods.Count}. " +
                "تأكد أن الـAPI الذي يعمل هو آخر نسخة وأنه متصل بقاعدة altayer_erp_db.");
        }

        return result;
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

        return await response.Content.ReadFromJsonAsync<CreateMobileVoucherResultDto>(JsonOptions, cancellationToken)
               ?? throw new InvalidOperationException("استجابة حفظ السند غير صالحة.");
    }

    public async Task UpdateAsync(UpdateMobileVoucherDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Voucher_ID <= 0)
            throw new InvalidOperationException("معرف السند غير صحيح.");

        var session = await GetSessionAsync();
        dto.Branch_ID = session.BranchId.ToString();
        dto.Fiscal_Year_ID = session.YearId;

        using var request = CreateRequest(HttpMethod.Put, $"api/FinancialVoucher/{dto.Voucher_ID}", session.AccessToken);
        request.Content = JsonContent.Create(dto, options: JsonOptions);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "تعذر تعديل سند القبض.", cancellationToken);
    }

    public async Task<StoredSessionDto> GetSessionAsync() =>
        await sessionStorage.GetAsync() ?? throw new InvalidOperationException("لا توجد جلسة دخول محفوظة.");

    private static HttpRequestMessage CreateRequest(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, NoStore = true };
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
                // نستخدم الرسالة الافتراضية عندما لا تكون الاستجابة JSON.
            }
        }
        throw new InvalidOperationException(fallback);
    }
}
