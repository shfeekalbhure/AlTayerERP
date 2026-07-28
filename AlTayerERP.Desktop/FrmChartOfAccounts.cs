using AlTayerERP.Desktop.Models;
using AlTayerERP.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    public partial class FrmChartOfAccounts : Form
    {
        private readonly HttpClient _client = ApiService.Client;
        private readonly string _baseUrl = ApiService.BaseUrl;
        private string CompanyId => CurrentSession.Company_ID;

        private string? _selectedAccountId;
        private List<AccountModel> _accountsCache = new();
        private List<AccountCategoryLookupModel> _categoriesCache = new();
        private bool _loadingControls;

        private readonly PrintDocument printDocument = new();
        private readonly PrintPreviewDialog printPreviewDialog = new();
        private int _currentPrintIndex;

        public FrmChartOfAccounts()
        {
            InitializeComponent();
            RegisterEvents();
            printDocument.PrintPage += PrintDocument_PrintPage;
        }

        private void RegisterEvents()
        {
            Load += FrmChartOfAccounts_Load;
            tvAccounts.AfterSelect += tvAccounts_AfterSelect;
            dgvSubAccounts.CellDoubleClick += dgvSubAccounts_CellDoubleClick;
            btnNew.Click += btnNew_Click;
            btnSave.Click += btnSave_Click;
            btnEdit.Click += btnEdit_Click;
            btnDelete.Click += btnDelete_Click;
            btnRefresh.Click += btnRefresh_Click;
            btnSearch.Click += btnSearch_Click;
            btnPrint.Click += btnPrint_Click;
            btnClose.Click += btnClose_Click;
            txtSearchTree.TextChanged += txtSearchTree_TextChanged;
            cmbParentAccount.SelectedIndexChanged += cmbParentAccount_SelectedIndexChanged;
            cmbAccountType.SelectedIndexChanged += cmbAccountType_SelectedIndexChanged;
        }

        private async void FrmChartOfAccounts_Load(object? sender, EventArgs e)
        {
            FillStaticCombos();
            SetupGrid();
            await LoadCategoriesAsync();
            await LoadAccountsDataFromServerAsync();
            NewAccount();
            UpdateStatusBar();
        }

        private void FillStaticCombos()
        {
            cmbAccountType.Items.Clear();
            cmbAccountType.Items.AddRange(new object[] { "أصل", "خصم", "حقوق ملكية", "إيراد", "مصروف" });

            cmbNormalBalance.Items.Clear();
            cmbNormalBalance.Items.AddRange(new object[] { "مدين", "دائن" });
            cmbNormalBalance.Enabled = false;

            cmbCurrency.Items.Clear();
            cmbCurrency.Items.AddRange(new object[] { "YER", "SAR", "USD" });

            cmbAccountStatus.Items.Clear();
            cmbAccountStatus.Items.AddRange(new object[] { "نشط", "موقوف" });
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                _categoriesCache = await _client.GetFromJsonAsync<List<AccountCategoryLookupModel>>(
                    $"{_baseUrl}AccountCategories?activeOnly=true") ?? new();
            }
            catch (Exception ex)
            {
                _categoriesCache = new();
                MessageBox.Show($"تعذر جلب تصنيفات الحسابات من الخادم: {ex.Message}",
                    "تصنيفات الحسابات", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BindCategoriesForCurrentType(string? selectedCode = null)
        {
            string accountType = ConvertAccountTypeToDb(cmbAccountType.Text.Trim());
            var rows = _categoriesCache
                .Where(x => string.Equals(x.Account_Type, accountType, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.Sort_Order)
                .ThenBy(x => x.Category_Name_AR)
                .ToList();

            _loadingControls = true;
            cmbAccountCategory.DataSource = null;
            cmbAccountCategory.DisplayMember = nameof(AccountCategoryLookupModel.Category_Name_AR);
            cmbAccountCategory.ValueMember = nameof(AccountCategoryLookupModel.Category_Code);
            cmbAccountCategory.DataSource = rows;

            if (!string.IsNullOrWhiteSpace(selectedCode))
                cmbAccountCategory.SelectedValue = selectedCode;
            else if (rows.Count > 0)
                cmbAccountCategory.SelectedIndex = 0;

            string normalBalance = rows.FirstOrDefault(x => x.Category_Code == (cmbAccountCategory.SelectedValue?.ToString()))?.Normal_Balance
                ?? DefaultBalanceForType(accountType);
            cmbNormalBalance.Text = ConvertDbToNormalBalance(normalBalance);
            _loadingControls = false;
        }

        private void SetupGrid()
        {
            dgvSubAccounts.Columns.Clear();
            dgvSubAccounts.AllowUserToAddRows = false;
            dgvSubAccounts.ReadOnly = true;
            dgvSubAccounts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSubAccounts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvSubAccounts.Columns.Add("AccountCode", "رقم الحساب");
            dgvSubAccounts.Columns.Add("AccountNameAR", "اسم الحساب العربي");
            dgvSubAccounts.Columns.Add("AccountType", "نوع الحساب");
            dgvSubAccounts.Columns.Add("AccountCategory", "التصنيف");
            dgvSubAccounts.Columns.Add("NormalBalance", "طبيعة الحساب");
            dgvSubAccounts.Columns.Add("Currency", "العملة");
            dgvSubAccounts.Columns.Add("Status", "الحالة");
        }

        private async Task LoadAccountsDataFromServerAsync()
        {
            if (string.IsNullOrWhiteSpace(CompanyId))
            {
                MessageBox.Show("لم يتم تحديد شركة للجلسة الحالية. يرجى إعادة تسجيل الدخول.",
                    "تنبيه النظام", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _accountsCache = await _client.GetFromJsonAsync<List<AccountModel>>(
                    $"{_baseUrl}ChartOfAccounts") ?? new();
                BuildAccountsTree();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ أثناء جلب دليل الحسابات: {ex.Message}",
                    "خطأ شبكة", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BuildAccountsTree()
        {
            tvAccounts.Nodes.Clear();
            cmbParentAccount.Items.Clear();
            cmbParentAccount.Items.Add("بدون حساب أب (حساب رئيسي)");

            foreach (var account in _accountsCache
                         .Where(a => string.IsNullOrWhiteSpace(a.Parent_Account_ID))
                         .OrderBy(a => a.Account_Code))
            {
                var node = new TreeNode($"{account.Account_Code} - {account.Account_Name_AR}")
                {
                    Tag = account.Account_ID
                };
                AddChildNodes(node, account.Account_ID);
                tvAccounts.Nodes.Add(node);
            }

            // الحساب الأب يجب أن يكون نشطاً وتجميعياً وغير قابل للحركة.
            foreach (var parent in _accountsCache
                         .Where(a => a.Is_Active && a.Is_Summary_Account && !a.Is_Postable &&
                                     a.Account_ID != _selectedAccountId)
                         .OrderBy(a => a.Account_Code))
            {
                cmbParentAccount.Items.Add($"{parent.Account_Code} - {parent.Account_Name_AR}");
            }

            tvAccounts.ExpandAll();
            UpdateStatusBar();
        }

        private void AddChildNodes(TreeNode parentNode, string parentId)
        {
            foreach (var child in _accountsCache
                         .Where(a => a.Parent_Account_ID == parentId)
                         .OrderBy(a => a.Account_Code))
            {
                var childNode = new TreeNode($"{child.Account_Code} - {child.Account_Name_AR}")
                {
                    Tag = child.Account_ID
                };
                AddChildNodes(childNode, child.Account_ID);
                parentNode.Nodes.Add(childNode);
            }
        }

        private void tvAccounts_AfterSelect(object? sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag == null) return;
            FillAccountToFields(e.Node.Tag.ToString()!);
        }

        private void FillAccountToFields(string accountId)
        {
            var account = _accountsCache.FirstOrDefault(a => a.Account_ID == accountId);
            if (account == null) return;

            _loadingControls = true;
            _selectedAccountId = account.Account_ID;
            txtAccountCode.Text = account.Account_Code;
            txtAccountNameAR.Text = account.Account_Name_AR;
            txtAccountNameEN.Text = account.Account_Name_EN ?? string.Empty;
            txtAccountLevel.Text = account.Account_Level.ToString();
            txtNotes.Text = account.Notes ?? string.Empty;

            cmbAccountType.Text = ConvertDbToAccountType(account.Account_Type);
            BindCategoriesForCurrentType(account.Account_Category);
            cmbNormalBalance.Text = ConvertDbToNormalBalance(account.Normal_Balance);
            cmbCurrency.Text = account.Currency_Code ?? string.Empty;
            cmbAccountStatus.Text = account.Is_Active ? "نشط" : "موقوف";

            if (!string.IsNullOrWhiteSpace(account.Parent_Account_ID))
            {
                var parent = _accountsCache.FirstOrDefault(p => p.Account_ID == account.Parent_Account_ID);
                cmbParentAccount.Text = parent == null
                    ? "بدون حساب أب (حساب رئيسي)"
                    : $"{parent.Account_Code} - {parent.Account_Name_AR}";
            }
            else
            {
                cmbParentAccount.SelectedIndex = 0;
            }

            chkIsPostable.Checked = account.Is_Postable;
            chkAllowManualEntry.Checked = account.Allow_ManualEntry;
            chkSystemAccount.Checked = account.System_Account;
            chkRequiresParty.Checked = account.Requires_Party;
            chkRequiresCostCenter.Checked = account.Requires_CostCenter;
            chkRequiresProject.Checked = account.Requires_Project;
            chkMultiCurrency.Checked = account.Multi_Currency;
            chkAffectsBalanceSheet.Checked = account.Affects_Balance_Sheet;
            chkAffectsIncomeStatement.Checked = account.Affects_Income_Statement;
            chkIsSummaryAccount.Checked = account.Is_Summary_Account;

            chkIsPostable.Enabled = false;
            chkIsSummaryAccount.Enabled = false;

            lblCurrentAccount.Text = "الحساب الحالي: " + account.Account_Code;
            LoadSubAccountsToGrid(account.Account_ID);
            BuildAccountsTree();
            _loadingControls = false;
        }

        private void dgvSubAccounts_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            string? accountCode = dgvSubAccounts.Rows[e.RowIndex].Cells["AccountCode"].Value?.ToString();
            var account = _accountsCache.FirstOrDefault(a => a.Account_Code == accountCode);
            if (account != null) FillAccountToFields(account.Account_ID);
        }

        private void LoadSubAccountsToGrid(string parentId)
        {
            dgvSubAccounts.Rows.Clear();
            foreach (var sub in _accountsCache.Where(a => a.Parent_Account_ID == parentId).OrderBy(a => a.Account_Code))
            {
                dgvSubAccounts.Rows.Add(
                    sub.Account_Code,
                    sub.Account_Name_AR,
                    ConvertDbToAccountType(sub.Account_Type),
                    CategoryName(sub.Account_Category),
                    ConvertDbToNormalBalance(sub.Normal_Balance),
                    sub.Currency_Code,
                    sub.Is_Active ? "نشط" : "موقوف");
            }
            UpdateStatusBar();
        }

        private void NewAccount()
        {
            _loadingControls = true;
            _selectedAccountId = null;
            txtAccountCode.Clear();
            txtAccountNameAR.Clear();
            txtAccountNameEN.Clear();
            txtAccountLevel.Text = "1";
            txtNotes.Clear();

            cmbParentAccount.SelectedIndex = cmbParentAccount.Items.Count > 0 ? 0 : -1;
            cmbAccountType.Text = "أصل";
            cmbAccountStatus.Text = "نشط";
            cmbCurrency.Text = "YER";
            BindCategoriesForCurrentType();

            chkIsPostable.Checked = false;
            chkIsPostable.Enabled = false;
            chkIsSummaryAccount.Checked = true;
            chkIsSummaryAccount.Enabled = false;
            chkAllowManualEntry.Checked = false;
            chkSystemAccount.Checked = false;
            chkRequiresParty.Checked = false;
            chkRequiresCostCenter.Checked = false;
            chkRequiresProject.Checked = false;
            chkMultiCurrency.Checked = false;
            chkAffectsBalanceSheet.Checked = true;
            chkAffectsIncomeStatement.Checked = false;

            lblCurrentAccount.Text = "الحساب الحالي: -";
            dgvSubAccounts.Rows.Clear();
            _loadingControls = false;
            txtAccountNameAR.Focus();
        }

        private bool ValidateAccount()
        {
            if (string.IsNullOrWhiteSpace(txtAccountNameAR.Text))
            {
                MessageBox.Show("يرجى إدخال اسم الحساب بالعربية.", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAccountNameAR.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(cmbAccountType.Text))
            {
                MessageBox.Show("يرجى اختيار نوع الحساب.", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (cmbAccountCategory.SelectedValue == null)
            {
                MessageBox.Show("لا يوجد تصنيف فعال لنوع الحساب المحدد. نفذ سكربت تصنيفات الحسابات أولاً.",
                    "التصنيف مطلوب", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private async Task SaveAccountAsync()
        {
            if (!ValidateAccount()) return;

            string? parentId = null;
            if (cmbParentAccount.SelectedIndex > 0)
            {
                string parentCode = cmbParentAccount.Text.Split('-')[0].Trim();
                parentId = _accountsCache.FirstOrDefault(a => a.Account_Code == parentCode)?.Account_ID;
            }

            var request = new CreateAccountRequest
            {
                Company_ID = CompanyId,
                Parent_Account_ID = parentId,
                Account_Code = txtAccountCode.Text.Trim(),
                Account_Name_AR = txtAccountNameAR.Text.Trim(),
                Account_Name_EN = txtAccountNameEN.Text.Trim(),
                Account_Type = ConvertAccountTypeToDb(cmbAccountType.Text.Trim()),
                Account_Level = int.TryParse(txtAccountLevel.Text, out int level) ? level : 1,
                Is_Postable = chkIsPostable.Checked,
                Currency_Code = chkMultiCurrency.Checked ? null : cmbCurrency.Text.Trim(),
                Is_Active = cmbAccountStatus.Text == "نشط",
                Account_Category = cmbAccountCategory.SelectedValue?.ToString() ?? string.Empty,
                Normal_Balance = ConvertNormalBalanceToDb(cmbNormalBalance.Text),
                Notes = txtNotes.Text.Trim(),
                Allow_ManualEntry = chkAllowManualEntry.Checked,
                System_Account = chkSystemAccount.Checked,
                Requires_Party = chkRequiresParty.Checked,
                Requires_CostCenter = chkRequiresCostCenter.Checked,
                Requires_Project = chkRequiresProject.Checked,
                Is_Summary_Account = chkIsSummaryAccount.Checked,
                Affects_Balance_Sheet = chkAffectsBalanceSheet.Checked,
                Affects_Income_Statement = chkAffectsIncomeStatement.Checked,
                Multi_Currency = chkMultiCurrency.Checked,
                Created_By = CurrentSession.Username,
                Updated_By = CurrentSession.Username,
                Updated_At = DateTime.Now,
                Account_Path = string.Empty,
                Account_Serial = 0
            };

            try
            {
                HttpResponseMessage response = string.IsNullOrWhiteSpace(_selectedAccountId)
                    ? await _client.PostAsJsonAsync($"{_baseUrl}ChartOfAccounts", request)
                    : await _client.PutAsJsonAsync($"{_baseUrl}ChartOfAccounts/{_selectedAccountId}", request);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show(await response.Content.ReadAsStringAsync(), "خطأ من الخادم",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MessageBox.Show("تم حفظ بيانات الحساب وتحديث دليل الحسابات بنجاح.",
                    "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAccountsDataFromServerAsync();
                NewAccount();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل الاتصال بالخادم:\n{ex.Message}", "خطأ اتصال",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnDelete_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_selectedAccountId))
            {
                MessageBox.Show("يرجى اختيار حساب من الشجرة أولاً.", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_accountsCache.Any(a => a.Parent_Account_ID == _selectedAccountId && a.Is_Active))
            {
                MessageBox.Show("لا يمكن إيقاف حساب له حسابات أبناء نشطة.", "حماية محاسبية",
                    MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            if (MessageBox.Show($"هل تريد إيقاف الحساب [{txtAccountCode.Text} - {txtAccountNameAR.Text}]؟",
                    "تأكيد الإيقاف", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                var response = await _client.DeleteAsync($"{_baseUrl}ChartOfAccounts/{_selectedAccountId}?reason=إيقاف من شاشة دليل الحسابات");
                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show(await response.Content.ReadAsStringAsync(), "فشل الإيقاف",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MessageBox.Show("تم إيقاف الحساب دون حذف تاريخه.", "تم الإيقاف",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAccountsDataFromServerAsync();
                NewAccount();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء الإيقاف: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmbAccountType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_loadingControls) return;
            BindCategoriesForCurrentType();
            ApplyStatementFlags();
        }

        private void cmbParentAccount_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_loadingControls) return;

            if (cmbParentAccount.SelectedIndex <= 0)
            {
                txtAccountLevel.Text = "1";
                chkIsPostable.Checked = false;
                chkIsSummaryAccount.Checked = true;
                return;
            }

            string parentCode = cmbParentAccount.Text.Split('-')[0].Trim();
            var parent = _accountsCache.FirstOrDefault(a => a.Account_Code == parentCode);
            if (parent == null) return;

            txtAccountLevel.Text = (parent.Account_Level + 1).ToString();
            cmbAccountType.Text = ConvertDbToAccountType(parent.Account_Type);
            cmbCurrency.Text = parent.Currency_Code ?? "YER";
            chkIsPostable.Checked = true;
            chkIsSummaryAccount.Checked = false;
            BindCategoriesForCurrentType(parent.Account_Category);
        }

        private void ApplyStatementFlags()
        {
            string type = ConvertAccountTypeToDb(cmbAccountType.Text);
            chkAffectsBalanceSheet.Checked = type is "Asset" or "Liability" or "Equity";
            chkAffectsIncomeStatement.Checked = type is "Revenue" or "Expense";
            cmbNormalBalance.Text = ConvertDbToNormalBalance(DefaultBalanceForType(type));
        }

        private string CategoryName(string? code) =>
            _categoriesCache.FirstOrDefault(x => x.Category_Code == code)?.Category_Name_AR ?? code ?? string.Empty;

        private static string ConvertAccountTypeToDb(string value) => value switch
        {
            "أصل" => "Asset",
            "خصم" or "خصوم" => "Liability",
            "حقوق ملكية" => "Equity",
            "إيراد" => "Revenue",
            "مصروف" => "Expense",
            _ => value
        };

        private static string ConvertDbToAccountType(string? value) => value switch
        {
            "Asset" => "أصل",
            "Liability" => "خصم",
            "Equity" => "حقوق ملكية",
            "Revenue" => "إيراد",
            "Expense" => "مصروف",
            _ => value ?? string.Empty
        };

        private static string ConvertDbToNormalBalance(string? value) => value switch
        {
            "Debit" => "مدين",
            "Credit" => "دائن",
            _ => value ?? string.Empty
        };

        private static string ConvertNormalBalanceToDb(string? value) => value switch
        {
            "مدين" => "Debit",
            "دائن" => "Credit",
            _ => value ?? string.Empty
        };

        private static string DefaultBalanceForType(string accountType) =>
            accountType is "Asset" or "Expense" ? "Debit" : "Credit";

        private void UpdateStatusBar()
        {
            lblAccountsCount.Text = "عدد الحسابات بالدليل: " + _accountsCache.Count;
            lblCurrentCompany.Text = "الشركة الحالية: " + CompanyId;
            lblCurrentUser.Text = "المستخدم الحالي: " + CurrentSession.Username;
        }

        private void btnNew_Click(object? sender, EventArgs e) => NewAccount();
        private async void btnSave_Click(object? sender, EventArgs e) => await SaveAccountAsync();
        private async void btnEdit_Click(object? sender, EventArgs e) => await SaveAccountAsync();

        private async void btnRefresh_Click(object? sender, EventArgs e)
        {
            await LoadCategoriesAsync();
            await LoadAccountsDataFromServerAsync();
            NewAccount();
        }

        private void btnClose_Click(object? sender, EventArgs e) => Close();

        private void btnSearch_Click(object? sender, EventArgs e)
        {
            string key = txtSearchTree.Text.Trim();
            if (string.IsNullOrWhiteSpace(key))
            {
                BuildAccountsTree();
                dgvSubAccounts.Rows.Clear();
                return;
            }

            var filtered = _accountsCache.Where(a =>
                    (a.Account_Code ?? string.Empty).Contains(key, StringComparison.OrdinalIgnoreCase) ||
                    (a.Account_Name_AR ?? string.Empty).Contains(key, StringComparison.OrdinalIgnoreCase) ||
                    (a.Account_Name_EN ?? string.Empty).Contains(key, StringComparison.OrdinalIgnoreCase) ||
                    ConvertDbToAccountType(a.Account_Type).Contains(key, StringComparison.OrdinalIgnoreCase) ||
                    CategoryName(a.Account_Category).Contains(key, StringComparison.OrdinalIgnoreCase) ||
                    ConvertDbToNormalBalance(a.Normal_Balance).Contains(key, StringComparison.OrdinalIgnoreCase) ||
                    (a.Currency_Code ?? string.Empty).Contains(key, StringComparison.OrdinalIgnoreCase))
                .OrderBy(a => a.Account_Code)
                .ToList();

            tvAccounts.Nodes.Clear();
            dgvSubAccounts.Rows.Clear();
            foreach (var account in filtered)
            {
                tvAccounts.Nodes.Add(new TreeNode($"{account.Account_Code} - {account.Account_Name_AR}")
                {
                    Tag = account.Account_ID
                });
                dgvSubAccounts.Rows.Add(account.Account_Code, account.Account_Name_AR,
                    ConvertDbToAccountType(account.Account_Type), CategoryName(account.Account_Category),
                    ConvertDbToNormalBalance(account.Normal_Balance), account.Currency_Code,
                    account.Is_Active ? "نشط" : "موقوف");
            }
        }

        private void txtSearchTree_TextChanged(object? sender, EventArgs e) => btnSearch.PerformClick();

        private void btnPrint_Click(object? sender, EventArgs e)
        {
            _currentPrintIndex = 0;
            printPreviewDialog.Document = printDocument;
            printPreviewDialog.WindowState = FormWindowState.Maximized;
            printPreviewDialog.ShowDialog();
        }

        private void PrintDocument_PrintPage(object? sender, PrintPageEventArgs e)
        {
            Font titleFont = new("Arial", 16, FontStyle.Bold);
            Font headerFont = new("Arial", 11, FontStyle.Bold);
            Font dataFont = new("Arial", 10);
            int y = e.MarginBounds.Top;

            if (_currentPrintIndex == 0)
            {
                e.Graphics.DrawString("شركة الطائر السعيد للنقل والخدمات اللوجستية", headerFont, Brushes.Black, 450, y);
                y += 30;
                e.Graphics.DrawString("تقرير دليل الحسابات", titleFont, Brushes.Black, 250, y);
                y += 40;
            }

            var rows = _accountsCache.OrderBy(a => a.Account_Code).ToList();
            while (_currentPrintIndex < rows.Count)
            {
                if (y + 24 > e.MarginBounds.Bottom)
                {
                    e.HasMorePages = true;
                    return;
                }

                var account = rows[_currentPrintIndex++];
                int indent = Math.Max(0, account.Account_Level - 1) * 15;
                e.Graphics.DrawString(account.Account_Code ?? string.Empty, dataFont, Brushes.Black, e.MarginBounds.Left, y);
                e.Graphics.DrawString(account.Account_Name_AR ?? string.Empty, dataFont, Brushes.Black, e.MarginBounds.Left + 120 + indent, y);
                e.Graphics.DrawString(ConvertDbToAccountType(account.Account_Type), dataFont, Brushes.Black, e.MarginBounds.Left + 420, y);
                e.Graphics.DrawString(CategoryName(account.Account_Category), dataFont, Brushes.Black, e.MarginBounds.Left + 520, y);
                e.Graphics.DrawString(ConvertDbToNormalBalance(account.Normal_Balance), dataFont, Brushes.Black, e.MarginBounds.Left + 650, y);
                y += 22;
            }

            e.HasMorePages = false;
        }

        // أحداث مصمم الشاشة القديمة التي يجب إبقاؤها لمنع أخطاء الربط.
        private void cbParentAccount_SelectedIndexChanged(object sender, EventArgs e) { }
        private void label13_Click(object sender, EventArgs e) { }
        private void pnlMainDetails_Paint(object sender, PaintEventArgs e) { }
        private void gbBasicInfo_Enter(object sender, EventArgs e) { }
        private void dgvSubAccounts_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void chkRequiresProject_CheckedChanged(object sender, EventArgs e) { }
        private void pnlToolbar_Paint(object sender, PaintEventArgs e) { }
        private void grpSystemInfo_Enter(object sender, EventArgs e) { }

        private sealed class AccountCategoryLookupModel
        {
            public string Category_ID { get; set; } = string.Empty;
            public string Category_Code { get; set; } = string.Empty;
            public string Category_Name_AR { get; set; } = string.Empty;
            public string? Category_Name_EN { get; set; }
            public string Account_Type { get; set; } = string.Empty;
            public string Normal_Balance { get; set; } = string.Empty;
            public bool Is_System { get; set; }
            public bool Is_Active { get; set; }
            public int Sort_Order { get; set; }
        }
    }
}
