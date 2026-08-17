using AlTayerERP.Desktop.Services;
using System;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// تجهيز سند قبض جديد وتوليد رقمه.
    /// </summary>
    public partial class FrmReceiptVoucher
    {
        #region === تجهيز سند جديد ===

        private void NewVoucher()
        {
            _selectedVoucherId = 0;
            _currentReviewStatus = 0;
            _currentApprovalStatus = 0;
            _loadedVoucherBranchId = CurrentSession.Branch_ID;
            _loadedFiscalYearId = CurrentSession.Year_ID;
            _loadedPartyId = null;
            _loadedReceivedFromName = string.Empty;
            _isCrossContextVoucher = false;
            UpdateReviewStatusDisplay(null, null);
            // لا يُحجز رقم عند فتح المسودة؛ الخادم هو المصدر الوحيد للرقم الرسمي عند الحفظ.
            txtVoucherNo.Text = "مسودة جديدة";

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

        private void btnNew_Click(object? sender, EventArgs e)
        {
            try
            {
                _isLoading = true;
                UseWaitCursor = true;

                NewVoucher();
                SetNewMode();

                txtAgainst.Focus();
            }
            catch (Exception ex)
            {
                txtVoucherNo.Text = "تعذر تجهيز المسودة";

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

    }
}
