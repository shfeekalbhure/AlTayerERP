using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Services
{
    // ======================================================
    // محرك السياسات المالية
    // مسؤول عن فحص السقوف المالية وإنشاء طلب اعتماد عند التجاوز
    // ======================================================
    public class FinancialPolicyService
    {
        private readonly AppDbContext _context;
        private readonly ApprovalService _approvalService;

        public FinancialPolicyService(
            AppDbContext context,
            ApprovalService approvalService)
        {
            _context = context;
            _approvalService = approvalService;
        }

        // ======================================================
        // فحص عملية مالية مقابل سياسة الرقابة المالية
        // ======================================================
        public async Task<bool> CheckPolicyAsync(
            string companyId,
            string entityType,
            string entityId,
            string limitType,
            string currencyCode,
            decimal amount,
            string? referenceType,
            string? referenceId,
            string? requestedBy)
        {
            var policy = await _context.Financial_Policies
                .FirstOrDefaultAsync(x =>
                    x.Company_ID == companyId &&
                    x.Entity_Type == entityType &&
                    x.Entity_ID == entityId &&
                    x.Limit_Type == limitType &&
                    x.Currency_Code == currencyCode &&
                    x.Is_Active);

            // إذا لا توجد سياسة، نسمح بالعملية مؤقتًا
            if (policy == null)
                return true;

            decimal newUsedAmount = policy.Used_Amount + amount;

            // إذا داخل السقف
            if (newUsedAmount <= policy.Limit_Amount)
            {
                policy.Used_Amount = newUsedAmount;
                policy.Updated_At = DateTime.UtcNow;

                await AddMovementAsync(
                    policy,
                    "WithinLimit",
                    referenceType,
                    referenceId,
                    amount,
                    newUsedAmount,
                    "العملية داخل السقف",
                    requestedBy);

                await _context.SaveChangesAsync();
                return true;
            }

            // إذا تجاوز السقف ويحتاج اعتماد
            if (policy.Requires_Approval)
            {
                await _approvalService.CreateApprovalRequestAsync(
                    companyId,
                    "FinancialLimitExceeded",
                    referenceType,
                    referenceId,
                    entityType,
                    entityId,
                    currencyCode,
                    amount,
                    "تم تجاوز السقف المالي المحدد",
                    requestedBy);

                await AddMovementAsync(
                    policy,
                    "Exceeded",
                    referenceType,
                    referenceId,
                    amount,
                    policy.Used_Amount,
                    "تم تجاوز السقف وتم إنشاء طلب اعتماد",
                    requestedBy);

                await _context.SaveChangesAsync();
                return false;
            }

            // إذا تجاوز السقف ولا يحتاج اعتماد، نمنع العملية
            return false;
        }

        // ======================================================
        // تسجيل حركة على السياسة المالية
        // ======================================================
        private async Task AddMovementAsync(
            FinancialPolicy policy,
            string movementType,
            string? referenceType,
            string? referenceId,
            decimal amount,
            decimal balanceAfter,
            string notes,
            string? createdBy)
        {
            var movement = new FinancialPolicyMovement
            {
                Limit_ID = policy.Limit_ID,
                Company_ID = policy.Company_ID,
                Movement_Date = DateTime.UtcNow,
                Movement_Type = movementType,
                Reference_Type = referenceType,
                Reference_ID = referenceId,
                Currency_Code = policy.Currency_Code,
                Amount = amount,
                Balance_After = balanceAfter,
                Notes = notes,
                Created_By = createdBy,
                Created_At = DateTime.UtcNow
            };

            await _context.Financial_Policy_Movements.AddAsync(movement);
        }
    }
}