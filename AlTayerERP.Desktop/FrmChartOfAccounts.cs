using AlTayerERP.Desktop.Models;
using AlTayerERP.Desktop.Services; // استيراد خدمات الجلسة الحالية
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    public partial class FrmChartOfAccounts : Form
    {
        // تهيئة عميل الـ API والرابط الأساسي المتفق عليه في خدمات النظام
        private readonly HttpClient _client = ApiService.Client;
        private readonly string _baseUrl = ApiService.BaseUrl;

        // [تعديل 1] إرجاع جلب معرف الشركة ديناميكياً من جلسة المستخدم الحالية بشكل رسمي
        private string CompanyId => CurrentSession.Company_ID;
        // متغيرات التحكم بحالة الشاشة والذاكرة المؤقتة للحسابات المستلمة من السيرفر  
        private string _selectedAccountId = null;
        private List<AccountModel> _accountsCache = new List<AccountModel>();
        private readonly PrintDocument printDocument = new PrintDocument();
        private readonly PrintPreviewDialog printPreviewDialog = new PrintPreviewDialog();
        private int _currentPrintIndex = 0; // مؤشر تتبع الحساب الحالي أثناء عملية الطباعة متعددة الصفحات  

        public FrmChartOfAccounts()
        {
            InitializeComponent();

            // تطبيق الثيم العربي الموحد والاختصارات على الشاشة القديمة.
            ArabicErpFormStyle.Apply(this);
            RegisterEvents();
            printDocument.PrintPage += PrintDocument_PrintPage;
        }

        /// <summary>  
        /// تسجيل كافة أحداث عناصر التحكم بالشاشة لربطها بالدوال البرمجية المخصصة  
        /// </summary>  
        private void RegisterEvents()
        {
            this.Load += FrmChartOfAccounts_Load;
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
        }

        private async void FrmChartOfAccounts_Load(object sender, EventArgs e)
        {
            FillCombos();
            SetupGrid();
            await LoadAccountsDataFromServerAsync();
            UpdateStatusBar();
        }

        /// <summary>  
        /// تعبئة القوائم المنسدلة بالقيم الثابتة المطلوبة للنظام المحاسبي  
        /// </summary>  
        private void FillCombos()
        {
            cmbAccountType.Items.Clear();
            cmbAccountType.Items.AddRange(new string[] { "أصل", "خصم", "حقوق ملكية", "إيراد", "مصروف" });

            cmbAccountCategory.Items.Clear();
            cmbAccountCategory.Items.AddRange(new string[] { "نقدية", "بنك", "عميل", "مورد", "إيراد", "مصروف", "أصل", "خصم" });

            cmbNormalBalance.Items.Clear();
            cmbNormalBalance.Items.AddRange(new string[] { "مدين", "دائن" });

            cmbCurrency.Items.Clear();
            cmbCurrency.Items.AddRange(new string[] { "YER", "SAR", "USD" });

            cmbAccountStatus.Items.Clear();
            cmbAccountStatus.Items.AddRange(new string[] { "نشط", "موقوف" });

            SetDefaultComboValues();
        }

        private void SetDefaultComboValues()
        {
            cmbAccountType.Text = "أصل";
            cmbAccountCategory.Text = "نقدية";
            cmbNormalBalance.Text = "مدين";
            cmbAccountStatus.Text = "نشط";
            cmbCurrency.Text = "YER";
        }

        /// <summary>  
        /// تهيئة الجدول لإظهار الحسابات الفرعية المنبثقة من الحساب الرئيسي المحدد  
        /// </summary>  
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
            dgvSubAccounts.Columns.Add("NormalBalance", "طبيعة الحساب");
            dgvSubAccounts.Columns.Add("Currency", "العملة");
            dgvSubAccounts.Columns.Add("Status", "الحالة");
        }

        /// <summary>  
        /// جلب بيانات دليل الحسابات كاملاً من الـ API وتخزينها بالذاكرة المؤقتة لشركة الجلسة الحالية حصرياً  
        /// </summary>  
        private async Task LoadAccountsDataFromServerAsync()
        {
            // فحص إضافي للتأكد من أن الجلسة تمرر رقم الشركة فعلاً
            if (string.IsNullOrEmpty(CompanyId))
            {
                MessageBox.Show("خطأ: لم يتم تحديد شركة للجلسة الحالية! يرجى إعادة تسجيل الدخول.", "تنبيه النظام", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var accounts = await _client.GetFromJsonAsync<List<AccountModel>>($"{_baseUrl}ChartOfAccounts?companyId={CompanyId}");
                _accountsCache = accounts ?? new List<AccountModel>();
                BuildAccountsTree();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ أثناء جلب الدليل من السيرفر: {ex.Message}", "خطأ شبكة", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>  
        /// بناء وتوزيع شجرة الحسابات (TreeView) وتجهيز قائمة الحساب الأب  
        /// </summary>  
        private void BuildAccountsTree()
        {
            tvAccounts.Nodes.Clear();
            cmbParentAccount.Items.Clear();
            cmbParentAccount.Items.Add("بدون حساب أب (حساب رئيسي)");

            var rootAccounts = _accountsCache.Where(a => string.IsNullOrEmpty(a.Parent_Account_ID)).OrderBy(a => a.Account_Code);
            foreach (var account in rootAccounts)
            {
                TreeNode rootNode = new TreeNode($"{account.Account_Code} - {account.Account_Name_AR}") { Tag = account.Account_ID };
                AddChildNodes(rootNode, account.Account_ID);
                tvAccounts.Nodes.Add(rootNode);
                cmbParentAccount.Items.Add($"{account.Account_Code} - {account.Account_Name_AR}");
            }
            tvAccounts.ExpandAll();
            UpdateStatusBar();
        }

        /// <summary>  
        /// دالة استدعاء ذاتي (Recursive) لبناء الفروع داخل فروعها إلى آخر مستوى محاسبي  
        /// </summary>  
        private void AddChildNodes(TreeNode parentNode, string parentId)
        {
            var children = _accountsCache.Where(a => a.Parent_Account_ID == parentId).OrderBy(a => a.Account_Code);
            foreach (var child in children)
            {
                TreeNode childNode = new TreeNode($"{child.Account_Code} - {child.Account_Name_AR}") { Tag = child.Account_ID };
                AddChildNodes(childNode, child.Account_ID);
                parentNode.Nodes.Add(childNode);
                cmbParentAccount.Items.Add($"{child.Account_Code} - {child.Account_Name_AR}");
            }
        }

        private void tvAccounts_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null || e.Node.Tag == null) return;
            FillAccountToFields(e.Node.Tag.ToString());
        }

        /// <summary>  
        /// قراءة بيانات الحساب المختار وتفريغها داخل صناديق النصوص وصناديق الاختيار المعدلة  
        /// </summary>  
        private void FillAccountToFields(string accountId)
        {
            var account = _accountsCache.FirstOrDefault(a => a.Account_ID == accountId);
            if (account == null) return;

            _selectedAccountId = account.Account_ID;
            txtAccountCode.Text = account.Account_Code;
            txtAccountNameAR.Text = account.Account_Name_AR;
            txtAccountNameEN.Text = account.Account_Name_EN;
            txtAccountLevel.Text = account.Account_Level.ToString();
            txtNotes.Text = account.Notes;

            cmbAccountType.Text = ConvertDbToAccountType(account.Account_Type);
            // [تعديل 3] عرض طبيعة الحساب بالعربية باستخدام الدالة الجديدة
            cmbNormalBalance.Text = ConvertDbToNormalBalance(account.Normal_Balance);
            cmbCurrency.Text = account.Currency_Code;
            cmbAccountCategory.Text = ConvertDbToCategory(account.Account_Category);
            cmbAccountStatus.Text = account.Is_Active ? "نشط" : "موقوف";

            if (!string.IsNullOrEmpty(account.Parent_Account_ID))
            {
                var parent = _accountsCache.FirstOrDefault(p => p.Account_ID == account.Parent_Account_ID);
                if (parent != null)
                {
                    cmbParentAccount.Text = $"{parent.Account_Code} - {parent.Account_Name_AR}";
                }
            }
            else
            {
                cmbParentAccount.SelectedIndex = 0;
            }

            if (chkIsPostable != null) chkIsPostable.Checked = account.Is_Postable;

            if (chkAllowManualEntry != null) chkAllowManualEntry.Checked = account.Allow_ManualEntry;
            if (chkSystemAccount != null) chkSystemAccount.Checked = account.System_Account;
            if (chkRequiresParty != null) chkRequiresParty.Checked = account.Requires_Party;
            if (chkRequiresCostCenter != null) chkRequiresCostCenter.Checked = account.Requires_CostCenter;
            if (chkRequiresProject != null) chkRequiresProject.Checked = account.Requires_Project;
            if (chkMultiCurrency != null) chkMultiCurrency.Checked = account.Multi_Currency;
            if (chkAffectsBalanceSheet != null) chkAffectsBalanceSheet.Checked = account.Affects_Balance_Sheet;
            if (chkAffectsIncomeStatement != null) chkAffectsIncomeStatement.Checked = account.Affects_Income_Statement;
            if (chkIsSummaryAccount != null) chkIsSummaryAccount.Checked = account.Is_Summary_Account;

            if (lblCurrentAccount != null) lblCurrentAccount.Text = "الحساب الحالي: " + account.Account_Code;
            LoadSubAccountsToGrid(account.Account_ID);
        }

        private void dgvSubAccounts_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var cellValue = dgvSubAccounts.Rows[e.RowIndex].Cells["AccountCode"].Value;
            if (cellValue == null) return;

            string accountCode = cellValue.ToString();
            var account = _accountsCache.FirstOrDefault(a => a.Account_Code == accountCode);
            if (account != null)
            {
                FillAccountToFields(account.Account_ID);
            }
        }

        private void LoadSubAccountsToGrid(string parentId)
        {
            dgvSubAccounts.Rows.Clear();
            var subAccounts = _accountsCache.Where(a => a.Parent_Account_ID == parentId).OrderBy(a => a.Account_Code);
            foreach (var sub in subAccounts)
            {
                dgvSubAccounts.Rows.Add(
                    sub.Account_Code,
                    sub.Account_Name_AR,
                    ConvertDbToAccountType(sub.Account_Type),
                    // [تعديل 3] عرض طبيعة الحساب بالعربية في الجدول الفرعي
                    ConvertDbToNormalBalance(sub.Normal_Balance),
                    sub.Currency_Code,
                    sub.Is_Active ? "نشط" : "موقوف"
                );
            }
            UpdateStatusBar();
        }

        /// <summary>  
        /// تصفير كافة الحقول وتجهيز الشاشة لإدراج حساب جديد  
        /// </summary>  
        private void NewAccount()
        {
            _selectedAccountId = null;
            txtAccountCode.Clear();
            txtAccountNameAR.Clear();
            txtAccountNameEN.Clear();
            txtAccountLevel.Text = "1";
            txtNotes.Clear();

            if (cmbParentAccount.Items.Count > 0) cmbParentAccount.SelectedIndex = 0;
            SetDefaultComboValues();

            if (chkIsPostable != null) chkIsPostable.Checked = true;

            if (chkAllowManualEntry != null) chkAllowManualEntry.Checked = true;
            if (chkSystemAccount != null) chkSystemAccount.Checked = false;
            if (chkRequiresParty != null) chkRequiresParty.Checked = false;
            if (chkRequiresCostCenter != null) chkRequiresCostCenter.Checked = false;
            if (chkRequiresProject != null) chkRequiresProject.Checked = false;
            if (chkMultiCurrency != null) chkMultiCurrency.Checked = false;
            if (chkAffectsBalanceSheet != null) chkAffectsBalanceSheet.Checked = false;
            if (chkAffectsIncomeStatement != null) chkAffectsIncomeStatement.Checked = false;
            if (chkIsSummaryAccount != null) chkIsSummaryAccount.Checked = false;

            if (lblCurrentAccount != null) lblCurrentAccount.Text = "الحساب الحالي: -";
            txtAccountNameAR.Focus();
        }

        private bool ValidateAccount()
        {
            if (string.IsNullOrWhiteSpace(txtAccountNameAR.Text))
            {
                MessageBox.Show("يرجى إدخال اسم الحساب بالعربي.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAccountNameAR.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(cmbAccountType.Text))
            {
                MessageBox.Show("يرجى اختيار نوع الحساب المحاسبي.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbAccountType.Focus();
                return false;
            }
            return true;
        }

        /// <summary>  
        /// دالة حفظ وتعديل البيانات بالاعتماد على مسارات الـ API وإرسال معرف الشركة الحالية من الجلسة النشطة  
        /// </summary>  
        private async Task SaveAccountAsync()
        {
            if (!ValidateAccount()) return;

            string parentId = null;
            if (cmbParentAccount.SelectedIndex > 0)
            {
                string selectedParentText = cmbParentAccount.Text;
                string parentCode = selectedParentText.Split('-')[0].Trim();
                var parentAcc = _accountsCache.FirstOrDefault(a => a.Account_Code == parentCode);
                parentId = parentAcc?.Account_ID;
            }

            // [تعديل 2] حماية جلب بيانات الجلسة والمستخدم من الوقوع في خطأ الـ NullReference
            string currentUsername = CurrentSession.Username;
            var request = new CreateAccountRequest
            {
                Company_ID = CompanyId,
                Parent_Account_ID = parentId,
                Account_Code = txtAccountCode.Text.Trim(),
                Account_Name_AR = txtAccountNameAR.Text.Trim(),
                Account_Name_EN = txtAccountNameEN.Text.Trim(),
                Account_Type = ConvertAccountTypeToDb(cmbAccountType.Text.Trim()),
                Account_Level = int.TryParse(txtAccountLevel.Text, out int lvl) ? lvl : 1,
                Is_Postable = chkIsPostable?.Checked ?? true,
                Currency_Code = cmbCurrency.Text.Trim(),
                Is_Active = cmbAccountStatus.Text == "نشط",
                Account_Category = ConvertCategoryToDb(cmbAccountCategory.Text.Trim()),
                Normal_Balance = cmbNormalBalance.Text.Trim(),
                Notes = txtNotes.Text.Trim(),

                Allow_ManualEntry = chkAllowManualEntry?.Checked ?? false,
                System_Account = chkSystemAccount?.Checked ?? false,
                Requires_Party = chkRequiresParty?.Checked ?? false,
                Requires_CostCenter = chkRequiresCostCenter?.Checked ?? false,
                Requires_Project = chkRequiresProject?.Checked ?? false,
                Is_Summary_Account = chkIsSummaryAccount?.Checked ?? false,
                Affects_Balance_Sheet = chkAffectsBalanceSheet?.Checked ?? false,
                Affects_Income_Statement = chkAffectsIncomeStatement?.Checked ?? false,
                Multi_Currency = chkMultiCurrency?.Checked ?? false,

                Created_By = currentUsername,
                Updated_By = currentUsername,
                Updated_At = DateTime.Now,
                Account_Path = "",
                Account_Serial = 0
            };

            try
            {
                HttpResponseMessage response;
                if (string.IsNullOrEmpty(_selectedAccountId))
                {
                    response = await _client.PostAsJsonAsync($"{_baseUrl}ChartOfAccounts", request);
                }
                else
                {
                    // [تعديل 5] تمرير معرف الشركة بصيغة الـ Query المتوقعة أمنياً في دالة التعديل (PUT) بالـ API لقاعدة البيانات
                    response = await _client.PutAsJsonAsync($"{_baseUrl}ChartOfAccounts/{_selectedAccountId}?companyId={CompanyId}", request);
                }

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("تمت العملية بنجاح وتحديث دليل الحسابات بنظام AlTayerERP.", "نجاح الحفظ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    NewAccount();
                    await LoadAccountsDataFromServerAsync();
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show(error, "خطأ من السيرفر", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل الاتصال بالـ API:\n{ex.Message}", "خطأ اتصال", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedAccountId))
            {
                MessageBox.Show("يرجى اختيار حساب من الشجرة أولاً لحذفه.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool hasChildren = _accountsCache.Any(a => a.Parent_Account_ID == _selectedAccountId);
            if (hasChildren)
            {
                MessageBox.Show("لا يمكن حذف هذا الحساب لأنه حساب أب يحتوي على حسابات فرعية تابعة له. قم بحذف الحسابات الفرعية أولاً.", "حماية النظام المحاسبي", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            var confirm = MessageBox.Show($"هل أنت متأكد من رغبتك في حذف الحساب [{txtAccountCode.Text} - {txtAccountNameAR.Text}] نهائياً؟", "تأكيد الحذف", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.No) return;

            try
            {
                var response = await _client.DeleteAsync($"{_baseUrl}ChartOfAccounts/{_selectedAccountId}?companyId={CompanyId}");
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("تم حذف الحساب المالي بنجاح من الدليل.", "تم الحذف", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    NewAccount();
                    await LoadAccountsDataFromServerAsync();
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show(error, "فشل الحذف", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء عملية الحذف: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ConvertAccountTypeToDb(string value)
        {
            return value switch
            {
                "أصل" => "Asset",
                "خصم" => "Liability",
                "خصوم" => "Liability",
                "حقوق ملكية" => "Equity",
                "إيراد" => "Revenue",
                "مصروف" => "Expense",
                _ => value
            };
        }

        private string ConvertDbToAccountType(string value)
        {
            return value switch
            {
                "Asset" => "أصل",
                "Liability" => "خصم",
                "Equity" => "حقوق ملكية",
                "Revenue" => "إيراد",
                "Expense" => "مصروف",
                _ => value
            };
        }

        private string ConvertCategoryToDb(string value)
        {
            return value switch
            {
                "نقدية" => "Cash",
                "بنك" => "Bank",
                "عميل" => "Customer",
                "مورد" => "Vendor",
                "إيراد" => "Revenue",
                "مصروف" => "Expense",
                "أصل" => "Asset",
                "خصم" => "Liability",
                _ => value
            };
        }

        private string ConvertDbToCategory(string value)
        {
            return value switch
            {
                "Cash" => "نقدية",
                "Bank" => "بنك",
                "Customer" => "عميل",
                "Vendor" => "مورد",
                "Revenue" => "إيراد",
                "Expense" => "مصروف",
                "Asset" => "أصل",
                "Liability" => "خصم",
                _ => value
            };
        }

        // [تعديل 3] دالة التحويل من قاعدة البيانات إلى الواجهة العربية لطبيعة الحساب (مدين/دائن)
        private string ConvertDbToNormalBalance(string value)
        {
            value = value?.Trim() ?? "";
            return value switch
            {
                "Debit" => "مدين",
                "Credit" => "دائن",
                _ => value
            };
        }

        private void cbParentAccount_SelectedIndexChanged(object sender, EventArgs e)
        {
            // تم دمج المنطق هنا مع دالة cmbParentAccount_SelectedIndexChanged لمنع التعارض المسميات في الكود الأصلي
        }

        private void cmbParentAccount_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbParentAccount.SelectedIndex <= 0)
            {
                txtAccountLevel.Text = "1";
                return;
            }

            string selectedParentText = cmbParentAccount.Text;
            string parentCode = selectedParentText.Split('-')[0].Trim();
            var parentAcc = _accountsCache.FirstOrDefault(a => a.Account_Code == parentCode);
            if (parentAcc != null)
            {
                txtAccountLevel.Text = (parentAcc.Account_Level + 1).ToString();
                cmbAccountType.Text = ConvertDbToAccountType(parentAcc.Account_Type);
                // [تعديل 3] تحويل طبيعة الحساب التابع للأب إلى العربية عند تغييره
                cmbNormalBalance.Text = ConvertDbToNormalBalance(parentAcc.Normal_Balance);
                cmbCurrency.Text = parentAcc.Currency_Code;
                cmbAccountCategory.Text = ConvertDbToCategory(parentAcc.Account_Category);
            }
        }

        private void UpdateStatusBar()
        {
            if (lblAccountsCount != null) lblAccountsCount.Text = "عدد الحسابات بالدليل: " + _accountsCache.Count;
            if (lblCurrentCompany != null) lblCurrentCompany.Text = "الشركة الحالية: " + CompanyId;
            //   if (lblCurrentUser != null) lblCurrentUser.Text = "المستخدم الحالي: " + (CurrentSession?.Username ?? "System");
            if (lblCurrentUser != null) lblCurrentUser.Text = "المستخدم الحالي: " + CurrentSession.Username;
        }

        private void btnNew_Click(object sender, EventArgs e) => NewAccount();
        private async void btnSave_Click(object sender, EventArgs e) => await SaveAccountAsync();
        private async void btnEdit_Click(object sender, EventArgs e) => await SaveAccountAsync();

        // [تعديل 4] تصفير وتفريغ مدخلات ومحددات الشاشة قبل بدء عملية جلب وعرض الدليل من السيرفر لمنع تداخل القراءة السابقة
        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            NewAccount();
            await LoadAccountsDataFromServerAsync();
        }

        private void btnClose_Click(object sender, EventArgs e) => Close();

        private void btnPrint_Click(object sender, EventArgs e)
        {
            _currentPrintIndex = 0;
            printPreviewDialog.Document = printDocument;
            printPreviewDialog.WindowState = FormWindowState.Maximized;
            printPreviewDialog.ShowDialog();
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            try
            {
                Font titleFont = new Font("Arial", 16, FontStyle.Bold);
                Font headerFont = new Font("Arial", 11, FontStyle.Bold);
                Font dataFont = new Font("Arial", 10, FontStyle.Regular);
                Brush blackBrush = Brushes.Black;
                int y = e.MarginBounds.Top;

                if (_currentPrintIndex == 0)
                {
                    e.Graphics.DrawString("شركة الطاير السعيد للنقل والخدمات اللوجستية", headerFont, blackBrush, 450, y);
                    y += 30;
                    e.Graphics.DrawString("تقرير الدليل المحاسبي الشجري العام (Chart of Accounts)", titleFont, blackBrush, 180, y);
                    y += 40;
                }

                e.Graphics.DrawLine(Pens.Black, e.MarginBounds.Left, y, e.MarginBounds.Right, y);
                y += 5;
                e.Graphics.DrawString("رقم الحساب", headerFont, blackBrush, e.MarginBounds.Left, y);
                e.Graphics.DrawString("اسم الحساب المحاسبي", headerFont, blackBrush, e.MarginBounds.Left + 130, y);
                e.Graphics.DrawString("النوع", headerFont, blackBrush, e.MarginBounds.Left + 400, y);
                e.Graphics.DrawString("العملة", headerFont, blackBrush, e.MarginBounds.Left + 500, y);
                e.Graphics.DrawString("طبيعة الحساب", headerFont, blackBrush, e.MarginBounds.Left + 600, y);
                y += 25;
                e.Graphics.DrawLine(Pens.Black, e.MarginBounds.Left, y, e.MarginBounds.Right, y);
                y += 15;

                var orderedAccounts = _accountsCache.OrderBy(a => a.Account_Code).ToList();
                while (_currentPrintIndex < orderedAccounts.Count)
                {
                    var acc = orderedAccounts[_currentPrintIndex];
                    if (y + 25 > e.MarginBounds.Bottom)
                    {
                        e.HasMorePages = true;
                        return;
                    }

                    int indent = (acc.Account_Level - 1) * 15;
                    e.Graphics.DrawString(acc.Account_Code ?? "", dataFont, blackBrush, e.MarginBounds.Left, y);
                    e.Graphics.DrawString(acc.Account_Name_AR ?? "", dataFont, blackBrush, e.MarginBounds.Left + 130 + indent, y);
                    e.Graphics.DrawString(ConvertDbToAccountType(acc.Account_Type), dataFont, blackBrush, e.MarginBounds.Left + 400, y);
                    e.Graphics.DrawString(acc.Currency_Code ?? "", dataFont, blackBrush, e.MarginBounds.Left + 500, y);
                    // [تعديل 3] طباعة طبيعة الحساب باللغة العربية داخل ملفات الـ PDF والتقارير المكتوبة
                    e.Graphics.DrawString(ConvertDbToNormalBalance(acc.Normal_Balance), dataFont, blackBrush, e.MarginBounds.Left + 600, y);
                    y += 22;
                    _currentPrintIndex++;
                }
                e.HasMorePages = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء محاولة طباعة التقرير الشجري: {ex.Message}", "خطأ في محرك الطباعة", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.HasMorePages = false;
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchKey = txtSearchTree.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(searchKey))
            {
                BuildAccountsTree();
                dgvSubAccounts.Rows.Clear();
                return;
            }

            var filtered = _accountsCache
                .Where(a => (a.Account_Code ?? "").ToLower().Contains(searchKey) ||
                            (a.Account_Name_AR ?? "").ToLower().Contains(searchKey) ||
                            (a.Account_Name_EN ?? "").ToLower().Contains(searchKey))
                .OrderBy(a => a.Account_Code)
                .ToList();

            tvAccounts.Nodes.Clear();
            dgvSubAccounts.Rows.Clear();

            foreach (var acc in filtered)
            {
                TreeNode node = new TreeNode($"{acc.Account_Code} - {acc.Account_Name_AR}") { Tag = acc.Account_ID };
                tvAccounts.Nodes.Add(node);
                dgvSubAccounts.Rows.Add(
                    acc.Account_Code,
                    acc.Account_Name_AR,
                    ConvertDbToAccountType(acc.Account_Type),
                    // [تعديل 3] تحويل قيم الطبيعة المحاسبية بجدول البحث الفرعي
                    ConvertDbToNormalBalance(acc.Normal_Balance),
                    acc.Currency_Code,
                    acc.Is_Active ? "نشط" : "موقوف"
                );
            }
            tvAccounts.ExpandAll();
        }

        private void txtSearchTree_TextChanged(object sender, EventArgs e)
        {
            btnSearch.PerformClick();
        }

        private void label13_Click(object sender, EventArgs e) { }
        private void pnlMainDetails_Paint(object sender, PaintEventArgs e) { }
        private void gbBasicInfo_Enter(object sender, EventArgs e) { }
        private void dgvSubAccounts_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void chkRequiresProject_CheckedChanged(object sender, EventArgs e) { }
        private void pnlToolbar_Paint(object sender, PaintEventArgs e) { }

        private void grpSystemInfo_Enter(object sender, EventArgs e)
        {

        }
    }
}