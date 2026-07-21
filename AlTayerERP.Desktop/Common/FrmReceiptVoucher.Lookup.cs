using System;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// اعتراض اختصار F9 داخل جدول سند القبض وتحويله إلى شاشة الاستعلام الموحدة.
    /// </summary>
    public partial class FrmReceiptVoucher
    {
        /// <summary>
        /// يدعم F9 حتى عندما تكون خلية رقم الحساب أو اسم الحساب في وضع التحرير.
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F9 &&
                IsFocusInsideVoucherGrid() &&
                IsCurrentAccountColumn() &&
                dgvVoucherDetails.CurrentCell != null)
            {
                OpenAccountLookupForm(dgvVoucherDetails.CurrentCell.RowIndex);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private bool IsFocusInsideVoucherGrid()
        {
            if (dgvVoucherDetails.Focused || dgvVoucherDetails.ContainsFocus)
            {
                return true;
            }

            Control? editingControl = dgvVoucherDetails.EditingControl;
            return editingControl != null && editingControl.ContainsFocus;
        }

        private bool IsCurrentAccountColumn()
        {
            if (dgvVoucherDetails.CurrentCell?.OwningColumn == null)
            {
                return false;
            }

            DataGridViewColumn column = dgvVoucherDetails.CurrentCell.OwningColumn;
            string columnName = column.Name?.Trim() ?? string.Empty;
            string headerText = column.HeaderText?.Trim() ?? string.Empty;

            return columnName.Equals("colAccountCode", StringComparison.OrdinalIgnoreCase) ||
                   columnName.Equals("colAccountName", StringComparison.OrdinalIgnoreCase) ||
                   headerText.Equals("رقم الحساب", StringComparison.OrdinalIgnoreCase) ||
                   headerText.Equals("اسم الحساب", StringComparison.OrdinalIgnoreCase);
        }
    }
}
