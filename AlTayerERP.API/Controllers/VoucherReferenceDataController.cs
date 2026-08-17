using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities.Accounting;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>
    /// مركز البيانات المرجعية المالية: طرق السداد وأنواع السندات وحالاتها.
    /// كل تعديل محصور بمدير النظام ومسجل في Audit_Logs؛ لا توجد عملية حذف فعلي.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public sealed class VoucherReferenceDataController : ControllerBase
    {
        private static readonly HashSet<string> ProtectedStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "DRAFT", "PENDING", "APPROVED", "POSTED", "CANCELLED", "REVERSED"
        };

        private readonly AppDbContext _context;
        private readonly AuditTrailService _audit;

        public VoucherReferenceDataController(AppDbContext context, AuditTrailService audit)
        {
            _context = context;
            _audit = audit;
        }

        private bool IsSystemAdmin(out ServerSession session)
        {
            session = HttpContext.Items["ServerSession"] as ServerSession ?? default!;
            return session != null && session.Is_System_Admin;
        }

        [HttpGet("PaymentMethods")]
        public async Task<IActionResult> GetPaymentMethods()
        {
            if (!IsSystemAdmin(out _)) return Forbid();
            return Ok(await _context.Payment_Methods.AsNoTracking()
                .OrderBy(x => x.Sort_Order).ThenBy(x => x.Payment_Method_Name_AR).ToListAsync());
        }

        [HttpPost("PaymentMethods")]
        public async Task<IActionResult> SavePaymentMethod([FromBody] PaymentMethodDto dto)
        {
            if (!IsSystemAdmin(out var session)) return Forbid();
            if (dto == null || string.IsNullOrWhiteSpace(dto.Payment_Method_Code) ||
                string.IsNullOrWhiteSpace(dto.Payment_Method_Name_AR))
                return BadRequest("كود وطريقة السداد بالعربية مطلوبان.");
            if (dto.Requires_Reference_Date && !dto.Requires_Reference)
                return BadRequest("تاريخ المرجع لا يستخدم دون إلزام رقم المرجع.");

            var code = dto.Payment_Method_Code.Trim().ToUpperInvariant();
            if (await _context.Payment_Methods.AnyAsync(x => x.Payment_Method_ID != dto.Payment_Method_ID && x.Payment_Method_Code == code))
                return Conflict("كود طريقة السداد مستخدم مسبقاً.");

            var row = dto.Payment_Method_ID > 0
                ? await _context.Payment_Methods.FindAsync(dto.Payment_Method_ID)
                : null;
            var before = row == null ? null : new
            {
                row.Payment_Method_Code, row.Payment_Method_Name_AR, row.Requires_Reference,
                row.Requires_Reference_Date, row.Is_Cash, row.Is_Bank, row.Is_Active
            };
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
            row.Sort_Order = Math.Max(0, dto.Sort_Order);
            row.Is_Active = dto.Is_Active;

            _audit.Add(session, HttpContext, "payment_methods",
                dto.Payment_Method_ID == 0 ? "new" : dto.Payment_Method_ID.ToString(),
                dto.Payment_Method_ID == 0 ? "CREATE" : "UPDATE", before,
                new { row.Payment_Method_Code, row.Payment_Method_Name_AR, row.Is_Active });
            await _context.SaveChangesAsync();
            return Ok(row);
        }

        [HttpGet("VoucherTypes")]
        public async Task<IActionResult> GetVoucherTypes()
        {
            if (!IsSystemAdmin(out _)) return Forbid();
            return Ok(await _context.Voucher_Types.AsNoTracking()
                .OrderBy(x => x.Sort_Order).ThenBy(x => x.Voucher_Type_Name_AR).ToListAsync());
        }

        [HttpPost("VoucherTypes")]
        public async Task<IActionResult> SaveVoucherType([FromBody] VoucherTypeDto dto)
        {
            if (!IsSystemAdmin(out var session)) return Forbid();
            if (dto == null || string.IsNullOrWhiteSpace(dto.Voucher_Type_Code) ||
                string.IsNullOrWhiteSpace(dto.Voucher_Type_Name_AR))
                return BadRequest("كود ونوع السند بالعربية مطلوبان.");

            var code = dto.Voucher_Type_Code.Trim().ToUpperInvariant();
            if (await _context.Voucher_Types.AnyAsync(x => x.Voucher_Type_ID != dto.Voucher_Type_ID && x.Voucher_Type_Code == code))
                return Conflict("كود نوع السند مستخدم مسبقاً.");

            var row = dto.Voucher_Type_ID > 0 ? await _context.Voucher_Types.FindAsync(dto.Voucher_Type_ID) : null;
            var before = row == null ? null : new { row.Voucher_Type_Code, row.Voucher_Type_Name_AR, row.Is_Active };
            if (row == null)
            {
                row = new VoucherType();
                _context.Voucher_Types.Add(row);
            }

            row.Voucher_Type_Code = code;
            row.Voucher_Type_Name_AR = dto.Voucher_Type_Name_AR.Trim();
            row.Voucher_Type_Name_EN = NullIfWhiteSpace(dto.Voucher_Type_Name_EN);
            row.Sort_Order = Math.Max(0, dto.Sort_Order);
            row.Is_Active = dto.Is_Active;

            _audit.Add(session, HttpContext, "voucher_types",
                dto.Voucher_Type_ID == 0 ? "new" : dto.Voucher_Type_ID.ToString(),
                dto.Voucher_Type_ID == 0 ? "CREATE" : "UPDATE", before,
                new { row.Voucher_Type_Code, row.Voucher_Type_Name_AR, row.Is_Active });
            await _context.SaveChangesAsync();
            return Ok(row);
        }

        [HttpGet("VoucherStatuses")]
        public async Task<IActionResult> GetVoucherStatuses()
        {
            if (!IsSystemAdmin(out _)) return Forbid();
            return Ok(await _context.Voucher_Statuses.AsNoTracking()
                .OrderBy(x => x.Sort_Order).ThenBy(x => x.Voucher_Status_Name_AR).ToListAsync());
        }

        [HttpPost("VoucherStatuses")]
        public async Task<IActionResult> SaveVoucherStatus([FromBody] VoucherStatusDto dto)
        {
            if (!IsSystemAdmin(out var session)) return Forbid();
            if (dto == null || string.IsNullOrWhiteSpace(dto.Voucher_Status_Code) ||
                string.IsNullOrWhiteSpace(dto.Voucher_Status_Name_AR))
                return BadRequest("كود وحالة السند بالعربية مطلوبان.");

            var code = dto.Voucher_Status_Code.Trim().ToUpperInvariant();
            if (await _context.Voucher_Statuses.AnyAsync(x => x.Voucher_Status_ID != dto.Voucher_Status_ID && x.Voucher_Status_Code == code))
                return Conflict("كود حالة السند مستخدم مسبقاً.");

            var row = dto.Voucher_Status_ID > 0 ? await _context.Voucher_Statuses.FindAsync(dto.Voucher_Status_ID) : null;
            if (row != null && ProtectedStatuses.Contains(row.Voucher_Status_Code) &&
                (!dto.Is_Active || !string.Equals(row.Voucher_Status_Code, code, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest("لا يمكن تعطيل أو تغيير كود حالة دورة المستند الأساسية.");
            }

            var before = row == null ? null : new { row.Voucher_Status_Code, row.Voucher_Status_Name_AR, row.Is_Active };
            if (row == null)
            {
                row = new VoucherStatus();
                _context.Voucher_Statuses.Add(row);
            }

            row.Voucher_Status_Code = code;
            row.Voucher_Status_Name_AR = dto.Voucher_Status_Name_AR.Trim();
            row.Voucher_Status_Name_EN = NullIfWhiteSpace(dto.Voucher_Status_Name_EN);
            row.Sort_Order = Math.Max(0, dto.Sort_Order);
            row.Is_Active = dto.Is_Active;

            _audit.Add(session, HttpContext, "voucher_statuses",
                dto.Voucher_Status_ID == 0 ? "new" : dto.Voucher_Status_ID.ToString(),
                dto.Voucher_Status_ID == 0 ? "CREATE" : "UPDATE", before,
                new { row.Voucher_Status_Code, row.Voucher_Status_Name_AR, row.Is_Active });
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
