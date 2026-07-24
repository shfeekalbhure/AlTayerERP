using AlTayerERP.Desktop.Services;
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// تجهيز سند قبض جديد وتوليد رقمه.
    /// </summary>
    public partial class FrmReceiptVoucher
    {
        #region === نموذج استجابة الترقيم ===

        private sealed class GeneratedDocumentNumberModel
        {
            // الرقم النهائي المحجوز من الخادم؛ لا يعاد بعد إلغاء السند.
            public string Document_Number { get; set; } = string.Empty;
            public string Document_Type { get; set; } = string.Empty;
            public int Serial_Number { get; set; }
        }

        #endregion

        #region === تجهيز سند جديد ===

        private void NewVoucher()
        {
            _selectedVoucherId = 0;
            _currentReviewStatus = 0;
            _currentApprovalStatus = 0;
            _loadedVoucherBranchId = CurrentSession.Branch_ID.ToString();
            _loadedFiscalYearId = CurrentSession.Year_ID;
            _loadedPartyId = null;
            _loadedReceivedFromName = string.Empty;
            _isCrossContextVoucher = false;
            UpdateReviewStatusDisplay(null, null);
            txtVoucherNo.Text = "جاري توليد الرقم...";

            dtVoucherDate.Value = DateTime.Today;
            dtReferenceDate.Value = DateTime.Today;

            cmbCashAccount.SelectedIndex = cmbCashAccount.Items.Count > 0 ? 0 : -1;
            cmbParty.SelectedIndex = cmbParty.Items.Count > 0 ? 0 : -1;
            cmbCostCenter.SelectedIndex = cmbCostCenter.Items.Count > 0 ? 0 : -1;
            cmbPaymentMethod.SelectedIndex = cmbPaymentMethod.Items.Count > 0 ? 0 : -1;

            txtReferenceNo.Clear();
            txtReference.Clear();
            txtAgainst.Clear();
            txtHeaderNotes.Clear();

            SetNumericValueSafe(numForeignAmount, 0m);
            SetNumericValueSafe(numLocalAmount, 0m);
            dgvVoucherDetails.Rows.Clear();
            AddNewVoucherDetailRow();
            // تصفير المبلغ
            SetNumericValueSafe(numAmount, 0m);
            // تصفير سعر الصرف
      //      SetNumericValueSafe(numExchangeRate, 1m);

            SetDefaultCurrencyAndExchangeRate();

            txtTotalAmount.Text = "0.00";
            txtTotalForeignAmount.Text = "0.00";
            txtDifference.Text = "0.00";
            txtJournalNo.Clear();

            chkPosted.Checked = false;
            checkBox2.Checked = false;

            txtCreatedBy.Text = CurrentSession.Full_Name;
            txtCreatedDate.Text = DateTime.Now.ToString("yyyy/MM/dd hh:mm tt");
            txtUpdatedBy.Clear();
            txtUpdatedDate.Clear();

            
            dtVoucherDate.Value = DateTime.Today;
            dtReferenceDate.Value = DateTime.Today;
        }

        #endregion

        #region === حدث زر جديد ===

        private async void btnNew_Click(object? sender, EventArgs e)
        {
            try
            {
                _isLoading = true;
                UseWaitCursor = true;

                NewVoucher();

                await GenerateVoucherNumberAsync();

                SetNewMode();

                txtAgainst.Focus();
            }
            catch (Exception ex)
            {
                txtVoucherNo.Text = "تعذر توليد الرقم";

                MessageBox.Show(
                    $"حدث خطأ أثناء تجهيز سند جديد:\n\n{ex.Message}",
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                SetViewMode();
            }
            finally
            {
                _isLoading = false;
                UseWaitCursor = false;
            }
        }

        #endregion

        #region === توليد رقم السند ===

        private async Task GenerateVoucherNumberAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentSession.Company_ID) ||
                CurrentSession.Branch_ID <= 0 || CurrentSession.Year_ID <= 0)
            {
                throw new InvalidOperationException("سياق الشركة والفرع والسنة المالية غير مكتمل.");
            }

            // الحجز عملية POST متعمدة: توليد رقم العرض يغير العداد المركزي،
            // ولذلك لا يمكن أن يتكرر الرقم إذا أغلق المستخدم المسودة أو ألغاها.
            const string documentType = "RECEIPT_VOUCHER";
            var response = await _client.PostAsJsonAsync(
                $"{_baseUrl}NumberingSettings/Reserve",
                new { Document_Type = documentType });

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(await response.Content.ReadAsStringAsync());

            var result = await response.Content.ReadFromJsonAsync<GeneratedDocumentNumberModel>();
            if (result == null || string.IsNullOrWhiteSpace(result.Document_Number))
                throw new InvalidOperationException("لم ترجع خدمة الترقيم رقماً محجوزاً للسند.");

            txtVoucherNo.Text = result.Document_Number;
        }
        #endregion

        #region === تحديد السنة المالية ===

        private int GetCurrentFiscalYearNumber()
        {
            if (!string.IsNullOrWhiteSpace(CurrentSession.Year_Name))
            {
                string yearName = CurrentSession.Year_Name.Trim();

                if (int.TryParse(yearName, out int directYear) && directYear >= 2000 && directYear <= 3000)
                    return directYear;

                string[] parts = yearName.Split(new[] { ' ', '/', '-' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string part in parts)
                {
                    if (int.TryParse(part, out int parsedYear) && parsedYear >= 2000 && parsedYear <= 3000)
                        return parsedYear;
                }
            }

            return dtVoucherDate.Value.Year;
        }

        #endregion
    }
}
