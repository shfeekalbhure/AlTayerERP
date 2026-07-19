using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// التحكم في حالات شاشة سند القبض:
    /// عرض، جديد، تعديل.
    /// </summary>
    public partial class FrmReceiptVoucher
    {
        #region === حالات الشاشة ===

        private enum VoucherScreenMode
        {
            View,
            New,
            Edit
        }

        private VoucherScreenMode _screenMode =
            VoucherScreenMode.View;

        #endregion

        #region === تغيير حالة الشاشة ===

        /// <summary>
        /// وضع العرض:
        /// يقفل حقول السند ويمنع التعديل المباشر.
        /// </summary>
        private void SetViewMode()
        {
            _screenMode = VoucherScreenMode.View;

            SetVoucherFieldsEditable(false);

            btnNew.Enabled = true;
            btnSearch.Enabled = true;
            btnRefresh.Enabled = _selectedVoucherId > 0;
            btnPrint.Enabled = _selectedVoucherId > 0;
            btnViewJournalEntry.Enabled = _selectedVoucherId > 0;

            btnSave.Enabled = false;
            btnUndo.Enabled = false;

            UpdateWorkflowButtonsState();
        }

        /// <summary>
        /// وضع إنشاء سند جديد.
        /// </summary>
        private void SetNewMode()
        {
            _screenMode = VoucherScreenMode.New;

            SetVoucherFieldsEditable(true);

            btnNew.Enabled = false;
            btnSearch.Enabled = false;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
            btnRefresh.Enabled = false;
            btnPrint.Enabled = false;
            btnViewJournalEntry.Enabled = false;

            btnSave.Enabled = true;
            btnUndo.Enabled = true;

            btnPost.Enabled = false;
            btnUnPost.Enabled = false;
            btnApprove.Enabled = false;
            btnCancelApprove.Enabled = false;
            btnImport.Enabled = false;
            btnExport.Enabled = false;

            cmbParty.Focus();
        }

        /// <summary>
        /// وضع تعديل سند محفوظ.
        /// </summary>
        private void SetEditMode()
        {
            if (_selectedVoucherId <= 0)
            {
                MessageBox.Show(
                    "ابحث عن السند المراد تعديله أولًا.",
                    "تنبيه",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (chkPosted.Checked)
            {
                MessageBox.Show(
                    "لا يمكن تعديل سند مرحل.\n" +
                    "يجب إلغاء الترحيل أولًا.",
                    "السند مرحل",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (_currentApprovalStatus == 2)
            {
                MessageBox.Show(
                    "لا يمكن تعديل سند معتمد.\nيجب إلغاء الاعتماد أولًا.",
                    "السند معتمد",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            _screenMode = VoucherScreenMode.Edit;

            SetVoucherFieldsEditable(true);

            btnNew.Enabled = false;
            btnSearch.Enabled = false;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
            btnRefresh.Enabled = false;
            btnPrint.Enabled = false;
            btnViewJournalEntry.Enabled = false;

            btnSave.Enabled = true;
            btnUndo.Enabled = true;

            btnPost.Enabled = false;
            btnUnPost.Enabled = false;
            btnApprove.Enabled = false;
            btnCancelApprove.Enabled = false;
            btnImport.Enabled = false;
            btnExport.Enabled = false;

            cmbParty.Focus();
        }

        #endregion

        /// <summary>
        /// توحيد حالة أزرار دورة السند حتى لا تنفذ عملية بترتيب خاطئ.
        /// </summary>
        private void UpdateWorkflowButtonsState()
        {
            bool hasVoucher = _selectedVoucherId > 0;
            bool isPosted = chkPosted.Checked;
            bool isApproved = _currentApprovalStatus == 2;
            bool isReviewed = _currentReviewStatus == 2;
            bool requiresApproval = checkBox2.Checked;
            bool allowedContext = !_isCrossContextVoucher;

            btnEdit.Enabled = allowedContext && hasVoucher && !isPosted && !isApproved;
            btnDelete.Enabled = allowedContext && hasVoucher && !isPosted && !isApproved;
            btnImport.Enabled = allowedContext && hasVoucher && !isPosted && !isApproved && !isReviewed;
            btnExport.Enabled = allowedContext && hasVoucher && !isPosted && !isApproved && _currentReviewStatus != 3;
            btnApprove.Enabled = allowedContext && hasVoucher && !isPosted && requiresApproval && isReviewed && !isApproved;
            btnCancelApprove.Enabled = allowedContext && hasVoucher && !isPosted && requiresApproval && isApproved;
            btnPost.Enabled = allowedContext && hasVoucher && !isPosted && isReviewed && (!requiresApproval || isApproved);
            btnUnPost.Enabled = allowedContext && hasVoucher && isPosted;
        }

        #region === فتح وقفل الحقول ===

        /// <summary>
        /// فتح أو قفل حقول الإدخال.
        /// الحقول التعريفية وحقول النظام تبقى مقفلة دائمًا.
        /// </summary>
        private void SetVoucherFieldsEditable(bool editable)
        {
            dtVoucherDate.Enabled = editable;
            // هذه شاشة سند قبض؛ نوع السند وحالته الأساسية يحددان من النظام.
            cmbVoucherType.Enabled = false;
            cmbStatus.Enabled = false;
            // الفرع يأتي من جلسة المستخدم ويُحفظ منها، لذلك لا يسمح بتغييره هنا.
            cmbBranch.Enabled = false;

            cmbParty.Enabled = editable;
            cmbCashAccount.Enabled = editable;
            cmbCurrency.Enabled = editable;
            cmbPaymentMethod.Enabled = editable;

            numAmount.ReadOnly = !editable;
            txtHeaderNotes.ReadOnly = !editable;

            dgvVoucherDetails.ReadOnly = !editable;
            dgvVoucherDetails.AllowUserToAddRows = editable;
            dgvVoucherDetails.AllowUserToDeleteRows = editable;

            // حقول محمية دائمًا
            txtVoucherNo.ReadOnly = true;
            txtJournalNo.ReadOnly = true;

            txtCreatedBy.ReadOnly = true;
            txtCreatedDate.ReadOnly = true;
            txtUpdatedBy.ReadOnly = true;
            txtUpdatedDate.ReadOnly = true;

            txtTotalAmount.ReadOnly = true;
            txtTotalForeignAmount.ReadOnly = true;
            txtDifference.ReadOnly = true;
            cmbCostCenter.Enabled = editable;
            dtReferenceDate.Enabled = editable;

            numLocalAmount.ReadOnly = true;
            numForeignAmount.ReadOnly = true;
            chkPosted.Enabled = false;
            checkBox2.Enabled = editable;
            txtReference.ReadOnly = !editable;
            txtReferenceNo.ReadOnly = !editable;
            txtAgainst.ReadOnly = !editable;

            // سعر الصرف يفتح فقط للعملة الأجنبية
            CurrencyLookupModel? currency =
                GetSelectedCurrency();

            bool foreignCurrency =
                currency != null &&
                !currency.Is_Local_Currency &&
                !string.Equals(
                    currency.Currency_Code,
                    "YER",
                    System.StringComparison.OrdinalIgnoreCase);

            numExchangeRate.ReadOnly =
                !editable || !foreignCurrency;
        }

        #endregion
    }
}
