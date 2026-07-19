// استدعاء المكاتب والقيود اللازمة لعمل الـ Controller والاتصال بقاعدة البيانات
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlTayerERP.API.DTOs;
using AlTayerERP.Core.Entities;

namespace AlTayerERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolePermissionsController : ControllerBase
    {
        // سياق اتصال قاعدة البيانات المركزي (DbContext)
        private readonly AppDbContext _context;

        /// <summary>
        /// مشيد الكنترولر لحقن الاعتمادية الخاصة بقاعدة البيانات
        /// </summary>
        public RolePermissionsController(AppDbContext context)
        {
            _context = context;
        }

        // ======================================================
        // جلب جميع الشاشات النشطة في النظام لاستخدامها في جداول الصلاحيات
        // الرابط: GET api/RolePermissions/GetScreens
        // ======================================================
        [HttpGet("GetScreens")]
        public async Task<IActionResult> GetScreens()
        {
            // جلب الشاشات النشطة فقط وترتيبها بحسب حقل Sort_Order المحدد مسبقاً
            var screens = await _context.SystemScreens
                .Where(x => x.Is_Active)
                .OrderBy(x => x.Sort_Order)
                .Select(x => new
                {
                    x.Screen_ID,
                    x.Screen_Code,
                    x.Screen_Name,
                    x.Module_Name
                })
                .ToListAsync();

            // إرجاع النتيجة بنجاح مع قائمة الشاشات
            return Ok(screens);
        }

        // ======================================================
        // جلب الصلاحيات الفعلية المخزنة لدور (مجموعة) معين بناءً على الـ roleId
        // الرابط: GET api/RolePermissions/GetRolePermissions/{roleId}
        // ======================================================
        [HttpGet("GetRolePermissions/{roleId}")]
        public async Task<IActionResult> GetRolePermissions(int roleId)
        {
            // استعلام مباشر لجلب أسطر الصلاحيات التابعة للدور المحدد من قاعدة البيانات
            var permissions = await _context.RolePermissions
                .Where(x => x.Role_ID == roleId)
                .ToListAsync();

            // إرجاع القائمة فوراً لتأشير خانات الـ CheckBoxes في الـ Desktop
            return Ok(permissions);
        }

        // ======================================================
        // حفظ وتحديث صلاحيات دور معين بأمان حركي كامل (Transactions)
        // الرابط: POST api/RolePermissions/SaveRolePermissions
        // ======================================================
        [HttpPost("SaveRolePermissions")]
        public async Task<IActionResult> SaveRolePermissions([FromBody] List<SaveRolePermissionDto> permissions)
        {
            // فحص أولي لضمان عدم إرسال مصفوفة فارغة تسبب أخطاء بالسيرفر
            if (permissions == null || permissions.Count == 0)
                return BadRequest("لا توجد صلاحيات للحفظ.");

            // 1. استخراج الـ Role_ID الرئيسي من السطر الأول للعملية
            int roleId = permissions.First().Role_ID;

            // 🛡️ حماية أمنية هندسية: التأكد من أن جميع الأسطر المرسلة تخص نفس الدور لمنع تداخل أو تزوير البيانات
            if (permissions.Any(x => x.Role_ID != roleId))
                return BadRequest("خطأ: لا يمكن حفظ صلاحيات لأدوار متعددة في طلب واحد.");

            // 🛠️ فتح عملية حركية (Transaction) لضمان تنفيذ الحذف والإضافة معاً بنجاح أو التراجع الكامل في حال حدوث خطأ
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 2. جلب وحذف جميع الصلاحيات القديمة التابعة لهذا الدور دفعة واحدة لتفادي تكرار الأسطر
                var oldPermissions = await _context.RolePermissions
                    .Where(x => x.Role_ID == roleId)
                    .ToListAsync();

                if (oldPermissions.Any())
                {
                    _context.RolePermissions.RemoveRange(oldPermissions);
                }

                // 3. تجميع السطور الجديدة داخل قائمة (List) لرفع كفاءة الإضافة الحجمية وتخفيف الضغط على السيرفر
                var newPermissionsList = new List<RolePermission>();
                foreach (var item in permissions)
                {
                    newPermissionsList.Add(new RolePermission
                    {
                        Role_ID = item.Role_ID,
                        Screen_ID = item.Screen_ID,
                        Can_View = item.Can_View,
                        Can_Add = item.Can_Add,
                        Can_Edit = item.Can_Edit,
                        Can_Delete = item.Can_Delete,
                        Can_Print = item.Can_Print,
                        Can_Export = item.Can_Export,
                        Can_Import = item.Can_Import,
                        Can_Approve = item.Can_Approve,
                        Can_UnApprove = item.Can_UnApprove
                    });
                }

                // 4. تنفيذ الإضافة الحجمية السريعة (Bulk Insert) لقائمة الكائنات بالكامل
                await _context.RolePermissions.AddRangeAsync(newPermissionsList);

                // 5. حفظ كافة التعديلات وتأكيد نجاح الترانزاكشن نهائياً في قاعدة البيانات
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // إرجاع رسالة النجاح الصريحة
                return Ok("تم حفظ الصلاحيات بنجاح.");
            }
            catch (Exception ex)
            {
                // ⚠️ في حال حدوث أي خطأ مفاجئ بالشبكة أو السيرفر، يتم التراجع عن عملية الحذف والإضافة فوراً لحماية النظام من تطاير الصلاحيات
                await transaction.RollbackAsync();
                return StatusCode(500, $"حدث خطأ داخلي بالسيرفر أثناء الحفظ: {ex.Message}");
            }
        }
    }
}