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

        // سند القبض يستهلك نفس Endpoint وDTO المرجعيين المستخدمين في Desktop.
        // لا نستخدم قوائم Endpoint الجوال القديم للصناديق أو الحسابات أو العملات.
        var desktopUrl = $"api/FinancialVoucherLookups?companyId={Uri.EscapeDataString(session.CompanyId)}&branchId={session.BranchId}";
        using var desktopRequest = CreateRequest(HttpMethod.Get, desktopUrl, session.AccessToken);
        using var desktopResponse = await httpClient.SendAsync(desktopRequest, cancellationToken);
        await EnsureSuccessAsync(desktopResponse, "تعذر تحميل بيانات السند.", cancellationToken);

        var lookups = await desktopResponse.Content.ReadFromJsonAsync<DesktopFinancialVoucherLookupsDto>(JsonOptions, cancellationToken)
                      ?? throw new InvalidOperationException("استجابة منسدلات سند القبض غير صالحة.");

        var voucherType = lookups.VoucherTypes.FirstOrDefault(x =>
            string.Equals(x.Voucher_Type_Code?.Trim(), normalized, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("نوع سند القبض غير مهيأ.");

        var draftStatus = lookups.VoucherStatuses.FirstOrDefault(x =>
            string.Equals(x.Voucher_Status_Code?.Trim(), "DRAFT", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("حالة المسودة غير مهيأة.");

        // Endpoint الديسكتوب لا يعيد الفترات المالية. نبقي الاستدعاء القديم مؤقتاً
        // للفترات المفتوحة فقط حتى لا نغير تصميم الشاشة أو قواعد تاريخ السند.
        var openPeriods = new List<VoucherEntryPeriodDto>();
        using (var periodRequest = CreateRequest(HttpMethod.Get,
                   $"api/mobile/voucher-entry-references?type={Uri.EscapeDataString(normalized)}",
                   session.AccessToken))
        using (var periodResponse = await httpClient.SendAsync(periodRequest, cancellationToken))
        {
            await EnsureSuccessAsync(periodResponse, "تعذر تحميل الفترة المالية المفتوحة.", cancellationToken);
            var periodResult = await periodResponse.Content.ReadFromJsonAsync<VoucherEntryReferencesDto>(JsonOptions, cancellationToken);
            openPeriods = periodResult?.OpenPeriods ?? [];
        }

        var result = new VoucherEntryReferencesDto
        {
            VoucherType = new VoucherEntryTypeDto
            {
                Id = voucherType.Voucher_Type_ID,
                Code = voucherType.Voucher_Type_Code,
                Name = voucherType.Voucher_Type_Name_AR
            },
            DraftStatus = new VoucherEntryStatusDto
            {
                Id = draftStatus.Voucher_Status_ID,
                Code = draftStatus.Voucher_Status_Code,
                Name = draftStatus.Voucher_Status_Name_AR
            },
            Sources = lookups.CashBoxes.Select(x => new VoucherEntrySourceDto
            {
                CashBoxId = x.Cash_Box_ID,
                AccountId = x.Account_ID,
                SourceType = "CASH",
                DisplayName = $"{x.Cash_Box_Code} - {x.Cash_Box_Name}"
            }).ToList(),
            Accounts = lookups.Accounts.Select(x => new VoucherEntryLookupDto
            {
                Id = x.Account_ID,
                DisplayName = $"{x.Account_Code} - {x.Account_Name_AR}"
            }).ToList(),
            CostCenters = lookups.CostCenters.Select(x => new VoucherEntryLookupDto
            {
                Id = x.Cost_Center_ID,
                DisplayName = $"{x.Cost_Center_Code} - {x.Cost_Center_Name_AR}"
            }).ToList(),
            Currencies = lookups.Currencies.Select(x => new VoucherEntryCurrencyDto
            {
                Id = x.Currency_ID,
                DisplayName = $"{x.Currency_Code} - {x.Currency_Name_AR}",
                ExchangeRate = x.Is_Local ? 1m : x.Exchange_Rate,
                IsLocal = x.Is_Local,
                IsDefault = x.Is_Default
            }).ToList(),
            Parties = lookups.Parties.Select(x => new VoucherEntryPartyDto
            {
                Id = x.Party_ID,
                Name = x.Party_Name_AR,
                DisplayName = $"{x.Party_Code} - {x.Party_Name_AR}"
            }).ToList(),
            PaymentMethods = lookups.PaymentMethods.Select(x => new VoucherEntryPaymentMethodDto
            {
                Id = x.Payment_Method_ID,
                Code = x.Payment_Method_Code,
                DisplayName = x.Payment_Method_Name_AR
            }).ToList(),
            OpenPeriods = openPeriods
        };

        result.SourceCount = result.Sources.Count;
        result.SourceMessage = result.SourceCount == 0
            ? "لا توجد صناديق نشطة ضمن الشركة والفرع الحاليين."
            : null;

        if (result.Sources.Count == 0 && result.Accounts.Count == 0 && result.Currencies.Count == 0)
            throw new InvalidOperationException("لا توجد بيانات مرجعية متاحة للسند في السياق الحالي.");

        return result;
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

    private sealed class DesktopFinancialVoucherLookupsDto
    {
        public List<DesktopVoucherTypeDto> VoucherTypes { get; set; } = [];
        public List<DesktopVoucherStatusDto> VoucherStatuses { get; set; } = [];
        public List<DesktopCashBoxDto> CashBoxes { get; set; } = [];
        public List<DesktopCurrencyDto> Currencies { get; set; } = [];
        public List<DesktopCostCenterDto> CostCenters { get; set; } = [];
        public List<DesktopPaymentMethodDto> PaymentMethods { get; set; } = [];
        public List<DesktopPartyDto> Parties { get; set; } = [];
        public List<DesktopAccountDto> Accounts { get; set; } = [];
    }

    private sealed class DesktopVoucherTypeDto
    {
        public int Voucher_Type_ID { get; set; }
        public string Voucher_Type_Code { get; set; } = string.Empty;
        public string Voucher_Type_Name_AR { get; set; } = string.Empty;
    }

    private sealed class DesktopVoucherStatusDto
    {
        public int Voucher_Status_ID { get; set; }
        public string Voucher_Status_Code { get; set; } = string.Empty;
        public string Voucher_Status_Name_AR { get; set; } = string.Empty;
    }

    private sealed class DesktopCashBoxDto
    {
        public int Cash_Box_ID { get; set; }
        public string Cash_Box_Code { get; set; } = string.Empty;
        public string Cash_Box_Name { get; set; } = string.Empty;
        public string Account_ID { get; set; } = string.Empty;
    }

    private sealed class DesktopCurrencyDto
    {
        public int Currency_ID { get; set; }
        public string Currency_Code { get; set; } = string.Empty;
        public string Currency_Name_AR { get; set; } = string.Empty;
        public decimal Exchange_Rate { get; set; }
        public bool Is_Default { get; set; }
        public bool Is_Local { get; set; }
    }

    private sealed class DesktopCostCenterDto
    {
        public string Cost_Center_ID { get; set; } = string.Empty;
        public string Cost_Center_Code { get; set; } = string.Empty;
        public string Cost_Center_Name_AR { get; set; } = string.Empty;
    }

    private sealed class DesktopPaymentMethodDto
    {
        public int Payment_Method_ID { get; set; }
        public string Payment_Method_Code { get; set; } = string.Empty;
        public string Payment_Method_Name_AR { get; set; } = string.Empty;
    }

    private sealed class DesktopPartyDto
    {
        public string Party_ID { get; set; } = string.Empty;
        public string Party_Code { get; set; } = string.Empty;
        public string Party_Name_AR { get; set; } = string.Empty;
    }

    private sealed class DesktopAccountDto
    {
        public string Account_ID { get; set; } = string.Empty;
        public string Account_Code { get; set; } = string.Empty;
        public string Account_Name_AR { get; set; } = string.Empty;
    }
}
