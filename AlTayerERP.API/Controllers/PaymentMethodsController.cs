using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AlTayerERP.API.Controllers
{
    /// <summary>
    /// قراءة طرق السداد النشطة لربطها بسند القبض والقوائم المرجعية.
    /// التعديل الإداري يبقى مغلقاً إلى أن تعتمد مصادقة خادمية وصلاحيات كتابة مالية.
    /// </summary>
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
            var items = await _context.Payment_Methods
                .AsNoTracking()
                .OrderBy(x => x.Sort_Order)
                .ThenBy(x => x.Payment_Method_Name_AR)
                .Select(x => new
                {
                    x.Payment_Method_ID,
                    x.Payment_Method_Code,
                    x.Payment_Method_Name_AR,
                    x.Payment_Method_Name_EN,
                    x.Requires_Reference,
                    x.Requires_Reference_Date,
                    x.Is_Cash,
                    x.Is_Bank,
                    x.Is_Active,
                    x.Sort_Order
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> GetActiveLookup()
        {
            var items = await _context.Payment_Methods
                .AsNoTracking()
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
    }
}