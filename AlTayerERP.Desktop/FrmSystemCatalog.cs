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
    /// شاشة عرض كتالوج النظام: الشاشات، الحقول، والأزرار المسجلة.
    /// التعديل مقصود أن يضاف بعد اعتماد المصادقة والصلاحيات الخادمية.
    /// </summary>
    public class FrmSystemCatalog : Form
    {
        private readonly HttpClient _client = ApiService.Client;
        private readonly string _baseUrl = ApiService.BaseUrl;

        private readonly DataGridView dgvScreens = CreateGrid();
        private readonly DataGridView dgvFields = CreateGrid();
        private readonly DataGridView dgvActions = CreateGrid();
        private readonly Label lblSelectedScreen = new();
        private readonly Button btnRefresh = new();

        public FrmSystemCatalog()
        {
            InitializeLayout();
            Load += async (_, _) => await LoadScreensAsync();
        }

        private void InitializeLayout()
        {
            Text = "كتالوج النظام";
            StartPosition = FormStartPosition.CenterParent;
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Tahoma", 10F);
            MinimumSize = new Size(980, 650);
            Width = 1180;
            Height = 760;

            btnRefresh.Text = "تحديث";
            btnRefresh.AutoSize = true;
            btnRefresh.Click += async (_, _) => await LoadScreensAsync();

            lblSelectedScreen.Dock = DockStyle.Fill;
            lblSelectedScreen.Text = "اختر شاشة لعرض الحقول والأزرار المعرفة لها.";
            lblSelectedScreen.TextAlign = ContentAlignment.MiddleRight;
            lblSelectedScreen.Padding = new Padding(8);

            var top = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 48,
                ColumnCount = 2,
                Padding = new Padding(8)
            };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            top.Controls.Add(btnRefresh, 0, 0);
            top.Controls.Add(lblSelectedScreen, 1, 0);

            var tabs = new TabControl { Dock = DockStyle.Fill, RightToLeftLayout = true };
            var tabScreens = new TabPage("الشاشات");
            var tabFields = new TabPage("الحقول");
            var tabActions = new TabPage("الأزرار والعمليات");

            tabScreens.Controls.Add(dgvScreens);
            tabFields.Controls.Add(dgvFields);
            tabActions.Controls.Add(dgvActions);
            tabs.TabPages.Add(tabScreens);
            tabs.TabPages.Add(tabFields);
            tabs.TabPages.Add(tabActions);

            Controls.Add(tabs);
            Controls.Add(top);

            dgvScreens.SelectionChanged += async (_, _) => await LoadSelectedScreenResourcesAsync();
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
                RightToLeft = RightToLeft.Yes
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
                lblSelectedScreen.Text = "اختر شاشة لعرض الحقول والأزرار المعرفة لها.";

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
            }
            catch (Exception ex)
            {
                dgvFields.DataSource = null;
                dgvActions.DataSource = null;
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