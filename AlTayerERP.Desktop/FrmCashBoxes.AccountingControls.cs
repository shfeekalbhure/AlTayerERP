using AlTayerERP.Desktop.Models;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// ضوابط محاسبية مرئية لشاشة الصناديق دون تغيير تصميمها الأساسي.
    /// </summary>
    public partial class FrmCashBoxes
    {
        private bool _accountingControlsApplied;
        private readonly ToolTip _cashBoxAccountingToolTip = new();
        private Label? _lblCurrentBalanceValue;

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ApplyCashBoxAccountingControls();
        }

        private void ApplyCashBoxAccountingControls()
        {
            if (_accountingControlsApplied || IsDisposed)
                return;

            _accountingControlsApplied = true;

            // الرصيد الافتتاحي لا يدخل أو يعدل من شاشة التعريف.
            numOpeningBalance.Enabled = false;
            numOpeningBalance.TabStop = false;
            _cashBoxAccountingToolTip.SetToolTip(
                numOpeningBalance,
                "الرصيد الافتتاحي ينشأ من مستند الأرصدة الافتتاحية أو قيد محاسبي مرحل، وليس من شاشة تعريف الصندوق.");

            _cashBoxAccountingToolTip.SetToolTip(
                cmbAccount,
                "حساب الصناديق الأب يثبت بعد إنشاء الصندوق ولا يغير من هذه الشاشة.");

            _cashBoxAccountingToolTip.SetToolTip(
                cmbCurrency,
                "يمكن تغيير العملة فقط قبل وجود أول حركة مالية مرحلة على الصندوق.");

            CreateCurrentBalanceIndicator();

            dgvCashBoxes.CellClick += ApplyAccountingLocksFromGrid;
            btnNew.Click += ResetAccountingLocksForNew;
        }

        private void CreateCurrentBalanceIndicator()
        {
            if (_lblCurrentBalanceValue != null || grpAdditionalData == null)
                return;

            _lblCurrentBalanceValue = new Label
            {
                Name = "lblCurrentBalanceValue",
                AutoSize = false,
                Text = "الرصيد الدفتري الحالي: 0.00",
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font(Font, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 78, 121),
                BackColor = Color.Transparent,
                Height = 28,
                Width = Math.Max(220, grpAdditionalData.ClientSize.Width - 36),
                Left = 18,
                Top = Math.Max(20, grpAdditionalData.ClientSize.Height - 38),
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            grpAdditionalData.Controls.Add(_lblCurrentBalanceValue);
            _lblCurrentBalanceValue.BringToFront();
        }

        private void ApplyAccountingLocksFromGrid(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 ||
                dgvCashBoxes.Rows[e.RowIndex].DataBoundItem is not CashBoxModel row)
            {
                return;
            }

            // حساب الأب تأسيسي ويقفل بعد إنشاء الصندوق.
            cmbAccount.Enabled = false;

            // العملة تقفل بعد أول حركة مرحلة، وتبقى قابلة للتعديل قبل ذلك فقط.
            cmbCurrency.Enabled = !row.Has_Posted_Movement;

            // الرصيد الافتتاحي لا يعدل مطلقاً من شاشة التعريف.
            numOpeningBalance.Enabled = false;

            if (_lblCurrentBalanceValue != null)
            {
                _lblCurrentBalanceValue.Text =
                    $"الرصيد الدفتري الحالي: {row.Current_Balance:N2} {row.Currency_Code}";

                _lblCurrentBalanceValue.ForeColor = row.Current_Balance < 0
                    ? Color.Firebrick
                    : Color.FromArgb(31, 78, 121);
            }

            if (row.Has_Posted_Movement)
            {
                _cashBoxAccountingToolTip.SetToolTip(
                    cmbCurrency,
                    "العملة مقفلة لأن الصندوق لديه حركات مالية مرحلة. أنشئ صندوقاً جديداً للعملة الأخرى.");
            }
        }

        private void ResetAccountingLocksForNew(object? sender, EventArgs e)
        {
            cmbAccount.Enabled = true;
            cmbCurrency.Enabled = true;
            numOpeningBalance.Enabled = false;
            numOpeningBalance.Value = 0;

            if (_lblCurrentBalanceValue != null)
            {
                _lblCurrentBalanceValue.Text = "الرصيد الدفتري الحالي: 0.00";
                _lblCurrentBalanceValue.ForeColor = Color.FromArgb(31, 78, 121);
            }
        }
    }
}
