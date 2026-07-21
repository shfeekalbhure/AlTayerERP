using AlTayerERP.Desktop.Services;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// التحكم في حالات شاشة سند القبض: عرض، جديد، تعديل،
    /// مع احترام صلاحيات الجلسة في كل حالة.
    /// </summary>
    public partial class FrmReceiptVoucher
    {
        private enum VoucherScreenMode
        {
            View,
            New,
            Edit
        }

        private VoucherScreenMode _screenMode = VoucherScreenMode.View;

        private static bool CanReceiptAction(string actionCode)
        {
            // لا يمنح أي فعل مالي حساس من التخمين. الأفعال التي لا يملك
            // جدول role_permissions حقاً مستقلاً لها تبقى مقفلة لغير مدير النظام
            // إلى أن ينفذ كتالوج الأزرار والإجراءات المعتمد.
            return actionCode switch
            {
                "VIEW" => CurrentSession.CanViewScreen("ReceiptVoucher"),
                "ADD" => CurrentSession.CanExecute("ReceiptVoucher", "ADD"),
                "EDIT" => CurrentSession.CanExecute("ReceiptVoucher", "EDIT"),
                "DELETE" => CurrentSession.CanExecute("ReceiptVoucher", "DELETE"),
                "PRINT" => CurrentSession.CanExecute("ReceiptVoucher", "PRINT"),
                "APPROVE" => CurrentSession.CanExecute("ReceiptVoucher", "APPROVE"),
                "UNAPPROVE" => CurrentSession.CanExecute("ReceiptVoucher", "UNAPPROVE"),
                _ => CurrentSession.Is_System_Admin
            };
        }

        private void SetViewMode()
        {
            _screenMode = VoucherScreenMode.View;
            SetVoucherFieldsEditable(false);

            btnNew.Enabled = CanReceiptAction("ADD");
            btnSearch.Enabled = CanReceiptAction("VIEW");
            btnRefresh.Enabled = CanReceiptAction("VIEW") && _selectedVoucherId > 0;
            btnPrint.Enabled = CanReceiptAction("PRINT") && _selectedVoucherId > 0;
            btnViewJournalEntry.Enabled = CanReceiptAction("VIEW") && _selectedVoucherId > 0;

            btnSave.Enabled = false;
            btnUndo.Enabled = false;

            UpdateWorkflowButtonsState();
        }

        private void SetNewMode()
        {
            if (!CanReceiptAction("ADD"))
            {
                SetViewMode();
                return;
            }

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

        private void SetEditMode()
        {
            if (!CanReceiptAction("EDIT"))
            {
                MessageBox.Show(
                    "ليس لديك صلاحية تعديل سند القبض.",
                    "رفض الوصول",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

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
                    "لا يمكن تعديل سند مرحل.
يجب إلغاء الترحيل أولًا.",
                    "السند مرحل",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (_currentApprovalStatus == 2)
            {
                MessageBox.Show(
                    "لا يمكن تعديل سند معتمد.
يجب إلغاء الاعتماد أولًا.",
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

        private void UpdateWorkflowButtonsState()
        {
            bool hasVoucher = _selectedVoucherId > 0;
            bool isPosted = chkPosted.Checked;
            bool isApproved = _currentApprovalStatus == 2;
            bool isReviewed = _currentReviewStatus == 2;
            bool requiresApproval = checkBox2.Checked;
            bool allowedContext = !_isCrossContextVoucher;

            btnEdit.Enabled = CanReceiptAction("EDIT") &&
                              allowedContext && hasVoucher && !isPosted && !isApproved;
            btnDelete.Enabled = CanReceiptAction("DELETE") &&
                                allowedContext && hasVoucher && !isPosted && !isApproved;
            btnImport.Enabled = CanReceiptAction("REVIEW") &&
                                allowedContext && hasVoucher && !isPosted && !isApproved && !isReviewed;
            btnExport.Enabled = CanReceiptAction("RETURN_CORRECTION") &&
                                allowedContext && hasVoucher && !isPosted && !isApproved && _currentReviewStatus != 3;
            btnApprove.Enabled = CanReceiptAction("APPROVE") &&
                                 allowedContext && hasVoucher && !isPosted && requiresApproval && isReviewed && !isApproved;
            btnCancelApprove.Enabled = CanReceiptAction("UNAPPROVE") &&
                                       allowedContext && hasVoucher && !isPosted && requiresApproval && isApproved;
            btnPost.Enabled = CanReceiptAction("POST") &&
                              allowedContext && hasVoucher && !isPosted && isReviewed && (!requiresApproval || isApproved);
            btnUnPost.Enabled = CanReceiptAction("UNPOST") &&
                                allowedContext && hasVoucher && isPosted;
        }

        private void SetVoucherFieldsEditable(bool editable)
        {
            bool canEdit = editable && (_screenMode == VoucherScreenMode.New
                ? CanReceiptAction("ADD")
                : CanReceiptAction("EDIT"));

            dtVoucherDate.Enabled = canEdit;
            cmbVoucherType.Enabled = false;
            cmbStatus.Enabled = false;
            cmbBranch.Enabled = false;

            cmbParty.Enabled = canEdit;
            cmbCashAccount.Enabled = canEdit;
            cmbCurrency.Enabled = canEdit;
            cmbPaymentMethod.Enabled = canEdit;

            numAmount.ReadOnly = !canEdit;
            txtHeaderNotes.ReadOnly = !canEdit;

            dgvVoucherDetails.ReadOnly = !canEdit;
            dgvVoucherDetails.AllowUserToAddRows = canEdit;
            dgvVoucherDetails.AllowUserToDeleteRows = canEdit;

            txtVoucherNo.ReadOnly = true;
            txtJournalNo.ReadOnly = true;
            txtCreatedBy.ReadOnly = true;
            txtCreatedDate.ReadOnly = true;
            txtUpdatedBy.ReadOnly = true;
            txtUpdatedDate.ReadOnly = true;
            txtTotalAmount.ReadOnly = true;
            txtTotalForeignAmount.ReadOnly = true;
            txtDifference.ReadOnly = true;
            cmbCostCenter.Enabled = canEdit;
            dtReferenceDate.Enabled = canEdit;

            numLocalAmount.ReadOnly = true;
            numForeignAmount.ReadOnly = true;
            chkPosted.Enabled = false;
            checkBox2.Enabled = canEdit;
            txtReference.ReadOnly = !canEdit;
            txtReferenceNo.ReadOnly = !canEdit;
            txtAgainst.ReadOnly = !canEdit;

            CurrencyLookupModel? currency = GetSelectedCurrency();
            bool foreignCurrency = currency != null &&
                                   !currency.Is_Local_Currency &&
                                   !string.Equals(
                                       currency.Currency_Code,
                                       "YER",
                                       System.StringComparison.OrdinalIgnoreCase);

            numExchangeRate.ReadOnly = !canEdit || !foreignCurrency;
        }
    }
}