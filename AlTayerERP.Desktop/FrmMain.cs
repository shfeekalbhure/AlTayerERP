using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlTayerERP.Desktop.Services;

namespace AlTayerERP.Desktop
{
    public partial class FrmMain : Form
    {
        private readonly HttpClient _client = ApiService.Client;
        private readonly string _baseUrl = ApiService.BaseUrl;
        private HashSet<string> _allowedScreenCodes = new(StringComparer.OrdinalIgnoreCase);

        public FrmMain()
        {
            InitializeComponent();

            // منع فتح الواجهة الرئيسية مباشرة بدون سياق دخول كامل.
            if (!CurrentSession.IsLoggedIn)
            {
                MessageBox.Show("انتهت الجلسة أو أن بياناتها غير مكتملة. سجل الدخول من جديد.", "الجلسة", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                BeginInvoke(new Action(Close));
                return;
            }

            btnLogout.Click += btnLogout_Click;
            btnSettings.Click += btnSettings_Click;
            lblCompanyName.Text = "جاري التحميل...";
            lblCurrentBranch.Text = "الفرع\n" + CurrentSession.Branch_ID;
            lblFiscalYear.Text = "جاري التحميل...";
            lblCurrentUser.Text = "المستخدم\n" + CurrentSession.Username;
            _ = LoadSessionDetails();
            _ = RefreshConnectionStatusAsync();
            lblStatusTime.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
            BuildMainMenu();
            _ = LoadAllowedScreensAsync();
            tvMainMenu.NodeMouseDoubleClick -= tvMainMenu_NodeMouseDoubleClick;
            tvMainMenu.NodeMouseDoubleClick += tvMainMenu_NodeMouseDoubleClick;
        }

        private async Task LoadSessionDetails()
        {
            try
            {
                var response = await _client.GetAsync($"{_baseUrl}Branches/GetSessionInfo?companyId={CurrentSession.Company_ID}&branchId={CurrentSession.Branch_ID}&yearId={CurrentSession.Year_ID}");
                if (!response.IsSuccessStatusCode) return;
                var data = await response.Content.ReadFromJsonAsync<SessionInfoResponse>();
                if (data == null) return;
                lblCompanyName.Text = "شركة\n" + data.Company_Name_AR;
                lblCurrentBranch.Text = "الفرع\n" + data.Branch_Name;
                lblFiscalYear.Text = "السنة المالية\n" + data.Year_Name;
            }
            catch
            {
                lblCompanyName.Text = "شركة\n" + CurrentSession.Company_ID;
                lblFiscalYear.Text = "السنة المالية\n" + CurrentSession.Year_ID;
            }
        }

        // فحص عملي للخادم وقاعدة البيانات وتحديث شريط الحالة دون تعطيل المستخدم.
        private async Task RefreshConnectionStatusAsync()
        {
            try
            {
                var response = await _client.GetAsync($"{_baseUrl}health");
                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException();

                lblStatusApi.Text = "API: متصل";
                lblStatusDatabase.Text = "قاعدة البيانات: متصلة";
            }
            catch
            {
                lblStatusApi.Text = "API: غير متصل";
                lblStatusDatabase.Text = "قاعدة البيانات: غير متصلة";
            }
        }

        // تحميل صلاحيات العرض الفعلية للدور الحالي؛ مدير النظام يملك وصولاً كاملاً.
        private async Task LoadAllowedScreensAsync()
        {
            if (CurrentSession.Is_System_Admin)
                return;

            try
            {
                var screens = await _client.GetFromJsonAsync<List<ScreenAccessRow>>($"{_baseUrl}RolePermissions/GetScreens") ?? new();
                var permissions = await _client.GetFromJsonAsync<List<RolePermissionRow>>(
                    $"{_baseUrl}RolePermissions/GetRolePermissions/${CurrentSession.Role_ID}") ?? new();

                _allowedScreenCodes = screens
                    .Where(screen => permissions.Any(permission =>
                        permission.Screen_ID == screen.Screen_ID && permission.Can_View))
                    .Select(screen => screen.Screen_Code)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                BuildMainMenu();
            }
            catch
            {
                // في حال تعذر الجلب لا نمنح صلاحيات افتراضية للمستخدم العادي.
                _allowedScreenCodes.Clear();
                BuildMainMenu();
                MessageBox.Show("تعذر تحميل صلاحيات الدور؛ تم إخفاء الشاشات لحماية النظام.",
                    "الصلاحيات", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool CanOpenScreen(string screenCode) =>
            CurrentSession.Is_System_Admin || _allowedScreenCodes.Contains(screenCode);

        private void AddScreen(TreeNode parent, string screenCode, string caption)
        {
            if (CanOpenScreen(screenCode))
                parent.Nodes.Add(screenCode, caption);
        }

        private sealed class ScreenAccessRow
        {
            public int Screen_ID { get; set; }
            public string Screen_Code { get; set; } = "";
        }

        private sealed class RolePermissionRow
        {
            public int Screen_ID { get; set; }
            public bool Can_View { get; set; }
        }

        private sealed class SessionInfoResponse
        {
            public string Company_Name_AR { get; set; } = "";
            public string Branch_Name { get; set; } = "";
            public string Year_Name { get; set; } = "";
        }

        // بناء شجرة النظام حسب مستوى الجلسة؛ شاشات التهيئة الحساسة لمدير النظام فقط.
        private void BuildMainMenu()
        {
            tvMainMenu.Nodes.Clear();

            TreeNode adminNode = new("الإدارة العامة");
            AddScreen(adminNode, "Companies", "الشركات");
            AddScreen(adminNode, "Branches", "الفروع");
            AddScreen(adminNode, "FiscalYears", "السنوات المالية");
            AddScreen(adminNode, "Users", "المستخدمون");
            AddScreen(adminNode, "Roles", "الأدوار");

            TreeNode accountingNode = new("الحسابات");
            AddScreen(accountingNode, "ChartOfAccounts", "الدليل المحاسبي");
            AddScreen(accountingNode, "Currencies", "العملات");
            AddScreen(accountingNode, "CostCenters", "مراكز التكلفة");
            AddScreen(accountingNode, "CashBoxes", "الصناديق");
            AddScreen(accountingNode, "ReceiptVoucher", "سند القبض");

            // شاشات التهيئة الحساسة مخصصة لمدير النظام إلى أن يكتمل محرك الصلاحيات التفصيلي.
            if (CurrentSession.Is_System_Admin)
            {
                AddScreen(adminNode, "RolePermissions", "صلاحيات الأدوار");

                TreeNode setupNode = new("التهيئة والإعدادات");
                AddScreen(setupNode, "GeneralSettings", "الإعدادات العامة والمالية");
                AddScreen(setupNode, "SystemScreens", "كتالوج شاشات النظام");
                AddScreen(setupNode, "NumberingSettings", "إعدادات الترقيم");
                AddScreen(setupNode, "FiscalPeriods", "الفترات المالية");
                AddScreen(setupNode, "ExchangeRates", "أسعار الصرف");
                AddScreen(setupNode, "PaymentMethods", "طرق السداد");
                AddScreen(setupNode, "VoucherTypes", "أنواع السندات");
                AddScreen(setupNode, "VoucherStatuses", "حالات السندات");
                AddScreen(setupNode, "ApprovalPolicies", "سياسات الاعتماد والسقوف");
                adminNode.Nodes.Add(setupNode);

                AddScreen(accountingNode, "Banks", "البنوك والحسابات البنكية");
                AddScreen(accountingNode, "Parties", "الأطراف المالية");
            }

            tvMainMenu.Nodes.Add(adminNode);
            tvMainMenu.Nodes.Add(accountingNode);
            tvMainMenu.ExpandAll();
        }

        private void tvMainMenu_NodeMouseDoubleClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Node.Name) && !CanOpenScreen(e.Node.Name))
            {
                MessageBox.Show("ليس لديك صلاحية لفتح هذه الشاشة.", "الصلاحيات", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Form? form = e.Node.Name switch
            {
                "Companies" => new CompanyForm(),
                "Branches" => new BranchForm(),
                "FiscalYears" => new FiscalYearForm(),
                "Users" => new FrmUsers(),
                "Roles" => new FrmRoles(),
                "RolePermissions" => new FrmRolePermissions(),
                "GeneralSettings" => new FrmGeneralSettings(),
                "SystemScreens" => new FrmSystemScreens(),
                "NumberingSettings" => new FrmNumberingSettings(),
                "FiscalPeriods" => new FrmFiscalPeriods(),
                "ExchangeRates" => new FrmExchangeRates(),
                "PaymentMethods" => new FrmPaymentMethods(),
                "VoucherTypes" => new FrmVoucherTypes(),
                "VoucherStatuses" => new FrmVoucherStatuses(),
                "ApprovalPolicies" => new FrmApprovalPolicies(),
                "ChartOfAccounts" => new FrmChartOfAccounts(),
                "Currencies" => new FrmCurrencies(),
                "CostCenters" => new FrmCostCenters(),
                "CashBoxes" => new FrmCashBoxes(),
                "Banks" => new FrmBanks(),
                "Parties" => new FrmParties(),
                "ReceiptVoucher" => new FrmReceiptVoucher(),
                _ => null
            };
            form?.ShowDialog(this);
        }

        // إنهاء الجلسة المحلية وإرجاع المستخدم إلى شاشة الدخول.
        private void btnLogout_Click(object? sender, EventArgs e)
        {
            if (MessageBox.Show("هل تريد تسجيل الخروج من الجلسة الحالية؟", "تسجيل الخروج", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            CurrentSession.Clear();
            Close();
        }

        // بوابة الإعدادات العامة لا تُفتح إلا لمدير النظام إلى أن يكتمل محرك الصلاحيات التفصيلي.
        private void btnSettings_Click(object? sender, EventArgs e)
        {
            if (!CurrentSession.Is_System_Admin)
            {
                MessageBox.Show("هذه الشاشة مخصصة لمدير النظام.", "الصلاحيات", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            new FrmGeneralSettings().ShowDialog(this);
        }

        private void btnUsers_Click(object sender, EventArgs e) => new FrmUsers().ShowDialog(this);
        private void button10_Click(object sender, EventArgs e) { }
        private void btnAccountingCenter_Click(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void lblStatusTime_Click(object sender, EventArgs e) { }
        private void tvMainMenu_AfterSelect(object sender, TreeViewEventArgs e) { }
        private void flpMenu_Paint(object sender, PaintEventArgs e) { }
        private void pnlWorkspace_Paint(object sender, PaintEventArgs e) { }
        private void tvMainMenu_AfterSelect_1(object sender, TreeViewEventArgs e) { }
    }
}
