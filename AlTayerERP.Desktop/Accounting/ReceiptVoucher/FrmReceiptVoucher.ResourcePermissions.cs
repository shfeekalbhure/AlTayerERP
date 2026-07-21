using AlTayerERP.Desktop.Services;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// يترجم قرارات الحقول الدقيقة إلى قيود مرئية داخل سند القبض.
    /// يبقى الخادم هو الحكم النهائي عند الحفظ أو الترحيل.
    /// </summary>
    public partial class FrmReceiptVoucher
    {
        private void ApplyFieldResourceRestrictions()
        {
            ApplyFieldRestriction("VoucherDate", dtVoucherDate);
            ApplyFieldRestriction("CashAccount", cmbCashAccount, _btnCashLookup);
            ApplyFieldRestriction("Currency", cmbCurrency);
            ApplyFieldRestriction("ExchangeRate", numExchangeRate);
            ApplyFieldRestriction("Amount", numAmount);
            ApplyFieldRestriction("AgainstText", txtAgainst);
            ApplyFieldRestriction("DetailsGrid", dgvVoucherDetails);
        }

        private static void ApplyFieldRestriction(string fieldCode, params Control[] controls)
        {
            bool hasViewDecision = CurrentSession.TryGetResourcePermission(
                "ReceiptVoucher", "FIELD", fieldCode, "VIEW", out bool canView);
            bool hasEditDecision = CurrentSession.TryGetResourcePermission(
                "ReceiptVoucher", "FIELD", fieldCode, "EDIT", out bool canEdit);

            foreach (Control control in controls)
            {
                // قرار الرؤية الصريح بالمنع يخفي المورد ولا يتركه قابلاً للوصول بلوحة المفاتيح.
                if (hasViewDecision && !canView)
                {
                    control.Visible = false;
                    control.Enabled = false;
                    control.TabStop = false;
                    continue;
                }

                // لا يعيد القرار بالسماح تمكين حقل أوقفته حالة السند؛
                // هو يمنع فقط عند وجود قرار صريح بالمنع.
                if (hasEditDecision && !canEdit)
                {
                    control.Enabled = false;
                    if (control is TextBox textBox)
                        textBox.ReadOnly = true;
                    else if (control is NumericUpDown numeric)
                        numeric.ReadOnly = true;
                    else if (control is DataGridView grid)
                    {
                        grid.ReadOnly = true;
                        grid.AllowUserToAddRows = false;
                        grid.AllowUserToDeleteRows = false;
                    }
                }
            }
        }
    }
}