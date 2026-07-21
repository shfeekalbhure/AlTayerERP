using AlTayerERP.Desktop.Models;
using AlTayerERP.Desktop.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    public partial class FrmCostCenters : Form
    {
        //====================================
        // متغيرات عامة
        //====================================
        private readonly HttpClient _client = ApiService.Client;
        private readonly string _baseUrl = ApiService.BaseUrl;

        private string _selectedCostCenterId = "";
        private BindingList<CostCenterModel> _costCentersCache = new();
        private bool _isBinding = false;

        //====================================
        // Constructor
        //====================================
        public FrmCostCenters()
        {
            InitializeComponent();

            // تطبيق الثيم العربي الموحد والاختصارات على الشاشة القديمة.
            ArabicErpFormStyle.Apply(this);

            // ربط الأحداث مرة واحدة فقط وبشكل صحيح
            this.Load += FrmCostCenters_Load;

            btnSave.Click += btnSave_Click;
            btnEdit.Click += btnEdit_Click;
            btnDelete.Click += btnDelete_Click;
            btnNew.Click += btnNew_Click;
            btnRefresh.Click += btnRefresh_Click;
            btnSearch.Click += btnSearch_Click;
            btnClose.Click += btnClose_Click;

            dgvCostCenters.CellClick += dgvCostCenters_CellClick;
            tvCostCenters.AfterSelect += tvCostCenters_AfterSelect;
            cmbParentCostCenter.SelectedIndexChanged += cmbParentCostCenter_SelectedIndexChanged;
            txtSearchTree.TextChanged += txtSearchTree_TextChanged;
        }

        //====================================
        // تحميل الشاشة
        //====================================
        private async void FrmCostCenters_Load(object sender, EventArgs e)
        {
            try
            {
                _isBinding = true;

                SetupGrid();
                FillCostCenterTypes();

                await LoadCostCentersAsync();

                FillParentCostCenters();
                BuildTree();

                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"خطأ أثناء تحميل الشاشة:\n{ex.Message}",
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _isBinding = false;
            }
        }

        //====================================
        // تجهيز الجدول
        //====================================
        private void SetupGrid()
        {
            dgvCostCenters.AutoGenerateColumns = false;
            dgvCostCenters.AllowUserToAddRows = false;
            dgvCostCenters.AllowUserToDeleteRows = false;
            dgvCostCenters.ReadOnly = true;
            dgvCostCenters.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCostCenters.MultiSelect = false;

            // جعل صندوق النص الخاص بالكود للقراءة فقط لأن السيرفر سيتولى إنشائه
            txtCostCenterCode.ReadOnly = true;
        }

        //====================================
        // تعبئة نوع المركز
        //====================================
        private void FillCostCenterTypes()
        {
            cmbCostCenterType.Items.Clear();

            cmbCostCenterType.Items.AddRange(new object[]
            {
                "رئيسي",
                "فرعي",
                "إدارة",
                "قسم",
                "مشروع",
                "فرع",
                "نشاط"
            });

            cmbCostCenterType.SelectedIndex = -1;
        }

        //====================================
        // تحميل البيانات
        //====================================
        private async Task LoadCostCentersAsync()
        {
            var response = await _client.GetAsync(
                $"{_baseUrl}CostCenters?companyId={CurrentSession.Company_ID}");

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }

            var data = await response.Content
                .ReadFromJsonAsync<List<CostCenterModel>>();

            _costCentersCache = data != null
                ? new BindingList<CostCenterModel>(data)
                : new BindingList<CostCenterModel>();

            dgvCostCenters.DataSource = null;
            dgvCostCenters.DataSource = _costCentersCache;
        }

        //====================================
        // تعبئة المركز الأب
        //====================================
        private void FillParentCostCenters()
        {
            string selectedId = cmbParentCostCenter.SelectedValue?.ToString() ?? "";

            var parents = _costCentersCache
                .Where(x => x.Cost_Center_ID != _selectedCostCenterId)
                .OrderBy(x => x.Center_Code)
                .ToList();

            cmbParentCostCenter.DataSource = null;
            cmbParentCostCenter.DisplayMember = "Center_Name_AR";
            cmbParentCostCenter.ValueMember = "Cost_Center_ID";
            cmbParentCostCenter.DataSource = parents;
            cmbParentCostCenter.SelectedIndex = -1;

            if (!string.IsNullOrWhiteSpace(selectedId))
                cmbParentCostCenter.SelectedValue = selectedId;
        }

        //====================================
        // بناء الشجرة (Tree View)
        //====================================
        private void BuildTree()
        {
            tvCostCenters.Nodes.Clear();

            var rootCenters = _costCentersCache
                .Where(x => string.IsNullOrWhiteSpace(x.Parent_Cost_Center_ID))
                .OrderBy(x => x.Center_Code)
                .ToList();

            foreach (var center in rootCenters)
            {
                TreeNode node = new TreeNode(
                    $"{center.Center_Code} - {center.Center_Name_AR}")
                {
                    Tag = center.Cost_Center_ID
                };

                AddChildNodes(node, center.Cost_Center_ID);
                tvCostCenters.Nodes.Add(node);
            }

            tvCostCenters.ExpandAll();
        }

        private void AddChildNodes(TreeNode parentNode, string parentId)
        {
            var children = _costCentersCache
                .Where(x => x.Parent_Cost_Center_ID == parentId)
                .OrderBy(x => x.Center_Code)
                .ToList();

            foreach (var child in children)
            {
                TreeNode childNode = new TreeNode(
                    $"{child.Center_Code} - {child.Center_Name_AR}")
                {
                    Tag = child.Cost_Center_ID
                };

                AddChildNodes(childNode, child.Cost_Center_ID);
                parentNode.Nodes.Add(childNode);
            }
        }

        //====================================
        // تنظيف الشاشة
        //====================================
        private void ClearForm()
        {
            _selectedCostCenterId = "";

            txtCostCenterCode.Clear();
            txtCostCenterNameAR.Clear();
            txtCostCenterNameEN.Clear();
            txtNotes.Clear();

            cmbParentCostCenter.SelectedIndex = -1;
            cmbCostCenterType.SelectedIndex = -1;

            txtLevel.Text = "1";

            chkIsPostable.Checked = true;
            chkIsActive.Checked = true;

            dgvCostCenters.ClearSelection();
            tvCostCenters.SelectedNode = null;

            txtCostCenterNameAR.Focus();
        }

        //====================================
        // بناء الطلب (Build Request)
        //====================================
        private object BuildRequest()
        {
            return new
            {
                Company_ID = CurrentSession.Company_ID,

                Parent_Cost_Center_ID =
                    cmbParentCostCenter.SelectedValue?.ToString(),

                // تعديل: نرسله فارغاً للسيرفر ليتولى الـ API الحساب والتوليد
                Center_Code = (string?)null,

                Center_Name_AR = txtCostCenterNameAR.Text.Trim(),

                Center_Name_EN = string.IsNullOrWhiteSpace(txtCostCenterNameEN.Text)
                    ? null
                    : txtCostCenterNameEN.Text.Trim(),

                Center_Level = int.TryParse(txtLevel.Text, out int level)
                    ? level
                    : 1,

                Is_Postable = chkIsPostable.Checked,

                Is_Active = chkIsActive.Checked,

                Notes = string.IsNullOrWhiteSpace(txtNotes.Text)
                    ? null
                    : txtNotes.Text.Trim(),

                Created_By = CurrentSession.Username,

                Updated_By = CurrentSession.Username
            };
        }

        //====================================
        // حفظ
        //====================================
        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCostCenterNameAR.Text))
            {
                MessageBox.Show("أدخل اسم مركز التكلفة");
                return;
            }

            try
            {
                var request = BuildRequest();

                var response = await _client.PostAsJsonAsync(
                    $"{_baseUrl}CostCenters",
                    request);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("تم الحفظ بنجاح");

                    await LoadCostCentersAsync();
                    FillParentCostCenters();
                    BuildTree();
                    ClearForm();
                }
                else
                {
                    MessageBox.Show(await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ أثناء الحفظ: {ex.Message}");
            }
        }

        //====================================
        // تعديل
        //====================================
        private async void btnEdit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_selectedCostCenterId))
            {
                MessageBox.Show("اختر مركز تكلفة");
                return;
            }

            try
            {
                var request = BuildRequest();

                var response = await _client.PutAsJsonAsync(
                    $"{_baseUrl}CostCenters/{_selectedCostCenterId}",
                    request);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("تم التعديل");

                    await LoadCostCentersAsync();
                    FillParentCostCenters();
                    BuildTree();
                }
                else
                {
                    MessageBox.Show(await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ أثناء التعديل: {ex.Message}");
            }
        }

        //====================================
        // حذف
        //====================================
        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_selectedCostCenterId))
            {
                MessageBox.Show("اختر مركز تكلفة");
                return;
            }

            if (MessageBox.Show(
                "هل تريد حذف مركز التكلفة؟",
                "تأكيد",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                var response = await _client.DeleteAsync(
                    $"{_baseUrl}CostCenters/{_selectedCostCenterId}?companyId={CurrentSession.Company_ID}");

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("تم الحذف");

                    await LoadCostCentersAsync();
                    FillParentCostCenters();
                    BuildTree();
                    ClearForm();
                }
                else
                {
                    MessageBox.Show(await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ أثناء الحذف: {ex.Message}");
            }
        }

        //====================================
        // جديد وتحديث وإغلاق
        //====================================
        private void btnNew_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                _isBinding = true;

                await LoadCostCentersAsync();

                FillParentCostCenters();
                BuildTree();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"خطأ أثناء التحديث:\n{ex.Message}",
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _isBinding = false;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //====================================
        // أحداث اختيار البيانات من العناصر
        //====================================
        private void dgvCostCenters_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (_isBinding || e.RowIndex < 0)
                return;

            var row = dgvCostCenters.Rows[e.RowIndex];

            var selected = row.DataBoundItem as CostCenterModel;

            if (selected != null)
                BindModelToForm(selected);
        }

        private void tvCostCenters_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (_isBinding || e.Node?.Tag == null)
                return;

            string id = e.Node.Tag.ToString() ?? "";

            var center = _costCentersCache
                .FirstOrDefault(x => x.Cost_Center_ID == id);

            if (center != null)
            {
                BindModelToForm(center);
                LoadChildCentersToGrid(id);
            }
        }

        private void LoadChildCentersToGrid(string parentId)
        {
            var children = _costCentersCache
                .Where(x => x.Parent_Cost_Center_ID == parentId)
                .OrderBy(x => x.Center_Code)
                .ToList();

            dgvCostCenters.DataSource = null;
            dgvCostCenters.DataSource =
                new BindingList<CostCenterModel>(children);
        }

        private void BindModelToForm(CostCenterModel model)
        {
            _isBinding = true;

            try
            {
                _selectedCostCenterId = model.Cost_Center_ID;

                txtCostCenterCode.Text = model.Center_Code;
                txtCostCenterNameAR.Text = model.Center_Name_AR;
                txtCostCenterNameEN.Text = model.Center_Name_EN ?? "";
                txtLevel.Text = model.Center_Level.ToString();
                txtNotes.Text = model.Notes ?? "";

                chkIsPostable.Checked = model.Is_Postable;
                chkIsActive.Checked = model.Is_Active;

                FillParentCostCenters();

                if (!string.IsNullOrWhiteSpace(model.Parent_Cost_Center_ID))
                    cmbParentCostCenter.SelectedValue = model.Parent_Cost_Center_ID;
                else
                    cmbParentCostCenter.SelectedIndex = -1;
            }
            finally
            {
                _isBinding = false;
            }
        }

        private void cmbParentCostCenter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isBinding)
                return;

            string parentId = cmbParentCostCenter.SelectedValue?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(parentId))
            {
                txtLevel.Text = "1";
            }
            else
            {
                var parent = _costCentersCache
                    .FirstOrDefault(x => x.Cost_Center_ID == parentId);

                txtLevel.Text = parent == null
                    ? "1"
                    : (parent.Center_Level + 1).ToString();
            }
        }

        //====================================
        // البحث الفلترة
        //====================================
        private void btnSearch_Click(object sender, EventArgs e)
        {
            string keyword =
                Microsoft.VisualBasic.Interaction.InputBox(
                    "أدخل كود أو اسم مركز التكلفة:",
                    "بحث مراكز التكلفة",
                    "");

            SearchCostCenters(keyword);
        }

        private void SearchCostCenters(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                dgvCostCenters.DataSource = null;
                dgvCostCenters.DataSource = _costCentersCache;

                BuildTree();
                return;
            }

            keyword = keyword.Trim();

            var result = _costCentersCache
                .Where(x =>
                    (x.Center_Code ?? "").Contains(
                        keyword,
                        StringComparison.OrdinalIgnoreCase) ||

                    (x.Center_Name_AR ?? "").Contains(
                        keyword,
                        StringComparison.OrdinalIgnoreCase) ||

                    (x.Center_Name_EN ?? "").Contains(
                        keyword,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

            dgvCostCenters.DataSource = null;
            dgvCostCenters.DataSource =
                new BindingList<CostCenterModel>(result);

            BuildFilteredTree(result);
        }

        private void txtSearchTree_TextChanged(object sender, EventArgs e)
        {
            SearchCostCenters(txtSearchTree.Text);
        }

        private void BuildFilteredTree(List<CostCenterModel> list)
        {
            tvCostCenters.Nodes.Clear();

            foreach (var center in list.OrderBy(x => x.Center_Code))
            {
                TreeNode node = new TreeNode(
                    $"{center.Center_Code} - {center.Center_Name_AR}")
                {
                    Tag = center.Cost_Center_ID
                };

                tvCostCenters.Nodes.Add(node);
            }

            tvCostCenters.ExpandAll();
        }
    }
}