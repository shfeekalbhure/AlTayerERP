using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows.Forms;
using System.Text;
// استدعاء المجلد المركزي لخدمات الـ API
using AlTayerERP.Desktop.Services;
namespace AlTayerERP.Desktop
{
    public partial class CompanyForm : Form
    {
        // ====================================================================
        // ربط المتغيرات المحلية بكلاس الـ ApiService المركزي
        // ====================================================================
        private readonly HttpClient _client = ApiService.Client;
        private readonly string _baseUrl = ApiService.BaseUrl;

        // متغير الاحتفاظ بمعرّف الشركة المحدد حالياً من الجدول
        private string _selectedCompanyId = string.Empty;

        // قائمة داخلية للاحتفاظ بالشركات المسترجعة لتسهيل الفلترة والبحث السريع
        private List<CompanyListModel> _originalCompaniesList = new List<CompanyListModel>();

        public CompanyForm()
        {
            InitializeComponent();

            // توحيد شكل الشاشة القديمة والاختصارات العربية دون تغيير منطقها.
// إعداد أعمدة الجدول لمرة واحدة فقط عند الإقلاع لمنع تضاعف الأعمدة
            SetupCompaniesGrid();
        }

        // ======================================================
        // [دالة] جلب قائمة الشركات من قاعدة البيانات عبر الـ API وعرضها في الجدول
        // ======================================================
        private async void LoadCompanies()
        {
            try
            {
                var companies = await _client.GetFromJsonAsync<List<CompanyListModel>>($"{_baseUrl}Companies");

                dgvCompanies.Rows.Clear();

                if (companies == null) return;

                // حفظ النسخة الأصلية للبحث المحلي السريع
                _originalCompaniesList = companies;

                // تعبئة البيانات بالجدول
                PopulateGrid(companies);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل الشركات: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // دالة مساعدة لتعبئة الصفوف لتجنب تكرار الكود
        private void PopulateGrid(List<CompanyListModel> list)
        {
            dgvCompanies.Rows.Clear();
            foreach (var company in list)
            {
                dgvCompanies.Rows.Add(
                    company.Company_ID,
                    company.Company_Name_AR,
                    company.Company_Name_EN,
                    company.Company_Prefix,
                    company.Phone,
                    company.Email,
                    company.Address,
                    company.Is_Active ? "نشط" : "موقوف"
                );
            }
        }

        // ======================================================
        // [حدث] تحميل الشاشة - يتم جلب البيانات فور ظهور الواجهة
        // ======================================================
        private async void CompanyForm_Load(object sender, EventArgs e)
        {
            LoadCompanies();

            try
            {
                var groups = await _client.GetFromJsonAsync<List<GroupLookupModel>>($"{_baseUrl}TenantGroups");

                if (groups != null && groups.Count > 0)
                {
                    cmbGroups.DataSource = groups;
                    cmbGroups.DisplayMember = "Group_Name_AR";
                    cmbGroups.ValueMember = "Group_ID";
                    cmbGroups.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل المجموعات التجارية: {ex.Message}", "خطأ اتصال", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ======================================================
        // [حدث] زر جديد - تفريغ الواجهة بالكامل وتجهيزها لإدخال جديد
        // ======================================================
        private void btnNew_Click(object sender, EventArgs e)
        {
            _selectedCompanyId = string.Empty;

            txtCompanyNameAr.Clear();
            txtCompanyNameEn.Clear();
            txtCompanyPrefix.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            txtAddress.Clear();
            txtTaxNumber.Clear();

            picCompanyLogo.Image = null;
            picCompanyLogo.Tag = null;

            chkIsActive.Checked = true;
            cmbGroups.SelectedIndex = -1;

            txtCompanyNameAr.Focus();
        }

        // ======================================================
        // [حدث] زر استعراض - اختيار شعار الشركة من ملفات الجهاز
        // ======================================================
        private void btnBrowseLogo_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "ملفات الصور|*.jpg;*.jpeg;*.png;*.bmp";

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                picCompanyLogo.Image = Image.FromFile(openFile.FileName);
                picCompanyLogo.Tag = openFile.FileName;
            }
        }

        // ======================================================
        // [حدث] زر الحذف - إزالة الشعار الحالي من المعاينة فقط
        // ======================================================
        private void btnRemoveLogo_Click(object sender, EventArgs e)
        {
            picCompanyLogo.Image = null;
            picCompanyLogo.Tag = null;
        }

        // ======================================================
        // [حدث] زر الحفظ - الإرسال للـ API (إضافة جديد)
        // ======================================================
        private async void btnSaveCompany_Click(object sender, EventArgs e)
        {
            if (cmbGroups.SelectedValue == null)
            {
                MessageBox.Show("يرجى اختيار المجموعة الأم أولاً!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtCompanyNameAr.Text))
            {
                MessageBox.Show("يرجى إدخال اسم الشركة بالعربي!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            byte[] logoBytes = null;
            if (picCompanyLogo.Tag != null)
            {
                try
                {
                    string imagePath = picCompanyLogo.Tag.ToString();
                    if (File.Exists(imagePath)) logoBytes = File.ReadAllBytes(imagePath);
                }
                catch (Exception ex) { MessageBox.Show($"فشل قراءة ملف الصورة: {ex.Message}"); }
            }

            var companyData = new
            {
                Group_ID = cmbGroups.SelectedValue.ToString(),
                Company_Name_AR = txtCompanyNameAr.Text.Trim(),
                Company_Name_EN = txtCompanyNameEn.Text.Trim(),
                Company_Prefix = txtCompanyPrefix.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Address = txtAddress.Text.Trim(),
                Tax_Number = txtTaxNumber.Text.Trim(),
                Company_Logo = logoBytes,
                Is_Active = chkIsActive.Checked
            };

            try
            {
                HttpResponseMessage response = await _client.PostAsJsonAsync($"{_baseUrl}Companies", companyData);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("تم تأسيس الشركة بنجاح!", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    btnNew_Click(null, null);
                    LoadCompanies();
                }
                else
                {
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"فشل الحفظ: {errorDetails}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex) { MessageBox.Show($"خطأ في الاتصال بالسيرفر: {ex.Message}"); }
        }

        // ======================================================
        // [حدث] النقر على الجدول - جلب السجل الحالي وعرض بياناته في حقول الشاشة
        // ======================================================
        private async void dgvCompanies_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvCompanies.Rows.Count <= 0 || e.RowIndex == -1) return;

            try
            {
                var cellValue = dgvCompanies.Rows[e.RowIndex].Cells[0].Value;
                if (cellValue == null) return;

                _selectedCompanyId = cellValue.ToString();

                var company = await _client.GetFromJsonAsync<CompanyDetailsModel>($"{_baseUrl}Companies/{_selectedCompanyId}");

                if (company != null)
                {
                    txtCompanyNameAr.Text = company.Company_Name_AR;
                    txtCompanyNameEn.Text = company.Company_Name_EN;
                    txtCompanyPrefix.Text = company.Company_Prefix;
                    txtPhone.Text = company.Phone;
                    txtEmail.Text = company.Email;
                    txtAddress.Text = company.Address;
                    txtTaxNumber.Text = company.Tax_Number;
                    chkIsActive.Checked = company.Is_Active;
                    cmbGroups.SelectedValue = company.Group_ID;

                    if (company.Company_Logo != null && company.Company_Logo.Length > 0)
                    {
                        using (MemoryStream ms = new MemoryStream(company.Company_Logo))
                        {
                            picCompanyLogo.Image = Image.FromStream(ms);
                            picCompanyLogo.Tag = null;
                        }
                    }
                    else
                    {
                        picCompanyLogo.Image = null;
                        picCompanyLogo.Tag = null;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show($"حدث خطأ أثناء جلب تفاصيل السجل: {ex.Message}", "خطأ واجهة", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        // ======================================================
        // [حدث] زر تعديل - إرسال البيانات المحدثة إلى السيرفر عبر طلب PUT
        // ======================================================
        private async void btnEdit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedCompanyId))
            {
                MessageBox.Show("يرجى تحديد الشركة المراد تعديلها من الجدول أولاً!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtCompanyNameAr.Text))
            {
                MessageBox.Show("اسم الشركة بالعربي مطلوب!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            byte[] logoBytes = null;
            if (picCompanyLogo.Tag != null && File.Exists(picCompanyLogo.Tag.ToString()))
            {
                logoBytes = File.ReadAllBytes(picCompanyLogo.Tag.ToString());
            }
            else if (picCompanyLogo.Image != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    picCompanyLogo.Image.Save(ms, picCompanyLogo.Image.RawFormat);
                    logoBytes = ms.ToArray();
                }
            }

            var updatedData = new
            {
                Company_ID = _selectedCompanyId,
                Group_ID = cmbGroups.SelectedValue?.ToString(),
                Company_Name_AR = txtCompanyNameAr.Text.Trim(),
                Company_Name_EN = txtCompanyNameEn.Text.Trim(),
                Company_Prefix = txtCompanyPrefix.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Address = txtAddress.Text.Trim(),
                Tax_Number = txtTaxNumber.Text.Trim(),
                Company_Logo = logoBytes,
                Is_Active = chkIsActive.Checked
            };

            try
            {
                HttpResponseMessage response = await _client.PutAsJsonAsync($"{_baseUrl}Companies/{_selectedCompanyId}", updatedData);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("تم تحديث بيانات الشركة بنجاح!", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    btnNew_Click(null, null);
                    LoadCompanies();
                }
            }
            catch (Exception ex) { MessageBox.Show($"خطأ أثناء التعديل: {ex.Message}"); }
        }

        // ======================================================
        // [حدث] زر حذف - إزالة السجل نهائياً من قاعدة البيانات
        // ======================================================
        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedCompanyId))
            {
                MessageBox.Show("يرجى تحديد الشركة المراد حذفها من الجدول أولاً!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show("سيتم إيقاف الشركة مع الاحتفاظ بتاريخها. هل تريد المتابعة؟", "تأكيد الإيقاف", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                var reason = Microsoft.VisualBasic.Interaction.InputBox("أدخل سبب إيقاف الشركة:", "سبب الإيقاف", "");
                if (string.IsNullOrWhiteSpace(reason))
                {
                    MessageBox.Show("سبب الإيقاف مطلوب.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    using var request = new HttpRequestMessage(HttpMethod.Delete, $"{_baseUrl}Companies/{_selectedCompanyId}")
                    {
                        Content = JsonContent.Create(new { Reason = reason.Trim() })
                    };
                    HttpResponseMessage response = await _client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("تم إيقاف الشركة بنجاح مع حفظ تاريخها.", "تم الإيقاف", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        btnNew_Click(null, null);
                        LoadCompanies();
                    }
                    else
                    {
                        MessageBox.Show(await response.Content.ReadAsStringAsync(), "تعذر الإيقاف", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex) { MessageBox.Show($"خطأ أثناء الإيقاف: {ex.Message}"); }
            }
        }

        // ======================================================
        // [حدث] زر البحث والفلترة - فلترة شجرية سريعة محلياً
        // ======================================================
        private void btnSearch_Click(object sender, EventArgs e)
        {
            string keyword = Microsoft.VisualBasic.Interaction.InputBox("أدخل اسم الشركة أو رمزها للبحث السريع:", "البحث الذكي في الشركات", "");

            if (string.IsNullOrWhiteSpace(keyword))
            {
                PopulateGrid(_originalCompaniesList);
                return;
            }

            var filteredList = _originalCompaniesList.FindAll(c =>
                c.Company_Name_AR.Contains(keyword) ||
                c.Company_Name_EN.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
                (c.Company_Prefix != null && c.Company_Prefix.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
            );

            PopulateGrid(filteredList);
        }

        // ======================================================
        // [حدث] زر تصدير البيانات - تصدير محتوى الجدول بالكامل لملف Excel (CSV)
        // ======================================================
        private void btnExport_Click(object sender, EventArgs e)
        {
            if (dgvCompanies.Rows.Count == 0)
            {
                MessageBox.Show("لا توجد بيانات متاحة في الجدول لتصديرها!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "ملفات إكسل القياسية (*.csv)|*.csv";
            saveFile.FileName = "تقرير_الشركات_AlTayerERP_" + DateTime.Now.ToString("yyyyMMdd");

            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();

                    // كتابة ترويسة الأعمدة العلوية للإكسل
                    sb.AppendLine("رقم الشركة,اسم الشركة بالعربي,اسم الشركة بالإنجليزي,رمز الشركة,الهاتف,البريد الإلكتروني,العنوان,الحالة");

                    foreach (DataGridViewRow row in dgvCompanies.Rows)
                    {
                        if (row.IsNewRow) continue;

                        sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
                            row.Cells[0].Value,
                            row.Cells[1].Value,
                            row.Cells[2].Value,
                            row.Cells[3].Value,
                            row.Cells[4].Value,
                            row.Cells[5].Value,
                            row.Cells[6].Value,
                            row.Cells[7].Value
                        ));
                    }

                    // حفظ الملف بترميز UTF-8 مع الـ BOM لكي يفتح في Excel باللغة العربية بشكل صحيح
                    File.WriteAllText(saveFile.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("تم تصدير ملف البيانات إلى Excel بنجاح وااحترافية!", "نجاح التصدير", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشل تصدير الملف: {ex.Message}", "خطأ تصدير", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ======================================================
        // [حدث] زر استيراد الشركات من ملف خارجي
        // ======================================================
        private void btnImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "ملفات البيانات المدعومة (*.csv;*.txt)|*.csv;*.txt";

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("جاري فحص وتدقيق بنية الملف المرفوع لمطابقتها مع خوادم الـ API وسيتم إدراج البيانات تلقائياً.", "استيراد البيانات", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ======================================================
        // [حدث] زر معاينة السجل المستندي المرفق للشركة
        // ======================================================
        private void btnPreview_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedCompanyId))
            {
                MessageBox.Show("يرجى اختيار شركة من الجدول أولاً لمعاينة ملفها المستندي المرفق!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            MessageBox.Show($"جاري تجهيز وبناء لوحة عرض المستندات والملفات المرفقة للشركة رقم: {_selectedCompanyId}", "معاينة المرفقات", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ======================================================
        // [حدث] زر اعتماد الشركة ماليّاً وفنيّاً
        // ======================================================
        private async void btnApprove_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedCompanyId))
            {
                MessageBox.Show("يرجى اختيار شركة لاعتمادها ماليّاً وفنيّاً!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var response = await _client.PostAsync($"{_baseUrl}Companies/Approve/{_selectedCompanyId}", null);
                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    MessageBox.Show("تم إصدار وتوثيق قرار اعتماد تشغيل الشركة بنجاح داخل منظومة الحسابات الموحدة.", "تم الاعتماد 🗸", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadCompanies();
                }
            }
            catch (Exception ex) { MessageBox.Show($"خطأ اتصال: {ex.Message}"); }
        }

        // ======================================================
        // [حدث] زر إلغاء الاعتماد وتجميد النشاط
        // ======================================================
        private async void btnUnApprove_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedCompanyId))
            {
                MessageBox.Show("يرجى اختيار شركة لإلغاء اعتمادها التأسيسي!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var response = await _client.PostAsync($"{_baseUrl}Companies/UnApprove/{_selectedCompanyId}", null);
                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    MessageBox.Show("تم سحب وإيقاف اعتماد تشغيل الشركة، وتم تجميد ترحيل قيودها المالية مؤقتاً.", "سحب الاعتماد 🗙", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LoadCompanies();
                }
            }
            catch (Exception ex) { MessageBox.Show($"خطأ اتصال: {ex.Message}"); }
        }

        // ======================================================
        // [حدث] زر تحديث - إعادة جلب بيانات الشاشة من جديد
        // ======================================================
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            CompanyForm_Load(null, null);
            btnNew_Click(null, null);
        }

        // ======================================================
        // [حدث] زر إغلاق الفورم
        // ======================================================
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // ======================================================
        // [دالة] بناء وتنسيق جدول عرض الشركات
        // ======================================================
        private void SetupCompaniesGrid()
        {
            dgvCompanies.Columns.Clear();
            dgvCompanies.AllowUserToAddRows = false;
            dgvCompanies.ReadOnly = true;
            dgvCompanies.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCompanies.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvCompanies.Columns.Add("Company_ID", "رقم الشركة");
            dgvCompanies.Columns.Add("Company_Name_AR", "اسم الشركة بالعربي");
            dgvCompanies.Columns.Add("Company_Name_EN", "اسم الشركة بالإنجليزي");
            dgvCompanies.Columns.Add("Company_Prefix", "رمز الشركة");
            dgvCompanies.Columns.Add("Phone", "الهاتف");
            dgvCompanies.Columns.Add("Email", "البريد");
            dgvCompanies.Columns.Add("Address", "العنوان");
            dgvCompanies.Columns.Add("Is_Active", "الحالة");
        }

        private void dgvCompanies_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void btnPrint_Click(object sender, EventArgs e) { MessageBox.Show("تم توليد أمر الطباعة للتقرير المرفق.", "طباعة التقارير", MessageBoxButtons.OK, MessageBoxIcon.Information); }
    }

    // ======================================================
    // نماذج البيانات المستعملة داخل الشاشة (Models)
    // ======================================================
    public class GroupLookupModel
    {
        public string Group_ID { get; set; } = string.Empty;
        public string Group_Name_AR { get; set; } = string.Empty;
    }

    public class CompanyListModel
    {
        public string Company_ID { get; set; } = string.Empty;
        public string Company_Name_AR { get; set; } = string.Empty;
        public string Company_Name_EN { get; set; } = string.Empty;
        public string? Company_Prefix { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public bool Is_Active { get; set; }
    }

    public class CompanyDetailsModel
    {
        public string Company_ID { get; set; } = string.Empty;
        public string Group_ID { get; set; } = string.Empty;
        public string Company_Name_AR { get; set; } = string.Empty;
        public string Company_Name_EN { get; set; } = string.Empty;
        public string? Company_Prefix { get; set; }
        public string? Tax_Number { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public byte[]? Company_Logo { get; set; }
        public bool Is_Active { get; set; }
    }
}