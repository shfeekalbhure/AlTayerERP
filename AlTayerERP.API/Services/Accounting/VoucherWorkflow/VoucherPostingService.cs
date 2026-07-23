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
                    GetSourceDocumentType(
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
                    GetCurrentCompanyId());
            }

            if (setting.Use_Branch)
            {
                parts.Add(
                    voucher.Branch_ID);
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
        private static string GetSourceDocumentType(
            int voucherTypeId)
        {
            return voucherTypeId switch
            {
                1 =>
                    "RECEIPT_VOUCHER",

                2 =>
                    "PAYMENT_VOUCHER",

                3 =>
                    "JOURNAL_VOUCHER",

                4 =>
                    "DEBIT_NOTE",

                5 =>
                    "CREDIT_NOTE",

                6 =>
                    "ADJUSTMENT_VOUCHER",

                7 =>
                    "CUSTODY_VOUCHER",

                8 =>
                    "COLLECTION_VOUCHER",

                9 =>
                    "SHIPMENT_COLLECTION",

                _ =>
                    $"FINANCIAL_VOUCHER_{voucherTypeId}"
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
        private static string GetCurrentCompanyId()
        {
            return "FG-00001";
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
        /// <summary>
        /// إنشاء قيد عكسي لقيد مرحل ناتج عن سند، دون حذف القيد الأصلي.
        /// </summary>
        public async Task<PostingResult> ReverseJournalForVoucherAsync(
            long voucherId, string userId, string reason,
            string actionChannel = VoucherAuditService.DESKTOP,
            string? deviceName = null, string? ipAddress = null)
        {
            if (voucherId <= 0 || string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(reason))
                return PostingResult.Fail("معرف السند والمستخدم وسبب العكس مطلوبة.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var voucher = await _context.Financial_Voucher_Headers
                    .FirstOrDefaultAsync(x => x.Voucher_ID == voucherId && x.Is_Active);
                if (voucher is null || !voucher.Is_Posted || !voucher.Journal_Entry_ID.HasValue)
                    return PostingResult.Fail("لا يوجد قيد مرحل قابل للعكس لهذا السند.");

                var original = await _context.Journal_Entry_Headers
                    .Include(x => x.Details)
                    .FirstOrDefaultAsync(x => x.Journal_Entry_ID == voucher.Journal_Entry_ID &&
                                              x.Is_Posted && !x.Is_Cancelled);
                if (original is null || original.Is_Reversed)
                    return PostingResult.Fail("القيد غير موجود أو سبق عكسه.");

                var now = DateTime.UtcNow;
                var reversal = new JournalEntryHeader
                {
                    Entry_No = await GenerateJournalEntryNumberAsync(voucher),
                    Entry_Type = 3,
                    Entry_Status_ID = JournalStatusPosted,
                    Branch_ID = original.Branch_ID,
                    Fiscal_Year_ID = original.Fiscal_Year_ID,
                    Entry_Date = now.Date,
                    Transaction_Date = now,
                    Source_System = "VOUCHER_REVERSAL",
                    Is_System_Generated = true,
                    Source_Voucher_ID = voucher.Voucher_ID,
                    Source_Document_Type = "REVERSAL",
                    Source_Document_No = original.Entry_No,
                    Description = $"قيد عكسي للقيد {original.Entry_No}",
                    Notes = reason.Trim(),
                    Total_Debit = original.Total_Credit,
                    Total_Credit = original.Total_Debit,
                    Is_Posted = true,
                    Posted_By = userId.Trim(),
                    Posted_At = now,
                    Is_Reversal = true,
                    Original_Journal_Entry_ID = original.Journal_Entry_ID,
                    Is_Active = true,
                    Created_By = userId.Trim(),
                    Created_At = now
                };

                foreach (var line in original.Details.OrderBy(x => x.Line_No))
                {
                    reversal.Details.Add(new JournalEntryDetail
                    {
                        Line_No = line.Line_No,
                        Account_ID = line.Account_ID,
                        Description = $"عكس: {line.Description}",
                        Cost_Center_ID = line.Cost_Center_ID,
                        Project_ID = line.Project_ID,
                        Currency_ID = line.Currency_ID,
                        Exchange_Rate = line.Exchange_Rate,
                        Foreign_Amount = line.Foreign_Amount,
                        Local_Amount = line.Local_Amount,
                        Debit_Amount = line.Credit_Amount,
                        Credit_Amount = line.Debit_Amount,
                        Reference_Type = "REVERSAL",
                        Reference_No = original.Entry_No,
                        Reference_Name = line.Reference_Name,
                        Reference_Date = now,
                        Source_Voucher_Detail_ID = line.Source_Voucher_Detail_ID,
                        Line_Type = line.Line_Type,
                        Notes = reason.Trim(),
                        Created_By = userId.Trim(),
                        Created_At = now
                    });
                }

                _context.Journal_Entry_Headers.Add(reversal);
                await _context.SaveChangesAsync();

                original.Is_Reversed = true;
                original.Reversal_Journal_Entry_ID = reversal.Journal_Entry_ID;
                original.Reversal_Reason = reason.Trim();
                original.Reversed_By = userId.Trim();
                original.Reversed_At = now;
                original.Updated_By = userId.Trim();
                original.Updated_At = now;

                voucher.Is_Posted = false;
                voucher.Is_Reversed = true;
                voucher.Reversal_Journal_Entry_ID = reversal.Journal_Entry_ID;
                voucher.Reversed_By = userId.Trim();
                voucher.Reversed_At = now;
                voucher.Reversal_Reason = reason.Trim();
                voucher.Unposted_By = userId.Trim();
                voucher.Unposted_At = now;
                voucher.Unpost_Reason = $"عكس محاسبي: {reason.Trim()}";
                voucher.Updated_By = userId.Trim();
                voucher.Updated_At = now;

                _context.Voucher_Action_Logs.Add(new VoucherActionLog
                {
                    Voucher_ID = voucher.Voucher_ID,
                    Action_Type = "REVERSE",
                    Old_Status_ID = voucher.Voucher_Status_ID,
                    New_Status_ID = voucher.Voucher_Status_ID,
                    User_ID = userId.Trim(),
                    Action_At = now,
                    Action_Channel = NormalizeActionChannel(actionChannel),
                    Device_Name = deviceName,
                    IP_Address = ipAddress,
                    Reason = reason.Trim(),
                    Notes = $"تم إنشاء القيد العكسي {reversal.Entry_No} للقيد {original.Entry_No}."
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return PostingResult.Ok("تم إنشاء القيد العكسي بنجاح.", reversal.Journal_Entry_ID, reversal.Entry_No);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return PostingResult.Fail($"تعذر إنشاء القيد العكسي: {ex.Message}");
            }
        }

    }
}