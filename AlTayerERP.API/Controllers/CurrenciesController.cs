using AlTayerERP.API.DTOs;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AlTayerERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrenciesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CurrenciesController(AppDbContext context)
        {
            _context = context;
        }

        //==================================================
        // جلب جميع العملات الخاصة بالشركة
        //==================================================

        [HttpGet]
        public async Task<IActionResult> GetCurrencies(
            [FromQuery] string companyId)
        {
            if (string.IsNullOrWhiteSpace(companyId))
            {
                return BadRequest("رقم الشركة مطلوب.");
            }

            var data = await _context.Currencies
                .AsNoTracking()
                .Where(x => x.Company_ID == companyId)
                .OrderByDescending(x => x.Is_Local_Currency)
                .ThenBy(x => x.Currency_Code)
                .Select(x => new
                {
                    x.Currency_ID,
                    x.Company_ID,
                    x.Currency_Code,
                    x.Currency_Name_AR,
                    x.Currency_Name_EN,
                    x.Currency_Symbol,
                    x.Decimal_Places,
                    x.Exchange_Rate,
                    x.Min_Exchange_Rate,
                    x.Max_Exchange_Rate,
                    x.Is_Local_Currency,
                    x.Is_Default,
                    x.Is_Active,
                    x.Notes,
                    x.Created_By,
                    x.Created_At,
                    x.Updated_By,
                    x.Updated_At
                })
                .ToListAsync();

            return Ok(data);
        }

        //==================================================
        // جلب عملة واحدة
        //==================================================

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCurrencyById(int id)
        {
            var currency = await _context.Currencies
                .AsNoTracking()
                .Where(x => x.Currency_ID == id)
                .Select(x => new
                {
                    x.Currency_ID,
                    x.Company_ID,
                    x.Currency_Code,
                    x.Currency_Name_AR,
                    x.Currency_Name_EN,
                    x.Currency_Symbol,
                    x.Decimal_Places,
                    x.Exchange_Rate,
                    x.Min_Exchange_Rate,
                    x.Max_Exchange_Rate,
                    x.Is_Local_Currency,
                    x.Is_Default,
                    x.Is_Active,
                    x.Notes,
                    x.Created_By,
                    x.Created_At,
                    x.Updated_By,
                    x.Updated_At
                })
                .FirstOrDefaultAsync();

            if (currency == null)
            {
                return NotFound("العملة غير موجودة.");
            }

            return Ok(currency);
        }

        //==================================================
        // إضافة عملة جديدة
        //==================================================

        [HttpPost]
        public async Task<IActionResult> CreateCurrency(
            [FromBody] CreateCurrencyDto dto)
        {
            if (dto == null)
            {
                return BadRequest("بيانات العملة غير صحيحة.");
            }

            string? validationMessage = ValidateCurrencyDto(dto);

            if (validationMessage != null)
            {
                return BadRequest(validationMessage);
            }

            string companyId = dto.Company_ID.Trim();
            string currencyCode =
                dto.Currency_Code.Trim().ToUpperInvariant();

            bool codeExists = await _context.Currencies.AnyAsync(x =>
                x.Company_ID == companyId &&
                x.Currency_Code.ToUpper() == currencyCode);

            if (codeExists)
            {
                return BadRequest("كود العملة موجود مسبقًا.");
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                /*
                 * إذا كانت العملة الجديدة محلية،
                 * نلغي صفة العملة المحلية عن أي عملة سابقة.
                 */
                if (dto.Is_Local_Currency)
                {
                    var previousLocalCurrencies =
                        await _context.Currencies
                            .Where(x =>
                                x.Company_ID == companyId &&
                                x.Is_Local_Currency)
                            .ToListAsync();

                    foreach (Currency item in previousLocalCurrencies)
                    {
                        item.Is_Local_Currency = false;
                        item.Updated_By =
                            dto.Created_By ?? dto.Updated_By;

                        item.Updated_At = DateTime.Now;
                    }
                }

                /*
                 * إذا كانت هذه العملة هي العملة الافتراضية،
                 * نلغي صفة الافتراضية عن بقية العملات.
                 */
                if (dto.Is_Default)
                {
                    var previousDefaultCurrencies =
                        await _context.Currencies
                            .Where(x =>
                                x.Company_ID == companyId &&
                                x.Is_Default)
                            .ToListAsync();

                    foreach (Currency item in previousDefaultCurrencies)
                    {
                        item.Is_Default = false;
                        item.Updated_By =
                            dto.Created_By ?? dto.Updated_By;
                        item.Updated_At =
                            DateTime.Now;
                    }
                }

                Currency currency = new Currency
                {
                    Company_ID = companyId,

                    Currency_Code = currencyCode,

                    Currency_Name_AR =
                        dto.Currency_Name_AR.Trim(),

                    Currency_Name_EN =
                        dto.Currency_Name_EN?.Trim(),

                    Currency_Symbol =
                        dto.Currency_Symbol?.Trim(),

                    Decimal_Places =
                        dto.Decimal_Places,

                    Exchange_Rate =
                        dto.Is_Local_Currency
                            ? 1
                            : dto.Exchange_Rate,

                    Min_Exchange_Rate =
                        dto.Is_Local_Currency
                            ? 1
                            : dto.Min_Exchange_Rate,

                    Max_Exchange_Rate =
                        dto.Is_Local_Currency
                            ? 1
                            : dto.Max_Exchange_Rate,

                    Is_Local_Currency =
                        dto.Is_Local_Currency,

                    Is_Default =
                        dto.Is_Default,

                    Is_Active =
                        dto.Is_Active,

                    Notes =
                        dto.Notes?.Trim(),

                    Created_By =
                        dto.Created_By,

                    Created_At =
                        DateTime.Now
                };

                await _context.Currencies.AddAsync(currency);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    Message = "تم حفظ العملة بنجاح.",
                    Currency = currency
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return StatusCode(
                    500,
                    $"حدث خطأ أثناء حفظ العملة: {ex.Message}");
            }
        }

        //==================================================
        // تعديل عملة
        //==================================================

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCurrency(
            int id,
            [FromBody] CreateCurrencyDto dto)
        {
            if (dto == null)
            {
                return BadRequest("بيانات العملة غير صحيحة.");
            }

            string? validationMessage = ValidateCurrencyDto(dto);

            if (validationMessage != null)
            {
                return BadRequest(validationMessage);
            }

            Currency? currency =
                await _context.Currencies
                    .FirstOrDefaultAsync(
                        x => x.Currency_ID == id);

            if (currency == null)
            {
                return NotFound("العملة غير موجودة.");
            }

            string companyId = dto.Company_ID.Trim();
            string currencyCode =
                dto.Currency_Code.Trim().ToUpperInvariant();

            bool codeExists = await _context.Currencies.AnyAsync(x =>
                x.Currency_ID != id &&
                x.Company_ID == companyId &&
                x.Currency_Code.ToUpper() == currencyCode);

            if (codeExists)
            {
                return BadRequest(
                    "يوجد عملة أخرى تستخدم نفس الكود.");
            }

            /*
             * لا نسمح بإلغاء العملة المحلية مباشرة.
             * لتغييرها، يتم اختيار عملة أخرى كعملة محلية.
             */
            if (currency.Is_Local_Currency &&
                !dto.Is_Local_Currency)
            {
                return BadRequest(
                    "لا يمكن إلغاء العملة المحلية مباشرة. " +
                    "اختر عملة أخرى وحددها كعملة محلية، " +
                    "وسيتم استبدال العملة المحلية تلقائيًا.");
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                /*
                 * عند اختيار العملة الحالية كعملة محلية،
                 * نلغي صفة المحلية عن بقية العملات.
                 */
                if (dto.Is_Local_Currency)
                {
                    var otherLocalCurrencies =
                        await _context.Currencies
                            .Where(x =>
                                x.Company_ID == companyId &&
                                x.Currency_ID != id &&
                                x.Is_Local_Currency)
                            .ToListAsync();

                    foreach (Currency item in otherLocalCurrencies)
                    {
                        item.Is_Local_Currency = false;

                        item.Updated_By =
                            dto.Updated_By ??
                            dto.Created_By;

                        item.Updated_At =
                            DateTime.Now;
                    }
                }

                /*
                 * إذا كانت هذه العملة هي العملة الافتراضية،
                 * نلغي صفة الافتراضية عن بقية العملات.
                 */
                if (dto.Is_Default)
                {
                    var previousDefaultCurrencies =
                        await _context.Currencies
                            .Where(x =>
                                x.Company_ID == companyId &&
                                x.Currency_ID != id &&
                                x.Is_Default)
                            .ToListAsync();

                    foreach (Currency item in previousDefaultCurrencies)
                    {
                        item.Is_Default = false;
                        item.Updated_By =
                            dto.Updated_By ?? dto.Created_By;
                        item.Updated_At =
                            DateTime.Now;
                    }
                }

                currency.Company_ID =
                    companyId;

                currency.Currency_Code =
                    currencyCode;

                currency.Currency_Name_AR =
                    dto.Currency_Name_AR.Trim();

                currency.Currency_Name_EN =
                    dto.Currency_Name_EN?.Trim();

                currency.Currency_Symbol =
                    dto.Currency_Symbol?.Trim();

                currency.Decimal_Places =
                    dto.Decimal_Places;

                currency.Exchange_Rate =
                    dto.Is_Local_Currency
                        ? 1
                        : dto.Exchange_Rate;

                currency.Min_Exchange_Rate =
                    dto.Is_Local_Currency
                        ? 1
                        : dto.Min_Exchange_Rate;

                currency.Max_Exchange_Rate =
                    dto.Is_Local_Currency
                        ? 1
                        : dto.Max_Exchange_Rate;

                currency.Is_Local_Currency =
                    dto.Is_Local_Currency;

                currency.Is_Default =
                    dto.Is_Default;

                currency.Is_Active =
                    dto.Is_Active;

                currency.Notes =
                    dto.Notes?.Trim();

                currency.Updated_By =
                    dto.Updated_By ??
                    dto.Created_By;

                currency.Updated_At =
                    DateTime.Now;

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    Message = "تم تعديل العملة بنجاح.",
                    Currency = currency
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return StatusCode(
                    500,
                    $"حدث خطأ أثناء تعديل العملة: {ex.Message}");
            }
        }

        //==================================================
        // حذف عملة
        //==================================================

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCurrency(int id)
        {
            Currency? currency =
                await _context.Currencies
                    .FirstOrDefaultAsync(
                        x => x.Currency_ID == id);

            if (currency == null)
            {
                return NotFound("العملة غير موجودة.");
            }

            if (currency.Is_Local_Currency)
            {
                return BadRequest(
                    "لا يمكن حذف العملة المحلية. " +
                    "اختر عملة أخرى كعملة محلية أولًا.");
            }

            try
            {
                _context.Currencies.Remove(currency);

                await _context.SaveChangesAsync();

                return Ok("تم حذف العملة بنجاح.");
            }
            catch (DbUpdateException)
            {
                return BadRequest(
                    "لا يمكن حذف العملة لأنها مستخدمة في عمليات أو حسابات أخرى. " +
                    "يمكنك إيقافها بدلًا من حذفها.");
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    $"حدث خطأ أثناء حذف العملة: {ex.Message}");
            }
        }

        //==================================================
        // قائمة العملات النشطة للشاشات الأخرى
        //==================================================

        [HttpGet("GetLookup")]
        public async Task<IActionResult> GetLookup(
            [FromQuery] string companyId)
        {
            if (string.IsNullOrWhiteSpace(companyId))
            {
                return BadRequest("رقم الشركة مطلوب.");
            }

            var data = await _context.Currencies
                .AsNoTracking()
                .Where(x =>
                    x.Company_ID == companyId &&
                    x.Is_Active)
                .OrderByDescending(x => x.Is_Local_Currency)
                .ThenBy(x => x.Currency_Name_AR)
                .Select(x => new
                {
                    x.Currency_ID,
                    x.Currency_Code,
                    x.Currency_Name_AR,
                    x.Currency_Symbol,
                    x.Decimal_Places,
                    x.Exchange_Rate,
                    x.Min_Exchange_Rate,
                    x.Max_Exchange_Rate,
                    x.Is_Local_Currency,
                    x.Is_Default
                })
                .ToListAsync();

            return Ok(data);
        }

        //==================================================
        // جلب العملة المحلية للشركة
        //==================================================

        [HttpGet("GetLocalCurrency")]
        public async Task<IActionResult> GetLocalCurrency(
            [FromQuery] string companyId)
        {
            if (string.IsNullOrWhiteSpace(companyId))
            {
                return BadRequest("رقم الشركة مطلوب.");
            }

            var currency = await _context.Currencies
                .AsNoTracking()
                .Where(x =>
                    x.Company_ID == companyId &&
                    x.Is_Local_Currency &&
                    x.Is_Active)
                .Select(x => new
                {
                    x.Currency_ID,
                    x.Currency_Code,
                    x.Currency_Name_AR,
                    x.Currency_Symbol,
                    x.Decimal_Places,
                    x.Exchange_Rate
                })
                .FirstOrDefaultAsync();

            if (currency == null)
            {
                return NotFound(
                    "لم يتم تحديد العملة المحلية للشركة.");
            }

            return Ok(currency);
        }

        //==================================================
        // التحقق من بيانات العملة
        //==================================================

        private static string? ValidateCurrencyDto(
            CreateCurrencyDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Company_ID))
            {
                return "رقم الشركة مطلوب.";
            }

            if (string.IsNullOrWhiteSpace(dto.Currency_Code))
            {
                return "كود العملة مطلوب.";
            }

            if (string.IsNullOrWhiteSpace(dto.Currency_Name_AR))
            {
                return "اسم العملة العربي مطلوب.";
            }

            if (dto.Decimal_Places < 0 ||
                dto.Decimal_Places > 6)
            {
                return
                    "عدد المنازل العشرية يجب أن يكون بين 0 و6.";
            }

            /*
             * العملة المحلية تُحفظ تلقائيًا بسعر صرف 1،
             * لذلك لا نحتاج فحص الحدود المدخلة لها.
             */
            if (dto.Is_Local_Currency)
            {
                return null;
            }

            if (dto.Exchange_Rate <= 0)
            {
                return "سعر الصرف يجب أن يكون أكبر من صفر.";
            }

            if (dto.Min_Exchange_Rate <= 0)
            {
                return
                    "أقل سعر صرف يجب أن يكون أكبر من صفر.";
            }

            if (dto.Max_Exchange_Rate <= 0)
            {
                return
                    "أعلى سعر صرف يجب أن يكون أكبر من صفر.";
            }

            if (dto.Min_Exchange_Rate >
                dto.Max_Exchange_Rate)
            {
                return
                    "أقل سعر صرف لا يمكن أن يكون أكبر من أعلى سعر صرف.";
            }

            if (dto.Exchange_Rate <
                dto.Min_Exchange_Rate)
            {
                return
                    $"سعر الصرف أقل من الحد الأدنى المسموح " +
                    $"({dto.Min_Exchange_Rate:N6}).";
            }

            if (dto.Exchange_Rate >
                dto.Max_Exchange_Rate)
            {
                return
                    $"سعر الصرف يتجاوز الحد الأعلى المسموح " +
                    $"({dto.Max_Exchange_Rate:N6}).";
            }

            return null;
        }
    }
}