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
    /// شاشة إدارة الإعدادات العامة ومتعددة النطاق.
    /// تحفظ التجاوزات كسجل جديد ولا تحذف القيم السابقة حفاظاً على الأثر الرقابي.
    /// </summary>
    public class FrmSettings : Form
    {
        private readonly HttpClient _client = ApiService.Client;
        private readonly string _baseUrl = ApiService.BaseUrl;

        private readonly DataGridView dgvSettings = CreateGrid();
        private readonly DataGridView dgvScopeValues = CreateGrid();
        private readonly TextBox txtSearch = new();
        private readonly Label lblSettingName = CreateValueLabel();
        private readonly Label lblSettingCode = CreateValueLabel();
        private readonly Label lblDataType = CreateValueLabel();
        private readonly Label lblDefaultValue = CreateValueLabel();
        private readonly ComboBox cmbScopeType = new();
        private readonly TextBox txtScopeId = new();
        private readonly TextBox txtValue = new();
        private readonly TextBox txtReason = new();
        private readonly DateTimePicker dtFrom = new();
        private readonly DateTimePicker dtTo = new();

        private readonly ToolStripButton btnNewOverride = new("تجاوز جديد");
        private readonly ToolStripButton btnSave = new("حفظ");
        private readonly ToolStripButton btnDeactivate = new("تعطيل");
        private readonly ToolStripButton btnRefresh = new("تحديث");
        private readonly ToolStripButton btnSearch = new("بحث");

        private readonly Label lblRecordUser = CreateValueLabel();
        private readonly Label lblRecordDate = CreateValueLabel();
        private readonly Label lblStatus = CreateValueLabel();

        private List<SettingItem> _settings = new();
        private long _selectedSettingId;
        private long _selectedScopeValueId;

        public FrmSettings()
        {
            BuildLayout();
            ConfigurePermissions();
            Load += async (_, _) => await LoadSettingsAsync();
        }

        private void BuildLayout()
        {
            Text = "إدارة الإعدادات";
            StartPosition = FormStartPosition.CenterParent;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1100, 700);
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
                Text = "إدارة الإعدادات والتهيئة",
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
                btnNewOverride, new ToolStripSeparator(), btnSave, new ToolStripSeparator(),
                btnDeactivate, new ToolStripSeparator(), btnRefresh, new ToolStripSeparator(), btnSearch
            });

            btnNewOverride.Click += (_, _) => ClearOverrideEditor();
            btnSave.Click += async (_, _) => await SaveOverrideAsync();
            btnDeactivate.Click += async (_, _) => await DeactivateOverrideAsync();
            btnRefresh.Click += async (_, _) => await LoadSettingsAsync();
            btnSearch.Click += (_, _) => txtSearch.Focus();

            var tabs = new TabControl { Dock = DockStyle.Fill, RightToLeftLayout = true };
            var catalogTab = new TabPage("كتالوج الإعدادات") { BackColor = BackColor };
            var valuesTab = new TabPage("قيم الإعداد والنطاقات") { BackColor = BackColor };

            catalogTab.Controls.Add(BuildCatalogLayout());
            valuesTab.Controls.Add(BuildValuesLayout());
            tabs.TabPages.Add(catalogTab);
            tabs.TabPages.Add(valuesTab);

            Controls.Add(tabs);
            Controls.Add(BuildAuditFooter());
            Controls.Add(toolbar);
            Controls.Add(header);

            txtSearch.TextChanged += (_, _) => ApplySearch();
            dgvSettings.SelectionChanged += async (_, _) => await LoadSelectedSettingAsync();
            dgvScopeValues.SelectionChanged += (_, _) => LoadSelectedScopeValue();
            cmbScopeType.SelectedIndexChanged += (_, _) => ApplyScopeTypeDefault();
        }

        private Control BuildCatalogLayout()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            var searchPanel = new Panel { Dock = DockStyle.Top, Height = 34 };
            searchPanel.Controls.Add(new Label
            {
                Text = "بحث في الكود أو الاسم:",
                Dock = DockStyle.Right,
                Width = 140,
                TextAlign = ContentAlignment.MiddleRight
            });
            txtSearch.Dock = DockStyle.Fill;
            txtSearch.BackColor = InputBackColor;
            searchPanel.Controls.Add(txtSearch);

            var group = new GroupBox
            {
                Text = "الإعدادات المعرفة",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };
            group.Controls.Add(dgvSettings);
            panel.Controls.Add(group);
            panel.Controls.Add(searchPanel);
            return panel;
        }

        private Control BuildValuesLayout()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            var settingInfo = new GroupBox
            {
                Text = "الإعداد المختار",
                Dock = DockStyle.Top,
                Height = 88,
                Padding = new Padding(8)
            };
            settingInfo.Controls.Add(BuildSettingInfoLayout());

            var editor = new GroupBox
            {
                Text = "إضافة تجاوز حسب النطاق",
                Dock = DockStyle.Top,
                Height = 158,
                Padding = new Padding(8)
            };
            editor.Controls.Add(BuildOverrideEditor());

            var history = new GroupBox
            {
                Text = "سجل القيم والتجاوزات",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };
            history.Controls.Add(dgvScopeValues);

            panel.Controls.Add(history);
            panel.Controls.Add(editor);
            panel.Controls.Add(settingInfo);
            return panel;
        }

        private TableLayoutPanel BuildSettingInfoLayout()
        {
            var layout = CreateTable(4, 2);
            AddField(layout, "الكود", lblSettingCode, 0, 0);
            AddField(layout, "اسم الإعداد", lblSettingName, 2, 0);
            AddField(layout, "نوع البيانات", lblDataType, 0, 1);
            AddField(layout, "القيمة الافتراضية", lblDefaultValue, 2, 1);
            return layout;
        }

        private TableLayoutPanel BuildOverrideEditor()
        {
            var layout = CreateTable(4, 3);
            cmbScopeType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbScopeType.Items.AddRange(new object[]
            {
                "GLOBAL", "GROUP", "COMPANY", "BRANCH", "MODULE", "SCREEN", "ROLE", "USER"
            });
            cmbScopeType.SelectedItem = "GLOBAL";

            ApplyInputStyle(txtScopeId);
            ApplyInputStyle(txtValue);
            ApplyInputStyle(txtReason);
            txtValue.Multiline = true;
            txtReason.Multiline = true;
            dtFrom.ShowCheckBox = true;
            dtTo.ShowCheckBox = true;
            dtFrom.Checked = false;
            dtTo.Checked = false;

            AddField(layout, "نوع النطاق", cmbScopeType, 0, 0);
            AddField(layout, "معرّف النطاق", txtScopeId, 2, 0);
            AddField(layout, "القيمة", txtValue, 0, 1);
            AddField(layout, "سبب التغيير", txtReason, 2, 1);
            AddField(layout, "فعّال من", dtFrom, 0, 2);
            AddField(layout, "فعّال إلى", dtTo, 2, 2);
            return layout;
        }

        private Panel BuildAuditFooter()
        {
            var audit = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 48,
                BackColor = Color.FromArgb(232, 231, 255),
                BorderStyle = BorderStyle.FixedSingle
            };
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                Padding = new Padding(8, 8, 8, 4),
                RightToLeft = RightToLeft.Yes
            };
            for (int i = 0; i < 6; i++)
                table.ColumnStyles.Add(new ColumnStyle(i % 2 == 0 ? SizeType.AutoSize : SizeType.Percent, i % 2 == 0 ? 0 : 33));

            AddAuditField(table, "مدخل السجل", lblRecordUser, 0);
            AddAuditField(table, "تاريخ الإدخال", lblRecordDate, 2);
            AddAuditField(table, "الحالة", lblStatus, 4);
            audit.Controls.Add(table);
            return audit;
        }

        private void ConfigurePermissions()
        {
            bool canManage = CurrentSession.Is_System_Admin;
            btnNewOverride.Enabled = canManage;
            btnSave.Enabled = canManage;
            btnDeactivate.Enabled = canManage;
            cmbScopeType.Enabled = canManage;
            txtScopeId.Enabled = canManage;
            txtValue.Enabled = canManage;
            txtReason.Enabled = canManage;
            dtFrom.Enabled = canManage;
            dtTo.Enabled = canManage;
        }

        private async Task LoadSettingsAsync()
        {
            try
            {
                btnRefresh.Enabled = false;
                _settings = await _client.GetFromJsonAsync<List<SettingItem>>(_baseUrl + "Settings")
                    ?? new List<SettingItem>();
                ApplySearch();
                ClearOverrideEditor();
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر تحميل الإعدادات.\n" + ex.Message, "الإعدادات",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRefresh.Enabled = true;
            }
        }

        private void ApplySearch()
        {
            string term = txtSearch.Text.Trim();
            var result = string.IsNullOrWhiteSpace(term)
                ? _settings
                : _settings.FindAll(x =>
                    x.Setting_Code.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    x.Setting_Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (x.Module_Name ?? string.Empty).Contains(term, StringComparison.OrdinalIgnoreCase));

            dgvSettings.DataSource = result;
        }

        private async Task LoadSelectedSettingAsync()
        {
            if (dgvSettings.CurrentRow?.DataBoundItem is not SettingItem item)
                return;

            _selectedSettingId = item.Setting_ID;
            lblSettingCode.Text = item.Setting_Code;
            lblSettingName.Text = item.Setting_Name;
            lblDataType.Text = item.Data_Type;
            lblDefaultValue.Text = item.Default_Value ?? string.Empty;
            lblStatus.Text = item.Is_Active ? "نشط" : "موقوف";

            try
            {
                var values = await _client.GetFromJsonAsync<List<ScopeValueItem>>(
                    _baseUrl + "Settings/" + _selectedSettingId + "/scope-values");
                dgvScopeValues.DataSource = values ?? new List<ScopeValueItem>();
                ClearOverrideEditor();
            }
            catch (Exception ex)
            {
                dgvScopeValues.DataSource = new List<ScopeValueItem>();
                MessageBox.Show("تعذر تحميل قيم الإعداد.\n" + ex.Message, "الإعدادات",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadSelectedScopeValue()
        {
            if (dgvScopeValues.CurrentRow?.DataBoundItem is not ScopeValueItem item)
                return;

            _selectedScopeValueId = item.Setting_Scope_Value_ID;
            lblRecordUser.Text = item.Created_By;
            lblRecordDate.Text = item.Created_At.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
            lblStatus.Text = item.Is_Active ? "نشط" : "معطل";
        }

        private void ClearOverrideEditor()
        {
            _selectedScopeValueId = 0;
            if (cmbScopeType.Items.Count > 0)
                cmbScopeType.SelectedItem = "GLOBAL";
            txtScopeId.Text = "GLOBAL";
            txtValue.Clear();
            txtReason.Clear();
            dtFrom.Checked = false;
            dtTo.Checked = false;
            lblRecordUser.Text = string.Empty;
            lblRecordDate.Text = string.Empty;
        }

        private void ApplyScopeTypeDefault()
        {
            if (string.Equals(cmbScopeType.SelectedItem?.ToString(), "GLOBAL", StringComparison.OrdinalIgnoreCase))
                txtScopeId.Text = "GLOBAL";
            else if (string.Equals(txtScopeId.Text, "GLOBAL", StringComparison.OrdinalIgnoreCase))
                txtScopeId.Clear();
        }

        private async Task SaveOverrideAsync()
        {
            if (!CurrentSession.Is_System_Admin)
                return;

            if (_selectedSettingId == 0)
            {
                MessageBox.Show("اختر إعداداً أولاً.", "الإعدادات",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var request = new ScopeValueRequest
            {
                Scope_Type = cmbScopeType.SelectedItem?.ToString() ?? string.Empty,
                Scope_ID = txtScopeId.Text.Trim(),
                Value = txtValue.Text.Trim(),
                Effective_From = dtFrom.Checked ? dtFrom.Value.Date : null,
                Effective_To = dtTo.Checked ? dtTo.Value.Date : null,
                Change_Reason = txtReason.Text.Trim()
            };

            try
            {
                using HttpResponseMessage response = await _client.PostAsJsonAsync(
                    _baseUrl + "Settings/" + _selectedSettingId + "/scope-values", request);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show(await ReadErrorAsync(response), "تعذر الحفظ",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                await LoadSelectedSettingAsync();
                MessageBox.Show("تم حفظ تجاوز الإعداد مع الاحتفاظ بالسجل السابق.", "الإعدادات",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر حفظ تجاوز الإعداد.\n" + ex.Message, "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DeactivateOverrideAsync()
        {
            if (!CurrentSession.Is_System_Admin || _selectedScopeValueId == 0)
            {
                MessageBox.Show("اختر قيمة من السجل لتعطيلها.", "الإعدادات",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("هل تريد تعطيل القيمة المختارة؟ سيبقى السجل محفوظاً.",
                "تأكيد التعطيل", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                using HttpResponseMessage response = await _client.PostAsync(
                    _baseUrl + "Settings/scope-values/" + _selectedScopeValueId + "/deactivate", null);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show(await ReadErrorAsync(response), "تعذر التعطيل",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                await LoadSelectedSettingAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر تعطيل القيمة.\n" + ex.Message, "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
        {
            string message = await response.Content.ReadAsStringAsync();
            return string.IsNullOrWhiteSpace(message) ? "تعذر إتمام العملية." : message;
        }

        private static TableLayoutPanel CreateTable(int columns, int rows)
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = columns,
                RowCount = rows,
                RightToLeft = RightToLeft.Yes
            };
            for (int i = 0; i < columns; i++)
                layout.ColumnStyles.Add(new ColumnStyle(i % 2 == 0 ? SizeType.AutoSize : SizeType.Percent, i % 2 == 0 ? 0 : 50));
            for (int i = 0; i < rows; i++)
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / rows));
            return layout;
        }

        private static readonly Color InputBackColor = Color.FromArgb(255, 255, 224);

        private static void ApplyInputStyle(TextBox textBox)
        {
            textBox.BackColor = InputBackColor;
            textBox.BorderStyle = BorderStyle.FixedSingle;
        }

        private static void AddField(TableLayoutPanel layout, string label, Control input, int column, int row)
        {
            input.Dock = DockStyle.Fill;
            layout.Controls.Add(new Label
            {
                Text = label + ":",
                AutoSize = true,
                Padding = new Padding(4),
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill
            }, column, row);
            layout.Controls.Add(input, column + 1, row);
        }

        private static Label CreateValueLabel()
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };
        }

        private static void AddAuditField(TableLayoutPanel table, string label, Label value, int column)
        {
            table.Controls.Add(new Label
            {
                Text = label + ":",
                AutoSize = true,
                Padding = new Padding(3)
            }, column, 0);
            table.Controls.Add(value, column + 1, 0);
        }

        private static DataGridView CreateGrid()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToOrderColumns = false,
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

        private sealed class SettingItem
        {
            public long Setting_ID { get; set; }
            public string Setting_Code { get; set; } = string.Empty;
            public string Setting_Name { get; set; } = string.Empty;
            public string? Module_Name { get; set; }
            public string Data_Type { get; set; } = string.Empty;
            public string? Default_Value { get; set; }
            public bool Is_Sensitive { get; set; }
            public bool Is_Active { get; set; }
        }

        private sealed class ScopeValueItem
        {
            public long Setting_Scope_Value_ID { get; set; }
            public string Scope_Type { get; set; } = string.Empty;
            public string Scope_ID { get; set; } = string.Empty;
            public string? Value { get; set; }
            public DateTime? Effective_From { get; set; }
            public DateTime? Effective_To { get; set; }
            public bool Is_Active { get; set; }
            public string Created_By { get; set; } = string.Empty;
            public DateTime Created_At { get; set; }
            public string? Change_Reason { get; set; }
        }

        private sealed class ScopeValueRequest
        {
            public string Scope_Type { get; set; } = string.Empty;
            public string Scope_ID { get; set; } = string.Empty;
            public string? Value { get; set; }
            public DateTime? Effective_From { get; set; }
            public DateTime? Effective_To { get; set; }
            public string? Change_Reason { get; set; }
        }
    }
}