using AlTayerERP.Core.Entities.Accounting;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AlTayerERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentMethodsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PaymentMethodsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _context.Payment_Methods.AsNoTracking()
                .OrderBy(x => x.Sort_Order)
                .ThenBy(x => x.Payment_Method_Name_AR)
                .ToListAsync();
            return Ok(items);
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> GetActiveLookup()
        {
            var items = await _context.Payment_Methods.AsNoTracking()
                .Where(x => x.Is_Active)
                .OrderBy(x => x.Sort_Order)
                .ThenBy(x => x.Payment_Method_Name_AR)
                .Select(x => new
                {
                    x.Payment_Method_ID,
                    x.Payment_Method_Code,
                    x.Payment_Method_Name_AR,
                    x.Requires_Reference,
                    x.Requires_Reference_Date,
                    x.Is_Cash,
                    x.Is_Bank
                })
                .ToListAsync();
            return Ok(items);
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PaymentMethodRequest request)
        {
            if (!IsValid(request, out string message))
                return BadRequest(message);

            string code = request.Payment_Method_Code.Trim().ToUpperInvariant();
            if (await _context.Payment_Methods.AnyAsync(x => x.Payment_Method_Code == code))
                return Conflict("كود طريقة السداد مستخدم مسبقاً.");

            var item = new PaymentMethod();
            Apply(item, request, code);
            _context.Payment_Methods.Add(item);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAll), new { id = item.Payment_Method_ID }, item);
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] PaymentMethodRequest request)
        {
            if (!IsValid(request, out string message))
                return BadRequest(message);

            PaymentMethod? item = await _context.Payment_Methods
                .FirstOrDefaultAsync(x => x.Payment_Method_ID == id);
            if (item == null)
                return NotFound("طريقة السداد غير موجودة.");

            string code = request.Payment_Method_Code.Trim().ToUpperInvariant();
            if (await _context.Payment_Methods.AnyAsync(
                x => x.Payment_Method_ID != id && x.Payment_Method_Code == code))
                return Conflict("كود طريقة السداد مستخدم مسبقاً.");

            Apply(item, request, code);
            await _context.SaveChangesAsync();
            return Ok(item);
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpPost("{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            PaymentMethod? item = await _context.Payment_Methods
                .FirstOrDefaultAsync(x => x.Payment_Method_ID == id);
            if (item == null)
                return NotFound("طريقة السداد غير موجودة.");

            item.Is_Active = false;
            await _context.SaveChangesAsync();
            return Ok(item);
        }

        private static bool IsValid(PaymentMethodRequest request, out string message)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Payment_Method_Code) ||
                string.IsNullOrWhiteSpace(request.Payment_Method_Name_AR))
            {
                message = "كود طريقة السداد واسمها العربي مطلوبان.";
                return false;
            }

            if (request.Is_Cash && request.Is_Bank)
            {
                message = "لا يمكن أن تكون طريقة السداد نقدية وبنكية في الوقت نفسه.";
                return false;
            }

            if (request.Requires_Reference_Date && !request.Requires_Reference)
            {
                message = "تاريخ المرجع يتطلب تفعيل رقم المرجع أولاً.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        private static void Apply(PaymentMethod item, PaymentMethodRequest request, string code)
        {
            item.Payment_Method_Code = code;
            item.Payment_Method_Name_AR = request.Payment_Method_Name_AR.Trim();
            item.Payment_Method_Name_EN = string.IsNullOrWhiteSpace(request.Payment_Method_Name_EN)
                ? null
                : request.Payment_Method_Name_EN.Trim();
            item.Requires_Reference = request.Requires_Reference;
            item.Requires_Reference_Date = request.Requires_Reference_Date;
            item.Is_Cash = request.Is_Cash;
            item.Is_Bank = request.Is_Bank;
            item.Is_Active = request.Is_Active;
            item.Sort_Order = request.Sort_Order;
        }
    }

    public class PaymentMethodRequest
    {
        public string Payment_Method_Code { get; set; } = string.Empty;
        public string Payment_Method_Name_AR { get; set; } = string.Empty;
        public string? Payment_Method_Name_EN { get; set; }
        public bool Requires_Reference { get; set; }
        public bool Requires_Reference_Date { get; set; }
        public bool Is_Cash { get; set; }
        public bool Is_Bank { get; set; }
        public bool Is_Active { get; set; } = true;
        public int Sort_Order { get; set; }
    }
}