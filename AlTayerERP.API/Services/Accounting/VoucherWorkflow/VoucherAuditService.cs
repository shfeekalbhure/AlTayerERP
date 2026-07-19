using System;
using System.Threading.Tasks;
using AlTayerERP.Core.Entities.Accounting;
using AlTayerERP.Infrastructure.Data;

namespace AlTayerERP.API.Services.Accounting.VoucherWorkflow
{
    /// <summary>
    /// خدمة تسجيل جميع العمليات التي تتم على السندات المالية.
    /// تعتبر هذه الخدمة المرجع الوحيد لتسجيل الأحداث
    /// داخل جدول Voucher_Action_Logs.
    /// </summary>
    public class VoucherAuditService
    {
        #region المتغيرات

        private readonly AppDbContext _context;

        #endregion

        #region المشيد

        public VoucherAuditService(
            AppDbContext context)
        {
            _context = context;
        }

        #endregion

        #region أنواع العمليات

        public const string CREATE = "CREATE";

        public const string UPDATE = "UPDATE";

        public const string DELETE = "DELETE";

        public const string APPROVE = "APPROVE";

        public const string REJECT = "REJECT";

        public const string REQUEST_REVISION = "REQUEST_REVISION";

        public const string CANCEL_APPROVAL = "CANCEL_APPROVAL";

        public const string POST = "POST";

        public const string UNPOST = "UNPOST";

        public const string PRINT = "PRINT";

        public const string EXPORT = "EXPORT";

        public const string IMPORT = "IMPORT";

        public const string ATTACHMENT_ADD = "ATTACHMENT_ADD";

        public const string ATTACHMENT_DELETE = "ATTACHMENT_DELETE";

        public const string UNDO = "UNDO";

        public const string RESTORE = "RESTORE";

        #endregion

        #region مصادر التنفيذ

        public const string DESKTOP = "DESKTOP";

        public const string MOBILE = "MOBILE";

        public const string WEB = "WEB";

        public const string API = "API";

        #endregion

        #region التسجيل العام

        /// <summary>
        /// تسجيل أي عملية تمت على السند المالي.
        /// </summary>
        public async Task LogAsync(
            long voucherId,
            string actionType,
            int? oldStatusId,
            int? newStatusId,
            string? userId,
            string actionChannel,
            string? deviceName = null,
            string? ipAddress = null,
            string? reason = null,
            string? notes = null)
        {
            VoucherActionLog log =
                new VoucherActionLog
                {
                    Voucher_ID = voucherId,

                    Action_Type = actionType,

                    Old_Status_ID = oldStatusId,

                    New_Status_ID = newStatusId,

                    User_ID = userId,

                    Action_At = DateTime.Now,

                    Action_Channel = actionChannel,

                    Device_Name = deviceName,

                    IP_Address = ipAddress,

                    Reason = reason,

                    Notes = notes
                };

            await _context
                .Voucher_Action_Logs
                .AddAsync(log);

            await _context
                .SaveChangesAsync();
        }

        #endregion
        #region إنشاء السند

        /// <summary>
        /// تسجيل إنشاء سند جديد.
        /// </summary>
        public async Task LogCreateAsync(
            long voucherId,
            int newStatusId,
            string? userId,
            string channel,
            string? deviceName = null,
            string? ipAddress = null)
        {
            await LogAsync(
                voucherId,
                CREATE,
                null,
                newStatusId,
                userId,
                channel,
                deviceName,
                ipAddress,
                null,
                "تم إنشاء السند.");
        }

        #endregion

        #region تعديل السند

        /// <summary>
        /// تسجيل تعديل سند.
        /// </summary>
        public async Task LogUpdateAsync(
            long voucherId,
            int oldStatusId,
            int newStatusId,
            string? userId,
            string channel,
            string? deviceName = null,
            string? ipAddress = null,
            string? notes = null)
        {
            await LogAsync(
                voucherId,
                UPDATE,
                oldStatusId,
                newStatusId,
                userId,
                channel,
                deviceName,
                ipAddress,
                null,
                notes);
        }

        #endregion

        #region حذف السند

        /// <summary>
        /// تسجيل حذف سند.
        /// </summary>
        public async Task LogDeleteAsync(
            long voucherId,
            int oldStatusId,
            string? userId,
            string channel,
            string? reason = null)
        {
            await LogAsync(
                voucherId,
                DELETE,
                oldStatusId,
                null,
                userId,
                channel,
                Environment.MachineName,
                null,
                reason,
                "تم حذف السند.");
        }

        #endregion

        #region اعتماد السند

        public async Task LogApproveAsync(
            long voucherId,
            int oldStatusId,
            int newStatusId,
            string? userId,
            string channel)
        {
            await LogAsync(
                voucherId,
                APPROVE,
                oldStatusId,
                newStatusId,
                userId,
                channel,
                Environment.MachineName,
                null,
                null,
                "تم اعتماد السند.");
        }

        #endregion

        #region رفض السند

        public async Task LogRejectAsync(
            long voucherId,
            int oldStatusId,
            int newStatusId,
            string? userId,
            string channel,
            string reason)
        {
            await LogAsync(
                voucherId,
                REJECT,
                oldStatusId,
                newStatusId,
                userId,
                channel,
                Environment.MachineName,
                null,
                reason,
                "تم رفض السند.");
        }

        #endregion

        #region طلب تعديل

        public async Task LogRequestRevisionAsync(
            long voucherId,
            int oldStatusId,
            int newStatusId,
            string? userId,
            string channel,
            string notes)
        {
            await LogAsync(
                voucherId,
                REQUEST_REVISION,
                oldStatusId,
                newStatusId,
                userId,
                channel,
                Environment.MachineName,
                null,
                null,
                notes);
        }

        #endregion

        #region إلغاء الاعتماد

        public async Task LogCancelApprovalAsync(
            long voucherId,
            int oldStatusId,
            int newStatusId,
            string? userId,
            string channel,
            string reason)
        {
            await LogAsync(
                voucherId,
                CANCEL_APPROVAL,
                oldStatusId,
                newStatusId,
                userId,
                channel,
                Environment.MachineName,
                null,
                reason,
                "تم إلغاء اعتماد السند.");
        }

        #endregion

        #region ترحيل السند

        public async Task LogPostAsync(
            long voucherId,
            int statusId,
            string? userId,
            string channel)
        {
            await LogAsync(
                voucherId,
                POST,
                statusId,
                statusId,
                userId,
                channel,
                Environment.MachineName,
                null,
                null,
                "تم ترحيل السند.");
        }

        #endregion

        #region إلغاء الترحيل

        public async Task LogUnpostAsync(
            long voucherId,
            int statusId,
            string? userId,
            string channel,
            string reason)
        {
            await LogAsync(
                voucherId,
                UNPOST,
                statusId,
                statusId,
                userId,
                channel,
                Environment.MachineName,
                null,
                reason,
                "تم إلغاء ترحيل السند.");
        }

        #endregion

        #region طباعة

        public async Task LogPrintAsync(
            long voucherId,
            int statusId,
            string? userId,
            string channel)
        {
            await LogAsync(
                voucherId,
                PRINT,
                statusId,
                statusId,
                userId,
                channel,
                Environment.MachineName,
                null,
                null,
                "تمت طباعة السند.");
        }

        #endregion

        #region تصدير

        public async Task LogExportAsync(
            long voucherId,
            int statusId,
            string? userId,
            string channel)
        {
            await LogAsync(
                voucherId,
                EXPORT,
                statusId,
                statusId,
                userId,
                channel,
                Environment.MachineName,
                null,
                null,
                "تم تصدير السند.");
        }

        #endregion

        #region استيراد

        public async Task LogImportAsync(
            long voucherId,
            int statusId,
            string? userId,
            string channel)
        {
            await LogAsync(
                voucherId,
                IMPORT,
                statusId,
                statusId,
                userId,
                channel,
                Environment.MachineName,
                null,
                null,
                "تم استيراد بيانات السند.");
        }

        #endregion

        #region المرفقات

        public async Task LogAttachmentAsync(
            long voucherId,
            string action,
            string? userId,
            string channel,
            string? notes = null)
        {
            await LogAsync(
                voucherId,
                action,
                null,
                null,
                userId,
                channel,
                Environment.MachineName,
                null,
                null,
                notes);
        }

        #endregion
    }
}