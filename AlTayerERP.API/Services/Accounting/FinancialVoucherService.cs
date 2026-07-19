using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlTayerERP.API.DTOs.Accounting;
using AlTayerERP.Core.Entities.Accounting;
using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace AlTayerERP.API.Services.Accounting
{
    /// <summary>
    /// الخدمة الرئيسية لإدارة السندات المالية.
    /// جميع عمليات سند القبض وسند الصرف تمر من هنا.
    /// </summary>
    public class FinancialVoucherService
    {
        private readonly AppDbContext _context;
        private readonly VoucherValidationService _validator;

        public FinancialVoucherService(
            AppDbContext context,
            VoucherValidationService validator)
        {
            _context = context;
            _validator = validator;
        }

        /// <summary>
        /// إنشاء سند مالي جديد.
        /// </summary>
        public async Task<(bool Success, string Message, long? VoucherId, string? VoucherNo)> CreateAsync(CreateFinancialVoucherDto dto)
        {
            // 1) التحقق من صحة السند
            var validation = await _validator.ValidateAsync(dto);

            if (!validation.IsValid)
            {
                return (false, validation.ErrorMessage, null, null);
            }

            // 2) بدء معاملة قاعدة بيانات لضمان حفظ الرأس والتفاصيل والتوزيعات معًا
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 3) حساب الإجماليات من التفاصيل، ولا نعتمد على القيم القادمة من الشاشة
                decimal totalDebit = decimal.Round(dto.Details.Sum(x => x.Debit_Amount), 2);
                decimal totalCredit = decimal.Round(dto.Details.Sum(x => x.Credit_Amount), 2);

                if (totalDebit != totalCredit)
                {
                    await transaction.RollbackAsync();

                    return (false, "إجمالي المدين لا يساوي إجمالي الدائن.", null, null);
                }

                decimal localTotal = totalDebit;

                // [التعديل الثاني]: حساب إجمالي العملة الأجنبية بناءً على سطور النقدية/البنك فقط (Line_Type == 1) لمنع التضاعف
                var cashLines = dto.Details.Where(x => x.Line_Type == 1).ToList();
                var cashCurrencies = cashLines.Select(x => x.Currency_ID).Distinct().ToList();

                decimal foreignTotal = cashCurrencies.Count == 1
                    ? decimal.Round(cashLines.Sum(x => x.Foreign_Amount), 2)
                    : 0m;

                // 4) توليد رقم السند الرسمي من إعدادات الترقيم. 
                // يتم تحديث آخر رقم داخل نفس المعاملة، لذلك إذا فشل الحفظ يتم التراجع عن الرقم والسند معًا.
                string voucherNumber = await GenerateOfficialVoucherNoAsync(dto);

                // 5) إنشاء رأس السند
                var voucher = new FinancialVoucherHeader
                {
                    Voucher_No = voucherNumber,
                    // [التعديل الأول]: ترك الحالة معتمدة على الـ DTO ومستقلة تماماً عن حالة طلب الاعتماد (Approval_Status)
                    Voucher_Type_ID = dto.Voucher_Type_ID,
                    Voucher_Status_ID = dto.Voucher_Status_ID,
                    Branch_ID = dto.Branch_ID,
                    Fiscal_Year_ID = dto.Fiscal_Year_ID,
                    Voucher_Date = dto.Voucher_Date,
                    Transaction_Date = dto.Transaction_Date,
                    Cash_Account_ID = dto.Cash_Account_ID,
                    Party_ID = dto.Party_ID,
                    Payment_Method_ID = dto.Payment_Method_ID,
                    Currency_ID = dto.Currency_ID,
                    Exchange_Rate = dto.Exchange_Rate,
                    Foreign_Total = foreignTotal,
                    Local_Total = localTotal,
                    Reference_No = dto.Reference_No,
                    Reference_Date = dto.Reference_Date,
                    Against_Text = dto.Against_Text,
                    Description = dto.Description,
                    Notes = dto.Notes,
                    Module_ID = dto.Module_ID,
                    Document_Type_ID = dto.Document_Type_ID,
                    Document_ID = dto.Document_ID,
                    Source_Document_No = dto.Source_Document_No,
                    Requires_Approval = dto.Requires_Approval,
                    Approval_Status = dto.Requires_Approval ? (byte)1 : (byte)0,
                    Approval_Requested_By_User_ID = dto.Requires_Approval ? dto.Created_By : null,
                    Approval_Requested_At = dto.Requires_Approval ? DateTime.Now : null,
                    Is_Posted = false,
                    Is_Active = true,
                    Created_By = dto.Created_By,
                    Created_At = DateTime.Now
                };

                // 6) إضافة تفاصيل السند
                foreach (var detailDto in dto.Details.OrderBy(x => x.Line_No))
                {
                    voucher.Details.Add(new FinancialVoucherDetail
                    {
                        Line_No = detailDto.Line_No,
                        Account_ID = detailDto.Account_ID,
                        Description = detailDto.Description,
                        Cost_Center_ID = detailDto.Cost_Center_ID,
                        Project_ID = detailDto.Project_ID,

                        Reference_Type = detailDto.Reference_Type,
                        Reference_No = detailDto.Reference_No,
                        Reference_Name = detailDto.Reference_Name,
                        Reference_Date = detailDto.Reference_Date,
                        Currency_ID = detailDto.Currency_ID,
                        Exchange_Rate = detailDto.Exchange_Rate,
                        Foreign_Amount = detailDto.Foreign_Amount,
                        Local_Amount = detailDto.Local_Amount,
                        Debit_Amount = detailDto.Debit_Amount,
                        Credit_Amount = detailDto.Credit_Amount,
                        Line_Type = detailDto.Line_Type,
                        Notes = detailDto.Notes,
                        Created_By = dto.Created_By,
                        Created_At = DateTime.Now
                    });
                }

                // 7) إضافة توزيعات المستندات مثل البوالص والتذاكر مع التحقق من الرصيد
                foreach (var allocationDto in dto.Allocations)
                {
                    // [التعديل الثالث]: احتساب الرصيد المتبقي في السيرفر والتحقق من عدم تخطي الحد المسموح به
                    decimal remainingBalance = allocationDto.Document_Total - allocationDto.Collected_Before - allocationDto.Collected_Now;

                    if (remainingBalance < 0)
                    {
                        await transaction.RollbackAsync();
                        return (false, $"المبلغ المحصل للمستند {allocationDto.Document_No} أكبر من رصيده المتبقي.", null, null);
                    }

                    voucher.DocumentAllocations.Add(new DocumentAllocation
                    {
                        Module_ID = allocationDto.Module_ID,
                        Document_Type_ID = allocationDto.Document_Type_ID,
                        Document_ID = allocationDto.Document_ID,
                        Document_No = allocationDto.Document_No,
                        Party_ID = allocationDto.Party_ID,
                        Currency_ID = allocationDto.Currency_ID,
                        Exchange_Rate = allocationDto.Exchange_Rate,
                        Document_Total = allocationDto.Document_Total,
                        Collected_Before = allocationDto.Collected_Before,
                        Collected_Now = allocationDto.Collected_Now,
                        Remaining_Balance = decimal.Round(remainingBalance, 2),
                        Is_Active = true,
                        Notes = allocationDto.Notes,
                        Created_By = dto.Created_By,
                        Created_At = DateTime.Now
                    });
                }

                // 8) تسجيل حركة إنشاء السند
                voucher.VoucherActionLogs.Add(new VoucherActionLog
                {
                    Action_Type = "CREATE",
                    Old_Status_ID = null,
                    New_Status_ID = voucher.Voucher_Status_ID,
                    User_ID = dto.Created_By,
                    Action_At = DateTime.Now,
                    Action_Channel = "DESKTOP",
                    Device_Name = Environment.MachineName,
                    Notes = "تم إنشاء السند المالي."
                });

                // 9) حفظ جميع البيانات
                await _context.Financial_Voucher_Headers.AddAsync(voucher);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (
                    true,
                    $"تم حفظ السند المالي بنجاح. رقم السند: {voucher.Voucher_No}",
                    voucher.Voucher_ID,
                    voucher.Voucher_No
                );
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();

                string error = ex.InnerException?.Message ?? ex.Message;

                return (false, $"تعذر حفظ السند في قاعدة البيانات: {error}", null, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return (false, $"حدث خطأ أثناء حفظ السند المالي: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// توليد رقم السند الرسمي من جدول إعدادات الترقيم،
        /// مع زيادة آخر رقم داخل معاملة حفظ السند نفسها.
        /// </summary>
        private async Task<string> GenerateOfficialVoucherNoAsync(CreateFinancialVoucherDto dto)
        {
            #region تحديد نوع المستند
            // تحديد كود إعداد الترقيم بحسب نوع السند المالي.
            string documentType = dto.Voucher_Type_ID switch
            {
                1 => "RECEIPT_VOUCHER",
                2 => "PAYMENT_VOUCHER",
                3 => "JOURNAL_ENTRY",
                _ => throw new InvalidOperationException($"نوع السند رقم {dto.Voucher_Type_ID} غير مدعوم في إعدادات الترقيم.")
            };
            #endregion

            #region جلب إعداد الترقيم
            var setting = await _context.Numbering_Settings
                .FirstOrDefaultAsync(x => x.Document_Type == documentType && x.Is_Active);

            if (setting == null)
            {
                throw new InvalidOperationException($"لا يوجد إعداد ترقيم فعال لنوع المستند ({documentType}).");
            }
            #endregion

            #region زيادة الرقم التسلسلي
            // زيادة آخر رقم رسميًا.
            // سيُحفظ هذا التغيير مع السند عند SaveChangesAsync.
            setting.Last_Number += 1;
            int nextNumber = setting.Last_Number;

            // تكوين الجزء الرقمي حسب عدد الخانات المحدد.
            string serialPart = nextNumber.ToString().PadLeft(setting.Digits_Count, '0');
            #endregion

            #region تكوين رقم السند
            List<string> numberParts = new List<string>();

            // إضافة البادئة.
            if (!string.IsNullOrWhiteSpace(setting.Prefix))
            {
                numberParts.Add(setting.Prefix.Trim().ToUpper());
            }

            // إضافة الشركة بحسب إعداد الترقيم.
            if (setting.Use_Company)
            {
                numberParts.Add(CurrentCompanyId(dto));
            }

            // إضافة الفرع بحسب إعداد الترقيم.
            if (setting.Use_Branch)
            {
          //      numberParts.Add(dto.Branch_ID.ToString());

                numberParts.Add(dto.Branch_ID);
            }

            // إضافة السنة بحسب إعداد الترقيم.
            if (setting.Use_Year)
            {
                numberParts.Add(dto.Voucher_Date.Year.ToString());
            }

            // إضافة الرقم التسلسلي في نهاية الرقم.
            numberParts.Add(serialPart);

            return string.Join("-", numberParts);
            #endregion
        }

        /// <summary>
        /// استخراج معرف الشركة المستخدم في رقم المستند.
        /// </summary>
        private string CurrentCompanyId(CreateFinancialVoucherDto dto)
        {
            // إذا كان DTO يحتوي Company_ID استخدم:
            // return dto.Company_ID;
            // حاليًا نستخدم الشركة المعتمدة في النظام.
            return "FG-00001";
        }

        /// <summary>
        /// تعديل سند مالي.
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateAsync(
            UpdateFinancialVoucherDto dto)
        {
            if (dto.Details == null || dto.Details.Count < 2)
            {
                return (false, "يجب أن يحتوي السند على سطرين محاسبيين على الأقل.");
            }

            foreach (var detail in dto.Details)
            {
                if (detail.Debit_Amount < 0m || detail.Credit_Amount < 0m)
                {
                    return (false, $"لا يسمح بمبلغ سالب في السطر رقم {detail.Line_No}.");
                }

                if (detail.Debit_Amount > 0m && detail.Credit_Amount > 0m)
                {
                    return (false, $"لا يمكن أن يكون السطر رقم {detail.Line_No} مدينًا ودائنًا معًا.");
                }

                if (detail.Debit_Amount == 0m && detail.Credit_Amount == 0m)
                {
                    return (false, $"يجب إدخال مبلغ مدين أو دائن في السطر رقم {detail.Line_No}.");
                }

                if (detail.Local_Amount <= 0m)
                {
                    return (false, $"المبلغ المحلي في السطر رقم {detail.Line_No} يجب أن يكون أكبر من صفر.");
                }
            }

            decimal totalDebit = decimal.Round(dto.Details.Sum(x => x.Debit_Amount), 2);
            decimal totalCredit = decimal.Round(dto.Details.Sum(x => x.Credit_Amount), 2);

            if (totalDebit != totalCredit)
            {
                return (false, "إجمالي المدين لا يساوي إجمالي الدائن.");
            }

            var cashLines = dto.Details.Where(x => x.Line_Type == 1).ToList();
            if (cashLines.Count != 1)
            {
                return (false, "يجب أن يحتوي السند على سطر صندوق أو بنك واحد فقط.");
            }

            var cashCurrencies = cashLines.Select(x => x.Currency_ID).Distinct().ToList();
            decimal foreignTotal = cashCurrencies.Count == 1
                ? decimal.Round(cashLines.Sum(x => x.Foreign_Amount), 2)
                : 0m;

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var voucher = await _context.Financial_Voucher_Headers
                    .Include(x => x.Details)
                    .Include(x => x.DocumentAllocations)
                    .FirstOrDefaultAsync(x => x.Voucher_ID == dto.Voucher_ID && x.Is_Active);

                if (voucher == null)
                {
                    await transaction.RollbackAsync();
                    return (false, "السند المالي غير موجود.");
                }

                if (voucher.Is_Posted)
                {
                    await transaction.RollbackAsync();
                    return (false, "لا يمكن تعديل سند مرحل. يجب إلغاء الترحيل أولًا.");
                }

                DateTime now = DateTime.Now;
                int oldStatusId = voucher.Voucher_Status_ID;

                voucher.Voucher_Type_ID = dto.Voucher_Type_ID;
                voucher.Voucher_Status_ID = dto.Voucher_Status_ID;
                voucher.Branch_ID = dto.Branch_ID;
                voucher.Fiscal_Year_ID = dto.Fiscal_Year_ID;
                voucher.Voucher_Date = dto.Voucher_Date;
                voucher.Transaction_Date = dto.Transaction_Date;
                voucher.Cash_Account_ID = dto.Cash_Account_ID;
                voucher.Party_ID = dto.Party_ID;
                voucher.Payment_Method_ID = dto.Payment_Method_ID;
                voucher.Currency_ID = dto.Currency_ID;
                voucher.Exchange_Rate = dto.Exchange_Rate;
                voucher.Foreign_Total = foreignTotal;
                voucher.Local_Total = totalDebit;
                voucher.Reference_No = dto.Reference_No;
                voucher.Reference_Date = dto.Reference_Date;
                voucher.Against_Text = dto.Against_Text;
                voucher.Description = dto.Description;
                voucher.Notes = dto.Notes;
                voucher.Module_ID = dto.Module_ID;
                voucher.Document_Type_ID = dto.Document_Type_ID;
                voucher.Document_ID = dto.Document_ID;
                voucher.Source_Document_No = dto.Source_Document_No;
                voucher.Requires_Approval = dto.Requires_Approval;

                // أي تعديل مالي يعيد حالة الاعتماد إلى البداية حتى لا يبقى
                // سند معدل معتمدًا ببيانات قديمة.
                voucher.Approval_Status = dto.Requires_Approval ? (byte)1 : (byte)0;
                voucher.Approval_Requested_By_User_ID = dto.Requires_Approval ? dto.Updated_By : null;
                voucher.Approval_Requested_At = dto.Requires_Approval ? now : null;
                voucher.Approved_By_User_ID = null;
                voucher.Approved_At = null;
                voucher.Rejected_By_User_ID = null;
                voucher.Rejected_At = null;
                voucher.Rejection_Reason = null;
                voucher.Updated_By = dto.Updated_By;
                voucher.Updated_At = now;
                voucher.Edit_Count += 1;

                _context.Financial_Voucher_Details.RemoveRange(voucher.Details);
                voucher.Details.Clear();

                foreach (var detailDto in dto.Details.OrderBy(x => x.Line_No))
                {
                    voucher.Details.Add(new FinancialVoucherDetail
                    {
                        Line_No = detailDto.Line_No,
                        Account_ID = detailDto.Account_ID,
                        Description = detailDto.Description,
                        Cost_Center_ID = detailDto.Cost_Center_ID,
                        Project_ID = detailDto.Project_ID,
                        Reference_Type = detailDto.Reference_Type,
                        Reference_No = detailDto.Reference_No,
                        Reference_Name = detailDto.Reference_Name,
                        Reference_Date = detailDto.Reference_Date,
                        Currency_ID = detailDto.Currency_ID,
                        Exchange_Rate = detailDto.Exchange_Rate,
                        Foreign_Amount = detailDto.Foreign_Amount,
                        Local_Amount = detailDto.Local_Amount,
                        Debit_Amount = detailDto.Debit_Amount,
                        Credit_Amount = detailDto.Credit_Amount,
                        Line_Type = detailDto.Line_Type,
                        Notes = detailDto.Notes,
                        Created_By = dto.Updated_By,
                        Created_At = now,
                        Updated_By = dto.Updated_By,
                        Updated_At = now
                    });
                }

                _context.Document_Allocations.RemoveRange(voucher.DocumentAllocations);
                voucher.DocumentAllocations.Clear();

                foreach (var allocationDto in dto.Allocations ?? new List<UpdateDocumentAllocationDto>())
                {
                    decimal remainingBalance = decimal.Round(
                        allocationDto.Document_Total -
                        allocationDto.Collected_Before -
                        allocationDto.Collected_Now,
                        2);

                    if (remainingBalance < 0m)
                    {
                        await transaction.RollbackAsync();
                        return (false, $"المبلغ المحصل للمستند {allocationDto.Document_No} أكبر من رصيده المتبقي.");
                    }

                    voucher.DocumentAllocations.Add(new DocumentAllocation
                    {
                        Module_ID = allocationDto.Module_ID,
                        Document_Type_ID = allocationDto.Document_Type_ID,
                        Document_ID = allocationDto.Document_ID,
                        Document_No = allocationDto.Document_No,
                        Party_ID = allocationDto.Party_ID,
                        Currency_ID = allocationDto.Currency_ID,
                        Exchange_Rate = allocationDto.Exchange_Rate,
                        Document_Total = allocationDto.Document_Total,
                        Collected_Before = allocationDto.Collected_Before,
                        Collected_Now = allocationDto.Collected_Now,
                        Remaining_Balance = remainingBalance,
                        Is_Active = true,
                        Notes = allocationDto.Notes,
                        Created_By = dto.Updated_By,
                        Created_At = now,
                        Updated_By = dto.Updated_By,
                        Updated_At = now
                    });
                }

                voucher.VoucherActionLogs.Add(new VoucherActionLog
                {
                    Action_Type = "UPDATE",
                    Old_Status_ID = oldStatusId,
                    New_Status_ID = voucher.Voucher_Status_ID,
                    User_ID = dto.Updated_By,
                    Action_At = now,
                    Action_Channel = "DESKTOP",
                    Device_Name = Environment.MachineName,
                    Notes = "تم تعديل بيانات السند وتفاصيله."
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, $"تم تعديل السند المالي بنجاح. رقم السند: {voucher.Voucher_No}");
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                string error = ex.InnerException?.Message ?? ex.Message;
                return (false, $"تعذر تعديل السند في قاعدة البيانات: {error}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"حدث خطأ أثناء تعديل السند المالي: {ex.Message}");
            }
        }

        /// <summary>
        /// حذف سند مالي.
        /// </summary>
        public async Task<(bool Success, string Message)> DeleteAsync(
            long voucherId)
        {
            await Task.CompletedTask;

            return (true, "سيتم تنفيذ الحذف لاحقاً.");
        }
        /// <summary>
        /// جلب سند مالي كامل بواسطة معرف السند.
        /// يشمل رأس السند والتفاصيل وتوزيعات المستندات.
        /// </summary>
       
        
        public async Task<FinancialVoucherResponseDto?> GetByIdAsync(
            long voucherId)
        {
            #region جلب السند من قاعدة البيانات

            var voucher =
                await _context.Financial_Voucher_Headers
                    .AsNoTracking()
                    .Include(x => x.Details)
                    .Include(x => x.DocumentAllocations)
                    .FirstOrDefaultAsync(x =>
                        x.Voucher_ID == voucherId &&
                        x.Is_Active);

            if (voucher == null)
            {
                return null;
            }

            #endregion

            #region تجهيز رأس السند

            var result =
                new FinancialVoucherResponseDto
                {
                    Voucher_ID =
                        voucher.Voucher_ID,

                    Voucher_No =
                        voucher.Voucher_No,

                    Voucher_Type_ID =
                        voucher.Voucher_Type_ID,

                    Voucher_Type_Name =
                        string.Empty,

                    Voucher_Status_ID =
                        voucher.Voucher_Status_ID,

                    Voucher_Status_Name =
                        string.Empty,

                    Branch_ID =
                        voucher.Branch_ID,

                    Fiscal_Year_ID =
                        voucher.Fiscal_Year_ID,

                    Voucher_Date =
                        voucher.Voucher_Date,

                    Transaction_Date =
                        voucher.Transaction_Date,

                    Cash_Account_ID =
                        voucher.Cash_Account_ID,
                   
                    Cash_Account_Name =
                 _context.Chart_Of_Accounts
                   .Where(a => a.Account_ID == voucher.Cash_Account_ID)
                   .Select(a => a.Account_Name_AR)
                   .FirstOrDefault() ?? string.Empty,


                    Party_ID =
                        voucher.Party_ID,

                    Party_Name =
                        string.IsNullOrWhiteSpace(voucher.Party_ID)
                            ? string.Empty
                            : _context.Parties
                                .Where(x => x.Party_ID == voucher.Party_ID)
                                .Select(x => x.Party_Name_AR)
                                .FirstOrDefault() ?? string.Empty,

                    Payment_Method_ID =
                        voucher.Payment_Method_ID,

                    Payment_Method_Name =
                        string.Empty,

                    Currency_ID =
                        voucher.Currency_ID,

                    Currency_Name =
                        string.Empty,

                    Exchange_Rate =
                        voucher.Exchange_Rate,

                    Foreign_Total =
                        voucher.Foreign_Total,

                    Local_Total =
                        voucher.Local_Total,

                    Reference_No =
                        voucher.Reference_No,

                    Reference_Date =
                        voucher.Reference_Date,

                    Against_Text =
                        voucher.Against_Text,

                    Description =
                        voucher.Description,

                    Notes =
                        voucher.Notes,

                    Requires_Approval =
                        voucher.Requires_Approval,

                    Approval_Status =
                        voucher.Approval_Status,

                    Is_Posted =
                        voucher.Is_Posted,

                    Journal_Entry_ID =
                        voucher.Journal_Entry_ID,

                    Journal_Entry_No =
                        voucher.Journal_Entry_ID.HasValue
                            ? _context.Journal_Entry_Headers
                                .Where(x => x.Journal_Entry_ID == voucher.Journal_Entry_ID.Value)
                                .Select(x => x.Entry_No)
                                .FirstOrDefault()
                            : null,

                    Edit_Count =
                        voucher.Edit_Count,

                    Print_Count =
                        voucher.Print_Count,

                    Last_Printed_By =
                        voucher.Last_Printed_By,

                    Last_Print_Date =
                        voucher.Last_Print_Date,

                    Undo_Count =
                        voucher.Undo_Count,

                    Last_Undo_By =
                        voucher.Last_Undo_By,

                    Last_Undo_At =
                        voucher.Last_Undo_At,

                    Created_By =
                        voucher.Created_By,

                    Created_At =
                        voucher.Created_At,

                    Updated_By =
                        voucher.Updated_By,

                    Updated_At =
                        voucher.Updated_At
                };

            #endregion

            #region تجهيز تفاصيل القيود المحاسبية

            result.Details =
                voucher.Details
                    .OrderBy(x => x.Line_No)
                    .Select(x =>
                        new FinancialVoucherDetailResponseDto
                        {
                            Voucher_Detail_ID =
                                x.Voucher_Detail_ID,

                            Line_No =
                                x.Line_No,

                            Account_ID =
                                x.Account_ID,


                            Account_Name =
                          _context.Chart_Of_Accounts
                         .Where(a => a.Account_ID == x.Account_ID)
                         .Select(a => a.Account_Name_AR)
                          .FirstOrDefault() ?? string.Empty,


                            Description =
                           x.Description,

                            Cost_Center_ID =
                           x.Cost_Center_ID,

                            Cost_Center_Name =
                            string.Empty,

                            Project_ID =
                           x.Project_ID,

                            Project_Name =
                                  string.Empty,

                            Reference_Type =
                          x.Reference_Type,

                            Reference_No =
                           x.Reference_No,

                            Reference_Name =
                          x.Reference_Name,

                            Reference_Date =
                          x.Reference_Date,

                            Currency_ID =
                          x.Currency_ID,

                            Currency_Name =
                           string.Empty,

                            Exchange_Rate =
                           x.Exchange_Rate,

                            Foreign_Amount =
                            x.Foreign_Amount,

                            Local_Amount =
                           x.Local_Amount,

                            Debit_Amount =
                           x.Debit_Amount,

                            Credit_Amount =
                           x.Credit_Amount,

                            Line_Type =
                           x.Line_Type,

                            Notes =
                            x.Notes



                        })
                    .ToList();

            #endregion

            #region تجهيز توزيعات المستندات

            result.Allocations =
                voucher.DocumentAllocations
                    .Where(x => x.Is_Active)
                    .OrderBy(x => x.Allocation_ID)
                    .Select(x =>
                        new DocumentAllocationResponseDto
                        {
                            Allocation_ID =
                                x.Allocation_ID,

                            Module_ID =
                                x.Module_ID,

                            Module_Name =
                                string.Empty,

                            Document_Type_ID =
                                x.Document_Type_ID,

                            Document_Type_Name =
                                string.Empty,

                            Document_ID =
                                x.Document_ID,

                            Document_No =
                                x.Document_No,

                            Document_Total =
                                x.Document_Total,

                            Collected_Before =
                                x.Collected_Before,

                            Collected_Now =
                                x.Collected_Now,

                            Remaining_Balance =
                                x.Remaining_Balance
                        })
                    .ToList();

            #endregion

            return result;
        }

        /// <summary>
        /// البحث عن سند مالي بواسطة رقم السند والفرع والسنة المالية.
        /// </summary>
        public async Task<FinancialVoucherResponseDto?>
            GetByVoucherNumberAsync(
                string voucherNumber,
                string branchId,
                int fiscalYearId)
        {
            #region التحقق من معاملات البحث

            if (string.IsNullOrWhiteSpace(voucherNumber))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(branchId))
            {
                return null;
            }

            if (fiscalYearId <= 0)
            {
                return null;
            }

            voucherNumber =
                voucherNumber.Trim();

            branchId =
                branchId.Trim();

            #endregion

            #region البحث عن معرف السند

            long? voucherId =
                await _context.Financial_Voucher_Headers
                    .AsNoTracking()
                    .Where(x =>
                        x.Voucher_No == voucherNumber &&
                        x.Branch_ID == branchId &&
                        x.Fiscal_Year_ID == fiscalYearId &&
                        x.Is_Active)
                    .Select(x =>
                        (long?)x.Voucher_ID)
                    .FirstOrDefaultAsync();

            if (!voucherId.HasValue)
            {
                return null;
            }

            #endregion

            return await GetByIdAsync(
                voucherId.Value);
        }


    }
}
