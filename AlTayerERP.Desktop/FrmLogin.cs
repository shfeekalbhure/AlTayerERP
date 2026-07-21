using AlTayerERP.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    public partial class FrmLogin : Form
    {
        private readonly HttpClient _client = ApiService.Client;
        private readonly string _baseUrl = ApiService.BaseUrl;

        public FrmLogin()
        {
            InitializeComponent();

            Load -= FrmLogin_Load;
            Load += FrmLogin_Load;
            btnLogin.Click -= btnLogin_Click;
            btnLogin.Click += btnLogin_Click;
            btnExit.Click -= btnExit_Click;
            btnExit.Click += btnExit_Click;
            btnAboutSystem.Click -= btnAboutSystem_Click;
            btnAboutSystem.Click += btnAboutSystem_Click;
            btnConnectionSettings.Click -= btnConnectionSettings_Click;
            btnConnectionSettings.Click += btnConnectionSettings_Click;
            cmbCompany.SelectedIndexChanged -= cmbCompany_SelectedIndexChanged;
            cmbCompany.SelectedIndexChanged += cmbCompany_SelectedIndexChanged;

            txtPassword.PasswordChar = '*';
            AcceptButton = btnLogin;
            CancelButton = btnExit;
        }

        private async void FrmLogin_Load(object? sender, EventArgs e)
        {
            CurrentSession.Clear();
            ApiService.ClearAccessToken();

            try
            {
                lblApiStatus.Text = "API: جاري الفحص...";
                lblDatabaseStatus.Text = "قاعدة البيانات: جاري الفحص...";
                lblLicenseStatus.Text = "الترخيص: ساري";
                lblVersion.Text = "الإصدار: 1.0.0";
                lblDateTime.Text = DateTime.Now.ToString("yyyy/MM/dd hh:mm tt");

                await LoadCompaniesAsync();

                lblApiStatus.Text = "API: متصل";
                lblDatabaseStatus.Text = "قاعدة البيانات: متصلة";
                cmbUsername.SelectedIndex = -1;
                cmbBranch.SelectedIndex = -1;
                txtPassword.Clear();
                cmbUsername.Focus();
            }
            catch (Exception ex)
            {
                lblApiStatus.Text = "API: غير متصل";
                lblDatabaseStatus.Text = "قاعدة البيانات: غير متصلة";
                btnLogin.Enabled = false;
                MessageBox.Show(
                    "فشل تحميل بيانات الدخول:\n" + ex.Message,
                    "خطأ تهيئة",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async Task LoadCompaniesAsync()
        {
            List<CompanyLookupModel>? data = await _client.GetFromJsonAsync<List<CompanyLookupModel>>(
                $"{_baseUrl}Branches/GetCompaniesLookup");

            cmbCompany.DataSource = data ?? new List<CompanyLookupModel>();
            cmbCompany.DisplayMember = "Company_Name_AR";
            cmbCompany.ValueMember = "Company_ID";
            cmbCompany.SelectedIndex = -1;
        }

        private async Task LoadBranchesAsync()
        {
            string companyId = cmbCompany.SelectedValue?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(companyId))
                return;

            List<BranchLookupModel>? data = await _client.GetFromJsonAsync<List<BranchLookupModel>>(
                $"{_baseUrl}Branches/GetActiveBranchesLookup?companyId={Uri.EscapeDataString(companyId)}");

            cmbBranch.DataSource = data ?? new List<BranchLookupModel>();
            cmbBranch.DisplayMember = "Branch_Name";
            cmbBranch.ValueMember = "Branch_ID";
            cmbBranch.SelectedIndex = -1;
        }

        private async Task LoadFiscalYearsAsync()
        {
            string companyId = cmbCompany.SelectedValue?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(companyId))
                return;

            List<FiscalYearLookupModel>? data = await _client.GetFromJsonAsync<List<FiscalYearLookupModel>>(
                $"{_baseUrl}FiscalYears?companyId={Uri.EscapeDataString(companyId)}");

            cmbFiscalYear.DataSource = null;
            cmbFiscalYear.DisplayMember = "Year_Name";
            cmbFiscalYear.ValueMember = "Fiscal_Year_ID";
            cmbFiscalYear.DataSource = data ?? new List<FiscalYearLookupModel>();

            FiscalYearLookupModel? defaultYear = data?.FirstOrDefault(x => x.Is_Default);
            if (defaultYear != null)
                cmbFiscalYear.SelectedValue = defaultYear.Fiscal_Year_ID;
            else
                cmbFiscalYear.SelectedIndex = -1;
        }

        private async Task LoadUsersAsync()
        {
            string companyId = cmbCompany.SelectedValue?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(companyId))
                return;

            List<UserLookupModel>? data = await _client.GetFromJsonAsync<List<UserLookupModel>>(
                $"{_baseUrl}Users/GetUsersLookup?companyId={Uri.EscapeDataString(companyId)}");

            cmbUsername.DataSource = data ?? new List<UserLookupModel>();
            cmbUsername.DisplayMember = "Login_Name";
            cmbUsername.ValueMember = "User_ID";
            cmbUsername.SelectedIndex = -1;
        }

        private Task LoadCompanyLogoAsync() => Task.CompletedTask;

        private async void btnLogin_Click(object? sender, EventArgs e)
        {
            btnLogin.Enabled = false;

            try
            {
                if (cmbCompany.SelectedValue == null ||
                    cmbBranch.SelectedValue == null ||
                    cmbFiscalYear.SelectedValue == null ||
                    cmbUsername.SelectedValue == null ||
                    string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show(
                        "اختر الشركة والفرع والسنة المالية والمستخدم، ثم أدخل كلمة المرور.",
                        "بيانات الدخول",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                var request = new LoginRequest
                {
                    Company_ID = cmbCompany.SelectedValue.ToString() ?? string.Empty,
                    Branch_ID = Convert.ToInt32(cmbBranch.SelectedValue),
                    Year_ID = Convert.ToInt32(cmbFiscalYear.SelectedValue),
                    User_ID = Convert.ToInt32(cmbUsername.SelectedValue),
                    Password = txtPassword.Text
                };

                using HttpResponseMessage response = await _client.PostAsJsonAsync(
                    $"{_baseUrl}Auth/Login",
                    request);

                if (!response.IsSuccessStatusCode)
                {
                    string message = await ReadLoginErrorAsync(response);
                    MessageBox.Show(message, "فشل الدخول", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.SelectAll();
                    txtPassword.Focus();
                    return;
                }

                LoginResultModel? result = await response.Content.ReadFromJsonAsync<LoginResultModel>();
                if (result == null || result.User_ID <= 0)
                {
                    MessageBox.Show(
                        "لم تصل جلسة دخول صالحة من الخادم.",
                        "فشل الدخول",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                CurrentSession.Clear();
                CurrentSession.Company_ID = result.Company_ID;
                CurrentSession.Company_Name = cmbCompany.Text;
                CurrentSession.Branch_ID = result.Branch_ID;
                CurrentSession.Branch_Name = cmbBranch.Text;
                CurrentSession.Year_ID = result.Year_ID;
                CurrentSession.Year_Name = cmbFiscalYear.Text;
                ApiService.SetAccessToken(result.Access_Token);
                CurrentSession.User_ID = result.User_ID;
                CurrentSession.Role_ID = result.Role_ID;
                CurrentSession.Username = result.Login_Name;
                CurrentSession.Full_Name = result.Full_Name;
                CurrentSession.Is_System_Admin = result.Is_System_Admin;
                CurrentSession.Login_Time = DateTime.Now;
                CurrentSession.SetScreenPermissions(
                    result.Screen_Permissions.Select(x => new CurrentSession.ScreenPermissionState
                    {
                        Screen_Code = x.Screen_Code,
                        Can_View = x.Can_View,
                        Can_Add = x.Can_Add,
                        Can_Edit = x.Can_Edit,
                        Can_Delete = x.Can_Delete,
                        Can_Print = x.Can_Print,
                        Can_Export = x.Can_Export,
                        Can_Import = x.Can_Import,
                        Can_Approve = x.Can_Approve,
                        Can_UnApprove = x.Can_UnApprove
                    }));

                Hide();
                new FrmMain().Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "خطأ أثناء تسجيل الدخول:\n" + ex.Message,
                    "خطأ غير متوقع",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                if (!IsDisposed && Visible)
                    btnLogin.Enabled = true;
            }
        }

        private static async Task<string> ReadLoginErrorAsync(HttpResponseMessage response)
        {
            string body = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body))
                return "تعذر تسجيل الدخول. تحقق من بياناتك ثم أعد المحاولة.";

            try
            {
                LoginErrorModel? error = System.Text.Json.JsonSerializer.Deserialize<LoginErrorModel>(
                    body,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return error?.message ?? "تعذر تسجيل الدخول. تحقق من بياناتك ثم أعد المحاولة.";
            }
            catch
            {
                return body;
            }
        }

        private void btnExit_Click(object? sender, EventArgs e) => Application.Exit();

        private void btnConnectionSettings_Click(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "شاشة إعداد الاتصال ستنفذ ضمن حزمة الإقلاع والبيئة.",
                "إعدادات الاتصال",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void btnAboutSystem_Click(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "نظام الطائر لإدارة النقل والشحن\nالإصدار 1.0.0",
                "حول النظام",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private async void cmbCompany_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbCompany.SelectedValue == null)
                return;

            try
            {
                UseWaitCursor = true;
                await LoadBranchesAsync();
                await LoadUsersAsync();
                await LoadFiscalYearsAsync();
                await LoadCompanyLogoAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "تعذر تحميل بيانات الشركة المختارة:\n" + ex.Message,
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private void picSystemLogo_Click(object? sender, EventArgs e) { }
        private void grpLogin_Enter(object? sender, EventArgs e) { }
        private void cmbBranch_SelectedIndexChanged(object? sender, EventArgs e) { }
    }

    public sealed class LoginRequest
    {
        public string Company_ID { get; set; } = string.Empty;
        public int Branch_ID { get; set; }
        public int Year_ID { get; set; }
        public int User_ID { get; set; }
        public string Password { get; set; } = string.Empty;
    }

    public sealed class LoginResultModel
    {
        public int User_ID { get; set; }
        public string Full_Name { get; set; } = string.Empty;
        public string Login_Name { get; set; } = string.Empty;
        public int Role_ID { get; set; }
        public int Branch_ID { get; set; }
        public string Company_ID { get; set; } = string.Empty;
        public int Year_ID { get; set; }
        public bool Is_System_Admin { get; set; }
        public string Access_Token { get; set; } = string.Empty;
        public List<ScreenPermissionModel> Screen_Permissions { get; set; } = new();
    }

    public sealed class ScreenPermissionModel
    {
        public string Screen_Code { get; set; } = string.Empty;
        public bool Can_View { get; set; }
        public bool Can_Add { get; set; }
        public bool Can_Edit { get; set; }
        public bool Can_Delete { get; set; }
        public bool Can_Print { get; set; }
        public bool Can_Export { get; set; }
        public bool Can_Import { get; set; }
        public bool Can_Approve { get; set; }
        public bool Can_UnApprove { get; set; }
    }

    public sealed class LoginErrorModel
    {
        public string? message { get; set; }
    }

    public sealed class FiscalYearLookupModel
    {
        public int Fiscal_Year_ID { get; set; }
        public string Year_Name { get; set; } = string.Empty;
        public bool Is_Default { get; set; }
    }

    public sealed class UserLookupModel
    {
        public int User_ID { get; set; }
        public string Login_Name { get; set; } = string.Empty;
    }
}