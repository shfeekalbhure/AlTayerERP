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
    /// شاشة تعريف طرق السداد وفق قالب واجهات الطائر المحاسبية.
    /// </summary>
    public class FrmPaymentMethods : Form
    {
        private readonly HttpClient _client = ApiService.Client;
        private readonly string _baseUrl = ApiService.BaseUrl;

        private readonly DataGridView dgvItems = CreateGrid();
        private readonly TextBox txtCode = new();
        private readonly TextBox txtNameAr = new();
        private readonly TextBox txtNameEn = new();
        private readonly NumericUpDown nudOrder = new();
        private readonly CheckBox chkReference = new() { Text = "يتطلب رقم مرجع" };
        private readonly CheckBox chkReferenceDate = new() { Text = "يتطلب تاريخ مرجع" };
        private readonly CheckBox chkCash = new() { Text = "نقدي" };
        private readonly CheckBox chkBank = new() { Text = "بنكي" };
        private readonly CheckBox chkActive = new() { Text = "نشط", Checked = true };

        private readonly ToolStripButton btnNew = new("جديد");
        private readonly ToolStripButton btnSave = new("حفظ");
        private readonly ToolStripButton btnDeactivate = new("تعطيل");
        private readonly ToolStripButton btnRefresh = new("تحديث");
        private readonly ToolStripButton btnSearch = new("بحث");

        private int _selectedId;
        private readonly Label lblRecordUser = CreateAuditValue();
        private readonly Label lblRecordDate = CreateAuditValue();
        private readonly Label lblLastUpdate = CreateAuditValue();
        private readonly Label lblStatus = CreateAuditValue();

        public FrmPaymentMethods()
        {
            BuildLayout();
            ConfigurePermissions();
            Load += async (_, _) => await LoadItemsAsync();
        }

        private void BuildLayout()
        {
            Text = "طرق السداد";
            StartPosition = FormStartPosition.CenterParent;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1040, 650);
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
                Text = "طرق السداد",
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
                BackColor = Color.FromArgb(245, 245, 240),
                RightToLeft = RightToLeft.Yes,
                RenderMode = ToolStripRenderMode.System
            };
            toolbar.Items.AddRange(new ToolStripItem[]
            {
                btnNew, new ToolStripSeparator(), btnSave, new ToolStripSeparator(),
                btnDeactivate, new ToolStripSeparator(), btnRefresh, new ToolStripSeparator(),
                btnSearch
            });

            btnNew.Click += (_, _) => ClearForm();
            btnRefresh.Click += async (_, _) => await LoadItemsAsync();
            btnSave.Click += async (_, _) => await SaveAsync();
            btnDeactivate.Click += async (_, _) => await DeactivateAsync();
            btnSearch.Click += (_, _) => txtCode.Focus();

            var tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                RightToLeftLayout = true,
                Appearance = TabAppearance.Normal
            };
            var mainTab = new TabPage("البيانات الرئيسية") { BackColor = BackColor };
            var noteTab = new TabPage("بيانات إضافية") { BackColor = BackColor };
            noteTab.Controls.Add(new Label
            {
                Text = "تظهر هنا خصائص إضافية عند اعتمادها في إعدادات النظام.",
                Dock = DockStyle.Top,
                Padding = new Padding(12),
                TextAlign = ContentAlignment.MiddleRight
            });

            var entryGroup = new GroupBox
            {
                Text = "بيانات طريقة السداد",
                Dock = DockStyle.Top,
                Height = 182,
                Padding = new Padding(12),
                RightToLeft = RightToLeft.Yes
            };
            entryGroup.Controls.Add(BuildEntryLayout());

            var gridGroup = new GroupBox
            {
                Text = "طرق السداد المعرفة",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };
            gridGroup.Controls.Add(dgvItems);

            mainTab.Controls.Add(gridGroup);
            mainTab.Controls.Add(entryGroup);
            tabs.TabPages.Add(mainTab);
            tabs.TabPages.Add(noteTab);

            var audit = BuildAuditFooter();

            Controls.Add(tabs);
            Controls.Add(audit);
            Controls.Add(toolbar);
            Controls.Add(header);

            chkReference.CheckedChanged += (_, _) =>
            {
                if (!chkReference.Checked)
                    chkReferenceDate.Checked = false;
            };
            dgvItems.SelectionChanged += (_, _) => LoadSelected();
        }

        private TableLayoutPanel BuildEntryLayout()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3,
                RightToLeft = RightToLeft.Yes
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 34));

            ApplyInputStyle(txtCode);
            ApplyInputStyle(txtNameAr);
            ApplyInputStyle(txtNameEn);
            nudOrder.BackColor = InputBackColor;
            nudOrder.Minimum = 0;
            nudOrder.Maximum = 999999;

            AddField(layout, "الكود", txtCode, 0, 0);
            AddField(layout, "الاسم العربي", txtNameAr, 2, 0);
            AddField(layout, "الاسم الإنجليزي", txtNameEn, 0, 1);
            AddField(layout, "ترتيب العرض", nudOrder, 2, 1);

            var flags = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(4)
            };
            flags.Controls.AddRange(new Control[]
            {
                chkActive, chkCash, chkBank, chkReference, chkReferenceDate
            });
            layout.Controls.Add(flags, 0, 2);
            layout.SetColumnSpan(flags, 4);
            return layout;
        }

        private static readonly Color InputBackColor = Color.FromArgb(255, 255, 224);

        private static void ApplyInputStyle(TextBox textBox)
        {
            textBox.BackColor = InputBackColor;
            textBox.BorderStyle = BorderStyle.FixedSingle;
        }

        private static void AddField(
            TableLayoutPanel layout, string label, Control input, int column, int row)
        {
            input.Dock = DockStyle.Fill;
            var caption = new Label
            {
                Text = label + ":",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill,
                Padding = new Padding(4)
            };
            layout.Controls.Add(caption, column, row);
            layout.Controls.Add(input, column + 1, row);
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

            AddAuditField(table, "مدخل السجل", lblRecordUser, 0);
            AddAuditField(table, "تاريخ الإدخال", lblRecordDate, 2);
            AddAuditField(table, "آخر تعديل", lblLastUpdate, 4);
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
            var grid = new DataGridView
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
            return grid;
        }

        private void ConfigurePermissions()
        {
            bool canMaintain = CurrentSession.Is_System_Admin;
            btnNew.Enabled = canMaintain;
            btnSave.Enabled = canMaintain;
            btnDeactivate.Enabled = canMaintain;
            SetEditorEnabled(canMaintain);
        }

        private void SetEditorEnabled(bool enabled)
        {
            foreach (Control control in new Control[]
            {
                txtCode, txtNameAr, txtNameEn, nudOrder, chkReference,
                chkReferenceDate, chkCash, chkBank, chkActive
            })
            {
                control.Enabled = enabled;
            }
        }

        private async Task LoadItemsAsync()
        {
            try
            {
                btnRefresh.Enabled = false;
                List<PaymentMethodItem>? items =
                    await _client.GetFromJsonAsync<List<PaymentMethodItem>>(_baseUrl + "PaymentMethods");
                dgvItems.DataSource = items ?? new List<PaymentMethodItem>();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر تحميل طرق السداد.\n" + ex.Message, "طرق السداد",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRefresh.Enabled = true;
            }
        }

        private void LoadSelected()
        {
            if (dgvItems.CurrentRow?.DataBoundItem is not PaymentMethodItem item)
                return;

            _selectedId = item.Payment_Method_ID;
            txtCode.Text = item.Payment_Method_Code;
            txtNameAr.Text = item.Payment_Method_Name_AR;
            txtNameEn.Text = item.Payment_Method_Name_EN ?? string.Empty;
            nudOrder.Value = Math.Clamp(item.Sort_Order, 0, 999999);
            chkReference.Checked = item.Requires_Reference;
            chkReferenceDate.Checked = item.Requires_Reference_Date;
            chkCash.Checked = item.Is_Cash;
            chkBank.Checked = item.Is_Bank;
            chkActive.Checked = item.Is_Active;

            lblRecordUser.Text = "النظام";
            lblRecordDate.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            lblLastUpdate.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            lblStatus.Text = item.Is_Active ? "نشط" : "موقوف";
        }

        private void ClearForm()
        {
            _selectedId = 0;
            txtCode.Clear();
            txtNameAr.Clear();
            txtNameEn.Clear();
            nudOrder.Value = 0;
            chkReference.Checked = false;
            chkReferenceDate.Checked = false;
            chkCash.Checked = false;
            chkBank.Checked = false;
            chkActive.Checked = true;
            lblRecordUser.Text = string.Empty;
            lblRecordDate.Text = string.Empty;
            lblLastUpdate.Text = string.Empty;
            lblStatus.Text = "جديد";
            txtCode.Focus();
        }

        private async Task SaveAsync()
        {
            if (!CurrentSession.Is_System_Admin)
                return;

            var request = new PaymentMethodRequest
            {
                Payment_Method_Code = txtCode.Text.Trim(),
                Payment_Method_Name_AR = txtNameAr.Text.Trim(),
                Payment_Method_Name_EN = txtNameEn.Text.Trim(),
                Requires_Reference = chkReference.Checked,
                Requires_Reference_Date = chkReferenceDate.Checked,
                Is_Cash = chkCash.Checked,
                Is_Bank = chkBank.Checked,
                Is_Active = chkActive.Checked,
                Sort_Order = (int)nudOrder.Value
            };

            try
            {
                using HttpResponseMessage response = _selectedId == 0
                    ? await _client.PostAsJsonAsync(_baseUrl + "PaymentMethods", request)
                    : await _client.PutAsJsonAsync(_baseUrl + "PaymentMethods/" + _selectedId, request);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show(await ReadErrorAsync(response), "تعذر الحفظ",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                await LoadItemsAsync();
                MessageBox.Show("تم حفظ طريقة السداد بنجاح.", "طرق السداد",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر حفظ طريقة السداد.\n" + ex.Message, "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DeactivateAsync()
        {
            if (!CurrentSession.Is_System_Admin || _selectedId == 0)
                return;

            if (MessageBox.Show("هل تريد تعطيل طريقة السداد المختارة؟", "تأكيد التعطيل",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                using HttpResponseMessage response =
                    await _client.PostAsync(_baseUrl + "PaymentMethods/" + _selectedId + "/deactivate", null);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show(await ReadErrorAsync(response), "تعذر التعطيل",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                await LoadItemsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر تعطيل طريقة السداد.\n" + ex.Message, "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
        {
            string message = await response.Content.ReadAsStringAsync();
            return string.IsNullOrWhiteSpace(message) ? "تعذر إتمام العملية." : message;
        }

        private sealed class PaymentMethodItem
        {
            public int Payment_Method_ID { get; set; }
            public string Payment_Method_Code { get; set; } = string.Empty;
            public string Payment_Method_Name_AR { get; set; } = string.Empty;
            public string? Payment_Method_Name_EN { get; set; }
            public bool Requires_Reference { get; set; }
            public bool Requires_Reference_Date { get; set; }
            public bool Is_Cash { get; set; }
            public bool Is_Bank { get; set; }
            public bool Is_Active { get; set; }
            public int Sort_Order { get; set; }
        }

        private sealed class PaymentMethodRequest
        {
            public string Payment_Method_Code { get; set; } = string.Empty;
            public string Payment_Method_Name_AR { get; set; } = string.Empty;
            public string Payment_Method_Name_EN { get; set; } = string.Empty;
            public bool Requires_Reference { get; set; }
            public bool Requires_Reference_Date { get; set; }
            public bool Is_Cash { get; set; }
            public bool Is_Bank { get; set; }
            public bool Is_Active { get; set; }
            public int Sort_Order { get; set; }
        }
    }
}