using AlTayerERP.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormsTimer = System.Windows.Forms.Timer;

namespace AlTayerERP.Desktop
{
    public partial class FrmLogin : Form
    {
        private readonly HttpClient _client = ApiService.Client;
        private readonly string _baseUrl = ApiService.BaseUrl;
        private bool _loadingCompany;
        private Button? _btnTogglePassword;
        private Label? _lblCapsLock;
        private Label? _lblWelcome;
        private Button? _btnInitialSetup;
        private WinFormsTimer? _clockTimer;

        public FrmLogin()
        {
            InitializeComponent();

            Load -= FrmLogin_Load;
            Load += FrmLogin_Load;
            btnLogin.Click -= btnLogin_Click;
            btnLogin.Click += btnLogin_Click;
            cmbCompany.SelectedIndexChanged -= cmbCompany_SelectedIndexChanged;
            cmbCompany.SelectedIndexChanged += cmbCompany_SelectedIndexChanged;

            btnExit.Click -= btnExit_Click;
            btnExit.Click += btnExit_Click;
            btnAboutSystem.Click -= btnAboutSystem_Click;
            btnAboutSystem.Click += btnAboutSystem_Click;
            btnConnectionSettings.Click -= btnConnectionSettings_Click;
            btnConnectionSettings.Click += btnConnectionSettings_Click;

            cmbUsername.DropDownStyle = ComboBoxStyle.DropDown;
            cmbUsername.AutoCompleteMode = AutoCompleteMode.None;
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.KeyDown += TxtPassword_KeyDown;
            txtPassword.KeyUp += TxtPassword_KeyUp;

            AcceptButton = btnLogin;
            CancelButton = btnExit;
            KeyPreview = true;
            KeyDown += FrmLogin_KeyDown;

            ApplyLoginVisuals();
            CreateRuntimeControls();
            ApplyDefaultBranding();
        }

        private void ApplyLoginVisuals()
        {
            var navy = Color.FromArgb(8, 49, 92);
            var blue = Color.FromArgb(30, 104, 194);

            Text = "تسجيل الدخول - نظام الطائر ERP";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(245, 248, 252);
            MinimumSize = new Size(920, 590);

            pnlHeader.BackColor = Color.White;
            pnlHeader.BorderStyle = BorderStyle.FixedSingle;
            lblSystemTitle.Text = "نظام الطائر لإدارة موارد المؤسسات";
            lblSystemTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblSystemTitle.ForeColor = navy;
            lblSystemSubtitle.Text = "AlTayer ERP | Enterprise Resource Planning";
            lblSystemSubtitle.Font = new Font("Segoe UI", 9.5F);
            lblSystemSubtitle.ForeColor = blue;

            pnlCompanyInfo.BackColor = Color.White;
            pnlCompanyInfo.BorderStyle = BorderStyle.FixedSingle;
            lblCompanyTitle.Text = "بيانات الشركة";
            lblCompanyTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblCompanyTitle.ForeColor = navy;
            lblCompanyName.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            foreach (var label in new[] { lblCompanyAddress, lblCompanyPhone, lblCompanyEmail })
            {
                label.Font = new Font("Segoe UI", 9.5F);
                label.ForeColor = Color.FromArgb(55, 65, 81);
            }

            pnlLogin.BackColor = Color.White;
            pnlLogin.BorderStyle = BorderStyle.FixedSingle;
            grpLogin.BackColor = Color.White;
            grpLogin.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            grpLogin.ForeColor = navy;

            foreach (var input in new Control[] { cmbCompany, cmbBranch, cmbFiscalYear, cmbUsername, txtPassword })
            {
                input.Font = new Font("Segoe UI", 10F);
                input.BackColor = Color.White;
            }

            foreach (var combo in new[] { cmbCompany, cmbBranch, cmbFiscalYear })
                combo.DropDownStyle = ComboBoxStyle.DropDownList;

            chkRememberMe.Text = "تذكر بيانات الدخول";
            chkRememberMe.Font = new Font("Segoe UI", 9F);

            btnLogin.Text = "تسجيل الدخول  (Enter)";
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.BackColor = blue;
            btnLogin.ForeColor = Color.White;
            btnLogin.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);

            btnExit.Text = "خروج  (Esc)";
            btnConnectionSettings.Text = "إعدادات الاتصال";
            btnAboutSystem.Text = "حول النظام";
            foreach (var button in new[] { btnAboutSystem, btnConnectionSettings, btnExit })
            {
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderColor = Color.FromArgb(190, 205, 220);
                button.BackColor = Color.White;
                button.ForeColor = navy;
                button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            }

            pnlStatusBar.BackColor = Color.White;
            pnlStatusBar.BorderStyle = BorderStyle.FixedSingle;
        }

        private void CreateRuntimeControls()
        {
            _btnTogglePassword = new Button
            {
                Name = "btnTogglePassword",
                Text = "إظهار",
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(8, 49, 92),
                TabStop = false,
                Size = new Size(58, txtPassword.Height + 2),
                Location = new Point(Math.Max(4, txtPassword.Left - 62), txtPassword.Top - 1),
                Anchor = txtPassword.Anchor
            };
            _btnTogglePassword.FlatAppearance.BorderColor = Color.FromArgb(170, 185, 205);
            _btnTogglePassword.Click += (_, _) =>
            {
                txtPassword.UseSystemPasswordChar = !txtPassword.UseSystemPasswordChar;
                _btnTogglePassword.Text = txtPassword.UseSystemPasswordChar ? "إظهار" : "إخفاء";
                txtPassword.Focus();
                txtPassword.SelectionStart = txtPassword.TextLength;
            };
            grpLogin.Controls.Add(_btnTogglePassword);
            _btnTogglePassword.BringToFront();

            _lblCapsLock = new Label
            {
                Name = "lblCapsLock",
                AutoSize = true,
                Text = "تنبيه: مفتاح Caps Lock مفعّل",
                ForeColor = Color.Firebrick,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Visible = false,
                Location = new Point(txtPassword.Left, txtPassword.Bottom + 4)
            };
            grpLogin.Controls.Add(_lblCapsLock);
            _lblCapsLock.BringToFront();

            _lblWelcome = new Label
            {
                Name = "lblWelcome",
                AutoSize = true,
                Text = "مرحباً بك في نظام الطائر ERP",
                ForeColor = Color.FromArgb(8, 49, 92),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(18, 28)
            };
            grpLogin.Controls.Add(_lblWelcome);
            _lblWelcome.BringToFront();

            _btnInitialSetup = new Button
            {
                Text = "تهيئة أول تشغيل",
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = Color.White, ForeColor = Color.FromArgb(8, 49, 92),
                Size = new Size(180, 32), Location = new Point(196, 340), Visible = false
            };
            _btnInitialSetup.FlatAppearance.BorderColor = Color.FromArgb(190, 205, 220);
            _btnInitialSetup.Click += async (_, _) =>
            {
                using var setup = new FrmInitialSetup();
                if (setup.ShowDialog(this) == DialogResult.OK) await LoadCompaniesAsync();
            };
            grpLogin.Controls.Add(_btnInitialSetup);
            _btnInitialSetup.BringToFront();

            _clockTimer = new WinFormsTimer { Interval = 1000 };
            _clockTimer.Tick += (_, _) => lblDateTime.Text = DateTime.Now.ToString("yyyy/MM/dd  HH:mm:ss");
            _clockTimer.Start();
        }

        private void ApplyDefaultBranding()
        {
            picSystemLogo.Image?.Dispose();
            picCompanyLogo.Image?.Dispose();
            picSystemLogo.Image = CreateLogoBitmap(128, "ERP");
            picCompanyLogo.Image = CreateLogoBitmap(300, "الطائر");
            picSystemLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picCompanyLogo.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private static Bitmap CreateLogoBitmap(int size, string caption)
        {
            var bitmap = new Bitmap(size, size);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Color.White);

            var rect = new RectangleF(size * 0.07F, size * 0.07F, size * 0.86F, size * 0.86F);
            using var background = new LinearGradientBrush(rect, Color.FromArgb(8, 49, 92), Color.FromArgb(30, 104, 194), 45F);
            graphics.FillEllipse(background, rect);

            using var wingPen = new Pen(Color.White, Math.Max(2F, size * 0.035F))
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };
            graphics.DrawArc(wingPen, size * 0.20F, size * 0.25F, size * 0.42F, size * 0.38F, 205F, 115F);
            graphics.DrawArc(wingPen, size * 0.40F, size * 0.25F, size * 0.42F, size * 0.38F, 220F, 115F);

            using var font = new Font("Segoe UI", Math.Max(9F, size * 0.105F), FontStyle.Bold);
            using var textBrush = new SolidBrush(Color.White);
            using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            graphics.DrawString(caption, font, textBrush, new RectangleF(0, size * 0.57F, size, size * 0.25F), format);
            return bitmap;
        }

        private async void FrmLogin_Load(object? sender, EventArgs e)
        {
            btnLogin.Enabled = false;
            lblApiStatus.Text = "API: جاري الفحص...";
            lblDatabaseStatus.Text = "قاعدة البيانات: جاري الفحص...";
            lblLicenseStatus.Text = "الترخيص: ساري";
            lblVersion.Text = "الإصدار: 1.0.0";
            lblDateTime.Text = DateTime.Now.ToString("yyyy/MM/dd  HH:mm:ss");

            try
            {
                var health = await _client.GetAsync($"{_baseUrl}health");
                if (!health.IsSuccessStatusCode)
                    throw new HttpRequestException("خدمة النظام أو قاعدة البيانات غير جاهزة.");

                await LoadCompaniesAsync();
                lblApiStatus.Text = "API: متصل";
                lblApiStatus.ForeColor = Color.DarkGreen;
                lblDatabaseStatus.Text = "قاعدة البيانات: متصلة";
                lblDatabaseStatus.ForeColor = Color.DarkGreen;
                btnLogin.Enabled = true;
            }
            catch (Exception ex)
            {
                lblApiStatus.Text = "API: غير متصل";
                lblApiStatus.ForeColor = Color.Firebrick;
                lblDatabaseStatus.Text = "قاعدة البيانات: غير متصلة";
                lblDatabaseStatus.ForeColor = Color.Firebrick;
                MessageBox.Show("تعذر الاتصال بخدمة النظام.\n" + ex.Message, "خطأ تهيئة", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadCompaniesAsync()
        {
            _loadingCompany = true;
            try
            {
                var companies = await _client.GetFromJsonAsync<List<CompanyLookupModel>>($"{_baseUrl}Branches/GetCompaniesLookup") ?? new();
                Bind(cmbCompany, companies, "Company_Name_AR", "Company_ID");
                if (_btnInitialSetup != null)
                    _btnInitialSetup.Visible = companies.Count == 0;
                if (companies.Count == 1)
                    cmbCompany.SelectedIndex = 0;
            }
            finally
            {
                _loadingCompany = false;
            }

            if (cmbCompany.SelectedValue != null)
                await LoadCompanyContextAsync();
        }

        private async Task LoadCompanyContextAsync()
        {
            string? companyId = cmbCompany.SelectedValue?.ToString();
            if (string.IsNullOrWhiteSpace(companyId))
                return;

            lblCompanyName.Text = cmbCompany.Text;
            lblCompanyAddress.Text = "العنوان: عدن - المنصورة";
            lblCompanyPhone.Text = "الهاتف: غير محدد";
            lblCompanyEmail.Text = "البريد الإلكتروني: غير محدد";

            var branchesTask = _client.GetFromJsonAsync<List<BranchLookupModel>>($"{_baseUrl}Branches/GetActiveBranchesLookup?companyId={Uri.EscapeDataString(companyId)}");
            var yearsTask = _client.GetFromJsonAsync<List<FiscalYearLookupModel>>($"{_baseUrl}FiscalYears/Lookup?companyId={Uri.EscapeDataString(companyId)}");
            await Task.WhenAll(branchesTask, yearsTask);

            var branches = branchesTask.Result ?? new List<BranchLookupModel>();
            var years = yearsTask.Result ?? new List<FiscalYearLookupModel>();
            Bind(cmbBranch, branches, "Branch_Name", "Branch_ID");
            Bind(cmbFiscalYear, years, "Year_Name", "Fiscal_Year_ID");

            if (branches.Count == 1)
                cmbBranch.SelectedIndex = 0;

            var defaultYear = years.FirstOrDefault(x => x.Is_Default);
            if (defaultYear != null)
                cmbFiscalYear.SelectedValue = defaultYear.Fiscal_Year_ID;
            else if (years.Count == 1)
                cmbFiscalYear.SelectedIndex = 0;

            cmbUsername.DataSource = null;
            cmbUsername.Text = string.Empty;
            txtPassword.Clear();
        }

        private static void Bind<T>(ComboBox combo, List<T> items, string displayMember, string valueMember)
        {
            combo.DataSource = null;
            combo.DisplayMember = displayMember;
            combo.ValueMember = valueMember;
            combo.DataSource = items;
            combo.SelectedIndex = -1;
        }

        private async void cmbCompany_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_loadingCompany || cmbCompany.SelectedValue == null)
                return;

            try
            {
                await LoadCompanyContextAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر تحميل بيانات الشركة.\n" + ex.Message, "الدخول", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void btnLogin_Click(object? sender, EventArgs e)
        {
            if (cmbCompany.SelectedValue == null || cmbBranch.SelectedValue == null ||
                cmbFiscalYear.SelectedValue == null || string.IsNullOrWhiteSpace(cmbUsername.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("حدد الشركة والفرع والسنة المالية والمستخدم، ثم أدخل كلمة المرور.", "بيانات ناقصة", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnLogin.Enabled = false;
            string originalText = btnLogin.Text;
            btnLogin.Text = "جاري التحقق...";
            UseWaitCursor = true;

            try
            {
                var request = new LoginRequest
                {
                    Company_ID = cmbCompany.SelectedValue.ToString() ?? string.Empty,
                    Branch_ID = Convert.ToInt32(cmbBranch.SelectedValue),
                    Year_ID = Convert.ToInt32(cmbFiscalYear.SelectedValue),
                    User_ID = 0,
                    Login_Name = cmbUsername.Text.Trim(),
                    Password = txtPassword.Text
                };

                var response = await _client.PostAsJsonAsync($"{_baseUrl}Auth/Login", request);
                if (!response.IsSuccessStatusCode)
                {
                    string message = await response.Content.ReadAsStringAsync();
                    MessageBox.Show(string.IsNullOrWhiteSpace(message) ? "تعذر التحقق من بيانات الدخول." : message,
                        "فشل الدخول", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.SelectAll();
                    txtPassword.Focus();
                    return;
                }

                var result = await response.Content.ReadFromJsonAsync<LoginResultModel>();
                if (result == null)
                    throw new InvalidOperationException("لم تُرجع الخدمة جلسة صالحة.");

                CurrentSession.Company_ID = result.Company_ID;
                CurrentSession.Company_Name = cmbCompany.Text;
                CurrentSession.Branch_ID = result.Branch_ID;
                CurrentSession.Branch_Name = cmbBranch.Text;
                CurrentSession.Year_ID = result.Year_ID;
                CurrentSession.Year_Name = cmbFiscalYear.Text;
                CurrentSession.User_ID = result.User_ID;
                CurrentSession.Role_ID = result.Role_ID;
                CurrentSession.Username = result.Login_Name;
                CurrentSession.Full_Name = result.Full_Name;
                CurrentSession.Is_System_Admin = result.Is_System_Admin;
                CurrentSession.Access_Token = result.Access_Token;
                CurrentSession.Login_Time = DateTime.Now;
                ApiService.ApplySessionToken(result.Access_Token);

                Hide();
                using var main = new FrmMain();
                main.ShowDialog(this);
                Show();
                txtPassword.Clear();
                cmbUsername.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر إتمام تسجيل الدخول.\n" + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
                btnLogin.Text = originalText;
                btnLogin.Enabled = true;
            }
        }

        private void TxtPassword_KeyDown(object? sender, KeyEventArgs e) => UpdateCapsLockWarning();
        private void TxtPassword_KeyUp(object? sender, KeyEventArgs e) => UpdateCapsLockWarning();

        private void UpdateCapsLockWarning()
        {
            if (_lblCapsLock != null)
                _lblCapsLock.Visible = Control.IsKeyLocked(Keys.CapsLock);
        }

        private void FrmLogin_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                btnAboutSystem.PerformClick();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F2)
            {
                btnConnectionSettings.PerformClick();
                e.Handled = true;
            }
        }

        private void btnExit_Click(object? sender, EventArgs e) => Application.Exit();

        private void btnAboutSystem_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("نظام الطائر لإدارة موارد المؤسسات\nAlTayer ERP\nالإصدار 1.0.0",
                "حول النظام", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnConnectionSettings_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("عنوان خدمة API الحالي:\n" + _baseUrl,
                "إعدادات الاتصال", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _clockTimer?.Stop();
            _clockTimer?.Dispose();
            picSystemLogo.Image?.Dispose();
            picCompanyLogo.Image?.Dispose();
            base.OnFormClosed(e);
        }
    }

    public class LoginRequest
    {
        public string Company_ID { get; set; } = string.Empty;
        public int Branch_ID { get; set; }
        public int Year_ID { get; set; }
        public int User_ID { get; set; }
        public string Login_Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResultModel
    {
        public int User_ID { get; set; }
        public string Full_Name { get; set; } = string.Empty;
        public string Login_Name { get; set; } = string.Empty;
        public int Role_ID { get; set; }
        public int Branch_ID { get; set; }
        public string Company_ID { get; set; } = string.Empty;
        public int Year_ID { get; set; }
        public bool Is_System_Admin { get; set; }
        public bool Must_Change_Password { get; set; }
        public string Access_Token { get; set; } = string.Empty;
    }

    public class FiscalYearLookupModel
    {
        public int Fiscal_Year_ID { get; set; }
        public string Year_Name { get; set; } = string.Empty;
        public bool Is_Default { get; set; }
    }

    public class UserLookupModel
    {
        public int User_ID { get; set; }
        public string Login_Name { get; set; } = string.Empty;
        public int Branch_ID { get; set; }
    }
}
