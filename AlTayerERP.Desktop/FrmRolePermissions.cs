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
    /// <summary>
    /// إدارة صلاحيات الأدوار على مستوى الشاشة والعملية.
    /// </summary>
    public sealed class FrmRolePermissions : Form
    {
        private readonly HttpClient _client = ApiService.Client;
        private readonly ComboBox cmbRoles = new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
        private readonly DataGridView dgvPermissions = new() { Dock = DockStyle.Fill, AutoGenerateColumns = false, AllowUserToAddRows = false, RowHeadersVisible = false };
        private readonly Button btnSave = new() { Text = "حفظ الصلاحيات", AutoSize = true };
        private readonly Button btnRefresh = new() { Text = "تحديث", AutoSize = true };
        private readonly Button btnGrantViewAll = new() { Text = "منح العرض للجميع", AutoSize = true };
        private readonly Button btnClearAll = new() { Text = "إلغاء كل الصلاحيات", AutoSize = true };

        public FrmRolePermissions()
        {
            Text = "صلاحيات الأدوار";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1100;
            Height = 650;
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;

            var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 52, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(12) };
            toolbar.Controls.Add(new Label { Text = "الدور:", AutoSize = true, Padding = new Padding(0, 8, 4, 0) });
            toolbar.Controls.Add(cmbRoles);
            cmbRoles.Width = 260;
            toolbar.Controls.Add(btnSave);
            toolbar.Controls.Add(btnGrantViewAll);
            toolbar.Controls.Add(btnClearAll);
            toolbar.Controls.Add(btnRefresh);

            Controls.Add(dgvPermissions);
            Controls.Add(toolbar);

            AddTextColumn("Module_Name", "النظام", 130);
            AddTextColumn("Screen_Name", "الشاشة", 220);
            AddCheckColumn("Can_View", "عرض");
            AddCheckColumn("Can_Add", "إضافة");
            AddCheckColumn("Can_Edit", "تعديل");
            AddCheckColumn("Can_Delete", "حذف");
            AddCheckColumn("Can_Print", "طباعة");
            AddCheckColumn("Can_Export", "تصدير");
            AddCheckColumn("Can_Import", "استيراد");
            AddCheckColumn("Can_Approve", "اعتماد");
            AddCheckColumn("Can_UnApprove", "إلغاء الاعتماد");

            Load += async (_, _) => await LoadRolesAsync();
            cmbRoles.SelectedIndexChanged += async (_, _) => await LoadPermissionsAsync();
            btnRefresh.Click += async (_, _) => await LoadRolesAsync();
            btnSave.Click += async (_, _) => await SavePermissionsAsync();
            btnGrantViewAll.Click += (_, _) => SetAllViewPermissions(true);
            btnClearAll.Click += (_, _) => SetAllViewPermissions(false);
        }

        private void AddTextColumn(string property, string title, int width) =>
            dgvPermissions.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = property, HeaderText = title, Width = width, ReadOnly = true });

        private void AddCheckColumn(string property, string title) =>
            dgvPermissions.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = property, HeaderText = title, Width = 68 });

        // تطبيق صلاحية العرض على الصفوف كلها لتسهيل التهيئة الأولية للدور.
        private void SetAllViewPermissions(bool canView)
        {
            if (dgvPermissions.DataSource is not List<PermissionRow> rows)
                return;

            foreach (var row in rows)
            {
                row.Can_View = canView;
                if (!canView)
                {
                    // عند منع العرض تُلغى صلاحيات العمليات التابعة للشاشة لمنع صلاحية متناقضة.
                    row.Can_Add = false;
                    row.Can_Edit = false;
                    row.Can_Delete = false;
                    row.Can_Print = false;
                    row.Can_Export = false;
                    row.Can_Import = false;
                    row.Can_Approve = false;
                    row.Can_UnApprove = false;
                }
            }

            dgvPermissions.Refresh();
        }

        private async Task LoadRolesAsync()
        {
            try
            {
                var roles = await _client.GetFromJsonAsync<List<RoleRow>>("Roles") ?? new();
                cmbRoles.DataSource = roles.Where(x => x.Is_Active).ToList();
                cmbRoles.DisplayMember = nameof(RoleRow.Role_Name);
                cmbRoles.ValueMember = nameof(RoleRow.Role_ID);
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر تحميل الأدوار: " + ex.Message, "صلاحيات الأدوار", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadPermissionsAsync()
        {
            if (cmbRoles.SelectedValue is not int roleId || roleId <= 0)
                return;

            try
            {
                var screens = await _client.GetFromJsonAsync<List<ScreenRow>>("RolePermissions/GetScreens") ?? new();
                var saved = await _client.GetFromJsonAsync<List<PermissionRow>>($"RolePermissions/GetRolePermissions/{roleId}") ?? new();
                var rows = screens.Select(screen =>
                {
                    var permission = saved.FirstOrDefault(x => x.Screen_ID == screen.Screen_ID);
                    return PermissionRow.From(screen, roleId, permission);
                }).ToList();

                dgvPermissions.DataSource = rows;
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر تحميل صلاحيات الدور: " + ex.Message, "صلاحيات الأدوار", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task SavePermissionsAsync()
        {
            if (!CurrentSession.Is_System_Admin)
            {
                MessageBox.Show("حفظ الصلاحيات مخصص لمدير النظام.", "الصلاحيات", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgvPermissions.DataSource is not List<PermissionRow> rows || rows.Count == 0)
            {
                MessageBox.Show("اختر دوراً أولاً.", "الصلاحيات", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnSave.Enabled = false;
            try
            {
                var response = await _client.PostAsJsonAsync("RolePermissions/SaveRolePermissions", rows);
                if (!response.IsSuccessStatusCode)
                    throw new InvalidOperationException(await response.Content.ReadAsStringAsync());

                MessageBox.Show("تم حفظ الصلاحيات بنجاح.", "صلاحيات الأدوار", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر حفظ الصلاحيات: " + ex.Message, "صلاحيات الأدوار", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }

        private sealed class RoleRow
        {
            public int Role_ID { get; set; }
            public string Role_Name { get; set; } = "";
            public bool Is_Active { get; set; }
        }

        private sealed class ScreenRow
        {
            public int Screen_ID { get; set; }
            public string Screen_Name { get; set; } = "";
            public string Module_Name { get; set; } = "";
        }

        private sealed class PermissionRow
        {
            public int Role_ID { get; set; }
            public int Screen_ID { get; set; }
            public string Screen_Name { get; set; } = "";
            public string Module_Name { get; set; } = "";
            public bool Can_View { get; set; }
            public bool Can_Add { get; set; }
            public bool Can_Edit { get; set; }
            public bool Can_Delete { get; set; }
            public bool Can_Print { get; set; }
            public bool Can_Export { get; set; }
            public bool Can_Import { get; set; }
            public bool Can_Approve { get; set; }
            public bool Can_UnApprove { get; set; }

            public static PermissionRow From(ScreenRow screen, int roleId, PermissionRow? saved) => new()
            {
                Role_ID = roleId, Screen_ID = screen.Screen_ID, Screen_Name = screen.Screen_Name, Module_Name = screen.Module_Name,
                Can_View = saved?.Can_View ?? false, Can_Add = saved?.Can_Add ?? false, Can_Edit = saved?.Can_Edit ?? false,
                Can_Delete = saved?.Can_Delete ?? false, Can_Print = saved?.Can_Print ?? false, Can_Export = saved?.Can_Export ?? false,
                Can_Import = saved?.Can_Import ?? false, Can_Approve = saved?.Can_Approve ?? false, Can_UnApprove = saved?.Can_UnApprove ?? false
            };
        }
    }
}
