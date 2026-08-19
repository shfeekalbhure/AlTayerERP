using AlTayerERP.API.Security;
using AlTayerERP.API.Services;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers;

/// <summary>
/// يوفر خيارات شاشة دخول الجوال دون إنشاء جلسة، ثم يتحقق من بيانات المستخدم
/// ويعيد فقط الفروع والسنوات المالية المصرح بها.
/// </summary>
[ApiController]
[Route("api/Auth")]
public sealed class LoginOptionsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly LoginSecurityService _loginSecurity;
    private readonly ILogger<LoginOptionsController> _logger;

    public LoginOptionsController(
        AppDbContext context,
        LoginSecurityService loginSecurity,
        ILogger<LoginOptionsController> logger)
    {
        _context = context;
        _loginSecurity = loginSecurity;
        _logger = logger;
    }

    /// <summary>
    /// يعيد الحد الأدنى اللازم من الشركات النشطة لشاشة الدخول.
    /// لا تظهر إلا شركات المجموعات النشطة المسموح بإظهارها في شاشة اختيار الشركة.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("LoginCompanies")]
    public async Task<IActionResult> GetLoginCompanies(CancellationToken cancellationToken)
    {
        var companies = await _context.Companies
            .AsNoTracking()
            .Join(
                _context.Tenant_Groups.AsNoTracking(),
                companyEntity => companyEntity.Group_ID,
                groupEntity => groupEntity.Group_ID,
                (companyEntity, groupEntity) => new
                {
                    Company = companyEntity,
                    Group = groupEntity
                })
            .Where(x =>
                x.Company.Is_Active &&
                x.Group.Is_Active &&
                x.Group.Show_In_Login)
            .OrderByDescending(x => x.Group.Is_Default)
            .ThenBy(x => x.Company.Company_Name_AR)
            .Select(x => new LoginCompanyOptionDto
            {
                Company_ID = x.Company.Company_ID,
                Company_Name = x.Company.Company_Name_AR
            })
            .ToListAsync(cancellationToken);

        return Ok(companies);
    }

    [AllowAnonymous]
    [HttpPost("LoginOptions")]
    public async Task<IActionResult> GetLoginOptions(
        [FromBody] LoginOptionsRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request == null ||
            string.IsNullOrWhiteSpace(request.Company_ID) ||
            string.IsNullOrWhiteSpace(request.Login_Name) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("الشركة واسم المستخدم وكلمة المرور مطلوبة.");
        }

        var companyId = request.Company_ID.Trim();
        var loginName = request.Login_Name.Trim();
        var deviceId = NormalizeDeviceId(request.Device_ID);
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(
                x => x.Login_Name == loginName,
                cancellationToken);

            if (user == null || !user.Is_Active || _loginSecurity.IsLocked(user))
                return await FailedAsync(user, loginName, companyId, "INVALID_CREDENTIALS",
                    ipAddress, userAgent, deviceId, cancellationToken);

            var role = await _context.Roles.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Role_ID == user.Role_ID && x.Is_Active, cancellationToken);

            if (role == null ||
                !PasswordProtector.Verify(user.Password_Hash, request.Password, out _))
            {
                return await FailedAsync(user, loginName, companyId, "INVALID_CREDENTIALS",
                    ipAddress, userAgent, deviceId, cancellationToken);
            }

            var companyExists = await _context.Companies.AsNoTracking()
                .AnyAsync(x => x.Company_ID == companyId && x.Is_Active, cancellationToken);

            if (!companyExists ||
                (!role.Is_System_Admin &&
                 !string.Equals(user.Company_ID?.Trim(), companyId, StringComparison.Ordinal)))
            {
                return await FailedAsync(user, loginName, companyId, "TENANT_ACCESS_DENIED",
                    ipAddress, userAgent, deviceId, cancellationToken);
            }

            var branchesQuery = _context.Tenant_Branches.AsNoTracking()
                .Where(x => x.Company_ID == companyId && x.Is_Active);

            if (!role.Is_System_Admin)
                branchesQuery = branchesQuery.Where(x => x.Branch_ID == user.Branch_ID);

            var branches = await branchesQuery
                .OrderBy(x => x.Branch_Name)
                .Select(x => new LoginBranchOptionDto
                {
                    Branch_ID = x.Branch_ID,
                    Branch_Code = x.Branch_Code,
                    Branch_Name = x.Branch_Name,
                    Is_Default = x.Branch_ID == user.Branch_ID
                })
                .ToListAsync(cancellationToken);

            var years = await _context.Fiscal_Years.AsNoTracking()
                .Where(x => x.Company_ID == companyId && x.Is_Active && !x.Is_Closed)
                .OrderByDescending(x => x.Is_Default)
                .ThenByDescending(x => x.Start_Date)
                .Select(x => new LoginYearOptionDto
                {
                    Year_ID = x.Fiscal_Year_ID,
                    Year_Name = x.Year_Name,
                    Is_Default = x.Is_Default
                })
                .ToListAsync(cancellationToken);

            if (branches.Count == 0 || years.Count == 0)
                return BadRequest("لا توجد فروع أو سنوات مالية متاحة لهذا المستخدم.");

            return Ok(new LoginOptionsResponseDto
            {
                User_ID = user.User_ID,
                Full_Name = user.Full_Name,
                Company_ID = companyId,
                Branches = branches,
                Years = years
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "فشل تحميل خيارات الدخول للمستخدم {LoginName} ضمن الشركة {CompanyId}.",
                loginName, companyId);
            return StatusCode(500, "تعذر تحميل خيارات الدخول حالياً.");
        }
    }

    private async Task<IActionResult> FailedAsync(
        AlTayerERP.Core.Entities.User? user,
        string loginName,
        string companyId,
        string reason,
        string? ipAddress,
        string? userAgent,
        string deviceId,
        CancellationToken cancellationToken)
    {
        await _loginSecurity.RecordFailureAsync(
            user, loginName, companyId, null, null, reason,
            ipAddress, userAgent, deviceId, cancellationToken);

        return Unauthorized("بيانات الدخول غير صحيحة أو أن الحساب غير متاح حالياً.");
    }

    private static string NormalizeDeviceId(string? deviceId) =>
        string.IsNullOrWhiteSpace(deviceId)
            ? "mobile-unknown"
            : deviceId.Trim()[..Math.Min(deviceId.Trim().Length, 128)];
}

public sealed class LoginCompanyOptionDto
{
    public string Company_ID { get; set; } = string.Empty;
    public string Company_Name { get; set; } = string.Empty;
}

public sealed class LoginOptionsRequestDto
{
    public string Company_ID { get; set; } = string.Empty;
    public string Login_Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Device_ID { get; set; }
}

public sealed class LoginOptionsResponseDto
{
    public int User_ID { get; set; }
    public string Full_Name { get; set; } = string.Empty;
    public string Company_ID { get; set; } = string.Empty;
    public List<LoginBranchOptionDto> Branches { get; set; } = [];
    public List<LoginYearOptionDto> Years { get; set; } = [];
}

public sealed class LoginBranchOptionDto
{
    public int Branch_ID { get; set; }
    public string Branch_Code { get; set; } = string.Empty;
    public string Branch_Name { get; set; } = string.Empty;
    public bool Is_Default { get; set; }
}

public sealed class LoginYearOptionDto
{
    public int Year_ID { get; set; }
    public string Year_Name { get; set; } = string.Empty;
    public bool Is_Default { get; set; }
}
