using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Http;
using System.Net.Http.Json;
using System.Linq;
using System.Threading.Tasks;
// [الخطوة الجديدة]: استدعاء المجلد الذي يحتوي على كلاس الخدمة المركزي للـ API
using AlTayerERP.Desktop.Services;

namespace AlTayerERP.Desktop
{
    public partial class FrmRoles : Form
    {
        // ====================================================================
        // [التعديل الجديد]: قراءة الـ Client والـ BaseUrl من الملف المركزي مباشرة
        // دون الحاجة لتعديل بقية أكواد الأزرار والدوال بالأسفل
        // ====================================================================
        private readonly HttpClient _client = ApiService.Client;
        private readonly string _baseUrl = ApiService.BaseUrl;

        private int _selectedRoleId = 0;
        private List<RoleModel> _rolesCache = new List<RoleModel>();

        // ==========================================
        // 1. دالة البناء (Constructor)
        // ==========================================
        public FrmRoles()
        {
            InitializeComponent();

            // توحيد شكل الشاشة القديمة والاختصارات العربية دون تغيير منطقها.
            ArabicErpFormStyle.Apply(this);

            // تهيئة وإعداد أعمدة الجدول
            SetupRolesGrid();

            // ربط أحداث الشاشة وعناصرها
            this.Load += FrmRoles_Load;

            // ربط كافة أزرار التحكم وأحداث الجدول بالدوال الخاصة بها
            btnSave.Click += btnSave_Click;
            btnEdit.Click += btnEdit_Click;
            btnDelete.Click += btnDelete_Click;
            btnNew.Click += btnNew_Click;
            btnRefresh.Click += btnRefresh_Click;
            btnClose.Click += btnClose_Click;
            dgvRoles.CellClick += dgvRoles_CellClick;
        }

        // ==========================================
        // 2. أحداث تحميل الشاشة وتهيئة الجدول
        // ==========================================
        private async void FrmRoles_Load(object sender, EventArgs e)
        {
            cmbStatus.Items.Clear();
            cmbStatus.Items.Add("نشط");
            cmbStatus.Items.Add("موقوف");
            cmbStatus.Text = "نشط";

            // جلب البيانات فور فتح الشاشة
            await LoadRolesAsync();
        }

        private void SetupRolesGrid()
        {
            dgvRoles.Columns.Clear();
            dgvRoles.AllowUserToAddRows = false;
            dgvRoles.ReadOnly = true;
            dgvRoles.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRoles.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvRoles.Columns.Add("Role_ID", "رقم الدور");
            dgvRoles.Columns.Add("Role_Code", "كود الدور");
            dgvRoles.Columns.Add("Role_Name", "اسم الدور");
            dgvRoles.Columns.Add("Description", "الوصف");
            dgvRoles.Columns.Add("Is_Active", "الحالة");
        }

        private async Task LoadRolesAsync()
        {
            try
            {
                var roles = await _client.GetFromJsonAsync<List<RoleModel>>($"{_baseUrl}Roles");
                dgvRoles.Rows.Clear();
                _rolesCache = roles ?? new List<RoleModel>();

                foreach (var role in _rolesCache)
                {
                    dgvRoles.Rows.Add(
                        role.Role_ID,
                        role.Role_Code,
                        role.Role_Name,
                        role.Description,
                        role.Is_Active ? "نشط" : "موقوف"
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("فشل تحميل الأدوار:\n" + ex.Message);
            }
        }

        // ==========================================
        // 3. الدوال البرمجية وأحداث التحكم (CRUD)
        // ==========================================

        // دالة تنظيف أدوات الإدخال وإعادة تعيين الشاشة لوضع الجاهزية
        private void ClearForm()
        {
            _selectedRoleId = 0;
            txtRoleCode.Clear();
            txtRoleName.Clear();
            txtDescription.Clear();
            cmbStatus.Text = "نشط";
            txtRoleName.Focus();
        }

        // حدث حفظ دور جديد
        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRoleName.Text))
            {
                MessageBox.Show("يرجى إدخال اسم الدور.");
                return;
            }

            var request = new
            {
                Role_Code = txtRoleCode.Text.Trim(),
                Role_Name = txtRoleName.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                Is_Active = cmbStatus.Text == "نشط"
            };

            var response = await _client.PostAsJsonAsync($"{_baseUrl}Roles", request);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("تم حفظ الدور بنجاح.");
                ClearForm();
                await LoadRolesAsync();
            }
            else
            {
                MessageBox.Show(await response.Content.ReadAsStringAsync());
            }
        }

        // حدث نقل البيانات من السطر المحدد بالجدول للحقول أعلاه
        private void dgvRoles_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dgvRoles.Rows[e.RowIndex];

            _selectedRoleId = Convert.ToInt32(row.Cells["Role_ID"].Value);
            txtRoleCode.Text = row.Cells["Role_Code"].Value?.ToString();
            txtRoleName.Text = row.Cells["Role_Name"].Value?.ToString();
            txtDescription.Text = row.Cells["Description"].Value?.ToString();
            cmbStatus.Text = row.Cells["Is_Active"].Value?.ToString();
        }

        // حدث التعديل
        private async void btnEdit_Click(object sender, EventArgs e)
        {
            if (_selectedRoleId == 0)
            {
                MessageBox.Show("اختر الدور من الجدول أولًا.");
                return;
            }

            var request = new
            {
                Role_ID = _selectedRoleId,
                Role_Code = txtRoleCode.Text.Trim(),
                Role_Name = txtRoleName.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                Is_Active = cmbStatus.Text == "نشط"
            };

            var response = await _client.PutAsJsonAsync($"{_baseUrl}Roles/{_selectedRoleId}", request);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("تم تعديل الدور بنجاح.");
                ClearForm();
                await LoadRolesAsync();
            }
            else
            {
                MessageBox.Show(await response.Content.ReadAsStringAsync());
            }
        }

        // حدث الحذف
        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedRoleId == 0)
            {
                MessageBox.Show("اختر الدور من الجدول أولًا.");
                return;
            }

            if (MessageBox.Show("هل تريد حذف هذا الدور؟", "تأكيد", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

            var response = await _client.DeleteAsync($"{_baseUrl}Roles/{_selectedRoleId}");

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("تم حذف الدور بنجاح.");
                ClearForm();
                await LoadRolesAsync();
            }
            else
            {
                MessageBox.Show(await response.Content.ReadAsStringAsync());
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            ClearForm();
            await LoadRolesAsync();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void grpRoleData_Enter(object sender, EventArgs e)
        {
        }
    } // نهاية كلاس FrmRoles

    // كلاس الموديل الخاص بالأدوار (RoleModel)
    public class RoleModel
    {
        public int Role_ID { get; set; }
        public string? Role_Code { get; set; }
        public string Role_Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Is_Active { get; set; }
    }
}