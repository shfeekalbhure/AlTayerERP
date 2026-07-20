using AlTayerERP.Core.Entities;
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

        [HttpGet]
        public async Task<IActionResult> GetScreens(
            [FromQuery] string? search = null,
            [FromQuery] string? moduleName = null,
            [FromQuery] bool activeOnly = false)
        {
            IQueryable<SystemScreen> query = _context.SystemScreens.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim();
                query = query.Where(x =>
                    x.Screen_Code.Contains(term) ||
                    x.Screen_Name.Contains(term) ||
                    x.Module_Name.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(moduleName))
            {
                string module = moduleName.Trim();
                query = query.Where(x => x.Module_Name == module);
            }

            if (activeOnly)
            {
                query = query.Where(x => x.Is_Active);
            }

            List<SystemScreenListItemDto> screens = await query
                .OrderBy(x => x.Module_Name)
                .ThenBy(x => x.Sort_Order)
                .ThenBy(x => x.Screen_Name)
                .Select(x => new SystemScreenListItemDto
                {
                    Screen_ID = x.Screen_ID,
                    Screen_Code = x.Screen_Code,
                    Screen_Name = x.Screen_Name,
                    Module_Name = x.Module_Name,
                    Is_Active = x.Is_Active,
                    Sort_Order = x.Sort_Order,
                    Created_At = x.Created_At
                })
                .ToListAsync();

            return Ok(screens);
        }

        [HttpGet("modules")]
        public async Task<IActionResult> GetModules()
        {
            List<SystemScreenModuleDto> modules = await _context.SystemScreens
                .AsNoTracking()
                .OrderBy(x => x.Module_Name)
                .ThenBy(x => x.Sort_Order)
                .GroupBy(x => x.Module_Name)
                .Select(group => new SystemScreenModuleDto
                {
                    Module_Name = group.Key,
                    Screen_Count = group.Count(),
                    Active_Screen_Count = group.Count(x => x.Is_Active),
                    Screens = group
                        .OrderBy(x => x.Sort_Order)
                        .ThenBy(x => x.Screen_Name)
                        .Select(x => new SystemScreenLookupDto
                        {
                            Screen_ID = x.Screen_ID,
                            Screen_Code = x.Screen_Code,
                            Screen_Name = x.Screen_Name,
                            Is_Active = x.Is_Active,
                            Sort_Order = x.Sort_Order
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(modules);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetScreenById(int id)
        {
            SystemScreen? screen = await _context.SystemScreens
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Screen_ID == id);

            if (screen is null)
            {
                return NotFound(new { message = "الشاشة المطلوبة غير موجودة." });
            }

            return Ok(new SystemScreenDetailsDto
            {
                Screen_ID = screen.Screen_ID,
                Screen_Code = screen.Screen_Code,
                Screen_Name = screen.Screen_Name,
                Module_Name = screen.Module_Name,
                Is_Active = screen.Is_Active,
                Sort_Order = screen.Sort_Order,
                Created_At = screen.Created_At
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateScreen([FromBody] UpsertSystemScreenRequest request)
        {
            string? validationError = ValidateRequest(request);
            if (validationError is not null)
            {
                return BadRequest(new { message = validationError });
            }

            bool duplicateExists = await _context.SystemScreens.AnyAsync(x => x.Screen_Code == request.Screen_Code.Trim());
            if (duplicateExists)
            {
                return Conflict(new { message = "رمز الشاشة مستخدم مسبقاً." });
            }

            SystemScreen entity = new SystemScreen
            {
                Screen_Code = request.Screen_Code.Trim(),
                Screen_Name = request.Screen_Name.Trim(),
                Module_Name = request.Module_Name.Trim(),
                Is_Active = request.Is_Active,
                Sort_Order = request.Sort_Order,
                Created_At = DateTime.Now
            };

            _context.SystemScreens.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetScreenById), new { id = entity.Screen_ID }, new
            {
                entity.Screen_ID,
                entity.Screen_Code,
                entity.Screen_Name,
                entity.Module_Name,
                entity.Is_Active,
                entity.Sort_Order
            });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateScreen(int id, [FromBody] UpsertSystemScreenRequest request)
        {
            string? validationError = ValidateRequest(request);
            if (validationError is not null)
            {
                return BadRequest(new { message = validationError });
            }

            SystemScreen? entity = await _context.SystemScreens.FirstOrDefaultAsync(x => x.Screen_ID == id);
            if (entity is null)
            {
                return NotFound(new { message = "الشاشة المطلوبة غير موجودة." });
            }

            string normalizedCode = request.Screen_Code.Trim();
            bool duplicateExists = await _context.SystemScreens.AnyAsync(x => x.Screen_ID != id && x.Screen_Code == normalizedCode);
            if (duplicateExists)
            {
                return Conflict(new { message = "رمز الشاشة مستخدم مسبقاً." });
            }

            entity.Screen_Code = normalizedCode;
            entity.Screen_Name = request.Screen_Name.Trim();
            entity.Module_Name = request.Module_Name.Trim();
            entity.Is_Active = request.Is_Active;
            entity.Sort_Order = request.Sort_Order;

            await _context.SaveChangesAsync();
            return Ok(new { message = "تم تحديث الشاشة بنجاح." });
        }

        private static string? ValidateRequest(UpsertSystemScreenRequest? request)
        {
            if (request is null)
            {
                return "بيانات الطلب غير صالحة.";
            }

            if (string.IsNullOrWhiteSpace(request.Screen_Code))
            {
                return "رمز الشاشة مطلوب.";
            }

            if (string.IsNullOrWhiteSpace(request.Screen_Name))
            {
                return "اسم الشاشة مطلوب.";
            }

            if (string.IsNullOrWhiteSpace(request.Module_Name))
            {
                return "اسم الوحدة مطلوب.";
            }

            if (request.Sort_Order < 0)
            {
                return "الترتيب يجب أن يكون صفراً أو أكبر.";
            }

            return null;
        }

        public sealed class SystemScreenListItemDto
        {
            public int Screen_ID { get; set; }
            public string Screen_Code { get; set; } = string.Empty;
            public string Screen_Name { get; set; } = string.Empty;
            public string Module_Name { get; set; } = string.Empty;
            public bool Is_Active { get; set; }
            public int Sort_Order { get; set; }
            public DateTime Created_At { get; set; }
        }

        public sealed class SystemScreenLookupDto
        {
            public int Screen_ID { get; set; }
            public string Screen_Code { get; set; } = string.Empty;
            public string Screen_Name { get; set; } = string.Empty;
            public bool Is_Active { get; set; }
            public int Sort_Order { get; set; }
        }

        public sealed class SystemScreenModuleDto
        {
            public string Module_Name { get; set; } = string.Empty;
            public int Screen_Count { get; set; }
            public int Active_Screen_Count { get; set; }
            public List<SystemScreenLookupDto> Screens { get; set; } = new();
        }

        public sealed class SystemScreenDetailsDto
        {
            public int Screen_ID { get; set; }
            public string Screen_Code { get; set; } = string.Empty;
            public string Screen_Name { get; set; } = string.Empty;
            public string Module_Name { get; set; } = string.Empty;
            public bool Is_Active { get; set; }
            public int Sort_Order { get; set; }
            public DateTime Created_At { get; set; }
        }

        public sealed class UpsertSystemScreenRequest
        {
            public string Screen_Code { get; set; } = string.Empty;
            public string Screen_Name { get; set; } = string.Empty;
            public string Module_Name { get; set; } = string.Empty;
            public bool Is_Active { get; set; } = true;
            public int Sort_Order { get; set; }
        }
    }
}
