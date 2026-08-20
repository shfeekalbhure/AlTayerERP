using AlTayerERP.API.DTOs;
using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>مراكز تكلفة شجرية مع منع الحلقات ونطاق الشركة الموثوق.</summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public sealed class CostCentersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly CostCenterNumberService _numbers;
        private readonly ScreenAuthorizationService _authorization;
        private readonly AuditTrailService _audit;
        public CostCentersController(AppDbContext context, CostCenterNumberService numbers, ScreenAuthorizationService authorization, AuditTrailService audit)
        { _context = context; _numbers = numbers; _authorization = authorization; _audit = audit; }
        private ServerSession? Session => HttpContext.Items["ServerSession"] as ServerSession;
        private async Task<IActionResult?> RequireAsync(ScreenOperation operation)
        {
            if (Session == null) return Unauthorized(new { message = "انتهت الجلسة أو أنها غير صالحة." });
            return await _authorization.IsAllowedAsync(Session, "CostCenters", operation) ? null : Forbid();
        }

        [HttpGet]
        public async Task<IActionResult> GetCostCenters()
        {
            var error = await RequireAsync(ScreenOperation.View);
            if (error != null || Session == null) return error!;
            return Ok(await _context.Cost_Centers.AsNoTracking().Where(x => x.Company_ID == Session.Company_ID).OrderBy(x => x.Center_Code).ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCostCenterById(string id)
        {
            var error = await RequireAsync(ScreenOperation.View);
            if (error != null || Session == null) return error!;
            var row = await _context.Cost_Centers.AsNoTracking().FirstOrDefaultAsync(x => x.Cost_Center_ID == id && x.Company_ID == Session.Company_ID);
            return row == null ? NotFound(new { message = "مركز التكلفة غير موجود ضمن الشركة الحالية." }) : Ok(row);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCostCenter([FromBody] CreateCostCenterDto dto)
        {
            var error = await RequireAsync(ScreenOperation.Add);
            if (error != null || Session == null) return error!;
            if (dto == null || string.IsNullOrWhiteSpace(dto.Center_Name_AR)) return BadRequest(new { message = "اسم مركز التكلفة العربي مطلوب." });

            var parent = await ParentAsync(dto.Parent_Cost_Center_ID);
            if (!string.IsNullOrWhiteSpace(dto.Parent_Cost_Center_ID) && (parent == null || !parent.Is_Active || parent.Is_Postable))
                return BadRequest(new { message = "مركز التكلفة الأب يجب أن يكون نشطاً وغير قابل للترحيل." });

            var row = new CostCenter
            {
                Cost_Center_ID = Guid.NewGuid().ToString(), Company_ID = Session.Company_ID,
                Parent_Cost_Center_ID = parent?.Cost_Center_ID,
                Center_Code = await _numbers.GenerateNextCodeAsync(Session.Company_ID, parent?.Cost_Center_ID),
                Center_Name_AR = dto.Center_Name_AR.Trim(), Center_Name_EN = Text(dto.Center_Name_EN),
                Center_Level = parent == null ? 1 : parent.Center_Level + 1,
                Is_Postable = dto.Is_Postable, Is_Active = dto.Is_Active, Created_At = DateTime.UtcNow
            };
            _context.Cost_Centers.Add(row);
            _audit.Add(Session, HttpContext, "cost_centers", row.Cost_Center_ID, "CREATE", null, new { row.Center_Code, row.Center_Name_AR, row.Parent_Cost_Center_ID, row.Is_Postable });
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCostCenterById), new { id = row.Cost_Center_ID }, row);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCostCenter(string id, [FromBody] CreateCostCenterDto dto)
        {
            var error = await RequireAsync(ScreenOperation.Edit);
            if (error != null || Session == null) return error!;
            if (dto == null || string.IsNullOrWhiteSpace(dto.Center_Name_AR)) return BadRequest(new { message = "اسم مركز التكلفة العربي مطلوب." });

            var row = await _context.Cost_Centers.FirstOrDefaultAsync(x => x.Cost_Center_ID == id && x.Company_ID == Session.Company_ID);
            if (row == null) return NotFound(new { message = "مركز التكلفة غير موجود ضمن الشركة الحالية." });
            if (dto.Parent_Cost_Center_ID == id || await IsDescendantAsync(dto.Parent_Cost_Center_ID, id)) return BadRequest(new { message = "لا يمكن إنشاء حلقة في شجرة مراكز التكلفة." });

            var parent = await ParentAsync(dto.Parent_Cost_Center_ID);
            if (!string.IsNullOrWhiteSpace(dto.Parent_Cost_Center_ID) && (parent == null || !parent.Is_Active || parent.Is_Postable))
                return BadRequest(new { message = "مركز التكلفة الأب غير صالح." });

            var old = new { row.Center_Name_AR, row.Parent_Cost_Center_ID, row.Is_Postable, row.Is_Active };
            row.Parent_Cost_Center_ID = parent?.Cost_Center_ID; row.Center_Level = parent == null ? 1 : parent.Center_Level + 1;
            row.Center_Name_AR = dto.Center_Name_AR.Trim(); row.Center_Name_EN = Text(dto.Center_Name_EN);
            row.Is_Postable = dto.Is_Postable; row.Is_Active = dto.Is_Active;
            _audit.Add(Session, HttpContext, "cost_centers", row.Cost_Center_ID, "UPDATE", old, new { row.Center_Name_AR, row.Parent_Cost_Center_ID, row.Is_Postable, row.Is_Active });
            await _context.SaveChangesAsync();
            return Ok(row);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Deactivate(string id, [FromQuery] string? reason)
        {
            var error = await RequireAsync(ScreenOperation.Delete);
            if (error != null || Session == null) return error!;
            var row = await _context.Cost_Centers.FirstOrDefaultAsync(x => x.Cost_Center_ID == id && x.Company_ID == Session.Company_ID);
            if (row == null) return NotFound(new { message = "مركز التكلفة غير موجود ضمن الشركة الحالية." });
            if (await _context.Cost_Centers.AnyAsync(x => x.Parent_Cost_Center_ID == id && x.Is_Active))
                return BadRequest(new { message = "لا يمكن إيقاف مركز تكلفة له مراكز أبناء نشطة." });
            row.Is_Active = false;
            _audit.Add(Session, HttpContext, "cost_centers", row.Cost_Center_ID, "DEACTIVATE", new { Is_Active = true }, new { Is_Active = false }, reason);
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم إيقاف مركز التكلفة دون حذف تاريخه." });
        }

        private async Task<CostCenter?> ParentAsync(string? id) => string.IsNullOrWhiteSpace(id) || Session == null ? null :
            await _context.Cost_Centers.FirstOrDefaultAsync(x => x.Cost_Center_ID == id && x.Company_ID == Session.Company_ID);
        private async Task<bool> IsDescendantAsync(string? candidate, string id)
        {
            var next = candidate;
            while (!string.IsNullOrWhiteSpace(next))
            {
                if (next == id) return true;
                next = await _context.Cost_Centers.AsNoTracking().Where(x => x.Cost_Center_ID == next).Select(x => x.Parent_Cost_Center_ID).FirstOrDefaultAsync();
            }
            return false;
        }
        private static string? Text(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}