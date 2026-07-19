using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SystemScreensController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SystemScreensController(AppDbContext context)
        {
            _context = context;
        }

        // ======================================================
        // جلب جميع شاشات النظام
        // ======================================================
        [HttpGet]
        public async Task<IActionResult> GetScreens()
        {
            var screens = await _context.SystemScreens
                .OrderBy(x => x.Sort_Order)
                .ToListAsync();

            return Ok(screens);
        }
    }
}