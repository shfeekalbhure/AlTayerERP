using AlTayerERP.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// البحث عن سند القبض وتحميل بياناته.
    /// </summary>
    public partial class FrmReceiptVoucher
    {
        #region === نماذج البيانات ===

        private sealed class SearchVoucherResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public FinancialVoucherResponseModel? Data { get; set; }
        }

        private sealed class FinancialVoucherResponseModel
        {
            public long Voucher_ID { get; set; }
            public string Voucher_No { get; set; } = string.Empty;
            public int Voucher_Type_ID { get; set; }
            public int Voucher_Status_ID { get; set; }
            public string Branch_ID { get; set; } = string.Empty;
            public int? Fiscal_Year_ID { get; set; }
            public DateTime Voucher_Date { get; set; }
            public string Cash_Account_ID { get; set; } = string.Empty;
            public string Cash_Account_Name { get; set; } = string.Empty;
            public string? Party_ID { get; set; }
            public string? Received_From_Name { get; set; }
            public int? Payment_Method_ID { get; set; }
            public int Currency_ID { get; set; }
            public decimal Exchange_Rate { get; set; }
            public decimal Foreign_Total { get; set; }
            public decimal Local_Total { get; set; }
            public decimal Amount { get; set; }
            public string? Reference_No { get; set; }
            public DateTime? Reference_Date { get; set; }
            public string? Against_Text { get; set; }
            public string? Notes { get; set; }
            public bool Is_Posted { get; set; }
            public byte Approval_Status { get; set; }
            public byte Review_Status { get; set; }
            public string? Reviewed_By_User_ID { get; set; }
            public DateTime? Reviewed_At { get; set; }
            public string? Review_Notes { get; set; }
            public long? Journal_Entry_ID { get; set; }
            public string? Journal_Entry_No { get; set; }
            public int Edit_Count { get; set; }
            public int Print_Count { get; set; }
            public DateTime? Last_Print_Date { get; set; }
            public string? Last_Printed_By { get; set; }
            public string? Created_By { get; set; }
            public DateTime Created_At { get; set; }
            public string? Updated_By { get; set; }
            public DateTime? Updated_At { get; set; }

           

            public bool Requires_Approval { get; set; }
            public List<FinancialVoucherDetailResponseModel> Details { get; set; } = new();
        }

        private sealed class FinancialVoucherDetailResponseModel
        {
            public int Line_No { get; set; }
            public byte Line_Type { get; set; }
            public string Account_ID { get; set; } = string.Empty;
            public string Account_Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string? Cost_Center_ID { get; set; }
            public int Currency_ID { get; set; }
            public decimal Exchange_Rate { get; set; }
            public decimal Foreign_Amount { get; set; }
            public decimal Local_Amount { get; set; }
            public string? Notes { get; set; }
            public string? Reference_No { get; set; }
            public string? Reference_Type { get; set; }
            public DateTime? Reference_Date { get; set; }
            public string? Reference_Name { get; set; }
        }

        #endregion

        #region === حدث البحث ===

        private async void btnSearch_Click(object? sender, EventArgs e)
        {
            try
            {
                // استخراج الرقم التسلسلي فقط من رقم السند الظاهر في الشاشة
                string defaultSequence =
                    ExtractVoucherSequence(txtVoucherNo.Text);

                string enteredValue =
                    Microsoft.VisualBasic.Interaction.InputBox(
                        "أدخل رقم السند فقط، مثال: 7",
                        "بحث عن سند قبض",
                        defaultSequence);

                if (string.IsNullOrWhiteSpace(enteredValue))
                {
                    return;
                }

                enteredValue = enteredValue.Trim();

                bool useCurrentContext =
                    int.TryParse(enteredValue, out int sequence) && sequence > 0;

                if (!useCurrentContext && !enteredValue.Contains('-'))
                {
                    throw new InvalidOperationException(
                        "أدخل الرقم التسلسلي فقط مثل 7، أو رقم السند كاملًا كما يظهر في الشاشة.");
                }

                await SearchVoucherAsync(enteredValue, useCurrentContext);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "بحث",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// استخراج الجزء التسلسلي من رقم السند.
        /// مثال: RCV-2026-0007 يرجع 7.
        /// </summary>
        private static string ExtractVoucherSequence(string? voucherNumber)
        {
            if (string.IsNullOrWhiteSpace(voucherNumber))
            {
                return string.Empty;
            }

            string value = voucherNumber.Trim();

            if (value.Contains("جاري", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("تعذر", StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            string lastPart =
                value.Split('-').LastOrDefault() ?? value;

            return int.TryParse(lastPart, out int sequence)
                ? sequence.ToString()
                : string.Empty;
        }

        #endregion

        #region === استدعاء API ===

        private async Task SearchVoucherAsync(
            string voucherNumber,
            bool useCurrentContext = false)
        {
            string requestUrl = BuildSearchUrl(voucherNumber, useCurrentContext);
            using HttpResponseMessage response = await _client.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                await ShowVoucherApiErrorAsync(response, "لم يتم العثور على السند المطلوب.");
                return;
            }

            SearchVoucherResponse? searchResponse =
                await response.Content.ReadFromJsonAsync<SearchVoucherResponse>();

            if (!ValidateResponse(searchResponse, out string errorMessage))
            {
                MessageBox.Show(errorMessage, "بحث", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LoadVoucherIntoScreen(searchResponse!.Data!);
        }

        private string BuildSearchUrl(
            string voucherNumber,
            bool useCurrentContext)
        {
            int voucherTypeId =
                cmbVoucherType.SelectedValue == null
                    ? 1
                    : Convert.ToInt32(cmbVoucherType.SelectedValue);

            string url = $"{_baseUrl}FinancialVoucher/ByNumber" +
                         $"?voucherNumber={Uri.EscapeDataString(voucherNumber.Trim())}" +
                         $"&voucherTypeId={voucherTypeId}";

            if (useCurrentContext)
            {
                url += $"&branchId={CurrentSession.Branch_ID}" +
                       $"&fiscalYearId={CurrentSession.Year_ID}";
            }

            return url;
        }

        private static bool ValidateResponse(SearchVoucherResponse? response, out string errorMessage)
        {
            if (response == null)
            {
                errorMessage = "لم تصل أي استجابة من الخادم.";
                return false;
            }

            if (!response.Success)
            {
                errorMessage = response.Message;
                return false;
            }

            if (response.Data == null)
            {
                errorMessage = "لم يتم العثور على بيانات السند.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        #endregion

        #region === تحميل البيانات ===

        private void LoadVoucherIntoScreen(FinancialVoucherResponseModel voucher)
        {
            _isLoading = true;

            try
            {
                LoadHeaderData(voucher);
                LoadSystemInfo(voucher);
                LoadDetailsGrid(voucher);
                UpdateTotals(voucher);
                UpdateButtonsState(voucher);

                SetViewMode();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void LoadHeaderData(FinancialVoucherResponseModel voucher)
        {
            _selectedVoucherId = voucher.Voucher_ID;
            _loadedVoucherBranchId = voucher.Branch_ID ?? string.Empty;
            _loadedFiscalYearId = voucher.Fiscal_Year_ID ?? 0;
            _loadedPartyId = voucher.Party_ID;
            _loadedReceivedFromName = voucher.Received_From_Name?.Trim() ?? string.Empty;
            _isCrossContextVoucher =
                !string.Equals(_loadedVoucherBranchId, CurrentSession.Branch_ID.ToString(), StringComparison.Ordinal) ||
                (_loadedFiscalYearId > 0 && _loadedFiscalYearId != CurrentSession.Year_ID);
            _currentApprovalStatus = voucher.Approval_Status;
            _currentReviewStatus = voucher.Review_Status;
            txtVoucherNo.Text = voucher.Voucher_No ?? string.Empty;
            SetDatePickerValue(dtVoucherDate, voucher.Voucher_Date);
            SetComboBoxValue(cmbVoucherType, voucher.Voucher_Type_ID);
            SetComboBoxValue(cmbStatus, voucher.Voucher_Status_ID);
            if (int.TryParse(voucher.Branch_ID, out int branchId))
            {
                SetComboBoxValue(cmbBranch, branchId);
                if (!string.Equals(cmbBranch.SelectedValue?.ToString(), voucher.Branch_ID, StringComparison.Ordinal))
                {
                    cmbBranch.Text = $"الفرع رقم {voucher.Branch_ID}";
                }
            }
            SetComboBoxValue(cmbCashAccount, voucher.Cash_Account_ID);
            if (!string.Equals(cmbCashAccount.SelectedValue?.ToString(), voucher.Cash_Account_ID, StringComparison.Ordinal) &&
                !string.IsNullOrWhiteSpace(voucher.Cash_Account_Name))
            {
                cmbCashAccount.Text = voucher.Cash_Account_Name;
            }
            SetComboBoxValue(cmbParty, voucher.Party_ID);
            if (!string.IsNullOrWhiteSpace(voucher.Received_From_Name))
            {
                cmbParty.Text = voucher.Received_From_Name.Trim();
            }
            SetComboBoxValue(cmbPaymentMethod, voucher.Payment_Method_ID);
            SetComboBoxValue(cmbCurrency, voucher.Currency_ID);

            numExchangeRate.Value = LimitNumericValue(numExchangeRate, voucher.Exchange_Rate > 0m ? voucher.Exchange_Rate : 1m);
            numForeignAmount.Value = LimitNumericValue(numForeignAmount, voucher.Foreign_Total);
            numLocalAmount.Value = LimitNumericValue(numLocalAmount, voucher.Local_Total);

            decimal enteredAmount = GetHeaderEnteredAmount(voucher);
            numAmount.Value = LimitNumericValue(numAmount, enteredAmount);

            txtReference.Text = voucher.Reference_No ?? string.Empty;
            txtReferenceNo.Text = voucher.Reference_No ?? string.Empty;
            SetDatePickerValue(dtReferenceDate, voucher.Reference_Date);
            txtAgainst.Text = voucher.Against_Text ?? string.Empty;
            txtHeaderNotes.Text = voucher.Notes ?? string.Empty;
            chkPosted.Checked = voucher.Is_Posted;
            checkBox2.Checked = voucher.Requires_Approval;
            UpdateReviewStatusDisplay(
                voucher.Reviewed_By_User_ID,
                voucher.Reviewed_At,
                voucher.Review_Notes);

            FinancialVoucherDetailResponseModel? cashLine =
                voucher.Details?.FirstOrDefault(x => x.Line_Type == 1);
            SetComboBoxValue(cmbCostCenter, cashLine?.Cost_Center_ID);

            txtJournalNo.Text =
                !string.IsNullOrWhiteSpace(voucher.Journal_Entry_No)
                    ? voucher.Journal_Entry_No
                    : voucher.Journal_Entry_ID?.ToString() ?? string.Empty;
        }

        private void LoadSystemInfo(FinancialVoucherResponseModel voucher)
        {
            txtCreatedBy.Text = voucher.Created_By ?? string.Empty;
            txtCreatedDate.Text = FormatDateTime(voucher.Created_At);
            txtUpdatedBy.Text = voucher.Updated_By ?? string.Empty;
            txtUpdatedDate.Text = FormatDateTime(voucher.Updated_At);
            SafeSetText(txtEditCount, voucher.Edit_Count.ToString());
            SafeSetText(txtPrintCount, voucher.Print_Count.ToString());
            SafeSetText(txtLastPrintDate, FormatDateTime(voucher.Last_Print_Date));
            SafeSetText(txtLastPrintedBy, voucher.Last_Printed_By ?? string.Empty);
        }

        private void LoadDetailsGrid(FinancialVoucherResponseModel voucher)
        {
            dgvVoucherDetails.Rows.Clear();
            if (voucher.Details == null) return;

            var detailLines = voucher.Details.Where(x => x.Line_Type != 1).OrderBy(x => x.Line_No);

            foreach (var detail in detailLines)
            {
                int rowIndex = dgvVoucherDetails.Rows.Add();
                DataGridViewRow row = dgvVoucherDetails.Rows[rowIndex];

                SetCellValue(row, colNo, rowIndex + 1);
                SetCellValue(row, colAccountCode, detail.Account_ID);
                SetCellValue(row, colAccountName, detail.Account_ID);
                SetCellValue(row, colDescription, detail.Description);
                SetCellValue(row, colCostCenter, detail.Cost_Center_ID);
                SetCellValue(row, colCurrency, detail.Currency_ID);
                SetCellValue(row, colExchangeRate, detail.Exchange_Rate);

                decimal enteredAmount = detail.Foreign_Amount > 0m ? detail.Foreign_Amount : detail.Local_Amount;
                SetCellValue(row, colAmount, enteredAmount);
                SetCellValue(row, colForeignAmount, detail.Foreign_Amount);
                SetCellValue(row, colLocalAmount, detail.Local_Amount);

                SafeSetCellValue(row, colReferenceNo, detail.Reference_No);
                SafeSetCellValue(row, colReferenceType, detail.Reference_Type);
                SafeSetCellValue(row, colReferenceDate,
                    detail.Reference_Date.HasValue ? detail.Reference_Date.Value.ToString("yyyy/MM/dd") : string.Empty);
                SafeSetCellValue(row, colReferenceName, detail.Reference_Name);
                SetCellValue(row, colNotes, detail.Notes);
            }
        }

        private void UpdateTotals(FinancialVoucherResponseModel voucher)
        {
            txtTotalAmount.Text = voucher.Local_Total.ToString("N2");
            txtTotalForeignAmount.Text = voucher.Foreign_Total.ToString("N2");
            txtDifference.Text = "0.00";
        }

        private void UpdateButtonsState(FinancialVoucherResponseModel voucher)
        {
            _currentApprovalStatus = voucher.Approval_Status;
            _currentReviewStatus = voucher.Review_Status;
            UpdateWorkflowButtonsState();
        }

        #endregion

        #region === مساعدات ===

        private decimal GetHeaderEnteredAmount(FinancialVoucherResponseModel voucher)
        {
            // السجلات الجديدة تعيد المبلغ الأصلي من العمود Amount.
            if (voucher.Amount > 0m)
            {
                return voucher.Amount;
            }

            // توافق مع السجلات القديمة التي سبقت إضافة العمود.
            return voucher.Foreign_Total > 0m
                ? voucher.Foreign_Total
                : voucher.Local_Total;
        }

        private void UpdateReviewStatusDisplay(
            string? reviewedBy,
            DateTime? reviewedAt,
            string? notes = null)
        {
            string statusText = _currentReviewStatus switch
            {
                1 => "قيد المراجعة",
                2 => "تمت المراجعة",
                3 => "معاد للتصحيح",
                _ => "غير مراجع"
            };

            _lblReviewStatus.Text = $"المراجعة: {statusText}";
            if (_isCrossContextVoucher)
            {
                _lblReviewStatus.Text += " | عرض فقط";
            }
            _lblReviewStatus.ForeColor = _currentReviewStatus switch
            {
                2 => System.Drawing.Color.DarkGreen,
                3 => System.Drawing.Color.DarkRed,
                1 => System.Drawing.Color.DarkOrange,
                _ => System.Drawing.Color.DarkRed
            };

            string details = $"حالة المراجعة: {statusText}";
            if (_isCrossContextVoucher)
            {
                details += "\nالسند تابع لفرع أو سنة أخرى؛ متاح للعرض والطباعة فقط.";
            }
            if (!string.IsNullOrWhiteSpace(reviewedBy))
            {
                details += $"\nالمراجع: {reviewedBy}";
            }
            if (reviewedAt.HasValue)
            {
                details += $"\nالتاريخ: {reviewedAt.Value:yyyy/MM/dd hh:mm tt}";
            }
            if (!string.IsNullOrWhiteSpace(notes))
            {
                details += $"\nالملاحظات: {notes}";
            }

            _workflowToolTip.SetToolTip(_lblReviewStatus, details);
        }

        #endregion
    }
}
