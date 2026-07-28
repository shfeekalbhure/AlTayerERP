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
using Microsoft.VisualBasic.FileIO;
using AlTayerERP.Desktop.Services;
using AlTayerERP.Desktop.Common;

namespace AlTayerERP.Desktop
{
    public partial class BranchForm : BaseForm
    {
        private readonly HttpClient _client = ApiService.Client;
        private readonly string _baseUrl = ApiService.BaseUrl;

        private int _selectedBranchId = 0;
        private List<BranchListModel> _branchesList = new List<BranchListModel>();
        private List<BranchTypeLookupModel> _branchTypes = new List<BranchTypeLookupModel>();
        private List<CityLookupModel> _cities = new List<CityLookupModel>();
        private int _defaultCurrencyId;
        // يمنع إعادة تحميل الجدول أثناء تعبئة قائمة الشركات عند فتح الشاشة.
        private bool _isLoadingCompanies;

        // كائنات نظام الطباعة والمعاينة
        private System.Drawing.Printing.PrintDocument printDocument = new System.Drawing.Printing.PrintDocument();
        private PrintPreviewDialog printPreviewDialog = new PrintPreviewDialog();
        private int _printRowIndex = 0;

        public BranchForm()
        {
            InitializeComponent();
            // يرث القالب المرئي الموحد من BaseForm دون نقل قواعد الحفظ أو التدقيق إلى الواجهة.
            ApplyBaseFormStyle();

            // توحيد شكل الشاشة القديمة والاختصارات العربية دون تغيير منطقها.
            this.Load -= BranchForm_Load;
            this.Load += BranchForm_Load;
            cmbCompanies.SelectedValueChanged += cmbCompanies_SelectedValueChanged;
            cmbCity.SelectedValueChanged += cmbCity_SelectedValueChanged;
            printDocument.PrintPage += PrintDocument_PrintPage;
        }

        private async void BranchForm_Load(object sender, EventArgs e)
        {
            SetupBranchesGrid();
            ClearBranchTypes();
            InitializeStatusComboBox();
            await LoadCompaniesAsync();
            await LoadCitiesAsync();
            await LoadBranchesAsync();
        }

        private void InitializeStatusComboBox()
        {
            cmbStatus.Items.Clear();
            cmbStatus.Items.Add("نشط");
            cmbStatus.Items.Add("موقوف");
            // الحالة للعرض فقط؛ تغييرها يمر حصراً عبر عمليتي الإيقاف وإعادة التفعيل المدققتين.
            cmbStatus.Enabled = false;
            cmbStatus.TabStop = false;
            // [تصحيح] التعيين عبر SelectedItem لضمان عدم رجوع الكومبو بوكس بـ Null
            cmbStatus.SelectedItem = "نشط";
        }

        private void ClearBranchTypes()
        {
            cmbBranchType.DataSource = null;
            cmbBranchType.Items.Clear();
            cmbBranchType.SelectedIndex = -1;
        }

        /// <summary>تحميل كل قائمة مستقلة كي تظهر للمستخدم رسالة تخص المنسدلة التي تعذر تحميلها فقط.</summary>
        private async Task LoadBranchReferenceDataAsync(string? companyId)
        {
            ClearBranchTypes();
            _defaultCurrencyId = 0;
            if (string.IsNullOrWhiteSpace(companyId)) return;

            await LoadBranchTypesAsync();
            await LoadCompanyCurrenciesAsync(companyId);
        }

        /// <summary>تحميل قائمة أنواع الفروع فقط.</summary>
        private async Task LoadBranchTypesAsync()
        {
            try
            {
                _branchTypes = await _client.GetFromJsonAsync<List<BranchTypeLookupModel>>(
                    $"{_baseUrl}branch-reference-lookups/branch-types") ?? new List<BranchTypeLookupModel>();
                cmbBranchType.DataSource = _branchTypes;
                cmbBranchType.DisplayMember = nameof(BranchTypeLookupModel.Branch_Type_Name_AR);
                cmbBranchType.ValueMember = nameof(BranchTypeLookupModel.Branch_Type_Code);
                cmbBranchType.SelectedIndex = -1;
                if (_branchTypes.Count == 0)
                    MessageBox.Show("لا توجد أنواع فروع نشطة. أضف نوع فرع أو فعّله ثم أعد المحاولة.", "قائمة أنواع الفروع", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                MessageBox.Show("تعذر تحميل قائمة أنواع الفروع لأن خدمة أنواع الفروع غير موجودة في API المشغّل. حدّث API ثم أعد تشغيله.", "قائمة أنواع الفروع", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر تحميل قائمة أنواع الفروع:\n" + ex.Message, "قائمة أنواع الفروع", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>تحميل عملات الشركة المختارة فقط، واختيار العملة الافتراضية تلقائياً.</summary>
        private async Task LoadCompanyCurrenciesAsync(string companyId)
        {
            try
            {
                var currencies = await _client.GetFromJsonAsync<List<CurrencyLookupModel>>(
                    $"{_baseUrl}branch-reference-lookups/currencies?companyId={Uri.EscapeDataString(companyId)}") ?? new List<CurrencyLookupModel>();
                var currency = currencies.FirstOrDefault(x => x.Is_Default)
                    ?? currencies.FirstOrDefault(x => x.Is_Local_Currency)
                    ?? currencies.FirstOrDefault();
                _defaultCurrencyId = currency?.Currency_ID ?? 0;
                if (_defaultCurrencyId <= 0)
                    MessageBox.Show("لا توجد عملة نشطة للشركة المختارة. أضف عملة افتراضية للشركة ثم أعد المحاولة.", "قائمة عملات الشركة", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                MessageBox.Show("تعذر تحميل قائمة عملات الشركة لأن الشركة أو خدمة العملات غير موجودة في API المشغّل.", "قائمة عملات الشركة", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر تحميل قائمة عملات الشركة:\n" + ex.Message, "قائمة عملات الشركة", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task LoadCompaniesAsync()
        {
            try
            {
                _isLoadingCompanies = true;
                var companies = await _client.GetFromJsonAsync<List<CompanyLookupModel>>($"{_baseUrl}Branches/GetCompaniesLookup");
                cmbCompanies.DataSource = companies;
                cmbCompanies.DisplayMember = "Company_Name_AR";
                cmbCompanies.ValueMember = "Company_ID";

                // سياق الجلسة هو الاختيار الابتدائي فقط؛ مدير النظام يستطيع اختيار شركة أخرى من القائمة.
                cmbCompanies.SelectedValue = CurrentSession.Company_ID;
                await LoadBranchReferenceDataAsync(cmbCompanies.SelectedValue?.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر تحميل قائمة الشركات:\n" + ex.Message, "قائمة الشركات", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                _isLoadingCompanies = false;
            }
        }

        private async Task LoadBranchesAsync(string? companyId = null)
        {
            try
            {
                // لا يعتمد تحميل الفروع على قيمة قديمة من CurrentSession بعد تغيير الشركة في الواجهة.
                var selectedCompanyId = companyId ?? cmbCompanies.SelectedValue?.ToString() ?? CurrentSession.Company_ID;
                if (string.IsNullOrWhiteSpace(selectedCompanyId))
                {
                    _branchesList = new List<BranchListModel>();
                    FillBranchesGrid(_branchesList);
                    return;
                }

                var branches = await _client.GetFromJsonAsync<List<BranchListModel>>($"{_baseUrl}Branches?companyId={Uri.EscapeDataString(selectedCompanyId)}");
                _branchesList = branches ?? new List<BranchListModel>();
                FillBranchesGrid(_branchesList);
                PopulateParentBranchComboBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show("فشل تحميل الفروع:\n" + ex.Message);
            }
        }

        /// <summary>تحميل المدن النشطة من جدول المدن لتكون مرجع موقع الفرع.</summary>
        private async Task LoadCitiesAsync()
        {
            try
            {
                _cities = await _client.GetFromJsonAsync<List<CityLookupModel>>(
                    $"{_baseUrl}GeographicReferences/cities?activeOnly=true") ?? new List<CityLookupModel>();
                cmbCity.DataSource = _cities;
                cmbCity.DisplayMember = nameof(CityLookupModel.City_Name_AR);
                cmbCity.ValueMember = nameof(CityLookupModel.City_ID);
                cmbCity.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر تحميل المدن من جدول المدن:\n" + ex.Message, "الفروع", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>يحمّل فروع الشركة المحددة فقط، ولا يغيّر سياق الجلسة أو صلاحياتها.</summary>
        private async void cmbCompanies_SelectedValueChanged(object? sender, EventArgs e)
        {
            if (_isLoadingCompanies || cmbCompanies.SelectedValue is null)
                return;

            var companyId = cmbCompanies.SelectedValue.ToString();
            await LoadBranchReferenceDataAsync(companyId);
            await LoadBranchesAsync(companyId);
            _selectedBranchId = 0;
        }

        private void PopulateParentBranchComboBox()
        {
            var parentBranches = _branchesList
                .Where(b => b.Is_Active && b.Branch_ID != _selectedBranchId && IsEligibleParentBranchType(b.Branch_Type))
                .ToList();
            cmbParentBranch.DataSource = null;
            cmbParentBranch.DataSource = parentBranches;
            cmbParentBranch.DisplayMember = "Branch_Name";
            cmbParentBranch.ValueMember = "Branch_ID";
            cmbParentBranch.SelectedIndex = -1;
        }

        /// <summary>الفرع الأب لا يكون إلا فرعاً رئيسياً أو فرعاً ضمن الشركة نفسها.</summary>
        private static bool IsEligibleParentBranchType(string? branchType)
        {
            var normalized = (branchType ?? string.Empty).Trim();
            return normalized.Equals("فرع رئيسي", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("فرع", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("MAIN", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("BRANCH", StringComparison.OrdinalIgnoreCase);
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
            cmbCity.SelectedIndex = -1;
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

            if (cmbBranchType.SelectedValue is null)
            {
                MessageBox.Show("يرجى اختيار نوع الفرع.");
                return false;
            }

            if (cmbCity.SelectedValue is null)
            {
                MessageBox.Show("يرجى اختيار المدينة من قائمة المدن.");
                return false;
            }

            if (cmbParentBranch.SelectedValue is not null &&
                Convert.ToInt32(cmbParentBranch.SelectedValue) == _selectedBranchId)
            {
                MessageBox.Show("لا يجوز اختيار الفرع نفسه كفرع أب.");
                return false;
            }

            if (_defaultCurrencyId <= 0)
            {
                MessageBox.Show("لا توجد عملة نشطة للشركة المختارة. أضف عملة افتراضية ثم أعد المحاولة.");
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
                City_ID = cmbCity.SelectedValue != null ? Convert.ToInt32(cmbCity.SelectedValue) : (int?)null,
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

                Currency_ID = _defaultCurrencyId
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
                    await UpdateFormWithSelectedBranchAsync();
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

        /// <summary>يجلب الموقع الجغرافي الذي لا يظهر في قائمة الجدول المختصرة.</summary>
        private async Task LoadBranchGeographyAsync(int branchId)
        {
            try
            {
                var geography = await _client.GetFromJsonAsync<BranchGeographyModel>($"{_baseUrl}branch-geography/{branchId}");
                cmbCity.SelectedValue = geography?.City_ID ?? -1;
                if (cmbCity.SelectedIndex < 0) cmbCity.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                cmbCity.SelectedIndex = -1;
                MessageBox.Show("تعذر تحميل مدينة الفرع:\n" + ex.Message, "الفروع", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>عند اختيار المدينة تحفظ الدولة والمحافظة التابعة لها تلقائياً في الخادم.</summary>
        private void cmbCity_SelectedValueChanged(object? sender, EventArgs e)
        {
            if (cmbCity.SelectedValue is not int cityId) return;
            var city = _cities.FirstOrDefault(x => x.City_ID == cityId);
            if (city != null && string.IsNullOrWhiteSpace(txtLocation.Text))
                txtLocation.Text = city.City_Name_AR;
        }

        // تعبئة جميع الحقول عند النقر على صف، بما فيها القوائم المنسدلة والموقع.
        private async Task UpdateFormWithSelectedBranchAsync()
        {
            var branch = _branchesList.FirstOrDefault(x => x.Branch_ID == _selectedBranchId);
            if (branch != null)
            {
                txtBranchCode.Text = branch.Branch_Code ?? "";
                txtBranchNameAr.Text = branch.Branch_Name ?? "";
                txtBranchNameEn.Text = branch.Branch_Name_EN ?? "";
                txtLocation.Text = branch.Address ?? "";
                var branchType = _branchTypes.FirstOrDefault(x =>
                    string.Equals(x.Branch_Type_Name_AR, branch.Branch_Type, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(x.Branch_Type_Code, branch.Branch_Type, StringComparison.OrdinalIgnoreCase));
                cmbBranchType.SelectedValue = branchType?.Branch_Type_Code;
                if (cmbBranchType.SelectedIndex < 0) cmbBranchType.Text = branch.Branch_Type ?? "";

                // التثبيت بـ SelectedItem لحماية الحالة
                cmbStatus.SelectedItem = branch.Is_Active ? "نشط" : "موقوف";

                cmbManager.Text = branch.Manager_Name ?? "";
                txtPhone.Text = branch.Phone ?? "";
                txtMobile.Text = branch.Mobile ?? "";
                txtWebsite.Text = branch.Website ?? "";
                txtEmail.Text = branch.Email ?? "";
                txtNotes.Text = branch.Notes ?? "";

                PopulateParentBranchComboBox();
                if (branch.Parent_Branch_ID.HasValue)
                    cmbParentBranch.SelectedValue = branch.Parent_Branch_ID.Value;
                else
                    cmbParentBranch.SelectedIndex = -1;

                chkAllowCredit.Checked = branch.Allow_Credit;
                chkAllowPercentage.Checked = branch.Allow_Percentage;
                await LoadBranchGeographyAsync(branch.Branch_ID);
            }
        }


        
        // ربط حدث النقر بالدالة المنظمة التي أنشأتها لإعادة التوزيع
        private async void dgvBranches_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dgvBranches.Rows[e.RowIndex];
            _selectedBranchId = Convert.ToInt32(row.Cells["Branch_ID"].Value);

            await UpdateFormWithSelectedBranchAsync();
        }

        /// <summary>
        /// إيقاف الفرع بدلاً من حذفه فعلياً. يرسل السبب فقط، بينما يملأ الخادم
        /// المستخدم والتاريخ وسجل التدقيق من الجلسة الموثوقة.
        /// </summary>
        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedBranchId <= 0)
            {
                MessageBox.Show("يرجى اختيار فرع من الجدول أولاً.");
                return;
            }

            var reason = Microsoft.VisualBasic.Interaction.InputBox(
                "أدخل سبب إيقاف الفرع (حقل إلزامي للتدقيق):",
                "إيقاف الفرع",
                string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(reason))
            {
                MessageBox.Show("لا يمكن إيقاف الفرع دون سبب.");
                return;
            }

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Delete, $"{_baseUrl}Branches/{_selectedBranchId}")
                {
                    Content = JsonContent.Create(new { Reason = reason })
                };
                var response = await _client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("تم إيقاف الفرع دون حذف تاريخه.");
                    ClearFormControls();
                    await LoadBranchesAsync();
                }
                else
                {
                    MessageBox.Show("فشل إيقاف الفرع:\n" + await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء إيقاف الفرع:\n" + ex.Message);
            }
        }

        // إعادة التفعيل والإيقاف عمليتان صريحتان، وليستا تعديل Is_Active داخل PUT العام.
        private async Task ApproveOrUnapproveBranch(bool activate)
        {
            if (_selectedBranchId <= 0)
            {
                MessageBox.Show("يرجى تحديد فرع من الجدول أولاً.");
                return;
            }

            var actionName = activate ? "إعادة تفعيل" : "إيقاف";
            var reason = Microsoft.VisualBasic.Interaction.InputBox(
                $"أدخل سبب {actionName} الفرع (حقل إلزامي للتدقيق):",
                actionName + " الفرع",
                string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(reason))
            {
                MessageBox.Show($"لا يمكن تنفيذ {actionName} دون سبب.");
                return;
            }

            try
            {
                HttpResponseMessage response;
                if (activate)
                {
                    response = await _client.PostAsJsonAsync(
                        $"{_baseUrl}Branches/{_selectedBranchId}/reactivate",
                        new { Reason = reason });
                }
                else
                {
                    using var request = new HttpRequestMessage(HttpMethod.Delete, $"{_baseUrl}Branches/{_selectedBranchId}")
                    {
                        Content = JsonContent.Create(new { Reason = reason })
                    };
                    response = await _client.SendAsync(request);
                }

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"تم {actionName} الفرع بنجاح.");
                    await LoadBranchesAsync();
                    await UpdateFormWithSelectedBranchAsync();
                }
                else
                {
                    MessageBox.Show($"فشل {actionName} الفرع:\n" + await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ أثناء {actionName} الفرع: " + ex.Message);
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

        /// <summary>
        /// استيراد فروع من ملف CSV. يجب أن تكون الأعمدة بالترتيب:
        /// Branch_Code,Branch_Name,Branch_Name_EN,Branch_Type,Parent_Branch_Code,City_ID,
        /// Address,Manager_Name,Phone,Mobile,Email,Website,Notes,Allow_Credit,Allow_Percentage.
        /// لا يمر أي صف إلا من خلال API ليطبق التحقق والتدقيق الهرمي نفسه المستخدم في الحفظ اليدوي.
        /// </summary>
        private async void btnImport_Click(object sender, EventArgs e)
        {
            if (cmbCompanies.SelectedValue is null || _defaultCurrencyId <= 0)
            {
                MessageBox.Show("اختر الشركة وانتظر تحميل عملتها قبل الاستيراد.", "استيراد الفروع", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dialog = new OpenFileDialog
            {
                Title = "اختيار ملف فروع CSV",
                Filter = "ملف CSV (*.csv)|*.csv",
                Multiselect = false
            };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            List<BranchImportRow> rows;
            try
            {
                rows = ReadBranchImportFile(dialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر قراءة ملف الاستيراد:\n" + ex.Message, "استيراد الفروع", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (rows.Count == 0)
            {
                MessageBox.Show("الملف لا يحتوي على سجلات صالحة للاستيراد.", "استيراد الفروع", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var errors = new List<string>();
            var importedCodes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var existingCodes = _branchesList
                .Where(x => !string.IsNullOrWhiteSpace(x.Branch_Code))
                .ToDictionary(x => x.Branch_Code!, x => x.Branch_ID, StringComparer.OrdinalIgnoreCase);

            foreach (var row in rows)
            {
                var validation = ValidateImportRow(row, existingCodes, importedCodes);
                if (validation is not null)
                {
                    errors.Add($"السطر {row.RowNumber}: {validation}");
                    continue;
                }

                int? parentId = null;
                if (!string.IsNullOrWhiteSpace(row.ParentBranchCode))
                {
                    if (!importedCodes.TryGetValue(row.ParentBranchCode, out var importedParentId) &&
                        !existingCodes.TryGetValue(row.ParentBranchCode, out importedParentId))
                    {
                        errors.Add($"السطر {row.RowNumber}: كود الفرع الأب غير موجود. يجب أن يكون موجوداً أو وارداً في سطر سابق.");
                        continue;
                    }
                    parentId = importedParentId;
                }

                var request = new
                {
                    Company_ID = cmbCompanies.SelectedValue.ToString() ?? string.Empty,
                    Branch_Code = row.BranchCode,
                    Branch_Name = row.BranchName,
                    Branch_Name_EN = row.BranchNameEn,
                    Branch_Type = row.BranchType,
                    Parent_Branch_ID = parentId,
                    City_ID = row.CityId,
                    Address = row.Address,
                    Manager_Name = row.ManagerName,
                    Phone = row.Phone,
                    Mobile = row.Mobile,
                    Email = row.Email,
                    Website = row.Website,
                    Notes = row.Notes,
                    Allow_Credit = row.AllowCredit,
                    Allow_Percentage = row.AllowPercentage,
                    Currency_ID = _defaultCurrencyId
                };

                try
                {
                    using var response = await _client.PostAsJsonAsync($"{_baseUrl}Branches", request);
                    if (!response.IsSuccessStatusCode)
                    {
                        errors.Add($"السطر {row.RowNumber}: {await response.Content.ReadAsStringAsync()}");
                        continue;
                    }

                    var saved = await response.Content.ReadFromJsonAsync<BranchListModel>();
                    if (saved?.Branch_ID > 0 && !string.IsNullOrWhiteSpace(row.BranchCode))
                        importedCodes[row.BranchCode] = saved.Branch_ID;
                }
                catch (Exception ex)
                {
                    errors.Add($"السطر {row.RowNumber}: {ex.Message}");
                }
            }

            await LoadBranchesAsync();
            var successCount = rows.Count - errors.Count;
            var message = $"تم استيراد {successCount} من {rows.Count} سجل.";
            if (errors.Count > 0)
                message += "\n\nالأخطاء:\n" + string.Join("\n", errors.Take(12)) + (errors.Count > 12 ? "\n..." : string.Empty);
            MessageBox.Show(message, "استيراد الفروع", MessageBoxButtons.OK,
                errors.Count == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        private List<BranchImportRow> ReadBranchImportFile(string filePath)
        {
            var rows = new List<BranchImportRow>();
            using var parser = new TextFieldParser(filePath, Encoding.UTF8)
            {
                TextFieldType = FieldType.Delimited,
                Delimiters = new[] { "," },
                HasFieldsEnclosedInQuotes = true,
                TrimWhiteSpace = true
            };
            if (!parser.EndOfData) parser.ReadFields(); // رأس الملف.

            var rowNumber = 1;
            while (!parser.EndOfData)
            {
                rowNumber++;
                var fields = parser.ReadFields() ?? Array.Empty<string>();
                if (fields.All(string.IsNullOrWhiteSpace)) continue;
                if (fields.Length < 6) throw new InvalidOperationException($"السطر {rowNumber} لا يحتوي على الأعمدة الستة الإلزامية.");

                rows.Add(new BranchImportRow
                {
                    RowNumber = rowNumber,
                    BranchCode = ReadField(fields, 0),
                    BranchName = ReadField(fields, 1),
                    BranchNameEn = ReadField(fields, 2),
                    BranchType = ReadField(fields, 3),
                    ParentBranchCode = ReadField(fields, 4),
                    CityId = int.TryParse(ReadField(fields, 5), out var cityId) ? cityId : 0,
                    Address = ReadField(fields, 6),
                    ManagerName = ReadField(fields, 7),
                    Phone = ReadField(fields, 8),
                    Mobile = ReadField(fields, 9),
                    Email = ReadField(fields, 10),
                    Website = ReadField(fields, 11),
                    Notes = ReadField(fields, 12),
                    AllowCredit = ParseImportBoolean(ReadField(fields, 13)),
                    AllowPercentage = ParseImportBoolean(ReadField(fields, 14))
                });
            }
            return rows;
        }

        private string? ValidateImportRow(BranchImportRow row, IReadOnlyDictionary<string, int> existingCodes, IReadOnlyDictionary<string, int> importedCodes)
        {
            if (string.IsNullOrWhiteSpace(row.BranchName)) return "اسم الفرع بالعربية مطلوب.";
            if (row.CityId <= 0 || !_cities.Any(x => x.City_ID == row.CityId)) return "City_ID غير صحيح أو المدينة موقوفة.";
            if (string.IsNullOrWhiteSpace(row.BranchType) || !_branchTypes.Any(x =>
                string.Equals(x.Branch_Type_Name_AR, row.BranchType, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.Branch_Type_Code, row.BranchType, StringComparison.OrdinalIgnoreCase)))
                return "نوع الفرع غير صحيح.";
            if (!string.IsNullOrWhiteSpace(row.BranchCode) && (existingCodes.ContainsKey(row.BranchCode) || importedCodes.ContainsKey(row.BranchCode)))
                return "كود الفرع مكرر داخل الشركة أو داخل ملف الاستيراد.";
            return null;
        }

        private static string ReadField(IReadOnlyList<string> fields, int index) => index < fields.Count ? fields[index].Trim() : string.Empty;
        private static bool ParseImportBoolean(string value) => value.Equals("1") || value.Equals("true", StringComparison.OrdinalIgnoreCase) || value.Equals("نعم");

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

    internal sealed class BranchReferenceLookupResponse
    {
        public List<BranchTypeLookupModel> BranchTypes { get; set; } = new List<BranchTypeLookupModel>();
        public List<CurrencyLookupModel> Currencies { get; set; } = new List<CurrencyLookupModel>();
    }

    internal sealed class BranchTypeLookupModel
    {
        public int Branch_Type_ID { get; set; }
        public string Branch_Type_Code { get; set; } = string.Empty;
        public string Branch_Type_Name_AR { get; set; } = string.Empty;
    }

    internal sealed class CurrencyLookupModel
    {
        public int Currency_ID { get; set; }
        public bool Is_Local_Currency { get; set; }
        public bool Is_Default { get; set; }
    }

    internal sealed class CityLookupModel
    {
        public int City_ID { get; set; }
        public int Country_ID { get; set; }
        public int Governorate_ID { get; set; }
        public string City_Name_AR { get; set; } = string.Empty;
    }

    internal sealed class BranchGeographyModel
    {
        public int Branch_ID { get; set; }
        public int? Country_ID { get; set; }
        public int? Governorate_ID { get; set; }
        public int? City_ID { get; set; }
    }

    internal sealed class BranchImportRow
    {
        public int RowNumber { get; set; }
        public string BranchCode { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string BranchNameEn { get; set; } = string.Empty;
        public string BranchType { get; set; } = string.Empty;
        public string ParentBranchCode { get; set; } = string.Empty;
        public int CityId { get; set; }
        public string Address { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public bool AllowCredit { get; set; }
        public bool AllowPercentage { get; set; }
    }
}
