using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    public partial class FrmChartOfAccounts
    {
        private bool _newChildBehaviorRegistered;
        private static readonly Color RequiredFieldColor = Color.FromArgb(255, 252, 220);

        /// <summary>
        /// تثبيت سلوك زر جديد وتمييز الحقول الإلزامية دون تغيير التصميم.
        /// </summary>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            ApplyRequiredFieldsColor();

            if (_newChildBehaviorRegistered)
                return;

            // استبدال الحدث القديم بحدث واحد واضح حتى لا يعتمد التنفيذ على ترتيب حدثين.
            btnNew.Click -= btnNew_Click;
            btnNew.Click -= btnNew_CreateUnderSelectedAccount;
            btnNew.Click += btnNew_CreateUnderSelectedAccount;
            _newChildBehaviorRegistered = true;
        }

        private void ApplyRequiredFieldsColor()
        {
            txtAccountNameAR.BackColor = RequiredFieldColor;
            cmbAccountType.BackColor = RequiredFieldColor;
            cmbNormalBalance.BackColor = RequiredFieldColor;
            cmbCurrency.BackColor = RequiredFieldColor;
            cmbAccountStatus.BackColor = RequiredFieldColor;
        }

        /// <summary>
        /// إنشاء حساب رئيسي عند عدم تحديد حساب، أو حساب فرعي تحت المحدد في الشجرة.
        /// </summary>
        private void btnNew_CreateUnderSelectedAccount(object? sender, EventArgs e)
        {
            string selectedAccountId = tvAccounts.SelectedNode?.Tag?.ToString() ?? string.Empty;
            var parent = string.IsNullOrWhiteSpace(selectedAccountId)
                ? null
                : _accountsCache.FirstOrDefault(x => x.Account_ID == selectedAccountId);

            // تفريغ الحقول أولاً مع الاحتفاظ بالحساب المحدد في متغير محلي.
            NewAccount();

            if (parent == null)
            {
                if (cmbParentAccount.Items.Count > 0)
                    cmbParentAccount.SelectedIndex = 0;

                txtAccountLevel.Text = "1";
                cmbParentAccount.BackColor = Color.White;

                if (lblCurrentAccount != null)
                    lblCurrentAccount.Text = "إضافة حساب رئيسي جديد";

                txtAccountNameAR.Focus();
                return;
            }

            string parentDisplay = $"{parent.Account_Code} - {parent.Account_Name_AR}";
            int parentIndex = cmbParentAccount.FindStringExact(parentDisplay);

            if (parentIndex < 0)
            {
                cmbParentAccount.Items.Add(parentDisplay);
                parentIndex = cmbParentAccount.FindStringExact(parentDisplay);
            }

            if (parentIndex >= 0)
                cmbParentAccount.SelectedIndex = parentIndex;

            cmbParentAccount.BackColor = RequiredFieldColor;
            txtAccountLevel.Text = (parent.Account_Level + 1).ToString();
            cmbAccountType.Text = ConvertDbToAccountType(parent.Account_Type);
            cmbNormalBalance.Text = ConvertDbToNormalBalance(parent.Normal_Balance);
            cmbCurrency.Text = parent.Currency_Code ?? string.Empty;
            cmbAccountCategory.Text = ConvertDbToCategory(parent.Account_Category);

            _selectedAccountId = null;
            txtAccountCode.Clear();

            if (lblCurrentAccount != null)
                lblCurrentAccount.Text = $"حساب جديد تحت: {parent.Account_Code} - {parent.Account_Name_AR}";

            txtAccountNameAR.Focus();
        }
    }
}
