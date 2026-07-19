using AlTayerERP.API.DTOs;
using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CostCentersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly CostCenterNumberService _costCenterNumberService;

        public CostCentersController(
            AppDbContext context,
            CostCenterNumberService costCenterNumberService)
        {
            _context = context;
            _costCenterNumberService = costCenterNumberService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCostCenters([FromQuery] string companyId)
        {
            if (string.IsNullOrWhiteSpace(companyId))
                return BadRequest("رقم الشركة مطلوب.");

            var data = await _context.Cost_Centers
                .AsNoTracking()
                .Where(x => x.Company_ID == companyId.Trim())
                .OrderBy(x => x.Center_Code)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCostCenterById(string id, [FromQuery] string companyId)
        {
            var item = await _context.Cost_Centers
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Cost_Center_ID == id &&
                    x.Company_ID == companyId.Trim());

            if (item == null)
                return NotFound("مركز التكلفة غير موجود.");

            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCostCenter([FromBody] CreateCostCenterDto dto)
        {
            if (dto == null)
                return BadRequest("البيانات غير صحيحة.");

            if (string.IsNullOrWhiteSpace(dto.Company_ID))
                return BadRequest("رقم الشركة مطلوب.");

            if (string.IsNullOrWhiteSpace(dto.Center_Name_AR))
                return BadRequest("اسم مركز التكلفة مطلوب.");

            string companyId = dto.Company_ID.Trim();

            int level = 1;

            if (!string.IsNullOrWhiteSpace(dto.Parent_Cost_Center_ID))
            {
                var parent = await _context.Cost_Centers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Cost_Center_ID == dto.Parent_Cost_Center_ID &&
                        x.Company_ID == companyId);

                if (parent == null)
                    return BadRequest("مركز التكلفة الأب غير موجود.");

                level = parent.Center_Level + 1;
            }

            var costCenter = new CostCenter
            {
                Cost_Center_ID = Guid.NewGuid().ToString(),
                Company_ID = companyId,
                Parent_Cost_Center_ID = string.IsNullOrWhiteSpace(dto.Parent_Cost_Center_ID)
                    ? null
                    : dto.Parent_Cost_Center_ID.Trim(),
                Center_Code = await _costCenterNumberService.GenerateNextCodeAsync(companyId, dto.Parent_Cost_Center_ID),
                Center_Name_AR = dto.Center_Name_AR.Trim(),
                Center_Name_EN = dto.Center_Name_EN?.Trim(),
                Center_Level = level,
                Is_Postable = dto.Is_Postable,
                Is_Active = dto.Is_Active,
                Created_At = DateTime.Now
            };

            await _context.Cost_Centers.AddAsync(costCenter);
            await _context.SaveChangesAsync();

            return Ok(costCenter);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCostCenter(string id, [FromBody] CreateCostCenterDto dto)
        {
            string companyId = dto.Company_ID.Trim();

            var item = await _context.Cost_Centers
                .FirstOrDefaultAsync(x =>
                    x.Cost_Center_ID == id &&
                    x.Company_ID == companyId);

            if (item == null)
                return NotFound("مركز التكلفة غير موجود.");

            // تنظيف ومعالجة المعرف الجديد للأب مع تفادي تحذير nullable
            string? newParentId = string.IsNullOrWhiteSpace(dto.Parent_Cost_Center_ID) ? null : dto.Parent_Cost_Center_ID.Trim();

            // التحقق من منع ربط المركز بنفسه كأب لمنع حلقة تكرارية نهائية (Infinite Loop)
            if (newParentId == id)
                return BadRequest("لا يمكن تعيين مركز التكلفة كأب لنفسه.");

            // إذا تم تغيير المركز الأب، نعيد احتساب كود الأب ومستواه، ونحدث مستويات الأبناء هرمياً
            if (item.Parent_Cost_Center_ID != newParentId)
            {
                int newLevel = 1;
                if (newParentId != null)
                {
                    var parent = await _context.Cost_Centers
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Cost_Center_ID == newParentId && x.Company_ID == companyId);

                    if (parent == null)
                        return BadRequest("مركز التكلفة الأب الجديد غير موجود.");

                    newLevel = parent.Center_Level + 1;
                }

                item.Parent_Cost_Center_ID = newParentId;
                item.Center_Level = newLevel;

                // توليد كود تسلسلي جديد للمركز الأب المنقول تحت الأب الجديد
                item.Center_Code = await _costCenterNumberService.GenerateNextCodeAsync(companyId, newParentId);

                // استدعاء دالة تحديث مستويات الأبناء المتأثرين هرمياً فقط
                await UpdateChildrenHierarchyAsync(companyId, item.Cost_Center_ID, item.Center_Level);
            }
            else
            {
                // إذا لم يتغير الأب، نقوم بتحديث الكود يدوياً فقط في حال تم إرساله من الـ DTO
                if (!string.IsNullOrWhiteSpace(dto.Center_Code))
                {
                    item.Center_Code = dto.Center_Code.Trim();
                }
            }

            item.Center_Name_AR = dto.Center_Name_AR.Trim();
            item.Center_Name_EN = dto.Center_Name_EN?.Trim();
            item.Is_Postable = dto.Is_Postable;
            item.Is_Active = dto.Is_Active;

            await _context.SaveChangesAsync();

            return Ok(item);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCostCenter(string id, [FromQuery] string companyId)
        {
            var item = await _context.Cost_Centers
                .FirstOrDefaultAsync(x =>
                    x.Cost_Center_ID == id &&
                    x.Company_ID == companyId.Trim());

            if (item == null)
                return NotFound("مركز التكلفة غير موجود.");

            bool hasChildren = await _context.Cost_Centers.AnyAsync(x =>
                x.Parent_Cost_Center_ID == id &&
                x.Company_ID == companyId.Trim());

            if (hasChildren)
                return BadRequest("لا يمكن حذف مركز تكلفة له مراكز فرعية.");

            _context.Cost_Centers.Remove(item);
            await _context.SaveChangesAsync();

            return Ok("تم حذف مركز التكلفة بنجاح.");
        }

        /// <summary>
        /// دالة داخلية تكرارية (Recursive) لتحديث مستويات (Center_Level) الأبناء والأحفاد فقط عند نقل الأب
        /// </summary>
        private async Task UpdateChildrenHierarchyAsync(string companyId, string parentId, int parentLevel)
        {
            var children = await _context.Cost_Centers
                .Where(x => x.Parent_Cost_Center_ID == parentId && x.Company_ID == companyId)
                .ToListAsync();

            foreach (var child in children)
            {
                child.Center_Level = parentLevel + 1;

                // استدعاء الدالة مجدداً لتحديث مستويات أحفاد هذا الابن تكرارياً دون المساس بأكوادهم
                await UpdateChildrenHierarchyAsync(companyId, child.Cost_Center_ID, child.Center_Level);
            }
        }
    }
}