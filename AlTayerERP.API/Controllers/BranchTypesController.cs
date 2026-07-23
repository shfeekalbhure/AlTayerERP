using AlTayerERP.API.DTOs;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers;

[ApiController]
[Route("api/branch-types")]
public class BranchTypesController : ControllerBase
{
    private readonly AppDbContext _db;
    public BranchTypesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Branch_Types.AsNoTracking().OrderBy(x => x.Sort_Order).ThenBy(x => x.Branch_Type_Name_AR).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(CreateBranchTypeDto dto)
    {
        var code = dto.Branch_Type_Code?.Trim().ToUpperInvariant();
        var name = dto.Branch_Type_Name_AR?.Trim();
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            return BadRequest(new { message = "رمز نوع الفرع واسمه العربي مطلوبان." });
        if (await _db.Branch_Types.AnyAsync(x => x.Branch_Type_Code == code || x.Branch_Type_Name_AR == name))
            return Conflict(new { message = "رمز نوع الفرع أو اسمه موجود مسبقاً." });

        var item = new BranchType { Branch_Type_Code = code, Branch_Type_Name_AR = name, Branch_Type_Name_EN = dto.Branch_Type_Name_EN?.Trim(), Allows_Financial_Operations = dto.Allows_Financial_Operations, Is_Active = dto.Is_Active, Sort_Order = dto.Sort_Order, Created_At = DateTime.UtcNow };
        _db.Branch_Types.Add(item);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = item.Branch_Type_ID }, item);
    }
}