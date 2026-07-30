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
        var normalized = type.Trim().ToUpperInvariant();
        // لسندات القبض والصرف نستخدم نقطة النهاية الخاصة بالسندات، لا قوائم طلب الصرف.
        // هذه النقطة تعتمد الشركة الفعلية للفرع وتتحقق من ارتباط الصندوق أو البنك بحساب مالي صالح.
        var url = $"api/mobile/voucher-entry-references?type={Uri.EscapeDataString(normalized)}";

        using var request = CreateRequest(HttpMethod.Get, url, session.AccessToken);

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw;
        }

        using (response)
        {
            await EnsureSuccessAsync(response, "تعذر تحميل بيانات السند.", cancellationToken);

            var raw = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(raw))
                throw new InvalidOperationException("استجابة منسدلات السند فارغة من الخادم.");

            VoucherEntryReferencesDto? result;
            try
            {
                result = JsonSerializer.Deserialize<VoucherEntryReferencesDto>(raw, JsonOptions);
            }
            catch (JsonException)
            {
                throw;
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
            result.PermissionDiagnostics ??= [];

            var allCoreListsEmpty = result.Sources.Count == 0 &&
                                    result.Accounts.Count == 0 &&
                                    result.Currencies.Count == 0 &&
                                    result.Parties.Count == 0 &&
                                    result.PaymentMethods.Count == 0;

            if (allCoreListsEmpty)
            {
                throw new InvalidOperationException("لا توجد بيانات مرجعية متاحة للسند في السياق الحالي.");
            }

            return result;
        }
    }

    public async Task<CreateMobileVoucherResultDto> CreateAsync(CreateMobileVoucherDto dto, CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync();
        dto.Branch_ID = session.BranchId;
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
        dto.Branch_ID = session.BranchId;
        dto.Fiscal_Year_ID = session.YearId;

        using var request = CreateRequest(HttpMethod.Put, $"api/FinancialVoucher/{dto.Voucher_ID}", session.AccessToken);
        request.Content = JsonContent.Create(dto, options: JsonOptions);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "تعذر تعديل السند.", cancellationToken);
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
        MobileApiErrorHandler.EnsureSuccess(response);
        await Task.CompletedTask;
    }
}
