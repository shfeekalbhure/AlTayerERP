
using AlTayerERP.Desktop.Models;
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
    public partial class FrmCashBoxes : Form
    {
        private readonly HttpClient _client = ApiService.Client;
        private readonly string _baseUrl = ApiService.BaseUrl;

        private string _selectedCashBoxId = "";
        private List<CashBoxModel> _cashBoxesCache = new();
        private bool _isBinding = false;

        public FrmCashBoxes()
        {
            InitializeComponent();

            // تطبيق الثيم العربي الموحد والاختصارات على الشاشة القديمة.
            ArabicErpFormStyle.Apply(this);

            // ربط الأحداث (Events) 
            Load += FrmCashBoxes_Load;

            btnNew.Click += btnNew_Click;
            btnSave.Click += btnSave_Click;
            btnEdit.Click += btnEdit_Click;
            btnDelete.Click += btnDelete_Click;
            btnSearch.Click += btnSearch_Click;
            btnRefresh.Click += btnRefresh_Click;
            btnClose.Click += btnClose_Click;

            dgvCashBoxes.CellClick += dgvCashBoxes_CellClick;

            cmbAccount.SelectedIndexChanged += cmbAccount_SelectedIndexChanged;

        }

        //================================================
        // نماذج الـ Lookup الخاصة بالمنسدلات (Strongly-typed Models)
        //================================================
        public class BranchCashLookup
        {
            public int Branch_ID { get; set; }
            public string Branch_Name { get; set; } = "";
        }

        public class CurrencyCashLookup
        {
            public string Currency_Code { get; set; } = "";
            public string Currency_Name_AR { get; set; } = "";
        }

        public class AccountCashLookup
        {
            public string Account_ID { get; set; } = "";
            public string Account_Name_AR { get; set; } = "";
        }

        private async void FrmCashBoxes_Load(object sender, EventArgs e)
        {
            try
            {
                _isBinding = true;
                SetupGrid();

                // تحميل القوائم المنسدلة (Lookups) بالنماذج الصريحة لحل مشاكل الـ Binding
                await LoadBranchesAsync();
                await LoadCurrenciesAsync();
                await LoadAccountsAsync();

                chkIsActive.Checked = true;
                await LoadCashBoxesAsync();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                _isBinding = false;
            }
        }

        private void SetupGrid()
        {
            dgvCashBoxes.AutoGenerateColumns = false;
            dgvCashBoxes.AllowUserToAddRows = false;
            dgvCashBoxes.AllowUserToDeleteRows = false;
            dgvCashBoxes.ReadOnly = true;
            dgvCashBoxes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCashBoxes.MultiSelect = false;
        }

        //==================================== 
        // تحميل البيانات من الـ API 
        //==================================== 
        private async Task LoadBranchesAsync()
        {
            try
            {
                var data = await _client.GetFromJsonAsync<List<BranchCashLookup>>(
                    $"{_baseUrl}Branches/GetActiveBranchesLookup?companyId={CurrentSession.Company_ID}");

                cmbBranch.DataSource = data;
                cmbBranch.DisplayMember = "Branch_Name";
                cmbBranch.ValueMember = "Branch_ID";
                cmbBranch.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ أثناء تحميل الفروع: {ex.Message}");
            }
        }

        private async Task LoadCurrenciesAsync()
        {
            try
            {
                var data = await _client.GetFromJsonAsync<List<CurrencyCashLookup>>(
                    $"{_baseUrl}Currencies/GetLookup?companyId={CurrentSession.Company_ID}");

                cmbCurrency.DataSource = data;
                cmbCurrency.DisplayMember = "Currency_Name_AR";
                cmbCurrency.ValueMember = "Currency_Code";
                cmbCurrency.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ أثناء تحميل العملات: {ex.Message}");
            }
        }

        private async Task LoadAccountsAsync()
        {
            try
            {
                var data = await _client.GetFromJsonAsync<List<AccountCashLookup>>(
                $"{_baseUrl}ChartOfAccounts/GetCashParentLookup?companyId={CurrentSession.Company_ID}");

      //              $"{_baseUrl}ChartOfAccounts?companyId={CurrentSession.Company_ID}");


                cmbAccount.DataSource = data;
                cmbAccount.DisplayMember = "Account_Name_AR";
                cmbAccount.ValueMember = "Account_ID";
                cmbAccount.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ أثناء تحميل الحسابات الدليلية: {ex.Message}");
            }
        }

        private async Task LoadCashBoxesAsync()
        {
            try
            {
                var data = await _client.GetFromJsonAsync<List<CashBoxModel>>(
                    $"{_baseUrl}CashBoxes?companyId={CurrentSession.Company_ID}"
                );
                _cashBoxesCache = data ?? new();
                dgvCashBoxes.DataSource = null;
                dgvCashBoxes.DataSource = _cashBoxesCache;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ أثناء تحميل البيانات: {ex.Message}");
            }
        }

        //==================================== 
        // جديد 
        //==================================== 
        private void btnNew_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        //==================================== 
        // حفظ 
        //==================================== 
        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCashBoxNameAR.Text))
            {
                MessageBox.Show("أدخل اسم الصندوق");
                return;
            }
            try
            {
                var request = BuildRequest();
                var response = await _client.PostAsJsonAsync($"{_baseUrl}CashBoxes", request);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("تم الحفظ بنجاح");
                    await LoadCashBoxesAsync();
                    ClearForm();
                }
                else
                {
                    MessageBox.Show(await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء الحفظ: {ex.Message}");
            }
        }

        //==================================== 
        // تعديل 
        //==================================== 
        private async void btnEdit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_selectedCashBoxId))
            {
                MessageBox.Show("اختر صندوقاً أولاً");
                return;
            }
            try
            {
                var request = BuildRequest();
                var response = await _client.PutAsJsonAsync($"{_baseUrl}CashBoxes/{_selectedCashBoxId}", request);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("تم التعديل بنجاح");
                    await LoadCashBoxesAsync();
                }
                else
                {
                    MessageBox.Show(await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء التعديل: {ex.Message}");
            }
        }

        //==================================== 
        // حذف 
        //==================================== 
        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_selectedCashBoxId))
            {
                MessageBox.Show("اختر صندوقاً");
                return;
            }
            if (MessageBox.Show("هل تريد حذف الصندوق؟", "تأكيد الحذف", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            try
            {
                var response = await _client.DeleteAsync($"{_baseUrl}CashBoxes/{_selectedCashBoxId}");
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("تم الحذف بنجاح");
                    await LoadCashBoxesAsync();
                    ClearForm();
                }
                else
                {
                    MessageBox.Show(await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء الحذف: {ex.Message}");
            }
        }

        //==================================== 
        // تحديث 
        //==================================== 
        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            await LoadCashBoxesAsync();
            ClearForm();
        }

        //==================================== 
        // النقر على الجدول 
        //==================================== 
        private void dgvCashBoxes_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgvCashBoxes.CurrentRow == null) return;

            var selectedRow = dgvCashBoxes.CurrentRow.DataBoundItem as CashBoxModel;
            if (selectedRow == null) return;

            _selectedCashBoxId = selectedRow.ID.ToString();
            txtCashBoxCode.Text = selectedRow.Code;
            txtCashBoxNameAR.Text = selectedRow.NameAR;
            txtCashBoxNameEN.Text = selectedRow.NameEN;
            chkIsActive.Checked = selectedRow.IsActive;

            // ربط القوائم المنسدلة 
            if (selectedRow.Branch_ID != null) cmbBranch.SelectedValue = selectedRow.Branch_ID;
            if (selectedRow.Currency_Code != null) cmbCurrency.SelectedValue = selectedRow.Currency_Code;
            if (selectedRow.Account_ID != null) cmbAccount.SelectedValue = selectedRow.Account_ID;

            // استكمال تعبئة القيود المالية والملاحظات عند اختيار سطر
            numOpeningBalance.Value = selectedRow.Opening_Balance;
            numMaximumLimit.Value = selectedRow.Max_Limit;
            numMinimumLimit.Value = selectedRow.Min_Limit;
            txtNotes.Text = selectedRow.Notes ?? "";
        }

        //==================================== 
        // البحث 
        //==================================== 
        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                dgvCashBoxes.DataSource = _cashBoxesCache;
                return;
            }
            var filteredList = _cashBoxesCache.Where(c =>
                (c.NameAR != null && c.NameAR.ToLower().Contains(searchText)) ||
                (c.Code != null && c.Code.ToLower().Contains(searchText))
            ).ToList();
            dgvCashBoxes.DataSource = null;
            dgvCashBoxes.DataSource = filteredList;
        }

        //==================================== 
        // إغلاق 
        //==================================== 
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //==================================== 
        // دالات مساعدة (Helper Methods) 
        //==================================== 
        private CashBoxModel BuildRequest()
        {
            return new CashBoxModel
            {
                Company_ID = CurrentSession.Company_ID,
                Code = txtCashBoxCode.Text.Trim(),
                NameAR = txtCashBoxNameAR.Text.Trim(),
                NameEN = txtCashBoxNameEN.Text.Trim(),
                IsActive = chkIsActive.Checked,

                Branch_ID = cmbBranch.SelectedValue != null
                ? Convert.ToInt32(cmbBranch.SelectedValue)
                  : 0,

                //          Branch_ID = cmbBranch.SelectedValue?.ToString(),
                Currency_Code = cmbCurrency.SelectedValue?.ToString(),
                Account_ID = cmbAccount.SelectedValue?.ToString(),

                // إضافة حقول الحفظ والقيود المالية وبيانات المستخدم الحالية
                Opening_Balance = numOpeningBalance.Value,
                Max_Limit = numMaximumLimit.Value,
                Min_Limit = numMinimumLimit.Value,
                Notes = txtNotes.Text.Trim(),
                Created_By = CurrentSession.Username,
                Updated_By = CurrentSession.Username
            };
        }

        private void ClearForm()
        {
            _selectedCashBoxId = "";
            if (txtCashBoxCode != null) txtCashBoxCode.Clear();
            txtCashBoxNameAR.Clear();
            if (txtCashBoxNameEN != null) txtCashBoxNameEN.Clear();
            if (txtSearch != null) txtSearch.Clear();

            // إعادة تعيين القوائم المنسدلة إلى وضع عدم الاختيار 
            if (cmbBranch != null) cmbBranch.SelectedIndex = -1;
            if (cmbCurrency != null) cmbCurrency.SelectedIndex = -1;
            if (cmbAccount != null) cmbAccount.SelectedIndex = -1;

            // تفريغ وتصفير حقول المبالغ والملاحظات المضافة
            numOpeningBalance.Value = 0;
            numMaximumLimit.Value = 0;
            numMinimumLimit.Value = 0;
            if (txtNotes != null) txtNotes.Clear();

            chkIsActive.Checked = true;
        }

        private void chkIsActive_CheckedChanged(object sender, EventArgs e) { }
        private void groupBox3_Enter(object sender, EventArgs e) { }
        private void groupBox4_Enter(object sender, EventArgs e) { }
        private void label11_Click(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }

      

        private async void cmbAccount_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isBinding)
                return;

            if (cmbAccount.SelectedValue == null)
            {
                txtCashBoxCode.Clear();
                return;
            }

            try
            {
                var response = await _client.GetAsync(
                    $"{_baseUrl}CashBoxes/GetNextCode?companyId={CurrentSession.Company_ID}");

                if (response.IsSuccessStatusCode)
                {
                    string code = await response.Content.ReadAsStringAsync();
                    txtCashBoxCode.Text = code.Trim('"');
                }
                else
                {
                    MessageBox.Show(await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"تعذر توليد كود الصندوق: {ex.Message}");
            }
        }

    }

}