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
        private readonly object _newIdValue;
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

        // قد يكون المفتاح رقمياً في القوائم المرجعية أو نصياً في الأطراف المالية.
        private object? _selectedId;

        protected FrmVoucherReferenceEditor(
            string title,
            string endpoint,
            string idProperty,
            params ReferenceEditorField[] fields)
        {
            Text = title;
            _endpoint = endpoint;
            _idProperty = idProperty;
            // أغلب القوائم تستخدم معرفاً رقمياً؛ الطرف المالي وحده مفتاحه نصي.
            _newIdValue = string.Equals(idProperty, "Party_ID", StringComparison.Ordinal)
                ? string.Empty
                : 0;
            _fields = fields;
            StartPosition = FormStartPosition.CenterParent;
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Segoe UI", 9.5F);
            BackColor = SystemColors.Control;
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
                Padding = new Padding(10)
            };
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
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
                Height = 58,
                Visible = true,
                Padding = new Padding(14, 4, 14, 4),
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
                Dock = DockStyle.Right,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = SystemColors.Control,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(8),
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Margin = new Padding(0, 0, 0, 6)
            };

            toolbar.Controls.Add(CreateButton("إغلاق  Esc", Color.FromArgb(107, 114, 128), (_, _) => Close()));

            if (IsFiscalPeriodsScreen)
            {
                toolbar.Controls.Add(CreateButton("فتح الفترة", Color.FromArgb(5, 122, 85), async (_, _) => await ChangeFiscalPeriodLifecycleAsync(reopen: true)));
                toolbar.Controls.Add(CreateButton("إقفال الفترة", Color.FromArgb(190, 83, 24), async (_, _) => await ChangeFiscalPeriodLifecycleAsync(reopen: false)));
            }

            toolbar.Controls.Add(CreateButton("تحديث  F5", Color.FromArgb(36, 99, 168), async (_, _) => await LoadAsync()));
            toolbar.Controls.Add(CreateButton("حفظ  F2", Color.FromArgb(22, 125, 84), async (_, _) => await SaveAsync()));
            toolbar.Controls.Add(CreateButton("جديد  F3", Color.FromArgb(28, 125, 184), (_, _) => ClearEditor()));
            return toolbar;
        }

        private Control CreateSearchCard()
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10, 6, 10, 6),
                Margin = new Padding(0, 0, 0, 6)
            };

            _searchBox.PlaceholderText = "بحث سريع بالكود أو الاسم…";
            _searchBox.Dock = DockStyle.Fill;
            _searchBox.BorderStyle = BorderStyle.FixedSingle;
            _searchBox.Font = new Font("Segoe UI", 10F);
            _searchBox.TextAlign = HorizontalAlignment.Right;
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
                Text = IsFiscalPeriodsScreen
                    ? $"بيانات الفترة المالية  —  السنة المالية الحالية: {CurrentSession.Year_Name ?? CurrentSession.Year_ID.ToString()}"
                    : "بيانات السجل",
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
                // إطار مستقل لكل حقل حتى لا تختفي حدود الإدخال في الشاشات العربية.
                var panel = new Panel
                {
                    Width = 250,
                    Height = 86,
                    Margin = new Padding(6),
                    Padding = new Padding(8, 4, 8, 6),
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle
                };
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
                        Maximum = 999999999,
                        DecimalPlaces = field.DecimalPlaces,
                        ThousandsSeparator = true
                    },
                    ReferenceEditorFieldKind.Date => new DateTimePicker
                    {
                        Dock = DockStyle.Fill,
                        Format = DateTimePickerFormat.Short,
                        ShowCheckBox = true,
                        Checked = false
                    },
                    ReferenceEditorFieldKind.Choice => new ComboBox
                    {
                        Dock = DockStyle.Fill,
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        DataSource = (field.Options ?? Array.Empty<string>()).ToArray()
                    },
                    _ => new TextBox
                    {
                        Dock = DockStyle.Fill,
                        MaxLength = field.MaxLength,
                        BorderStyle = BorderStyle.FixedSingle
                    }
                };

                ConfigureEditorInput(input);

                _inputs[field.Code] = input;
                panel.Controls.Add(input);
                editor.Controls.Add(panel);

                _grid.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = field.Code,
                    HeaderText = IsFiscalPeriodsScreen && field.Code == "Is_Closed" ? "الحالة" : field.Caption,
                    DataPropertyName = field.Code,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                });
            }

            card.Controls.Add(editor);
            card.Controls.Add(title);

            ConfigureFiscalPeriodLifecycleInputs();
            return card;
        }

        /// <summary>
        /// توحيد أبعاد عناصر الإدخال حتى تكون واضحة وقابلة للاستخدام في الشاشات العربية.
        /// </summary>
        private static void ConfigureEditorInput(Control input)
        {
            input.Dock = DockStyle.Bottom;
            input.Height = 34;
            input.Margin = new Padding(0, 4, 0, 0);
            input.BackColor = Color.White;
            input.Font = new Font("Segoe UI", 10F);

            switch (input)
            {
                case ComboBox combo:
                    combo.AutoSize = false;
                    combo.Height = 34;
                    combo.DropDownHeight = 240;
                    break;
                case DateTimePicker date:
                    date.Height = 34;
                    break;
                case NumericUpDown number:
                    number.Height = 34;
                    number.TextAlign = HorizontalAlignment.Right;
                    break;
                case TextBox text:
                    text.MinimumSize = new Size(0, 30);
                    text.BorderStyle = BorderStyle.FixedSingle;
                    text.TextAlign = HorizontalAlignment.Right;
                    break;
                case CheckBox check:
                    check.Height = 34;
                    break;
            }
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
                FlatStyle = FlatStyle.Standard,
                UseVisualStyleBackColor = true,
                BackColor = SystemColors.Control,
                ForeColor = SystemColors.ControlText,
                Cursor = Cursors.Default,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            button.Click += handler;
            return button;
        }

        private async Task LoadAsync()
        {
            try
            {
                UseWaitCursor = true;
                using var response = await _client.GetAsync(_endpoint);
                if (!response.IsSuccessStatusCode)
                {
                    throw new ReferenceDataLoadException(response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                var rows = await response.Content.ReadFromJsonAsync<List<Dictionary<string, JsonElement>>>() ?? new();
                _grid.Rows.Clear();

                foreach (var row in rows)
                {
                    var index = _grid.Rows.Add();
                    var gridRow = _grid.Rows[index];
                    gridRow.Tag = row;

                    foreach (var field in _fields)
                    {
                        if (row.TryGetValue(field.Code, out var value))
                        {
                            var display = ReadJsonValue(value);
                            if (IsFiscalPeriodsScreen && field.Code == "Is_Closed")
                            {
                                var closed = value.ValueKind == JsonValueKind.True ||
                                             (value.ValueKind == JsonValueKind.String && bool.TryParse(value.GetString(), out var parsed) && parsed);
                                display = closed ? "مقفلة" : "مفتوحة";
                                gridRow.Cells[field.Code].Style.ForeColor = closed
                                    ? Color.FromArgb(185, 28, 28)
                                    : Color.FromArgb(5, 122, 85);
                                gridRow.Cells[field.Code].Style.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                            }

                            gridRow.Cells[field.Code].Value = display;
                        }
                    }
                }

                _recordCount.Text = $"عدد السجلات: {_grid.Rows.Count}";
                _emptyState.Visible = _grid.Rows.Count == 0;
                ClearEditor();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    BuildLoadErrorMessage(ex),
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        /// <summary>
        /// يحول أخطاء التحميل التقنية إلى رسالة عربية قابلة للتنفيذ.
        /// حالة 500 في هذه الشاشات تعني غالباً أن جداول المرحلة الأولى أو أعمدتها
        /// لم تُطبّق بعد على قاعدة البيانات، وليست خطأ في إدخال المستخدم.
        /// </summary>
        private static string BuildLoadErrorMessage(Exception exception)
        {
            if (exception is ReferenceDataLoadException apiError &&
                (int)apiError.StatusCode >= 500)
            {
                return "تعذر تحميل البيانات لأن قاعدة البيانات تحتاج إلى تحديث المرحلة الأولى.\n\n" +
                       "شغّل ملف Database/2026-07-21_phase1_setup_compatibility.sql مرة واحدة، " +
                       "ثم أعد تشغيل خدمة API والشاشة.\n\n" +
                       "رمز الاستجابة: " + (int)apiError.StatusCode + ".";
            }

            return "تعذر تحميل البيانات. تحقق من اتصال API وصلاحيتك ثم أعد المحاولة.";
        }

        /// <summary>
        /// يحمل رمز حالة API ومحتوى الخطأ للاستخدام التشخيصي داخل التطبيق فقط.
        /// </summary>
        private sealed class ReferenceDataLoadException : Exception
        {
            public System.Net.HttpStatusCode StatusCode { get; }

            public ReferenceDataLoadException(System.Net.HttpStatusCode statusCode, string responseContent)
                : base(responseContent)
            {
                StatusCode = statusCode;
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

            _selectedId = row.TryGetValue(_idProperty, out var id)
                ? ReadIdentifier(id)
                : null;

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
                    case DateTimePicker datePicker:
                        if (json.ValueKind == JsonValueKind.String &&
                            DateTime.TryParse(json.GetString(), out var parsedDate))
                        {
                            datePicker.Value = parsedDate;
                            datePicker.Checked = true;
                        }
                        else
                        {
                            datePicker.Checked = false;
                        }
                        break;
                    case ComboBox comboBox:
                        var selected = ReadJsonValue(json);
                        var matchingOption = comboBox.Items.Cast<object>()
                            .FirstOrDefault(item =>
                                string.Equals(item?.ToString(), selected, StringComparison.OrdinalIgnoreCase) ||
                                item?.ToString()?.StartsWith(selected + " |", StringComparison.OrdinalIgnoreCase) == true);
                        if (matchingOption != null)
                            comboBox.SelectedItem = matchingOption;
                        break;
                    case TextBox textBox:
                        textBox.Text = ReadJsonValue(json);
                        break;
                }
            }

            ConfigureFiscalPeriodLifecycleInputs();
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
                     field.Code.EndsWith("_Key", StringComparison.Ordinal) ||
                     field.Code.EndsWith("_Name_AR", StringComparison.Ordinal) ||
                     field.Code.EndsWith("_Name", StringComparison.Ordinal)))
                .FirstOrDefault(field => _inputs[field.Code] is TextBox textBox && string.IsNullOrWhiteSpace(textBox.Text));

            if (required != null)
            {
                MessageBox.Show($"الحقل «{required.Caption}» مطلوب.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _inputs[required.Code].Focus();
                return;
            }

            var payload = new Dictionary<string, object?> { [_idProperty] = _selectedId ?? _newIdValue };

            foreach (var field in _fields)
            {
                var input = _inputs[field.Code];
                payload[field.Code] = input switch
                {
                    CheckBox checkBox => checkBox.Checked,
                    // لا نقرب القيم العشرية: سعر الصرف قد يصل إلى ست منازل عشرية.
                    NumericUpDown number => number.DecimalPlaces == 0
                        ? Convert.ToInt32(number.Value)
                        : number.Value,
                    DateTimePicker datePicker => datePicker.Checked ? datePicker.Value.Date : null,
                    ComboBox comboBox => comboBox.SelectedItem?.ToString(),
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
            _selectedId = null;
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
                    case DateTimePicker datePicker:
                        datePicker.Value = DateTime.Today;
                        datePicker.Checked = false;
                        break;
                    case ComboBox comboBox when comboBox.Items.Count > 0:
                        comboBox.SelectedIndex = 0;
                        break;
                    case TextBox textBox:
                        textBox.Clear();
                        break;
                }
            }

            _grid.ClearSelection();
            ConfigureFiscalPeriodLifecycleInputs();
        }

        private bool IsFiscalPeriodsScreen =>
            string.Equals(_endpoint, "FiscalPeriods", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// حقول حالة الإقفال سجلٌ معلوماتي فقط. لا تُعدل مباشرة؛ إذ تُغيّر عبر
        /// زري الإقفال/الفتح حتى يبقى السبب والتدقيق إلزاميين على الخادم.
        /// </summary>
        private void ConfigureFiscalPeriodLifecycleInputs()
        {
            if (!IsFiscalPeriodsScreen)
                return;

            if (_inputs.TryGetValue("Is_Closed", out var closed))
            {
                closed.Enabled = false;
                if (closed is CheckBox check)
                    check.Text = check.Checked ? "مقفلة" : "مفتوحة";
            }

            if (_inputs.TryGetValue("Close_Date", out var closeDate))
                closeDate.Enabled = false;

            if (_inputs.TryGetValue("Close_Reason", out var closeReason))
                closeReason.Enabled = false;
        }

        private async Task ChangeFiscalPeriodLifecycleAsync(bool reopen)
        {
            if (_selectedId is not int periodId || periodId <= 0)
            {
                MessageBox.Show("اختر الفترة المطلوبة من الجدول أولاً.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var reason = PromptForReason(reopen ? "سبب إعادة فتح الفترة" : "سبب إقفال الفترة");
            if (string.IsNullOrWhiteSpace(reason))
                return;

            try
            {
                UseWaitCursor = true;
                var action = reopen ? "Reopen" : "Close";
                using var response = await _client.PostAsJsonAsync($"{_endpoint}/{periodId}/{action}", new { Reason = reason });
                if (!response.IsSuccessStatusCode)
                    throw new InvalidOperationException(await response.Content.ReadAsStringAsync());

                MessageBox.Show(reopen ? "تمت إعادة فتح الفترة بنجاح." : "تم إقفال الفترة بنجاح.",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر تنفيذ الإجراء. تحقق من الصلاحية وحالة الفترة والمستندات المعلقة.\n\n" + ex.Message,
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private string? PromptForReason(string title)
        {
            using var dialog = new Form
            {
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                RightToLeft = RightToLeft.Yes,
                RightToLeftLayout = true,
                Font = new Font("Segoe UI", 10F),
                ClientSize = new Size(470, 170),
                MinimizeBox = false,
                MaximizeBox = false,
                FormBorderStyle = FormBorderStyle.FixedDialog
            };

            var label = new Label
            {
                Text = "السبب إلزامي ويسجل في سجل التدقيق:",
                Dock = DockStyle.Top,
                Height = 32,
                Padding = new Padding(12, 8, 12, 0),
                TextAlign = ContentAlignment.MiddleRight
            };
            var reason = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 54,
                Multiline = true,
                TextAlign = HorizontalAlignment.Right,
                Margin = new Padding(12),
                MaxLength = 500
            };
            var confirm = new Button { Text = "تأكيد", DialogResult = DialogResult.OK, Width = 100, Height = 32 };
            var cancel = new Button { Text = "إلغاء", DialogResult = DialogResult.Cancel, Width = 100, Height = 32 };
            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 46,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(12, 6, 12, 6)
            };
            buttons.Controls.Add(confirm);
            buttons.Controls.Add(cancel);
            dialog.Controls.Add(buttons);
            dialog.Controls.Add(reason);
            dialog.Controls.Add(label);
            dialog.AcceptButton = confirm;
            dialog.CancelButton = cancel;

            return dialog.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(reason.Text)
                ? reason.Text.Trim()
                : null;
        }

        private void FrmVoucherReferenceEditor_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                _ = SaveAsync();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F3)
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

        private static object? ReadIdentifier(JsonElement value) => value.ValueKind switch
        {
            JsonValueKind.Number when value.TryGetInt32(out var integer) => integer,
            JsonValueKind.Number when value.TryGetInt64(out var longInteger) => longInteger,
            JsonValueKind.String => value.GetString(),
            _ => null
        };

        private static string ReadJsonValue(JsonElement value) => value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? string.Empty,
            JsonValueKind.True => "نعم",
            JsonValueKind.False => "لا",
            JsonValueKind.Number => value.ToString(),
            _ => string.Empty
        };
    }

    public enum ReferenceEditorFieldKind { Text, Number, Boolean, Date, Choice }

    public sealed record ReferenceEditorField(
        string Code,
        string Caption,
        ReferenceEditorFieldKind Kind = ReferenceEditorFieldKind.Text,
        int MaxLength = 150,
        bool DefaultBoolean = false,
        IReadOnlyList<string>? Options = null,
        int DecimalPlaces = 0);
}
