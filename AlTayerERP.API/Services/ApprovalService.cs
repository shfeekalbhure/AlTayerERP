using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;

namespace AlTayerERP.API.Services
{
    // ======================================================
    // محرك الاعتمادات
    // مسؤول عن إنشاء طلب اعتماد عند تجاوز سقف أو صلاحية
    // ======================================================
    public class ApprovalService
    {
        private readonly AppDbContext _context;

        public ApprovalService(AppDbContext context)
        {
            _context = context;
        }

        // ======================================================
        // إنشاء طلب اعتماد جديد
        // ======================================================
        public async Task<ApprovalRequest> CreateApprovalRequestAsync(
            string companyId,
            string requestType,
            string? referenceType,
            string? referenceId,
            string? entityType,
            string? entityId,
            string? currencyCode,
            decimal? amount,
            string? reason,
            string? requestedBy)
        {
            var request = new ApprovalRequest
            {
                Company_ID = companyId,
                Request_Type = requestType,
                Reference_Type = referenceType,
                Reference_ID = referenceId,
                Entity_Type = entityType,
                Entity_ID = entityId,
                Currency_Code = currencyCode,
                Amount = amount,
                Reason = reason,
                Status = ApprovalStatus.Pending.ToString(),
                Requested_By = requestedBy,
                Requested_At = DateTime.UtcNow
            };

            await _context.Approval_Requests.AddAsync(request);
            await _context.SaveChangesAsync();

            return request;
        }
    }
}