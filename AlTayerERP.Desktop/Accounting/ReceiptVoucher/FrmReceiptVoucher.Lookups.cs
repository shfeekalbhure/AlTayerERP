using AlTayerERP.Desktop.Common;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// اختصارات F9 للمراجع المحاسبية في سند القبض.
    /// </summary>
    public partial class FrmReceiptVoucher
    {
        private readonly Button _btnCashLookup = CreateLookupButton();
        private readonly Button _btnCostCenterLookup = CreateLookupButton();

        /// <summary>
        /// يضيف زري البحث للماوس بجانب الحقول التي تدعم اختصار F9.
        /// </summary>
        private void ConfigureReferenceLookupButtons()
        {
            AttachLookupButton(cmbCashAccount, _btnCashLookup, OpenCashBoxLookup);
            AttachLookupButton(cmbCostCenter, _btnCostCenterLookup, OpenCostCenterLookup);
        }

        private static Button CreateLookupButton()
        {
            return new Button
            {
                Text = "…",
                Width = 26,
                Height = 28,
                FlatStyle = FlatStyle.System,
                TabStop = false,
                AccessibleName = "فتح شاشة البحث"
            };
        }

        private static void AttachLookupButton(
            ComboBox combo,
            Button button,
            Action openLookup)
        {
            if (combo.Parent == null)
                return;

            if (button.Parent != combo.Parent)
                combo.Parent.Controls.Add(button);

            button.Location = new Point(combo.Left - button.Width - 3, combo.Top);
            button.Click -= LookupButtonClick;
            button.Click += LookupButtonClick;

            void LookupButtonClick(object? sender, EventArgs e)
            {
                openLookup();
            }
        }

        private void FrmReceiptVoucher_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.F9)
                return;

            if (cmbCashAccount.Focused)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                OpenCashBoxLookup();
                return;
            }

            if (cmbCostCenter.Focused)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                OpenCostCenterLookup();
                return;
            }

            if (dgvVoucherDetails.ContainsFocus &&
                dgvVoucherDetails.CurrentCell != null &&
                dgvVoucherDetails.CurrentCell.RowIndex >= 0 &&
                dgvVoucherDetails.CurrentCell.OwningColumn?.Name == colCostCenter.Name)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                OpenGridCostCenterLookup(dgvVoucherDetails.CurrentCell.RowIndex);
            }
        }

        /// <summary>
        /// يفتح شاشة الصناديق والبنوك ثم يعيد الحساب المرتبط بالصندوق إلى ترويسة السند.
        /// </summary>
        private void OpenCashBoxLookup()
        {
            if (_cashBoxLookups.Count == 0)
            {
                MessageBox.Show("لا توجد صناديق أو بنوك متاحة في هذا الفرع.", "الصناديق",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string currentValue = cmbCashAccount.SelectedValue?.ToString() ?? string.Empty;
            using var lookup = new FrmReferenceLookup(
                "اختيار الصندوق أو البنك",
                _cashBoxLookups.Select(x => new ReferenceLookupItem
                {
                    Id = x.Account_ID,
                    Code = x.Cash_Box_Code,
                    Name = x.Cash_Box_Name
                }),
                currentValue);

            if (lookup.ShowDialog(this) == DialogResult.OK)
                cmbCashAccount.SelectedValue = lookup.SelectedId;
        }

        /// <summary>
        /// يفتح شاشة مراكز التكلفة لتحديد مركز الترويسة.
        /// </summary>
        private void OpenCostCenterLookup()
        {
            if (_costCenterLookups.Count == 0)
            {
                MessageBox.Show("لا توجد مراكز تكلفة متاحة.", "مراكز التكلفة",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string currentValue = cmbCostCenter.SelectedValue?.ToString() ?? string.Empty;
            using var lookup = CreateCostCenterLookup(currentValue);

            if (lookup.ShowDialog(this) == DialogResult.OK)
                cmbCostCenter.SelectedValue = lookup.SelectedId;
        }

        /// <summary>
        /// يفتح شاشة مراكز التكلفة لسطر التفاصيل الذي يقف عليه المستخدم.
        /// </summary>
        private void OpenGridCostCenterLookup(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgvVoucherDetails.Rows.Count)
                return;

            if (_costCenterLookups.Count == 0)
            {
                MessageBox.Show("لا توجد مراكز تكلفة متاحة.", "مراكز التكلفة",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var row = dgvVoucherDetails.Rows[rowIndex];
            string currentValue = row.Cells[colCostCenter.Name].Value?.ToString() ?? string.Empty;
            using var lookup = CreateCostCenterLookup(currentValue);

            if (lookup.ShowDialog(this) != DialogResult.OK)
                return;

            row.Cells[colCostCenter.Name].Value = lookup.SelectedId;
            dgvVoucherDetails.EndEdit();
            dgvVoucherDetails.CurrentCell = row.Cells[colCostCenter.Name];
            dgvVoucherDetails.Refresh();
        }

        private FrmReferenceLookup CreateCostCenterLookup(string initialSearch)
        {
            return new FrmReferenceLookup(
                "اختيار مركز التكلفة",
                _costCenterLookups.Select(x => new ReferenceLookupItem
                {
                    Id = x.Cost_Center_ID,
                    Code = x.Cost_Center_Code,
                    Name = x.Cost_Center_Name_AR
                }),
                initialSearch);
        }
    }
}