using System;
using System.Linq;
using System.Threading.Tasks;
using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Services.Accounting.VoucherWorkflow
{
    /// <summary>
    /// خدمة إدارة دورة اعتماد السندات المالية.
    ///
    /// تدعم:
    /// 1- اعتماد السند.
    /// 2- إلغاء الاعتماد.
    /// 3- رفض السند.
    /// 4- طلب تعديل السند.
    ///
    /// تستخدم لجميع أنواع السندات:
    /// سند قبض، سند صرف، قيد يومية، إشعار مدين، إشعار دائن.
    /// </summary>
    public class VoucherApprovalService
    {
        #region حالات الاعتماد

        /// <summary>
        /// لا يحتاج إلى اعتماد أو لم تبدأ دورة الاعتماد.
        /// </summary>
        public const byte ApprovalNotRequired = 0;

        /// <summary>
        /// السند بانتظار الاعتماد.
        /// </summary>
        public const byte ApprovalPending = 1;

        /// <summary>
        /// تم اعتماد السند.
        /// </summary>
        public const byte ApprovalApproved = 2;

        /// <summary>
        /// تم رفض السند.
        /// </summary>
        public const byte ApprovalRejected = 3;

        /// <summary>
        /// تمت إعادة السند لطلب تعديل.
        /// </summary>
        public const byte ApprovalNeedsRevision = 4;

        #endregion

        #region المتغيرات

        private readonly AppDbContext _context;
        private readonly VoucherAuditService _auditService;

        #endregion

        #region المشيد

        public VoucherApprovalService(
            AppDbContext context,
            VoucherAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        #endregion

        #region اعتماد السند

        /// <summary>
        /// اعتماد سند مالي.
        /// </summary>
        public async Task<(bool Success, string Message)> ApproveAsync(
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
                return (false, "معرف السند غير صحيح.");
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                return (false, "معرف المستخدم مطلوب لتنفيذ الاعتماد.");
            }

            userId = userId.Trim();

            #endregion

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                #region جلب السند

                var voucher =
                    await _context.Financial_Voucher_Headers
                        .Include(x => x.Details)
                        .FirstOrDefaultAsync(x =>
                            x.Voucher_ID == voucherId);

                if (voucher == null)
                {
                    await transaction.RollbackAsync();

                    return (false, "السند المالي غير موجود.");
                }

                #endregion

                #region التحقق من حالة السند

                if (!voucher.Is_Active)
                {
                    await transaction.RollbackAsync();

                    return (false, "لا يمكن اعتماد سند ملغي أو غير نشط.");
                }

                if (!voucher.Requires_Approval)
                {
                    await transaction.RollbackAsync();

                    return (false, "هذا السند لا يتطلب اعتمادًا.");
                }

                if (voucher.Approval_Status == ApprovalApproved)
                {
                    await transaction.RollbackAsync();

                    return (false, "السند معتمد مسبقًا.");
                }

                if (voucher.Is_Posted)
                {
                    await transaction.RollbackAsync();

                    return (
                        false,
                        "لا يمكن اعتماد السند لأنه مرحل محاسبيًا مسبقًا."
                    );
                }

                if (voucher.Details == null ||
                    voucher.Details.Count == 0)
                {
                    await transaction.RollbackAsync();

                    return (
                        false,
                        "لا يمكن اعتماد سند لا يحتوي على تفاصيل محاسبية."
                    );
                }

                #endregion

                #region التحقق من توازن السند

                decimal totalDebit =
                    decimal.Round(
                        voucher.Details.Sum(x => x.Debit_Amount),
                        2);

                decimal totalCredit =
                    decimal.Round(
                        voucher.Details.Sum(x => x.Credit_Amount),
                        2);

                if (totalDebit <= 0 &&
                    totalCredit <= 0)
                {
                    await transaction.RollbackAsync();

                    return (
                        false,
                        "لا يمكن اعتماد سند لا يحتوي على مبالغ."
                    );
                }

                if (totalDebit != totalCredit)
                {
                    await transaction.RollbackAsync();

                    return (
                        false,
                        $"السند غير متوازن. المدين: {totalDebit:N2}، " +
                        $"الدائن: {totalCredit:N2}."
                    );
                }

                #endregion

                #region تحديث بيانات الاعتماد

                DateTime actionDate =
                    DateTime.Now;

                voucher.Approval_Status =
                    ApprovalApproved;

                voucher.Approved_By_User_ID =
                    userId;

                voucher.Approved_At =
                    actionDate;

                voucher.Rejected_By_User_ID =
                    null;

                voucher.Rejected_At =
                    null;

                voucher.Rejection_Reason =
                    null;

                voucher.Updated_By =
                    userId;

                voucher.Updated_At =
                    actionDate;

                #endregion

                #region تسجيل حركة الاعتماد

                await _auditService.LogAsync(
                    voucherId: voucher.Voucher_ID,
                    actionType: VoucherAuditService.APPROVE,
                    oldStatusId: voucher.Voucher_Status_ID,
                    newStatusId: voucher.Voucher_Status_ID,
                    userId: userId,
                    actionChannel: actionChannel,
                    deviceName: deviceName,
                    ipAddress: ipAddress,
                    reason: null,
                    notes: string.IsNullOrWhiteSpace(notes)
                        ? "تم اعتماد السند المالي."
                        : notes.Trim());

                #endregion

                await transaction.CommitAsync();

                return (
                    true,
                    $"تم اعتماد السند رقم {voucher.Voucher_No} بنجاح."
                );
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();

                string error =
                    ex.InnerException?.Message ??
                    ex.Message;

                return (
                    false,
                    $"تعذر اعتماد السند في قاعدة البيانات: {error}"
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return (
                    false,
                    $"حدث خطأ أثناء اعتماد السند: {ex.Message}"
                );
            }
        }

        #endregion

        #region إلغاء الاعتماد

        /// <summary>
        /// إلغاء اعتماد سند وإعادته إلى حالة انتظار الاعتماد.
        /// لا يسمح بإلغاء الاعتماد إذا كان السند مرحلًا.
        /// </summary>
        public async Task<(bool Success, string Message)>
            CancelApprovalAsync(
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
                return (false, "معرف السند غير صحيح.");
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                return (
                    false,
                    "معرف المستخدم مطلوب لإلغاء الاعتماد."
                );
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                return (
                    false,
                    "سبب إلغاء الاعتماد مطلوب."
                );
            }

            userId = userId.Trim();
            reason = reason.Trim();

            #endregion

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var voucher =
                    await _context.Financial_Voucher_Headers
                        .FirstOrDefaultAsync(x =>
                            x.Voucher_ID == voucherId);

                if (voucher == null)
                {
                    await transaction.RollbackAsync();

                    return (false, "السند المالي غير موجود.");
                }

                if (!voucher.Is_Active)
                {
                    await transaction.RollbackAsync();

                    return (
                        false,
                        "لا يمكن إلغاء اعتماد سند ملغي أو غير نشط."
                    );
                }

                if (voucher.Approval_Status !=
                    ApprovalApproved)
                {
                    await transaction.RollbackAsync();

                    return (
                        false,
                        "السند غير معتمد حاليًا."
                    );
                }

                if (voucher.Is_Posted)
                {
                    await transaction.RollbackAsync();

                    return (
                        false,
                        "لا يمكن إلغاء الاعتماد لأن السند مرحل محاسبيًا. " +
                        "يجب إلغاء الترحيل أولًا."
                    );
                }

                DateTime actionDate =
                    DateTime.Now;

                voucher.Approval_Status =
                    ApprovalPending;

                voucher.Approved_By_User_ID =
                    null;

                voucher.Approved_At =
                    null;

                voucher.Updated_By =
                    userId;

                voucher.Updated_At =
                    actionDate;

                await _auditService.LogAsync(
                    voucherId: voucher.Voucher_ID,
                    actionType:
                        VoucherAuditService.CANCEL_APPROVAL,
                    oldStatusId: voucher.Voucher_Status_ID,
                    newStatusId: voucher.Voucher_Status_ID,
                    userId: userId,
                    actionChannel: actionChannel,
                    deviceName: deviceName,
                    ipAddress: ipAddress,
                    reason: reason,
                    notes: "تم إلغاء اعتماد السند وإعادته للانتظار.");

                await transaction.CommitAsync();

                return (
                    true,
                    $"تم إلغاء اعتماد السند رقم " +
                    $"{voucher.Voucher_No} بنجاح."
                );
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();

                string error =
                    ex.InnerException?.Message ??
                    ex.Message;

                return (
                    false,
                    $"تعذر إلغاء الاعتماد في قاعدة البيانات: {error}"
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return (
                    false,
                    $"حدث خطأ أثناء إلغاء الاعتماد: {ex.Message}"
                );
            }
        }

        #endregion

        #region رفض السند

        /// <summary>
        /// رفض السند مع تسجيل سبب الرفض.
        /// </summary>
        public async Task<(bool Success, string Message)> RejectAsync(
            long voucherId,
            string userId,
            string reason,
            string actionChannel = VoucherAuditService.DESKTOP,
            string? deviceName = null,
            string? ipAddress = null)
        {
            if (voucherId <= 0)
            {
                return (false, "معرف السند غير صحيح.");
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                return (
                    false,
                    "معرف المستخدم مطلوب لرفض السند."
                );
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                return (
                    false,
                    "سبب رفض السند مطلوب."
                );
            }

            userId = userId.Trim();
            reason = reason.Trim();

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var voucher =
                    await _context.Financial_Voucher_Headers
                        .FirstOrDefaultAsync(x =>
                            x.Voucher_ID == voucherId);

                if (voucher == null)
                {
                    await transaction.RollbackAsync();

                    return (false, "السند المالي غير موجود.");
                }

                if (!voucher.Is_Active)
                {
                    await transaction.RollbackAsync();

                    return (
                        false,
                        "لا يمكن رفض سند ملغي أو غير نشط."
                    );
                }

                if (voucher.Is_Posted)
                {
                    await transaction.RollbackAsync();

                    return (
                        false,
                        "لا يمكن رفض سند مرحل محاسبيًا."
                    );
                }

                if (voucher.Approval_Status ==
                    ApprovalApproved)
                {
                    await transaction.RollbackAsync();

                    return (
                        false,
                        "السند معتمد. يجب إلغاء الاعتماد أولًا."
                    );
                }

                DateTime actionDate =
                    DateTime.Now;

                voucher.Approval_Status =
                    ApprovalRejected;

                voucher.Rejected_By_User_ID =
                    userId;

                voucher.Rejected_At =
                    actionDate;

                voucher.Rejection_Reason =
                    reason;

                voucher.Approved_By_User_ID =
                    null;

                voucher.Approved_At =
                    null;

                voucher.Updated_By =
                    userId;

                voucher.Updated_At =
                    actionDate;

                await _auditService.LogAsync(
                    voucherId: voucher.Voucher_ID,
                    actionType: VoucherAuditService.REJECT,
                    oldStatusId: voucher.Voucher_Status_ID,
                    newStatusId: voucher.Voucher_Status_ID,
                    userId: userId,
                    actionChannel: actionChannel,
                    deviceName: deviceName,
                    ipAddress: ipAddress,
                    reason: reason,
                    notes: "تم رفض السند المالي.");

                await transaction.CommitAsync();

                return (
                    true,
                    $"تم رفض السند رقم {voucher.Voucher_No}."
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return (
                    false,
                    $"حدث خطأ أثناء رفض السند: {ex.Message}"
                );
            }
        }

        #endregion

        #region طلب تعديل

        /// <summary>
        /// إعادة السند إلى المدخل من أجل التعديل.
        /// </summary>
        public async Task<(bool Success, string Message)>
            RequestRevisionAsync(
                long voucherId,
                string userId,
                string revisionNotes,
                string actionChannel =
                    VoucherAuditService.DESKTOP,
                string? deviceName = null,
                string? ipAddress = null)
        {
            if (voucherId <= 0)
            {
                return (false, "معرف السند غير صحيح.");
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                return (
                    false,
                    "معرف المستخدم مطلوب لطلب التعديل."
                );
            }

            if (string.IsNullOrWhiteSpace(revisionNotes))
            {
                return (
                    false,
                    "ملاحظات التعديل مطلوبة."
                );
            }

            userId = userId.Trim();
            revisionNotes = revisionNotes.Trim();

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var voucher =
                    await _context.Financial_Voucher_Headers
                        .FirstOrDefaultAsync(x =>
                            x.Voucher_ID == voucherId);

                if (voucher == null)
                {
                    await transaction.RollbackAsync();

                    return (false, "السند المالي غير موجود.");
                }

                if (!voucher.Is_Active)
                {
                    await transaction.RollbackAsync();

                    return (
                        false,
                        "لا يمكن طلب تعديل سند ملغي أو غير نشط."
                    );
                }

                if (voucher.Is_Posted)
                {
                    await transaction.RollbackAsync();

                    return (
                        false,
                        "لا يمكن طلب تعديل سند مرحل محاسبيًا."
                    );
                }

                if (voucher.Approval_Status ==
                    ApprovalApproved)
                {
                    await transaction.RollbackAsync();

                    return (
                        false,
                        "السند معتمد. يجب إلغاء الاعتماد أولًا."
                    );
                }

                DateTime actionDate =
                    DateTime.Now;

                voucher.Approval_Status =
                    ApprovalNeedsRevision;

                voucher.Rejected_By_User_ID =
                    null;

                voucher.Rejected_At =
                    null;

                voucher.Rejection_Reason =
                    revisionNotes;

                voucher.Approved_By_User_ID =
                    null;

                voucher.Approved_At =
                    null;

                voucher.Updated_By =
                    userId;

                voucher.Updated_At =
                    actionDate;

                await _auditService.LogAsync(
                    voucherId: voucher.Voucher_ID,
                    actionType:
                        VoucherAuditService.REQUEST_REVISION,
                    oldStatusId: voucher.Voucher_Status_ID,
                    newStatusId: voucher.Voucher_Status_ID,
                    userId: userId,
                    actionChannel: actionChannel,
                    deviceName: deviceName,
                    ipAddress: ipAddress,
                    reason: revisionNotes,
                    notes: "تمت إعادة السند إلى المدخل للتعديل.");

                await transaction.CommitAsync();

                return (
                    true,
                    $"تمت إعادة السند رقم " +
                    $"{voucher.Voucher_No} للتعديل."
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return (
                    false,
                    $"حدث خطأ أثناء طلب تعديل السند: {ex.Message}"
                );
            }
        }

        #endregion

        #region جلب حالة الاعتماد

        /// <summary>
        /// جلب حالة اعتماد السند لاستخدامها في الكمبيوتر أو الهاتف.
        /// </summary>
        public async Task<VoucherApprovalStatusResult?>
            GetApprovalStatusAsync(long voucherId)
        {
            if (voucherId <= 0)
            {
                return null;
            }

            return await _context
                .Financial_Voucher_Headers
                .AsNoTracking()
                .Where(x => x.Voucher_ID == voucherId)
                .Select(x =>
                    new VoucherApprovalStatusResult
                    {
                        Voucher_ID = x.Voucher_ID,
                        Voucher_No = x.Voucher_No,

                        Requires_Approval =
                            x.Requires_Approval,

                        Approval_Status =
                            x.Approval_Status,

                        Approval_Status_Name =
                            GetApprovalStatusName(
                                x.Approval_Status),

                        Approved_By_User_ID =
                            x.Approved_By_User_ID,

                        Approved_At =
                            x.Approved_At,

                        Rejected_By_User_ID =
                            x.Rejected_By_User_ID,

                        Rejected_At =
                            x.Rejected_At,

                        Rejection_Reason =
                            x.Rejection_Reason,

                        Is_Posted =
                            x.Is_Posted
                    })
                .FirstOrDefaultAsync();
        }

        #endregion

        #region اسم حالة الاعتماد

        private static string GetApprovalStatusName(
            byte approvalStatus)
        {
            return approvalStatus switch
            {
                ApprovalNotRequired =>
                    "لا يتطلب اعتمادًا",

                ApprovalPending =>
                    "بانتظار الاعتماد",

                ApprovalApproved =>
                    "معتمد",

                ApprovalRejected =>
                    "مرفوض",

                ApprovalNeedsRevision =>
                    "يحتاج تعديل",

                _ =>
                    "حالة غير معروفة"
            };
        }

        #endregion

        #region نموذج حالة الاعتماد

        public sealed class VoucherApprovalStatusResult
        {
            public long Voucher_ID { get; set; }

            public string Voucher_No { get; set; }
                = string.Empty;

            public bool Requires_Approval { get; set; }

            public byte Approval_Status { get; set; }

            public string Approval_Status_Name { get; set; }
                = string.Empty;

            public string? Approved_By_User_ID { get; set; }

            public DateTime? Approved_At { get; set; }

            public string? Rejected_By_User_ID { get; set; }

            public DateTime? Rejected_At { get; set; }

            public string? Rejection_Reason { get; set; }

            public bool Is_Posted { get; set; }
        }

        #endregion
    }
}