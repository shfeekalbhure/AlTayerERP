using AlTayerERP.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
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
            public DateTime Voucher_Date { get; set; }
            public string Cash_Account_ID { get; set; } = string.Empty;
            public string? Party_ID { get; set; }
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
            public int Edit_Count { get; set; }
            public int Print_Count { get; set; }
            public DateTime? Last_Print_At { get; set; }
            public string? Last_Print_By { get; set; }
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

                // إذا أدخل المستخدم الرقم الكامل نستخدمه كما هو،
                // وإذا أدخل رقمًا فقط نبني رقم السند الكامل.
                string voucherNumber =
                    enteredValue.Contains("-")
                        ? enteredValue
                        : BuildReceiptVoucherNumber(enteredValue);

                await SearchVoucherAsync(voucherNumber);
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

        /// <summary>
        /// بناء رقم سند القبض الكامل من الرقم التسلسلي.
        /// مثال: 7 يتحول إلى RCV-2026-0007.
        /// </summary>
        private string BuildReceiptVoucherNumber(string sequenceText)
        {
            if (!int.TryParse(sequenceText, out int sequence) ||
                sequence <= 0)
            {
                throw new InvalidOperationException(
                    "أدخل رقم سند صحيحًا، مثال: 7");
            }

            return $"RCV-{CurrentSession.Year_Name}-{sequence:0000}";
        }
        #endregion

        #region === استدعاء API ===

        private async Task SearchVoucherAsync(string voucherNumber)
        {
            string requestUrl = BuildSearchUrl(voucherNumber);
            SearchVoucherResponse? searchResponse = await _client.GetFromJsonAsync<SearchVoucherResponse>(requestUrl);

            if (!ValidateResponse(searchResponse, out string errorMessage))
            {
                MessageBox.Show(errorMessage, "بحث", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LoadVoucherIntoScreen(searchResponse!.Data!);
        }

        private string BuildSearchUrl(string voucherNumber)
        {
            return $"{_baseUrl}FinancialVoucher/ByNumber" +
                   $"?voucherNumber={Uri.EscapeDataString(voucherNumber)}" +
                   $"&branchId={CurrentSession.Branch_ID}" +
                   $"&fiscalYearId={CurrentSession.Year_ID}";
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
                UpdateStatusLabels();

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
            txtVoucherNo.Text = voucher.Voucher_No ?? string.Empty;
            SetDatePickerValue(dtVoucherDate, voucher.Voucher_Date);
            SetComboBoxValue(cmbVoucherType, voucher.Voucher_Type_ID);
            SetComboBoxValue(cmbStatus, voucher.Voucher_Status_ID);
            SetComboBoxValue(cmbCashAccount, voucher.Cash_Account_ID);
            SetComboBoxValue(cmbParty, voucher.Party_ID);
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
        }

        private void LoadSystemInfo(FinancialVoucherResponseModel voucher)
        {
            txtCreatedBy.Text = voucher.Created_By ?? string.Empty;
            txtCreatedDate.Text = FormatDateTime(voucher.Created_At);
            txtUpdatedBy.Text = voucher.Updated_By ?? string.Empty;
            txtUpdatedDate.Text = FormatDateTime(voucher.Updated_At);
            SafeSetText(txtEditCount, voucher.Edit_Count.ToString());
            SafeSetText(txtPrintCount, voucher.Print_Count.ToString());
            SafeSetText(txtLastPrintDate, FormatDateTime(voucher.Last_Print_At));
            SafeSetText(txtLastPrintedBy, voucher.Last_Print_By ?? string.Empty);
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
            bool isPosted = voucher.Is_Posted;
            btnSave.Enabled = false;
            btnEdit.Enabled = !isPosted;
            btnDelete.Enabled = !isPosted;
            btnPost.Enabled = !isPosted;
            btnUnPost.Enabled = isPosted;
            btnPrint.Enabled = true;
        }

        private void UpdateStatusLabels()
        {
            lblStatusApi.Text = "API: متصل";
            lblStatusDatabase.Text = "قاعدة البيانات: متصلة";
        }

        #endregion

        #region === مساعدات ===

        private decimal GetHeaderEnteredAmount(FinancialVoucherResponseModel voucher)
        {
            if (voucher.Amount > 0m) return voucher.Amount;

            CurrencyLookupModel? currency = _currencyLookups.FirstOrDefault(x => x.Currency_ID == voucher.Currency_ID);
            if (currency != null && currency.Is_Local_Currency)
                return voucher.Local_Total;

            return voucher.Foreign_Total;
        }

        #endregion
    }
}