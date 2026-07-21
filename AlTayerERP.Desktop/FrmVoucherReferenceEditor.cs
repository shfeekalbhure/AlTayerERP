using AlTayerERP.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// محرر موحد للقوائم المرجعية التي يحتاجها سند القبض.
    /// يعمل مع الـ API الحقيقي ولا يعرض رسالة حفظ شكلية.
    /// </summary>
    public abstract class FrmVoucherReferenceEditor : Form
    {
        private readonly string _endpoint;
        private readonly string _idProperty;
        private readonly IReadOnlyList<ReferenceEditorField> _fields;
        private readonly HttpClient _client = ApiService.Client;
        private readonly Dictionary<string, Control> _inputs = new();
        private readonly DataGridView _grid = new()
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            AutoGenerateColumns = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            BackgroundColor = Color.White
        };

        private int _selectedId;

        protected FrmVoucherReferenceEditor(
            string title,
            string endpoint,
            string idProperty,
            params ReferenceEditorField[] fields)
        {
            Text = title;
            _endpoint = endpoint;
            _idProperty = idProperty;
            _fields = fields;
            StartPosition = FormStartPosition.CenterParent;
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Segoe UI", 10);
            Width = 1050;
            Height = 680;

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 54,
                Padding = new Padding(10),
                FlowDirection = FlowDirection.RightToLeft
            };
            var btnNew = NewButton("جديد", (_, _) => ClearEditor());
            var btnSave = NewButton("حفظ", async (_, _) => await SaveAsync());
            var btnRefresh = NewButton("تحديث", async (_, _) => await LoadAsync());
            var btnClose = NewButton("إغلاق", (_, _) => Close());
            toolbar.Controls.AddRange(new Control[] { btnNew, btnSave, btnRefresh, btnClose });

            var editor = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(12),
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = true
            };

            foreach (var field in _fields)
            {
                var panel = new Panel { Width = 220, Height = 64, Margin = new Padding(5) };
                panel.Controls.Add(new Label { Text = field.Caption, Dock = DockStyle.Top, Height = 24 });

                Control input = field.Kind switch
                {
                    ReferenceEditorFieldKind.Boolean => new CheckBox
                    {
                        Text = "نعم",
                        Dock = DockStyle.Fill,
                        Checked = field.DefaultBoolean
                    },
                    ReferenceEditorFieldKind.Number => new NumericUpDown
                    {
                        Dock = DockStyle.Fill,
                        Maximum = 999999,
                        DecimalPlaces = 0,
                        ThousandsSeparator = true
                    },
                    _ => new TextBox { Dock = DockStyle.Fill, MaxLength = field.MaxLength }
                };

                _inputs[field.Code] = input;
                panel.Controls.Add(input);
                editor.Controls.Add(panel);

                _grid.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = field.Code,
                    HeaderText = field.Caption,
                    DataPropertyName = field.Code,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                });
            }

            _grid.SelectionChanged += (_, _) => LoadSelectedRowIntoEditor();

            Controls.Add(_grid);
            Controls.Add(editor);
            Controls.Add(toolbar);
            Load += async (_, _) => await LoadAsync();
        }

        private static Button NewButton(string caption, EventHandler handler)
        {
            var button = new Button { Text = caption, Width = 92, Height = 32, Margin = new Padding(4) };
            button.Click += handler;
            return button;
        }

        private async Task LoadAsync()
        {
            try
            {
                var rows = await _client.GetFromJsonAsync<List<Dictionary<string, JsonElement>>>(_endpoint) ?? new();
                _grid.Rows.Clear();

                foreach (var row in rows)
                {
                    var index = _grid.Rows.Add();
                    var gridRow = _grid.Rows[index];
                    gridRow.Tag = row;

                    foreach (var field in _fields)
                    {
                        if (row.TryGetValue(field.Code, out var value))
                            gridRow.Cells[field.Code].Value = ReadJsonValue(value);
                    }
                }

                ClearEditor();
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر تحميل البيانات: " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSelectedRowIntoEditor()
        {
            if (_grid.CurrentRow?.Tag is not Dictionary<string, JsonElement> row)
                return;

            _selectedId = row.TryGetValue(_idProperty, out var id) && id.TryGetInt32(out var value) ? value : 0;

            foreach (var field in _fields)
            {
                if (!row.TryGetValue(field.Code, out var json) || !_inputs.TryGetValue(field.Code, out var input))
                    continue;

                switch (input)
                {
                    case CheckBox checkBox:
                        checkBox.Checked = json.ValueKind == JsonValueKind.True ||
                                           (json.ValueKind == JsonValueKind.String && bool.TryParse(json.GetString(), out var flag) && flag);
                        break;
                    case NumericUpDown number when json.TryGetDecimal(out var decimalValue):
                        number.Value = Math.Min(number.Maximum, Math.Max(number.Minimum, decimalValue));
                        break;
                    case TextBox textBox:
                        textBox.Text = ReadJsonValue(json);
                        break;
                }
            }
        }

        private async Task SaveAsync()
        {
            if (!CurrentSession.Is_System_Admin)
            {
                MessageBox.Show("إدارة هذه القائمة مخصصة لمدير النظام.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var payload = new Dictionary<string, object?> { [_idProperty] = _selectedId };

            foreach (var field in _fields)
            {
                var input = _inputs[field.Code];
                payload[field.Code] = input switch
                {
                    CheckBox checkBox => checkBox.Checked,
                    NumericUpDown number => Convert.ToInt32(number.Value),
                    TextBox textBox => textBox.Text.Trim(),
                    _ => null
                };
            }

            try
            {
                var response = await _client.PostAsJsonAsync(_endpoint, payload);
                if (!response.IsSuccessStatusCode)
                    throw new InvalidOperationException(await response.Content.ReadAsStringAsync());

                MessageBox.Show("تم الحفظ بنجاح.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر الحفظ: " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearEditor()
        {
            _selectedId = 0;
            foreach (var field in _fields)
            {
                var input = _inputs[field.Code];
                switch (input)
                {
                    case CheckBox checkBox:
                        checkBox.Checked = field.DefaultBoolean;
                        break;
                    case NumericUpDown number:
                        number.Value = 0;
                        break;
                    case TextBox textBox:
                        textBox.Clear();
                        break;
                }
            }

            _grid.ClearSelection();
        }

        private static string ReadJsonValue(JsonElement value) => value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? string.Empty,
            JsonValueKind.True => "نعم",
            JsonValueKind.False => "لا",
            JsonValueKind.Number => value.ToString(),
            _ => string.Empty
        };
    }

    public enum ReferenceEditorFieldKind { Text, Number, Boolean }

    public sealed record ReferenceEditorField(
        string Code,
        string Caption,
        ReferenceEditorFieldKind Kind = ReferenceEditorFieldKind.Text,
        int MaxLength = 150,
        bool DefaultBoolean = false);
}
