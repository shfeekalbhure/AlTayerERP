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
    /// إدارة طرق السداد المرجعية لسندات القبض.
    /// الكتابة متاحة لمدير النظام فقط ومحمية برمز Bearer لدى الخادم.
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
        private readonly Button btnNew = new() { Text = "جديد" };
        private readonly Button btnSave = new() { Text = "حفظ" };
        private readonly Button btnDeactivate = new() { Text = "تعطيل" };
        private readonly Button btnRefresh = new() { Text = "تحديث" };

        private int _selectedId;

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
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Tahoma", 10F);
            Width = 1120;
            Height = 690;
            MinimumSize = new Size(900, 560);

            btnNew.Click += (_, _) => ClearForm();
            btnRefresh.Click += async (_, _) => await LoadItemsAsync();
            btnSave.Click += async (_, _) => await SaveAsync();
            btnDeactivate.Click += async (_, _) => await DeactivateAsync();
            chkReference.CheckedChanged += (_, _) =>
            {
                if (!chkReference.Checked)
                    chkReferenceDate.Checked = false;
            };
            dgvItems.SelectionChanged += (_, _) => LoadSelected();

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(8),
                FlowDirection = FlowDirection.RightToLeft
            };
            toolbar.Controls.AddRange(new Control[] { btnNew, btnSave, btnDeactivate, btnRefresh });

            var editor = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 170,
                ColumnCount = 4,
                RowCount = 3,
                Padding = new Padding(10)
            };
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            AddEditor(editor, "الكود", txtCode, 0, 0);
            AddEditor(editor, "الاسم العربي", txtNameAr, 2, 0);
            AddEditor(editor, "الاسم الإنجليزي", txtNameEn, 0, 1);
            nudOrder.Minimum = 0;
            nudOrder.Maximum = 999999;
            AddEditor(editor, "ترتيب العرض", nudOrder, 2, 1);

            var flags = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            flags.Controls.AddRange(new Control[] { chkActive, chkCash, chkBank, chkReference, chkReferenceDate });
            editor.Controls.Add(flags, 0, 2);
            editor.SetColumnSpan(flags, 4);

            Controls.Add(dgvItems);
            Controls.Add(editor);
            Controls.Add(toolbar);
        }

        private static void AddEditor(TableLayoutPanel layout, string label, Control input, int column, int row)
        {
            input.Dock = DockStyle.Fill;
            layout.Controls.Add(new Label
            {
                Text = label + ":",
                TextAlign = ContentAlignment.MiddleRight,
                AutoSize = true,
                Padding = new Padding(4)
            }, column, row);
            layout.Controls.Add(input, column + 1, row);
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
                RowHeadersVisible = false
            };
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
                _selectedId = 0;
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