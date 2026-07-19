// ==========================================================
// استدعاء المكتبات التي يحتاجها هذا الـ Controller
// ==========================================================

// استدعاء كلاس إعدادات الترقيم الموجود في مشروع Core
using AlTayerERP.Core.Entities;

// استدعاء الاتصال بقاعدة البيانات
using AlTayerERP.Infrastructure.Data;

// استدعاء مكتبات إنشاء Web API
using Microsoft.AspNetCore.Mvc;

// استدعاء Entity Framework
using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;


namespace AlTayerERP.API.Controllers
{
    // ==========================================================
    // هذا Controller خاص بإعدادات الترقيم
    // الرابط سيكون:
    // http://localhost:5012/api/NumberingSettings
    // ==========================================================
    [Route("api/[controller]")]
    [ApiController]
    public class NumberingSettingsController : ControllerBase
    {
        // الاتصال بقاعدة البيانات
        private readonly AppDbContext _context;

        // Constructor يستقبل الاتصال بقاعدة البيانات تلقائياً
        public NumberingSettingsController(AppDbContext context)
        {
            _context = context;
        }

        // ======================================================
        // جلب جميع إعدادات الترقيم
        // GET api/NumberingSettings
        // ======================================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await _context.Numbering_Settings
                    .OrderBy(x => x.Document_Type)
                    .ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }

        // ======================================================
        // حفظ أو تعديل إعداد ترقيم
        // POST api/NumberingSettings
        // ======================================================
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] NumberingSetting model)
        {
            try
            {
                if (model == null)
                    return BadRequest("لم تصل بيانات إعداد الترقيم.");

                if (string.IsNullOrWhiteSpace(model.Document_Type))
                    return BadRequest("نوع المستند مطلوب.");

                if (string.IsNullOrWhiteSpace(model.Prefix))
                    return BadRequest("البادئة مطلوبة.");

                if (string.IsNullOrWhiteSpace(model.Reset_Type))
                    return BadRequest("طريقة التصفير مطلوبة.");

                // تنظيف القيم قبل الحفظ
                model.Document_Type = model.Document_Type.Trim();
                model.Prefix = model.Prefix.Trim().ToUpper();
                model.Reset_Type = model.Reset_Type.Trim();

                if (model.Digits_Count <= 0)
                    model.Digits_Count = 4;

                if (model.Last_Number < 0)
                    model.Last_Number = 0;

                // ==================================================
                // إذا كان رقم السجل = صفر فهذا سجل جديد
                // ==================================================
                if (model.Numbering_ID == 0)
                {
                    // منع تكرار نفس نوع المستند
                    bool exists = await _context.Numbering_Settings
                        .AnyAsync(x => x.Document_Type == model.Document_Type);

                    if (exists)
                        return BadRequest("يوجد إعداد ترقيم لهذا المستند مسبقاً.");

                    await _context.Numbering_Settings.AddAsync(model);
                }
                else
                {
                    // ==================================================
                    // إذا كان السجل موجوداً نقوم بتحديثه
                    // ==================================================

                    var oldSetting = await _context.Numbering_Settings
                        .FirstOrDefaultAsync(x => x.Numbering_ID == model.Numbering_ID);

                    if (oldSetting == null)
                        return NotFound("إعداد الترقيم غير موجود.");

                    // منع تغيير نوع المستند إلى نوع موجود في سجل آخر
                    bool duplicate = await _context.Numbering_Settings
                        .AnyAsync(x =>
                            x.Document_Type == model.Document_Type &&
                            x.Numbering_ID != model.Numbering_ID);

                    if (duplicate)
                        return BadRequest("يوجد إعداد ترقيم آخر لنفس نوع المستند.");

                    oldSetting.Document_Type = model.Document_Type;
                    oldSetting.Prefix = model.Prefix;
                    oldSetting.Digits_Count = model.Digits_Count;
                    oldSetting.Reset_Type = model.Reset_Type;
                    oldSetting.Last_Number = model.Last_Number;
                    oldSetting.Use_Company = model.Use_Company;
                    oldSetting.Use_Branch = model.Use_Branch;
                    oldSetting.Use_Year = model.Use_Year;
                    oldSetting.Is_Active = model.Is_Active;
                }

                await _context.SaveChangesAsync();

                return Ok(model);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }

        // ======================================================
        // حذف إعداد ترقيم حسب رقمه
        // DELETE api/NumberingSettings/1
        // ======================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var setting = await _context.Numbering_Settings.FindAsync(id);

                if (setting == null)
                    return NotFound("إعداد الترقيم غير موجود.");

                _context.Numbering_Settings.Remove(setting);

                await _context.SaveChangesAsync();

                return Ok("تم الحذف بنجاح");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }
        // ======================================================
        // توليد رقم مستند جديد حسب إعدادات الترقيم
        // GET api/NumberingSettings/GenerateNumber
        // مثال:
        // api/NumberingSettings/GenerateNumber?documentType=RECEIPT&companyId=FG-00001&branchId=1&year=2026
        // ======================================================
        [HttpGet("GenerateNumber")]
        public async Task<IActionResult> GenerateNumber(
            [FromQuery] string documentType,
            [FromQuery] string companyId,
            [FromQuery] int branchId,
            [FromQuery] int year)
        {
            try
            {
                // التحقق من البيانات المطلوبة
                if (string.IsNullOrWhiteSpace(documentType))
                    return BadRequest("نوع المستند مطلوب.");

                if (string.IsNullOrWhiteSpace(companyId))
                    return BadRequest("معرف الشركة مطلوب.");

                if (branchId <= 0)
                    return BadRequest("معرف الفرع مطلوب.");

                if (year <= 0)
                    return BadRequest("السنة المالية مطلوبة.");

                documentType = documentType.Trim();

                // جلب إعداد الترقيم النشط لنوع المستند
                var setting = await _context.Numbering_Settings
                    .FirstOrDefaultAsync(x =>
                        x.Document_Type == documentType &&
                        x.Is_Active);

                if (setting == null)
                {
                    return NotFound(
                        $"لا يوجد إعداد ترقيم نشط لنوع المستند: {documentType}");
                }

                // زيادة آخر رقم
                int nextNumber = setting.Last_Number + 1;

                // إنشاء الجزء الرقمي حسب عدد الخانات
                string serialPart =
                    nextNumber.ToString().PadLeft(
                        setting.Digits_Count,
                        '0');

                // تكوين أجزاء الرقم النهائي
                var numberParts = new List<string>();

                // إضافة البادئة
                if (!string.IsNullOrWhiteSpace(setting.Prefix))
                {
                    numberParts.Add(
                        setting.Prefix.Trim().ToUpper());
                }

                // إضافة الشركة حسب الإعداد
                if (setting.Use_Company)
                {
                    numberParts.Add(companyId);
                }

                // إضافة الفرع حسب الإعداد
                if (setting.Use_Branch)
                {
                    numberParts.Add(branchId.ToString());
                }

                // إضافة السنة حسب الإعداد
                if (setting.Use_Year)
                {
                    numberParts.Add(year.ToString());
                }

                // إضافة الرقم التسلسلي
                numberParts.Add(serialPart);

                // تكوين الرقم النهائي
                string generatedNumber =
                    string.Join("-", numberParts);

                // ملاحظة:
                // هنا لا نقوم بتحديث Last_Number حتى لا نحجز الرقم
                // قبل الحفظ الفعلي للسند.
                // التحديث النهائي سيتم داخل عملية حفظ السند.

                return Ok(new
                {
                    Document_Type = documentType,
                    Generated_Number = generatedNumber,
                    Next_Number = nextNumber
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }
    }
}