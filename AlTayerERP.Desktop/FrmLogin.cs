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
        private bool _loadingCompany;

        public FrmLogin()
        {
            InitializeComponent();
            Load += FrmLogin_Load;
            btnLogin.Click += btnLogin_Click;
            btnExit.Click += (_, _) => Application.Exit();
            btnAboutSystem.Click += (_, _) => MessageBox.Show("نظام الطائر لإدارة النقل والشحن\nالإصدار 1.0.0", "حول النظام");
            btnConnectionSettings.Click += (_, _) => MessageBox.Show("عنوان الـ API الحالي:\n" + _baseUrl, "إعدادات الاتصال");
            // حدث تغيير الشركة مربوط من ملف التصميم مرة واحدة لتفادي تكرار تحميل القوائم.
            cmbUsername.DropDownStyle = ComboBoxStyle.DropDown;
            cmbUsername.AutoCompleteMode = AutoCompleteMode.None;
            txtPassword.PasswordChar = '*';
            AcceptButton = btnLogin;
            CancelButton = btnExit;
        }

        // تهيئة الشاشة والتحقق من إمكانية قراءة بيانات الشركات قبل تفعيل الدخول.
        private async void FrmLogin_Load(object? sender, EventArgs e)
        {
            lblApiStatus.Text = "API: جاري الفحص...";
            lblDatabaseStatus.Text = "قاعدة البيانات: جاري الفحص...";
            lblLicenseStatus.Text = "الترخيص: ساري";
            lblVersion.Text = "الإصدار: 1.0.0";
            lblDateTime.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
            try
            {
                // الفحص مستقل عن تحميل القوائم حتى لا يظهر اتصال قاعدة البيانات كاذباً عند فشلها.
                var health = await _client.GetAsync($"{_baseUrl}health");
                if (!health.IsSuccessStatusCode)
                    throw new HttpRequestException("خدمة النظام أو قاعدة البيانات غير جاهزة.");

                await LoadCompaniesAsync();
                lblApiStatus.Text = "API: متصل";
                lblDatabaseStatus.Text = "قاعدة البيانات: متصلة";
            }
            catch (Exception ex)
            {
                lblApiStatus.Text = "API: غير متصل";
                lblDatabaseStatus.Text = "قاعدة البيانات: غير متصلة";
                btnLogin.Enabled = false;
                MessageBox.Show("تعذر الاتصال بخدمة النظام.\n" + ex.Message, "خطأ تهيئة", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadCompaniesAsync()
        {
            _loadingCompany = true;
            try
            {
                var companies = await _client.GetFromJsonAsync<List<CompanyLookupModel>>($"{_baseUrl}Branches/GetCompaniesLookup") ?? new();
                cmbCompany.DataSource = companies;
                cmbCompany.DisplayMember = "Company_Name_AR";
                cmbCompany.ValueMember = "Company_ID";
                cmbCompany.SelectedIndex = companies.Count == 1 ? 0 : -1;
            }
            finally { _loadingCompany = false; }

            if (cmbCompany.SelectedValue != null) await LoadCompanyContextAsync();
        }

        // تحميل الفروع والسنوات للشركة المختارة فقط؛ لا تُحمّل المستخدمين قبل التحقق.
        private async Task LoadCompanyContextAsync()
        {
            var companyId = cmbCompany.SelectedValue?.ToString();
            if (string.IsNullOrWhiteSpace(companyId)) return;

            var branchesTask = _client.GetFromJsonAsync<List<BranchLookupModel>>($"{_baseUrl}Branches/GetActiveBranchesLookup?companyId={companyId}");
            var yearsTask = _client.GetFromJsonAsync<List<FiscalYearLookupModel>>($"{_baseUrl}FiscalYears/Lookup?companyId={companyId}");
            await Task.WhenAll(branchesTask, yearsTask);

            Bind(cmbBranch, branchesTask.Result ?? new(), "Branch_Name", "Branch_ID");
            Bind(cmbFiscalYear, yearsTask.Result ?? new(), "Year_Name", "Fiscal_Year_ID");
            cmbUsername.DataSource = null;
            cmbUsername.Text = "";

            var defaultYear = (yearsTask.Result ?? new()).FirstOrDefault(x => x.Is_Default);
            if (defaultYear != null) cmbFiscalYear.SelectedValue = defaultYear.Fiscal_Year_ID;
            txtPassword.Clear();
        }

        private static void Bind<T>(ComboBox combo, List<T> items, string display, string value)
        {
            combo.DataSource = null;
            combo.DisplayMember = display;
            combo.ValueMember = value;
            combo.DataSource = items;
            combo.SelectedIndex = -1;
        }

        private async void cmbCompany_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_loadingCompany || cmbCompany.SelectedValue == null) return;
            try { await LoadCompanyContextAsync(); }
            catch (Exception ex) { MessageBox.Show("تعذر تحميل بيانات الشركة.\n" + ex.Message, "الدخول", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        // لا يُنشأ السياق المحلي إلا بعد قبول الخادم لكامل سياق الدخول.
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
            try
            {
                var request = new LoginRequest
                {
                    Company_ID = cmbCompany.SelectedValue.ToString() ?? "",
                    Branch_ID = Convert.ToInt32(cmbBranch.SelectedValue),
                    Year_ID = Convert.ToInt32(cmbFiscalYear.SelectedValue),
                    User_ID = 0,
                    Login_Name = cmbUsername.Text.Trim(),
                    Password = txtPassword.Text
                };

                var response = await _client.PostAsJsonAsync($"{_baseUrl}Auth/Login", request);
                if (!response.IsSuccessStatusCode)
                {
                    var message = await response.Content.ReadAsStringAsync();
                    MessageBox.Show(string.IsNullOrWhiteSpace(message) ? "تعذر التحقق من بيانات الدخول." : message, "فشل الدخول", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.SelectAll();
                    txtPassword.Focus();
                    return;
                }

                var result = await response.Content.ReadFromJsonAsync<LoginResultModel>();
                if (result == null) throw new InvalidOperationException("لم تُرجع الخدمة جلسة صالحة.");

                // حفظ البيانات التي أعادها الخادم فقط، فهي المرجع الموثوق للجلسة.
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
                ApiService.ApplySessionToken(result.Access_Token);
                CurrentSession.Login_Time = DateTime.Now;

                // تُغلق الشاشة الرئيسية عند الخروج فتظهر شاشة الدخول نفسها من جديد.
                Hide();
                using var main = new FrmMain();
                main.ShowDialog(this);
                Show();
                txtPassword.Clear();
                cmbUsername.Focus();
            }
            catch (Exception ex) { MessageBox.Show("تعذر إتمام تسجيل الدخول.\n" + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { btnLogin.Enabled = true; }
        }
    }

    public class LoginRequest { public string Company_ID { get; set; } = ""; public int Branch_ID { get; set; } public int Year_ID { get; set; } public int User_ID { get; set; } public string Login_Name { get; set; } = ""; public string Password { get; set; } = ""; }
    public class LoginResultModel { public int User_ID { get; set; } public string Full_Name { get; set; } = ""; public string Login_Name { get; set; } = ""; public int Role_ID { get; set; } public int Branch_ID { get; set; } public string Company_ID { get; set; } = ""; public int Year_ID { get; set; } public bool Is_System_Admin { get; set; } public bool Must_Change_Password { get; set; } public string Access_Token { get; set; } = ""; }
    public class FiscalYearLookupModel { public int Fiscal_Year_ID { get; set; } public string Year_Name { get; set; } = ""; public bool Is_Default { get; set; } }
    public class UserLookupModel { public int User_ID { get; set; } public string Login_Name { get; set; } = ""; public int Branch_ID { get; set; } }
}