using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Services;

/// <summary>
/// محرك تقييم RBAC على الخادم. قاعدة الأولوية هي:
/// مدير النظام، ثم Deny المستخدم، ثم Allow المستخدم، ثم Deny الدور، ثم Allow الدور.
/// كما يتحقق من نطاق الشركة/الفرع وحد الاعتماد عند طلبهما.
/// </summary>
public sealed class PermissionService
{
    private readonly AppDbContext _context;

    public PermissionService(AppDbContext context) => _context = context;

    /// <summary>تقييم صلاحية محددة لهوية جلسة موثوقة.</summary>
    public async Task<PermissionEvaluationResult> EvaluateAsync(
        ServerSession session,
        string permissionCode,
        string? companyId = null,
        int? branchId = null,
        decimal? amountLocal = null,
        string? voucherTypeCode = null)
    {
        if (string.IsNullOrWhiteSpace(permissionCode))
            return PermissionEvaluationResult.Denied("رمز الصلاحية مطلوب.");

        // مدير النظام يمر من فحص الصلاحية، لكن لا يتجاوز تحقق البيانات أو الفترة المالية.
        if (session.Is_System_Admin)
            return PermissionEvaluationResult.Allowed("مدير النظام.");

        var now = DateTime.UtcNow;
        var permission = await _context.System_Permissions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Permission_Code == permissionCode && x.Is_Active);
        if (permission is null)
            return PermissionEvaluationResult.Denied("الصلاحية غير معرفة أو غير فعالة.");

        if (!await HasCompanyAccessAsync(session.User_ID, session.Company_ID, companyId, now))
            return PermissionEvaluationResult.Denied("لا تملك نطاق الشركة المطلوب.");

        if (!await HasBranchAccessAsync(session.User_ID, session.Company_ID, session.Branch_ID, companyId, branchId, now))
            return PermissionEvaluationResult.Denied("لا تملك نطاق الفرع المطلوب.");

        var overrideEffect = await _context.User_Permission_Overrides.AsNoTracking()
            .Where(x => x.User_ID == session.User_ID &&
                        x.System_Permission_ID == permission.Permission_ID &&
                        x.Is_Active && x.Effective_From <= now &&
                        (x.Effective_To == null || x.Effective_To >= now))
            .OrderByDescending(x => x.Effective_From)
            .Select(x => x.Effect)
            .FirstOrDefaultAsync();

        if (string.Equals(overrideEffect, "Deny", StringComparison.OrdinalIgnoreCase))
            return PermissionEvaluationResult.Denied("منع صريح على المستخدم.");
        if (string.Equals(overrideEffect, "Allow", StringComparison.OrdinalIgnoreCase))
            return await CheckApprovalLimitAsync(session, companyId, branchId, amountLocal, voucherTypeCode, now);

        // يدعم النظام أدواراً متعددة. الدور الأساسي في الجلسة توافقٌ انتقالي للسجلات القديمة.
        var roleIds = await _context.User_Roles.AsNoTracking()
            .Where(x => x.User_ID == session.User_ID && x.Is_Active &&
                        x.Effective_From <= now && (x.Effective_To == null || x.Effective_To >= now))
            .Select(x => x.Role_ID)
            .ToListAsync();
        if (roleIds.Count == 0) roleIds.Add(session.Role_ID);

        var effects = await _context.RolePermissions.AsNoTracking()
            .Where(x => roleIds.Contains(x.Role_ID) &&
                        x.System_Permission_ID == permission.Permission_ID &&
                        x.Is_Active && x.Effective_From <= now &&
                        (x.Effective_To == null || x.Effective_To >= now))
            .Select(x => x.Effect)
            .ToListAsync();

        if (effects.Any(x => string.Equals(x, "Deny", StringComparison.OrdinalIgnoreCase)))
            return PermissionEvaluationResult.Denied("منع صريح من أحد الأدوار.");
        if (!effects.Any(x => string.Equals(x, "Allow", StringComparison.OrdinalIgnoreCase)))
            return PermissionEvaluationResult.Denied("لا يوجد منح فعال لهذه الصلاحية.");

        return await CheckApprovalLimitAsync(session, companyId, branchId, amountLocal, voucherTypeCode, now);
    }

    /// <summary>يتحقق من الشركة، ويقبل شركة الجلسة افتراضياً عند عدم إرسالها.</summary>
    private async Task<bool> HasCompanyAccessAsync(int userId, string sessionCompanyId, string? requestedCompanyId, DateTime now)
    {
        var companyId = string.IsNullOrWhiteSpace(requestedCompanyId) ? sessionCompanyId : requestedCompanyId.Trim();
        if (string.Equals(companyId, sessionCompanyId, StringComparison.Ordinal))
            return true;

        return await _context.Company_Access.AsNoTracking().AnyAsync(x =>
            x.User_ID == userId && x.Company_ID == companyId && x.Is_Active &&
            x.Effective_From <= now && (x.Effective_To == null || x.Effective_To >= now));
    }

    /// <summary>يتحقق من الفرع ولا يسمح بالقفز إلى فرع خارج الشركة المطلوبة.</summary>
    private async Task<bool> HasBranchAccessAsync(int userId, string sessionCompanyId, int sessionBranchId,
        string? requestedCompanyId, int? requestedBranchId, DateTime now)
    {
        var companyId = string.IsNullOrWhiteSpace(requestedCompanyId) ? sessionCompanyId : requestedCompanyId.Trim();
        var branchId = requestedBranchId ?? sessionBranchId;
        if (companyId == sessionCompanyId && branchId == sessionBranchId)
            return true;

        return await _context.Branch_Access.AsNoTracking().AnyAsync(x =>
            x.User_ID == userId && x.Company_ID == companyId && x.Branch_ID == branchId &&
            x.Is_Active && x.Effective_From <= now &&
            (x.Effective_To == null || x.Effective_To >= now));
    }

    /// <summary>يفرض حد الاعتماد عندما يرسل المستدعي مبلغاً محلياً.</summary>
    private async Task<PermissionEvaluationResult> CheckApprovalLimitAsync(
        ServerSession session, string? companyId, int? branchId, decimal? amountLocal,
        string? voucherTypeCode, DateTime now)
    {
        if (amountLocal is null) return PermissionEvaluationResult.Allowed("الصلاحية فعالة.");

        var effectiveCompany = string.IsNullOrWhiteSpace(companyId) ? session.Company_ID : companyId.Trim();
        var effectiveBranch = branchId ?? session.Branch_ID;
        var roleIds = await _context.User_Roles.AsNoTracking()
            .Where(x => x.User_ID == session.User_ID && x.Is_Active &&
                        x.Effective_From <= now && (x.Effective_To == null || x.Effective_To >= now))
            .Select(x => x.Role_ID).ToListAsync();
        if (roleIds.Count == 0) roleIds.Add(session.Role_ID);

        var limit = await _context.Approval_Limits.AsNoTracking()
            .Where(x => x.Is_Active && x.Effective_From <= now &&
                        (x.Effective_To == null || x.Effective_To >= now) &&
                        (x.User_ID == session.User_ID || (x.User_ID == null && x.Role_ID != null && roleIds.Contains(x.Role_ID.Value))) &&
                        (x.Company_ID == null || x.Company_ID == effectiveCompany) &&
                        (x.Branch_ID == null || x.Branch_ID == effectiveBranch) &&
                        (x.Voucher_Type_Code == null || x.Voucher_Type_Code == voucherTypeCode))
            .OrderByDescending(x => x.User_ID == session.User_ID)
            .ThenByDescending(x => x.Branch_ID != null)
            .ThenByDescending(x => x.Voucher_Type_Code != null)
            .Select(x => (decimal?)x.Max_Amount_Local)
            .FirstOrDefaultAsync();

        if (limit is null)
            return PermissionEvaluationResult.Denied("لا يوجد حد اعتماد فعّال لهذا المستند.");
        return amountLocal <= limit
            ? PermissionEvaluationResult.Allowed("الصلاحية والحد المالي صالحان.", limit)
            : PermissionEvaluationResult.Denied("المبلغ يتجاوز حد اعتماد المستخدم.", limit);
    }
}

/// <summary>نتيجة تقييم الصلاحية الصالحة للإظهار للعميل دون كشف تفاصيل النظام.</summary>
public sealed record PermissionEvaluationResult(bool Is_Allowed, string Reason, decimal? Approval_Limit)
{
    public static PermissionEvaluationResult Allowed(string reason, decimal? limit = null) =>
        new(true, reason, limit);
    public static PermissionEvaluationResult Denied(string reason, decimal? limit = null) =>
        new(false, reason, limit);
}