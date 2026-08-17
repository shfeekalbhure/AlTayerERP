using System;
using System.Linq;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// دعم ترقيم طلبات الصرف داخل شاشة إعدادات الترقيم.
    /// يظهر الاسم بالعربية للمستخدم ويحفظ بالكود البرمجي PAYMENT_REQUEST.
    /// </summary>
    public partial class FrmNumberingSettings
    {
        private const string PaymentRequestArabic = "طلب صرف";
        private const string PaymentRequestCode = "PAYMENT_REQUEST";
        private bool _paymentRequestNumberingHooksApplied;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (_paymentRequestNumberingHooksApplied)
                return;

            _paymentRequestNumberingHooksApplied = true;

            btnSave.Click -= btnSave_Click;
            btnSave.Click += PaymentRequestSave_Click;

            btnEdit.Click -= btnEdit_Click;
            btnEdit.Click += PaymentRequestEdit_Click;

            cmbDocumentType.SelectedIndexChanged += PaymentRequestDocumentType_SelectedIndexChanged;
            dgvNumberingSettings.DataBindingComplete += NumberingGrid_DataBindingComplete;
            btnNew.Click += (_, _) => EnsurePaymentRequestTypeAvailable();
            btnRefresh.Click += (_, _) => BeginInvoke(new Action(EnsurePaymentRequestTypeAvailable));

            NormalizePaymentRequestGridText();
            EnsurePaymentRequestTypeAvailable();
        }

        private void PaymentRequestDocumentType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (!string.Equals(cmbDocumentType.Text, PaymentRequestArabic, StringComparison.Ordinal))
                return;

            txtPrefix.Text = "PRQ";
            numDigitsCount.Value = 6;
            cmbResetType.Text = "حسب الفرع والسنة";
            chkUseCompany.Checked = false;
            chkUseBranch.Checked = true;
            chkUseYear.Checked = true;
            chkIsActive.Checked = true;
        }

        private void NumberingGrid_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            NormalizePaymentRequestGridText();
            EnsurePaymentRequestTypeAvailable();
        }

        private void NormalizePaymentRequestGridText()
        {
            if (!dgvNumberingSettings.Columns.Contains("Document_Type"))
                return;

            foreach (DataGridViewRow row in dgvNumberingSettings.Rows)
            {
                if (row.IsNewRow)
                    continue;

                var value = row.Cells["Document_Type"].Value?.ToString();
                if (string.Equals(value, PaymentRequestCode, StringComparison.OrdinalIgnoreCase))
                    row.Cells["Document_Type"].Value = PaymentRequestArabic;
            }
        }

        private void EnsurePaymentRequestTypeAvailable()
        {
            var alreadyConfigured = dgvNumberingSettings.Columns.Contains("Document_Type") &&
                dgvNumberingSettings.Rows.Cast<DataGridViewRow>()
                    .Where(row => !row.IsNewRow)
                    .Select(row => row.Cells["Document_Type"].Value?.ToString())
                    .Any(value =>
                        string.Equals(value, PaymentRequestArabic, StringComparison.Ordinal) ||
                        string.Equals(value, PaymentRequestCode, StringComparison.OrdinalIgnoreCase));

            if (alreadyConfigured)
            {
                RemoveComboItem(PaymentRequestArabic);
                RemoveComboItem(PaymentRequestCode);
                return;
            }

            if (!cmbDocumentType.Items.Cast<object>()
                    .Any(item => string.Equals(item?.ToString(), PaymentRequestArabic, StringComparison.Ordinal)))
            {
                cmbDocumentType.Items.Add(PaymentRequestArabic);
            }
        }

        private void RemoveComboItem(string value)
        {
            var item = cmbDocumentType.Items.Cast<object>()
                .FirstOrDefault(x => string.Equals(x?.ToString(), value, StringComparison.OrdinalIgnoreCase));
            if (item != null)
                cmbDocumentType.Items.Remove(item);
        }

        private void PaymentRequestSave_Click(object? sender, EventArgs e)
        {
            ExecuteWithPaymentRequestCode(() => btnSave_Click(sender!, e));
        }

        private void PaymentRequestEdit_Click(object? sender, EventArgs e)
        {
            ExecuteWithPaymentRequestCode(() => btnEdit_Click(sender!, e));
        }

        private void ExecuteWithPaymentRequestCode(Action action)
        {
            if (!string.Equals(cmbDocumentType.Text, PaymentRequestArabic, StringComparison.Ordinal))
            {
                action();
                return;
            }

            var prefix = txtPrefix.Text;
            var digits = numDigitsCount.Value;
            var resetType = cmbResetType.Text;
            var useCompany = chkUseCompany.Checked;
            var useBranch = chkUseBranch.Checked;
            var useYear = chkUseYear.Checked;
            var active = chkIsActive.Checked;

            cmbDocumentType.Text = PaymentRequestCode;
            action();

            cmbDocumentType.Text = PaymentRequestArabic;
            txtPrefix.Text = prefix;
            numDigitsCount.Value = digits;
            cmbResetType.Text = resetType;
            chkUseCompany.Checked = useCompany;
            chkUseBranch.Checked = useBranch;
            chkUseYear.Checked = useYear;
            chkIsActive.Checked = active;
        }
    }
}
