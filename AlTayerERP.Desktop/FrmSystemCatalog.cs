using AlTayerERP.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// كتالوج النظام: مصدر تعريف الشاشات وحقولها وأزرارها.
    /// </summary>
    public class FrmSystemCatalog : Form
    {
        private readonly HttpClient _client = ApiService.Client;
        private readonly string _baseUrl = ApiService.BaseUrl;

        private readonly DataGridView dgvScreens = CreateGrid();
        private readonly DataGridView dgvFields = CreateGrid();
        private readonly DataGridView dgvActions = CreateGrid();
        private readonly Label lblSelectedScreen = new();
        private readonly ToolStripButton btnRefresh = new("تحديث");
        private readonly ToolStripButton btnView = new("عرض الموارد");
        private readonly Label lblAuditUser = CreateAuditValue();
        private readonly Label lblAuditDate = CreateAuditValue();
        private readonly Label lblScreenCode = CreateAuditValue();
        private readonly Label lblStatus = CreateAuditValue();

        public FrmSystemCatalog()
        {
            BuildLayout();
            Load += async (_, _) => await LoadScreensAsync();
        }

        private void BuildLayout()
        {
            Text = "كتالوج النظام";
            StartPosition = FormStartPosition.CenterParent;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1050, 650);
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
                Text = "كتالوج النظام والصلاحيات",
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
                btnRefresh, new ToolStripSeparator(), btnView
            });
            btnRefresh.Click += async (_, _) => await LoadScreensAsync();
            btnView.Click += async (_, _) => await LoadSelectedScreenResourcesAsync();

            var tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                RightToLeftLayout = true
            };

            var screenTab = new TabPage("الشاشات") { BackColor = BackColor };
            var fieldTab = new TabPage("الحقول") { BackColor = BackColor };
            var actionTab = new TabPage("الأزرار والعمليات") { BackColor = BackColor };

            var screenContext = new Panel
            {
                Dock = DockStyle.Top,
                Height = 42,
                BackColor = Color.FromArgb(238, 238, 226),
                Padding = new Padding(8)
            };
            lblSelectedScreen.Dock = DockStyle.Fill;
            lblSelectedScreen.Text = "اختر شاشة من الجدول لعرض حقولها وأزرارها المعرفة.";
            lblSelectedScreen.TextAlign = ContentAlignment.MiddleRight;
            screenContext.Controls.Add(lblSelectedScreen);

            screenTab.Controls.Add(WrapGrid("الشاشات المعرفة", dgvScreens));
            fieldTab.Controls.Add(WrapGrid("حقول الشاشة المختارة", dgvFields));
            actionTab.Controls.Add(WrapGrid("أزرار وعمليات الشاشة المختارة", dgvActions));
            tabs.TabPages.Add(screenTab);
            tabs.TabPages.Add(fieldTab);
            tabs.TabPages.Add(actionTab);

            var audit = BuildAuditFooter();

            Controls.Add(tabs);
            Controls.Add(screenContext);
            Controls.Add(audit);
            Controls.Add(toolbar);
            Controls.Add(header);

            dgvScreens.SelectionChanged += async (_, _) => await LoadSelectedScreenResourcesAsync();
        }

        private static GroupBox WrapGrid(string caption, DataGridView grid)
        {
            var group = new GroupBox
            {
                Text = caption,
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };
            group.Controls.Add(grid);
            return group;
        }

        private Panel BuildAuditFooter()
        {
            var audit = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                BackColor = Color.FromArgb(232, 231, 255),
                BorderStyle = BorderStyle.FixedSingle
            };
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 8,
                RightToLeft = RightToLeft.Yes,
                Padding = new Padding(6, 8, 6, 4)
            };

            for (int i = 0; i < 8; i++)
                table.ColumnStyles.Add(new ColumnStyle(i % 2 == 0 ? SizeType.AutoSize : SizeType.Percent, i % 2 == 0 ? 0 : 25));

            AddAuditField(table, "المستخدم", lblAuditUser, 0);
            AddAuditField(table, "تاريخ العرض", lblAuditDate, 2);
            AddAuditField(table, "رمز الشاشة", lblScreenCode, 4);
            AddAuditField(table, "الحالة", lblStatus, 6);
            audit.Controls.Add(table);
            return audit;
        }

        private static Label CreateAuditValue()
        {
            return new Label
            {
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
        }

        private static void AddAuditField(TableLayoutPanel table, string label, Label value, int column)
        {
            table.Controls.Add(new Label
            {
                Text = label + ":",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(3)
            }, column, 0);
            table.Controls.Add(value, column + 1, 0);
        }

        private static DataGridView CreateGrid()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToOrderColumns = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                EnableHeadersVisualStyles = false,
                GridColor = Color.Gray,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(247, 247, 247)
                },
                RowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    SelectionBackColor = Color.FromArgb(206, 244, 246),
                    SelectionForeColor = Color.Black
                },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(225, 242, 246),
                    ForeColor = Color.Black,
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Font = new Font("Tahoma", 9F, FontStyle.Bold)
                }
            };
        }

        private async Task LoadScreensAsync()
        {
            try
            {
                btnRefresh.Enabled = false;
                List<SystemScreenCatalogItem>? screens =
                    await _client.GetFromJsonAsync<List<SystemScreenCatalogItem>>(
                        _baseUrl + "SystemCatalog/screens");

                dgvScreens.DataSource = screens ?? new List<SystemScreenCatalogItem>();
                dgvFields.DataSource = null;
                dgvActions.DataSource = null;
                lblSelectedScreen.Text = "اختر شاشة من الجدول لعرض حقولها وأزرارها المعرفة.";
                lblAuditUser.Text = CurrentSession.Full_Name;
                lblAuditDate.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                lblScreenCode.Text = string.Empty;
                lblStatus.Text = "جاهز";

                if (dgvScreens.Rows.Count > 0)
                    dgvScreens.Rows[0].Selected = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "تعذر تحميل كتالوج النظام.\n" + ex.Message,
                    "كتالوج النظام",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                lblStatus.Text = "تعذر التحميل";
            }
            finally
            {
                btnRefresh.Enabled = true;
            }
        }

        private async Task LoadSelectedScreenResourcesAsync()
        {
            if (dgvScreens.CurrentRow?.DataBoundItem is not SystemScreenCatalogItem screen)
                return;

            try
            {
                lblSelectedScreen.Text =
                    "الشاشة المختارة: " + screen.Screen_Name + " (" + screen.Screen_Code + ")";

                Task<List<SystemScreenFieldCatalogItem>?> fieldsTask =
                    _client.GetFromJsonAsync<List<SystemScreenFieldCatalogItem>>(
                        _baseUrl + "SystemCatalog/screens/" + screen.Screen_ID + "/fields");
                Task<List<SystemScreenActionCatalogItem>?> actionsTask =
                    _client.GetFromJsonAsync<List<SystemScreenActionCatalogItem>>(
                        _baseUrl + "SystemCatalog/screens/" + screen.Screen_ID + "/actions");

                await Task.WhenAll(fieldsTask, actionsTask);

                dgvFields.DataSource = fieldsTask.Result ?? new List<SystemScreenFieldCatalogItem>();
                dgvActions.DataSource = actionsTask.Result ?? new List<SystemScreenActionCatalogItem>();
                lblScreenCode.Text = screen.Screen_Code;
                lblStatus.Text = screen.Is_Active ? "نشط" : "موقوف";
            }
            catch (Exception ex)
            {
                dgvFields.DataSource = null;
                dgvActions.DataSource = null;
                lblStatus.Text = "تعذر التحميل";
                MessageBox.Show(
                    "تعذر تحميل موارد الشاشة.\n" + ex.Message,
                    "كتالوج النظام",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private sealed class SystemScreenCatalogItem
        {
            public int Screen_ID { get; set; }
            public string Screen_Code { get; set; } = string.Empty;
            public string Screen_Name { get; set; } = string.Empty;
            public string? Module_Name { get; set; }
            public bool Is_Active { get; set; }
            public int Sort_Order { get; set; }
        }

        private sealed class SystemScreenFieldCatalogItem
        {
            public long Screen_Field_ID { get; set; }
            public string Field_Code { get; set; } = string.Empty;
            public string Field_Name { get; set; } = string.Empty;
            public bool Is_Sensitive { get; set; }
            public bool Default_Required { get; set; }
            public bool Is_Active { get; set; }
            public int Sort_Order { get; set; }
        }

        private sealed class SystemScreenActionCatalogItem
        {
            public long Screen_Action_ID { get; set; }
            public string Action_Code { get; set; } = string.Empty;
            public string Action_Name { get; set; } = string.Empty;
            public bool Is_Sensitive { get; set; }
            public bool Requires_Reason { get; set; }
            public bool Is_Active { get; set; }
            public int Sort_Order { get; set; }
        }
    }
}