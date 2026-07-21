using AlTayerERP.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// إدارة صلاحيات الأدوار على مستوى الشاشة والعملية.
    /// تمنع الواجهة الصلاحيات المتناقضة، ويبقى التحقق النهائي في API.
    /// </summary>
    public sealed class FrmRolePermissions : Form
    {
        private readonly HttpClient _client = ApiService.Client;
        private readonly ComboBox cmbRoles = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 270 };
        private readonly TextBox txtSearch = new() { Width = 280, PlaceholderText = "ابحث باسم الشاشة أو الوحدة…" };
        private readonly DataGridView dgvPermissions = new()
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false
        };
        private readonly Button btnSave = new();
        private readonly Button btnRefresh = new();
        private readonly Button btnGrantViewAll = new();
        private readonly Button btnClearAll = new();
        private readonly Label lblRecordCount = new() { AutoSize = false, Width = 150 };
        private bool _normalizingPermissions;

        public FrmRolePermissions()
        {
            Text = "صلاحيات الأدوار";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1240;
            Height = 720;
            MinimumSize = new Size(980, 630);
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Segoe UI", 9.5F);
            BackColor = Color.FromArgb(244, 247, 251);
            KeyPreview = true;

            var shell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(14),
                BackColor = BackColor
            };
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

            shell.Controls.Add(CreateHeader(), 0, 0);
            shell.Controls.Add(CreateToolbar(), 0, 1);
            shell.Controls.Add(CreateSearchPanel(), 0, 2);
            shell.Controls.Add(CreateGridCard(), 0, 3);
            shell.Controls.Add(CreateFooter(), 0, 4);
            Controls.Add(shell);

            AddTextColumn("Module_Name", "النظام / الوحدة", 160);
            AddTextColumn("Screen_Name", "الشاشة", 240);
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
            txtSearch.TextChanged += (_, _) => FilterRows();
            dgvPermissions.CurrentCellDirtyStateChanged += (_, _) =>
            {
                if (dgvPermissions.IsCurrentCellDirty)
                    dgvPermissions.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            dgvPermissions.CellValueChanged += dgvPermissions_CellValueChanged;
            KeyDown += FrmRolePermissions_KeyDown;
        }

        private Control CreateHeader()
        {
            var header = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(27, 62, 104),
                Padding = new Padding(20, 10, 20, 10),
                Margin = new Padding(0, 0, 0, 8)
            };
            header.Controls.Add(new Label
            {
                Text = "صلاحيات الأدوار",
                Dock = DockStyle.Top,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Height = 33,
                TextAlign = ContentAlignment.MiddleRight
            });
            header.Controls.Add(new Label
            {
                Text = "امنح أقل قدر من الصلاحيات اللازم للعمل. أي عملية تتطلب حق العرض للشاشة.",
                Dock = DockStyle.Bottom,
                ForeColor = Color.FromArgb(220, 232, 247),
                Font = new Font("Segoe UI", 8.5F),
                Height = 20,
                TextAlign = ContentAlignment.MiddleRight
            });
            return header;
        }

        private Control CreateToolbar()
        {
            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(8),
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Margin = new Padding(0, 0, 0, 6)
            };

            toolbar.Controls.Add(new Label
            {
                Text = "الدور:",
                Width = 52,
                Height = 32,
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Color.FromArgb(55, 65, 81),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            });
            toolbar.Controls.Add(cmbRoles);

            btnSave = CreateButton("حفظ  Ctrl+S", Color.FromArgb(22, 125, 84));
            btnGrantViewAll = CreateButton("منح العرض", Color.FromArgb(36, 99, 168));
            btnClearAll = CreateButton("إلغاء الكل", Color.FromArgb(180, 83, 9));
            btnRefresh = CreateButton("تحديث  F5", Color.FromArgb(75, 85, 99));

            toolbar.Controls.Add(btnSave);
            toolbar.Controls.Add(btnGrantViewAll);
            toolbar.Controls.Add(btnClearAll);
            toolbar.Controls.Add(btnRefresh);
            return toolbar;
        }

        private Control CreateSearchPanel()
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10, 7, 10, 7),
                Margin = new Padding(0, 0, 0, 6)
            };
            txtSearch.Dock = DockStyle.Fill;
            txtSearch.BorderStyle = BorderStyle.FixedSingle;
            card.Controls.Add(txtSearch);
            card.Controls.Add(new Label
            {
                Text = "بحث",
                Dock = DockStyle.Right,
                Width = 60,
                ForeColor = Color.FromArgb(55, 65, 81),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight
            });
            return card;
        }

        private Control CreateGridCard()
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(1)
            };

            dgvPermissions.EnableHeadersVisualStyles = false;
            dgvPermissions.BackgroundColor = Color.White;
            dgvPermissions.BorderStyle = BorderStyle.None;
            dgvPermissions.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvPermissions.GridColor = Color.FromArgb(226, 232, 240);
            dgvPermissions.RowTemplate.Height = 34;
            dgvPermissions.ColumnHeadersHeight = 40;
            dgvPermissions.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(232, 239, 248),
                ForeColor = Color.FromArgb(31, 58, 92),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                WrapMode = DataGridViewTriState.True
            };
            dgvPermissions.DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                SelectionBackColor = Color.FromArgb(218, 232, 247),
                SelectionForeColor = Color.FromArgb(20, 44, 75)
            };
            dgvPermissions.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            card.Controls.Add(dgvPermissions);
            return card;
        }

        private Control CreateFooter()
        {
            var footer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4, 4, 4, 0) };
            lblRecordCount.Dock = DockStyle.Right;
            lblRecordCount.TextAlign = ContentAlignment.MiddleRight;
            lblRecordCount.ForeColor = Color.FromArgb(75, 85, 99);
            lblRecordCount.Text = "عدد الشاشات: 0";
            footer.Controls.Add(lblRecordCount);
            footer.Controls.Add(new Label
            {
                Text = "تُحفظ التغييرات لمدير النظام فقط.",
                Dock = DockStyle.Left,
                ForeColor = Color.FromArgb(75, 85, 99),
                TextAlign = ContentAlignment.MiddleLeft
            });
            return footer;
        }

        private static Button CreateButton(string text, Color color) => new()
        {
            Text = text,
            Width = 120,
            Height = 32,
            Margin = new Padding(4, 0, 4, 0),
            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 0 },
            BackColor = color,
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
        };

        private void AddTextColumn(string property, string title, int width) =>
            dgvPermissions.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = property,
                HeaderText = title,
                Width = width,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight }
            });

        private void AddCheckColumn(string property, string title) =>
            dgvPermissions.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = property,
                HeaderText = title,
                Width = 76
            });

        /// <summary>
        /// تطبيق صلاحية العرض على الصفوف كلها لتسهيل التهيئة الأولية للدور.
        /// عند الإلغاء تُلغى كل عمليات الشاشة لتبقى الصلاحيات متسقة.
        /// </summary>
        private void SetAllViewPermissions(bool canView)
        {
            if (dgvPermissions.DataSource is not List<PermissionRow> rows)
                return;

            _normalizingPermissions = true;
            try
            {
                foreach (var row in rows)
                {
                    row.Can_View = canView;
                    if (!canView)
                        row.ClearActions();
                }
            }
            finally
            {
                _normalizingPermissions = false;
            }

            dgvPermissions.Refresh();
        }

        private async Task LoadRolesAsync()
        {
            try
            {
                UseWaitCursor = true;
                var roles = await _client.GetFromJsonAsync<List<RoleRow>>("Roles") ?? new();
                cmbRoles.DataSource = roles.Where(x => x.Is_Active).ToList();
                cmbRoles.DisplayMember = nameof(RoleRow.Role_Name);
                cmbRoles.ValueMember = nameof(RoleRow.Role_ID);
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر تحميل الأدوار. تحقق من اتصال API والصلاحيات.\n\n" + ex.Message,
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private async Task LoadPermissionsAsync()
        {
            if (cmbRoles.SelectedValue is not int roleId || roleId <= 0)
                return;

            try
            {
                UseWaitCursor = true;
                var screens = await _client.GetFromJsonAsync<List<ScreenRow>>("RolePermissions/GetScreens") ?? new();
                var saved = await _client.GetFromJsonAsync<List<PermissionRow>>(
                    $"RolePermissions/GetRolePermissions/{roleId}") ?? new();

                var rows = screens.Select(screen =>
                {
                    var permission = saved.FirstOrDefault(x => x.Screen_ID == screen.Screen_ID);
                    return PermissionRow.From(screen, roleId, permission);
                }).ToList();

                dgvPermissions.DataSource = rows;
                lblRecordCount.Text = $"عدد الشاشات: {rows.Count}";
                FilterRows();
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر تحميل صلاحيات الدور.\n\n" + ex.Message,
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private async Task SavePermissionsAsync()
        {
            if (!CurrentSession.Is_System_Admin)
            {
                MessageBox.Show("حفظ الصلاحيات مخصص لمدير النظام.", "الصلاحيات",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgvPermissions.DataSource is not List<PermissionRow> rows || rows.Count == 0)
            {
                MessageBox.Show("اختر دوراً أولاً.", "الصلاحيات",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnSave.Enabled = false;
            try
            {
                var response = await _client.PostAsJsonAsync("RolePermissions/SaveRolePermissions", rows);
                if (!response.IsSuccessStatusCode)
                    throw new InvalidOperationException(await response.Content.ReadAsStringAsync());

                MessageBox.Show("تم حفظ الصلاحيات بنجاح.", "الصلاحيات",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر حفظ الصلاحيات.\n\n" + ex.Message, "الصلاحيات",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }

        private void FilterRows()
        {
            var query = txtSearch.Text.Trim();
            foreach (DataGridViewRow row in dgvPermissions.Rows)
            {
                row.Visible = string.IsNullOrWhiteSpace(query) ||
                    row.Cells.Cast<DataGridViewCell>().Take(2)
                        .Any(cell => (cell.Value?.ToString() ?? string.Empty)
                            .IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0);
            }
        }

        private void dgvPermissions_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (_normalizingPermissions || e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (dgvPermissions.Rows[e.RowIndex].DataBoundItem is not PermissionRow row)
                return;

            var property = dgvPermissions.Columns[e.ColumnIndex].DataPropertyName;
            if (string.IsNullOrWhiteSpace(property))
                return;

            _normalizingPermissions = true;
            try
            {
                if (property == nameof(PermissionRow.Can_View) && !row.Can_View)
                {
                    row.ClearActions();
                }
                else if (property != nameof(PermissionRow.Can_View) &&
                         property.StartsWith("Can_", StringComparison.Ordinal) &&
                         GetActionValue(row, property))
                {
                    row.Can_View = true;
                }
            }
            finally
            {
                _normalizingPermissions = false;
            }

            dgvPermissions.Refresh();
        }

        private static bool GetActionValue(PermissionRow row, string property) => property switch
        {
            nameof(PermissionRow.Can_Add) => row.Can_Add,
            nameof(PermissionRow.Can_Edit) => row.Can_Edit,
            nameof(PermissionRow.Can_Delete) => row.Can_Delete,
            nameof(PermissionRow.Can_Print) => row.Can_Print,
            nameof(PermissionRow.Can_Export) => row.Can_Export,
            nameof(PermissionRow.Can_Import) => row.Can_Import,
            nameof(PermissionRow.Can_Approve) => row.Can_Approve,
            nameof(PermissionRow.Can_UnApprove) => row.Can_UnApprove,
            _ => false
        };

        private void FrmRolePermissions_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                _ = SavePermissionsAsync();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F5)
            {
                _ = LoadRolesAsync();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.F)
            {
                txtSearch.Focus();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Close();
                e.SuppressKeyPress = true;
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

            public void ClearActions()
            {
                Can_Add = false;
                Can_Edit = false;
                Can_Delete = false;
                Can_Print = false;
                Can_Export = false;
                Can_Import = false;
                Can_Approve = false;
                Can_UnApprove = false;
            }

            public static PermissionRow From(ScreenRow screen, int roleId, PermissionRow? saved) => new()
            {
                Role_ID = roleId,
                Screen_ID = screen.Screen_ID,
                Screen_Name = screen.Screen_Name,
                Module_Name = screen.Module_Name,
                Can_View = saved?.Can_View ?? false,
                Can_Add = saved?.Can_Add ?? false,
                Can_Edit = saved?.Can_Edit ?? false,
                Can_Delete = saved?.Can_Delete ?? false,
                Can_Print = saved?.Can_Print ?? false,
                Can_Export = saved?.Can_Export ?? false,
                Can_Import = saved?.Can_Import ?? false,
                Can_Approve = saved?.Can_Approve ?? false,
                Can_UnApprove = saved?.Can_UnApprove ?? false
            };
        }
    }
}
