using System;
using System.Linq;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    public partial class FrmChartOfAccounts
    {
        private bool _newChildBehaviorRegistered;

        /// <summary>
        /// تسجيل سلوك إنشاء حساب جديد تحت الحساب المحدد بعد اكتمال تهيئة الشاشة.
        /// لا يغيّر التصميم ولا يلغي إمكانية اختيار حساب أب آخر يدوياً.
        /// </summary>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (_newChildBehaviorRegistered)
                return;

            // يسجل بعد الحدث الأصلي حتى يتم أولاً تفريغ الحقول، ثم تثبيت الحساب المحدد كأب.
            btnNew.Click += btnNew_CreateUnderSelectedAccount;
            _newChildBehaviorRegistered = true;
        }

        private void btnNew_CreateUnderSelectedAccount(object? sender, EventArgs e)
        {
            TreeNode? selectedNode = tvAccounts.SelectedNode;
            if (selectedNode?.Tag == null)
            {
                // لا يوجد حساب محدد: يبقى الحساب الجديد رئيسياً من المستوى الأول.
                if (cmbParentAccount.Items.Count > 0)
                    cmbParentAccount.SelectedIndex = 0;

                txtAccountLevel.Text = "1";
                if (lblCurrentAccount != null)
                    lblCurrentAccount.Text = "إضافة حساب رئيسي جديد";
                return;
            }

            string selectedAccountId = selectedNode.Tag.ToString() ?? string.Empty;
            var parent = _accountsCache.FirstOrDefault(x => x.Account_ID == selectedAccountId);
            if (parent == null)
                return;

            string parentDisplay = $"{parent.Account_Code} - {parent.Account_Name_AR}";
            int parentIndex = cmbParentAccount.FindStringExact(parentDisplay);
            if (parentIndex >= 0)
                cmbParentAccount.SelectedIndex = parentIndex;
            else
                cmbParentAccount.Text = parentDisplay;

            // تثبيت المستوى والخصائص المحاسبية الموروثة من الحساب الأب.
            txtAccountLevel.Text = (parent.Account_Level + 1).ToString();
            cmbAccountType.Text = ConvertDbToAccountType(parent.Account_Type);
            cmbNormalBalance.Text = ConvertDbToNormalBalance(parent.Normal_Balance);
            cmbCurrency.Text = parent.Currency_Code ?? string.Empty;
            cmbAccountCategory.Text = ConvertDbToCategory(parent.Account_Category);

            if (lblCurrentAccount != null)
                lblCurrentAccount.Text = $"إضافة حساب فرعي تحت: {parent.Account_Code} - {parent.Account_Name_AR}";

            txtAccountNameAR.Focus();
        }
    }
}
