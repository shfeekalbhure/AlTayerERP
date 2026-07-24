using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
// استدعاء المجلد المركزي لخدمات الـ API
using AlTayerERP.Desktop.Services;

namespace AlTayerERP.Desktop
{
    public partial class FrmNumberingSettings : Form
    {
        // ======================================================
        // كائن الاتصال بالـ API
        // ======================================================
        // إنشاء كائن ثابت لـ HttpClient للتعامل مع اتصالات الشبكة والـ API من الملف المركزي
        private readonly HttpClient _client = ApiService.Client;
        // ======================================================
        // رابط الـ API
        // ======================================================
        // الرابط الأساسي الثابت الذي يشير إلى موقع الـ API من الملف المركزي
        private readonly string _baseUrl = ApiService.BaseUrl;

        // ======================================================
        // رقم سجل الترقيم المحدد من الجدول
        // إذا كان 0 يعني لا يوجد سجل محدد
        // ======================================================
        private int _selectedNumberingId = 0;

        public FrmNumberingSettings()
        {
            InitializeComponent();

            // آخر رقم للعرض فقط: العداد المركزي في API هو المالك الوحيد لهذه القيمة.
            numLastNumber.Enabled = false;
            numLastNumber.ReadOnly = true;

            // تطبيق الثيم العربي الموحد والاختصارات على الشاشة القديمة.
// ==================================================
            // ربط حدث فتح الشاشة
            // ==================================================
            this.Load -= FrmNumberingSettings_Load;
            this.Load += FrmNumberingSettings_Load;

            // ==================================================
            // ربط الأزرار بالأحداث
            // ==================================================
            btnNew.Click -= btnNew_Click;
            btnNew.Click += btnNew_Click;

            btnSave.Click -= btnSave_Click;
            btnSave.Click += btnSave_Click;

            btnEdit.Click -= btnEdit_Click;
            btnEdit.Click += btnEdit_Click;

            btnDelete.Click -= btnDelete_Click;
            btnDelete.Click += btnDelete_Click;

            btnRefresh.Click -= btnRefresh_Click;
            btnRefresh.Click += btnRefresh_Click;

            btnClose.Click -= btnClose_Click;
            btnClose.Click += btnClose_Click;

            // ==================================================
            // ربط حدث الضغط على صف من الجدول
            // ==================================================
            dgvNumberingSettings.CellClick -= dgvNumberingSettings_CellClick;
            dgvNumberingSettings.CellClick += dgvNumberingSettings_CellClick;

            // ربط حدث تغيير نوع المستند حتى يتم اقتراح البادئة تلقائياً
            cmbDocumentType.SelectedIndexChanged -= cmbDocumentType_SelectedIndexChanged;
            cmbDocumentType.SelectedIndexChanged += cmbDocumentType_SelectedIndexChanged;

        }

        // ======================================================
        // عند فتح الشاشة
        // ======================================================
        private async void FrmNumberingSettings_Load(object sender, EventArgs e)
        {
            FillCombos();
            ClearForm();
            await LoadNumberingSettings();
        }




        // ======================================================
        // تعبئة القوائم بالقيم العربية الظاهرة للمستخدم
        // ======================================================
        private void FillCombos()
        {
            cmbDocumentType.Items.Clear();
            cmbDocumentType.Items.AddRange(new string[]
            {
                "الفروع",
                "الشركات",
                "البوالص",
                "التذاكر",
                "سند قبض",
                "سند صرف",
                "قيد محاسبي",
                "الرحلات",
                "العملاء",
                "المركبات",
                "السائقين"
            });

            cmbResetType.Items.Clear();
            cmbResetType.Items.AddRange(new string[]
            {
                "بدون تصفير",
                "حسب الشركة",
                "حسب الفرع",
                "حسب السنة",
                "حسب الشركة والسنة",
                "حسب الفرع والسنة"
            });
        }



// ======================================================
// عند اختيار نوع المستند
// يتم اقتراح البادئة وعدد الأرقام وطريقة التصفير تلقائياً
// ======================================================
private void cmbDocumentType_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cmbDocumentType.Text)
            {
                case "الفروع":
                    txtPrefix.Text = "BR";
                    numDigitsCount.Value = 4;
                    cmbResetType.Text = "حسب الشركة";
                    chkUseCompany.Checked = true;
                    chkUseBranch.Checked = false;
                    chkUseYear.Checked = false;
                    break;

                case "الشركات":
                    txtPrefix.Text = "CO";
                    numDigitsCount.Value = 4;
                    cmbResetType.Text = "بدون تصفير";
                    chkUseCompany.Checked = false;
                    chkUseBranch.Checked = false;
                    chkUseYear.Checked = false;
                    break;

                case "البوالص":
                    txtPrefix.Text = "SH";
                    numDigitsCount.Value = 6;
                    cmbResetType.Text = "حسب الفرع والسنة";
                    chkUseCompany.Checked = false;
                    chkUseBranch.Checked = true;
                    chkUseYear.Checked = true;
                    break;

                case "التذاكر":
                    txtPrefix.Text = "TK";
                    numDigitsCount.Value = 6;
                    cmbResetType.Text = "حسب السنة";
                    chkUseCompany.Checked = false;
                    chkUseBranch.Checked = false;
                    chkUseYear.Checked = true;
                    break;

                case "سند قبض":
                    txtPrefix.Text = "RCV";
                    numDigitsCount.Value = 6;
                    cmbResetType.Text = "حسب السنة";
                    chkUseCompany.Checked = false;
                    chkUseBranch.Checked = false;
                    chkUseYear.Checked = true;
                    break;

                case "سند صرف":
                    txtPrefix.Text = "PAY";
                    numDigitsCount.Value = 6;
                    cmbResetType.Text = "حسب السنة";
                    chkUseCompany.Checked = false;
                    chkUseBranch.Checked = false;
                    chkUseYear.Checked = true;
                    break;

                case "قيد محاسبي":
                    txtPrefix.Text = "JV";
                    numDigitsCount.Value = 6;
                    cmbResetType.Text = "حسب السنة";
                    chkUseCompany.Checked = false;
                    chkUseBranch.Checked = false;
                    chkUseYear.Checked = true;
                    break;

                case "الرحلات":
                    txtPrefix.Text = "TR";
                    numDigitsCount.Value = 5;
                    cmbResetType.Text = "حسب الفرع والسنة";
                    chkUseCompany.Checked = false;
                    chkUseBranch.Checked = true;
                    chkUseYear.Checked = true;
                    break;

                case "العملاء":
                    txtPrefix.Text = "CUS";
                    numDigitsCount.Value = 6;
                    cmbResetType.Text = "بدون تصفير";
                    chkUseCompany.Checked = false;
                    chkUseBranch.Checked = false;
                    chkUseYear.Checked = false;
                    break;

                case "المركبات":
                    txtPrefix.Text = "VEH";
                    numDigitsCount.Value = 5;
                    cmbResetType.Text = "بدون تصفير";
                    chkUseCompany.Checked = false;
                    chkUseBranch.Checked = false;
                    chkUseYear.Checked = false;
                    break;

                case "السائقين":
                    txtPrefix.Text = "DRV";
                    numDigitsCount.Value = 5;
                    cmbResetType.Text = "بدون تصفير";
                    chkUseCompany.Checked = false;
                    chkUseBranch.Checked = false;
                    chkUseYear.Checked = false;
                    break;
            }
        }






        // ======================================================
        // تحميل بيانات إعدادات الترقيم من الـ API إلى الجدول
        // ======================================================
        private async Task LoadNumberingSettings()
        {
            try
            {
                var data = await _client.GetFromJsonAsync<List<NumberingSettingModel>>(
                    $"{_baseUrl}NumberingSettings");

                if (data != null)
                {
                    foreach (var item in data)
                    {
                        item.Document_Type = GetDocumentTypeArabic(item.Document_Type);
                        item.Reset_Type = GetResetTypeArabic(item.Reset_Type);
                    }
                }

                dgvNumberingSettings.DataSource = data;


                if (dgvNumberingSettings.Columns.Count == 0)
                    return;

                // ==================================================
                // تحويل عناوين الجدول إلى عربي
                // ==================================================
                dgvNumberingSettings.Columns["Numbering_ID"].HeaderText = "رقم";
                dgvNumberingSettings.Columns["Document_Type"].HeaderText = "نوع المستند";
                dgvNumberingSettings.Columns["Prefix"].HeaderText = "البادئة";
                dgvNumberingSettings.Columns["Digits_Count"].HeaderText = "عدد الأرقام";
                dgvNumberingSettings.Columns["Reset_Type"].HeaderText = "طريقة التصفير";
                if (dgvNumberingSettings.Columns.Contains("Last_Number"))
                    dgvNumberingSettings.Columns["Last_Number"].HeaderText = "آخر رقم محجوز (للعرض فقط)";
                dgvNumberingSettings.Columns["Use_Company"].HeaderText = "حسب الشركة";
                dgvNumberingSettings.Columns["Use_Branch"].HeaderText = "حسب الفرع";
                dgvNumberingSettings.Columns["Use_Year"].HeaderText = "حسب السنة";
                dgvNumberingSettings.Columns["Is_Active"].HeaderText = "فعال";

                // ==================================================
                // تنسيق الجدول
                // ==================================================
                dgvNumberingSettings.RightToLeft = RightToLeft.Yes;
                dgvNumberingSettings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgvNumberingSettings.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgvNumberingSettings.ReadOnly = true;
                dgvNumberingSettings.AllowUserToAddRows = false;
                dgvNumberingSettings.ClearSelection();

                FillAvailableDocumentTypes();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"فشل تحميل إعدادات الترقيم:\n{ex.Message}",
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ======================================================
        // تعبئة قائمة نوع المستند بالأنواع غير المستخدمة فقط
        // ======================================================
        private void FillAvailableDocumentTypes()
        {
            // كل أنواع المستندات الممكنة
            List<string> allTypes = new List<string>
    {
        "الفروع",
        "الشركات",
        "البوالص",
        "التذاكر",
        "سند قبض",
        "سند صرف",
        "قيد محاسبي",
        "الرحلات",
        "العملاء",
        "المركبات",
        "السائقين"
    };

            // حذف الأنواع الموجودة مسبقاً في الجدول
            foreach (DataGridViewRow row in dgvNumberingSettings.Rows)
            {
                if (row.IsNewRow)
                    continue;

                string existingType = row.Cells["Document_Type"].Value?.ToString() ?? "";
                existingType = GetDocumentTypeArabic(existingType);

                allTypes.Remove(existingType);
            }

            // إعادة تعبئة القائمة بالأنواع المتبقية فقط
            cmbDocumentType.Items.Clear();
            cmbDocumentType.Items.AddRange(allTypes.ToArray());
        }


        // ======================================================
        // زر جديد
        // ======================================================
        private void btnNew_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        // ======================================================
        // زر حفظ سجل جديد فقط
        // ======================================================
        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (_selectedNumberingId != 0)
            {
                MessageBox.Show(
                    "هذا السجل موجود بالفعل. استخدم زر تعديل بدل حفظ.",
                    "تنبيه",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateForm())
                return;

            var data = BuildNumberingObject(0);

            try
            {
                var response = await _client.PostAsJsonAsync(
                    $"{_baseUrl}NumberingSettings",
                    data);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("تم حفظ إعداد الترقيم بنجاح");
                    ClearForm();
                    await LoadNumberingSettings();
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"فشل الحفظ:\n{error}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في الاتصال بالـ API:\n{ex.Message}");
            }
        }

        // ======================================================
        // زر تعديل السجل المحدد
        // ======================================================
        private async void btnEdit_Click(object sender, EventArgs e)
        {
            if (_selectedNumberingId == 0)
            {
                MessageBox.Show("يرجى اختيار سجل من الجدول أولاً");
                return;
            }

            if (!ValidateForm())
                return;

            var data = BuildNumberingObject(_selectedNumberingId);

            try
            {
                var response = await _client.PostAsJsonAsync(
                    $"{_baseUrl}NumberingSettings",
                    data);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("تم تعديل إعداد الترقيم بنجاح");
                    ClearForm();
                    await LoadNumberingSettings();
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"فشل التعديل:\n{error}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في الاتصال بالـ API:\n{ex.Message}");
            }
        }

        // ======================================================
        // زر حذف السجل المحدد
        // ======================================================
        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedNumberingId == 0)
            {
                MessageBox.Show("يرجى اختيار سجل من الجدول أولاً");
                return;
            }

            var confirm = MessageBox.Show(
                "هل أنت متأكد من حذف إعداد الترقيم المحدد؟",
                "تأكيد الحذف",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
                return;

            try
            {
                var response = await _client.DeleteAsync(
                    $"{_baseUrl}NumberingSettings/{_selectedNumberingId}");

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("تم حذف إعداد الترقيم بنجاح");
                    ClearForm();
                    await LoadNumberingSettings();
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"فشل الحذف:\n{error}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في الاتصال بالـ API:\n{ex.Message}");
            }
        }

        // ======================================================
        // زر تحديث
        // ======================================================
        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            await LoadNumberingSettings();
        }

        // ======================================================
        // زر إغلاق
        // ======================================================
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // ======================================================
        // عند الضغط على صف من الجدول
        // يتم نقل بياناته إلى الحقول
        // ======================================================
        private void dgvNumberingSettings_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            DataGridViewRow row = dgvNumberingSettings.Rows[e.RowIndex];

            _selectedNumberingId = Convert.ToInt32(row.Cells["Numbering_ID"].Value);


            // جلب نوع المستند الحالي وتحويله إلى عربي
            string currentDocumentType = GetDocumentTypeArabic(
                row.Cells["Document_Type"].Value?.ToString());

            // أثناء التعديل نعرض نوع المستند الحالي فقط
            cmbDocumentType.Items.Clear();
            cmbDocumentType.Items.Add(currentDocumentType);
            cmbDocumentType.SelectedIndex = 0;

            txtPrefix.Text = row.Cells["Prefix"].Value?.ToString();

            numDigitsCount.Value = Convert.ToDecimal(row.Cells["Digits_Count"].Value);

            cmbResetType.Text = GetResetTypeArabic(
                row.Cells["Reset_Type"].Value?.ToString());

            numLastNumber.Value = dgvNumberingSettings.Columns.Contains("Last_Number") &&
                row.Cells["Last_Number"].Value != null
                ? Convert.ToDecimal(row.Cells["Last_Number"].Value)
                : 0;

            chkUseCompany.Checked = Convert.ToBoolean(row.Cells["Use_Company"].Value);
            chkUseBranch.Checked = Convert.ToBoolean(row.Cells["Use_Branch"].Value);
            chkUseYear.Checked = Convert.ToBoolean(row.Cells["Use_Year"].Value);
            chkIsActive.Checked = Convert.ToBoolean(row.Cells["Is_Active"].Value);
        }

        // ======================================================
        // التحقق من الحقول قبل الحفظ أو التعديل
        // ======================================================
        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(cmbDocumentType.Text))
            {
                MessageBox.Show("يرجى اختيار نوع المستند");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPrefix.Text))
            {
                MessageBox.Show("يرجى إدخال البادئة");
                return false;
            }

            if (string.IsNullOrWhiteSpace(cmbResetType.Text))
            {
                MessageBox.Show("يرجى اختيار طريقة التصفير");
                return false;
            }

            return true;
        }

        // ======================================================
        // تحويل نوع المستند من عربي إلى كود برمجي للحفظ
        // ======================================================
        private string GetDocumentTypeCode()
        {
            return cmbDocumentType.Text switch
            {
                "الفروع" => "BRANCH",
                "الشركات" => "COMPANY",
                "البوالص" => "SHIPMENT",
                "التذاكر" => "TICKET",
                "سند قبض" => "RECEIPT_VOUCHER",
                "سند صرف" => "PAYMENT_VOUCHER",
                "قيد محاسبي" => "JOURNAL_ENTRY",
                "الرحلات" => "TRIP",
                "العملاء" => "CUSTOMER",
                "المركبات" => "VEHICLE",
                "السائقين" => "DRIVER",
                _ => cmbDocumentType.Text.Trim()
            };
        }

        // ======================================================
        // تحويل طريقة التصفير من عربي إلى كود برمجي للحفظ
        // ======================================================
        private string GetResetTypeCode()
        {
            return cmbResetType.Text switch
            {
                "بدون تصفير" => "None",
                "حسب الشركة" => "Company",
                "حسب الفرع" => "Branch",
                "حسب السنة" => "Year",
                "حسب الشركة والسنة" => "CompanyYear",
                "حسب الفرع والسنة" => "BranchYear",
                _ => cmbResetType.Text.Trim()
            };
        }

        // ======================================================
        // تحويل نوع المستند من كود برمجي إلى عربي للعرض
        // ======================================================
        private string GetDocumentTypeArabic(string? code)
        {
            return code switch
            {
                "BRANCH" => "الفروع",
                "COMPANY" => "الشركات",
                "SHIPMENT" => "البوالص",
                "TICKET" => "التذاكر",
                "RECEIPT_VOUCHER" => "سند قبض",
                "PAYMENT_VOUCHER" => "سند صرف",
                "JOURNAL_ENTRY" => "قيد محاسبي",
                "TRIP" => "الرحلات",
                "CUSTOMER" => "العملاء",
                "VEHICLE" => "المركبات",
                "DRIVER" => "السائقين",
                _ => code ?? string.Empty
            };
        }

        // ======================================================
        // تحويل طريقة التصفير من كود برمجي إلى عربي للعرض
        // ======================================================
        private string GetResetTypeArabic(string? code)
        {
            return code switch
            {
                "None" => "بدون تصفير",
                "Company" => "حسب الشركة",
                "Branch" => "حسب الفرع",
                "Year" => "حسب السنة",
                "CompanyYear" => "حسب الشركة والسنة",
                "BranchYear" => "حسب الفرع والسنة",
                _ => code ?? string.Empty
            };
        }

        // ======================================================
        // تجهيز كائن البيانات الذي سيتم إرساله إلى API
        // ======================================================
        private object BuildNumberingObject(int numberingId)
        {
            return new
            {
                Numbering_ID = numberingId,
                Document_Type = GetDocumentTypeCode(),
                Prefix = txtPrefix.Text.Trim().ToUpper(),
                Digits_Count = (int)numDigitsCount.Value,
                Reset_Type = GetResetTypeCode(),
                Use_Company = chkUseCompany.Checked,
                Use_Branch = chkUseBranch.Checked,
                Use_Year = chkUseYear.Checked,
                Is_Active = chkIsActive.Checked
            };
        }

        // ======================================================
        // تنظيف الحقول
        // ======================================================
        private void ClearForm()
        {
            _selectedNumberingId = 0;

            cmbDocumentType.SelectedIndex = -1;
            txtPrefix.Clear();
            numDigitsCount.Value = 4;
            cmbResetType.SelectedIndex = -1;
            numLastNumber.Value = 0;

            chkUseCompany.Checked = false;
            chkUseBranch.Checked = false;
            chkUseYear.Checked = false;
            chkIsActive.Checked = true;

            dgvNumberingSettings.ClearSelection();
        }

        // ======================================================
        // حدث فارغ أنشأه المصمم
        // ======================================================
        private void grpNumbering_Enter(object sender, EventArgs e)
        {
        }
    }

    // ======================================================
    // كلاس استقبال بيانات إعدادات الترقيم من API
    // ======================================================
    public class NumberingSettingModel
    {
        public int Numbering_ID { get; set; }
        public string Document_Type { get; set; } = string.Empty;
        public string Prefix { get; set; } = string.Empty;
        public int Digits_Count { get; set; }
        public string Reset_Type { get; set; } = string.Empty;
        public int Last_Number { get; set; }
        public bool Use_Company { get; set; }
        public bool Use_Branch { get; set; }
        public bool Use_Year { get; set; }
        public bool Is_Active { get; set; }
    }
}