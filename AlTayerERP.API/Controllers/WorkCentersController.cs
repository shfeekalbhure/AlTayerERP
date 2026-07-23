using AlTayerERP.API.DTOs;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers;

[ApiController]
[Route("api/work-centers")]
public class WorkCentersController : ControllerBase
{
    private readonly AppDbContext _db;
    public WorkCentersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string companyId, [FromQuery] int? branchId = null)
    {
        if (string.IsNullOrWhiteSpace(companyId)) return BadRequest(new { message = "معرف الشركة مطلوب." });
        var query = _db.Work_Centers.AsNoTracking().Where(x => x.Company_ID == companyId);
        if (branchId.HasValue) query = query.Where(x => x.Branch_ID == branchId || x.Branch_ID == null);
        return Ok(await query.OrderBy(x => x.Sort_Order).ThenBy(x => x.Work_Center_Name_AR).ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateWorkCenterDto dto)
    {
        var companyId = dto.Company_ID?.Trim();
        var code = dto.Work_Center_Code?.Trim().ToUpperInvariant();
        var name = dto.Work_Center_Name_AR?.Trim();
        var type = dto.Work_Center_Type?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(companyId) || string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(type))
            return BadRequest(new { message = "الشركة والرمز والاسم العربي ونوع مركز العمل مطلوبة." });
        if (!await _db.Companies.AnyAsync(x => x.Company_ID == companyId && x.Is_Active))
            return BadRequest(new { message = "الشركة المختارة غير موجودة أو غير نشطة." });
        if (dto.Branch_ID.HasValue && !await _db.Tenant_Branches.AnyAsync(x => x.Branch_ID == dto.Branch_ID && x.Company_ID == companyId && x.Is_Active))
            return BadRequest(new { message = "الفرع لا يتبع الشركة المحددة أو غير نشط." });
        if (dto.Parent_Work_Center_ID.HasValue && !await _db.Work_Centers.AnyAsync(x => x.Work_Center_ID == dto.Parent_Work_Center_ID && x.Company_ID == companyId))
            return BadRequest(new { message = "مركز العمل الأب لا يتبع الشركة المحددة." });
        if (await _db.Work_Centers.AnyAsync(x => x.Company_ID == companyId && x.Work_Center_Code == code))
            return Conflict(new { message = "رمز مركز العمل موجود مسبقاً داخل الشركة." });

        var item = new WorkCenter { Company_ID = companyId, Branch_ID = dto.Branch_ID, Work_Center_Code = code, Work_Center_Name_AR = name, Work_Center_Name_EN = dto.Work_Center_Name_EN?.Trim(), Work_Center_Type = type, Parent_Work_Center_ID = dto.Parent_Work_Center_ID, Is_Active = dto.Is_Active, Sort_Order = dto.Sort_Order, Created_At = DateTime.UtcNow };
        _db.Work_Centers.Add(item);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { companyId = item.Company_ID, branchId = item.Branch_ID }, item);
    }
}