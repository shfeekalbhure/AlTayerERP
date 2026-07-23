using System;
using System.Collections.Generic;
using System.Drawing;
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
        // مساحة عمل موحّدة: شاشة إعداد واحدة = تبويب واحد فقط.
        // يمنع ذلك فقدان حالة الشاشة أو فتح نسخ مكررة من بيانات مرجعية حساسة.
        private readonly TabControl _workspaceTabs = new();
        private readonly Dictionary<string, TabPage> _openTabs = new(StringComparer.OrdinalIgnoreCase);

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
            ApplyMainShellVisuals();
            InitializeWorkspaceTabs();
            BuildDashboard();
            BuildMainMenu();
            _ = LoadAllowedScreensAsync();
            tvMainMenu.NodeMouseDoubleClick -= tvMainMenu_NodeMouseDoubleClick;
            tvMainMenu.NodeMouseDoubleClick += tvMainMenu_NodeMouseDoubleClick;
        }

        private void InitializeWorkspaceTabs()
        {
            _workspaceTabs.Dock = DockStyle.Fill;
            _workspaceTabs.RightToLeft = RightToLeft.Yes;
            _workspaceTabs.RightToLeftLayout = true;
            _workspaceTabs.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _workspaceTabs.Padding = new Point(16, 5);
            _workspaceTabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            _workspaceTabs.DrawItem += WorkspaceTabs_DrawItem;
            _workspaceTabs.MouseDown += WorkspaceTabs_MouseDown;

            var menu = new ContextMenuStrip();
            menu.Items.Add("إغلاق التبويب", null, (_, _) => CloseSelectedTab());
            menu.Items.Add("إغلاق الكل", null, (_, _) => CloseAllTabs(keepHome: true));
            menu.Items.Add("إغلاق ما عدا هذا", null, (_, _) => CloseAllExceptSelected());
            _workspaceTabs.ContextMenuStrip = menu;

            pnlWorkspace.Controls.Clear();
            pnlWorkspace.Controls.Add(_workspaceTabs);
        }

        private void WorkspaceTabs_DrawItem(object? sender, DrawItemEventArgs e)
        {
            var page = _workspaceTabs.TabPages[e.Index];
            var selected = e.Index == _workspaceTabs.SelectedIndex;
            using var background = new SolidBrush(selected ? Color.White : Color.FromArgb(231, 237, 244));
            using var text = new SolidBrush(selected ? Color.FromArgb(8, 49, 92) : Color.FromArgb(67, 80, 96));
            e.Graphics.FillRectangle(background, e.Bounds);
            var caption = page.Text + (page.Name == "Home" ? string.Empty : "   ×");
            TextRenderer.DrawText(e.Graphics, caption, _workspaceTabs.Font, e.Bounds, text.Color,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.RightToLeft);
        }

        private void WorkspaceTabs_MouseDown(object? sender, MouseEventArgs e)
        {
            for (var index = 0; index < _workspaceTabs.TabPages.Count; index++)
            {
                if (!_workspaceTabs.GetTabRect(index).Contains(e.Location)) continue;
                _workspaceTabs.SelectedIndex = index;
                // زر الإغلاق المرئي في طرف التبويب؛ الصفحة الرئيسية لا تغلق.
                if (e.Button == MouseButtons.Left && _workspaceTabs.TabPages[index].Name != "Home" &&
                    e.X >= _workspaceTabs.GetTabRect(index).Right - 28)
                    CloseTab(_workspaceTabs.TabPages[index]);
                return;
            }
        }

        /// <summary>
        /// تصميم الواجهة الرئيسية وفق هوية الطائر: شريط علوي كحلي،
        /// قائمة جانبية ثابتة، ومساحة عمل هادئة مناسبة للشاشات المحاسبية.
        /// </summary>
        private void ApplyMainShellVisuals()
        {
            var navy = Color.FromArgb(8, 49, 92);
            var navyDark = Color.FromArgb(5, 36, 69);
            var blue = Color.FromArgb(20, 102, 190);

            // اتجاه التطبيق عربي: شجرة النظام في اليمين ومساحة العمل في اليسار.
            RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            pnlSideMenu.Dock = DockStyle.Right;
            pnlSideMenu.Width = 235;
            pnlWorkspace.Dock = DockStyle.Fill;

            // ألوان أخف لتقليل كثافة الواجهة مع المحافظة على هوية الطائر.
            pnlTopBar.BackColor = navy;
            pnlSideMenu.BackColor = navyDark;
            pnlWorkspace.BackColor = Color.FromArgb(249, 250, 252);
            pnlStatusBar.BackColor = Color.White;
            pnlStatusBar.BorderStyle = BorderStyle.FixedSingle;

            foreach (var label in new[] { lblCompanyName, lblCurrentBranch, lblFiscalYear, lblCurrentUser })
            {
                label.ForeColor = Color.White;
                label.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            }

            foreach (var button in new[] { btnSettings, btnNotifications, btnAboutSystem })
            {
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                button.BackColor = navy;
                button.ForeColor = Color.White;
                button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            }

            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.BackColor = Color.FromArgb(202, 48, 45);
            btnLogout.ForeColor = Color.White;
            btnLogout.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            tvMainMenu.BackColor = navyDark;
            tvMainMenu.ForeColor = Color.White;
            tvMainMenu.BorderStyle = BorderStyle.None;
            tvMainMenu.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            // إظهار التفرعات فعلياً حتى تبدو القائمة كشجرة نظام واضحة.
            tvMainMenu.LineColor = Color.FromArgb(96, 144, 186);
            tvMainMenu.ShowLines = true;
            tvMainMenu.ShowPlusMinus = true;
            tvMainMenu.ShowRootLines = true;
            tvMainMenu.Indent = 24;
            tvMainMenu.ItemHeight = 32;
            tvMainMenu.HotTracking = true;
            tvMainMenu.RightToLeft = System.Windows.Forms.RightToLeft.Yes;

            lblStatusApi.ForeColor = Color.FromArgb(0, 132, 78);
            lblStatusDatabase.ForeColor = Color.FromArgb(0, 132, 78);
            lblStatusLicense.ForeColor = navy;
            lblVersion.ForeColor = navy;
            lblStatusTime.ForeColor = Color.FromArgb(70, 80, 90);
        }

        /// <summary>
        /// لوحة مختصرة كبداية للشاشة الرئيسية. الأرقام لا تكون وهمية:
        /// لذلك تعرض بطاقات الوصول السريع إلى أن يكتمل مؤشر العمليات.
        /// </summary>
        private void BuildDashboard()
        {
            _workspaceTabs.TabPages.Clear();
            _openTabs.Clear();

            var home = new TabPage("الرئيسية")
            {
                Name = "Home",
                BackColor = Color.FromArgb(249, 250, 252),
                Padding = Padding.Empty
            };

            var shell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18),
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Color.FromArgb(249, 250, 252)
            };
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            shell.Controls.Add(new Label
            {
                Text = "لوحة الملخص",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(8, 49, 92),
                TextAlign = ContentAlignment.MiddleRight
            }, 0, 0);

            var cards = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(0, 8, 0, 8)
            };
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            cards.Controls.Add(CreateDashboardCard("سندات القبض", "إنشاء وحفظ ومراجعة السندات", "▣", Color.FromArgb(38, 107, 201), () => OpenScreen("ReceiptVoucher")), 0, 0);
            cards.Controls.Add(CreateDashboardCard("التهيئة والإعدادات", "الشركة والفرع والسنة والصلاحيات", "⚙", Color.FromArgb(33, 141, 103), () => OpenScreen("GeneralSettings")), 1, 0);
            cards.Controls.Add(CreateDashboardCard("الدليل المحاسبي", "الحسابات ومراكز التكلفة والعملات", "▤", Color.FromArgb(140, 84, 184), () => OpenScreen("ChartOfAccounts")), 2, 0);
            shell.Controls.Add(cards, 0, 1);

            var quick = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(18)
            };
            quick.Controls.Add(new Label
            {
                Text = "العمليات السريعة",
                Dock = DockStyle.Top,
                Height = 34,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(8, 49, 92),
                TextAlign = ContentAlignment.MiddleRight
            });

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 70,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = true,
                Padding = new Padding(0, 12, 0, 0)
            };
            buttons.Controls.Add(CreateQuickAction("سند قبض جديد", Color.FromArgb(22, 125, 84), () => OpenScreen("ReceiptVoucher")));
            buttons.Controls.Add(CreateQuickAction("دليل الحسابات", Color.FromArgb(20, 102, 190), () => OpenScreen("ChartOfAccounts")));
            buttons.Controls.Add(CreateQuickAction("المستخدمون والصلاحيات", Color.FromArgb(107, 70, 160), () => OpenScreen("Users")));
            buttons.Controls.Add(CreateQuickAction("الإعدادات", Color.FromArgb(78, 89, 101), () => OpenScreen("GeneralSettings")));
            quick.Controls.Add(buttons);
            shell.Controls.Add(quick, 0, 2);

            shell.Controls.Add(new Label
            {
                Text = "يتم عرض مؤشرات التشغيل الفعلية هنا بعد اكتمال وحدة التقارير، دون أرقام تجريبية.",
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(95, 105, 115),
                TextAlign = ContentAlignment.MiddleRight
            }, 0, 3);

            home.Controls.Add(shell);
            _workspaceTabs.TabPages.Add(home);
            _workspaceTabs.SelectedTab = home;
        }

        private static Control CreateDashboardCard(string title, string description, string icon, Color accent, Action click)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(7),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Cursor = Cursors.Hand,
                Padding = new Padding(16)
            };

            card.Controls.Add(new Label
            {
                Text = icon,
                Dock = DockStyle.Left,
                Width = 58,
                Font = new Font("Segoe UI Symbol", 26F),
                ForeColor = accent,
                TextAlign = ContentAlignment.MiddleCenter
            });
            card.Controls.Add(new Label
            {
                Text = description,
                Dock = DockStyle.Bottom,
                Height = 34,
                ForeColor = Color.FromArgb(95, 105, 115),
                TextAlign = ContentAlignment.MiddleRight
            });
            card.Controls.Add(new Label
            {
                Text = title,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 40, 55),
                TextAlign = ContentAlignment.MiddleRight
            });

            card.Click += (_, _) => click();
            foreach (Control child in card.Controls)
                child.Click += (_, _) => click();
            return card;
        }

        private static Button CreateQuickAction(string text, Color color, Action click)
        {
            var button = new Button
            {
                Text = text,
                Width = 185,
                Height = 40,
                Margin = new Padding(5),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            button.Click += (_, _) => click();
            return button;
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
                // هذا المسار يطبق الدور والجلسة في الخادم؛ لا نجلب كل الكتالوج ثم
                // نقرر الصلاحية في سطح المكتب.
                var screens = await _client.GetFromJsonAsync<List<ScreenAccessRow>>(
                    $"{_baseUrl}SystemScreens") ?? new();

                _allowedScreenCodes = screens
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
            // التسلسل ظاهر كما يُنفذ: المجموعة ← الشركة ← نوع الفرع ← الفرع.
            TreeNode organizationNode = new("الهيكل المؤسسي");
            AddScreen(organizationNode, "BusinessGroups", "المجموعات التجارية");
            AddScreen(organizationNode, "Companies", "الشركات");
            AddScreen(organizationNode, "BranchTypes", "إعدادات أنواع الفروع");
            AddScreen(organizationNode, "Branches", "الفروع");
            if (organizationNode.Nodes.Count > 0) adminNode.Nodes.Add(organizationNode);

            // السنة والفترة تابعة للشركة في تسلسل التهيئة، وليست إعداداً مالياً عاماً.
            TreeNode fiscalNode = new("السنوات والفترات المالية");
            AddScreen(fiscalNode, "FiscalYears", "السنوات المالية");
            AddScreen(fiscalNode, "FiscalPeriods", "الفترات المالية");
            if (fiscalNode.Nodes.Count > 0) adminNode.Nodes.Add(fiscalNode);

            TreeNode securityNode = new("الأمن والصلاحيات");
            AddScreen(securityNode, "Users", "المستخدمون");
            AddScreen(securityNode, "Roles", "الأدوار");
            AddScreen(securityNode, "RolePermissions", "صلاحيات الأدوار");
            // كتالوج الشاشات هو مصدر شجرة النظام وصلاحياتها، لذلك مكانه الطبيعي هنا.
            AddScreen(securityNode, "SystemScreens", "شجرة النظام / كتالوج الشاشات");
            if (securityNode.Nodes.Count > 0) adminNode.Nodes.Add(securityNode);

            TreeNode accountingNode = new("الحسابات");
            AddScreen(accountingNode, "ChartOfAccounts", "الدليل المحاسبي");
            AddScreen(accountingNode, "CostCenters", "مراكز التكلفة");
            AddScreen(accountingNode, "CashBoxes", "الصناديق");
            AddScreen(accountingNode, "ReceiptVoucher", "سند القبض");

            // شاشات التهيئة الحساسة مخصصة لمدير النظام إلى أن يكتمل محرك الصلاحيات التفصيلي.
            if (CurrentSession.Is_System_Admin)
            {
                TreeNode setupNode = new("التهيئة والإعدادات");
                AddScreen(setupNode, "Currencies", "العملات");
                AddScreen(setupNode, "ExchangeRates", "أسعار الصرف");
                AddScreen(setupNode, "GeneralSettings", "الإعدادات العامة والمالية");
                AddScreen(setupNode, "NumberingSettings", "إعدادات الترقيم");
                AddScreen(setupNode, "PaymentMethods", "طرق السداد");
                AddScreen(setupNode, "VoucherTypes", "أنواع السندات");
                AddScreen(setupNode, "VoucherStatuses", "حالات السندات");
                AddScreen(setupNode, "ApprovalPolicies", "سياسات الاعتماد والسقوف");
                adminNode.Nodes.Add(setupNode);

                AddScreen(accountingNode, "Banks", "البنوك والحسابات البنكية");
                AddScreen(accountingNode, "Parties", "الأطراف المالية");
            }

            // لا نعرض قسماً فارغاً للمستخدم إذا لم تكن له أي شاشة مسموح بها داخله.
            if (adminNode.Nodes.Count > 0)
                tvMainMenu.Nodes.Add(adminNode);

            if (accountingNode.Nodes.Count > 0)
                tvMainMenu.Nodes.Add(accountingNode);

            // نفتح مستوى الأقسام فقط؛ تبقى الفروع الداخلية قابلة للفتح والإغلاق.
            tvMainMenu.CollapseAll();
            foreach (TreeNode root in tvMainMenu.Nodes)
                root.Expand();
        }

        private void tvMainMenu_NodeMouseDoubleClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            // الضغط على قسم يفتح/يغلق فروعه، أما الورقة فتفتح الشاشة.
            if (e.Node.Nodes.Count > 0)
            {
                e.Node.Toggle();
                return;
            }

            OpenScreen(e.Node.Name);
        }

        /// <summary>يفتح الشاشة بعد تطبيق صلاحية العرض الحالية.</summary>
        private void OpenScreen(string screenCode)
        {
            if (string.IsNullOrWhiteSpace(screenCode) || !CanOpenScreen(screenCode))
            {
                MessageBox.Show("ليس لديك صلاحية لفتح هذه الشاشة.", "الصلاحيات", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_openTabs.TryGetValue(screenCode, out var existingPage))
            {
                _workspaceTabs.SelectedTab = existingPage;
                return;
            }

            Form? form = screenCode switch
            {
                "BusinessGroups" => new FrmBusinessGroups(),
                "Companies" => new CompanyForm(),
                "BranchTypes" => new FrmBranchTypes(),
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
            if (form == null)
                return;

            var tab = new TabPage(form.Text)
            {
                Name = screenCode,
                BackColor = Color.FromArgb(249, 250, 252),
                Padding = Padding.Empty
            };

            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            form.Visible = true;
            form.FormClosed += (_, _) =>
            {
                _openTabs.Remove(screenCode);
                if (_workspaceTabs.TabPages.Contains(tab))
                    _workspaceTabs.TabPages.Remove(tab);
                tab.Dispose();
            };

            tab.Controls.Add(form);
            _workspaceTabs.TabPages.Add(tab);
            _openTabs[screenCode] = tab;
            _workspaceTabs.SelectedTab = tab;
        }

        private void CloseSelectedTab()
        {
            if (_workspaceTabs.SelectedTab != null)
                CloseTab(_workspaceTabs.SelectedTab);
        }

        private void CloseTab(TabPage tab)
        {
            if (tab.Name == "Home") return;
            if (tab.Controls.OfType<Form>().FirstOrDefault() is { } form)
                form.Close();
            else
                _workspaceTabs.TabPages.Remove(tab);
        }

        private void CloseAllTabs(bool keepHome)
        {
            foreach (var tab in _workspaceTabs.TabPages.Cast<TabPage>().ToList())
                if (!keepHome || tab.Name != "Home") CloseTab(tab);
        }

        private void CloseAllExceptSelected()
        {
            var selected = _workspaceTabs.SelectedTab;
            foreach (var tab in _workspaceTabs.TabPages.Cast<TabPage>().ToList())
                if (tab != selected && tab.Name != "Home") CloseTab(tab);
        }

        // إنهاء الجلسة المحلية وإرجاع المستخدم إلى شاشة الدخول.
        private async void btnLogout_Click(object? sender, EventArgs e)
        {
            if (MessageBox.Show("هل تريد تسجيل الخروج من الجلسة الحالية؟", "تسجيل الخروج", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                // حتى عند فشل الشبكة نمسح الجلسة محلياً، والخادم ينهيها تلقائياً بانتهاء مدتها.
                await _client.PostAsync("Auth/Logout", content: null);
            }
            catch
            {
                // لا تمنع المستخدم من الخروج المحلي بسبب تعذر الاتصال.
            }

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

            OpenScreen("GeneralSettings");
        }

        private void btnUsers_Click(object sender, EventArgs e) => OpenScreen("Users");
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
