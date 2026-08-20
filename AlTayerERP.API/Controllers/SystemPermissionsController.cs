using AlTayerERP.Infrastructure.Data;
using AlTayerERP.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>كتالوج إجراءات النظام الثابتة المستخدمة في الصلاحيات والشاشات.</summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SystemPermissionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SystemPermissionsController(AppDbContext context) => _context = context;

        [HttpGet("GetByType/{type}")]
        public async Task<ActionResult<IEnumerable<SystemPermission>>> GetByType(string type)
        {
            if (HttpContext.Items["ServerSession"] is not AlTayerERP.API.Services.ServerSession session ||
                !session.Is_System_Admin)
            {
                return Forbid();
            }

            var normalizedType = type.Trim().ToUpperInvariant();
            var list = await _context.System_Permissions.AsNoTracking()
                .Where(x => x.Permission_Type == normalizedType && x.Is_Active)
                .OrderBy(x => x.Sort_Order)
                .ToListAsync();

            return Ok(list);
        }

        [HttpGet("Actions")]
        public Task<ActionResult<IEnumerable<SystemPermission>>> GetActions() => GetByType("ACTION");
    }
}
