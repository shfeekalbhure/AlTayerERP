using AlTayerERP.Desktop.Services; // استيراد خدمات المشروع مثل ApiService و CurrentSession
using System;
using System.Collections.Generic;
using System.Net.Http; // استيراد مكتبة التعامل مع بروتوكول HTTP لارسال واستقبال البيانات
using System.Net.Http.Json; // استيراد ميزة تحويل البيانات تلقائياً من وإلى صيغة JSON
using System.Threading.Tasks; // استيراد مكتبة العمليات غير المتزامنة Asynchronous Tasks
using System.Windows.Forms; // استيراد مكتبة واجهات ويندوز الفيجوال الأساسية
using System.Linq; // مطلوب لاستخدام FirstOrDefault

namespace AlTayerERP.Desktop
{
    // كلاس شاشة تسجيل الدخول الرئيسي والمصمم كـ partial ليتكامل مع ملف الـ Designer
    public partial class FrmLogin : Form
    {
        // استدعاء الكائن المركزي لإرسال طلبات الويب من كلاس الخدمات الموحد للمشروع
        private readonly HttpClient _client = ApiService.Client;
        // جلب الرابط الأساسي للـ API المتفق عليه في خدمات النظام
        private readonly string _baseUrl = ApiService.BaseUrl;

        // مُشيّد الشاشة (Constructor) - يتم استدعاؤه فور إنشاء الفورم 
        public FrmLogin()
        {
            InitializeComponent(); // بناء وتهيئة عناصر الواجهة الرسومية المصممة 
                                   // الإلغاء ثم الاشتراك لحدث تحميل الشاشة لتفادي تكرار التنفيذ في الذاكرة 
            this.Load -= FrmLogin_Load;
            this.Load += FrmLogin_Load;
            // ربط حدث النقر على زر تسجيل الدخول بالدالة البرمجية الخاصة به بأمان 
            btnLogin.Click -= btnLogin_Click;
            btnLogin.Click += btnLogin_Click;
            // ربط حدث النقر على زر الخروج 
            btnExit.Click -= btnExit_Click;
            btnExit.Click += btnExit_Click;
            // ربط حدث النقر على زر "حول النظام" 
            btnAboutSystem.Click -= btnAboutSystem_Click;
            btnAboutSystem.Click += btnAboutSystem_Click;
            // ربط حدث النقر على زر إعدادات الاتصال بالسيرفر 
            btnConnectionSettings.Click -= btnConnectionSettings_Click;
            btnConnectionSettings.Click += btnConnectionSettings_Click;

            // ربط حدث تغيير الشركة المختارة بأمان لمنع تكرار الاستدعاء
            cmbCompany.SelectedIndexChanged -= cmbCompany_SelectedIndexChanged;
            cmbCompany.SelectedIndexChanged += cmbCompany_SelectedIndexChanged;

            // قناع حقل كلمة المرور لجعل النص يظهر بنجوم لحماية السرية بالكامل 
            txtPassword.PasswordChar = '*';
            this.AcceptButton = btnLogin;
            this.CancelButton = btnExit;
        }

        // الحدث المسؤول عن تهيئة الشاشة بمجرد تشغيلها وجلب الشركات فقط
        private async void FrmLogin_Load(object sender, EventArgs e)
        {
            try
            {
                // تحديث النصوص في شريط الحالة السفلي لبدء عملية الفحص والتحضير 
                lblApiStatus.Text = "API: جاري الفحص...";
                lblDatabaseStatus.Text = "قاعدة البيانات: جاري الفحص...";
                lblLicenseStatus.Text = "الترخيص: ساري";
                lblVersion.Text = "الإصدار: 1.0.0";
                lblDateTime.Text = DateTime.Now.ToString("yyyy/MM/dd hh:mm tt"); // تنسيق الوقت والتاريخ الحالي 

                // استدعاء دالة جلب الشركات فقط عند الإقلاع لتفعيل التسلسل المنطقي
                await LoadCompaniesAsync();

                // عند نجاح جلب البيانات بالكامل يتم تحديث شريط الحالة بنجاح الاتصال 
                lblApiStatus.Text = "API: متصل";
                lblDatabaseStatus.Text = "قاعدة البيانات: متصلة";
                // ضبط القائمة المنسدلة لأسماء المستخدمين لتبدأ بدون اختيار افتراضي ومسح كلمة المرور 
                cmbUsername.SelectedIndex = -1;
                txtPassword.Clear();
                cmbUsername.Focus(); // توجيه مؤشر التركيز تلقائياً إلى خانة اختيار المستخدم 
            }
            catch (Exception ex)
            {
                // في حال حدوث أي خطأ في الشبكة أو جلب البيانات يتم إخطار المستخدم وتحديث شريط الحالة بفشل الاتصال 
                lblApiStatus.Text = "API: غير متصل";
                lblDatabaseStatus.Text = "قاعدة البيانات: غير متصلة";

                // تعطيل زر الدخول عند فشل تحميل البيانات من السيرفر لمنع الطلبات العشوائية
                btnLogin.Enabled = false;

                MessageBox.Show("فشل تحميل بيانات الدخول:\n" + ex.Message, "خطأ تهيئة", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // دالة جلب الشركات المتاحة من السيرفر وتعبئتها داخل الـ ComboBox الخاص بالشركات 
        private async Task LoadCompaniesAsync()
        {
            var data = await _client.GetFromJsonAsync<List<CompanyLookupModel>>($"{_baseUrl}Branches/GetCompaniesLookup");
            cmbCompany.DataSource = data ?? new List<CompanyLookupModel>(); // إسناد البيانات أو قائمة فارغة لتجنب الـ Null 
            cmbCompany.DisplayMember = "Company_Name_AR"; // النص الظاهر للمستخدم يطابق الـ API تماماً 
            cmbCompany.ValueMember = "Company_ID"; // القيمة البرمجية المخفية في الخلفية (معرف الشركة) 
        }

        // دالة جلب فروع المؤسسة المصفاة بحسب الشركة المحددة
        private async Task LoadBranchesAsync()
        {
            string companyId = cmbCompany.SelectedValue?.ToString() ?? "";
            var data = await _client.GetFromJsonAsync<List<BranchLookupModel>>($"{_baseUrl}Branches/GetActiveBranchesLookup?companyId={companyId}");
            cmbBranch.DataSource = data ?? new List<BranchLookupModel>();
            cmbBranch.DisplayMember = "Branch_Name";
            cmbBranch.ValueMember = "Branch_ID";
            cmbBranch.SelectedIndex = -1;
        }

        // دالة جلب السنوات المالية المعتمدة والمصفاة بحسب الشركة المحددة وتحديد الافتراضي بالترتيب الصحيح
        private async Task LoadFiscalYearsAsync()
        {
            string companyId = cmbCompany.SelectedValue?.ToString() ?? "";
            var data = await _client.GetFromJsonAsync<List<FiscalYearLookupModel>>($"{_baseUrl}FiscalYears?companyId={companyId}");

            cmbFiscalYear.DataSource = null;
            cmbFiscalYear.DisplayMember = "Year_Name";
            cmbFiscalYear.ValueMember = "Fiscal_Year_ID";
            cmbFiscalYear.DataSource = data ?? new List<FiscalYearLookupModel>();

            var defaultYear = data?.FirstOrDefault(x => x.Is_Default);
            if (defaultYear != null)
                cmbFiscalYear.SelectedValue = defaultYear.Fiscal_Year_ID;
        }

        // دالة جلب بيانات حسابات المستخدمين المصفاة بحسب الشركة المحددة
        private async Task LoadUsersAsync()
        {
            string companyId = cmbCompany.SelectedValue?.ToString() ?? "";
            var data = await _client.GetFromJsonAsync<List<UserLookupModel>>($"{_baseUrl}Users/GetUsersLookup?companyId={companyId}");
            cmbUsername.DataSource = data ?? new List<UserLookupModel>();
            cmbUsername.DisplayMember = "Login_Name"; // اسم المستخدم المخصص لتسجيل الدخول 
            cmbUsername.ValueMember = "User_ID"; // معرف المستخدم الفريد بقاعدة البيانات 
        }

        // الدالة الجديدة والمجهزة لاستقبال جلب صورة الشعار من السيرفر لاحقاً
        private async Task LoadCompanyLogoAsync()
        {
            if (cmbCompany.SelectedValue == null) return;
            // سيتم استدعاء API لجلب شعار الشركة لاحقاً 
            await Task.CompletedTask;
        }

        // الحدث الرئيسي والمسؤول عن الضغط على زر تسجيل الدخول وفحص الصلاحيات مع الـ API 
        private async void btnLogin_Click(object sender, EventArgs e)
        {
            btnLogin.Enabled = false; // تعطيل الزر لمنع طلبات مكررة 
            try
            {
                // التحقق من الحقول الأساسية مع بقاء الفرع اختيارياً
                if (cmbCompany.SelectedValue == null || cmbFiscalYear.SelectedValue == null || cmbUsername.SelectedValue == null || string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show("يرجى إدخال واختيار جميع بيانات الدخول الأساسية.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    btnLogin.Enabled = true;
                    return;
                }

                string companyId = cmbCompany.SelectedValue?.ToString() ?? "";
                int branchId = cmbBranch.SelectedValue != null ? Convert.ToInt32(cmbBranch.SelectedValue) : 0;
                int yearId = cmbFiscalYear.SelectedValue != null ? Convert.ToInt32(cmbFiscalYear.SelectedValue) : 0;
                int userId = cmbUsername.SelectedValue != null ? Convert.ToInt32(cmbUsername.SelectedValue) : 0;

                var request = new LoginRequest { Company_ID = companyId, Branch_ID = branchId, Year_ID = yearId, User_ID = userId, Password = txtPassword.Text.Trim() };
                var response = await _client.PostAsJsonAsync($"{_baseUrl}Auth/Login", request);
                if (!response.IsSuccessStatusCode)
                {
                    string errorMsg = "اسم المستخدم أو كلمة المرور غير صحيحة، أو السيرفر غير مستجيب.";
                    if (response.Content.Headers.ContentType?.MediaType == "text/plain")
                    {
                        errorMsg = await response.Content.ReadAsStringAsync();
                    }
                    MessageBox.Show(errorMsg, "فشل الدخول", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.SelectAll();
                    txtPassword.Focus();
                    btnLogin.Enabled = true;
                    return;
                }
                var result = await response.Content.ReadFromJsonAsync<LoginResultModel>();
                // تعبئة بيانات الجلسة المركزية من الـ السيرفر أو من قيم الطلب الحالية بشكل دقيق لمنع التعارض 



              // معرفات الجلسة
               CurrentSession.Company_ID = result?.Company_ID ?? request.Company_ID;
                CurrentSession.Branch_ID = result?.Branch_ID ?? request.Branch_ID;
                CurrentSession.Year_ID = result?.Year_ID ?? request.Year_ID;

                CurrentSession.User_ID = result?.User_ID ?? request.User_ID;
                CurrentSession.Role_ID = result?.Role_ID ?? 0;

                // أسماء الجلسة
                CurrentSession.Company_Name =
                    cmbCompany.Text;

                CurrentSession.Branch_Name =
                    cmbBranch.Text;

                CurrentSession.Year_Name =
                    cmbFiscalYear.Text;

                CurrentSession.Username =
                    result?.Login_Name ?? cmbUsername.Text;

                CurrentSession.Full_Name =
                    result?.Full_Name ?? cmbUsername.Text;

                // بيانات إضافية
                CurrentSession.Is_System_Admin =
                    result?.Is_System_Admin ?? false;

                CurrentSession.Login_Time =
                    DateTime.Now;




                // ربط حقل مدير النظام مع الـ Static Property في الـ CurrentSession
                CurrentSession.Is_System_Admin = result?.Is_System_Admin ?? false;

                this.Hide();
                new FrmMain().Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء تسجيل الدخول:\n" + ex.Message, "خطأ غير متوقع", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnLogin.Enabled = true;
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnConnectionSettings_Click(object sender, EventArgs e)
        {
            MessageBox.Show("إعدادات الاتصال ستضاف لاحقاً.", "الإعدادات", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnAboutSystem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("نظام الطائر لإدارة النقل والشحن\nالإصدار 1.0.0", "حول النظام", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void picSystemLogo_Click(object sender, EventArgs e) { }
        private void grpLogin_Enter(object sender, EventArgs e) { }

        // الحدث المستدعى بشكل غير متزامن عند تغيير الشركة المختارة لتعبئة الفروع والمستخدمين والسنوات والشعار
        private async void cmbCompany_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCompany.SelectedValue == null) return;
            await LoadBranchesAsync();
            await LoadUsersAsync();
            await LoadFiscalYearsAsync();
            await LoadCompanyLogoAsync();
        }
        private void cmbBranch_SelectedIndexChanged(object sender, EventArgs e) { }
    }

    public class LoginRequest { public string Company_ID { get; set; } = ""; public int Branch_ID { get; set; } public int Year_ID { get; set; } public int User_ID { get; set; } public string Password { get; set; } = ""; }

    public class LoginResultModel
    {
        public int User_ID { get; set; }
        public string Full_Name { get; set; } = "";
        public string Login_Name { get; set; } = "";
        public int Role_ID { get; set; }
        public int Branch_ID { get; set; }
        public string Company_ID { get; set; } = "";
        public int Year_ID { get; set; }

        // الحقل الجديد المضاف لاستقبال حالة مدير النظام
        public bool Is_System_Admin { get; set; }
    }

    public class FiscalYearLookupModel
    {
        public int Fiscal_Year_ID { get; set; }
        public string Year_Name { get; set; } = "";
        public bool Is_Default { get; set; }
    }

    public class UserLookupModel { public int User_ID { get; set; } public string Login_Name { get; set; } = ""; }

    // تم الحفاظ على الكلاسات أدناه لضمان عدم حدوث خطأ أثناء تجميع الكود (Compilation) في الواجهة التبادلية للـ Combobox
    // public class CompanyLookupModel { public string Company_ID { get; set; } = ""; public string Company_Name_AR { get; set; } = ""; }
    // public class BranchLookupModel { public int Branch_ID { get; set; } public string Branch_Name { get; set; } = ""; }
}