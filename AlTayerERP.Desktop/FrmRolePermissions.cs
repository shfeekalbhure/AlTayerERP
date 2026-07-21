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
    /// إدارة صلاحيات الدور على مستوى الشاشات والعمليات الأساسية.
    /// </summary>
    public class FrmRolePermissions : Form
    {
        private readonly HttpClient _client = ApiService.Client;
        private readonly string _baseUrl = ApiService.BaseUrl;
        private readonly ComboBox cmbRoles = new();
        private readonly DataGridView dgvPermissions = CreatePermissionsGrid();
        private readonly ToolStripButton btnRefresh = new("تحديث");
        private readonly ToolStripButton btnSave = new("حفظ الصلاحيات");
        private readonly Label lblStatus = CreateAuditValue();
        private readonly Label lblUser = CreateAuditValue();
        private List<RoleItem> _roles = new();

        public FrmRolePermissions()
        {
            BuildLayout();
            Load += async (_, _) => await LoadRolesAsync();
        }

        private void BuildLayout()
        {
            Text = "صلاحيات الأدوار";
            StartPosition = FormStartPosition.CenterParent;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1120, 680);
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Tahoma", 9F);
            BackColor = Color.FromArgb(245, 245, 240);

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 42,
                BackColor = Color.FromArgb(52, 123, 177)
            };
            header.Controls.Add(new Label
            {
                Text = "صلاحيات الأدوار والشاشات",
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font("Tahoma", 14F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            });

            var toolbar = new ToolStrip
            {
                Dock = DockStyle.Top,
                Height = 36,
                GripStyle = ToolStripGripStyle.Hidden,
                RightToLeft = RightToLeft.Yes,
                BackColor = BackColor,
                RenderMode = ToolStripRenderMode.System
            };
            toolbar.Items.AddRange(new ToolStripItem[]
            {
                btnRefresh, new ToolStripSeparator(), btnSave
            });
            btnRefresh.Click += async (_, _) => await LoadRolesAsync();
            btnSave.Click += async (_, _) => await SaveAsync();

            cmbRoles.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRoles.BackColor = Color.FromArgb(255, 255, 224);
            cmbRoles.SelectedIndexChanged += async (_, _) => await LoadPermissionsAsync();

            var selector = new GroupBox
            {
                Text = "اختيار الدور",
                Dock = DockStyle.Top,
                Height = 78,
                Padding = new Padding(12)
            };
            var selectorLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RightToLeft = RightToLeft.Yes
            };
            selectorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            selectorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            selectorLayout.Controls.Add(new Label
            {
                Text = "الدور:",
                AutoSize = true,
                Padding = new Padding(5),
                TextAlign = ContentAlignment.MiddleRight
            }, 0, 0);
            selectorLayout.Controls.Add(cmbRoles, 1, 0);
            selector.Controls.Add(selectorLayout);

            var tabs = new TabControl { Dock = DockStyle.Fill, RightToLeftLayout = true };
            var screenTab = new TabPage("صلاحيات الشاشات والعمليات") { BackColor = BackColor };
            var screenGroup = new GroupBox
            {
                Text = "حدد العمليات المسموحة لكل شاشة",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };
            screenGroup.Controls.Add(dgvPermissions);
            screenTab.Controls.Add(screenGroup);

            var noteTab = new TabPage("الحقول والأزرار الدقيقة") { BackColor = BackColor };
            noteTab.Controls.Add(new Label
            {
                Text = "تظهر هنا صلاحيات الحقول والأزرار الدقيقة بعد اختيار الشاشة من كتالوج النظام.",
                Dock = DockStyle.Top,
                Padding = new Padding(14),
                TextAlign = ContentAlignment.MiddleRight
            });
            tabs.TabPages.Add(screenTab);
            tabs.TabPages.Add(noteTab);

            Controls.Add(tabs);
            Controls.Add(selector);
            Controls.Add(BuildAuditFooter());
            Controls.Add(toolbar);
            Controls.Add(header);
        }

        private Panel BuildAuditFooter()
        {
            var footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                BackColor = Color.FromArgb(232, 231, 255),
                BorderStyle = BorderStyle.FixedSingle
            };
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RightToLeft = RightToLeft.Yes,
                Padding = new Padding(6, 8, 6, 4)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            AddAuditField(table, "المستخدم", lblUser, 0);
            AddAuditField(table, "الحالة", lblStatus, 2);
            footer.Controls.Add(table);
            return footer;
        }

        private static void AddAuditField(TableLayoutPanel table, string caption, Label value, int column)
        {
            table.Controls.Add(new Label
            {
                Text = caption + ":",
                AutoSize = true,
                Padding = new Padding(3),
                TextAlign = ContentAlignment.MiddleRight
            }, column, 0);
            table.Controls.Add(value, column + 1, 0);
        }

        private static Label CreateAuditValue()
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };
        }

        private static DataGridView CreatePermissionsGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToOrderColumns = false,
                AutoGenerateColumns = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                EnableHeadersVisualStyles = false,
                GridColor = Color.Gray
            };

            grid.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(206, 244, 246);
            grid.RowsDefaultCellStyle.SelectionForeColor = Color.Black;
            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(225, 242, 246),
                ForeColor = Color.Black,
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = new Font("Tahoma", 8F, FontStyle.Bold)
            };

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Module_Name", HeaderText = "القسم", DataPropertyName = "Module_Name",
                ReadOnly = true, Width = 90
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Screen_Name", HeaderText = "الشاشة", DataPropertyName = "Screen_Name",
                ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Can_View", HeaderText = "عرض", DataPropertyName = "Can_View", Width = 45 });
            grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Can_Add", HeaderText = "إضافة", DataPropertyName = "Can_Add", Width = 50 });
            grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Can_Edit", HeaderText = "تعديل", DataPropertyName = "Can_Edit", Width = 50 });
            grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Can_Delete", HeaderText = "حذف", DataPropertyName = "Can_Delete", Width = 45 });
            grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Can_Print", HeaderText = "طباعة", DataPropertyName = "Can_Print", Width = 50 });
            grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Can_Export", HeaderText = "تصدير", DataPropertyName = "Can_Export", Width = 50 });
            grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Can_Import", HeaderText = "استيراد", DataPropertyName = "Can_Import", Width = 55 });
            grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Can_Approve", HeaderText = "اعتماد", DataPropertyName = "Can_Approve", Width = 55 });
            grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Can_UnApprove", HeaderText = "فك اعتماد", DataPropertyName = "Can_UnApprove", Width = 60 });
            return grid;
        }

        private async Task LoadRolesAsync()
        {
            try
            {
                btnRefresh.Enabled = false;
                _roles = await _client.GetFromJsonAsync<List<RoleItem>>(_baseUrl + "Roles")
                    ?? new List<RoleItem>();
                cmbRoles.DataSource = _roles.Where(x => x.Is_Active).ToList();
                cmbRoles.DisplayMember = "Role_Name";
                cmbRoles.ValueMember = "Role_ID";
                lblUser.Text = CurrentSession.Full_Name;
                lblStatus.Text = "جاهز";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "تعذر التحميل";
                MessageBox.Show("تعذر تحميل الأدوار.\n" + ex.Message, "الصلاحيات",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRefresh.Enabled = true;
            }
        }

        private async Task LoadPermissionsAsync()
        {
            if (cmbRoles.SelectedValue is not int roleId || roleId <= 0)
                return;

            try
            {
                var permissions = await _client.GetFromJsonAsync<List<RoleScreenPermissionItem>>(
                    _baseUrl + "Roles/" + roleId + "/permissions");
                dgvPermissions.DataSource = permissions ?? new List<RoleScreenPermissionItem>();
                lblStatus.Text = "تم تحميل الصلاحيات";
            }
            catch (Exception ex)
            {
                dgvPermissions.DataSource = null;
                lblStatus.Text = "تعذر تحميل الصلاحيات";
                MessageBox.Show("تعذر تحميل صلاحيات الدور.\n" + ex.Message, "الصلاحيات",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task SaveAsync()
        {
            if (!CurrentSession.Is_System_Admin || cmbRoles.SelectedValue is not int roleId || roleId <= 0)
                return;

            dgvPermissions.EndEdit();
            var items = dgvPermissions.DataSource as List<RoleScreenPermissionItem>;
            if (items == null)
                return;

            try
            {
                btnSave.Enabled = false;
                using HttpResponseMessage response = await _client.PutAsJsonAsync(
                    _baseUrl + "Roles/" + roleId + "/permissions", items);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show(await response.Content.ReadAsStringAsync(), "تعذر الحفظ",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                lblStatus.Text = "تم حفظ الصلاحيات";
                MessageBox.Show("تم حفظ صلاحيات الدور بنجاح.", "الصلاحيات",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "تعذر الحفظ";
                MessageBox.Show("تعذر حفظ صلاحيات الدور.\n" + ex.Message, "الصلاحيات",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSave.Enabled = CurrentSession.Is_System_Admin;
            }
        }

        private sealed class RoleItem
        {
            public int Role_ID { get; set; }
            public string Role_Name { get; set; } = string.Empty;
            public bool Is_Active { get; set; }
        }

        private sealed class RoleScreenPermissionItem
        {
            public int Screen_ID { get; set; }
            public string Screen_Code { get; set; } = string.Empty;
            public string Screen_Name { get; set; } = string.Empty;
            public string Module_Name { get; set; } = string.Empty;
            public bool Can_View { get; set; }
            public bool Can_Add { get; set; }
            public bool Can_Edit { get; set; }
            public bool Can_Delete { get; set; }
            public bool Can_Print { get; set; }
            public bool Can_Export { get; set; }
            public bool Can_Import { get; set; }
            public bool Can_Approve { get; set; }
            public bool Can_UnApprove { get; set; }
        }
    }
}