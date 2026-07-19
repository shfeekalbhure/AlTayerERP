using AlTayerERP.API.DTOs;
using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlTayerERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CashBoxesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AccountNumberService _accountNumberService;
        private readonly CashBoxNumberService _cashBoxNumberService;

        public CashBoxesController(
            AppDbContext context,
            AccountNumberService accountNumberService,
            CashBoxNumberService cashBoxNumberService)
        {
            _context = context;
            _accountNumberService = accountNumberService;
            _cashBoxNumberService = cashBoxNumberService;
        }

        //====================================
        // جلب جميع الصناديق مع دمج اسم الحساب المالي
        //====================================
        [HttpGet]
        public async Task<IActionResult> GetCashBoxes([FromQuery] string companyId)
        {
            if (string.IsNullOrWhiteSpace(companyId))
                return BadRequest("رقم الشركة مطلوب.");

            try
            {
                var data = await (from box in _context.Cash_Boxes
                                  join acc in _context.Chart_Of_Accounts
                                  on box.Account_ID equals acc.Account_ID into accJoin
                                  from subAcc in accJoin.DefaultIfEmpty()
                                  where box.Company_ID == companyId
                                  orderby box.CashBox_Code
                                  select new
                                  {
                                      box.Cash_Box_ID,
                                      box.Company_ID,
                                      box.Branch_ID,
                                      box.Account_ID,
                                      Account_Name_AR = subAcc != null ? subAcc.Account_Name_AR : "غير مرتبط",
                                      box.Currency_Code,
                                      Code = box.CashBox_Code,
                                      NameAR = box.Box_Name_AR,
                                      NameEN = box.Box_Name_EN,
                                      box.Opening_Balance,
                                      box.Max_Limit,
                                      box.Min_Limit,
                                      box.Is_Active,
                                      box.Notes
                                  }).AsNoTracking().ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        //====================================
        // جلب صندوق واحد
        //====================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCashBox(string id)
        {
            var item = await _context.Cash_Boxes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Cash_Box_ID == id);

            if (item == null)
                return NotFound("الصندوق غير موجود.");

            return Ok(item);
        }



         [HttpGet("GetNextCode")]
        public async Task<IActionResult> GetNextCode([FromQuery] string companyId)
        {
            if (string.IsNullOrWhiteSpace(companyId))
                return BadRequest("رقم الشركة مطلوب.");

            string code = await _cashBoxNumberService.GenerateCashBoxCodeAsync(companyId.Trim());

            return Ok(code);
        }



        //====================================
        // إضافة صندوق جديد (تأمين الترتيب والتوقيت)
        //====================================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCashBoxDto dto)
        {
            if (dto == null)
                return BadRequest("البيانات المرسلة فارغة.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. جلب الحساب الأب للتأكد من وجوده واحتساب المستوى
                var parentAccount = await _context.Chart_Of_Accounts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Account_ID == dto.Account_ID && x.Company_ID == dto.Company_ID.Trim());

                if (parentAccount == null)
                    return BadRequest("حساب الصناديق الرئيسي المختار غير موجود في دليل الحسابات.");

                int accountLevel = parentAccount.Account_Level + 1;
                var currentUtcTime = DateTime.UtcNow; // توحيد وقت الإنشاء للجدولين

                // 2. بناء الحساب المالي الجديد في الدليل
                var account = new ChartOfAccount
                {
                    Account_ID = Guid.NewGuid().ToString(),
                    Company_ID = dto.Company_ID.Trim(),
                    Parent_Account_ID = dto.Account_ID,
                    Account_Code = await _accountNumberService.GenerateAccountCodeAsync(dto.Company_ID.Trim(), dto.Account_ID),
                    Account_Name_AR = dto.Box_Name_AR.Trim(),
                    Account_Name_EN = dto.Box_Name_EN?.Trim(),
                    Account_Type = "Asset",
                    Account_Category = "Cash",
                    Normal_Balance = "Debit",
                    Account_Level = accountLevel,
                    Account_Serial = 1,
                    Is_Postable = true,
                    Currency_Code = dto.Currency_Code,
                    Is_Active = dto.Is_Active,
                    Allow_ManualEntry = false, // تم تعديلها لـ false لكي لا يتم التلاعب بالصندوق يدوياً من قيود اليومية العامة
                    System_Account = false,
                    Requires_CostCenter = false,
                    Requires_Party = false,
                    Requires_Project = false,
                    Created_By = dto.Created_By,
                    Created_At = currentUtcTime
                };

                await _context.Chart_Of_Accounts.AddAsync(account);

                // حفظ الحساب أولاً لحل مشكلة قيد المفتاح الخارجي (FK Constraint)
                await _context.SaveChangesAsync();

                // 3. توليد كود الصندوق تلقائياً
                string generatedBoxCode = await _cashBoxNumberService.GenerateCashBoxCodeAsync(dto.Company_ID.Trim());

                // 4. بناء كائن الصندوق وربطه بالـ Account_ID المحفوظ
                var item = new CashBox
                {
                    Cash_Box_ID = Guid.NewGuid().ToString(),
                    Company_ID = dto.Company_ID.Trim(),
                    Branch_ID = dto.Branch_ID,
                    Account_ID = account.Account_ID,
                    Currency_Code = dto.Currency_Code,
                    CashBox_Code = generatedBoxCode,
                    Box_Name_AR = dto.Box_Name_AR.Trim(),
                    Box_Name_EN = dto.Box_Name_EN?.Trim(),
                    Opening_Balance = dto.Opening_Balance,
                    Max_Limit = dto.Max_Limit,
                    Min_Limit = dto.Min_Limit,
                    Is_Active = dto.Is_Active,
                    Notes = dto.Notes,
                    Created_By = dto.Created_By,
                    Created_At = currentUtcTime
                };

                await _context.Cash_Boxes.AddAsync(item);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(item);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        //====================================
        // تعديل بيانات الصندوق والحساب المرتبط بأمان
        //====================================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] CreateCashBoxDto dto)
        {
            if (dto == null)
                return BadRequest("البيانات فارغة.");

            var item = await _context.Cash_Boxes
                .FirstOrDefaultAsync(x => x.Cash_Box_ID == id);

            if (item == null)
                return NotFound("الصندوق غير موجود.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var currentUtcTime = DateTime.UtcNow;

                // 1. تحديث الحساب المالي المرتبط تلقائياً في دليل الحسابات
                var account = await _context.Chart_Of_Accounts
                    .FirstOrDefaultAsync(x => x.Account_ID == item.Account_ID && x.Company_ID == item.Company_ID);

                if (account != null)
                {
                    account.Account_Name_AR = dto.Box_Name_AR.Trim();
                    account.Account_Name_EN = dto.Box_Name_EN?.Trim();
                    account.Currency_Code = dto.Currency_Code;
                    account.Is_Active = dto.Is_Active;
                    account.Updated_By = dto.Updated_By;
                    account.Updated_At = currentUtcTime;
                }

                // 2. تحديث بيانات الصندوق
                item.Branch_ID = dto.Branch_ID;
                item.Currency_Code = dto.Currency_Code;
                item.Box_Name_AR = dto.Box_Name_AR.Trim();
                item.Box_Name_EN = dto.Box_Name_EN?.Trim();
                item.Opening_Balance = dto.Opening_Balance;
                item.Max_Limit = dto.Max_Limit;
                item.Min_Limit = dto.Min_Limit;
                item.Is_Active = dto.Is_Active;
                item.Notes = dto.Notes;
                item.Updated_By = dto.Updated_By;
                item.Updated_At = currentUtcTime;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(item);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        //====================================
        // حذف الصندوق مع تأمين قيود الحركات المالية
        //====================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var item = await _context.Cash_Boxes
                .FirstOrDefaultAsync(x => x.Cash_Box_ID == id);

            if (item == null)
                return NotFound("الصندوق غير موجود.");

            // [حماية أساسية]: التحقق مما إذا كان الحساب قد سجل حركات مالية (سندات صرف/قبض/قيود) لمنع انهيار قاعدة البيانات
            // افترضنا هنا وجود جدول تفاصيل القيود Journal_Entry_Details كمثال، قم بتغييره حسب نظامك
    //        var hasTransactions = await _context.Set<JournalEntryDetail>()
   //             .AnyAsync(x => x.Account_ID == item.Account_ID);

    //        if (hasTransactions)
  //          {
   //             return BadRequest("لا يمكن حذف الصندوق لوجود حركات مالية مسجلة عليه. يمكنك إلغاء تفعيله بدلاً من ذلك.");
   //         }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var account = await _context.Chart_Of_Accounts
                    .FirstOrDefaultAsync(x => x.Account_ID == item.Account_ID && x.Company_ID == item.Company_ID);

                // حذف الصندوق أولاً
                _context.Cash_Boxes.Remove(item);

                // حذف الحساب من الدليل
                if (account != null)
                {
                    _context.Chart_Of_Accounts.Remove(account);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }
    }
}