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
    /// إدارة الاستثناءات الدقيقة للمستخدم فوق الصلاحيات الموروثة من دوره.
    /// </summary>
    public class FrmUserResourcePermissions : Form
    {
        private readonly HttpClient _client = ApiService.Client;
        private readonly string _baseUrl = ApiService.BaseUrl;
        private readonly ComboBox cmbUsers = CreateSelector();
        private readonly ComboBox cmbScreens = CreateSelector();
        private readonly DataGridView dgvFields = CreateGrid(true);
        private readonly DataGridView dgvActions = CreateGrid(false);
        private readonly ToolStripButton btnRefresh = new("تحديث");
        private readonly ToolStripButton btnSave = new("حفظ الاستثناءات");
        private readonly Label lblStatus = CreateAuditValue();
        private List<UserItem> _users = new();
        private List<ScreenItem> _screens = new();

        public FrmUserResourcePermissions()
        {
            BuildLayout();
            Load += async (_, _) => await LoadSelectorsAsync();
        }

        private void BuildLayout()
        {
            Text = "صلاحيات المستخدم الدقيقة";
            StartPosition = FormStartPosition.CenterParent;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1050, 650);
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Tahoma", 9F);
            BackColor = Color.FromArgb(245, 245, 240);

            var header = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = Color.FromArgb(52, 123, 177) };
            header.Controls.Add(new Label
            {
                Text = "صلاحيات المستخدم: الحقول والأزرار",
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
                BackColor = BackColor
            };
            toolbar.Items.AddRange(new ToolStripItem[] { btnRefresh, new ToolStripSeparator(), btnSave });
            btnRefresh.Click += async (_, _) => await LoadSelectorsAsync();
            btnSave.Click += async (_, _) => await SaveAsync();

            cmbUsers.SelectedIndexChanged += async (_, _) => await LoadPermissionsAsync();
            cmbScreens.SelectedIndexChanged += async (_, _) => await LoadPermissionsAsync();

            var selector = new GroupBox { Text = "نطاق الاستثناء", Dock = DockStyle.Top, Height = 82, Padding = new Padding(12) };
            var selectorTable = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RightToLeft = RightToLeft.Yes };
            selectorTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            selectorTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            selectorTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            selectorTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            AddSelector(selectorTable, "المستخدم", cmbUsers, 0);
            AddSelector(selectorTable, "الشاشة", cmbScreens, 2);
            selector.Controls.Add(selectorTable);

            var hint = new Label
            {
                Text = "وراثة: يتبع الدور. سماح: يمنح حقاً إضافياً. منع: يلغي هذا الحق للمستخدم حتى لو كان دوره يسمح به.",
                Dock = DockStyle.Top,
                Height = 34,
                Padding = new Padding(10),
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = Color.FromArgb(238, 238, 226)
            };

            var tabs = new TabControl { Dock = DockStyle.Fill, RightToLeftLayout = true };
            var fieldsTab = new TabPage("الحقول") { BackColor = BackColor };
            var actionsTab = new TabPage("الأزرار والإجراءات") { BackColor = BackColor };
            fieldsTab.Controls.Add(dgvFields);
            actionsTab.Controls.Add(dgvActions);
            tabs.TabPages.Add(fieldsTab);
            tabs.TabPages.Add(actionsTab);

            Controls.Add(tabs);
            Controls.Add(hint);
            Controls.Add(selector);
            Controls.Add(BuildFooter());
            Controls.Add(toolbar);
            Controls.Add(header);
        }

        private static ComboBox CreateSelector()
        {
            return new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(255, 255, 224)
            };
        }

        private static void AddSelector(TableLayoutPanel table, string caption, Control control, int column)
        {
            table.Controls.Add(new Label
            {
                Text = caption + ":",
                AutoSize = true,
                Padding = new Padding(4),
                TextAlign = ContentAlignment.MiddleRight
            }, column, 0);
            table.Controls.Add(control, column + 1, 0);
        }

        private Panel BuildFooter()
        {
            var footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(232, 231, 255),
                BorderStyle = BorderStyle.FixedSingle
            };
            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RightToLeft = RightToLeft.Yes, Padding = new Padding(7) };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            table.Controls.Add(new Label { Text = "الحالة:", AutoSize = true, Padding = new Padding(3) }, 0, 0);
            table.Controls.Add(lblStatus, 1, 0);
            footer.Controls.Add(table);
            return footer;
        }

        private static Label CreateAuditValue()
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleCenter
            };
        }

        private static DataGridView CreateGrid(bool fields)
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToOrderColumns = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
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
                HeaderText = fields ? "الحقل" : "الزر أو الإجراء",
                DataPropertyName = "Resource_Name",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            if (fields)
            {
                grid.Columns.Add(CreateModeColumn("View_Mode", "الرؤية"));
                grid.Columns.Add(CreateModeColumn("Edit_Mode", "التعديل"));
            }
            else
            {
                grid.Columns.Add(CreateModeColumn("Execute_Mode", "التنفيذ"));
            }
            return grid;
        }

        private static DataGridViewComboBoxColumn CreateModeColumn(string property, string caption)
        {
            return new DataGridViewComboBoxColumn
            {
                HeaderText = caption,
                DataPropertyName = property,
                DataSource = new[] { "INHERIT", "ALLOW", "DENY" },
                Width = 140,
                FlatStyle = FlatStyle.Flat
            };
        }

        private async Task LoadSelectorsAsync()
        {
            try
            {
                btnRefresh.Enabled = false;
                _users = await _client.GetFromJsonAsync<List<UserItem>>(_baseUrl + "UserResourcePermissions/users")
                    ?? new List<UserItem>();
                _screens = await _client.GetFromJsonAsync<List<ScreenItem>>(_baseUrl + "SystemCatalog/screens")
                    ?? new List<ScreenItem>();

                cmbUsers.DataSource = _users;
                cmbUsers.DisplayMember = nameof(UserItem.Display_Name);
                cmbUsers.ValueMember = nameof(UserItem.User_ID);
                cmbScreens.DataSource = _screens.Where(x => x.Is_Active).ToList();
                cmbScreens.DisplayMember = nameof(ScreenItem.Display_Name);
                cmbScreens.ValueMember = nameof(ScreenItem.Screen_ID);
                lblStatus.Text = "جاهز";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "تعذر تحميل بيانات الاختيار";
                MessageBox.Show("تعذر تحميل المستخدمين أو الشاشات.\n" + ex.Message, "الصلاحيات",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRefresh.Enabled = true;
            }
        }

        private async Task LoadPermissionsAsync()
        {
            if (cmbUsers.SelectedValue is not int userId || userId <= 0 ||
                cmbScreens.SelectedValue is not int screenId || screenId <= 0)
                return;

            try
            {
                var response = await _client.GetFromJsonAsync<UserResourceResponse>(
                    _baseUrl + "UserResourcePermissions/" + userId + "/resource-permissions/" + screenId);

                dgvFields.DataSource = response?.Fields ?? new List<ResourceItem>();
                dgvActions.DataSource = response?.Actions ?? new List<ResourceItem>();
                lblStatus.Text = "تم تحميل الصلاحيات الموروثة والاستثناءات";
            }
            catch (Exception ex)
            {
                dgvFields.DataSource = null;
                dgvActions.DataSource = null;
                lblStatus.Text = "تعذر تحميل الصلاحيات";
                MessageBox.Show("تعذر تحميل استثناءات المستخدم.\n" + ex.Message, "الصلاحيات",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task SaveAsync()
        {
            if (cmbUsers.SelectedValue is not int userId || cmbScreens.SelectedValue is not int screenId)
                return;

            try
            {
                btnSave.Enabled = false;
                dgvFields.EndEdit();
                dgvActions.EndEdit();
                var request = new UserResourceResponse
                {
                    Screen_ID = screenId,
                    Fields = (dgvFields.DataSource as List<ResourceItem>) ?? new List<ResourceItem>(),
                    Actions = (dgvActions.DataSource as List<ResourceItem>) ?? new List<ResourceItem>()
                };

                using var response = await _client.PutAsJsonAsync(
                    _baseUrl + "UserResourcePermissions/" + userId + "/resource-permissions/" + screenId, request);
                if (!response.IsSuccessStatusCode)
                {
                    string message = await response.Content.ReadAsStringAsync();
                    throw new InvalidOperationException(string.IsNullOrWhiteSpace(message) ? "رفض الخادم حفظ الصلاحيات." : message);
                }

                lblStatus.Text = "تم الحفظ";
                MessageBox.Show("تم حفظ استثناءات المستخدم بنجاح.", "الصلاحيات",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "فشل الحفظ";
                MessageBox.Show("تعذر حفظ الصلاحيات.\n" + ex.Message, "الصلاحيات",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }

        private sealed class UserItem
        {
            public int User_ID { get; set; }
            public string Full_Name { get; set; } = string.Empty;
            public string Login_Name { get; set; } = string.Empty;
            public string Display_Name => Full_Name + " - " + Login_Name;
        }

        private sealed class ScreenItem
        {
            public int Screen_ID { get; set; }
            public string Screen_Name { get; set; } = string.Empty;
            public string Module_Name { get; set; } = string.Empty;
            public bool Is_Active { get; set; }
            public string Display_Name => Module_Name + " - " + Screen_Name;
        }

        private sealed class UserResourceResponse
        {
            public int Screen_ID { get; set; }
            public List<ResourceItem> Fields { get; set; } = new();
            public List<ResourceItem> Actions { get; set; } = new();
        }

        private sealed class ResourceItem
        {
            public string Resource_Type { get; set; } = string.Empty;
            public string Resource_Code { get; set; } = string.Empty;
            public string Resource_Name { get; set; } = string.Empty;
            public string View_Mode { get; set; } = "INHERIT";
            public string Edit_Mode { get; set; } = "INHERIT";
            public string Execute_Mode { get; set; } = "INHERIT";
        }
    }
}