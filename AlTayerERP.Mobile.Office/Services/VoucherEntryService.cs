using System.Diagnostics;
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
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException("تعذر الاتصال بخادم القوائم. تحقق من إعداد الاتصال ثم أعد المحاولة.", ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new InvalidOperationException("انتهت مهلة الاتصال بخادم القوائم.", ex);
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
            catch (JsonException ex)
            {
                throw new InvalidOperationException("تعذر قراءة بيانات منسدلات السند القادمة من الخادم.", ex);
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
                throw new InvalidOperationException(
                    "لم تصل بيانات كافية لإدخال السند. تحقق من صلاحيات المستخدم وربط الشركة والفرع والسنة المالية، ثم أعد تحميل البيانات.");
            }

            return result;
        }
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
        if (response.IsSuccessStatusCode)
            return;

        var developerDetails = await response.Content.ReadAsStringAsync(cancellationToken);
        Debug.WriteLine(
            $"[MobileVoucherSaveError] Endpoint={response.RequestMessage?.RequestUri?.AbsolutePath} " +
            $"StatusCode={(int)response.StatusCode} Details={developerDetails}");

        // نعرض فقط رسالة عربية قصيرة ومضبوطة صادرة من API؛ أما النص الخام
        // فيبقى في سجل التطوير حتى لا تظهر تفاصيل تقنية أو أسرار للمستخدم.
        var serverMessage = ExtractSafeServerMessage(developerDetails);
        throw new InvalidOperationException(serverMessage ?? fallback);
    }

    private static string? ExtractSafeServerMessage(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
            return null;

        try
        {
            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;

            foreach (var propertyName in new[] { "message", "title" })
            {
                if (root.TryGetProperty(propertyName, out var property) &&
                    property.ValueKind == JsonValueKind.String)
                {
                    var message = property.GetString()?.Trim();
                    if (IsSafeArabicMessage(message))
                        return message;
                }
            }

            // يدعم ValidationProblemDetails إذا أعاد الخادم أخطاء حقول متعددة.
            if (root.TryGetProperty("errors", out var errors) &&
                errors.ValueKind == JsonValueKind.Object)
            {
                var messages = new List<string>();
                foreach (var error in errors.EnumerateObject())
                {
                    if (error.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in error.Value.EnumerateArray())
                        {
                            if (item.ValueKind == JsonValueKind.String)
                                messages.Add(item.GetString()!.Trim());
                        }
                    }
                    else if (error.Value.ValueKind == JsonValueKind.String)
                    {
                        messages.Add(error.Value.GetString()!.Trim());
                    }
                }

                var combined = string.Join(" ", messages.Where(IsSafeArabicMessage).Distinct());
                if (IsSafeArabicMessage(combined))
                    return combined;
            }
        }
        catch (JsonException)
        {
            // يبقى استخدام الرسالة العامة عند استجابة غير JSON.
        }

        return null;
    }

    private static bool IsSafeArabicMessage(string? message) =>
        !string.IsNullOrWhiteSpace(message) &&
        message.Length <= 500 &&
        message.Any(character => character is >= '\u0600' and <= '\u06FF') &&
        !message.Contains("connection string", StringComparison.OrdinalIgnoreCase) &&
        !message.Contains("password", StringComparison.OrdinalIgnoreCase) &&
        !message.Contains("token", StringComparison.OrdinalIgnoreCase) &&
        !message.Contains("stack trace", StringComparison.OrdinalIgnoreCase) &&
        !message.Contains(" at ", StringComparison.OrdinalIgnoreCase);
    }
}
