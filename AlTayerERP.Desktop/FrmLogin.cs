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
    /// <summary>
    /// شاشة تسجيل الدخول المطورة للمرحلة الأولى.
    /// تركز على تحميل بيانات الجلسة بشكل متسلسل وآمن ثم تسليم المستخدم
    /// إلى مساحة العمل الرئيسية بعد تعبئة CurrentSession بالكامل.
    /// </summary>
    public partial class FrmLogin : Form
    {
        private readonly HttpClient _client = ApiService.Client;
        private readonly string _baseUrl = ApiService.BaseUrl;
        private readonly Timer _clockTimer = new Timer();
        private bool _isLoadingReferenceData;

        public FrmLogin()
        {
            InitializeComponent();
            WireEvents();
            InitializeUiState();
        }

        private void WireEvents()
        {
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

            FormClosed -= FrmLogin_FormClosed;
            FormClosed += FrmLogin_FormClosed;
        }

        private void InitializeUiState()
        {
            txtPassword.PasswordChar = '*';
            AcceptButton = btnLogin;
            CancelButton = btnExit;

            btnLogin.Enabled = false;
            lblVersion.Text = "الإصدار: 1.0.0";
            lblLicenseStatus.Text = "الترخيص: ساري";
            UpdateStatusTexts(apiText: "API: جاري الفحص...", dbText: "قاعدة البيانات: جاري الفحص...");

            _clockTimer.Interval = 1000;
            _clockTimer.Tick += (_, _) =>
            {
                lblDateTime.Text = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss tt");
            };
            _clockTimer.Start();
        }

        private async void FrmLogin_Load(object? sender, EventArgs e)
        {
            await InitializeLoginAsync();
        }

        private async Task InitializeLoginAsync()
        {
            try
            {
                _isLoadingReferenceData = true;
                btnLogin.Enabled = false;
                ClearSelectionLists();

                await LoadCompaniesAsync();

                UpdateStatusTexts(apiText: "API: متصل", dbText: "قاعدة البيانات: متصلة");
                txtPassword.Clear();
                cmbUsername.Focus();
            }
            catch (Exception ex)
            {
                UpdateStatusTexts(apiText: "API: غير متصل", dbText: "قاعدة البيانات: غير متصلة");
                btnLogin.Enabled = false;

                MessageBox.Show(
                    "فشل تحميل بيانات شاشة الدخول:\n" + ex.Message,
                    "خطأ تهيئة",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _isLoadingReferenceData = false;
                UpdateLoginAvailability();
            }
        }

        private void ClearSelectionLists()
        {
            cmbBranch.DataSource = null;
            cmbFiscalYear.DataSource = null;
            cmbUsername.DataSource = null;
        }

        private void UpdateStatusTexts(string apiText, string dbText)
        {
            lblApiStatus.Text = apiText;
            lblDatabaseStatus.Text = dbText;
            lblDateTime.Text = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss tt");
        }

        private void UpdateLoginAvailability()
        {
            btnLogin.Enabled =
                !_isLoadingReferenceData &&
                cmbCompany.SelectedValue != null &&
                cmbFiscalYear.SelectedValue != null &&
                cmbUsername.SelectedValue != null;
        }

        private async Task LoadCompaniesAsync()
        {
            List<CompanyLookupModel>? data = await _client.GetFromJsonAsync<List<CompanyLookupModel>>(
                $"{_baseUrl}Branches/GetCompaniesLookup");

            List<CompanyLookupModel> companies = data ?? new List<CompanyLookupModel>();

            cmbCompany.DataSource = companies;
            cmbCompany.DisplayMember = nameof(CompanyLookupModel.Company_Name_AR);
            cmbCompany.ValueMember = nameof(CompanyLookupModel.Company_ID);

            if (!string.IsNullOrWhiteSpace(CurrentSession.Company_ID))
            {
                cmbCompany.SelectedValue = CurrentSession.Company_ID;
            }

            if (cmbCompany.SelectedIndex < 0 && companies.Count > 0)
            {
                cmbCompany.SelectedIndex = 0;
            }
        }

        private async Task LoadBranchesAsync(string companyId)
        {
            List<BranchLookupModel>? data = await _client.GetFromJsonAsync<List<BranchLookupModel>>(
                $"{_baseUrl}Branches/GetActiveBranchesLookup?companyId={companyId}");

            List<BranchLookupModel> branches = data ?? new List<BranchLookupModel>();

            cmbBranch.DataSource = branches;
            cmbBranch.DisplayMember = nameof(BranchLookupModel.Branch_Name);
            cmbBranch.ValueMember = nameof(BranchLookupModel.Branch_ID);

            if (CurrentSession.Branch_ID > 0)
            {
                cmbBranch.SelectedValue = CurrentSession.Branch_ID;
            }
            else
            {
                cmbBranch.SelectedIndex = branches.Count == 1 ? 0 : -1;
            }
        }

        private async Task LoadFiscalYearsAsync(string companyId)
        {
            List<FiscalYearLookupModel>? data = await _client.GetFromJsonAsync<List<FiscalYearLookupModel>>(
                $"{_baseUrl}FiscalYears?companyId={companyId}");

            List<FiscalYearLookupModel> fiscalYears = data ?? new List<FiscalYearLookupModel>();

            cmbFiscalYear.DataSource = fiscalYears;
            cmbFiscalYear.DisplayMember = nameof(FiscalYearLookupModel.Year_Name);
            cmbFiscalYear.ValueMember = nameof(FiscalYearLookupModel.Fiscal_Year_ID);

            FiscalYearLookupModel? defaultYear = fiscalYears.FirstOrDefault(x => x.Is_Default)
                ?? fiscalYears.FirstOrDefault();

            if (CurrentSession.Year_ID > 0)
            {
                cmbFiscalYear.SelectedValue = CurrentSession.Year_ID;
            }
            else if (defaultYear != null)
            {
                cmbFiscalYear.SelectedValue = defaultYear.Fiscal_Year_ID;
            }
        }

        private async Task LoadUsersAsync(string companyId)
        {
            List<UserLookupModel>? data = await _client.GetFromJsonAsync<List<UserLookupModel>>(
                $"{_baseUrl}Users/GetUsersLookup?companyId={companyId}");

            List<UserLookupModel> users = data ?? new List<UserLookupModel>();

            cmbUsername.DataSource = users;
            cmbUsername.DisplayMember = nameof(UserLookupModel.Login_Name);
            cmbUsername.ValueMember = nameof(UserLookupModel.User_ID);
            cmbUsername.SelectedIndex = -1;
        }

        private async Task LoadCompanyLogoAsync()
        {
            await Task.CompletedTask;
        }

        private async void cmbCompany_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isLoadingReferenceData)
            {
                return;
            }

            if (cmbCompany.SelectedValue is null)
            {
                return;
            }

            string companyId = cmbCompany.SelectedValue.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(companyId))
            {
                return;
            }

            try
            {
                _isLoadingReferenceData = true;
                btnLogin.Enabled = false;

                await LoadBranchesAsync(companyId);
                await LoadFiscalYearsAsync(companyId);
                await LoadUsersAsync(companyId);
                await LoadCompanyLogoAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "فشل تحميل بيانات الشركة المختارة:\n" + ex.Message,
                    "خطأ تحميل",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            finally
            {
                _isLoadingReferenceData = false;
                UpdateLoginAvailability();
            }
        }

        private async void btnLogin_Click(object? sender, EventArgs e)
        {
            btnLogin.Enabled = false;

            try
            {
                if (!ValidateLoginInputs())
                {
                    return;
                }

                string companyId = cmbCompany.SelectedValue?.ToString() ?? string.Empty;
                int branchId = cmbBranch.SelectedValue is null ? 0 : Convert.ToInt32(cmbBranch.SelectedValue);
                int yearId = cmbFiscalYear.SelectedValue is null ? 0 : Convert.ToInt32(cmbFiscalYear.SelectedValue);
                int userId = cmbUsername.SelectedValue is null ? 0 : Convert.ToInt32(cmbUsername.SelectedValue);

                var request = new LoginRequest
                {
                    Company_ID = companyId,
                    Branch_ID = branchId,
                    Year_ID = yearId,
                    User_ID = userId,
                    Password = txtPassword.Text.Trim()
                };

                using HttpResponseMessage response = await _client.PostAsJsonAsync(
                    $"{_baseUrl}Auth/Login",
                    request);

                if (!response.IsSuccessStatusCode)
                {
                    string errorMessage = "اسم المستخدم أو كلمة المرور غير صحيحة، أو أن الخدمة غير متاحة.";
                    if (response.Content.Headers.ContentType?.MediaType == "text/plain")
                    {
                        errorMessage = await response.Content.ReadAsStringAsync();
                    }

                    MessageBox.Show(
                        errorMessage,
                        "فشل الدخول",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    txtPassword.SelectAll();
                    txtPassword.Focus();
                    return;
                }

                LoginResultModel? result = await response.Content.ReadFromJsonAsync<LoginResultModel>();
                ApplySession(result, request);

                Hide();
                FrmMain main = new FrmMain();
                main.FormClosed += (_, _) => Close();
                main.Show();
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
                if (!IsDisposed)
                {
                    UpdateLoginAvailability();
                }
            }
        }

        private bool ValidateLoginInputs()
        {
            if (cmbCompany.SelectedValue == null)
            {
                MessageBox.Show("يرجى اختيار الشركة.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbCompany.Focus();
                return false;
            }

            if (cmbFiscalYear.SelectedValue == null)
            {
                MessageBox.Show("يرجى اختيار السنة المالية.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbFiscalYear.Focus();
                return false;
            }

            if (cmbUsername.SelectedValue == null)
            {
                MessageBox.Show("يرجى اختيار المستخدم.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbUsername.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("يرجى إدخال كلمة المرور.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return false;
            }

            return true;
        }

        private void ApplySession(LoginResultModel? result, LoginRequest request)
        {
            CurrentSession.Company_ID = result?.Company_ID ?? request.Company_ID;
            CurrentSession.Branch_ID = result?.Branch_ID ?? request.Branch_ID;
            CurrentSession.Year_ID = result?.Year_ID ?? request.Year_ID;
            CurrentSession.User_ID = result?.User_ID ?? request.User_ID;
            CurrentSession.Role_ID = result?.Role_ID ?? 0;

            CurrentSession.Company_Name = cmbCompany.Text;
            CurrentSession.Branch_Name = cmbBranch.Text;
            CurrentSession.Year_Name = cmbFiscalYear.Text;
            CurrentSession.Username = result?.Login_Name ?? cmbUsername.Text;
            CurrentSession.Full_Name = result?.Full_Name ?? cmbUsername.Text;
            CurrentSession.Is_System_Admin = result?.Is_System_Admin ?? false;
            CurrentSession.Login_Time = DateTime.Now;
        }

        private void btnExit_Click(object? sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnConnectionSettings_Click(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "إعدادات الاتصال ستربط في الدفعة التالية ضمن مركز الإعدادات.",
                "الإعدادات",
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

        private void FrmLogin_FormClosed(object? sender, FormClosedEventArgs e)
        {
            _clockTimer.Stop();
            _clockTimer.Dispose();
        }

        private void picSystemLogo_Click(object sender, EventArgs e) { }
        private void grpLogin_Enter(object sender, EventArgs e) { }
        private void cmbBranch_SelectedIndexChanged(object sender, EventArgs e) { }
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

    public sealed class CompanyLookupModel
    {
        public string Company_ID { get; set; } = string.Empty;
        public string Company_Name_AR { get; set; } = string.Empty;
    }

    public sealed class BranchLookupModel
    {
        public int Branch_ID { get; set; }
        public string Branch_Name { get; set; } = string.Empty;
    }
}
