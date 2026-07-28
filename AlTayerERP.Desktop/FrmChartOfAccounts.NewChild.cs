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
        /// تثبيت سلوك زر جديد وتطبيق قواعد دليل الحسابات دون تغيير تصميم الشاشة.
        /// </summary>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            ApplyRequiredFieldsColor();
            EnsureCategoryColumn();

            if (_newChildBehaviorRegistered)
                return;

            btnNew.Click -= btnNew_Click;
            btnNew.Click -= btnNew_CreateUnderSelectedAccount;
            btnNew.Click += btnNew_CreateUnderSelectedAccount;

            cmbParentAccount.SelectedIndexChanged -= cmbParentAccount_ApplyAutomaticMovementRule;
            cmbParentAccount.SelectedIndexChanged += cmbParentAccount_ApplyAutomaticMovementRule;
            cmbParentAccount.DropDown -= cmbParentAccount_FilterEligibleParents;
            cmbParentAccount.DropDown += cmbParentAccount_FilterEligibleParents;

            // استبدال البحث القديم ببحث يشمل التصنيف وبقية خصائص الحساب.
            btnSearch.Click -= btnSearch_Click;
            btnSearch.Click -= btnSearch_Enhanced;
            btnSearch.Click += btnSearch_Enhanced;

            tvAccounts.AfterSelect -= tvAccounts_UpdateCategoryColumn;
            tvAccounts.AfterSelect += tvAccounts_UpdateCategoryColumn;

            // الخياران يحسبهما النظام ولا يدخلهما المستخدم يدوياً.
            if (chkIsPostable != null) chkIsPostable.Enabled = false;
            if (chkIsSummaryAccount != null) chkIsSummaryAccount.Enabled = false;

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
        /// إضافة عمود التصنيف إلى جدول الحسابات الفرعية مرة واحدة.
        /// </summary>
        private void EnsureCategoryColumn()
        {
            if (dgvSubAccounts.Columns.Contains("AccountCategory"))
                return;

            dgvSubAccounts.Columns.Add("AccountCategory", "التصنيف");
        }

        /// <summary>
        /// تعرض منسدلة الحساب الأب الحسابات النشطة التجميعية غير القابلة للحركة فقط.
        /// الحسابات الفرعية النهائية التي تقبل الحركة لا تصلح كحساب أب.
        /// </summary>
        private void cmbParentAccount_FilterEligibleParents(object? sender, EventArgs e)
        {
            RefreshEligibleParentAccounts();
        }

        private void RefreshEligibleParentAccounts()
        {
            string selectedText = cmbParentAccount.Text;

            var eligibleParents = _accountsCache
                .Where(account =>
                    account.Is_Active &&
                    account.Is_Summary_Account &&
                    !account.Is_Postable &&
                    account.Account_ID != _selectedAccountId)
                .OrderBy(account => account.Account_Code)
                .ToList();

            cmbParentAccount.BeginUpdate();
            try
            {
                cmbParentAccount.Items.Clear();
                cmbParentAccount.Items.Add("بدون حساب أب (حساب رئيسي)");

                foreach (var account in eligibleParents)
                    cmbParentAccount.Items.Add($"{account.Account_Code} - {account.Account_Name_AR}");

                int previousIndex = cmbParentAccount.FindStringExact(selectedText);
                cmbParentAccount.SelectedIndex = previousIndex >= 0 ? previousIndex : 0;
            }
            finally
            {
                cmbParentAccount.EndUpdate();
            }
        }

        /// <summary>
        /// الحساب الرئيسي لا يقبل الحركة، والحساب الفرعي يقبل الحركة تلقائياً.
        /// </summary>
        private void ApplyAutomaticMovementRuleForNewAccount()
        {
            if (_selectedAccountId != null)
                return;

            bool isSubAccount = cmbParentAccount.SelectedIndex > 0;

            if (chkIsPostable != null)
                chkIsPostable.Checked = isSubAccount;

            if (chkIsSummaryAccount != null)
                chkIsSummaryAccount.Checked = !isSubAccount;
        }

        private void cmbParentAccount_ApplyAutomaticMovementRule(object? sender, EventArgs e)
        {
            ApplyAutomaticMovementRuleForNewAccount();
        }

        /// <summary>
        /// استكمال عمود التصنيف بعد تحميل أبناء الحساب المحدد.
        /// </summary>
        private void tvAccounts_UpdateCategoryColumn(object? sender, TreeViewEventArgs e)
        {
            PopulateCategoryColumn();
        }

        private void PopulateCategoryColumn()
        {
            EnsureCategoryColumn();

            foreach (DataGridViewRow row in dgvSubAccounts.Rows)
            {
                string accountCode = row.Cells["AccountCode"].Value?.ToString() ?? string.Empty;
                var account = _accountsCache.FirstOrDefault(x => x.Account_Code == accountCode);
                row.Cells["AccountCategory"].Value = account == null
                    ? string.Empty
                    : ConvertDbToCategory(account.Account_Category);
            }
        }

        /// <summary>
        /// البحث برقم واسم الحساب والنوع والتصنيف والطبيعة والعملة والحالة.
        /// </summary>
        private void btnSearch_Enhanced(object? sender, EventArgs e)
        {
            string searchKey = txtSearchTree.Text.Trim();
            if (string.IsNullOrWhiteSpace(searchKey))
            {
                BuildAccountsTree();
                dgvSubAccounts.Rows.Clear();
                return;
            }

            var filtered = _accountsCache
                .Where(account =>
                    ContainsSearch(account.Account_Code, searchKey) ||
                    ContainsSearch(account.Account_Name_AR, searchKey) ||
                    ContainsSearch(account.Account_Name_EN, searchKey) ||
                    ContainsSearch(ConvertDbToAccountType(account.Account_Type), searchKey) ||
                    ContainsSearch(ConvertDbToCategory(account.Account_Category), searchKey) ||
                    ContainsSearch(ConvertDbToNormalBalance(account.Normal_Balance), searchKey) ||
                    ContainsSearch(account.Currency_Code, searchKey) ||
                    ContainsSearch(account.Is_Active ? "نشط" : "موقوف", searchKey))
                .OrderBy(account => account.Account_Code)
                .ToList();

            tvAccounts.Nodes.Clear();
            dgvSubAccounts.Rows.Clear();
            EnsureCategoryColumn();

            foreach (var account in filtered)
            {
                tvAccounts.Nodes.Add(new TreeNode($"{account.Account_Code} - {account.Account_Name_AR}")
                {
                    Tag = account.Account_ID
                });

                dgvSubAccounts.Rows.Add(
                    account.Account_Code,
                    account.Account_Name_AR,
                    ConvertDbToAccountType(account.Account_Type),
                    ConvertDbToNormalBalance(account.Normal_Balance),
                    account.Currency_Code,
                    account.Is_Active ? "نشط" : "موقوف",
                    ConvertDbToCategory(account.Account_Category));
            }

            tvAccounts.ExpandAll();
        }

        private static bool ContainsSearch(string? value, string searchKey)
        {
            return (value ?? string.Empty).IndexOf(searchKey, StringComparison.CurrentCultureIgnoreCase) >= 0;
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

            NewAccount();
            RefreshEligibleParentAccounts();

            if (parent == null)
            {
                if (cmbParentAccount.Items.Count > 0)
                    cmbParentAccount.SelectedIndex = 0;

                txtAccountLevel.Text = "1";
                cmbParentAccount.BackColor = Color.White;
                ApplyAutomaticMovementRuleForNewAccount();

                if (lblCurrentAccount != null)
                    lblCurrentAccount.Text = "إضافة حساب رئيسي جديد - لا يقبل الحركة";

                txtAccountNameAR.Focus();
                return;
            }

            // لا يسمح باستخدام حساب نهائي قابل للحركة كحساب أب.
            if (parent.Is_Postable || !parent.Is_Summary_Account || !parent.Is_Active)
            {
                MessageBox.Show(
                    "الحساب المحدد حساب فرعي نهائي يقبل الحركة، لذلك لا يمكن إنشاء حساب تحته. اختر حساباً رئيسياً أو تجميعياً.",
                    "تنبيه دليل الحسابات",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            string parentDisplay = $"{parent.Account_Code} - {parent.Account_Name_AR}";
            int parentIndex = cmbParentAccount.FindStringExact(parentDisplay);

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
            ApplyAutomaticMovementRuleForNewAccount();

            if (lblCurrentAccount != null)
                lblCurrentAccount.Text = $"حساب فرعي جديد تحت: {parent.Account_Code} - يقبل الحركة";

            txtAccountNameAR.Focus();
        }
    }
}
