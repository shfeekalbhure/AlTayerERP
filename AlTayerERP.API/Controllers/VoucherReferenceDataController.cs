using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities.Accounting;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>
    /// إدارة القوائم المرجعية اللازمة لسند القبض.
    /// هذه العمليات مخصصة لمدير النظام، وتبقى كل قائمة مستقلة في قاعدة البيانات.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public sealed class VoucherReferenceDataController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VoucherReferenceDataController(AppDbContext context) => _context = context;

        private bool IsSystemAdmin() =>
            HttpContext.Items["ServerSession"] is ServerSession session && session.Is_System_Admin;

        private IActionResult AdminOnly() => Forbid();

        [HttpGet("PaymentMethods")]
        public async Task<IActionResult> GetPaymentMethods()
        {
            if (!IsSystemAdmin()) return AdminOnly();

            return Ok(await _context.Payment_Methods.AsNoTracking()
                .OrderBy(x => x.Sort_Order).ThenBy(x => x.Payment_Method_Name_AR)
                .ToListAsync());
        }

        [HttpPost("PaymentMethods")]
        public async Task<IActionResult> SavePaymentMethod([FromBody] PaymentMethodDto dto)
        {
            if (!IsSystemAdmin()) return AdminOnly();
            if (dto == null || string.IsNullOrWhiteSpace(dto.Payment_Method_Code) ||
                string.IsNullOrWhiteSpace(dto.Payment_Method_Name_AR))
                return BadRequest("كود وطريقة السداد بالعربية مطلوبان.");

            var code = dto.Payment_Method_Code.Trim().ToUpperInvariant();
            var exists = await _context.Payment_Methods.AnyAsync(x =>
                x.Payment_Method_ID != dto.Payment_Method_ID && x.Payment_Method_Code == code);
            if (exists) return BadRequest("كود طريقة السداد مستخدم مسبقاً.");

            var row = dto.Payment_Method_ID > 0
                ? await _context.Payment_Methods.FindAsync(dto.Payment_Method_ID)
                : null;

            if (row == null)
            {
                row = new PaymentMethod();
                _context.Payment_Methods.Add(row);
            }

            row.Payment_Method_Code = code;
            row.Payment_Method_Name_AR = dto.Payment_Method_Name_AR.Trim();
            row.Payment_Method_Name_EN = NullIfWhiteSpace(dto.Payment_Method_Name_EN);
            row.Requires_Reference = dto.Requires_Reference;
            row.Requires_Reference_Date = dto.Requires_Reference_Date;
            row.Is_Cash = dto.Is_Cash;
            row.Is_Bank = dto.Is_Bank;
            row.Sort_Order = dto.Sort_Order;
            row.Is_Active = dto.Is_Active;
            await _context.SaveChangesAsync();

            return Ok(row);
        }

        [HttpGet("VoucherTypes")]
        public async Task<IActionResult> GetVoucherTypes()
        {
            if (!IsSystemAdmin()) return AdminOnly();

            return Ok(await _context.Voucher_Types.AsNoTracking()
                .OrderBy(x => x.Sort_Order).ThenBy(x => x.Voucher_Type_Name_AR)
                .ToListAsync());
        }

        [HttpPost("VoucherTypes")]
        public async Task<IActionResult> SaveVoucherType([FromBody] VoucherTypeDto dto)
        {
            if (!IsSystemAdmin()) return AdminOnly();
            if (dto == null || string.IsNullOrWhiteSpace(dto.Voucher_Type_Code) ||
                string.IsNullOrWhiteSpace(dto.Voucher_Type_Name_AR))
                return BadRequest("كود ونوع السند بالعربية مطلوبان.");

            var code = dto.Voucher_Type_Code.Trim().ToUpperInvariant();
            var exists = await _context.Voucher_Types.AnyAsync(x =>
                x.Voucher_Type_ID != dto.Voucher_Type_ID && x.Voucher_Type_Code == code);
            if (exists) return BadRequest("كود نوع السند مستخدم مسبقاً.");

            var row = dto.Voucher_Type_ID > 0
                ? await _context.Voucher_Types.FindAsync(dto.Voucher_Type_ID)
                : null;

            if (row == null)
            {
                row = new VoucherType();
                _context.Voucher_Types.Add(row);
            }

            row.Voucher_Type_Code = code;
            row.Voucher_Type_Name_AR = dto.Voucher_Type_Name_AR.Trim();
            row.Voucher_Type_Name_EN = NullIfWhiteSpace(dto.Voucher_Type_Name_EN);
            row.Sort_Order = dto.Sort_Order;
            row.Is_Active = dto.Is_Active;
            await _context.SaveChangesAsync();

            return Ok(row);
        }

        [HttpGet("VoucherStatuses")]
        public async Task<IActionResult> GetVoucherStatuses()
        {
            if (!IsSystemAdmin()) return AdminOnly();

            return Ok(await _context.Voucher_Statuses.AsNoTracking()
                .OrderBy(x => x.Sort_Order).ThenBy(x => x.Voucher_Status_Name_AR)
                .ToListAsync());
        }

        [HttpPost("VoucherStatuses")]
        public async Task<IActionResult> SaveVoucherStatus([FromBody] VoucherStatusDto dto)
        {
            if (!IsSystemAdmin()) return AdminOnly();
            if (dto == null || string.IsNullOrWhiteSpace(dto.Voucher_Status_Code) ||
                string.IsNullOrWhiteSpace(dto.Voucher_Status_Name_AR))
                return BadRequest("كود وحالة السند بالعربية مطلوبان.");

            var code = dto.Voucher_Status_Code.Trim().ToUpperInvariant();
            var exists = await _context.Voucher_Statuses.AnyAsync(x =>
                x.Voucher_Status_ID != dto.Voucher_Status_ID && x.Voucher_Status_Code == code);
            if (exists) return BadRequest("كود حالة السند مستخدم مسبقاً.");

            var row = dto.Voucher_Status_ID > 0
                ? await _context.Voucher_Statuses.FindAsync(dto.Voucher_Status_ID)
                : null;

            if (row == null)
            {
                row = new VoucherStatus();
                _context.Voucher_Statuses.Add(row);
            }

            row.Voucher_Status_Code = code;
            row.Voucher_Status_Name_AR = dto.Voucher_Status_Name_AR.Trim();
            row.Voucher_Status_Name_EN = NullIfWhiteSpace(dto.Voucher_Status_Name_EN);
            row.Sort_Order = dto.Sort_Order;
            row.Is_Active = dto.Is_Active;
            await _context.SaveChangesAsync();

            return Ok(row);
        }

        private static string? NullIfWhiteSpace(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public sealed class PaymentMethodDto
    {
        public int Payment_Method_ID { get; set; }
        public string Payment_Method_Code { get; set; } = string.Empty;
        public string Payment_Method_Name_AR { get; set; } = string.Empty;
        public string? Payment_Method_Name_EN { get; set; }
        public bool Requires_Reference { get; set; }
        public bool Requires_Reference_Date { get; set; }
        public bool Is_Cash { get; set; }
        public bool Is_Bank { get; set; }
        public int Sort_Order { get; set; }
        public bool Is_Active { get; set; } = true;
    }

    public sealed class VoucherTypeDto
    {
        public int Voucher_Type_ID { get; set; }
        public string Voucher_Type_Code { get; set; } = string.Empty;
        public string Voucher_Type_Name_AR { get; set; } = string.Empty;
        public string? Voucher_Type_Name_EN { get; set; }
        public int Sort_Order { get; set; }
        public bool Is_Active { get; set; } = true;
    }

    public sealed class VoucherStatusDto
    {
        public int Voucher_Status_ID { get; set; }
        public string Voucher_Status_Code { get; set; } = string.Empty;
        public string Voucher_Status_Name_AR { get; set; } = string.Empty;
        public string? Voucher_Status_Name_EN { get; set; }
        public int Sort_Order { get; set; }
        public bool Is_Active { get; set; } = true;
    }
}
