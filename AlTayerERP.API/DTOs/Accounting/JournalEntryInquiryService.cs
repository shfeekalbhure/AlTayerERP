using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlTayerERP.API.Services.Accounting
{
    /// <summary>
    /// خدمة استعلام واستعراض القيود المحاسبية الناتجة عن السندات المالية.
    ///
    /// تقوم الخدمة بالآتي:
    /// 1- البحث عن السند بواسطة رقم السند.
    /// 2- جلب بيانات رأس السند.
    /// 3- جلب جميع سطور القيد المحاسبي.
    /// 4- جلب أسماء الحسابات والعملات ومراكز التكلفة.
    /// 5- حساب إجمالي المدين والدائن بالعملة المحلية.
    /// </summary>
    public class JournalEntryInquiryService
    {
        #region المتغيرات

        private readonly AppDbContext _context;

        #endregion

        #region المشيد

        public JournalEntryInquiryService(AppDbContext context)
        {
            _context = context;
        }

        #endregion

        #region نماذج نتيجة الاستعلام

        /// <summary>
        /// يمثل النتيجة الكاملة للقيد المحاسبي.
        /// </summary>
        public sealed class JournalEntryInquiryResult
        {
            public long Voucher_ID { get; set; }

            public string Voucher_No { get; set; } = string.Empty;

            public int Voucher_Type_ID { get; set; }

            public string Voucher_Type_Name { get; set; } = string.Empty;

            public int Voucher_Status_ID { get; set; }

            public string Voucher_Status_Name { get; set; } = string.Empty;

            public string Branch_ID { get; set; } = string.Empty;

            public int? Fiscal_Year_ID { get; set; }

            public DateTime Voucher_Date { get; set; }

            public DateTime Transaction_Date { get; set; }

            public string Cash_Account_ID { get; set; } = string.Empty;

            public string? Party_ID { get; set; }

            public string? Party_Name { get; set; }

            public int Currency_ID { get; set; }

            public string Currency_Code { get; set; } = string.Empty;

            public string Currency_Name { get; set; } = string.Empty;

            public decimal Exchange_Rate { get; set; }

            public decimal Foreign_Total { get; set; }

            public decimal Local_Total { get; set; }

            public string? Reference_No { get; set; }

            public DateTime? Reference_Date { get; set; }

            public string? Against_Text { get; set; }

            public string? Description { get; set; }

            public string? Notes { get; set; }

            public bool Is_Posted { get; set; }

            public long? Journal_Entry_ID { get; set; }

            public string? Posted_By_User_ID { get; set; }

            public DateTime? Posted_At { get; set; }

            public int Edit_Count { get; set; }

            public int Print_Count { get; set; }

            public bool Is_Active { get; set; }

            public string? Created_By { get; set; }

            public DateTime Created_At { get; set; }

            public string? Updated_By { get; set; }

            public DateTime? Updated_At { get; set; }

            public decimal Total_Debit { get; set; }

            public decimal Total_Credit { get; set; }

            public decimal Difference { get; set; }

            public bool Is_Balanced { get; set; }

            public List<JournalEntryLineResult> Details { get; set; }
                = new List<JournalEntryLineResult>();
        }

        /// <summary>
        /// يمثل سطرًا واحدًا داخل القيد المحاسبي.
        /// </summary>
        public sealed class JournalEntryLineResult
        {
            public long Voucher_Detail_ID { get; set; }

            public int Line_No { get; set; }

            public string Account_ID { get; set; } = string.Empty;

            public string Account_Code { get; set; } = string.Empty;

            public string Account_Name { get; set; } = string.Empty;

            public decimal Debit_Amount { get; set; }

            public decimal Credit_Amount { get; set; }

            public int Currency_ID { get; set; }

            public string Currency_Code { get; set; } = string.Empty;

            public string Currency_Name { get; set; } = string.Empty;

            public decimal Exchange_Rate { get; set; }

            public decimal Foreign_Amount { get; set; }

            public decimal Local_Amount { get; set; }

            public decimal Local_Debit { get; set; }

            public decimal Local_Credit { get; set; }

            public string? Cost_Center_ID { get; set; }

            public string Cost_Center_Name { get; set; } = string.Empty;

            public string? Project_ID { get; set; }

            public string? Description { get; set; }

            public string? Reference_Type { get; set; }

            public string? Reference_No { get; set; }

            public string? Reference_Name { get; set; }

            public DateTime? Reference_Date { get; set; }

            public byte Line_Type { get; set; }

            public string Line_Type_Name { get; set; } = string.Empty;

            public string? Notes { get; set; }
        }

        /// <summary>
        /// يمثل نتيجة مختصرة عند البحث عن عدة سندات.
        /// </summary>
        public sealed class JournalEntrySearchResult
        {
            public long Voucher_ID { get; set; }

            public string Voucher_No { get; set; } = string.Empty;

            public string Voucher_Type_Name { get; set; } = string.Empty;

            public DateTime Voucher_Date { get; set; }

            public string Branch_ID { get; set; } = string.Empty;

            public string? Description { get; set; }

            public decimal Local_Total { get; set; }

            public bool Is_Posted { get; set; }

            public long? Journal_Entry_ID { get; set; }
        }

        #endregion

        #region جلب قيد بواسطة رقم السند

        /// <summary>
        /// يجلب القيد المحاسبي بواسطة رقم السند.
        /// </summary>
        public async Task<JournalEntryInquiryResult?> GetByVoucherNoAsync(
            string voucherNo,
            string? branchId = null,
            int? voucherTypeId = null)
        {
            if (string.IsNullOrWhiteSpace(voucherNo))
            {
                return null;
            }

            voucherNo = voucherNo.Trim();

            var headerQuery = _context.Financial_Voucher_Headers
                .AsNoTracking()
                .Where(x =>
                    x.Voucher_No == voucherNo &&
                    x.Is_Active);

            if (!string.IsNullOrWhiteSpace(branchId))
            {
                headerQuery = headerQuery.Where(
                    x => x.Branch_ID == branchId);
            }

            if (voucherTypeId.HasValue)
            {
                headerQuery = headerQuery.Where(
                    x => x.Voucher_Type_ID == voucherTypeId.Value);
            }

            var header = await headerQuery
                .OrderByDescending(x => x.Voucher_Date)
                .ThenByDescending(x => x.Voucher_ID)
                .FirstOrDefaultAsync();

            if (header == null)
            {
                return null;
            }

            return await BuildResultAsync(header.Voucher_ID);
        }

        #endregion

        #region جلب قيد بواسطة معرف السند

        /// <summary>
        /// يجلب القيد المحاسبي بواسطة معرف السند.
        /// </summary>
        public async Task<JournalEntryInquiryResult?> GetByVoucherIdAsync(
            long voucherId)
        {
            if (voucherId <= 0)
            {
                return null;
            }

            return await BuildResultAsync(voucherId);
        }

        #endregion

        #region جلب قيد بواسطة معرف القيد المحاسبي

        /// <summary>
        /// يجلب السند المرتبط بمعرف القيد المحاسبي.
        /// </summary>
        public async Task<JournalEntryInquiryResult?> GetByJournalEntryIdAsync(
            long journalEntryId)
        {
            if (journalEntryId <= 0)
            {
                return null;
            }

            var voucherId = await _context.Financial_Voucher_Headers
                .AsNoTracking()
                .Where(x =>
                    x.Journal_Entry_ID == journalEntryId &&
                    x.Is_Active)
                .Select(x => (long?)x.Voucher_ID)
                .FirstOrDefaultAsync();

            if (!voucherId.HasValue)
            {
                return null;
            }

            return await BuildResultAsync(voucherId.Value);
        }

        #endregion

        #region البحث العام

        /// <summary>
        /// البحث عن السندات والقيود حسب مجموعة من الخيارات.
        /// </summary>
        public async Task<List<JournalEntrySearchResult>> SearchAsync(
            string? voucherNo = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? branchId = null,
            int? voucherTypeId = null,
            bool postedOnly = false)
        {
            var query = _context.Financial_Voucher_Headers
                .AsNoTracking()
                .Where(x => x.Is_Active);

            if (!string.IsNullOrWhiteSpace(voucherNo))
            {
                string searchText = voucherNo.Trim();

                query = query.Where(
                    x => x.Voucher_No.Contains(searchText));
            }

            if (fromDate.HasValue)
            {
                DateTime startDate = fromDate.Value.Date;

                query = query.Where(
                    x => x.Voucher_Date >= startDate);
            }

            if (toDate.HasValue)
            {
                DateTime endDate = toDate.Value.Date.AddDays(1);

                query = query.Where(
                    x => x.Voucher_Date < endDate);
            }

            if (!string.IsNullOrWhiteSpace(branchId))
            {
                query = query.Where(
                    x => x.Branch_ID == branchId);
            }

            if (voucherTypeId.HasValue)
            {
                query = query.Where(
                    x => x.Voucher_Type_ID == voucherTypeId.Value);
            }

            if (postedOnly)
            {
                query = query.Where(x => x.Is_Posted);
            }

            var result = await (
                from header in query

                join voucherType in _context.Voucher_Types.AsNoTracking()
                    on header.Voucher_Type_ID equals
                    voucherType.Voucher_Type_ID into voucherTypeGroup

                from voucherType in voucherTypeGroup.DefaultIfEmpty()

                orderby header.Voucher_Date descending,
                        header.Voucher_ID descending

                select new JournalEntrySearchResult
                {
                    Voucher_ID = header.Voucher_ID,
                    Voucher_No = header.Voucher_No,

                    Voucher_Type_Name =
                        voucherType != null
                            ? voucherType.Voucher_Type_Name_AR
                            : string.Empty,

                    Voucher_Date = header.Voucher_Date,
                    Branch_ID = header.Branch_ID,
                    Description = header.Description,
                    Local_Total = header.Local_Total,
                    Is_Posted = header.Is_Posted,
                    Journal_Entry_ID = header.Journal_Entry_ID
                })
                .Take(500)
                .ToListAsync();

            return result;
        }

        #endregion

        #region بناء النتيجة الكاملة

        /// <summary>
        /// يبني بيانات رأس القيد وتفاصيله.
        /// </summary>
        private async Task<JournalEntryInquiryResult?> BuildResultAsync(
            long voucherId)
        {
            var header = await (
                from voucher in _context.Financial_Voucher_Headers
                    .AsNoTracking()

                join voucherType in _context.Voucher_Types.AsNoTracking()
                    on voucher.Voucher_Type_ID equals
                    voucherType.Voucher_Type_ID into voucherTypeGroup

                from voucherType in voucherTypeGroup.DefaultIfEmpty()

                join voucherStatus in _context.Voucher_Statuses.AsNoTracking()
                    on voucher.Voucher_Status_ID equals
                    voucherStatus.Voucher_Status_ID into statusGroup

                from voucherStatus in statusGroup.DefaultIfEmpty()

                join currency in _context.Currencies.AsNoTracking()
                    on voucher.Currency_ID equals
                    currency.Currency_ID into currencyGroup

                from currency in currencyGroup.DefaultIfEmpty()

                join party in _context.Parties.AsNoTracking()
                    on voucher.Party_ID equals
                    party.Party_ID into partyGroup

                from party in partyGroup.DefaultIfEmpty()

                where voucher.Voucher_ID == voucherId &&
                      voucher.Is_Active

                select new JournalEntryInquiryResult
                {
                    Voucher_ID = voucher.Voucher_ID,
                    Voucher_No = voucher.Voucher_No,
                    Voucher_Type_ID = voucher.Voucher_Type_ID,

                    Voucher_Type_Name =
                        voucherType != null
                            ? voucherType.Voucher_Type_Name_AR
                            : string.Empty,

                    Voucher_Status_ID = voucher.Voucher_Status_ID,

                    Voucher_Status_Name =
                        voucherStatus != null
                            ? voucherStatus.Voucher_Status_Name_AR
                            : string.Empty,

                    Branch_ID = voucher.Branch_ID,
                    Fiscal_Year_ID = voucher.Fiscal_Year_ID,
                    Voucher_Date = voucher.Voucher_Date,
                    Transaction_Date = voucher.Transaction_Date,
                    Cash_Account_ID = voucher.Cash_Account_ID,
                    Party_ID = voucher.Party_ID,

                    Party_Name =
                        party != null
                            ? party.Party_Name_AR
                            : string.Empty,

                    Currency_ID = voucher.Currency_ID,

                    Currency_Code =
                        currency != null
                            ? currency.Currency_Code
                            : string.Empty,

                    Currency_Name =
                        currency != null
                            ? currency.Currency_Name_AR
                            : string.Empty,

                    Exchange_Rate = voucher.Exchange_Rate,
                    Foreign_Total = voucher.Foreign_Total,
                    Local_Total = voucher.Local_Total,
                    Reference_No = voucher.Reference_No,
                    Reference_Date = voucher.Reference_Date,
                    Against_Text = voucher.Against_Text,
                    Description = voucher.Description,
                    Notes = voucher.Notes,
                    Is_Posted = voucher.Is_Posted,
                    Journal_Entry_ID = voucher.Journal_Entry_ID,
                    Posted_By_User_ID = voucher.Posted_By_User_ID,
                    Posted_At = voucher.Posted_At,
                    Edit_Count = voucher.Edit_Count,
                    Print_Count = voucher.Print_Count,
                    Is_Active = voucher.Is_Active,
                    Created_By = voucher.Created_By,
                    Created_At = voucher.Created_At,
                    Updated_By = voucher.Updated_By,
                    Updated_At = voucher.Updated_At
                })
                .FirstOrDefaultAsync();

            if (header == null)
            {
                return null;
            }

            header.Details = await (
                from detail in _context.Financial_Voucher_Details
                    .AsNoTracking()

                join account in _context.Chart_Of_Accounts.AsNoTracking()
                    on detail.Account_ID equals
                    account.Account_ID into accountGroup

                from account in accountGroup.DefaultIfEmpty()

                join currency in _context.Currencies.AsNoTracking()
                    on detail.Currency_ID equals
                    currency.Currency_ID into currencyGroup

                from currency in currencyGroup.DefaultIfEmpty()

                join costCenter in _context.Cost_Centers.AsNoTracking()
                    on detail.Cost_Center_ID equals
                    costCenter.Cost_Center_ID into costCenterGroup

                from costCenter in costCenterGroup.DefaultIfEmpty()

                where detail.Voucher_ID == voucherId

                orderby detail.Line_No

                select new JournalEntryLineResult
                {
                    Voucher_Detail_ID = detail.Voucher_Detail_ID,
                    Line_No = detail.Line_No,
                    Account_ID = detail.Account_ID,

                    Account_Code =
                        account != null
                            ? account.Account_Code
                            : detail.Account_ID,

                    Account_Name =
                        account != null
                            ? account.Account_Name_AR
                            : string.Empty,

                    Debit_Amount = detail.Debit_Amount,
                    Credit_Amount = detail.Credit_Amount,
                    Currency_ID = detail.Currency_ID,

                    Currency_Code =
                        currency != null
                            ? currency.Currency_Code
                            : string.Empty,

                    Currency_Name =
                        currency != null
                            ? currency.Currency_Name_AR
                            : string.Empty,

                    Exchange_Rate = detail.Exchange_Rate,
                    Foreign_Amount = detail.Foreign_Amount,
                    Local_Amount = detail.Local_Amount,

                    Local_Debit =
                        detail.Debit_Amount > 0
                            ? detail.Local_Amount
                            : 0,

                    Local_Credit =
                        detail.Credit_Amount > 0
                            ? detail.Local_Amount
                            : 0,

                    Cost_Center_ID = detail.Cost_Center_ID,

                    Cost_Center_Name =
                        costCenter != null
                            ? costCenter.Center_Name_AR
                            : string.Empty,

                    Project_ID = detail.Project_ID,
                    Description = detail.Description,
                    Reference_Type = detail.Reference_Type,
                    Reference_No = detail.Reference_No,
                    Reference_Name = detail.Reference_Name,
                    Reference_Date = detail.Reference_Date,
                    Line_Type = detail.Line_Type,

                    Line_Type_Name =
                        detail.Line_Type == 1 ? "صندوق أو بنك" :
                        detail.Line_Type == 2 ? "حساب مقابل" :
                        detail.Line_Type == 3 ? "ضريبة" :
                        detail.Line_Type == 4 ? "خصم" :
                        detail.Line_Type == 5 ? "فرق عملة" :
                        detail.Line_Type == 6 ? "تلقائي" :
                        detail.Line_Type == 7 ? "تسوية" :
                        "غير محدد",

                    Notes = detail.Notes
                })
                .ToListAsync();

            header.Total_Debit =
                header.Details.Sum(x => x.Local_Debit);

            header.Total_Credit =
                header.Details.Sum(x => x.Local_Credit);

            header.Difference =
                header.Total_Debit - header.Total_Credit;

            header.Is_Balanced =
                Math.Abs(header.Difference) < 0.01m;

            return header;
        }

        #endregion
    }
}