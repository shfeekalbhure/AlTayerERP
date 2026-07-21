using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AlTayerERP.API.Controllers
{
    /// <summary>
    /// كتالوج وصفي للشاشات وحقولها وأزرارها.
    /// لا يغير البيانات التشغيلية؛ وتكون إدارة التعديل محكومة لاحقاً بالمصادقة الخادمية.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SystemCatalogController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SystemCatalogController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("screens")]
        public async Task<IActionResult> GetScreens()
        {
            var screens = await _context.SystemScreens
                .AsNoTracking()
                .OrderBy(x => x.Module_Name)
                .ThenBy(x => x.Sort_Order)
                .ThenBy(x => x.Screen_Name)
                .Select(x => new
                {
                    x.Screen_ID,
                    x.Screen_Code,
                    x.Screen_Name,
                    x.Module_Name,
                    x.Is_Active,
                    x.Sort_Order
                })
                .ToListAsync();

            return Ok(screens);
        }

        [HttpGet("screens/{screenId:int}/fields")]
        public async Task<IActionResult> GetFields(int screenId)
        {
            bool screenExists = await _context.SystemScreens
                .AsNoTracking()
                .AnyAsync(x => x.Screen_ID == screenId);

            if (!screenExists)
                return NotFound("الشاشة غير موجودة.");

            var fields = await _context.System_Screen_Fields
                .AsNoTracking()
                .Where(x => x.Screen_ID == screenId)
                .OrderBy(x => x.Sort_Order)
                .ThenBy(x => x.Field_Name)
                .Select(x => new
                {
                    x.Screen_Field_ID,
                    x.Field_Code,
                    x.Field_Name,
                    x.Is_Sensitive,
                    x.Default_Required,
                    x.Is_Active,
                    x.Sort_Order
                })
                .ToListAsync();

            return Ok(fields);
        }

        [HttpGet("screens/{screenId:int}/actions")]
        public async Task<IActionResult> GetActions(int screenId)
        {
            bool screenExists = await _context.SystemScreens
                .AsNoTracking()
                .AnyAsync(x => x.Screen_ID == screenId);

            if (!screenExists)
                return NotFound("الشاشة غير موجودة.");

            var actions = await _context.System_Screen_Actions
                .AsNoTracking()
                .Where(x => x.Screen_ID == screenId)
                .OrderBy(x => x.Sort_Order)
                .ThenBy(x => x.Action_Name)
                .Select(x => new
                {
                    x.Screen_Action_ID,
                    x.Action_Code,
                    x.Action_Name,
                    x.Is_Sensitive,
                    x.Requires_Reason,
                    x.Is_Active,
                    x.Sort_Order
                })
                .ToListAsync();

            return Ok(actions);
        }
    }
}