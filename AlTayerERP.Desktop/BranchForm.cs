using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlTayerERP.Desktop.Services;

namespace AlTayerERP.Desktop
{
    public partial class BranchForm : Form
    {
        private readonly HttpClient _client = ApiService.Client;
        private readonly string _baseUrl = ApiService.BaseUrl;

        private int _selectedBranchId = 0;
        private List<BranchListModel> _branchesList = new List<BranchListModel>();

        // كائنات نظام الطباعة والمعاينة
        private System.Drawing.Printing.PrintDocument printDocument = new System.Drawing.Printing.PrintDocument();
        private PrintPreviewDialog printPreviewDialog = new PrintPreviewDialog();
        private int _printRowIndex = 0;

        public BranchForm()
        {
            InitializeComponent();

            // توحيد شكل الشاشة القديمة والاختصارات العربية دون تغيير منطقها.
this.Load -= BranchForm_Load;
            this.Load += BranchForm_Load;
            printDocument.PrintPage += PrintDocument_PrintPage;
        }

        private async void BranchForm_Load(object sender, EventArgs e)
        {
            SetupBranchesGrid();
            FillBranchTypes();
            InitializeStatusComboBox();
            await LoadCompaniesAsync();
            await LoadBranchesAsync();
        }

        private void InitializeStatusComboBox()
        {
            cmbStatus.Items.Clear();
            cmbStatus.Items.Add("نشط");
            cmbStatus.Items.Add("موقوف");
            // [تصحيح] التعيين عبر SelectedItem لضمان عدم رجوع الكومبو بوكس بـ Null
            cmbStatus.SelectedItem = "نشط";
        }

        private void FillBranchTypes()
        {
            cmbBranchType.Items.Clear();
            cmbBranchType.Items.AddRange(new string[] { "رئيسي", "فرعي", "نقطة توزيع", "مستودع" });
            cmbBranchType.SelectedIndex = -1;
        }

        private async Task LoadCompaniesAsync()
        {
            try
            {
                var companies = await _client.GetFromJsonAsync<List<CompanyLookupModel>>($"{_baseUrl}Branches/GetCompaniesLookup");
                cmbCompanies.DataSource = companies;
                cmbCompanies.DisplayMember = "Company_Name_AR";
                cmbCompanies.ValueMember = "Company_ID";

                cmbCompanies.SelectedValue = CurrentSession.Company_ID;
            }
            catch (Exception ex)
            {
                MessageBox.Show("فشل تحميل الشركات:\n" + ex.Message);
            }
        }

        private async Task LoadBranchesAsync()
        {
            try
            {
                var branches = await _client.GetFromJsonAsync<List<BranchListModel>>($"{_baseUrl}Branches?companyId={CurrentSession.Company_ID}");
                _branchesList = branches ?? new List<BranchListModel>();
                FillBranchesGrid(_branchesList);
                PopulateParentBranchComboBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show("فشل تحميل الفروع:\n" + ex.Message);
            }
        }

        private void PopulateParentBranchComboBox()
        {
            var parentBranches = _branchesList.Where(b => b.Branch_Type == "رئيسي").ToList();
            cmbParentBranch.DataSource = null;
            cmbParentBranch.DataSource = parentBranches;
            cmbParentBranch.DisplayMember = "Branch_Name";
            cmbParentBranch.ValueMember = "Branch_ID";
            cmbParentBranch.SelectedIndex = -1;
        }

        private void SetupBranchesGrid()
        {
            dgvBranches.Columns.Clear();
            dgvBranches.AllowUserToAddRows = false;
            dgvBranches.ReadOnly = true;
            dgvBranches.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBranches.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvBranches.Columns.Add("Branch_ID", "رقم الفرع");
            dgvBranches.Columns.Add("Branch_Code", "كود الفرع");
            dgvBranches.Columns.Add("Branch_Name", "اسم الفرع");
            dgvBranches.Columns.Add("Branch_Name_EN", "الاسم الإنجليزي");
            dgvBranches.Columns.Add("Branch_Type", "نوع الفرع");
            dgvBranches.Columns.Add("Address", "العنوان/الموقع");
            dgvBranches.Columns.Add("Manager_Name", "المسؤول");
            dgvBranches.Columns.Add("Phone", "الهاتف");
            dgvBranches.Columns.Add("Mobile", "الجوال");
            dgvBranches.Columns.Add("Email", "البريد");
            dgvBranches.Columns.Add("Website", "الموقع");
            dgvBranches.Columns.Add("Notes", "ملاحظات");
            dgvBranches.Columns.Add("Is_Active", "الحالة");
        }

        private void FillBranchesGrid(List<BranchListModel> branches)
        {
            dgvBranches.Rows.Clear();
            foreach (var b in branches)
            {
                dgvBranches.Rows.Add(
                    b.Branch_ID,
                    b.Branch_Code,
                    b.Branch_Name,
                    b.Branch_Name_EN,
                    b.Branch_Type,
                    b.Address,
                    b.Manager_Name,
                    b.Phone,
                    b.Mobile,
                    b.Email,
                    b.Website,
                    b.Notes,
                    b.Is_Active ? "نشط" : "موقوف"
                );
            }
        }

        private void ClearFormControls()
        {
            _selectedBranchId = 0;

            txtBranchCode.Clear();
            txtBranchNameAr.Clear();
            txtBranchNameEn.Clear();
            txtLocation.Clear();
            txtPhone.Clear();
            txtMobile.Clear();
            txtEmail.Clear();
            txtWebsite.Clear();
            txtNotes.Clear();

            cmbCompanies.SelectedValue = CurrentSession.Company_ID;
            cmbBranchType.SelectedIndex = -1;
            cmbParentBranch.SelectedIndex = -1;
            cmbManager.SelectedIndex = -1;

            cmbStatus.SelectedItem = "نشط";
            chkAllowCredit.Checked = false;
            chkAllowPercentage.Checked = false;

            txtBranchNameAr.Focus();
        }

        private async void btnSaveBranch_Click(object sender, EventArgs e)
        {
            if (!ValidateForm()) return;

            var branchData = BuildBranchRequest();
            try
            {
                var response = await _client.PostAsJsonAsync($"{_baseUrl}Branches", branchData);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("تم حفظ الفرع بنجاح.");
                    ClearFormControls();
                    await LoadBranchesAsync();
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show("فشل حفظ الفرع:\n" + error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ في الاتصال:\n" + ex.Message);
            }
        }

        private bool ValidateForm()
        {
            if (cmbCompanies.SelectedValue == null)
            {
                MessageBox.Show("يرجى اختيار الشركة التابعة.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtBranchNameAr.Text))
            {
                MessageBox.Show("يرجى إدخال اسم الفرع بالعربي.");
                return false;
            }

            return true;
        }

        private object BuildBranchRequest()
        {
            return new
            {
                Company_ID = cmbCompanies.SelectedValue?.ToString() ?? string.Empty,
                Branch_Code = txtBranchCode.Text.Trim(),
                Branch_Name = txtBranchNameAr.Text.Trim(),
                Branch_Name_EN = txtBranchNameEn.Text.Trim(),
                Address = txtLocation.Text.Trim(),
                Branch_Type = cmbBranchType.Text.Trim(),
                Parent_Branch_ID = cmbParentBranch.SelectedValue != null ? (int?)Convert.ToInt32(cmbParentBranch.SelectedValue) : null,
                Manager_Name = cmbManager.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                Mobile = txtMobile.Text.Trim(),
                Website = txtWebsite.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Notes = txtNotes.Text.Trim(),
                Allow_Credit = chkAllowCredit.Checked,
                Allow_Percentage = chkAllowPercentage.Checked,

                // [تصحيح الثغرة لحماية الحفظ]: فحص مزدوج قوي يمنع خطأ الـ Null النصي
                Is_Active = cmbStatus.SelectedItem?.ToString() == "نشط" || cmbStatus.Text.Trim() == "نشط",

                Currency_ID = 1
            };
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            ClearFormControls();
        }

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                await LoadCompaniesAsync();
                await LoadBranchesAsync();
                ClearFormControls();
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء تحديث البيانات:\n" + ex.Message);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string keyword = Microsoft.VisualBasic.Interaction.InputBox(
                "أدخل اسم الفرع أو كود الفرع للبحث:",
                "بحث الفروع",
                "");

            if (string.IsNullOrWhiteSpace(keyword))
            {
                FillBranchesGrid(_branchesList);
                return;
            }

            var result = _branchesList.Where(b =>
                (b.Branch_Name ?? "").Contains(keyword) ||
                (b.Branch_Code ?? "").Contains(keyword) ||
                (b.Branch_Name_EN ?? "").Contains(keyword)
            ).ToList();

            FillBranchesGrid(result);
        }

        private async void btnEdit_Click(object sender, EventArgs e)
        {
            if (_selectedBranchId <= 0)
            {
                MessageBox.Show("يرجى اختيار فرع من الجدول أولاً.");
                return;
            }

            var branchData = BuildBranchRequest();
            try
            {
                var response = await _client.PutAsJsonAsync($"{_baseUrl}Branches/{_selectedBranchId}", branchData);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("تم تعديل بيانات الفرع بنجاح.");
                    await LoadBranchesAsync();
                    UpdateFormWithSelectedBranch(); // إعادة تعبئة الحقول والاحتفاظ بالسجل الحالي دون وهم الحذف
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show("فشل التعديل:\n" + error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء التعديل:\n" + ex.Message);
            }
        }

        // [تعديل] تحديث الدالة لتعتمد كلياً على التخصيص الاحترافي SelectedItem للحالة ومنع سقوط حقل تابع لفرع
        private void UpdateFormWithSelectedBranch()
        {
            var branch = _branchesList.FirstOrDefault(x => x.Branch_ID == _selectedBranchId);
            if (branch != null)
            {
                txtBranchCode.Text = branch.Branch_Code ?? "";
                txtBranchNameAr.Text = branch.Branch_Name ?? "";
                txtBranchNameEn.Text = branch.Branch_Name_EN ?? "";
                txtLocation.Text = branch.Address ?? "";
                cmbBranchType.Text = branch.Branch_Type ?? "";

                // التثبيت بـ SelectedItem لحماية الحالة
                cmbStatus.SelectedItem = branch.Is_Active ? "نشط" : "موقوف";

                cmbManager.Text = branch.Manager_Name ?? "";
                txtPhone.Text = branch.Phone ?? "";
                txtMobile.Text = branch.Mobile ?? "";
                txtWebsite.Text = branch.Website ?? "";
                txtEmail.Text = branch.Email ?? "";
                txtNotes.Text = branch.Notes ?? "";

                if (branch.Parent_Branch_ID.HasValue)
                    cmbParentBranch.SelectedValue = branch.Parent_Branch_ID.Value;
                else
                    cmbParentBranch.SelectedIndex = -1;

                chkAllowCredit.Checked = branch.Allow_Credit;
                chkAllowPercentage.Checked = branch.Allow_Percentage;
            }
        }


        
        // ربط حدث النقر بالدالة المنظمة التي أنشأتها لإعادة التوزيع
        private void dgvBranches_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dgvBranches.Rows[e.RowIndex];
            _selectedBranchId = Convert.ToInt32(row.Cells["Branch_ID"].Value);

            UpdateFormWithSelectedBranch();
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedBranchId <= 0)
            {
                MessageBox.Show("يرجى اختيار فرع من الجدول أولاً.");
                return;
            }

            var confirm = MessageBox.Show(
                "هل أنت متأكد من حذف هذا الفرع؟",
                "تأكيد الحذف",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            try
            {
                var response = await _client.DeleteAsync($"{_baseUrl}Branches/{_selectedBranchId}");
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("تم حذف الفرع بنجاح.");
                    ClearFormControls();
                    await LoadBranchesAsync();
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show("فشل الحذف:\n" + error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء الحذف:\n" + ex.Message);
            }
        }

        // لوجستيات الاعتماد والموافقة الفورية
        private async Task ApproveOrUnapproveBranch(bool activate)
        {
            if (_selectedBranchId <= 0)
            {
                MessageBox.Show(activate ? "يرجى تحديد الفرع المراد اعتماده وتنشيطه من الجدول أولاً." : "يرجى تحديد الفرع المراد تجميده وإلغاء اعتماده من الجدول.");
                return;
            }

            try
            {
                cmbStatus.SelectedItem = activate ? "نشط" : "موقوف";
                var branchData = BuildBranchRequest();
                var response = await _client.PutAsJsonAsync($"{_baseUrl}Branches/{_selectedBranchId}", branchData);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show(activate ? "تم اعتماد وتنشيط الفرع المحدد بنجاح في النظام." : "تم تجميد وإلغاء اعتماد الفرع المحدد بنجاح.");
                    await LoadBranchesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء معالجة حالة الاعتماد: " + ex.Message);
            }
        }

        private async void btnApprove_Click(object sender, EventArgs e) => await ApproveOrUnapproveBranch(activate: true);
        private async void btnUnApprove_Click(object sender, EventArgs e) => await ApproveOrUnapproveBranch(activate: false);
        private void btnClose_Click(object sender, EventArgs e) => Close();

        // أزرار الطباعة والمعاينة والتصدير
        private void btnPrint_Click(object sender, EventArgs e)
        {
            _printRowIndex = 0;
            printDocument.Print();
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            _printRowIndex = 0;
            printPreviewDialog.Document = printDocument;
            printPreviewDialog.WindowState = FormWindowState.Maximized;
            printPreviewDialog.ShowDialog();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvBranches.Rows.Count == 0)
                {
                    MessageBox.Show("لا توجد بيانات متاحة بالجدول للتصدير.");
                    return;
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("رقم الفرع,كود الفرع,اسم الفرع,النوع,الهاتف,الحالة");

                foreach (var b in _branchesList)
                {
                    sb.AppendLine($"{b.Branch_ID},{b.Branch_Code},{b.Branch_Name},{b.Branch_Type},{b.Phone},{(b.Is_Active ? "نشط" : "موقوف")}");
                }

                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Branches_Export.csv");
                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

                MessageBox.Show($"تم تصدير البيانات بنجاح وحفظ الملف على سطح المكتب باسم:\nBranches_Export.csv", "تصدير البيانات", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("فشل تصدير البيانات: " + ex.Message);
            }
        }

        private void btnImport_Click(object sender, EventArgs e) => MessageBox.Show("يرجى تحديد ملف الإكسل (CSV) المعتمد لاستيراد الفروع دفعة واحدة.", "استيراد البيانات", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

        private void PrintDocument_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Font titleFont = new Font("Arial", 16, FontStyle.Bold);
            Font headerFont = new Font("Arial", 10, FontStyle.Bold);
            Font rowFont = new Font("Arial", 9);
            int y = 50;
            int x = 50;

            e.Graphics.DrawString("تقرير الفروع", titleFont, Brushes.Black, 350, y);
            y += 50;

            e.Graphics.DrawString("رقم", headerFont, Brushes.Black, x, y);
            e.Graphics.DrawString("كود الفرع", headerFont, Brushes.Black, x + 80, y);
            e.Graphics.DrawString("اسم الفرع", headerFont, Brushes.Black, x + 180, y);
            e.Graphics.DrawString("النوع", headerFont, Brushes.Black, x + 380, y);
            e.Graphics.DrawString("الهاتف", headerFont, Brushes.Black, x + 480, y);
            e.Graphics.DrawString("الحالة", headerFont, Brushes.Black, x + 600, y);
            y += 30;

            while (_printRowIndex < _branchesList.Count)
            {
                var b = _branchesList[_printRowIndex];
                if (y > e.MarginBounds.Bottom - 40)
                {
                    e.HasMorePages = true;
                    return;
                }

                e.Graphics.DrawString(b.Branch_ID.ToString(), rowFont, Brushes.Black, x, y);
                e.Graphics.DrawString(b.Branch_Code ?? "", rowFont, Brushes.Black, x + 80, y);
                e.Graphics.DrawString(b.Branch_Name ?? "", rowFont, Brushes.Black, x + 180, y);
                e.Graphics.DrawString(b.Branch_Type ?? "", rowFont, Brushes.Black, x + 380, y);
                e.Graphics.DrawString(b.Phone ?? "", rowFont, Brushes.Black, x + 480, y);
                e.Graphics.DrawString(b.Is_Active ? "نشط" : "موقوف", rowFont, Brushes.Black, x + 600, y);
                y += 25;
                _printRowIndex++;
            }
            e.HasMorePages = false;
        }

        private void label13_Click(object sender, EventArgs e) { }
        private void dgvBranches_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void chkIsStop_CheckedChanged(object sender, EventArgs e) { }
        private void chkAllowCredit_CheckedChanged(object sender, EventArgs e) { }
    }

    public class CompanyLookupModel
    {
        public string Company_ID { get; set; } = string.Empty;
        public string Company_Name_AR { get; set; } = string.Empty;
    }

    public class BranchListModel
    {
        public int Branch_ID { get; set; }
        public string Company_ID { get; set; } = string.Empty;
        public string? Branch_Code { get; set; }
        public string? Branch_Name { get; set; }
        public string? Branch_Name_EN { get; set; }
        public string? Address { get; set; }
        public string? Branch_Type { get; set; }
        public int? Parent_Branch_ID { get; set; }
        public string? Phone { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? Manager_Name { get; set; }
        public string? Notes { get; set; }
        public bool Allow_Credit { get; set; }
        public bool Allow_Percentage { get; set; }
        public bool Is_Active { get; set; }
        public int Currency_ID { get; set; }
    }
}