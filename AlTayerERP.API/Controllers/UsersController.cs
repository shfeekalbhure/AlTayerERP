using AlTayerERP.API.DTOs;
using AlTayerERP.API.Security;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers;

/// <summary>
/// إدارة المستخدمين. جميع الاستجابات تستبعد Password_Hash وكلمة المرور.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    public UsersController(AppDbContext context) => _context = context;

    /// <summary>جلب المستخدمين دون أي بيانات اعتماد حساسة.</summary>
    [HttpGet]
    public async Task<IActionResult> GetUsers() =>
        Ok(await _context.Users.AsNoTracking().OrderBy(x => x.User_ID)
            .Select(ToResponseExpression()).ToListAsync());

    /// <summary>جلب مستخدم واحد دون Password_Hash.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.User_ID == id);
        return user is null ? NotFound("المستخدم غير موجود.") : Ok(ToResponse(user));
    }

    /// <summary>إنشاء مستخدم جديد وتخزين كلمة مروره كتجزئة آمنة فقط.</summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.Full_Name) ||
            string.IsNullOrWhiteSpace(dto.Login_Name) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("الاسم واسم الدخول وكلمة المرور مطلوبة.");

        var login = dto.Login_Name.Trim();
        if (await _context.Users.AnyAsync(x => x.Login_Name == login))
            return Conflict("اسم الدخول مستخدم مسبقاً.");

        var branchValid = await _context.Tenant_Branches.AnyAsync(x =>
            x.Branch_ID == dto.Branch_ID && x.Company_ID == dto.Company_ID && x.Is_Active);
        if (!branchValid) return BadRequest("الفرع لا يتبع الشركة أو غير نشط.");

        var roleValid = await _context.Roles.AnyAsync(x => x.Role_ID == dto.Role_ID && x.Is_Active);
        if (!roleValid) return BadRequest("الدور المختار غير موجود أو غير نشط.");

        var user = new User
        {
            Company_ID = dto.Company_ID.Trim(), Branch_ID = dto.Branch_ID, Role_ID = dto.Role_ID,
            User_Code = dto.User_Code?.Trim() ?? string.Empty, Full_Name = dto.Full_Name.Trim(),
            Login_Name = login,
            // Password_Hash: لا يُخزّن النص الصريح لكلمة المرور مطلقاً.
            Password_Hash = PasswordProtector.Hash(dto.Password),
            Phone = dto.Phone?.Trim(), Email = dto.Email?.Trim(), Notes = dto.Notes?.Trim(),
            Must_Change_Password = dto.Must_Change_Password, Is_Active = dto.Is_Active,
            Created_At = DateTime.UtcNow
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUserById), new { id = user.User_ID }, ToResponse(user));
    }

    /// <summary>تعديل المستخدم. كلمة المرور اختيارية، وعند تغييرها تجزأ قبل الحفظ.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] CreateUserDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.User_ID == id);
        if (user is null) return NotFound("المستخدم غير موجود.");
        if (dto is null || string.IsNullOrWhiteSpace(dto.Full_Name) || string.IsNullOrWhiteSpace(dto.Login_Name))
            return BadRequest("الاسم الكامل واسم الدخول حقول مطلوبة.");

        var login = dto.Login_Name.Trim();
        if (await _context.Users.AnyAsync(x => x.User_ID != id && x.Login_Name == login))
            return Conflict("اسم الدخول مستخدم مسبقاً.");

        var branchValid = await _context.Tenant_Branches.AnyAsync(x =>
            x.Branch_ID == dto.Branch_ID && x.Company_ID == dto.Company_ID && x.Is_Active);
        if (!branchValid) return BadRequest("الفرع لا يتبع الشركة أو غير نشط.");

        user.Company_ID = dto.Company_ID.Trim(); user.Branch_ID = dto.Branch_ID; user.Role_ID = dto.Role_ID;
        user.User_Code = dto.User_Code?.Trim() ?? string.Empty; user.Full_Name = dto.Full_Name.Trim();
        user.Login_Name = login; user.Phone = dto.Phone?.Trim(); user.Email = dto.Email?.Trim();
        user.Notes = dto.Notes?.Trim(); user.Must_Change_Password = dto.Must_Change_Password;
        user.Is_Active = dto.Is_Active; user.Updated_At = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(dto.Password))
            user.Password_Hash = PasswordProtector.Hash(dto.Password);

        await _context.SaveChangesAsync();
        return Ok(ToResponse(user));
    }

    /// <summary>إيقاف مستخدم بدلاً من حذفه لحفظ تاريخ السندات والتدقيق.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeactivateUser(int id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.User_ID == id);
        if (user is null) return NotFound("المستخدم غير موجود.");
        user.Is_Active = false; user.Updated_At = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(new { message = "تم إيقاف المستخدم دون حذف بياناته التاريخية." });
    }

    [HttpGet("GetBranchesLookup")]
    public async Task<IActionResult> GetBranchesLookup([FromQuery] string companyId) =>
        Ok(await _context.Tenant_Branches.AsNoTracking().Where(x => x.Company_ID == companyId && x.Is_Active)
            .OrderBy(x => x.Branch_Name).Select(x => new { x.Branch_ID, x.Branch_Name }).ToListAsync());

    [HttpGet("GetRolesLookup")]
    public async Task<IActionResult> GetRolesLookup() =>
        Ok(await _context.Roles.AsNoTracking().Where(x => x.Is_Active)
            .Select(x => new { x.Role_ID, x.Role_Name }).ToListAsync());

    [HttpGet("GetUsersLookup")]
    public async Task<IActionResult> GetUsersLookup() =>
        Ok(await _context.Users.AsNoTracking().Where(x => x.Is_Active)
            .Select(x => new { x.User_ID, x.Login_Name }).ToListAsync());

    /// <summary>تحويل كيان المستخدم إلى DTO آمن يستبعد Password_Hash.</summary>
    private static UserResponseDto ToResponse(User x) => new()
    {
        User_ID=x.User_ID, Company_ID=x.Company_ID, Branch_ID=x.Branch_ID, Role_ID=x.Role_ID,
        User_Code=x.User_Code, Full_Name=x.Full_Name, Login_Name=x.Login_Name, Phone=x.Phone,
        Email=x.Email, Notes=x.Notes, Must_Change_Password=x.Must_Change_Password,
        Is_Active=x.Is_Active, Created_At=x.Created_At, Updated_At=x.Updated_At
    };

    private static System.Linq.Expressions.Expression<Func<User, UserResponseDto>> ToResponseExpression() =>
        x => new UserResponseDto
        {
            User_ID=x.User_ID, Company_ID=x.Company_ID, Branch_ID=x.Branch_ID, Role_ID=x.Role_ID,
            User_Code=x.User_Code, Full_Name=x.Full_Name, Login_Name=x.Login_Name, Phone=x.Phone,
            Email=x.Email, Notes=x.Notes, Must_Change_Password=x.Must_Change_Password,
            Is_Active=x.Is_Active, Created_At=x.Created_At, Updated_At=x.Updated_At
        };
}