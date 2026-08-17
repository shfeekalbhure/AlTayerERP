using AlTayerERP.API.DTOs;
using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlTayerERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        // سياق الاتصال بقاعدة البيانات (DbContext)
        private readonly AppDbContext _context;

        // الخدمة المخصصة لتوليد أرقام السجلات التلقائية بالتسلسل
        private readonly NumberGeneratorService _numberGenerator;

        // مشيد الكلاس لحقن التبعيات (Dependency Injection) لقاعدة البيانات وخدمة الترقيم
        public CompaniesController(AppDbContext context, NumberGeneratorService numberGenerator)
        {
            _context = context;
            _numberGenerator = numberGenerator;
        }

        private IActionResult? RequireSystemAdmin()
        {
            var session = HttpContext.Items["ServerSession"] as ServerSession;
            if (session == null) return Unauthorized("انتهت الجلسة أو أنها غير صالحة.");
            return session.Is_System_Admin ? null : Forbid();
        }

        // ======================================================
        // 1. جلب قائمة الشركات (المستخدمة لتعبئة الجدول الرئيسي في واجهة سطح المكتب)
        // GET: api/Companies
        // ======================================================
        [HttpGet]
        public async Task<IActionResult> GetCompanies()
        {
            var accessError = RequireSystemAdmin();
            if (accessError != null) return accessError;
            try
            {
                // نختار حقولاً محددة فقط لعرضها بالجدول لتقليل حجم البيانات وتسريع الاستجابة
                var companies = await _context.Companies
                    .Select(c => new
                    {
                        c.Company_ID,
                        c.Company_Name_AR,
                        c.Company_Name_EN,
                        c.Company_Prefix,
                        c.Activity_Type,
                        c.Tax_Number,
                        c.Phone,
                        c.Mobile,
                        c.Email,
                        c.Address,
                        c.Is_Active
                    })
                    .ToListAsync();

                return Ok(companies);
            }
            catch (Exception ex)
            {
                // إرجاع خطأ 500 مع تفاصيل الاستثناء في حال حدوث خلل بالسيرفر
                return StatusCode(500, $"حدث خطأ أثناء جلب القائمة: {ex.Message}");
            }
        }

        // ======================================================
        // 2. جلب تفاصيل شركة واحدة بالكامل (تُستدعى فور النقر على سطر بالجدول لعرض الشعار وباقي البيانات)
        // GET: api/Companies/{id}
        // ======================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompany(string id)
        {
            var accessError = RequireSystemAdmin();
            if (accessError != null) return accessError;
            try
            {
                // البحث عن الشركة في قاعدة البيانات بواسطة المعرّف الفريد
                var company = await _context.Companies.FindAsync(id);

                // إذا لم يتم العثور على الشركة نرجع كود الحالة 404
                if (company == null)
                {
                    return NotFound(new { message = "الشركة المطلوبة غير موجودة في النظام!" });
                }

                // إرجاع كائن الشركة كاملاً متضمناً مصفوفة بايتات الشعار
                return Ok(company);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"حدث خطأ أثناء جلب تفاصيل الشركة: {ex.Message}");
            }
        }

        // ======================================================
        // 3. حفظ وحقن شركة جديدة في النظام
        // POST: api/Companies
        // ======================================================
        [HttpPost]
        public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyDto dto)
        {
            var accessError = RequireSystemAdmin();
            if (accessError != null) return accessError;
            // التحقق الأولي من صحة واكتمال المدخلات الضرورية
            if (dto == null || string.IsNullOrWhiteSpace(dto.Company_Name_AR) || string.IsNullOrWhiteSpace(dto.Group_ID))
            {
                return BadRequest("بيانات الشركة غير مكتملة أو لم يتم تحديد المجموعة الأم!");
            }

            try
            {
                var groupId = dto.Group_ID.Trim();
                if (!await _context.Tenant_Groups.AnyAsync(x => x.Group_ID == groupId && x.Is_Active))
                    return BadRequest("المجموعة التجارية المحددة غير موجودة أو موقوفة.");

                // استدعاء خدمة الترقيم لتوليد المعرف الفريد القادم للشركات تلقائياً
                string companyNumber = await _numberGenerator.GenerateNextNumberAsync("COMPANY");

                // تحويل بيانات الـ DTO المستقبلة إلى كائن الكينونة الأساسي (Company Entity) للحفظ
                var newCompany = new Company
                {
                    Company_ID = companyNumber,
                    Group_ID = groupId,
                    Company_Name_AR = dto.Company_Name_AR.Trim(),
                    Company_Name_EN = dto.Company_Name_EN?.Trim() ?? string.Empty,
                    Company_Prefix = dto.Company_Prefix?.Trim().ToUpper() ?? string.Empty,
                    Activity_Type = dto.Activity_Type,
                    Tax_Number = dto.Tax_Number,
                    Phone = dto.Phone,
                    Mobile = dto.Mobile,
                    Email = dto.Email,
                    Address = dto.Address,
                    Company_Logo = dto.Company_Logo, // استقبال الشعار كمصفوفة بايتات بنجاح
                    Is_Active = dto.Is_Active,
                    Created_At = DateTime.UtcNow,
                    Updated_At = DateTime.UtcNow
                };

                // إضافة السجل الجديد بشكل غير متزامن وحفظ التغييرات في قاعدة البيانات
                await _context.Companies.AddAsync(newCompany);
                await _context.SaveChangesAsync();

                // نرجع كود النجاح 201 مع توجيه تفاصيل الكائن المضاف حديثاً
                return CreatedAtAction(nameof(GetCompany), new { id = newCompany.Company_ID }, newCompany);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"فشل تأسيس الشركة: {ex.Message}");
            }
        }

        // ======================================================
        // 4. تعديل وتحديث بيانات شركة موجودة فعلياً
        // PUT: api/Companies/{id}
        // ======================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompany(string id, [FromBody] Company updatedCompany)
        {
            var accessError = RequireSystemAdmin();
            if (accessError != null) return accessError;
            if (updatedCompany == null || string.IsNullOrWhiteSpace(updatedCompany.Group_ID) || string.IsNullOrWhiteSpace(updatedCompany.Company_Name_AR))
                return BadRequest("المجموعة التجارية واسم الشركة العربي مطلوبان.");
            // التحقق من أن معرف الرابط يطابق معرف الكائن المرسل في جسم الطلب
            if (id != updatedCompany.Company_ID)
            {
                return BadRequest("معرّفات الشركة غير متطابقة في الطلب!");
            }

            try
            {
                if (!await _context.Tenant_Groups.AnyAsync(x => x.Group_ID == updatedCompany.Group_ID.Trim() && x.Is_Active))
                    return BadRequest("المجموعة التجارية المحددة غير موجودة أو موقوفة.");
                // جلب السجل الأصلي الحالي المخزن في قاعدة البيانات للتعديل عليه
                var existingCompany = await _context.Companies.FindAsync(id);
                if (existingCompany == null)
                {
                    return NotFound(new { message = "الشركة المراد تعديل بياناتها غير موجودة!" });
                }

                // تحديث الحقول النصية والمنطقية بالقيم المرفوعة الجديدة من الشاشة
                existingCompany.Group_ID = updatedCompany.Group_ID.Trim();
                existingCompany.Company_Name_AR = updatedCompany.Company_Name_AR.Trim();
                existingCompany.Company_Name_EN = updatedCompany.Company_Name_EN?.Trim() ?? string.Empty;
                existingCompany.Company_Prefix = updatedCompany.Company_Prefix?.Trim().ToUpper() ?? string.Empty;
                existingCompany.Activity_Type = updatedCompany.Activity_Type;
                existingCompany.Tax_Number = updatedCompany.Tax_Number;
                existingCompany.Phone = updatedCompany.Phone;
                existingCompany.Mobile = updatedCompany.Mobile;
                existingCompany.Email = updatedCompany.Email;
                existingCompany.Address = updatedCompany.Address;
                existingCompany.Is_Active = updatedCompany.Is_Active;
                existingCompany.Updated_At = DateTime.UtcNow; // تسجيل توقيت التعديل الحالي لقواعد التدقيق

                // معالجة ذكية للشعار: نقوم بتحديث الشعار فقط في حال قام المستخدم برفع ملف جديد
                if (updatedCompany.Company_Logo != null && updatedCompany.Company_Logo.Length > 0)
                {
                    existingCompany.Company_Logo = updatedCompany.Company_Logo;
                }

                // إعلام الـ Entity Framework بأن حالة هذا الكائن قد تم تعديلها ووجب تحديثه
                _context.Entry(existingCompany).State = EntityState.Modified;

                // حفظ التغييرات النهائية في MySQL/PostgreSQL
                await _context.SaveChangesAsync();

                return Ok(new { message = "تم تحديث بيانات الشركة بنجاح داخل النظام." });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Companies.Any(e => e.Company_ID == id))
                {
                    return NotFound(new { message = "لم يتم العثور على الشركة، قد تكون حُذفت من مستخدم آخر!" });
                }
                throw;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"حدث خطأ أثناء معالجة التعديل: {ex.Message}");
            }
        }

        // ======================================================
        // 5. حذف شركة نهائياً من قاعدة البيانات
        // DELETE: api/Companies/{id}
        // ======================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompany(string id)
        {
            var accessError = RequireSystemAdmin();
            if (accessError != null) return accessError;
            try
            {
                // جلب السجل المراد حذفه للتأكد من وجوده مسبقاً
                var company = await _context.Companies.FindAsync(id);
                if (company == null)
                {
                    return NotFound(new { message = "الشركة غير موجودة بالفعل أو تم حذفها مسبقاً!" });
                }

                if (await _context.Tenant_Branches.AnyAsync(x => x.Company_ID == id))
                    return BadRequest("لا يمكن حذف الشركة لأنها مرتبطة بفروع. أوقفها بدلاً من الحذف.");

                if (await _context.Users.AnyAsync(x => x.Company_ID == id))
                    return BadRequest("لا يمكن حذف الشركة لأنها مرتبطة بمستخدمين. أوقفها بدلاً من الحذف.");

                // إزالة السجل من حاوية الشركات وحفظ التغييرات
                _context.Companies.Remove(company);
                await _context.SaveChangesAsync();

                return Ok(new { message = "تم حذف الشركة وإزالة كافة الروابط المرتبطة بها بنجاح." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"فشل إجراء الحذف من قاعدة البيانات: {ex.Message}");
            }
        }
    }
}
