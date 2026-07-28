using System;
using System.Drawing;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// ضوابط جودة واستخدام نهائية لشاشة الصناديق قبل التسليم.
    /// </summary>
    public partial class FrmCashBoxes
    {
        private readonly ErrorProvider _cashBoxErrors = new();
        private bool _deliveryReadinessApplied;

        private void ApplyCashBoxDeliveryReadiness()
        {
            if (_deliveryReadinessApplied || IsDisposed)
                return;

            _deliveryReadinessApplied = true;
            _cashBoxErrors.ContainerControl = this;
            _cashBoxErrors.BlinkStyle = ErrorBlinkStyle.NeverBlink;

            KeyPreview = true;
            KeyDown += CashBoxes_KeyDown;
            FormClosing += CashBoxes_FormClosing;

            txtCashBoxNameAR.MaxLength = 200;
            txtCashBoxNameEN.MaxLength = 200;
            txtNotes.MaxLength = 1000;
            txtSearch.MaxLength = 200;

            txtCashBoxCode.AccessibleName = "كود الصندوق";
            txtCashBoxNameAR.AccessibleName = "اسم الصندوق بالعربية";
            txtCashBoxNameEN.AccessibleName = "اسم الصندوق بالإنجليزية";
            cmbBranch.AccessibleName = "الفرع";
            cmbCurrency.AccessibleName = "عملة الصندوق";
            cmbAccount.AccessibleName = "حساب الصناديق الأب";
            numOpeningBalance.AccessibleName = "الرصيد الافتتاحي";
            numMinimumLimit.AccessibleName = "الحد الأدنى";
            numMaximumLimit.AccessibleName = "الحد الأعلى";
            txtNotes.AccessibleName = "ملاحظات الصندوق";
            dgvCashBoxes.AccessibleName = "قائمة الصناديق";

            txtCashBoxNameAR.TabIndex = 0;
            txtCashBoxNameEN.TabIndex = 1;
            cmbBranch.TabIndex = 2;
            cmbCurrency.TabIndex = 3;
            cmbAccount.TabIndex = 4;
            numMinimumLimit.TabIndex = 5;
            numMaximumLimit.TabIndex = 6;
            txtNotes.TabIndex = 7;

            ConfigureMoneyControl(numOpeningBalance);
            ConfigureMoneyControl(numMinimumLimit);
            ConfigureMoneyControl(numMaximumLimit);

            dgvCashBoxes.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dgvCashBoxes.RowTemplate.Height = 30;
            dgvCashBoxes.ColumnHeadersHeight = 36;
            dgvCashBoxes.AllowUserToResizeRows = false;
            dgvCashBoxes.StandardTab = true;
            dgvCashBoxes.DataBindingComplete += CashBoxes_DataBindingComplete;
            dgvCashBoxes.CellFormatting += CashBoxes_CellFormatting;

            txtCashBoxNameAR.TextChanged += (_, _) => ClearFieldError(txtCashBoxNameAR);
            cmbCurrency.SelectedIndexChanged += (_, _) => ClearFieldError(cmbCurrency);
            cmbAccount.SelectedIndexChanged += (_, _) => ClearFieldError(cmbAccount);
            numMinimumLimit.ValueChanged += (_, _) => ValidateLimitsInline();
            numMaximumLimit.ValueChanged += (_, _) => ValidateLimitsInline();

            AcceptButton = btnSave;
            CancelButton = btnClose;
        }

        private static void ConfigureMoneyControl(NumericUpDown control)
        {
            control.DecimalPlaces = 2;
            control.ThousandsSeparator = true;
            control.Maximum = 999999999999m;
            control.Minimum = 0m;
            control.TextAlign = HorizontalAlignment.Left;
        }

        private void CashBoxes_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.N)
            {
                btnNew.PerformClick();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.S)
            {
                btnSave.PerformClick();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F5)
            {
                btnRefresh.PerformClick();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.F)
            {
                txtSearch.Focus();
                txtSearch.SelectAll();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape && !UseWaitCursor)
            {
                Close();
                e.SuppressKeyPress = true;
            }
        }

        private void CashBoxes_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!UseWaitCursor)
                return;

            e.Cancel = true;
            MessageBox.Show(
                "انتظر حتى تكتمل العملية الحالية قبل إغلاق الشاشة.",
                "عملية قيد التنفيذ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void CashBoxes_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvCashBoxes.ClearSelection();
            dgvCashBoxes.CurrentCell = null;
        }

        private void CashBoxes_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || dgvCashBoxes.Rows[e.RowIndex].DataBoundItem is not Models.CashBoxModel row)
                return;

            if (!row.IsActive)
            {
                dgvCashBoxes.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.DimGray;
                dgvCashBoxes.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            }

            if (dgvCashBoxes.Columns[e.ColumnIndex].DataPropertyName is "Opening_Balance" or "Current_Balance" or "Max_Limit" or "Min_Limit")
            {
                e.Value = e.Value is decimal value ? value.ToString("N2") : e.Value;
                e.FormattingApplied = true;
            }
        }

        private void ValidateLimitsInline()
        {
            if (numMinimumLimit.Value <= numMaximumLimit.Value)
            {
                _cashBoxErrors.SetError(numMinimumLimit, string.Empty);
                _cashBoxErrors.SetError(numMaximumLimit, string.Empty);
                return;
            }

            const string message = "الحد الأدنى يجب ألا يتجاوز الحد الأعلى.";
            _cashBoxErrors.SetError(numMinimumLimit, message);
            _cashBoxErrors.SetError(numMaximumLimit, message);
        }

        private void ClearFieldError(Control control) =>
            _cashBoxErrors.SetError(control, string.Empty);

        private bool ValidateDeliveryFields()
        {
            _cashBoxErrors.Clear();
            var valid = true;

            if (string.IsNullOrWhiteSpace(txtCashBoxNameAR.Text))
            {
                _cashBoxErrors.SetError(txtCashBoxNameAR, "اسم الصندوق العربي مطلوب.");
                valid = false;
            }

            if (cmbCurrency.SelectedValue == null)
            {
                _cashBoxErrors.SetError(cmbCurrency, "اختر عملة الصندوق.");
                valid = false;
            }

            if (cmbAccount.SelectedValue == null)
            {
                _cashBoxErrors.SetError(cmbAccount, "اختر حساب الصناديق الأب.");
                valid = false;
            }

            if (numMinimumLimit.Value > numMaximumLimit.Value)
            {
                ValidateLimitsInline();
                valid = false;
            }

            if (!valid)
            {
                MessageBox.Show(
                    "راجع الحقول المعلّمة ثم أعد المحاولة.",
                    "بيانات غير مكتملة",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            return valid;
        }
    }
}
