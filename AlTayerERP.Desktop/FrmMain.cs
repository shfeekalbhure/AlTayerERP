using AlTayerERP.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// الغلاف الرئيسي للنظام. يعرض سياق الجلسة الموثوق، ويبني القائمة من الصلاحيات
    /// الفعلية، ويفتح الشاشات في تبويبات تمنع النسخ المكررة.
    /// </summary>
    public partial class FrmMain : Form
    {
        private readonly HttpClient _client = ApiService.Client;
        private readonly MainWorkspaceManager _workspace;
        private readonly System.Windows.Forms.Timer _statusTimer = new() { Interval = 30_000 };
        private readonly System.Windows.Forms.Timer _sessionTimer = new() { Interval = 60_000 };
        private readonly List<ScreenAccessRow> _allowedScreens = new();
        private bool _allowClose;
        private bool _loggingOut;

        public FrmMain()
        {
            InitializeComponent();
            ApplyMainShellVisuals();
            _workspace = new MainWorkspaceManager(pnlWorkspace);

            // لا تفتح الشاشة الرئيسية من المصمم أو من Program من دون جلسة مكتملة.
            if (!CurrentSession.IsLoggedIn)
            {
                MessageBox.Show("انتهت الجلسة أو أن بياناتها غير مكتملة. سجل الدخول من جديد.",
                    "الجلسة", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                BeginInvoke(new Action(Close));
                return;
            }

            WireEvents();
            PopulateHeaderFromSession();
            BuildDashboard();

            _statusTimer.Tick += async (_, _) => await RefreshShellStatusAsync();
            _sessionTimer.Tick += async (_, _) => await RefreshAccessTokenIfNeededAsync();
            _statusTimer.Start();
            _sessionTimer.Start();
            _ = InitializeShellAsync();
        }

        /// <summary>تهيئة الغلاف بعد عرضه دون تعطيل واجهة المستخدم.</summary>
        private async Task InitializeShellAsync()
        {
            await Task.WhenAll(
                LoadSessionDetailsAsync(),
                RefreshShellStatusAsync(),
                LoadAllowedScreensAsync());

            BuildMainMenu();
            BuildDashboard();
        }

        private void WireEvents()
        {
            btnLogout.Click += async (_, _) => await LogoutAsync(showConfirmation: true);
            btnSettings.Click += (_, _) => OpenScreen("GeneralSettings");
            btnNotifications.Click += (_, _) => ShowNotifications();
            btnAboutSystem.Click += (_, _) => ShowAbout();
            tvMainMenu.NodeMouseDoubleClick += tvMainMenu_NodeMouseDoubleClick;
            tvMainMenu.KeyDown += tvMainMenu_KeyDown;
            FormClosing += FrmMain_FormClosing;
            FormClosed += (_, _) =>
            {
                _statusTimer.Stop();
                _sessionTimer.Stop();
                _workspace.Dispose();
                _statusTimer.Dispose();
                _sessionTimer.Dispose();
            };
        }

        /// <summary>هوية موحدة RTL للغلاف الرئيسي.</summary>
        private void ApplyMainShellVisuals()
        {
            var navy = Color.FromArgb(8, 49, 92);
            var navyDark = Color.FromArgb(5, 36, 69);

            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            BackColor = Color.FromArgb(249, 250, 252);
            pnlTopBar.BackColor = navy;
            pnlSideMenu.BackColor = navyDark;
            pnlWorkspace.BackColor = Color.FromArgb(249, 250, 252);
            pnlStatusBar.BackColor = Color.White;
            pnlStatusBar.BorderStyle = BorderStyle.FixedSingle;
            pnlSideMenu.Dock = DockStyle.Right;
            pnlSideMenu.Width = 250;

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
            btnLogout.BackColor = Color.FromArgb(190, 50, 48);
            btnLogout.ForeColor = Color.White;
            btnLogout.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            tvMainMenu.BackColor = navyDark;
            tvMainMenu.ForeColor = Color.White;
            tvMainMenu.BorderStyle = BorderStyle.None;
            tvMainMenu.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            tvMainMenu.LineColor = Color.FromArgb(96, 144, 186);
            tvMainMenu.FullRowSelect = true;
            tvMainMenu.HideSelection = false;
            tvMainMenu.ShowLines = true;
            tvMainMenu.ShowPlusMinus = true;
            tvMainMenu.ShowRootLines = true;
            tvMainMenu.ItemHeight = 34;
            tvMainMenu.HotTracking = true;

            lblStatusApi.ForeColor = Color.FromArgb(0, 132, 78);
            lblStatusDatabase.ForeColor = Color.FromArgb(0, 132, 78);
            lblStatusLicense.ForeColor = navy;
            lblVersion.ForeColor = navy;
            lblStatusTime.ForeColor = Color.FromArgb(70, 80, 90);
        }

        /// <summary>يعرض سياق الدخول الذاكري مؤقتاً إلى أن يعيد API أسماءه الرسمية.</summary>
        private void PopulateHeaderFromSession()
        {
            lblCompanyName.Text = "الشركة\n" + (string.IsNullOrWhiteSpace(CurrentSession.Company_Name)
                ? CurrentSession.Company_ID : CurrentSession.Company_Name);
            lblCurrentBranch.Text = "الفرع\n" + (string.IsNullOrWhiteSpace(CurrentSession.Branch_Name)
                ? CurrentSession.Branch_ID.ToString() : CurrentSession.Branch_Name);
            lblFiscalYear.Text = "السنة المالية\n" + (string.IsNullOrWhiteSpace(CurrentSession.Year_Name)
                ? CurrentSession.Year_ID.ToString() : CurrentSession.Year_Name);
            lblCurrentUser.Text = "المستخدم\n" + CurrentSession.Full_Name;
            lblStatusLicense.Text = "الترخيص: ساري";
            lblVersion.Text = "الإصدار: 4.2";
            lblStatusTime.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        }

        /// <summary>يسحب أسماء الشركة والفرع والسنة وفق سياق الجلسة، لا وفق مدخلات مستخدم.</summary>
        private async Task LoadSessionDetailsAsync()
        {
            try
            {
                var response = await _client.GetAsync(
                    $"Branches/GetSessionInfo?companyId={Uri.EscapeDataString(CurrentSession.Company_ID)}&branchId={CurrentSession.Branch_ID}&yearId={CurrentSession.Year_ID}");

                if (!response.IsSuccessStatusCode)
                    return;

                var data = await response.Content.ReadFromJsonAsync<SessionInfoResponse>();
                if (data == null)
                    return;

                CurrentSession.Company_Name = data.Company_Name_AR;
                CurrentSession.Branch_Name = data.Branch_Name;
                CurrentSession.Year_Name = data.Year_Name;
                PopulateHeaderFromSession();
            }
            catch
            {
                // تبقى القيم التي جرى التحقق منها عند الدخول ظاهرة حتى يعود الاتصال.
            }
        }

        /// <summary>يحدث شريط الحالة والوقت من حالة API الحقيقية.</summary>
        private async Task RefreshShellStatusAsync()
        {
            lblStatusTime.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

            try
            {
                var response = await _client.GetAsync("health");
                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException();

                lblStatusApi.Text = "API: متصل";
                lblStatusApi.ForeColor = Color.FromArgb(0, 132, 78);
                lblStatusDatabase.Text = "قاعدة البيانات: متصلة";
                lblStatusDatabase.ForeColor = Color.FromArgb(0, 132, 78);
            }
            catch
            {
                lblStatusApi.Text = "API: غير متصل";
                lblStatusApi.ForeColor = Color.Firebrick;
                lblStatusDatabase.Text = "قاعدة البيانات: غير متصلة";
                lblStatusDatabase.ForeColor = Color.Firebrick;
            }
        }

        /// <summary>
        /// يجلب API قائمة الشاشات المسموح بها للدور. مدير النظام يستخدم القائمة
        /// النشطة نفسها، لكنه لا يتقيد بمصفوفة الدور مؤقتاً وفق السياسة المعتمدة.
        /// </summary>
        private async Task LoadAllowedScreensAsync()
        {
            _allowedScreens.Clear();

            try
            {
                var rows = await _client.GetFromJsonAsync<List<ScreenAccessRow>>("SystemScreens") ?? new();
                _allowedScreens.AddRange(rows.Where(x => x.Is_Active && IsSupportedScreen(x.Screen_Code)));
            }
            catch
            {
                // لا تمنح صلاحيات بديلة لمستخدم عادي. مدير النظام فقط يملك تجاوز التطوير المعتمد.
                if (CurrentSession.Is_System_Admin)
                    _allowedScreens.AddRange(KnownScreens);
                else
                    MessageBox.Show("تعذر تحميل صلاحيات الدور؛ أخفيت القوائم لحماية النظام.",
                        "الصلاحيات", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool CanOpenScreen(string screenCode) =>
            CurrentSession.Is_System_Admin
                ? IsSupportedScreen(screenCode)
                : _allowedScreens.Any(x => string.Equals(x.Screen_Code, screenCode, StringComparison.OrdinalIgnoreCase));

        /// <summary>يبني شجرة القائمة من وحدة الكتالوج وصلاحية العرض المرتبطة بالدور.</summary>
        private void BuildMainMenu()
        {
            tvMainMenu.BeginUpdate();
            try
            {
                tvMainMenu.Nodes.Clear();
                var rows = _allowedScreens
                    .Where(x => IsSupportedScreen(x.Screen_Code))
                    .OrderBy(x => x.Module_Name)
                    .ThenBy(x => x.Sort_Order)
                    .ThenBy(x => x.Screen_Name)
                    .ToList();

                foreach (var module in rows.GroupBy(x => string.IsNullOrWhiteSpace(x.Module_Name)
                             ? "شاشات النظام" : x.Module_Name))
                {
                    var root = new TreeNode(module.Key);
                    foreach (var screen in module)
                        root.Nodes.Add(screen.Screen_Code, screen.Screen_Name);

                    if (root.Nodes.Count > 0)
                        tvMainMenu.Nodes.Add(root);
                }

                foreach (TreeNode node in tvMainMenu.Nodes)
                    node.Expand();
            }
            finally
            {
                tvMainMenu.EndUpdate();
            }
        }

        private void tvMainMenu_NodeMouseDoubleClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Nodes.Count > 0)
            {
                e.Node.Toggle();
                return;
            }

            OpenScreen(e.Node.Name);
        }

        private void tvMainMenu_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && tvMainMenu.SelectedNode is { Nodes.Count: 0 } node)
            {
                OpenScreen(node.Name);
                e.Handled = true;
            }
        }

        /// <summary>
        /// يفتح الشاشة في تبويب مركزي؛ الاستدعاء المتكرر لنفس Screen_Code ينشط
        /// التبويب الحالي ولا ينشئ نسخة ثانية.
        /// </summary>
        private void OpenScreen(string screenCode, string? recordKey = null)
        {
            if (!CanOpenScreen(screenCode))
            {
                MessageBox.Show("ليس لديك صلاحية لفتح هذه الشاشة.",
                    "الصلاحيات", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var factory = GetScreenFactory(screenCode);
            if (factory == null)
            {
                MessageBox.Show("هذه الشاشة غير متاحة في حزمة المرحلة الأولى الحالية.",
                    "الشاشة", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var caption = _allowedScreens.FirstOrDefault(x =>
                string.Equals(x.Screen_Code, screenCode, StringComparison.OrdinalIgnoreCase))?.Screen_Name
                ?? GetKnownScreen(screenCode)?.Screen_Name
                ?? screenCode;

            _workspace.Open(screenCode, caption, factory, recordKey);
        }

        private static Func<Form>? GetScreenFactory(string screenCode) =>
            screenCode switch
            {
                "TenantGroups" => () => new FrmTenantGroups(),
                "Companies" => () => new CompanyForm(),
                "Branches" => () => new BranchForm(),
                "Countries" => () => new FrmCountries(),
                "Governorates" => () => new FrmGovernorates(),
                "Cities" => () => new FrmCities(),
                "FiscalYears" => () => new FiscalYearForm(),
                "Users" => () => new FrmUsers(),
                "Roles" => () => new FrmRoles(),
                "RolePermissions" => () => new FrmRolePermissions(),
                "AuditLogs" => () => new FrmAuditLogs(),
                "Sessions" => () => new FrmSessions(),
                "GeneralSettings" => () => new FrmGeneralSettings(),
                "SystemScreens" => () => new FrmSystemScreens(),
                "NumberingSettings" => () => new FrmNumberingSettings(),
                "FiscalPeriods" => () => new FrmFiscalPeriods(),
                "ExchangeRates" => () => new FrmExchangeRates(),
                "PaymentMethods" => () => new FrmPaymentMethods(),
                "VoucherTypes" => () => new FrmVoucherTypes(),
                "VoucherStatuses" => () => new FrmVoucherStatuses(),
                "ApprovalPolicies" => () => new FrmApprovalPolicies(),
                "ChartOfAccounts" => () => new FrmChartOfAccounts(),
                "Currencies" => () => new FrmCurrencies(),
                "CostCenters" => () => new FrmCostCenters(),
                "CashBoxes" => () => new FrmCashBoxes(),
                "Banks" => () => new FrmBanks(),
                "Parties" => () => new FrmParties(),
                "ReceiptVoucher" => () => new FrmReceiptVoucher(),
                "PaymentVoucher" => () => new FrmPaymentVoucher(),
                "JournalVoucher" => () => new FrmJournalVoucher(),
                "DocumentSearch" => () => new FrmDocumentSearch(),
                _ => null
            };

        private static bool IsSupportedScreen(string screenCode) =>
            GetScreenFactory(screenCode) != null;

        /// <summary>لوحة تحكم بلا أرقام تجريبية؛ تظهر فقط الإجراءات المصرح بها.</summary>
        private void BuildDashboard()
        {
            var dashboard = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18),
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Color.FromArgb(249, 250, 252)
            };
            dashboard.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            dashboard.RowStyles.Add(new RowStyle(SizeType.Absolute, 145));
            dashboard.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            dashboard.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            dashboard.Controls.Add(new Label
            {
                Text = "مركز التحكم",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = Color.FromArgb(8, 49, 92),
                TextAlign = ContentAlignment.MiddleRight
            }, 0, 0);

            var cards = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0, 10, 0, 10)
            };
            AddDashboardCard(cards, "سندات القبض", "إنشاء وحفظ ومراجعة السندات", "ReceiptVoucher", Color.FromArgb(38, 107, 201));
            AddDashboardCard(cards, "سندات الصرف", "صرف آمن واعتماد وترحيل", "PaymentVoucher", Color.FromArgb(196, 97, 42));
            AddDashboardCard(cards, "القيود اليومية", "قيود يدوية ضمن الفترة المفتوحة", "JournalVoucher", Color.FromArgb(100, 75, 155));
            AddDashboardCard(cards, "الدليل المحاسبي", "الحسابات ومراكز التكلفة والعملات", "ChartOfAccounts", Color.FromArgb(140, 84, 184));
            AddDashboardCard(cards, "الإعدادات", "السياسات والسنوات والترقيم", "GeneralSettings", Color.FromArgb(33, 141, 103));
            dashboard.Controls.Add(cards, 0, 1);

            var quickPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(18) };
            quickPanel.Controls.Add(new Label
            {
                Text = "الإجراءات السريعة",
                Dock = DockStyle.Top,
                Height = 32,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(8, 49, 92),
                TextAlign = ContentAlignment.MiddleRight
            });

            var quickActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 80,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = true,
                Padding = new Padding(0, 12, 0, 0)
            };
            AddQuickAction(quickActions, "سند قبض جديد", "ReceiptVoucher", Color.FromArgb(22, 125, 84));
            AddQuickAction(quickActions, "سند صرف جديد", "PaymentVoucher", Color.FromArgb(196, 97, 42));
            AddQuickAction(quickActions, "قيد يومي جديد", "JournalVoucher", Color.FromArgb(100, 75, 155));
            AddQuickAction(quickActions, "بحث المستندات", "DocumentSearch", Color.FromArgb(70, 105, 125));
            AddQuickAction(quickActions, "دليل الحسابات", "ChartOfAccounts", Color.FromArgb(20, 102, 190));
            AddQuickAction(quickActions, "المستخدمون", "Users", Color.FromArgb(107, 70, 160));
            AddQuickAction(quickActions, "الإعدادات", "GeneralSettings", Color.FromArgb(78, 89, 101));
            quickPanel.Controls.Add(quickActions);
            dashboard.Controls.Add(quickPanel, 0, 2);

            dashboard.Controls.Add(new Label
            {
                Text = "تعرض المؤشرات التشغيلية الفعلية بعد اكتمال وحدة التقارير؛ لا يعرض النظام أرقاماً تجريبية.",
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(95, 105, 115),
                TextAlign = ContentAlignment.MiddleRight
            }, 0, 3);

            _workspace.SetHome(dashboard);
        }

        private void AddDashboardCard(FlowLayoutPanel target, string title, string description, string screenCode, Color accent)
        {
            if (!CanOpenScreen(screenCode))
                return;

            var card = new Panel
            {
                Width = 240,
                Height = 110,
                Margin = new Padding(7),
                BackColor = Color.White,
                Cursor = Cursors.Hand,
                Padding = new Padding(15)
            };
            card.Controls.Add(new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 34,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = accent,
                TextAlign = ContentAlignment.MiddleRight
            });
            card.Controls.Add(new Label
            {
                Text = description,
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(75, 85, 99),
                TextAlign = ContentAlignment.MiddleRight
            });
            card.Click += (_, _) => OpenScreen(screenCode);
            foreach (Control child in card.Controls)
                child.Click += (_, _) => OpenScreen(screenCode);
            target.Controls.Add(card);
        }

        private void AddQuickAction(FlowLayoutPanel target, string caption, string screenCode, Color color)
        {
            if (!CanOpenScreen(screenCode))
                return;

            var button = new Button
            {
                Text = caption,
                Width = 180,
                Height = 40,
                Margin = new Padding(5),
                FlatStyle = FlatStyle.Flat,
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            button.FlatAppearance.BorderSize = 0;
            button.Click += (_, _) => OpenScreen(screenCode);
            target.Controls.Add(button);
        }

        private void ShowNotifications() =>
            MessageBox.Show("لا توجد إشعارات تشغيلية جديدة حالياً.",
                "الإشعارات", MessageBoxButtons.OK, MessageBoxIcon.Information);

        private void ShowAbout() =>
            MessageBox.Show("نظام الطائر لإدارة موارد المؤسسات\nAlTayerERP 4.2\nمكتب الطائر السعيد للنقل",
                "حول النظام", MessageBoxButtons.OK, MessageBoxIcon.Information);

        /// <summary>تجديد Access Token قبل انتهائه مع إبقاء الواجهة على نفس سياق العمل.</summary>
        private async Task RefreshAccessTokenIfNeededAsync()
        {
            if (CurrentSession.Access_Token_Expires_At > DateTime.UtcNow.AddMinutes(3))
                return;

            var refreshed = await SessionService.RefreshAccessTokenAsync();
            if (refreshed || CurrentSession.Access_Token_Expires_At > DateTime.UtcNow)
                return;

            _sessionTimer.Stop();
            CurrentSession.Clear();
            _allowClose = true;
            MessageBox.Show("انتهت الجلسة وتعذر تجديدها. سجل الدخول من جديد.",
                "الجلسة", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            Close();
        }

        /// <summary>خروج آمن: إغلاق التبويبات أولاً، إبطال الخادم، ثم مسح الذاكرة محلياً.</summary>
        private async Task LogoutAsync(bool showConfirmation)
        {
            if (_loggingOut)
                return;

            if (showConfirmation && MessageBox.Show(
                    "هل تريد تسجيل الخروج من الجلسة الحالية؟",
                    "تسجيل الخروج",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            if (!_workspace.TryCloseAll())
                return;

            _loggingOut = true;
            btnLogout.Enabled = false;
            try
            {
                await _client.PostAsync("Auth/Logout", content: null);
            }
            catch
            {
                // يمسح العميل رموزه دائماً؛ Access Token قصير العمر ويصبح غير صالح تلقائياً.
            }
            finally
            {
                CurrentSession.Clear();
                _allowClose = true;
                Close();
            }
        }

        private async void FrmMain_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_allowClose)
                return;

            // إغلاق النافذة من زر X يتبع مسار خروج آمن نفسه.
            e.Cancel = true;
            await LogoutAsync(showConfirmation: true);
        }

        private static ScreenAccessRow? GetKnownScreen(string code) =>
            KnownScreens.FirstOrDefault(x => string.Equals(x.Screen_Code, code, StringComparison.OrdinalIgnoreCase));

        // قائمة مطابقة فقط للشاشات المنفذة حالياً؛ مصدر الإتاحة الفعلي يبقى API.
        private static readonly ScreenAccessRow[] KnownScreens =
        {
            new("TenantGroups", "المجموعات التجارية", "الإدارة العامة", 5),
            new("Companies", "الشركات", "الإدارة العامة", 10),
            new("Branches", "الفروع", "الإدارة العامة", 20),
            new("Countries", "الدول", "الإدارة العامة", 25),
            new("Governorates", "المحافظات", "الإدارة العامة", 26),
            new("Cities", "المدن", "الإدارة العامة", 27),
            new("FiscalYears", "السنوات المالية", "الإدارة العامة", 30),
            new("Users", "المستخدمون", "الإدارة العامة", 40),
            new("Roles", "الأدوار", "الإدارة العامة", 50),
            new("RolePermissions", "صلاحيات الأدوار", "الإدارة العامة", 60),
            new("AuditLogs", "سجل التدقيق والرقابة", "الإدارة العامة", 70),
            new("Sessions", "الجلسات النشطة", "الإدارة العامة", 80),
            new("GeneralSettings", "الإعدادات العامة والمالية", "التهيئة والإعدادات", 10),
            new("SystemScreens", "كتالوج شاشات النظام", "التهيئة والإعدادات", 20),
            new("NumberingSettings", "إعدادات الترقيم", "التهيئة والإعدادات", 30),
            new("FiscalPeriods", "الفترات المالية", "التهيئة والإعدادات", 40),
            new("ExchangeRates", "أسعار الصرف", "التهيئة والإعدادات", 50),
            new("PaymentMethods", "طرق السداد", "التهيئة والإعدادات", 60),
            new("VoucherTypes", "أنواع السندات", "التهيئة والإعدادات", 70),
            new("VoucherStatuses", "حالات السندات", "التهيئة والإعدادات", 80),
            new("ApprovalPolicies", "سياسات الاعتماد والسقوف", "التهيئة والإعدادات", 90),
            new("ChartOfAccounts", "الدليل المحاسبي", "الحسابات", 10),
            new("Currencies", "العملات", "الحسابات", 20),
            new("CostCenters", "مراكز التكلفة", "الحسابات", 30),
            new("CashBoxes", "الصناديق", "الحسابات", 40),
            new("Banks", "البنوك والحسابات البنكية", "الحسابات", 50),
            new("Parties", "الأطراف المالية", "الحسابات", 60),
            new("ReceiptVoucher", "سند القبض", "الحسابات", 70),
            new("PaymentVoucher", "سند الصرف", "الحسابات", 80),
            new("JournalVoucher", "القيد اليومي", "الحسابات", 90),
            new("DocumentSearch", "البحث عن المستندات", "الحسابات", 100)
        };

        private sealed class ScreenAccessRow
        {
            public ScreenAccessRow() { }
            public ScreenAccessRow(string code, string name, string module, int sortOrder)
            {
                Screen_Code = code;
                Screen_Name = name;
                Module_Name = module;
                Sort_Order = sortOrder;
                Is_Active = true;
            }

            public int Screen_ID { get; set; }
            public string Screen_Code { get; set; } = string.Empty;
            public string Screen_Name { get; set; } = string.Empty;
            public string Module_Name { get; set; } = string.Empty;
            public int Sort_Order { get; set; }
            public bool Is_Active { get; set; } = true;
        }

        // تبقى هذه المعالجات متوافقة مع ملف المصمم الحالي؛ لا تحتوي منطق أعمال.
        private void lblStatusTime_Click(object? sender, EventArgs e) { }
        private void label4_Click(object? sender, EventArgs e) { }
        private void pnlWorkspace_Paint(object? sender, PaintEventArgs e) { }
        private void tvMainMenu_AfterSelect_1(object? sender, TreeViewEventArgs e) { }

        private sealed class SessionInfoResponse
        {
            public string Company_Name_AR { get; set; } = string.Empty;
            public string Branch_Name { get; set; } = string.Empty;
            public string Year_Name { get; set; } = string.Empty;
        }
    }
}