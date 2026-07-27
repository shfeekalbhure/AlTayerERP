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
        /// تسجيل سلوك إنشاء حساب جديد تحت الحساب المحدد وتمييز الحقول الإلزامية.
        /// </summary>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            ApplyRequiredFieldsColor();

            if (_newChildBehaviorRegistered)
                return;

            // يسجل بعد الحدث الأصلي حتى يتم تفريغ الحقول أولاً ثم تثبيت الحساب المحدد كأب.
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

        private void btnNew_CreateUnderSelectedAccount(object? sender, EventArgs e)
        {
            TreeNode? selectedNode = tvAccounts.SelectedNode;
            if (selectedNode?.Tag == null)
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

            string selectedAccountId = selectedNode.Tag.ToString() ?? string.Empty;
            var parent = _accountsCache.FirstOrDefault(x => x.Account_ID == selectedAccountId);
            if (parent == null)
            {
                MessageBox.Show("تعذر تحديد الحساب الأب من الشجرة. أعد تحديد الحساب ثم اضغط جديد.",
                    "دليل الحسابات", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string parentDisplay = $"{parent.Account_Code} - {parent.Account_Name_AR}";
            int parentIndex = cmbParentAccount.FindStringExact(parentDisplay);

            if (parentIndex >= 0)
                cmbParentAccount.SelectedIndex = parentIndex;
            else
            {
                cmbParentAccount.Items.Add(parentDisplay);
                cmbParentAccount.SelectedItem = parentDisplay;
            }

            cmbParentAccount.BackColor = RequiredFieldColor;
            txtAccountLevel.Text = (parent.Account_Level + 1).ToString();
            cmbAccountType.Text = ConvertDbToAccountType(parent.Account_Type);
            cmbNormalBalance.Text = ConvertDbToNormalBalance(parent.Normal_Balance);
            cmbCurrency.Text = parent.Currency_Code ?? string.Empty;
            cmbAccountCategory.Text = ConvertDbToCategory(parent.Account_Category);

            // يمنع اعتبار الحساب الأب نفسه هو الحساب الجاري المراد تعديله.
            _selectedAccountId = null;
            txtAccountCode.Clear();

            if (lblCurrentAccount != null)
                lblCurrentAccount.Text = $"حساب جديد تحت: {parent.Account_Code} - {parent.Account_Name_AR}";

            txtAccountNameAR.Focus();
        }
    }
}
