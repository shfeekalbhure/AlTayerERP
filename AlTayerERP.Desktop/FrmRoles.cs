using AlTayerERP.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// إدارة الأدوار بصورة مستقلة عن المستخدمين.
    /// الحذف في هذه الشاشة إيقاف منطقي، وتدار صلاحيات الشاشات من شاشة صلاحيات الأدوار.
    /// </summary>
    public partial class FrmRoles : Form
    {
        private readonly HttpClient _client = ApiService.Client;
        private readonly string _baseUrl = ApiService.BaseUrl;
        private readonly TextBox _txtSearch = new();
        private readonly Button _btnPermissions = new();

        private int _selectedRoleId;
        private bool _isBusy;
        private List<RoleModel> _rolesCache = new();

        public FrmRoles()
        {
            InitializeComponent();
            ConfigureScreen();
            WireEvents();
        }

        private void ConfigureScreen()
        {
            Text = "إدارة الأدوار";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(1000, 650);
            KeyPreview = true;

            grpRoleData.Text = "بيانات الدور";
            cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;

            btnDelete.Text = "إيقاف";
            btnSave.Text = "حفظ  Ctrl+S";
            btnNew.Text = "جديد  Ctrl+N";
            btnRefresh.Text = "تحديث  F5";
            btnClose.Text = "إغلاق  Esc";

            _txtSearch.Name = "txtRoleSearch";
            _txtSearch.PlaceholderText = "بحث بالكود أو اسم الدور";
            _txtSearch.Width = 210;
            _txtSearch.Height = 30;
            _txtSearch.Location = new Point(15, 20);
            _txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            _btnPermissions.Name = "btnPermissions";
            _btnPermissions.Text = "صلاحيات الدور";
            _btnPermissions.Width = 115;
            _btnPermissions.Height = 30;
            _btnPermissions.Location = new Point(350, 20);
            _btnPermissions.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            pnlToolbar.Controls.Add(_txtSearch);
            pnlToolbar.Controls.Add(_btnPermissions);
            _txtSearch.BringToFront();
            _btnPermissions.BringToFront();

            SetupRolesGrid();
            ApplyAuthorizationState();
        }

        private void WireEvents()
        {
            Load -= FrmRoles_Load;
            Load += FrmRoles_Load;

            btnSave.Click -= btnSave_Click;
            btnSave.Click += btnSave_Click;
            btnEdit.Click -= btnEdit_Click;
            btnEdit.Click += btnEdit_Click;
            btnDelete.Click -= btnDelete_Click;
            btnDelete.Click += btnDelete_Click;
            btnNew.Click -= btnNew_Click;
            btnNew.Click += btnNew_Click;
            btnRefresh.Click -= btnRefresh_Click;
            btnRefresh.Click += btnRefresh_Click;
            btnClose.Click -= btnClose_Click;
            btnClose.Click += btnClose_Click;
            btnSearch.Click -= btnSearch_Click;
            btnSearch.Click += btnSearch_Click;
            dgvRoles.CellClick -= dgvRoles_CellClick;
            dgvRoles.CellClick += dgvRoles_CellClick;

            _txtSearch.TextChanged += (_, _) => ApplyFilter();
            _btnPermissions.Click += (_, _) => OpenRolePermissions();
            KeyDown += FrmRoles_KeyDown;
        }

        private void ApplyAuthorizationState()
        {
            bool canManage = CurrentSession.Is_System_Admin;
            btnSave.Enabled = canManage;
            btnEdit.Enabled = canManage;
            btnDelete.Enabled = canManage;
            btnNew.Enabled = canManage;
            _btnPermissions.Enabled = canManage;

            if (!canManage)
                grpRoleData.Enabled = false;
        }

        private async void FrmRoles_Load(object? sender, EventArgs e)
        {
            cmbStatus.Items.Clear();
            cmbStatus.Items.AddRange(new object[] { "نشط", "موقوف" });
            cmbStatus.SelectedIndex = 0;
            await LoadRolesAsync();
            ClearForm();
        }

        private void SetupRolesGrid()
        {
            dgvRoles.Columns.Clear();
            dgvRoles.AllowUserToAddRows = false;
            dgvRoles.AllowUserToDeleteRows = false;
            dgvRoles.ReadOnly = true;
            dgvRoles.RowHeadersVisible = false;
            dgvRoles.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRoles.MultiSelect = false;
            dgvRoles.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvRoles.Columns.Add("Role_ID", "رقم الدور");
            dgvRoles.Columns.Add("Role_Code", "كود الدور");
            dgvRoles.Columns.Add("Role_Name", "اسم الدور");
            dgvRoles.Columns.Add("Description", "الوصف");
            dgvRoles.Columns.Add("Is_System_Admin", "دور نظام");
            dgvRoles.Columns.Add("Is_Active", "الحالة");

            dgvRoles.Columns["Role_ID"].FillWeight = 55;
            dgvRoles.Columns["Role_Code"].FillWeight = 90;
            dgvRoles.Columns["Role_Name"].FillWeight = 150;
            dgvRoles.Columns["Description"].FillWeight = 260;
            dgvRoles.Columns["Is_System_Admin"].FillWeight = 75;
            dgvRoles.Columns["Is_Active"].FillWeight = 75;
        }

        private async Task LoadRolesAsync()
        {
            if (_isBusy) return;

            try
            {
                SetBusy(true);
                _rolesCache = await _client.GetFromJsonAsync<List<RoleModel>>($"{_baseUrl}Roles") ?? new();
                ApplyFilter();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("تعذر الاتصال بخدمة الأدوار. تحقق من تشغيل API والجلسة.\n\n" + ex.Message,
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("فشل تحميل الأدوار.\n\n" + ex.Message,
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void ApplyFilter()
        {
            string query = _txtSearch.Text.Trim();
            var rows = string.IsNullOrWhiteSpace(query)
                ? _rolesCache
                : _rolesCache.Where(x =>
                    (x.Role_Code ?? string.Empty).Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                    x.Role_Name.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                    (x.Description ?? string.Empty).Contains(query, StringComparison.CurrentCultureIgnoreCase))
                  .ToList();

            dgvRoles.Rows.Clear();
            foreach (var role in rows.OrderBy(x => x.Role_Name))
            {
                dgvRoles.Rows.Add(
                    role.Role_ID,
                    role.Role_Code,
                    role.Role_Name,
                    role.Description,
                    role.Is_System_Admin ? "نعم" : "لا",
                    role.Is_Active ? "نشط" : "موقوف");
            }
        }

        private void ClearForm()
        {
            _selectedRoleId = 0;
            txtRoleCode.Clear();
            txtRoleName.Clear();
            txtDescription.Clear();
            cmbStatus.SelectedIndex = 0;
            txtRoleCode.ReadOnly = false;
            btnDelete.Enabled = CurrentSession.Is_System_Admin;
            txtRoleName.Focus();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtRoleName.Text))
            {
                MessageBox.Show("اسم الدور مطلوب.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRoleName.Focus();
                return false;
            }

            if (txtRoleName.Text.Trim().Length < 3)
            {
                MessageBox.Show("اسم الدور يجب أن يتكون من ثلاثة أحرف على الأقل.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRoleName.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtRoleCode.Text) && txtRoleCode.Text.Trim().Any(char.IsWhiteSpace))
            {
                MessageBox.Show("كود الدور لا يقبل المسافات.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRoleCode.Focus();
                return false;
            }

            return true;
        }

        private async void btnSave_Click(object? sender, EventArgs e)
        {
            if (!EnsureAdministrator() || !ValidateInput()) return;

            var request = new
            {
                Role_Code = txtRoleCode.Text.Trim(),
                Role_Name = txtRoleName.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                Is_Active = true
            };

            await ExecuteWriteAsync(
                () => _client.PostAsJsonAsync($"{_baseUrl}Roles", request),
                "تم حفظ الدور بنجاح.");
        }

        private async void btnEdit_Click(object? sender, EventArgs e)
        {
            if (!EnsureAdministrator() || _selectedRoleId <= 0)
            {
                if (_selectedRoleId <= 0)
                    MessageBox.Show("اختر الدور المراد تعديله من الجدول.", Text,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateInput()) return;

            var selectedRole = _rolesCache.FirstOrDefault(x => x.Role_ID == _selectedRoleId);
            if (selectedRole?.Is_System_Admin == true)
            {
                MessageBox.Show("دور مدير النظام محمي ولا يعدل من هذه الشاشة.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            await ExecuteWriteAsync(
                () => _client.PutAsJsonAsync($"{_baseUrl}Roles/{_selectedRoleId}", request),
                "تم تعديل الدور بنجاح.");
        }

        private async void btnDelete_Click(object? sender, EventArgs e)
        {
            if (!EnsureAdministrator()) return;
            if (_selectedRoleId <= 0)
            {
                MessageBox.Show("اختر الدور المراد إيقافه من الجدول.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedRole = _rolesCache.FirstOrDefault(x => x.Role_ID == _selectedRoleId);
            if (selectedRole?.Is_System_Admin == true)
            {
                MessageBox.Show("لا يمكن إيقاف دور مدير النظام.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedRole?.Is_Active == false)
            {
                MessageBox.Show("الدور موقوف بالفعل. يمكن إعادة تنشيطه من زر تعديل بعد اختيار الحالة نشط.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("سيتم إيقاف الدور من الاستخدام دون حذف سجله أو صلاحياته. هل تريد المتابعة؟",
                    "تأكيد إيقاف الدور", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            await ExecuteWriteAsync(
                () => _client.DeleteAsync($"{_baseUrl}Roles/{_selectedRoleId}"),
                "تم إيقاف الدور بنجاح.");
        }

        private async Task ExecuteWriteAsync(Func<Task<HttpResponseMessage>> operation, string successMessage)
        {
            if (_isBusy) return;

            try
            {
                SetBusy(true);
                using var response = await operation();
                if (!response.IsSuccessStatusCode)
                {
                    string details = await response.Content.ReadAsStringAsync();
                    string title = response.StatusCode == HttpStatusCode.Forbidden ? "غير مصرح" : "تعذر تنفيذ العملية";
                    MessageBox.Show(string.IsNullOrWhiteSpace(details) ? response.ReasonPhrase : details,
                        title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show(successMessage, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                await LoadRolesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر تنفيذ العملية.\n\n" + ex.Message,
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void SetBusy(bool busy)
        {
            _isBusy = busy;
            UseWaitCursor = busy;
            btnSave.Enabled = !busy && CurrentSession.Is_System_Admin;
            btnEdit.Enabled = !busy && CurrentSession.Is_System_Admin;
            btnDelete.Enabled = !busy && CurrentSession.Is_System_Admin;
            btnRefresh.Enabled = !busy;
            _btnPermissions.Enabled = !busy && CurrentSession.Is_System_Admin;
        }

        private bool EnsureAdministrator()
        {
            if (CurrentSession.Is_System_Admin) return true;

            MessageBox.Show("إدارة الأدوار مخصصة لمدير النظام.", "الصلاحيات",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        private void dgvRoles_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dgvRoles.Rows[e.RowIndex];
            _selectedRoleId = Convert.ToInt32(row.Cells["Role_ID"].Value);
            var role = _rolesCache.FirstOrDefault(x => x.Role_ID == _selectedRoleId);
            if (role == null) return;

            txtRoleCode.Text = role.Role_Code;
            txtRoleName.Text = role.Role_Name;
            txtDescription.Text = role.Description;
            cmbStatus.Text = role.Is_Active ? "نشط" : "موقوف";
            txtRoleCode.ReadOnly = role.Is_System_Admin;
            btnDelete.Enabled = CurrentSession.Is_System_Admin && !role.Is_System_Admin && role.Is_Active;
        }

        private void OpenRolePermissions()
        {
            if (!EnsureAdministrator()) return;
            using var form = new FrmRolePermissions();
            form.ShowDialog(this);
        }

        private void btnNew_Click(object? sender, EventArgs e) => ClearForm();

        private async void btnRefresh_Click(object? sender, EventArgs e)
        {
            ClearForm();
            await LoadRolesAsync();
        }

        private void btnSearch_Click(object? sender, EventArgs e)
        {
            _txtSearch.Focus();
            _txtSearch.SelectAll();
        }

        private void btnClose_Click(object? sender, EventArgs e) => Close();

        private void FrmRoles_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.N)
            {
                ClearForm();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.S)
            {
                if (_selectedRoleId > 0)
                    btnEdit.PerformClick();
                else
                    btnSave.PerformClick();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.F)
            {
                btnSearch.PerformClick();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F5)
            {
                btnRefresh.PerformClick();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Close();
                e.SuppressKeyPress = true;
            }
        }

        private void grpRoleData_Enter(object? sender, EventArgs e)
        {
        }
    }

    public class RoleModel
    {
        public int Role_ID { get; set; }
        public string? Role_Code { get; set; }
        public string Role_Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Is_Active { get; set; }
        public bool Is_System_Admin { get; set; }
    }
}
