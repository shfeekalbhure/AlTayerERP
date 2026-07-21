using AlTayerERP.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// محرر موحّد للقوائم المرجعية المرتبطة بالسندات.
    /// يقرأ ويحفظ عبر API الحقيقي، ويطبق قالب الواجهة العربية المشترك.
    /// </summary>
    public abstract class FrmVoucherReferenceEditor : Form
    {
        private readonly string _endpoint;
        private readonly string _idProperty;
        private readonly IReadOnlyList<ReferenceEditorField> _fields;
        private readonly HttpClient _client = ApiService.Client;
        private readonly Dictionary<string, Control> _inputs = new();
        private readonly TextBox _searchBox = new();
        private readonly Label _recordCount = new();
        private readonly Label _emptyState = new();
        private readonly DataGridView _grid = new()
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            AutoGenerateColumns = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false
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
            Font = new Font("Segoe UI", 9.5F);
            BackColor = Color.FromArgb(244, 247, 251);
            Width = 1180;
            Height = 720;
            MinimumSize = new Size(980, 650);
            KeyPreview = true;

            var shell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = BackColor,
                ColumnCount = 1,
                RowCount = 6,
                Padding = new Padding(14)
            };
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            shell.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));

            shell.Controls.Add(CreateHeader(title), 0, 0);
            shell.Controls.Add(CreateToolbar(), 0, 1);
            shell.Controls.Add(CreateSearchCard(), 0, 2);
            shell.Controls.Add(CreateEditorCard(), 0, 3);
            shell.Controls.Add(CreateGridCard(), 0, 4);
            shell.Controls.Add(CreateFooter(), 0, 5);
            Controls.Add(shell);

            _grid.SelectionChanged += (_, _) => LoadSelectedRowIntoEditor();
            Load += async (_, _) => await LoadAsync();
            KeyDown += FrmVoucherReferenceEditor_KeyDown;
        }

        private Control CreateHeader(string title)
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
                Text = title,
                Dock = DockStyle.Top,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Height = 33,
                TextAlign = ContentAlignment.MiddleRight
            });
            header.Controls.Add(new Label
            {
                Text = BuildSessionCaption(),
                Dock = DockStyle.Bottom,
                ForeColor = Color.FromArgb(220, 232, 247),
                Font = new Font("Segoe UI", 8.5F),
                Height = 20,
                TextAlign = ContentAlignment.MiddleRight
            });

            return header;
        }

        private static string BuildSessionCaption()
        {
            var company = string.IsNullOrWhiteSpace(CurrentSession.Company_Name)
                ? CurrentSession.Company_ID
                : CurrentSession.Company_Name;
            var branch = string.IsNullOrWhiteSpace(CurrentSession.Branch_Name)
                ? CurrentSession.Branch_ID.ToString()
                : CurrentSession.Branch_Name;
            var year = string.IsNullOrWhiteSpace(CurrentSession.Year_Name)
                ? CurrentSession.Year_ID.ToString()
                : CurrentSession.Year_Name;

            return $"نطاق العمل: {company}  •  {branch}  •  {year}";
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

            toolbar.Controls.Add(CreateButton("إغلاق  Esc", Color.FromArgb(107, 114, 128), (_, _) => Close()));
            toolbar.Controls.Add(CreateButton("تحديث  F5", Color.FromArgb(36, 99, 168), async (_, _) => await LoadAsync()));
            toolbar.Controls.Add(CreateButton("حفظ  Ctrl+S", Color.FromArgb(22, 125, 84), async (_, _) => await SaveAsync()));
            toolbar.Controls.Add(CreateButton("جديد  F2", Color.FromArgb(28, 125, 184), (_, _) => ClearEditor()));
            return toolbar;
        }

        private Control CreateSearchCard()
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10, 7, 10, 7),
                Margin = new Padding(0, 0, 0, 6)
            };

            _searchBox.PlaceholderText = "بحث سريع بالكود أو الاسم…";
            _searchBox.Dock = DockStyle.Fill;
            _searchBox.BorderStyle = BorderStyle.FixedSingle;
            _searchBox.TextChanged += (_, _) => FilterGrid();

            card.Controls.Add(_searchBox);
            card.Controls.Add(new Label
            {
                Text = "بحث",
                Dock = DockStyle.Right,
                Width = 72,
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Color.FromArgb(55, 65, 81),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            });

            return card;
        }

        private Control CreateEditorCard()
        {
            var card = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(14, 10, 14, 10),
                Margin = new Padding(0, 0, 0, 8)
            };

            var title = new Label
            {
                Text = "بيانات السجل",
                Dock = DockStyle.Top,
                Height = 27,
                ForeColor = Color.FromArgb(27, 62, 104),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight
            };

            var editor = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = true,
                Padding = new Padding(0, 4, 0, 0)
            };

            foreach (var field in _fields)
            {
                var panel = new Panel { Width = 220, Height = 66, Margin = new Padding(5) };
                panel.Controls.Add(new Label
                {
                    Text = field.Caption,
                    Dock = DockStyle.Top,
                    Height = 24,
                    ForeColor = Color.FromArgb(55, 65, 81),
                    TextAlign = ContentAlignment.MiddleRight
                });

                Control input = field.Kind switch
                {
                    ReferenceEditorFieldKind.Boolean => new CheckBox
                    {
                        Text = "نعم",
                        Dock = DockStyle.Fill,
                        Checked = field.DefaultBoolean,
                        TextAlign = ContentAlignment.MiddleRight
                    },
                    ReferenceEditorFieldKind.Number => new NumericUpDown
                    {
                        Dock = DockStyle.Fill,
                        Maximum = 999999,
                        DecimalPlaces = 0,
                        ThousandsSeparator = true
                    },
                    _ => new TextBox
                    {
                        Dock = DockStyle.Fill,
                        MaxLength = field.MaxLength,
                        BorderStyle = BorderStyle.FixedSingle
                    }
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

            card.Controls.Add(editor);
            card.Controls.Add(title);
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

            ConfigureGrid();

            _emptyState.Text = "لا توجد سجلات مطابقة للعرض\nاستخدم «جديد» لإضافة سجل أو عدّل عبارة البحث.";
            _emptyState.Dock = DockStyle.Fill;
            _emptyState.TextAlign = ContentAlignment.MiddleCenter;
            _emptyState.Font = new Font("Segoe UI", 10F);
            _emptyState.ForeColor = Color.FromArgb(107, 114, 128);
            _emptyState.BackColor = Color.White;

            card.Controls.Add(_grid);
            card.Controls.Add(_emptyState);
            return card;
        }

        private Control CreateFooter()
        {
            var footer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4, 4, 4, 0) };
            _recordCount.Dock = DockStyle.Right;
            _recordCount.Width = 170;
            _recordCount.ForeColor = Color.FromArgb(75, 85, 99);
            _recordCount.TextAlign = ContentAlignment.MiddleRight;
            _recordCount.Text = "عدد السجلات: 0";

            footer.Controls.Add(_recordCount);
            footer.Controls.Add(new Label
            {
                Text = "الحفظ محمي بصلاحيات مدير النظام",
                Dock = DockStyle.Left,
                ForeColor = Color.FromArgb(75, 85, 99),
                TextAlign = ContentAlignment.MiddleLeft
            });

            return footer;
        }

        private void ConfigureGrid()
        {
            _grid.RowHeadersVisible = false;
            _grid.EnableHeadersVisualStyles = false;
            _grid.BackgroundColor = Color.White;
            _grid.BorderStyle = BorderStyle.None;
            _grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            _grid.GridColor = Color.FromArgb(226, 232, 240);
            _grid.RowTemplate.Height = 34;
            _grid.ColumnHeadersHeight = 38;
            _grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(232, 239, 248),
                ForeColor = Color.FromArgb(31, 58, 92),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleRight
            };
            _grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                SelectionBackColor = Color.FromArgb(218, 232, 247),
                SelectionForeColor = Color.FromArgb(20, 44, 75),
                Alignment = DataGridViewContentAlignment.MiddleRight
            };
            _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        }

        private static Button CreateButton(string caption, Color color, EventHandler handler)
        {
            var button = new Button
            {
                Text = caption,
                Width = 122,
                Height = 32,
                Margin = new Padding(4, 0, 4, 0),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                BackColor = color,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
            };
            button.Click += handler;
            return button;
        }

        private async Task LoadAsync()
        {
            try
            {
                UseWaitCursor = true;
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

                _recordCount.Text = $"عدد السجلات: {_grid.Rows.Count}";
                _emptyState.Visible = _grid.Rows.Count == 0;
                ClearEditor();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "تعذر تحميل البيانات. تحقق من اتصال API وصلاحيتك ثم أعد المحاولة.\n\n" + ex.Message,
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private void FilterGrid()
        {
            var query = _searchBox.Text.Trim();
            foreach (DataGridViewRow row in _grid.Rows)
            {
                row.Visible = string.IsNullOrWhiteSpace(query) ||
                    row.Cells.Cast<DataGridViewCell>()
                        .Any(cell => (cell.Value?.ToString() ?? string.Empty)
                        .IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0);
            }

            _emptyState.Visible = _grid.Rows.Cast<DataGridViewRow>().All(row => !row.Visible);
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

            var required = _fields
                .Where(field => field.Kind == ReferenceEditorFieldKind.Text &&
                    (field.Code.EndsWith("_Code", StringComparison.Ordinal) ||
                     field.Code.EndsWith("_Name_AR", StringComparison.Ordinal) ||
                     field.Code.EndsWith("_Name", StringComparison.Ordinal)))
                .FirstOrDefault(field => _inputs[field.Code] is TextBox textBox && string.IsNullOrWhiteSpace(textBox.Text));

            if (required != null)
            {
                MessageBox.Show($"الحقل «{required.Caption}» مطلوب.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _inputs[required.Code].Focus();
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
                UseWaitCursor = true;
                var response = await _client.PostAsJsonAsync(_endpoint, payload);
                if (!response.IsSuccessStatusCode)
                    throw new InvalidOperationException(await response.Content.ReadAsStringAsync());

                MessageBox.Show("تم الحفظ بنجاح.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "تعذر الحفظ. تحقق من الحقول وصلاحية المستخدم واتصال API.\n\n" + ex.Message,
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
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

        private void FrmVoucherReferenceEditor_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                ClearEditor();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F5)
            {
                _ = LoadAsync();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.S)
            {
                _ = SaveAsync();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.F)
            {
                _searchBox.Focus();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Close();
                e.SuppressKeyPress = true;
            }
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
