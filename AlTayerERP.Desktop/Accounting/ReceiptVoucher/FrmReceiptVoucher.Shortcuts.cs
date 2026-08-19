using AlTayerERP.Desktop.Common;
using System;
using System.Linq;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// اختصارات الاستعلام في سند القبض.
    /// F9 يفتح شاشة بحث مستقلة عند الوقوف على الصندوق أو مركز التكلفة.
    /// </summary>
    public partial class FrmReceiptVoucher
    {
        private void RegisterLookupShortcutEvents()
        {
            cmbCashAccount.KeyDown -= cmbCashAccount_KeyDown;
            cmbCashAccount.KeyDown += cmbCashAccount_KeyDown;

            cmbCostCenter.KeyDown -= cmbCostCenter_KeyDown;
            cmbCostCenter.KeyDown += cmbCostCenter_KeyDown;
        }

        /// <summary>
        /// اختصارات سند القبض الموحدة: F2 حفظ، F3 جديد، F4 تعديل، F5 تحديث.
        /// F9 يبقى مخصصاً للاستعلام بحسب الحقل النشط.
        /// </summary>
        private void FrmReceiptVoucher_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2 && btnSave.Enabled)
            {
                btnSave.PerformClick();
            }
            else if (e.KeyCode == Keys.F3 && btnNew.Enabled)
            {
                btnNew.PerformClick();
            }
            else if (e.KeyCode == Keys.F4 && btnEdit.Enabled)
            {
                btnEdit.PerformClick();
            }
            else if (e.KeyCode == Keys.F5 && btnRefresh.Enabled)
            {
                btnRefresh.PerformClick();
            }
            else
            {
                return;
            }

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void cmbCashAccount_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.F9)
            {
                return;
            }

            e.Handled = true;
            e.SuppressKeyPress = true;
            OpenCashBoxLookupForm();
        }

        private void cmbCostCenter_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.F9)
            {
                return;
            }

            e.Handled = true;
            e.SuppressKeyPress = true;
            OpenCostCenterLookupForm();
        }

        private void OpenCashBoxLookupForm()
        {
            var items = _cashBoxLookups
                .Select(x => new LookupDialogItem
                {
                    Id = x.Account_ID,
                    Code = x.Cash_Box_Code,
                    Name = x.Cash_Box_Name
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Id))
                .ToList();

            if (items.Count == 0)
            {
                MessageBox.Show(
                    "لا توجد صناديق متاحة ضمن الفرع الحالي.",
                    "استعلام الصناديق",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            using var lookup = new FrmCashBoxLookup(items, cmbCashAccount.Text);
            if (lookup.ShowDialog(this) != DialogResult.OK || lookup.SelectedItem == null)
            {
                return;
            }

            cmbCashAccount.SelectedValue = lookup.SelectedItem.Id;
        }

        private void OpenCostCenterLookupForm()
        {
            var items = _costCenterLookups
                .Select(x => new LookupDialogItem
                {
                    Id = x.Cost_Center_ID,
                    Code = x.Cost_Center_Code,
                    Name = x.Cost_Center_Name_AR
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Id))
                .ToList();

            if (items.Count == 0)
            {
                MessageBox.Show(
                    "لا توجد مراكز تكلفة متاحة ضمن الشركة الحالية.",
                    "استعلام مراكز التكلفة",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            using var lookup = new FrmCostCenterLookup(items, cmbCostCenter.Text);
            if (lookup.ShowDialog(this) != DialogResult.OK || lookup.SelectedItem == null)
            {
                return;
            }

            cmbCostCenter.SelectedValue = lookup.SelectedItem.Id;
        }

        /// <summary>
        /// يفتح استعلام مركز التكلفة لسطر التفاصيل المحدد ثم يعيد القيمة المختارة للخلية.
        /// </summary>
        private void OpenCostCenterLookupForm(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgvVoucherDetails.Rows.Count)
            {
                return;
            }

            var items = _costCenterLookups
                .Select(x => new LookupDialogItem
                {
                    Id = x.Cost_Center_ID,
                    Code = x.Cost_Center_Code,
                    Name = x.Cost_Center_Name_AR
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Id))
                .ToList();

            if (items.Count == 0)
            {
                MessageBox.Show(
                    "لا توجد مراكز تكلفة متاحة ضمن الشركة الحالية.",
                    "استعلام مراكز التكلفة",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            string currentValue = Convert.ToString(
                dgvVoucherDetails.Rows[rowIndex].Cells["colCostCenter"].Value) ?? string.Empty;

            dgvVoucherDetails.EndEdit();

            using var lookup = new FrmCostCenterLookup(items, currentValue);
            if (lookup.ShowDialog(this) != DialogResult.OK || lookup.SelectedItem == null)
            {
                return;
            }

            DataGridViewRow row = dgvVoucherDetails.Rows[rowIndex];
            row.Cells["colCostCenter"].Value = lookup.SelectedItem.Id;
            dgvVoucherDetails.CurrentCell = row.Cells["colCostCenter"];
            dgvVoucherDetails.Refresh();
        }

        private void ComboCostCenter_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.F9 || dgvVoucherDetails.CurrentCell == null)
            {
                return;
            }

            e.Handled = true;
            e.SuppressKeyPress = true;
            OpenCostCenterLookupForm(dgvVoucherDetails.CurrentCell.RowIndex);
        }
    }
}
