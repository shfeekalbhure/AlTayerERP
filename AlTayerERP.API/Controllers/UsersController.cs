using AlTayerERP.API.DTOs;
using AlTayerERP.API.Security;
using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>
    /// إدارة المستخدمين. لا يسمح API للعميل بتحديد هوية منفذ العملية أو نطاقه؛
    /// سياق الجلسة الموثوق هو المرجع، والحذف يتحول إلى إيقاف منطقي.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ScreenAuthorizationService _authorization;
        private readonly AuditTrailService _audit;

        public UsersController(AppDbContext context, ScreenAuthorizationService authorization, AuditTrailService audit)
        {
            _context = context;
            _authorization = authorization;
            _audit = audit;
        }

        private bool TryGetSession(out ServerSession session)
        {
            session = HttpContext.Items["ServerSession"] as ServerSession ?? default!;
            return session != null;
        }

        private async Task<IActionResult?> DenyUnlessAsync(ScreenOperation operation)
        {
            if (!TryGetSession(out var session)) return Unauthorized();
            return await _authorization.IsAllowedAsync(session, "Users", operation) ? null : Forbid();
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var denied = await DenyUnlessAsync(ScreenOperation.View);
            if (denied != null) return denied;
            TryGetSession(out var session);

            var query = _context.Users.AsNoTracking().AsQueryable();
            if (!session.Is_System_Admin)
                query = query.Where(x => x.Company_ID == session.Company_ID && x.Branch_ID == session.Branch_ID);

            var users = await query.OrderBy(x => x.User_ID)
                .Select(x => new
                {
                    x.User_ID, x.Company_ID, x.Branch_ID, x.Role_ID, x.User_Code,
                    x.Full_Name, x.Login_Name, x.Phone, x.Email, x.Notes,
                    x.Must_Change_Password, x.Is_Active, x.Created_At, x.Updated_At,
                    x.Last_Login_At, x.Locked_Until
                })
                .ToListAsync();
            return Ok(users);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var denied = await DenyUnlessAsync(ScreenOperation.View);
            if (denied != null) return denied;
            TryGetSession(out var session);

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.User_ID == id);
            if (user == null) return NotFound("المستخدم غير موجود.");
            if (!session.Is_System_Admin &&
                (user.Company_ID != session.Company_ID || user.Branch_ID != session.Branch_ID))
                return Forbid();

            // لا يعاد Password_Hash إلى الواجهة تحت أي ظرف.
            return Ok(new
            {
                user.User_ID, user.Company_ID, user.Branch_ID, user.Role_ID, user.User_Code,
                user.Full_Name, user.Login_Name, user.Phone, user.Email, user.Notes,
                user.Must_Change_Password, user.Is_Active, user.Created_At, user.Updated_At,
                user.Last_Login_At, user.Locked_Until
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            var denied = await DenyUnlessAsync(ScreenOperation.Add);
            if (denied != null) return denied;
            TryGetSession(out var session);

            if (dto == null || string.IsNullOrWhiteSpace(dto.Full_Name) ||
                string.IsNullOrWhiteSpace(dto.Login_Name) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("الاسم واسم الدخول وكلمة المرور حقول مطلوبة.");

            var companyId = session.Is_System_Admin ? dto.Company_ID?.Trim() : session.Company_ID;
            var branchId = session.Is_System_Admin ? dto.Branch_ID : session.Branch_ID;
            if (string.IsNullOrWhiteSpace(companyId) || branchId <= 0)
                return BadRequest("يجب تحديد الشركة والفرع.");

            if (await _context.Users.AnyAsync(x => x.Login_Name == dto.Login_Name.Trim()))
                return Conflict("اسم الدخول مستخدم مسبقاً.");

            var branchValid = await _context.Tenant_Branches.AnyAsync(x =>
                x.Branch_ID == branchId && x.Company_ID == companyId && x.Is_Active);
            var role = await _context.Roles.AsNoTracking().FirstOrDefaultAsync(x => x.Role_ID == dto.Role_ID && x.Is_Active);
            if (!branchValid || role == null) return BadRequest("الشركة أو الفرع أو الدور غير صالح.");
            if (!session.Is_System_Admin && role.Is_System_Admin) return Forbid();

            var user = new User
            {
                Company_ID = companyId,
                Branch_ID = branchId,
                Role_ID = dto.Role_ID,
                User_Code = string.IsNullOrWhiteSpace(dto.User_Code) ? $"USR{DateTime.UtcNow:yyyyMMddHHmmssfff}" : dto.User_Code.Trim(),
                Full_Name = dto.Full_Name.Trim(),
                Login_Name = dto.Login_Name.Trim(),
                Password_Hash = PasswordProtector.Hash(dto.Password),
                Phone = dto.Phone?.Trim(),
                Email = dto.Email?.Trim(),
                Notes = dto.Notes?.Trim(),
                Must_Change_Password = dto.Must_Change_Password,
                Is_Active = true,
                Created_At = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _audit.Add(session, HttpContext, "users", "new", "CREATE", newValues: new
            {
                user.Company_ID, user.Branch_ID, user.Role_ID, user.User_Code, user.Login_Name, user.Is_Active
            });
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUserById), new { id = user.User_ID }, new { user.User_ID });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] CreateUserDto dto)
        {
            var denied = await DenyUnlessAsync(ScreenOperation.Edit);
            if (denied != null) return denied;
            TryGetSession(out var session);

            if (dto == null || string.IsNullOrWhiteSpace(dto.Full_Name) || string.IsNullOrWhiteSpace(dto.Login_Name))
                return BadRequest("الاسم الكامل واسم الدخول حقول مطلوبة.");

            var user = await _context.Users.FirstOrDefaultAsync(x => x.User_ID == id);
            if (user == null) return NotFound("المستخدم غير موجود.");
            if (!session.Is_System_Admin &&
                (user.Company_ID != session.Company_ID || user.Branch_ID != session.Branch_ID))
                return Forbid();

            if (await _context.Users.AnyAsync(x => x.Login_Name == dto.Login_Name.Trim() && x.User_ID != id))
                return Conflict("اسم الدخول مستخدم من قبل مستخدم آخر.");

            var newCompany = session.Is_System_Admin ? dto.Company_ID?.Trim() : user.Company_ID;
            var newBranch = session.Is_System_Admin ? dto.Branch_ID : user.Branch_ID;
            var role = await _context.Roles.AsNoTracking().FirstOrDefaultAsync(x => x.Role_ID == dto.Role_ID && x.Is_Active);
            var branchValid = await _context.Tenant_Branches.AnyAsync(x => x.Branch_ID == newBranch && x.Company_ID == newCompany && x.Is_Active);
            if (role == null || !branchValid) return BadRequest("الشركة أو الفرع أو الدور غير صالح.");
            if (!session.Is_System_Admin && role.Is_System_Admin) return Forbid();

            var before = new { user.Company_ID, user.Branch_ID, user.Role_ID, user.Login_Name, user.Is_Active };
            user.Company_ID = newCompany!;
            user.Branch_ID = newBranch;
            user.Role_ID = dto.Role_ID;
            user.Full_Name = dto.Full_Name.Trim();
            user.Login_Name = dto.Login_Name.Trim();
            user.Phone = dto.Phone?.Trim();
            user.Email = dto.Email?.Trim();
            user.Notes = dto.Notes?.Trim();
            user.Must_Change_Password = dto.Must_Change_Password;
            user.Is_Active = dto.Is_Active;
            user.Updated_At = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.Password_Hash = PasswordProtector.Hash(dto.Password);
                user.Must_Change_Password = true;
            }

            _audit.Add(session, HttpContext, "users", id.ToString(), "UPDATE", before, new
            {
                user.Company_ID, user.Branch_ID, user.Role_ID, user.Login_Name, user.Is_Active, PasswordChanged = !string.IsNullOrWhiteSpace(dto.Password)
            });
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم تعديل المستخدم بنجاح." });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            var denied = await DenyUnlessAsync(ScreenOperation.Delete);
            if (denied != null) return denied;
            TryGetSession(out var session);

            if (id == session.User_ID) return BadRequest("لا يمكن للمستخدم إيقاف حسابه أثناء جلسته.");

            var user = await _context.Users.FirstOrDefaultAsync(x => x.User_ID == id);
            if (user == null) return NotFound("المستخدم غير موجود.");
            if (!session.Is_System_Admin &&
                (user.Company_ID != session.Company_ID || user.Branch_ID != session.Branch_ID))
                return Forbid();

            user.Is_Active = false;
            user.Updated_At = DateTime.UtcNow;
            _audit.Add(session, HttpContext, "users", id.ToString(), "DEACTIVATE",
                new { Is_Active = true }, new { Is_Active = false });
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم إيقاف المستخدم بنجاح." });
        }

        [HttpPost("{id:int}/unlock")]
        public async Task<IActionResult> UnlockUser(int id)
        {
            var denied = await DenyUnlessAsync(ScreenOperation.Edit);
            if (denied != null) return denied;
            TryGetSession(out var session);

            var user = await _context.Users.FirstOrDefaultAsync(x => x.User_ID == id);
            if (user == null) return NotFound("المستخدم غير موجود.");
            if (!session.Is_System_Admin &&
                (user.Company_ID != session.Company_ID || user.Branch_ID != session.Branch_ID))
                return Forbid();

            user.Failed_Login_Count = 0;
            user.Last_Failed_Login_At = null;
            user.Locked_Until = null;
            user.Updated_At = DateTime.UtcNow;
            _audit.Add(session, HttpContext, "users", id.ToString(), "UNLOCK");
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم إلغاء قفل حساب المستخدم." });
        }

        [HttpGet("GetBranchesLookup")]
        public async Task<IActionResult> GetBranchesLookup([FromQuery] string? companyId)
        {
            var denied = await DenyUnlessAsync(ScreenOperation.View);
            if (denied != null) return denied;
            TryGetSession(out var session);

            var effectiveCompany = session.Is_System_Admin && !string.IsNullOrWhiteSpace(companyId)
                ? companyId.Trim() : session.Company_ID;
            return Ok(await _context.Tenant_Branches.AsNoTracking()
                .Where(x => x.Company_ID == effectiveCompany && x.Is_Active)
                .OrderBy(x => x.Branch_Name)
                .Select(x => new { x.Branch_ID, x.Branch_Name })
                .ToListAsync());
        }

        [HttpGet("GetRolesLookup")]
        public async Task<IActionResult> GetRolesLookup()
        {
            var denied = await DenyUnlessAsync(ScreenOperation.View);
            if (denied != null) return denied;
            TryGetSession(out var session);

            var query = _context.Roles.AsNoTracking().Where(x => x.Is_Active);
            if (!session.Is_System_Admin) query = query.Where(x => !x.Is_System_Admin);
            return Ok(await query.OrderBy(x => x.Role_Name)
                .Select(x => new { x.Role_ID, x.Role_Name }).ToListAsync());
        }
    }
}
