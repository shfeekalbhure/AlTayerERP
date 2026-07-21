using AlTayerERP.API.DTOs;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AlTayerERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SystemAdmin")]
    public class UsersController : ControllerBase
    {
        private static readonly PasswordHasher<User> PasswordHasher = new();
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // 1. جلب كل المستخدمين
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .OrderBy(x => x.User_ID)
                .ToListAsync();

            return Ok(users);
        }

        // ======================================================
        // --- الدالة المضافة حديثاً: جلب تفاصيل مستخدم واحد بواسطة الـ ID ---
        // ======================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.User_ID == id);

            if (user == null)
                return NotFound("المستخدم غير موجود.");

            return Ok(user);
        }

        // 2. حفظ مستخدم جديد
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            if (dto == null)
                return BadRequest("بيانات المستخدم غير صحيحة.");

            if (string.IsNullOrWhiteSpace(dto.Full_Name))
                return BadRequest("اسم الموظف الكامل مطلوب.");

            if (string.IsNullOrWhiteSpace(dto.Login_Name))
                return BadRequest("اسم الدخول مطلوب.");

            if (string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("كلمة المرور مطلوبة.");

            var exists = await _context.Users
                .AnyAsync(x => x.Login_Name == dto.Login_Name);

            if (exists)
                return BadRequest("اسم الدخول مستخدم مسبقاً.");

            var user = new User
            {
                Company_ID = dto.Company_ID,
                Branch_ID = dto.Branch_ID,
                Role_ID = dto.Role_ID,
                User_Code = dto.User_Code,
                Full_Name = dto.Full_Name.Trim(),
                Login_Name = dto.Login_Name.Trim(),
                Password_Hash = PasswordHasher.HashPassword(user: new User(), password: dto.Password),
                Phone = dto.Phone,
                Email = dto.Email,
                Notes = dto.Notes,
                Must_Change_Password = dto.Must_Change_Password,
                Is_Active = dto.Is_Active,
                Created_At = DateTime.Now
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        // ======================================================
        // --- الدالة المضافة حديثاً: تعديل بيانات مستخدم موجود ---
        // ======================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] CreateUserDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.User_ID == id);
            if (user == null)
                return NotFound("المستخدم غير موجود في النظام لتعديله.");

            if (string.IsNullOrWhiteSpace(dto.Full_Name) || string.IsNullOrWhiteSpace(dto.Login_Name))
                return BadRequest("الاسم الكامل واسم الدخول حقول مطلوبة.");

            // التحقق من أن اسم الدخول الجديد غير محجوز لمستخدم آخر
            var loginExists = await _context.Users
                .AnyAsync(x => x.Login_Name == dto.Login_Name && x.User_ID != id);

            if (loginExists)
                return BadRequest("اسم الدخول هذا مستخدم من قبل موظف آخر.");

            // تحديث الحقول
            user.Branch_ID = dto.Branch_ID;
            user.Role_ID = dto.Role_ID;
            user.Full_Name = dto.Full_Name.Trim();
            user.Login_Name = dto.Login_Name.Trim();
            user.Phone = dto.Phone;
            user.Email = dto.Email;
            user.Notes = dto.Notes;
            user.Must_Change_Password = dto.Must_Change_Password;
            user.Is_Active = dto.Is_Active;
            user.Updated_At = DateTime.Now; // تعيين وقت التعديل

            // إذا قام المدير بكتابة كلمة مرور جديدة يتم تعديلها، عدا ذلك يحتفظ بالقديمة
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.Password_Hash = PasswordHasher.HashPassword(user, dto.Password);
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        // ======================================================
        // --- الدالة المضافة حديثاً: حذف مستخدم من النظام ---
        // ======================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.User_ID == id);
            if (user == null)
                return NotFound("المستخدم غير موجود بالفعل.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok("تم حذف المستخدم بنجاح.");
        }

        // 3. جلب الفروع للقائمة المنسدلة
        //     [HttpGet("GetBranchesLookup")]
        //    public async Task<IActionResult> GetBranchesLookup()
        //      {
        //        var branches = await _context.Tenant_Branches
        ////             .Where(x => x.Is_Active)
        //             .Select(x => new
        //            {
        //                 x.Branch_ID,
        //                 x.Branch_Name
        //            })
        //             .ToListAsync();

        //         return Ok(branches);
        //      }
        [HttpGet("GetBranchesLookup")]
        public async Task<IActionResult> GetBranchesLookup([FromQuery] string companyId)
        {
            var branches = await _context.Tenant_Branches
                .Where(x => x.Company_ID == companyId && x.Is_Active)
                .OrderBy(x => x.Branch_Name)
                .Select(x => new
                {
                    x.Branch_ID,
                    x.Branch_Name
                })
                .ToListAsync();

            return Ok(branches);
        }





        // 4. جلب الأدوار للقائمة المنسدلة
        [HttpGet("GetRolesLookup")]
        public async Task<IActionResult> GetRolesLookup()
        {
            var roles = await _context.Roles
                .Where(x => x.Is_Active)
                .Select(x => new
                {
                    x.Role_ID,
                    x.Role_Name
                })
                .ToListAsync();

            return Ok(roles);
        }

        // ======================================================
        // --- الدالة المضافة لتلبية طلب شاشة الدخول بالـ Desktop ---
        // GET: api/Users/GetUsersLookup
        // ======================================================
        [HttpGet("GetUsersLookup")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUsersLookup()
        {
            try
            {
                var users = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.Is_Active == true)
                    .Select(u => new
                    {
                        User_ID = u.User_ID,
                        Login_Name = u.Login_Name
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"خطأ في خادم المستخدمين: {ex.Message}");
            }
        }
    }
}