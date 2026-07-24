using System.Text.Json;
using AlTayerERP.Core.Entities.Accounting;
using AlTayerERP.Infrastructure.Data;

namespace AlTayerERP.API.Services
{
    /// <summary>
    /// خدمة سجل التدقيق المركزي. مصدر الهوية والسياق هو ServerSession فقط،
    /// لذلك لا يقبل الخادم اسم مستخدم أو فرعاً مزوراً من تطبيق العميل.
    /// </summary>
    public sealed class AuditTrailService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _http;

        public AuditTrailService(AppDbContext context, IHttpContextAccessor http)
        {
            _context = context;
            _http = http;
        }

        /// <summary>يسجل العملية دون حفظ مستقل، كي تبقى مع العملية الأساسية في Transaction واحد.</summary>
        public void Add(
            string tableName,
            string recordId,
            string actionType,
            object? oldValues = null,
            object? newValues = null,
            string? notes = null)
        {
            var context = _http.HttpContext;
            var session = context?.Items["ServerSession"] as ServerSession;
            var correlationId = context?.TraceIdentifier;
            var device = context?.Request.Headers["X-Device-ID"].ToString();

            _context.Audit_Logs.Add(new AuditLog
            {
                Table_Name = tableName,
                Record_ID = recordId,
                Action_Type = actionType,
                User_ID = session?.User_ID.ToString(),
                Branch_ID = session?.Branch_ID.ToString(),
                Action_At = DateTime.UtcNow,
                Old_Values = oldValues == null ? null : JsonSerializer.Serialize(oldValues),
                New_Values = newValues == null ? null : JsonSerializer.Serialize(newValues),
                Action_Channel = "API",
                Device_Name = Trim(device, 150),
                IP_Address = Trim(context?.Connection.RemoteIpAddress?.ToString(), 50),
                Notes = Trim(string.IsNullOrWhiteSpace(notes)
                    ? $"Correlation-ID: {correlationId}"
                    : $"{notes} | Correlation-ID: {correlationId}", 500)
            });
        }

        private static string? Trim(string? value, int maxLength) =>
            string.IsNullOrWhiteSpace(value) ? null :
            value.Trim().Length <= maxLength ? value.Trim() : value.Trim()[..maxLength];
    }
}
