// استدعاء مكتبات النظام الأساسية للتعامل مع الواجهات
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlTayerERP.Desktop.Services;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// الشاشة الرئيسية بعد تطويرها إلى مساحة عمل موحدة تعتمد التبويبات
    /// بدل فتح النوافذ المتكررة بشكل حواري منفصل.
    /// </summary>
    public partial class FrmMain : Form
    {
        private readonly HttpClient _client = ApiService.Client;
        private readonly string _baseUrl = ApiService.BaseUrl;
        private readonly Dictionary<string, Form> _openedForms = new();
        private readonly Timer _statusTimer = new();
        private TabControl? _workspaceTabs;
        private Label? _welcomeLabel;

        public FrmMain()
        {
            InitializeComponent();

            WireEvents();
            InitializeWorkspace();
            InitializeStatusBar();
            InitializeSessionSummary();
            BuildMainMenu();
            _ = LoadSessionDetailsAsync();
        }

        private void WireEvents()
        {
            tvMainMenu.NodeMouseDoubleClick -= tvMainMenu_NodeMouseDoubleClick;
            tvMainMenu.NodeMouseDoubleClick += tvMainMenu_NodeMouseDoubleClick;

            btnLogout.Click -= btnLogout_Click;
            btnLogout.Click += btnLogout_Click;

            btnAboutSystem.Click -= btnAboutSystem_Click;
            btnAboutSystem.Click += btnAboutSystem_Click;

            btnSettings.Click -= btnSettings_Click;
            btnSettings.Click += btnSettings_Click;

            btnNotifications.Click -= btnNotifications_Click;
            btnNotifications.Click += btnNotifications_Click;

            KeyPreview = true;
            KeyDown -= FrmMain_KeyDown;
            KeyDown += FrmMain_KeyDown;
        }

        private void InitializeWorkspace()
        {
            pnlWorkspace.Controls.Clear();
            pnlWorkspace.Padding = new Padding(12);
            pnlWorkspace.BackColor = Color.WhiteSmoke;

            _workspaceTabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Name = "tabWorkspace",
                RightToLeft = RightToLeft.Yes,
                RightToLeftLayout = true,
                Font = Font
            };
            _workspaceTabs.SelectedIndexChanged += (_, _) => RefreshWelcomePanelVisibility();
            pnlWorkspace.Controls.Add(_workspaceTabs);

            _welcomeLabel = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.White,
                Font = new Font(Font.FontFamily, 12F, FontStyle.Bold),
                Text = BuildWelcomeText()
            };
            pnlWorkspace.Controls.Add(_welcomeLabel);
            _welcomeLabel.BringToFront();
            RefreshWelcomePanelVisibility();
        }

        private void InitializeStatusBar()
        {
            lblVersion.Text = "الإصدار: 1.0.0";
            lblStatusApi.Text = "API: جاري الفحص";
            lblStatusDatabase.Text = "قاعدة البيانات: جاري الفحص";
            lblStatusLicense.Text = "الترخيص: ساري";

            _statusTimer.Interval = 1000;
            _statusTimer.Tick += (_, _) =>
            {
                lblStatusTime.Text = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss tt");
            };
            _statusTimer.Start();
        }

        private void InitializeSessionSummary()
        {
            lblCompanyName.Text = $"الشركة\n{SafeOrPlaceholder(CurrentSession.Company_Name, CurrentSession.Company_ID)}";
            lblCurrentBranch.Text = $"الفرع\n{SafeOrPlaceholder(CurrentSession.Branch_Name, CurrentSession.Branch_ID.ToString())}";
            lblFiscalYear.Text = $"السنة المالية\n{SafeOrPlaceholder(CurrentSession.Year_Name, CurrentSession.Year_ID.ToString())}";
            lblCurrentUser.Text = $"المستخدم\n{SafeOrPlaceholder(CurrentSession.Full_Name, CurrentSession.Username)}";
        }

        private async Task LoadSessionDetailsAsync()
        {
            try
            {
                string url = $"{_baseUrl}Branches/GetSessionInfo?companyId={CurrentSession.Company_ID}&branchId={CurrentSession.Branch_ID}&yearId={CurrentSession.Year_ID}";
                using HttpResponseMessage response = await _client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    lblStatusApi.Text = "API: متصل";
                    lblStatusDatabase.Text = "قاعدة البيانات: غير متاحة";
                    return;
                }

                SessionInfoResponse? data = await response.Content.ReadFromJsonAsync<SessionInfoResponse>();
                lblStatusApi.Text = "API: متصل";
                lblStatusDatabase.Text = "قاعدة البيانات: متصلة";

                if (data is null)
                {
                    return;
                }

                CurrentSession.Company_Name = data.Company_Name_AR;
                CurrentSession.Branch_Name = data.Branch_Name;
                CurrentSession.Year_Name = data.Year_Name;
                InitializeSessionSummary();
                if (_welcomeLabel is not null)
                {
                    _welcomeLabel.Text = BuildWelcomeText();
                }
            }
            catch
            {
                lblStatusApi.Text = "API: غير متصل";
                lblStatusDatabase.Text = "قاعدة البيانات: غير متاحة";
                InitializeSessionSummary();
            }
        }

        private string BuildWelcomeText()
        {
            return "مرحبًا بك في مساحة العمل الموحدة\n\n"
                   + $"الشركة: {SafeOrPlaceholder(CurrentSession.Company_Name, CurrentSession.Company_ID)}\n"
                   + $"الفرع: {SafeOrPlaceholder(CurrentSession.Branch_Name, CurrentSession.Branch_ID.ToString())}\n"
                   + $"السنة المالية: {SafeOrPlaceholder(CurrentSession.Year_Name, CurrentSession.Year_ID.ToString())}\n"
                   + $"المستخدم: {SafeOrPlaceholder(CurrentSession.Full_Name, CurrentSession.Username)}\n\n"
                   + "انقر نقراً مزدوجاً على أي شاشة من القائمة اليمنى لفتحها داخل تبويب.\n"
                   + "Ctrl + W لإغلاق التبويب الحالي، و F5 لتحديث الحالة.";
        }

        private static string SafeOrPlaceholder(string? preferred, string fallback)
        {
            return string.IsNullOrWhiteSpace(preferred) ? fallback : preferred;
        }

        private void BuildMainMenu()
        {
            tvMainMenu.Nodes.Clear();

            TreeNode adminNode = new TreeNode("الإدارة العامة");
            adminNode.Nodes.Add("Companies", "الشركات");
            adminNode.Nodes.Add("Branches", "الفروع");
            adminNode.Nodes.Add("FiscalYears", "السنوات المالية");
            adminNode.Nodes.Add("Users", "المستخدمون والصلاحيات");
            adminNode.Nodes.Add("Roles", "الأدوار");
            adminNode.Nodes.Add("NumberingSettings", "إعدادات الترقيم");

            TreeNode accountingNode = new TreeNode("الحسابات");
            accountingNode.Nodes.Add("ChartOfAccounts", "الدليل المحاسبي");
            accountingNode.Nodes.Add("Currencies", "العملات");
            accountingNode.Nodes.Add("CostCenters", "مراكز التكلفة");
            accountingNode.Nodes.Add("CashBoxes", "الصناديق");
            accountingNode.Nodes.Add("ReceiptVoucher", "سند القبض");
            accountingNode.Nodes.Add("PaymentVoucher", "سند الصرف");

            tvMainMenu.Nodes.Add(adminNode);
            tvMainMenu.Nodes.Add(accountingNode);
            tvMainMenu.ExpandAll();
        }

        private void tvMainMenu_NodeMouseDoubleClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Node.Name))
            {
                return;
            }

            OpenScreen(e.Node.Name, e.Node.Text);
        }

        private void OpenScreen(string screenCode, string caption)
        {
            if (_workspaceTabs is null)
            {
                return;
            }

            if (_openedForms.TryGetValue(screenCode, out Form? existingForm))
            {
                foreach (TabPage page in _workspaceTabs.TabPages)
                {
                    if (page.Tag == existingForm)
                    {
                        _workspaceTabs.SelectedTab = page;
                        existingForm.Focus();
                        return;
                    }
                }

                _openedForms.Remove(screenCode);
            }

            Form? form = CreateScreenInstance(screenCode);
            if (form is null)
            {
                MessageBox.Show("هذه الشاشة غير متاحة بعد في مساحة العمل.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            form.RightToLeft = RightToLeft.Yes;
            form.RightToLeftLayout = true;

            TabPage tabPage = new TabPage(caption)
            {
                Tag = form,
                ToolTipText = caption
            };
            tabPage.Controls.Add(form);
            _workspaceTabs.TabPages.Add(tabPage);
            _workspaceTabs.SelectedTab = tabPage;

            form.FormClosed += (_, _) => CloseTab(screenCode);
            _openedForms[screenCode] = form;
            form.Show();
            RefreshWelcomePanelVisibility();
        }

        private Form? CreateScreenInstance(string screenCode)
        {
            return screenCode switch
            {
                "Companies" => new CompanyForm(),
                "Branches" => new BranchForm(),
                "FiscalYears" => new FiscalYearForm(),
                "Users" => new FrmUsers(),
                "Roles" => new FrmRoles(),
                "NumberingSettings" => new FrmNumberingSettings(),
                "ChartOfAccounts" => new FrmChartOfAccounts(),
                "Currencies" => new FrmCurrencies(),
                "CostCenters" => new FrmCostCenters(),
                "CashBoxes" => new FrmCashBoxes(),
                "ReceiptVoucher" => new FrmReceiptVoucher(),
                "PaymentVoucher" => new FrmPaymentVoucher(),
                _ => null
            };
        }

        private void CloseTab(string screenCode)
        {
            if (_workspaceTabs is null)
            {
                return;
            }

            if (!_openedForms.TryGetValue(screenCode, out Form? form))
            {
                return;
            }

            TabPage? targetPage = null;
            foreach (TabPage page in _workspaceTabs.TabPages)
            {
                if (page.Tag == form)
                {
                    targetPage = page;
                    break;
                }
            }

            if (targetPage is not null)
            {
                _workspaceTabs.TabPages.Remove(targetPage);
                targetPage.Dispose();
            }

            _openedForms.Remove(screenCode);
            RefreshWelcomePanelVisibility();
        }

        private void RefreshWelcomePanelVisibility()
        {
            if (_workspaceTabs is null || _welcomeLabel is null)
            {
                return;
            }

            _welcomeLabel.Visible = _workspaceTabs.TabPages.Count == 0;
            if (_welcomeLabel.Visible)
            {
                _welcomeLabel.BringToFront();
            }
        }

        private void btnLogout_Click(object? sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "هل تريد تسجيل الخروج من النظام؟",
                "تأكيد",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            CurrentSession.Clear();
            Hide();
            FrmLogin login = new FrmLogin();
            login.Show();
            Close();
        }

        private void btnAboutSystem_Click(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "نظام الطائر لإدارة النقل والشحن\nمساحة العمل الموحدة - المرحلة الأولى",
                "حول النظام",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void btnSettings_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("سيتم ربط مركز الإعدادات الموحد في الدفعة التالية.", "الإعدادات", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnNotifications_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("سيتم ربط صندوق المهام والإشعارات في مرحلة مسارات الاعتماد.", "الإشعارات", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void FrmMain_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.W)
            {
                CloseCurrentTab();
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.F5)
            {
                _ = LoadSessionDetailsAsync();
                e.SuppressKeyPress = true;
            }
        }

        private void CloseCurrentTab()
        {
            if (_workspaceTabs is null || _workspaceTabs.SelectedTab is null)
            {
                return;
            }

            TabPage currentTab = _workspaceTabs.SelectedTab;
            if (currentTab.Tag is Form form)
            {
                string? screenCode = null;
                foreach (KeyValuePair<string, Form> pair in _openedForms)
                {
                    if (pair.Value == form)
                    {
                        screenCode = pair.Key;
                        break;
                    }
                }

                form.Close();
                if (screenCode is not null)
                {
                    _openedForms.Remove(screenCode);
                }
            }
            else
            {
                _workspaceTabs.TabPages.Remove(currentTab);
            }

            RefreshWelcomePanelVisibility();
        }

        private sealed class SessionInfoResponse
        {
            public string Company_ID { get; set; } = string.Empty;
            public string Company_Name_AR { get; set; } = string.Empty;
            public int Branch_ID { get; set; }
            public string Branch_Name { get; set; } = string.Empty;
            public string Year_Name { get; set; } = string.Empty;
        }

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
