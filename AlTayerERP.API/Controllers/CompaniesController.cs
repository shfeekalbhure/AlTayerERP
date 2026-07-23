using AlTayerERP.API.DTOs;
using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AlTayerERP.API.Controllers
{
    /// <summary>إدارة الشركات. جميع عمليات الإنشاء والتعديل والإيقاف محمية ومدققة من الخادم.</summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly NumberGeneratorService _numberGenerator;

        public CompaniesController(AppDbContext context, NumberGeneratorService numberGenerator)
        {
            _context = context;
            _numberGenerator = numberGenerator;
        }

        /// <summary>يستخرج جلسة مدير النظام الموثوقة من Middleware الطلب.</summary>
        private bool TryGetAdminSession(out ServerSession session)
        {
            session = HttpContext.Items["ServerSession"] as ServerSession
                ?? new ServerSession(string.Empty, 0, 0, false, string.Empty, 0, 0, DateTime.MinValue);
            return session.Is_System_Admin;
        }

        /// <summary>عرض الشركات مع تدقيق مختصر للعرض الإداري.</summary>
        [HttpGet]
        public async Task<IActionResult> GetCompanies()
        {
            if (!TryGetAdminSession(out _)) return Forbid();
            return Ok(await _context.Companies.AsNoTracking()
                .OrderBy(x => x.Company_Name_AR).ToListAsync());
        }

        /// <summary>عرض شركة واحدة وفق معرفها الداخلي.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompany(string id)
        {
            if (!TryGetAdminSession(out _)) return Forbid();
            var company = await _context.Companies.AsNoTracking().FirstOrDefaultAsync(x => x.Company_ID == id);
            return company is null ? NotFound("الشركة غير موجودة.") : Ok(company);
        }

        /// <summary>إنشاء الشركة بعد التحقق من المجموعة النشطة وتوليد رقمها مركزياً.</summary>
        [HttpPost]
        public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyDto dto)
        {
            if (!TryGetAdminSession(out var session)) return Forbid();
            var error = await ValidateAsync(dto);
            if (error is not null) return BadRequest(error);

            var company = new Company
            {
                Company_ID = await _numberGenerator.GenerateNextNumberAsync("COMPANY"),
                Created_At = DateTime.UtcNow,
                Created_By = session.User_ID,
                Edit_Count = 0
            };
            Map(dto, company);
            _context.Companies.Add(company);
            AddAudit(session, company, "CREATE", null, Snapshot(company));
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCompany), new { id = company.Company_ID }, company);
        }

        /// <summary>تعديل شركة باستخدام DTO آمن؛ لا يقبل Created_By أو Edit_Count من العميل.</summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompany(string id, [FromBody] CreateCompanyDto dto)
        {
            if (!TryGetAdminSession(out var session)) return Forbid();
            var company = await _context.Companies.FirstOrDefaultAsync(x => x.Company_ID == id);
            if (company is null) return NotFound("الشركة غير موجودة.");

            var error = await ValidateAsync(dto);
            if (error is not null) return BadRequest(error);

            var oldValues = Snapshot(company);
            Map(dto, company);
            company.Updated_At = DateTime.UtcNow;
            company.Updated_By = session.User_ID;
            company.Edit_Count += 1;
            AddAudit(session, company, "UPDATE", oldValues, Snapshot(company));
            await _context.SaveChangesAsync();
            return Ok(company);
        }

        /// <summary>إيقاف الشركة بدلاً من حذفها، مع منع الإيقاف عند وجود فروع نشطة.</summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeactivateCompany(string id, [FromBody] RecordStatusChangeDto dto)
        {
            if (!TryGetAdminSession(out var session)) return Forbid();
            if (dto is null || string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest("سبب الإيقاف مطلوب.");

            var company = await _context.Companies.FirstOrDefaultAsync(x => x.Company_ID == id);
            if (company is null) return NotFound("الشركة غير موجودة.");

            if (await _context.Tenant_Branches.AnyAsync(x => x.Company_ID == id && x.Is_Active))
                return Conflict("لا يمكن إيقاف الشركة قبل إيقاف فروعها النشطة.");

            var oldValues = Snapshot(company);
            company.Is_Active = false;
            company.Updated_At = DateTime.UtcNow;
            company.Updated_By = session.User_ID;
            company.Stopped_By = session.User_ID;
            company.Stopped_At = DateTime.UtcNow;
            company.Stopped_Reason = dto.Reason.Trim();
            company.Edit_Count += 1;
            AddAudit(session, company, "DEACTIVATE", oldValues, Snapshot(company));
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم إيقاف الشركة دون حذف تاريخها." });
        }


        /// <summary>إعادة تفعيل شركة مع سبب إلزامي بعد التحقق من مجموعتها التجارية.</summary>
        [HttpPost("{id}/reactivate")]
        public async Task<IActionResult> ReactivateCompany(string id, [FromBody] RecordStatusChangeDto dto)
        {
            if (!TryGetAdminSession(out var session)) return Forbid();
            if (dto is null || string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest("سبب إعادة التفعيل مطلوب.");

            var company = await _context.Companies.FirstOrDefaultAsync(x => x.Company_ID == id);
            if (company is null) return NotFound("الشركة غير موجودة.");
            if (!await _context.Tenant_Groups.AnyAsync(x => x.Group_ID == company.Group_ID && x.Is_Active))
                return Conflict("لا يمكن إعادة تفعيل الشركة قبل تفعيل مجموعتها التجارية.");

            var oldValues = Snapshot(company);
            company.Is_Active = true;
            company.Updated_At = DateTime.UtcNow;
            company.Updated_By = session.User_ID;
            company.Reactivated_By = session.User_ID;
            company.Reactivated_At = DateTime.UtcNow;
            company.Reactivate_Reason = dto.Reason.Trim();
            company.Edit_Count += 1;
            AddAudit(session, company, "REACTIVATE", oldValues, Snapshot(company));
            await _context.SaveChangesAsync();
            return Ok(company);
        }

        /// <summary>التحقق من بيانات العمل وعلاقة المجموعة قبل الحفظ.</summary>
        private async Task<string?> ValidateAsync(CreateCompanyDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Group_ID) || string.IsNullOrWhiteSpace(dto.Company_Name_AR))
                return "المجموعة التجارية واسم الشركة بالعربية حقول مطلوبة.";

            if (!await _context.Tenant_Groups.AnyAsync(x => x.Group_ID == dto.Group_ID && x.Is_Active))
                return "المجموعة التجارية غير موجودة أو موقوفة.";

            if (!string.IsNullOrWhiteSpace(dto.Company_Prefix) && dto.Company_Prefix.Trim().Length > 15)
                return "رمز الشركة يجب ألا يتجاوز 15 حرفاً.";
            return null;
        }

        /// <summary>نقل حقول العمل فقط من DTO؛ حقول التتبع لا تدخل من العميل.</summary>
        private static void Map(CreateCompanyDto dto, Company company)
        {
            company.Group_ID = dto.Group_ID.Trim();
            company.Company_Name_AR = dto.Company_Name_AR.Trim();
            company.Company_Name_EN = dto.Company_Name_EN?.Trim() ?? string.Empty;
            company.Company_Prefix = dto.Company_Prefix?.Trim().ToUpperInvariant();
            company.Activity_Type = dto.Activity_Type?.Trim();
            company.Tax_Number = dto.Tax_Number?.Trim();
            company.Phone = dto.Phone?.Trim();
            company.Mobile = dto.Mobile?.Trim();
            company.Email = dto.Email?.Trim();
            company.Address = dto.Address?.Trim();
            // لا نمسح الشعار تلقائياً عند إرسال DTO بلا صورة؛ الإزالة تحتاج عملية صريحة لاحقاً.
            if (dto.Company_Logo is { Length: > 0 })
                company.Company_Logo = dto.Company_Logo;
            company.Is_Active = dto.Is_Active;
        }

        /// <summary>إدراج سجل التدقيق في نفس وحدة الحفظ، مع المستخدم والفرع من الجلسة.</summary>
        private void AddAudit(ServerSession session, Company company, string action, string? oldValues, string newValues)
        {
            _context.Audit_Logs.Add(new AlTayerERP.Core.Entities.Accounting.AuditLog
            {
                Table_Name = "companies", Record_ID = company.Company_ID, Action_Type = action,
                User_ID = session.User_ID.ToString(), Branch_ID = session.Branch_ID.ToString(),
                Action_At = DateTime.UtcNow, Old_Values = oldValues, New_Values = newValues,
                Action_Channel = "DESKTOP", Device_Name = Request.Headers["X-Device-ID"].ToString(),
                IP_Address = HttpContext.Connection.RemoteIpAddress?.ToString(), Notes = "إدارة الشركات"
            });
        }

        /// <summary>لقطة بيانات عمل للمراجعة من دون الشعار أو أسرار المستخدمين.</summary>
        private static string Snapshot(Company company) => JsonSerializer.Serialize(new
        {
            company.Company_ID, company.Group_ID, company.Company_Name_AR, company.Company_Name_EN,
            company.Company_Prefix, company.Activity_Type, company.Tax_Number, company.Phone,
            company.Email, company.Address, company.Is_Active, company.Edit_Count
        });
    }
}