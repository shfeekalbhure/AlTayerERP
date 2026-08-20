using System.Data;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Services
{
    /// <summary>
    /// محرك الترقيم المركزي (NumberGeneratorService).
    /// يحجز الرقم في العداد داخل Transaction متسلسل؛ لذلك لا يعاد الرقم بعد إلغائه
    /// ولا ينتج رقمان متطابقان عند طلبات متزامنة.
    /// </summary>
    public sealed class NumberGeneratorService
    {
        private readonly AppDbContext _context;

        public NumberGeneratorService(AppDbContext context) => _context = context;

        public async Task<string> GenerateNextNumberAsync(string documentType) =>
            (await ReserveNextNumberAsync(documentType, null, null, null)).Document_Number;

        public async Task<string> GenerateNextNumberAsync(string documentType, string companyId) =>
            (await ReserveNextNumberAsync(documentType, companyId, null, null)).Document_Number;

        /// <summary>
        /// حجز رقم عرض مالي. FiscalYearId هو معرّف السنة المالي من الجلسة، لا سنة جهاز العميل.
        /// </summary>
        public async Task<NumberReservation> ReserveNextNumberAsync(
            string documentType,
            string? companyId,
            int? branchId,
            int? fiscalYearId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(documentType))
                throw new NumberingException("نوع المستند مطلوب.");

            var type = documentType.Trim().ToUpperInvariant();

            // يسمح التكرار فقط عند سباق إنشاء صف عداد جديد، وتمنعه قاعدة البيانات بفهرس فريد.
            for (var attempt = 1; attempt <= 3; attempt++)
            {
                // عند تغليف العملية المالية بمعاملة خادمية أوسع (مثل Idempotency)
                // يجب أن ينضم العداد إلى المعاملة نفسها حتى لا يُحجز رقم لمستند فشل حفظه.
                var ownsTransaction = _context.Database.CurrentTransaction is null;
                await using var transaction = ownsTransaction
                    ? await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
                    : null;
                try
                {
                    var setting = await _context.Numbering_Settings
                        .FirstOrDefaultAsync(x => x.Document_Type == type && x.Is_Active, cancellationToken);
                    if (setting == null)
                        throw new NumberingException($"لا يوجد إعداد ترقيم نشط لنوع المستند: {type}.");

                    ValidateSetting(setting);
                    var scope = await ResolveScopeAsync(setting, companyId, branchId, fiscalYearId, cancellationToken);

                    var counter = await _context.Numbering_Counters
                        .FirstOrDefaultAsync(x =>
                            x.Document_Type == type &&
                            x.Company_ID == scope.Company_ID &&
                            x.Branch_ID == scope.Branch_ID &&
                            x.Year_Value == scope.Fiscal_Year_ID, cancellationToken);

                    if (counter == null)
                    {
                        counter = new NumberingCounter
                        {
                            Document_Type = type,
                            // القيم غير المستخدمة تطبع بفراغ/صفر حتى يعمل المفتاح الفريد في MySQL.
                            Company_ID = scope.Company_ID,
                            Branch_ID = scope.Branch_ID,
                            Year_Value = scope.Fiscal_Year_ID,
                            Last_Number = 0,
                            Created_At = DateTime.UtcNow
                        };
                        _context.Numbering_Counters.Add(counter);
                    }

                    var next = checked(counter.Last_Number + 1);
                    var maximum = (int)Math.Pow(10, setting.Digits_Count) - 1;
                    if (next > maximum)
                        throw new NumberingException($"وصل عداد {type} إلى الحد الأقصى ({maximum:N0}). عدّل عدد الخانات قبل المتابعة.");

                    counter.Last_Number = next;
                    counter.Updated_At = DateTime.UtcNow;

                    await _context.SaveChangesAsync(cancellationToken);
                    if (transaction != null)
                        await transaction.CommitAsync(cancellationToken);

                    var number = BuildDocumentNumber(setting, scope, next);
                    return new NumberReservation(
                        number, type, counter.Counter_ID, next,
                        scope.Company_ID, scope.Branch_ID, scope.Fiscal_Year_ID);
                }
                catch (DbUpdateException) when (transaction != null && attempt < 3)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _context.ChangeTracker.Clear();
                    await Task.Delay(TimeSpan.FromMilliseconds(20 * attempt), cancellationToken);
                }
                catch
                {
                    if (transaction != null)
                        await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }

            throw new NumberingException("تعذر حجز الرقم بسبب تعارض متزامن. أعد المحاولة.");
        }

        private async Task<NumberingScope> ResolveScopeAsync(
            NumberingSetting setting,
            string? companyId,
            int? branchId,
            int? fiscalYearId,
            CancellationToken cancellationToken)
        {
            var normalizedCompany = setting.Use_Company
                ? companyId?.Trim() ?? string.Empty
                : string.Empty;
            var normalizedBranch = setting.Use_Branch ? branchId.GetValueOrDefault() : 0;
            var normalizedYear = setting.Use_Year ? fiscalYearId.GetValueOrDefault() : 0;

            if (setting.Use_Company && string.IsNullOrWhiteSpace(normalizedCompany))
                throw new NumberingException("إعداد الترقيم يتطلب شركة من سياق الجلسة.");
            if (setting.Use_Branch && normalizedBranch <= 0)
                throw new NumberingException("إعداد الترقيم يتطلب فرعاً من سياق الجلسة.");
            if (setting.Use_Year && normalizedYear <= 0)
                throw new NumberingException("إعداد الترقيم يتطلب سنة مالية من سياق الجلسة.");

            string companyPrefix = string.Empty;
            if (setting.Use_Company)
            {
                var company = await _context.Companies.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Company_ID == normalizedCompany && x.Is_Active, cancellationToken);
                if (company == null)
                    throw new NumberingException("شركة سياق الترقيم غير موجودة أو موقوفة.");
                companyPrefix = string.IsNullOrWhiteSpace(company.Company_Prefix)
                    ? normalizedCompany
                    : company.Company_Prefix.Trim().ToUpperInvariant();
            }

            return new NumberingScope(normalizedCompany, normalizedBranch, normalizedYear, companyPrefix);
        }

        private static void ValidateSetting(NumberingSetting setting)
        {
            if (setting.Digits_Count is < 1 or > 9)
                throw new NumberingException("عدد خانات الترقيم يجب أن يكون من 1 إلى 9.");
            if (string.IsNullOrWhiteSpace(setting.Prefix))
                throw new NumberingException("بادئة الرقم مطلوبة.");
        }

        private static string BuildDocumentNumber(NumberingSetting setting, NumberingScope scope, int serial)
        {
            // تدعم البادئة قوالب اختيارية: {COMPANY} و{BRANCH} و{YEAR}.
            var prefix = setting.Prefix.Trim().ToUpperInvariant()
                .Replace("{COMPANY}", scope.Company_Prefix, StringComparison.OrdinalIgnoreCase)
                .Replace("{BRANCH}", scope.Branch_ID == 0 ? string.Empty : scope.Branch_ID.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("{YEAR}", scope.Fiscal_Year_ID == 0 ? string.Empty : scope.Fiscal_Year_ID.ToString(), StringComparison.OrdinalIgnoreCase)
                .Trim('-');

            var parts = new List<string> { prefix };
            if (setting.Use_Company && !setting.Prefix.Contains("{COMPANY}", StringComparison.OrdinalIgnoreCase))
                parts.Add(scope.Company_Prefix);
            if (setting.Use_Branch && !setting.Prefix.Contains("{BRANCH}", StringComparison.OrdinalIgnoreCase))
                parts.Add(scope.Branch_ID.ToString());
            if (setting.Use_Year && !setting.Prefix.Contains("{YEAR}", StringComparison.OrdinalIgnoreCase))
                parts.Add(scope.Fiscal_Year_ID.ToString());

            parts.Add(serial.ToString().PadLeft(setting.Digits_Count, '0'));
            return string.Join("-", parts.Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        private sealed record NumberingScope(
            string Company_ID,
            int Branch_ID,
            int Fiscal_Year_ID,
            string Company_Prefix);
    }

    /// <summary>نتيجة الحجز: الرقم النهائي والنطاق الذي منع تكراره.</summary>
    public sealed record NumberReservation(
        string Document_Number,
        string Document_Type,
        int Counter_ID,
        int Serial_Number,
        string Company_ID,
        int Branch_ID,
        int Fiscal_Year_ID);

    public sealed class NumberingException : Exception
    {
        public NumberingException(string message) : base(message) { }
    }
}
