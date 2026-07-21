using AlTayerERP.API.DTOs.Accounting;
using AlTayerERP.API.Services;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>
    /// متحكم تحميل القوائم المساعدة الخاصة بالسندات المالية.
    /// يقوم بإرجاع جميع البيانات اللازمة لفتح شاشة سند القبض دفعة واحدة.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class FinancialVoucherLookupsController : ControllerBase
    {
        #region المتغيرات العامة

        /// <summary>
        /// سياق قاعدة البيانات الرئيسي.
        /// </summary>
        private readonly AppDbContext _context;

        #endregion

        #region المشيد

        /// <summary>
        /// إنشاء متحكم القوائم المساعدة وحقن سياق قاعدة البيانات.
        /// </summary>
        public FinancialVoucherLookupsController(
            AppDbContext context)
        {
            _context = context;
        }

        #endregion

        #region جلب جميع القوائم

        /// <summary>
        /// جلب جميع القوائم المطلوبة لشاشة السند المالي.
        /// </summary>
        /// <param name="companyId">
        /// معرف الشركة الحالية.
        /// </param>
        /// <param name="branchId">
        /// معرف الفرع الحالي.
        /// </param>
        /// <returns>
        /// الفروع وأنواع السندات والحالات والصناديق والعملات
        /// ومراكز التكلفة وطرق السداد والأطراف والحسابات.
        /// </returns>
        [HttpGet]
        public async Task<ActionResult<FinancialVoucherLookupsDto>>
            GetLookups(
                [FromQuery] string companyId,
                [FromQuery] int branchId)
        {
            #region التحقق من المعاملات

            // لا يسمح بتحميل قوائم فرع آخر بتغيير معاملات الرابط.
            if (HttpContext.Items["ServerSession"] is not ServerSession session)
                return Unauthorized("انتهت الجلسة أو أنها غير صالحة.");

            if (!string.Equals(companyId?.Trim(), session.Company_ID, StringComparison.Ordinal) ||
                branchId != session.Branch_ID)
            {
                return Forbid();
            }

            // يعتمد الاستعلام اللاحق دائماً على نطاق الجلسة الموثوق.
            companyId = session.Company_ID;
            branchId = session.Branch_ID;

            #endregion

            #region تحميل الفروع

            List<BranchLookupDto> branches =
                await _context.Tenant_Branches
                    .AsNoTracking()
                    .Where(x =>
                        x.Company_ID == companyId &&
                        x.Is_Active)
                    .OrderBy(x => x.Branch_Name)
                    .Select(x => new BranchLookupDto
                    {
                        Branch_ID = x.Branch_ID,
                        Branch_Name = x.Branch_Name
                    })
                    .ToListAsync();

            #endregion

            #region تحميل أنواع السندات

            List<VoucherTypeLookupDto> voucherTypes =
                await _context.Voucher_Types
                    .AsNoTracking()
                 
                    .Where(x => x.Is_Active)
                   
                    .OrderBy(x => x.Voucher_Type_Name_AR)
                    .Select(x => new VoucherTypeLookupDto
                    {
                        Voucher_Type_ID =
                            x.Voucher_Type_ID,

                        Voucher_Type_Code =
                            x.Voucher_Type_Code,

                        Voucher_Type_Name_AR =
                            x.Voucher_Type_Name_AR
                    })
                    .ToListAsync();

            #endregion

            #region تحميل حالات السندات

            List<VoucherStatusLookupDto> voucherStatuses =
                await _context.Voucher_Statuses
                    .AsNoTracking()
                    .Where(x => x.Is_Active)
                    .OrderBy(x => x.Voucher_Status_Name_AR)
                    .Select(x => new VoucherStatusLookupDto
                    {
                        Voucher_Status_ID =
                            x.Voucher_Status_ID,

                        Voucher_Status_Code =
                            x.Voucher_Status_Code,

                        Voucher_Status_Name_AR =
                            x.Voucher_Status_Name_AR
                    })
                    .ToListAsync();

            #endregion

            #region تحميل الصناديق

            List<CashBoxLookupDto> cashBoxes =
                await _context.Cash_Boxes
                    .AsNoTracking()
                    .Where(x =>
                        x.Company_ID == companyId &&
                        x.Branch_ID == branchId &&
                        x.Is_Active)
                  
                       



                    .OrderBy(x => x.Box_Name_AR)
                    .Select(x => new CashBoxLookupDto
                    {
                       Cash_Box_ID = x.Cash_Box_ID,
                       Cash_Box_Code = x.CashBox_Code,
                       Cash_Box_Name = x.Box_Name_AR,
                       Account_ID = x.Account_ID,
                          Branch_ID = x.Branch_ID
                        })

             
                    .ToListAsync();

            #endregion

            #region تحميل العملات

            List<CurrencyLookupDto> currencies =
                await _context.Currencies
                    .AsNoTracking()
                    .Where(x =>
                        x.Company_ID == companyId &&
                        x.Is_Active)
                    .OrderByDescending(x => x.Is_Default)
                    .ThenBy(x => x.Currency_Name_AR)
                    .Select(x => new CurrencyLookupDto
                    {
                        Currency_ID =
                            x.Currency_ID,

                        Currency_Code =
                            x.Currency_Code,

                        Currency_Name_AR =
                            x.Currency_Name_AR,

                        Exchange_Rate =
                            x.Exchange_Rate,

                        Is_Default =
                            x.Is_Default,

                        // هل هذه العملة هي العملة المحلية؟
                        Is_Local =
                            x.Is_Local_Currency
                    })
                    .ToListAsync();





            #endregion

            #region تحميل مراكز التكلفة

            List <CostCenterLookupDto> costCenters =
                await _context.Cost_Centers
                    .AsNoTracking()
                    .Where(x =>
                        x.Company_ID == companyId &&
                        x.Is_Active)
      //              .OrderBy(x => x.Cost_Center_Code)
                    .OrderBy(x => x.Center_Code)


                    .Select(x => new CostCenterLookupDto
                    {
                        Cost_Center_ID =
                            x.Cost_Center_ID,

            //            Cost_Center_Code =
            //                x.Cost_Center_Code,

             //           Cost_Center_Name_AR =
                //            x.Cost_Center_Name_AR
                        
                            
                            
                            Cost_Center_Code =
                          x.Center_Code,

                        Cost_Center_Name_AR =
                            x.Center_Name_AR




                    })
                    .ToListAsync();

            #endregion

            #region تحميل طرق السداد

            List<PaymentMethodLookupDto> paymentMethods =
                await _context.Payment_Methods
                    .AsNoTracking()
                    .Where(x => x.Is_Active)
                    .OrderBy(x => x.Payment_Method_Name_AR)
                    .Select(x => new PaymentMethodLookupDto
                    {
                        Payment_Method_ID =
                            x.Payment_Method_ID,

                        Payment_Method_Code =
                            x.Payment_Method_Code,

                        Payment_Method_Name_AR =
                            x.Payment_Method_Name_AR
                    })
                    .ToListAsync();

            #endregion

            #region تحميل الأطراف

            List<PartyLookupDto> parties =
                await _context.Parties
                    .AsNoTracking()
                    .Where(x =>
                        x.Company_ID == companyId &&
                        x.Is_Active)
                    .OrderBy(x => x.Party_Name_AR)
                    .Select(x => new PartyLookupDto
                    {
                        Party_ID =
                            x.Party_ID,

                        Party_Code =
                            x.Party_Code,

                        Party_Name_AR =
                            x.Party_Name_AR
                    })
                    .ToListAsync();

            #endregion

            #region تحميل الحسابات

            List<AccountLookupDto> accounts =
                await _context.Chart_Of_Accounts
                    .AsNoTracking()
                    .Where(x =>
                        x.Company_ID == companyId &&
                        x.Is_Active &&
                        x.Is_Postable)
                    .OrderBy(x => x.Account_Code)
                    .Select(x => new AccountLookupDto
                    {
                        Account_ID =
                            x.Account_ID,

                        Account_Code =
                            x.Account_Code,

                        Account_Name_AR =
                            x.Account_Name_AR
                    })
                    .ToListAsync();

            #endregion

            #region تجهيز النتيجة النهائية

            var result =
                new FinancialVoucherLookupsDto
                {
                    Branches =
                        branches,

                    VoucherTypes =
                        voucherTypes,

                    VoucherStatuses =
                        voucherStatuses,

                    CashBoxes =
                        cashBoxes,

                    Currencies =
                        currencies,

                    CostCenters =
                        costCenters,

                    PaymentMethods =
                        paymentMethods,

                    Parties =
                        parties,

                    Accounts =
                        accounts
                };

            return Ok(result);

            #endregion
        }

        #endregion
    }
}