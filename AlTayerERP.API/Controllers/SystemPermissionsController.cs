using AlTayerERP.Infrastructure.Data;
using AlTayerERP.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SystemPermissionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SystemPermissionsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetByType/{type}")]
        public async Task<ActionResult<IEnumerable<SystemPermission>>> GetByType(string type)
        {
            var list = await _context.System_Permissions
                .Where(x => x.Permission_Type == type && x.Is_Active)
                .OrderBy(x => x.Sort_Order)
                .ToListAsync();

            return Ok(list);
        }
    }
}