using AlTayerERP.Desktop.Services;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// مساحة العمل الرئيسية. تعرض فقط الشاشات التي تسمح بها الجلسة الحالية.
    /// </summary>
    public partial class FrmMain : Form
    {
        private readonly HttpClient _client = ApiService.Client;
        private readonly string _baseUrl = ApiService.BaseUrl;

        public FrmMain()
        {
            InitializeComponent();

            if (!CurrentSession.IsLoggedIn)
            {
                MessageBox.Show(
                    "لا توجد جلسة دخول صالحة. أعد تسجيل الدخول.",
                    "جلسة غير صالحة",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                BeginInvoke(new Action(Close));
                return;
            }

            lblCompanyName.Text = "جاري التحميل...";
            lblCurrentBranch.Text = "الفرع\n" + CurrentSession.Branch_ID;
            lblFiscalYear.Text = "جاري التحميل...";
            lblCurrentUser.Text = "المستخدم\n" + CurrentSession.Username;

            _ = LoadSessionDetailsAsync();
            BuildMainMenu();

            tvMainMenu.NodeMouseDoubleClick -= tvMainMenu_NodeMouseDoubleClick;
            tvMainMenu.NodeMouseDoubleClick += tvMainMenu_NodeMouseDoubleClick;
        }

        private async Task LoadSessionDetailsAsync()
        {
            try
            {
                string url =
                    $"{_baseUrl}Branches/GetSessionInfo?companyId={Uri.EscapeDataString(CurrentSession.Company_ID)}" +
                    $"&branchId={CurrentSession.Branch_ID}&yearId={CurrentSession.Year_ID}";

                using HttpResponseMessage response = await _client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return;

                SessionInfoResponse? data =
                    await response.Content.ReadFromJsonAsync<SessionInfoResponse>();

                if (data == null)
                    return;

                CurrentSession.Company_Name = data.Company_Name_AR;
                CurrentSession.Branch_Name = data.Branch_Name;
                CurrentSession.Year_Name = data.Year_Name;

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

        private void BuildMainMenu()
        {
            tvMainMenu.BeginUpdate();

            try
            {
                tvMainMenu.Nodes.Clear();

                TreeNode adminNode = new("الإدارة العامة");
                AddMenuItem(adminNode, "Companies", "الشركات");
                AddMenuItem(adminNode, "Branches", "الفروع");
                AddMenuItem(adminNode, "FiscalYears", "السنوات المالية");
                AddMenuItem(adminNode, "Users", "المستخدمون والصلاحيات");
                AddMenuItem(adminNode, "Roles", "الأدوار");
                AddMenuItem(adminNode, "NumberingSettings", "إعدادات الترقيم");

                TreeNode accountingNode = new("الحسابات");
                AddMenuItem(accountingNode, "ChartOfAccounts", "الدليل المحاسبي");
                AddMenuItem(accountingNode, "Currencies", "العملات");
                AddMenuItem(accountingNode, "CostCenters", "مراكز التكلفة");
                AddMenuItem(accountingNode, "CashBoxes", "الصناديق");
                AddMenuItem(accountingNode, "ReceiptVoucher", "سند القبض");

                if (adminNode.Nodes.Count > 0)
                    tvMainMenu.Nodes.Add(adminNode);

                if (accountingNode.Nodes.Count > 0)
                    tvMainMenu.Nodes.Add(accountingNode);

                tvMainMenu.ExpandAll();

                if (tvMainMenu.Nodes.Count == 0)
                {
                    MessageBox.Show(
                        "لا توجد شاشات مصرح بها لهذا المستخدم. راجع صلاحيات الدور.",
                        "الصلاحيات",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            finally
            {
                tvMainMenu.EndUpdate();
            }
        }

        private static void AddMenuItem(TreeNode parent, string screenCode, string caption)
        {
            if (CurrentSession.CanViewScreen(screenCode))
                parent.Nodes.Add(screenCode, caption);
        }

        private void tvMainMenu_NodeMouseDoubleClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            string screenCode = e.Node.Name;
            if (string.IsNullOrWhiteSpace(screenCode))
                return;

            if (!CurrentSession.CanViewScreen(screenCode))
            {
                MessageBox.Show(
                    "ليس لديك صلاحية لفتح هذه الشاشة.",
                    "رفض الوصول",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            switch (screenCode)
            {
                case "Companies":
                    new CompanyForm().ShowDialog(this);
                    break;
                case "Branches":
                    new BranchForm().ShowDialog(this);
                    break;
                case "FiscalYears":
                    new FiscalYearForm().ShowDialog(this);
                    break;
                case "Users":
                    new FrmUsers().ShowDialog(this);
                    break;
                case "Roles":
                    new FrmRoles().ShowDialog(this);
                    break;
                case "NumberingSettings":
                    new FrmNumberingSettings().ShowDialog(this);
                    break;
                case "ChartOfAccounts":
                    new FrmChartOfAccounts().ShowDialog(this);
                    break;
                case "Currencies":
                    new FrmCurrencies().ShowDialog(this);
                    break;
                case "CostCenters":
                    new FrmCostCenters().ShowDialog(this);
                    break;
                case "CashBoxes":
                    new FrmCashBoxes().ShowDialog(this);
                    break;
                case "ReceiptVoucher":
                    new FrmReceiptVoucher().ShowDialog(this);
                    break;
            }
        }

        private void btnUsers_Click(object sender, EventArgs e)
        {
            if (CurrentSession.CanViewScreen("Users"))
                new FrmUsers().ShowDialog(this);
        }

        private void button10_Click(object sender, EventArgs e) { }
        private void btnAccountingCenter_Click(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void lblStatusTime_Click(object sender, EventArgs e) { }
        private void tvMainMenu_AfterSelect(object sender, TreeViewEventArgs e) { }
        private void flpMenu_Paint(object sender, PaintEventArgs e) { }
        private void pnlWorkspace_Paint(object sender, PaintEventArgs e) { }
        private void tvMainMenu_AfterSelect_1(object sender, TreeViewEventArgs e) { }

        private sealed class SessionInfoResponse
        {
            public string Company_ID { get; set; } = "";
            public string Company_Name_AR { get; set; } = "";
            public int Branch_ID { get; set; }
            public string Branch_Name { get; set; } = "";
            public string Year_Name { get; set; } = "";
        }
    }
}