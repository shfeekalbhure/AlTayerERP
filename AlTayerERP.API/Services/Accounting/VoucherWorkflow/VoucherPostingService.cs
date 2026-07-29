using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlTayerERP.Core.Entities.Accounting;
using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Services.Accounting.VoucherWorkflow
{
    /// <summary>
    /// خدمة ترحيل وإلغاء ترحيل السندات المالية.
    ///
    /// تستخدم لجميع أنواع السندات المالية:
    /// - سند قبض.
    /// - سند صرف.
    /// - قيد يومية.
    /// - سند عهدة.
    /// - سند تحصيل.
    /// - إشعار مدين.
    /// - إشعار دائن.
    /// - سند تسوية.
    ///
    /// عند الترحيل يتم:
    /// 1- التحقق من صحة السند.
    /// 2- إنشاء قيد محاسبي عام.
    /// 3- نسخ تفاصيل السند إلى تفاصيل القيد.
    /// 4- ربط السند بالقيد.
    /// 5- تسجيل عملية الترحيل في سجل العمليات.
    /// </summary>
    public class VoucherPostingService
    {
        #region ثوابت حالات القيود

        /// <summary>
        /// القيد في حالة مسودة.
        /// </summary>
        private const int JournalStatusDraft = 1;

        /// <summary>
        /// القيد مرحل.
        /// </summary>
        private const int JournalStatusPosted = 2;

        /// <summary>
        /// القيد ملغي.
        /// </summary>
        private const int JournalStatusCancelled = 3;

        /// <summary>
        /// القيد معكوس.
        /// </summary>
        private const int JournalStatusReversed = 4;

        #endregion

        #region ثوابت أنواع القيود

        /// <summary>
        /// قيد تلقائي ناتج عن سند مالي.
        /// </summary>
        private const byte AutomaticEntryType = 1;

        #endregion

        #region المتغيرات

        private readonly AppDbContext _context;
        private readonly VoucherAuditService _auditService;

        #endregion

        #region المشيد

        public VoucherPostingService(
            AppDbContext context,
            VoucherAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        #endregion

        #region ترحيل السند

        /// <summary>
        /// ترحيل سند مالي وإنشاء القيد المحاسبي الناتج عنه.
        /// </summary>
        public async Task<PostingResult> PostAsync(
            long voucherId,
            string userId,
            string actionChannel = VoucherAuditService.DESKTOP,
            string? deviceName = null,
            string? ipAddress = null,
            string? notes = null)
        {
            #region التحقق من المدخلات

            if (voucherId <= 0)
            {
                return PostingResult.Fail(
                    "معرف السند غير صحيح.");
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                return PostingResult.Fail(
                    "معرف المستخدم مطلوب لتنفيذ الترحيل.");
            }

            userId = userId.Trim();

            actionChannel =
                NormalizeActionChannel(actionChannel);

            #endregion

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                #region جلب السند

                FinancialVoucherHeader? voucher =
                    await _context.Financial_Voucher_Headers
                        .Include(x => x.Details)
                        .FirstOrDefaultAsync(x =>
                            x.Voucher_ID == voucherId);

                if (voucher == null)
                {
                    await transaction.RollbackAsync();

                    return PostingResult.Fail(
                        "السند المالي غير موجود.");
                }

                #endregion

                #region التحقق من حالة السند

                if (!voucher.Is_Active)
                {
                    await transaction.RollbackAsync();

                    return PostingResult.Fail(
                        "لا يمكن ترحيل سند ملغي أو غير نشط.");
                }

                if (voucher.Is_Posted)
                {
                    await transaction.RollbackAsync();

                    return PostingResult.Fail(
                        "السند مرحل محاسبيًا مسبقًا.");
                }

                if (voucher.Journal_Entry_ID.HasValue)
                {
                    await transaction.RollbackAsync();

                    return PostingResult.Fail(
                        "السند مرتبط مسبقًا بقيد محاسبي.");
                }

                if (voucher.Requires_Approval &&
                    voucher.Approval_Status !=
                    VoucherApprovalService.ApprovalApproved)
                {
                    await transaction.RollbackAsync();

                    return PostingResult.Fail(
                        "يجب اعتماد السند قبل ترحيله محاسبيًا.");
                }

                if (voucher.Details == null ||
                    voucher.Details.Count == 0)
                {
                    await transaction.RollbackAsync();

                    return PostingResult.Fail(
                        "لا يمكن ترحيل سند لا يحتوي على تفاصيل.");
                }

                #endregion

                #region التحقق من توازن السند

                decimal totalDebit =
                    decimal.Round(
                        voucher.Details.Sum(
                            x => x.Debit_Amount),
                        2);

                decimal totalCredit =
                    decimal.Round(
                        voucher.Details.Sum(
                            x => x.Credit_Amount),
                        2);

                if (totalDebit <= 0m &&
                    totalCredit <= 0m)
                {
                    await transaction.RollbackAsync();

                    return PostingResult.Fail(
                        "لا يمكن ترحيل سند لا يحتوي على مبالغ.");
                }

                if (totalDebit != totalCredit)
                {
                    await transaction.RollbackAsync();

                    return PostingResult.Fail(
                        $"السند غير متوازن. " +
                        $"إجمالي المدين: {totalDebit:N2}، " +
                        $"إجمالي الدائن: {totalCredit:N2}.");
                }

                #endregion

                #region التحقق من سياق الفترة والبيانات المرجعية عند الترحيل

                int branchId = voucher.Branch_ID;

                bool periodIsOpen = await _context.Fiscal_Periods.AsNoTracking().AnyAsync(x =>
                    x.Branch_ID == branchId &&
                    x.Fiscal_Year_ID == voucher.Fiscal_Year_ID &&
                    x.Is_Active &&
                    !x.Is_Closed &&
                    voucher.Voucher_Date.Date >= x.Start_Date.Date &&
                    voucher.Voucher_Date.Date <= x.End_Date.Date);

                if (!periodIsOpen)
                {
                    await transaction.RollbackAsync();
                    return PostingResult.Fail("لا يمكن الترحيل خارج فترة مالية مفتوحة.");
                }

                var accountIds = voucher.Details.Select(x => x.Account_ID).Distinct().ToList();
                int activeAccounts = await _context.Chart_Of_Accounts.AsNoTracking()
                    .Where(x => accountIds.Contains(x.Account_ID) && x.Is_Active && x.Is_Postable)
                    .CountAsync();
                if (activeAccounts != accountIds.Count)
                {
                    await transaction.RollbackAsync();
                    return PostingResult.Fail("لا يمكن الترحيل: يوجد حساب موقوف أو غير قابل للترحيل.");
                }

                var currencyIds = voucher.Details.Select(x => x.Currency_ID).Append(voucher.Currency_ID).Distinct().ToList();
                int activeCurrencies = await _context.Currencies.AsNoTracking()
                    .Where(x => currencyIds.Contains(x.Currency_ID) && x.Is_Active)
                    .CountAsync();
                if (activeCurrencies != currencyIds.Count)
                {
                    await transaction.RollbackAsync();
                    return PostingResult.Fail("لا يمكن الترحيل: توجد عملة موقوفة أو غير صالحة.");
                }

                #endregion

                #region منع تكرار القيد من نفس السند

                bool journalAlreadyExists =
                    await _context.Journal_Entry_Headers
                        .AnyAsync(x =>
                            x.Source_Voucher_ID ==
                            voucher.Voucher_ID &&
                            x.Is_Active &&
                            !x.Is_Cancelled);

                if (journalAlreadyExists)
                {
                    await transaction.RollbackAsync();

                    return PostingResult.Fail(
                        "يوجد قيد محاسبي فعال ناتج عن هذا السند.");
                }

                #endregion

                #region توليد رقم القيد

                string entryNumber =
                    await GenerateJournalEntryNumberAsync(
                        voucher);

                #endregion

                #region إنشاء رأس القيد

                DateTime postingDate =
                    DateTime.Now;

                string sourceDocumentType =
                    await GetSourceDocumentTypeAsync(
                        voucher.Voucher_Type_ID);

                var journalEntry =
                    new JournalEntryHeader
                    {
                        Entry_No =
                            entryNumber,

                        Entry_Type =
                            AutomaticEntryType,

                        Entry_Status_ID =
                            JournalStatusPosted,

                        Branch_ID =
                            voucher.Branch_ID,

                        Fiscal_Year_ID =
                            voucher.Fiscal_Year_ID,

                        Entry_Date =
                            voucher.Voucher_Date,

                        Transaction_Date =
                            voucher.Transaction_Date,

                        Source_System =
                            "VOUCHER",

                        Is_System_Generated =
                            true,

                        Source_Voucher_ID =
                            voucher.Voucher_ID,

                        Source_Document_Type =
                            sourceDocumentType,

                        Source_Document_No =
                            voucher.Voucher_No,

                        Description =
                            BuildJournalDescription(voucher),

                        Notes =
                            notes,

                        Total_Debit =
                            totalDebit,

                        Total_Credit =
                            totalCredit,

                        Is_Posted =
                            true,

                        Posted_By =
                            userId,

                        Posted_At =
                            postingDate,

                        Is_Reversal =
                            false,

                        Original_Journal_Entry_ID =
                            null,

                        Is_Reversed =
                            false,

                        Reversal_Journal_Entry_ID =
                            null,

                        Reversal_Reason =
                            null,

                        Reversed_By =
                            null,

                        Reversed_At =
                            null,

                        Is_Cancelled =
                            false,

                        Cancellation_Reason =
                            null,

                        Cancelled_By =
                            null,

                        Cancelled_At =
                            null,

                        Is_Active =
                            true,

                        Created_By =
                            userId,

                        Created_At =
                            postingDate
                    };

                #endregion

                #region إنشاء تفاصيل القيد

                foreach (
                    FinancialVoucherDetail voucherDetail
                    in voucher.Details.OrderBy(
                        x => x.Line_No))
                {
                    decimal localDebit =
                        voucherDetail.Debit_Amount;

                    decimal localCredit =
                        voucherDetail.Credit_Amount;

                    journalEntry.Details.Add(
                        new JournalEntryDetail
                        {
                            Line_No =
                                voucherDetail.Line_No,

                            Account_ID =
                                voucherDetail.Account_ID,

                            Description =
                                voucherDetail.Description ??
                                voucher.Description,

                            Cost_Center_ID =
                                voucherDetail.Cost_Center_ID,

                            Project_ID =
                                voucherDetail.Project_ID,

                            Currency_ID =
                                voucherDetail.Currency_ID,

                            Exchange_Rate =
                                voucherDetail.Exchange_Rate,

                            Foreign_Amount =
                                voucherDetail.Foreign_Amount,

                            Local_Amount =
                                voucherDetail.Local_Amount,

                            Debit_Amount =
                                localDebit,

                            Credit_Amount =
                                localCredit,

                            Reference_Type =
                                voucherDetail.Reference_Type,

                            Reference_No =
                                voucherDetail.Reference_No,

                            Reference_Name =
                                voucherDetail.Reference_Name,

                            Reference_Date =
                                voucherDetail.Reference_Date,

                            Source_Voucher_Detail_ID =
                                voucherDetail
                                    .Voucher_Detail_ID,

                            Line_Type =
                                voucherDetail.Line_Type,

                            Notes =
                                voucherDetail.Notes,

                            Created_By =
                                userId,

                            Created_At =
                                postingDate
                        });
                }

                #endregion

                #region حفظ القيد للحصول على معرفه

                await _context.Journal_Entry_Headers
                    .AddAsync(journalEntry);

                await _context.SaveChangesAsync();

                #endregion

                #region ربط السند بالقيد

                voucher.Is_Posted =
                    true;

                voucher.Journal_Entry_ID =
                    journalEntry.Journal_Entry_ID;

                voucher.Posted_By_User_ID =
                    userId;

                voucher.Posted_At =
                    postingDate;

                voucher.Unposted_By =
                    null;

                voucher.Unposted_At =
                    null;

                voucher.Unpost_Reason =
                    null;

                voucher.Updated_By =
                    userId;

                voucher.Updated_At =
                    postingDate;

                var postedStatus = await _context.Voucher_Statuses
                    .FirstOrDefaultAsync(x => x.Voucher_Status_Code == "POSTED" && x.Is_Active);
                if (postedStatus == null)
                {
                    await transaction.RollbackAsync();
                    return PostingResult.Fail("حالة POSTED المرجعية غير مهيأة.");
                }
                voucher.Voucher_Status_ID = postedStatus.Voucher_Status_ID;

                #endregion

                #region تسجيل عملية الترحيل

                await _auditService.LogAsync(
                    voucherId:
                        voucher.Voucher_ID,

                    actionType:
                        VoucherAuditService.POST,

                    oldStatusId:
                        voucher.Voucher_Status_ID,

                    newStatusId:
                        voucher.Voucher_Status_ID,

                    userId:
                        userId,

                    actionChannel:
                        actionChannel,

                    deviceName:
                        deviceName,

                    ipAddress:
                        ipAddress,

                    reason:
                        null,

                    notes:
                        $"تم ترحيل السند وإنشاء القيد " +
                        $"{journalEntry.Entry_No}.");

                #endregion

                await transaction.CommitAsync();

                return PostingResult.Ok(
                    message:
                        $"تم ترحيل السند رقم " +
                        $"{voucher.Voucher_No} بنجاح.",

                    journalEntryId:
                        journalEntry.Journal_Entry_ID,

                    journalEntryNo:
                        journalEntry.Entry_No);
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();

                string error =
                    ex.InnerException?.Message ??
                    ex.Message;

                return PostingResult.Fail(
                    $"تعذر ترحيل السند في قاعدة البيانات: " +
                    $"{error}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return PostingResult.Fail(
                    $"حدث خطأ أثناء ترحيل السند: " +
                    $"{ex.Message}");
            }
        }

        #endregion

        #region إلغاء الترحيل

        /// <summary>
        /// إلغاء ترحيل سند مالي.
        ///
        /// لا يتم حذف القيد المحاسبي نهائيًا،
        /// بل يتم تحويله إلى حالة ملغي وإبقاؤه للرقابة.
        /// </summary>
        public async Task<(bool Success, string Message)>
            UnpostAsync(
                long voucherId,
                string userId,
                string reason,
                string actionChannel =
                    VoucherAuditService.DESKTOP,
                string? deviceName = null,
                string? ipAddress = null)
        {
            #region التحقق من المدخلات

            if (voucherId <= 0)
            {
                return (
                    false,
                    "معرف السند غير صحيح.");
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                return (
                    false,
                    "معرف المستخدم مطلوب لإلغاء الترحيل.");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                return (
                    false,
                    "سبب إلغاء الترحيل مطلوب.");
            }

            userId =
                userId.Trim();

            reason =
                reason.Trim();

            actionChannel =
                NormalizeActionChannel(actionChannel);

            #endregion

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                #region جلب السند

                FinancialVoucherHeader? voucher =
                    await _context.Financial_Voucher_Headers
                        .FirstOrDefaultAsync(x =>
                            x.Voucher_ID == voucherId);

                if (voucher == null)
                {
                    await transaction.RollbackAsync();

                    return (
                        false,
                        "السند المالي غير موجود.");
                }

                #endregion

                #region التحقق من حالة السند

                if (!voucher.Is_Posted)
                {
                    await transaction.RollbackAsync();

                    return (
                        false,
                        "السند غير مرحل محاسبيًا.");
                }

                if (!voucher.Journal_Entry_ID.HasValue)
                {
                    await transaction.RollbackAsync();

                    return (
                        false,
                        "لا يوجد قيد محاسبي مرتبط بالسند.");
                }

                #endregion

                #region جلب القيد المرتبط

                JournalEntryHeader? journalEntry =
                    await _context.Journal_Entry_Headers
                        .FirstOrDefaultAsync(x =>
                            x.Journal_Entry_ID ==
                            voucher.Journal_Entry_ID.Value);

                if (journalEntry == null)
                {
                    await transaction.RollbackAsync();

                    return (
                        false,
                        "القيد المحاسبي المرتبط بالسند غير موجود.");
                }

                if (journalEntry.Is_Reversed)
                {
                    await transaction.RollbackAsync();

                    return (
                        false,
                        "لا يمكن إلغاء ترحيل قيد تم عكسه محاسبيًا.");
                }

                if (journalEntry.Is_Cancelled)
                {
                    await transaction.RollbackAsync();

                    return (
                        false,
                        "القيد المحاسبي ملغي مسبقًا.");
                }

                #endregion

                #region إلغاء القيد مع الاحتفاظ به

                DateTime unpostDate =
                    DateTime.Now;

                long cancelledJournalEntryId =
                    journalEntry.Journal_Entry_ID;

                string cancelledJournalEntryNo =
                    journalEntry.Entry_No;

                journalEntry.Entry_Status_ID =
                    JournalStatusCancelled;

                journalEntry.Is_Posted =
                    false;

                journalEntry.Is_Cancelled =
                    true;

                journalEntry.Cancellation_Reason =
                    reason;

                journalEntry.Cancelled_By =
                    userId;

                journalEntry.Cancelled_At =
                    unpostDate;

                journalEntry.Updated_By =
                    userId;

                journalEntry.Updated_At =
                    unpostDate;

                #endregion

                #region إعادة السند إلى غير مرحل

                voucher.Is_Posted =
                    false;

                voucher.Journal_Entry_ID =
                    null;

                voucher.Unposted_By =
                    userId;

                voucher.Unposted_At =
                    unpostDate;

                voucher.Unpost_Reason =
                    reason;

                voucher.Updated_By =
                    userId;

                voucher.Updated_At =
                    unpostDate;

                var pendingStatus = await _context.Voucher_Statuses
                    .FirstOrDefaultAsync(x => x.Voucher_Status_Code == "PENDING" && x.Is_Active);
                if (pendingStatus == null)
                {
                    await transaction.RollbackAsync();
                    return (false, "حالة PENDING المرجعية غير مهيأة.");
                }
                voucher.Voucher_Status_ID = pendingStatus.Voucher_Status_ID;

                #endregion

                #region تسجيل إلغاء الترحيل

                await _auditService.LogAsync(
                    voucherId:
                        voucher.Voucher_ID,

                    actionType:
                        VoucherAuditService.UNPOST,

                    oldStatusId:
                        voucher.Voucher_Status_ID,

                    newStatusId:
                        voucher.Voucher_Status_ID,

                    userId:
                        userId,

                    actionChannel:
                        actionChannel,

                    deviceName:
                        deviceName,

                    ipAddress:
                        ipAddress,

                    reason:
                        reason,

                    notes:
                        $"تم إلغاء ترحيل القيد " +
                        $"{cancelledJournalEntryNo}، " +
                        $"معرف القيد: " +
                        $"{cancelledJournalEntryId}.");

                #endregion

                await transaction.CommitAsync();

                return (
                    true,
                    $"تم إلغاء ترحيل السند رقم " +
                    $"{voucher.Voucher_No} بنجاح.");
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();

                string error =
                    ex.InnerException?.Message ??
                    ex.Message;

                return (
                    false,
                    $"تعذر إلغاء الترحيل في قاعدة البيانات: " +
                    $"{error}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return (
                    false,
                    $"حدث خطأ أثناء إلغاء الترحيل: " +
                    $"{ex.Message}");
            }
        }

        #endregion

        #region جلب حالة الترحيل

        /// <summary>
        /// جلب حالة ترحيل السند والقيد المرتبط به.
        /// </summary>
        public async Task<PostingStatusResult?>
            GetPostingStatusAsync(long voucherId)
        {
            if (voucherId <= 0)
            {
                return null;
            }

            return await (
                from voucher
                    in _context.Financial_Voucher_Headers
                        .AsNoTracking()

                join journal
                    in _context.Journal_Entry_Headers
                        .AsNoTracking()

                    on voucher.Journal_Entry_ID equals
                    journal.Journal_Entry_ID
                    into journalGroup

                from journal
                    in journalGroup.DefaultIfEmpty()

                where voucher.Voucher_ID == voucherId

                select new PostingStatusResult
                {
                    Voucher_ID =
                        voucher.Voucher_ID,

                    Voucher_No =
                        voucher.Voucher_No,

                    Is_Posted =
                        voucher.Is_Posted,

                    Journal_Entry_ID =
                        voucher.Journal_Entry_ID,

                    Journal_Entry_No =
                        journal != null
                            ? journal.Entry_No
                            : null,

                    Journal_Is_Posted =
                        journal != null &&
                        journal.Is_Posted,

                    Journal_Is_Cancelled =
                        journal != null &&
                        journal.Is_Cancelled,

                    Posted_By_User_ID =
                        voucher.Posted_By_User_ID,

                    Posted_At =
                        voucher.Posted_At,

                    Unposted_By =
                        voucher.Unposted_By,

                    Unposted_At =
                        voucher.Unposted_At,

                    Unpost_Reason =
                        voucher.Unpost_Reason
                })
                .FirstOrDefaultAsync();
        }

        #endregion

        #region توليد رقم القيد المحاسبي

        /// <summary>
        /// توليد رقم القيد من إعدادات الترقيم.
        /// Document_Type = JOURNAL_ENTRY
        /// </summary>
        private async Task<string>
            GenerateJournalEntryNumberAsync(
                FinancialVoucherHeader voucher)
        {
            var setting =
                await _context.Numbering_Settings
                    .FirstOrDefaultAsync(x =>
                        x.Document_Type ==
                        "JOURNAL_ENTRY" &&
                        x.Is_Active);

            if (setting == null)
            {
                throw new InvalidOperationException(
                    "لا يوجد إعداد ترقيم فعال للقيد المحاسبي " +
                    "(JOURNAL_ENTRY).");
            }

            setting.Last_Number += 1;

            int nextNumber =
                setting.Last_Number;

            int digitsCount =
                setting.Digits_Count > 0
                    ? setting.Digits_Count
                    : 6;

            string serialPart =
                nextNumber.ToString()
                    .PadLeft(
                        digitsCount,
                        '0');

            var parts =
                new List<string>();

            if (!string.IsNullOrWhiteSpace(
                setting.Prefix))
            {
                parts.Add(
                    setting.Prefix
                        .Trim()
                        .ToUpper());
            }

            if (setting.Use_Company)
            {
                parts.Add(
                    await GetCurrentCompanyIdAsync(voucher.Branch_ID));
            }

            if (setting.Use_Branch)
            {
                parts.Add(
                    await GetCurrentBranchCodeAsync(voucher.Branch_ID));
            }

            if (setting.Use_Year)
            {
                parts.Add(
                    voucher.EntryYear());
            }

            parts.Add(serialPart);

            return string.Join(
                "-",
                parts);
        }

        #endregion

        #region تحديد نوع المستند المصدر

        /// <summary>
        /// تحويل نوع السند الرقمي إلى كود برمجي واضح.
        /// يمكن إضافة أنواع جديدة مستقبلًا.
        /// </summary>
        private async Task<string> GetSourceDocumentTypeAsync(
            int voucherTypeId)
        {
            var voucherTypeCode = await _context.Voucher_Types.AsNoTracking()
                .Where(x => x.Voucher_Type_ID == voucherTypeId && x.Is_Active)
                .Select(x => x.Voucher_Type_Code)
                .SingleOrDefaultAsync();

            return voucherTypeCode?.Trim().ToUpperInvariant() switch
            {
                "RECEIPT" => "RECEIPT_VOUCHER",
                "PAYMENT" => "PAYMENT_VOUCHER",
                "JOURNAL" => "JOURNAL_VOUCHER",
                "ADJUSTMENT" => "ADJUSTMENT_VOUCHER",
                "OPENING" => "OPENING_VOUCHER",
                _ => throw new InvalidOperationException(
                    "نوع السند غير مهيأ أو موقوف ولا يمكن ترحيله.")
            };
        }

        #endregion

        #region تكوين بيان القيد

        /// <summary>
        /// تكوين بيان واضح للقيد الناتج عن السند.
        /// </summary>
        private static string BuildJournalDescription(
            FinancialVoucherHeader voucher)
        {
            if (!string.IsNullOrWhiteSpace(
                voucher.Description))
            {
                return voucher.Description.Trim();
            }

            if (!string.IsNullOrWhiteSpace(
                voucher.Against_Text))
            {
                return voucher.Against_Text.Trim();
            }

            return
                $"قيد محاسبي ناتج عن السند رقم " +
                $"{voucher.Voucher_No}.";
        }

        #endregion

        #region الأدوات المساعدة

        /// <summary>
        /// معرف الشركة الحالي المستخدم في الترقيم.
        /// يستبدل لاحقًا بقيمة الجلسة الحالية.
        /// </summary>
        private async Task<string> GetCurrentCompanyIdAsync(int branchId)
        {
            var companyId = await _context.Tenant_Branches.AsNoTracking()
                .Where(x => x.Branch_ID == branchId && x.Is_Active)
                .Select(x => x.Company_ID)
                .FirstOrDefaultAsync();

            return string.IsNullOrWhiteSpace(companyId)
                ? throw new InvalidOperationException("تعذر تحديد شركة الفرع لتوليد رقم القيد.")
                : companyId;
        }

        /// <summary>يستخرج كود الفرع النصي للاستخدام داخل رقم المستند.</summary>
        private async Task<string> GetCurrentBranchCodeAsync(int branchId)
        {
            var branchCode = await _context.Tenant_Branches.AsNoTracking()
                .Where(x => x.Branch_ID == branchId && x.Is_Active)
                .Select(x => x.Branch_Code)
                .FirstOrDefaultAsync();

            return string.IsNullOrWhiteSpace(branchCode)
                ? throw new InvalidOperationException("تعذر تحديد كود الفرع لتوليد رقم القيد.")
                : branchCode;
        }

        /// <summary>
        /// ضبط اسم قناة تنفيذ العملية.
        /// </summary>
        private static string NormalizeActionChannel(
            string? actionChannel)
        {
            if (string.IsNullOrWhiteSpace(
                actionChannel))
            {
                return VoucherAuditService.API;
            }

            string channel =
                actionChannel.Trim().ToUpper();

            return channel switch
            {
                "DESKTOP" =>
                    VoucherAuditService.DESKTOP,

                "MOBILE" =>
                    VoucherAuditService.MOBILE,

                "ANDROID" =>
                    "ANDROID",

                "IOS" =>
                    "IOS",

                "WEB" =>
                    VoucherAuditService.WEB,

                "API" =>
                    VoucherAuditService.API,

                _ =>
                    VoucherAuditService.API
            };
        }

        #endregion

        #region نموذج نتيجة الترحيل

        /// <summary>
        /// نتيجة عملية ترحيل السند.
        /// </summary>
        public sealed class PostingResult
        {
            public bool Success { get; set; }

            public string Message { get; set; }
                = string.Empty;

            public long? Journal_Entry_ID { get; set; }

            public string? Journal_Entry_No { get; set; }

            public static PostingResult Ok(
                string message,
                long journalEntryId,
                string journalEntryNo)
            {
                return new PostingResult
                {
                    Success = true,
                    Message = message,
                    Journal_Entry_ID =
                        journalEntryId,
                    Journal_Entry_No =
                        journalEntryNo
                };
            }

            public static PostingResult Fail(
                string message)
            {
                return new PostingResult
                {
                    Success = false,
                    Message = message
                };
            }
        }

        #endregion

        #region نموذج حالة الترحيل

        /// <summary>
        /// بيانات حالة ترحيل السند.
        /// </summary>
        public sealed class PostingStatusResult
        {
            public long Voucher_ID { get; set; }

            public string Voucher_No { get; set; }
                = string.Empty;

            public bool Is_Posted { get; set; }

            public long? Journal_Entry_ID { get; set; }

            public string? Journal_Entry_No { get; set; }

            public bool Journal_Is_Posted { get; set; }

            public bool Journal_Is_Cancelled { get; set; }

            public string? Posted_By_User_ID { get; set; }

            public DateTime? Posted_At { get; set; }

            public string? Unposted_By { get; set; }

            public DateTime? Unposted_At { get; set; }

            public string? Unpost_Reason { get; set; }
        }

        #endregion
    }

    /// <summary>
    /// دوال مساعدة خاصة بتاريخ السند.
    /// </summary>
    internal static class FinancialVoucherPostingExtensions
    {
        /// <summary>
        /// إرجاع سنة السند كنص لاستخدامها في رقم القيد.
        /// </summary>
        public static string EntryYear(
            this FinancialVoucherHeader voucher)
        {
            return voucher.Voucher_Date.Year
                .ToString();
        }
    }
}
