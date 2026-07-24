using System.Text.Json;
using AlTayerERP.Core.Entities.Accounting;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Http;

namespace AlTayerERP.API.Services
{
    /// <summary>
    /// خدمة سجل التدقيق المركزي. تمرر لها هوية الجلسة الموثوقة من Controller،
    /// ولا تقبل أبداً اسم المستخدم أو الفرع من جسم طلب العميل.
    /// </summary>
    public sealed class AuditTrailService
    {
        private readonly AppDbContext _context;

        public AuditTrailService(AppDbContext context) => _context = context;

        /// <summary>يضيف السجل بلا حفظ مستقل كي يبقى ضمن Transaction العملية.</summary>
        public void Add(
            ServerSession session,
            HttpContext httpContext,
            string tableName,
            string recordId,
            string actionType,
            object? oldValues = null,
            object? newValues = null,
            string? notes = null)
        {
            var correlationId = httpContext.TraceIdentifier;
            var device = httpContext.Request.Headers["X-Device-ID"].ToString();

            _context.Audit_Logs.Add(new AuditLog
            {
                Table_Name = tableName,
                Record_ID = recordId,
                Action_Type = actionType,
                User_ID = session.User_ID.ToString(),
                Branch_ID = session.Branch_ID.ToString(),
                Action_At = DateTime.UtcNow,
                Old_Values = oldValues == null ? null : JsonSerializer.Serialize(oldValues),
                New_Values = newValues == null ? null : JsonSerializer.Serialize(newValues),
                Action_Channel = "API",
                Device_Name = Trim(device, 150),
                IP_Address = Trim(httpContext.Connection.RemoteIpAddress?.ToString(), 50),
                Notes = Trim(string.IsNullOrWhiteSpace(notes)
                    ? $"Correlation-ID: {correlationId}"
                    : $"{notes} | Correlation-ID: {correlationId}", 500)
            });
        }

        /// <summary>يستخدم عند عملية تدقيق مستقلة لا تملك حفظاً لاحقاً.</summary>
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
            _context.SaveChangesAsync(cancellationToken);

        private static string? Trim(string? value, int maxLength) =>
            string.IsNullOrWhiteSpace(value) ? null :
            value.Trim().Length <= maxLength ? value.Trim() : value.Trim()[..maxLength];
    }
}
