using AlTayerERP.Desktop.Common;
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
        private readonly Dictionary<string, Label> _auditValues = new();
        private DateTime? _fiscalYearStart;
        private DateTime? _fiscalYearEnd;
        // شاشة الفترات تحتاج مساحة إضافية لحقول التواريخ والإقفال، بخلاف القوائم المرجعية الأخرى.
        private bool IsFiscalPeriods => string.Equals(_endpoint, "FiscalPeriods", StringComparison.OrdinalIgnoreCase);
        private bool IsParties => string.Equals(_endpoint, "Parties", StringComparison.OrdinalIgnoreCase);
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
        private Button? _saveButton;
        private Button? _closePeriodButton;
        private Button? _reopenPeriodButton;
        // زر الإيقاف لا يحذف الطرف؛ يحافظ على تاريخه ومستنداته.
        private Button? _deactivateButton;

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
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 0));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            // تكبير حاوية البحث في الفترات بمقدار نصف سنتيمتر تقريباً (19px).
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, IsFiscalPeriods ? 59 : 40));
            shell.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            // جدول الفترات أصغر بمقدار 2 سم تقريباً (76px) من المساحة المعتادة.
            // نخفض جدول الفترات 2 سم إضافية لإظهار حاوية التدقيق أسفله.
            shell.RowStyles.Add(IsFiscalPeriods
                ? new RowStyle(SizeType.Absolute, 289)
                : new RowStyle(SizeType.Percent, 100));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, IsFiscalPeriods ? 96 : 28));

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
                BackColor = SystemColors.Control,
                Height = 0,
                Visible = false,
                Padding = System.Windows.Forms.Padding.Empty,
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
            // الحاوية تثبت شريط الأزرار في أقصى اليمين حتى داخل نافذة MDI.
            var host = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BackColor,
                Margin = new Padding(0, 0, 0, 6)
            };
            var toolbar = new FlowLayoutPanel
            {
                // عرض ثابت يمنع ظهور شريط تمرير عند وجود أزرار الفترات كلها.
                AutoSize = false,
                Width = IsFiscalPeriods ? 980 : IsParties ? 820 : 670,
                Height = 42,
                Dock = DockStyle.Right,
                BackColor = BackColor,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(0, 4, 0, 4),
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                AutoScroll = false
            };

            // أول زر مضاف يظهر في أقصى يمين الشريط.
            toolbar.Controls.Add(CreateButton("جديد  F2", Color.FromArgb(28, 125, 184), (_, _) => ClearEditor()));
            _saveButton = CreateButton("حفظ  Ctrl+S", Color.FromArgb(22, 125, 84), async (_, _) => await SaveAsync());
            toolbar.Controls.Add(_saveButton);
            toolbar.Controls.Add(CreateButton("تعديل", Color.FromArgb(37, 99, 235), (_, _) => BeginEdit()));

            if (IsFiscalPeriods)
            {
                _closePeriodButton = CreateButton("إقفال الفترة", Color.FromArgb(185, 28, 28),
                    async (_, _) => await RunFiscalPeriodLifecycleAsync("Close", "إقفال"));
                _reopenPeriodButton = CreateButton("إعادة الفتح", Color.FromArgb(180, 83, 9),
                    async (_, _) => await RunFiscalPeriodLifecycleAsync("Reopen", "إعادة فتح"));
                toolbar.Controls.Add(_closePeriodButton);
                toolbar.Controls.Add(_reopenPeriodButton);
            }

            if (IsParties)
            {
                _deactivateButton = CreateButton("إيقاف الطرف", Color.FromArgb(185, 28, 28),
                    async (_, _) => await DeactivatePartyAsync());
                toolbar.Controls.Add(_deactivateButton);
            }

            toolbar.Controls.Add(CreateButton("تحديث  F5", Color.FromArgb(36, 99, 168), async (_, _) => await LoadAsync()));
            toolbar.Controls.Add(CreateButton("إغلاق  Esc", Color.FromArgb(107, 114, 128), (_, _) => Close()));
            host.Controls.Add(toolbar);
            UpdateFiscalPeriodActions();
            return host;
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
                // يبقى ارتفاع الحاوية ثابتاً؛ نعوض تقليل البطاقات من المساحة السفلية.
                Padding = IsFiscalPeriods
                    ? new Padding(14, 10, 14, 124)
                    : IsParties
                        // تكبير حاوية بيانات الأطراف المالية 4 سم تقريباً (152px)
                        // يخفض مساحة الجدول بنفس المقدار مع الحفاظ على ترتيب الحقول.
                        ? new Padding(14, 10, 14, 162)
                        : new Padding(14, 10, 14, 10),
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

            // في RTL آخر بطاقة مضافة تظهر في أقصى اليمين؛ نعكسها ليبدأ بكود الفترة.
            var editorFields = IsFiscalPeriods ? _fields.Reverse() : _fields;
            foreach (var field in editorFields)
            {
                // إطار مستقل لكل حقل حتى لا تختفي حدود الإدخال في الشاشات العربية.
                var panel = new Panel
                {
                    Width = 250,
                    // بطاقات الفترات أقل نصف سم (19px)؛ فيرتفع حقلها والصف الثاني تلقائياً.
                    Height = IsFiscalPeriods ? 96 : 86,
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

                // الحقول التشغيلية لا تعدل يدوياً؛ تنفذ عبر الإقفال وإعادة الفتح المدقّقين.
                if (IsFiscalPeriods &&
                    (field.Code == "Is_Closed" || field.Code == "Close_Date" || field.Code == "Close_Reason"))
                    input.Enabled = false;

                _inputs[field.Code] = input;

                if (IsParties && field.Code == "Account_ID" && input is TextBox accountBox)
                {
                    // الحساب يحفظ بمعرفه الداخلي؛ زر الاختيار يمنع إدخال رقم غير صالح يرفضه الخادم.
                    accountBox.ReadOnly = true;
                    accountBox.Dock = DockStyle.Fill;

                    var lookupHost = new Panel { Dock = DockStyle.Bottom, Height = 34 };
                    var lookupButton = new Button
                    {
                        Text = "اختيار",
                        Dock = DockStyle.Left,
                        Width = 66,
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.FromArgb(37, 99, 235),
                        ForeColor = Color.White,
                        Cursor = Cursors.Hand
                    };
                    lookupButton.Click += (_, _) => SelectPartyAccount(accountBox);
                    lookupHost.Controls.Add(accountBox);
                    lookupHost.Controls.Add(lookupButton);
                    panel.Controls.Add(lookupHost);
                }
                else
                {
                    panel.Controls.Add(input);
                }

                editor.Controls.Add(panel);
            }

            // ترتيب أعمدة الجدول مستقل عن البطاقات ويبدأ بكود الفترة من اليمين.
            foreach (var field in _fields)
            {
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
            if (IsFiscalPeriods)
                return CreateFiscalAuditFooter();

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

        private Control CreateFiscalAuditFooter()
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(8, 4, 8, 4)
            };
            var title = new Label
            {
                Text = "بيانات الإنشاء والتعديل",
                Dock = DockStyle.Top,
                Height = 20,
                ForeColor = Color.FromArgb(27, 62, 104),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight
            };
            var fields = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                AutoScroll = false,
                Padding = Padding.Empty
            };

            AddAuditField(fields, "Created_By", "أنشئ بواسطة");
            AddAuditField(fields, "Created_At", "تاريخ الإنشاء");
            AddAuditField(fields, "Updated_By", "عُدّل بواسطة");
            AddAuditField(fields, "Updated_At", "تاريخ التعديل");
            AddAuditField(fields, "Edit_Count", "عدد التعديلات");
            AddAuditField(fields, "Print_Count", "عدد الطباعة");

            card.Controls.Add(fields);
            card.Controls.Add(title);
            return card;
        }

        private void AddAuditField(FlowLayoutPanel fields, string key, string caption)
        {
            var item = new Panel
            {
                Width = 205,
                Height = 55,
                Margin = new Padding(3, 0, 3, 0),
                Padding = new Padding(6, 2, 6, 3),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(248, 250, 252)
            };
            item.Controls.Add(new Label
            {
                Text = caption,
                Dock = DockStyle.Top,
                Height = 20,
                ForeColor = Color.FromArgb(75, 85, 99),
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 8F)
            });
            var value = new Label
            {
                Text = "—",
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(31, 41, 55),
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
            };
            _auditValues[key] = value;
            item.Controls.Add(value);
            fields.Controls.Add(item);
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
                // لون نص بيانات الجدول: رمادي أسود واضح على الخلفية البيضاء.
                ForeColor = Color.FromArgb(31, 41, 55),
                SelectionBackColor = Color.FromArgb(218, 232, 247),
                SelectionForeColor = Color.FromArgb(20, 44, 75),
                Alignment = DataGridViewContentAlignment.MiddleRight
            };
            _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            _grid.AlternatingRowsDefaultCellStyle.ForeColor = Color.FromArgb(31, 41, 55);
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
                UseVisualStyleBackColor = false,
                BackColor = color,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            button.Click += handler;
            return button;
        }

        private async Task LoadAsync()
        {
            try
            {
                UseWaitCursor = true;
                if (IsFiscalPeriods)
                    await LoadFiscalYearBoundsAsync();

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
                        if (TryGetJsonValue(row, field.Code, out var value))
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

            _selectedId = TryGetJsonValue(row, _idProperty, out var id)
                ? ReadIdentifier(id)
                : null;

            foreach (var field in _fields)
            {
                if (!TryGetJsonValue(row, field.Code, out var json) || !_inputs.TryGetValue(field.Code, out var input))
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
                        var selected = GetChoiceDisplayValue(field, ReadJsonValue(json));
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

            UpdateAuditFooter(row);
            UpdateFiscalPeriodActions();
        }

        /// <summary>
        /// يفتح دليل الحسابات ويربط الطرف بحساب قابل للترحيل فقط.
        /// </summary>
        private void SelectPartyAccount(TextBox target)
        {
            using var dialog = new FrmAccountLookup(target.Text);
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            target.Text = dialog.SelectedAccountId;
            new ToolTip().SetToolTip(target,
                $"الحساب المختار: {dialog.SelectedAccountCode} - {dialog.SelectedAccountName}");
        }

        /// <summary>
        /// يوقف الطرف بصورة آمنة ولا يحذف تاريخه أو قيوده السابقة.
        /// </summary>
        private async Task DeactivatePartyAsync()
        {
            if (!IsParties || _selectedId is not string id || string.IsNullOrWhiteSpace(id))
            {
                MessageBox.Show("اختر طرفاً مالياً من الجدول أولاً.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var reason = PromptForPartyDeactivationReason();
            if (reason == null)
                return;

            try
            {
                UseWaitCursor = true;
                using var response = await _client.DeleteAsync(
                    $"{_endpoint}/{Uri.EscapeDataString(id)}?reason={Uri.EscapeDataString(reason)}");

                if (!response.IsSuccessStatusCode)
                    throw new InvalidOperationException(await response.Content.ReadAsStringAsync());

                MessageBox.Show("تم إيقاف الطرف المالي دون حذف تاريخه.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر إيقاف الطرف المالي.\n\n" + ExtractApiMessage(ex.Message), Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private string? PromptForPartyDeactivationReason()
        {
            using var dialog = new Form
            {
                Text = "سبب إيقاف الطرف المالي",
                StartPosition = FormStartPosition.CenterParent,
                RightToLeft = RightToLeft.Yes,
                RightToLeftLayout = true,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MinimizeBox = false,
                MaximizeBox = false,
                ShowInTaskbar = false,
                ClientSize = new Size(470, 170)
            };

            var label = new Label
            {
                Text = "اكتب سبب الإيقاف (مطلوب):",
                Dock = DockStyle.Top,
                Height = 34,
                Padding = new Padding(10, 8, 10, 0),
                TextAlign = ContentAlignment.MiddleRight
            };
            var reasonBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                MaxLength = 500,
                TextAlign = HorizontalAlignment.Right
            };
            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 45,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(8)
            };
            var confirm = new Button { Text = "تأكيد", DialogResult = DialogResult.OK, Width = 90 };
            var cancel = new Button { Text = "إلغاء", DialogResult = DialogResult.Cancel, Width = 90 };
            actions.Controls.Add(confirm);
            actions.Controls.Add(cancel);
            dialog.Controls.Add(reasonBox);
            dialog.Controls.Add(actions);
            dialog.Controls.Add(label);
            dialog.AcceptButton = confirm;
            dialog.CancelButton = cancel;

            return dialog.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(reasonBox.Text)
                ? reasonBox.Text.Trim()
                : null;
        }

        private void BeginEdit()
        {
            if (_selectedId == null)
            {
                MessageBox.Show("اختر سجلاً من الجدول أولاً.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (IsFiscalPeriods &&
                _closePeriodButton?.Enabled == false &&
                _reopenPeriodButton?.Enabled == true)
            {
                MessageBox.Show("الفترة مقفلة؛ استخدم «إعادة الفتح» أولاً.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var firstEditable = _inputs.FirstOrDefault(pair => pair.Value.Enabled).Value;
            firstEditable?.Focus();
        }

        private async Task RunFiscalPeriodLifecycleAsync(string operation, string caption)
        {
            if (_selectedId is not int id || id <= 0)
            {
                MessageBox.Show("اختر فترة مالية من الجدول أولاً.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var reason = PromptForReason(caption);
            if (string.IsNullOrWhiteSpace(reason))
                return;

            try
            {
                UseWaitCursor = true;
                using var response = await _client.PostAsJsonAsync(
                    $"{_endpoint}/{id}/{operation}",
                    new Dictionary<string, string> { ["Reason"] = reason.Trim() });

                if (!response.IsSuccessStatusCode)
                    throw new InvalidOperationException(await response.Content.ReadAsStringAsync());

                MessageBox.Show($"تم {caption} الفترة المالية بنجاح.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"تعذر {caption} الفترة المالية.\n\n{ex.Message}", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private string? PromptForReason(string actionCaption)
        {
            using var dialog = new Form
            {
                Text = $"سبب {actionCaption} الفترة",
                StartPosition = FormStartPosition.CenterParent,
                RightToLeft = RightToLeft.Yes,
                RightToLeftLayout = true,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MinimizeBox = false,
                MaximizeBox = false,
                ShowInTaskbar = false,
                ClientSize = new Size(470, 170)
            };
            var label = new Label
            {
                Text = $"اكتب سبب {actionCaption} الفترة المالية (مطلوب):",
                Dock = DockStyle.Top,
                Height = 34,
                Padding = new Padding(10, 8, 10, 0),
                TextAlign = ContentAlignment.MiddleRight
            };
            var reasonBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                MaxLength = 500,
                TextAlign = HorizontalAlignment.Right
            };
            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 45,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(8)
            };
            var confirm = new Button { Text = "تأكيد", DialogResult = DialogResult.OK, Width = 90 };
            var cancel = new Button { Text = "إلغاء", DialogResult = DialogResult.Cancel, Width = 90 };
            actions.Controls.Add(confirm);
            actions.Controls.Add(cancel);
            dialog.Controls.Add(reasonBox);
            dialog.Controls.Add(actions);
            dialog.Controls.Add(label);
            dialog.AcceptButton = confirm;
            dialog.CancelButton = cancel;

            return dialog.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(reasonBox.Text)
                ? reasonBox.Text.Trim()
                : null;
        }

        private void UpdateFiscalPeriodActions()
        {
            if (!IsFiscalPeriods)
                return;

            var isClosed = false;
            if (_grid.CurrentRow?.Tag is Dictionary<string, JsonElement> row &&
                TryGetJsonValue(row, "Is_Closed", out var state))
            {
                isClosed = state.ValueKind == JsonValueKind.True ||
                           (state.ValueKind == JsonValueKind.String &&
                            bool.TryParse(state.GetString(), out var value) && value);
            }

            var hasSelection = _selectedId is int id && id > 0;
            if (_closePeriodButton != null) _closePeriodButton.Enabled = hasSelection && !isClosed;
            if (_reopenPeriodButton != null) _reopenPeriodButton.Enabled = hasSelection && isClosed;
            if (_saveButton != null) _saveButton.Enabled = !hasSelection || !isClosed;

            foreach (var pair in _inputs)
            {
                if (pair.Key == "Is_Closed" || pair.Key == "Close_Date" || pair.Key == "Close_Reason")
                    continue;
                pair.Value.Enabled = !hasSelection || !isClosed;
            }
        }

        private async Task SaveAsync()
        {
            // صلاحيات الفترات (إضافة/تعديل/اعتماد) يحسمها API، لا شرط محلي عام.
            if (!IsFiscalPeriods && !IsParties && !CurrentSession.Is_System_Admin)
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

            // يمنع الإرسال الخاطئ الذي ظهر في الشاشة: تاريخ البداية يجب أن يسبق أو يساوي تاريخ النهاية.
            if (IsFiscalPeriods && !ValidateFiscalPeriodDates())
                return;

            if (IsParties && !ValidatePartyInput())
                return;

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
                    ComboBox comboBox => GetChoicePayloadValue(field, comboBox.SelectedItem?.ToString()),
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
                    "تعذر الحفظ.\n\n" + ExtractApiMessage(ex.Message),
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
        /// يحول مسميات أنواع الأطراف العربية إلى الرموز المعتمدة في API.
        /// </summary>
        private string? GetChoicePayloadValue(ReferenceEditorField field, string? selected)
        {
            if (!IsParties || field.Code != "Party_Type" || string.IsNullOrWhiteSpace(selected))
                return selected;

            return selected.Trim() switch
            {
                "عميل" => "CUSTOMER",
                "مورد" => "VENDOR",
                "موظف" => "EMPLOYEE",
                "سائق" => "DRIVER",
                "مندوب" => "REPRESENTATIVE",
                "وكيل" => "AGENT",
                "جهة حكومية" => "OTHER",
                "أخرى" => "OTHER",
                _ => selected.Trim()
            };
        }

        private string GetChoiceDisplayValue(ReferenceEditorField field, string value)
        {
            if (!IsParties || field.Code != "Party_Type")
                return value;

            return value.Trim().ToUpperInvariant() switch
            {
                "CUSTOMER" => "عميل",
                "VENDOR" => "مورد",
                "EMPLOYEE" => "موظف",
                "DRIVER" => "سائق",
                "REPRESENTATIVE" => "مندوب",
                "AGENT" => "وكيل",
                "OTHER" => "أخرى",
                _ => value
            };
        }

        private bool ValidatePartyInput()
        {
            if (_inputs.TryGetValue("Party_Type", out var input) &&
                input is ComboBox partyType &&
                partyType.SelectedItem == null)
            {
                MessageBox.Show("اختر نوع الطرف المالي.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                partyType.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// يتحقق محلياً من تواريخ الفترة قبل استدعاء API لكي تظهر للمستخدم رسالة مباشرة وواضحة.
        /// </summary>
        private bool ValidateFiscalPeriodDates()
        {
            var start = (DateTimePicker)_inputs["Start_Date"];
            var end = (DateTimePicker)_inputs["End_Date"];

            if (!start.Checked || !end.Checked)
            {
                MessageBox.Show("تاريخ البداية وتاريخ النهاية مطلوبان للفترة المالية.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                (!start.Checked ? start : end).Focus();
                return false;
            }

            if (end.Value.Date < start.Value.Date)
            {
                MessageBox.Show("تاريخ النهاية يجب أن يكون بعد تاريخ البداية أو مساويًا له.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                end.Focus();
                return false;
            }

            if (_fiscalYearStart.HasValue && _fiscalYearEnd.HasValue &&
                (start.Value.Date < _fiscalYearStart.Value.Date || end.Value.Date > _fiscalYearEnd.Value.Date))
            {
                MessageBox.Show(
                    $"تواريخ الفترة يجب أن تقع داخل السنة المالية الحالية: {_fiscalYearStart:yyyy/MM/dd} إلى {_fiscalYearEnd:yyyy/MM/dd}.",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                (start.Value.Date < _fiscalYearStart.Value.Date ? start : end).Focus();
                return false;
            }

            return true;
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
            ResetAuditFooter();
            UpdateFiscalPeriodActions();
        }

        private async Task LoadFiscalYearBoundsAsync()
        {
            try
            {
                using var response = await _client.GetAsync("FiscalYears?includeClosed=true");
                if (!response.IsSuccessStatusCode) return;

                var years = await response.Content.ReadFromJsonAsync<List<Dictionary<string, JsonElement>>>() ?? new();
                var year = years.FirstOrDefault(row =>
                    TryGetJsonValue(row, "Fiscal_Year_ID", out var id) &&
                    ReadIdentifier(id) is int yearId &&
                    yearId == CurrentSession.Year_ID);
                if (year == null ||
                    !TryGetJsonValue(year, "Start_Date", out var startJson) ||
                    !TryGetJsonValue(year, "End_Date", out var endJson) ||
                    !DateTime.TryParse(ReadJsonValue(startJson), out var start) ||
                    !DateTime.TryParse(ReadJsonValue(endJson), out var end))
                    return;

                _fiscalYearStart = start.Date;
                _fiscalYearEnd = end.Date;
                ConfigureFiscalDatePicker("Start_Date");
                ConfigureFiscalDatePicker("End_Date");
            }
            catch
            {
                // يبقى التحقق الخادمي هو مصدر الحقيقة إذا تعذر جلب حدود السنة للواجهة.
            }
        }

        private void ConfigureFiscalDatePicker(string key)
        {
            if (!_inputs.TryGetValue(key, out var input) || input is not DateTimePicker picker ||
                !_fiscalYearStart.HasValue || !_fiscalYearEnd.HasValue)
                return;

            picker.MinDate = _fiscalYearStart.Value;
            picker.MaxDate = _fiscalYearEnd.Value;
            if (picker.Value.Date < picker.MinDate || picker.Value.Date > picker.MaxDate)
                picker.Value = picker.MinDate;
        }

        private void UpdateAuditFooter(IReadOnlyDictionary<string, JsonElement> row)
        {
            foreach (var pair in _auditValues)
            {
                var text = TryGetJsonValue(row, pair.Key, out var value)
                    ? ReadJsonValue(value)
                    : "—";

                if ((pair.Key == "Created_At" || pair.Key == "Updated_At") &&
                    DateTime.TryParse(text, out var date))
                    text = date.ToString("yyyy/MM/dd HH:mm");

                pair.Value.Text = string.IsNullOrWhiteSpace(text) ? "—" : text;
            }
        }

        private void ResetAuditFooter()
        {
            foreach (var value in _auditValues.Values)
                value.Text = "—";
        }

        private static string ExtractApiMessage(string raw)
        {
            try
            {
                using var document = JsonDocument.Parse(raw);
                if (document.RootElement.TryGetProperty("message", out var message))
                    return message.GetString() ?? "تعذر تنفيذ العملية.";
            }
            catch
            {
                // الرسالة ليست JSON؛ تعرض كما هي في السطر التالي.
            }

            return string.IsNullOrWhiteSpace(raw) ? "تحقق من الحقول وصلاحية المستخدم واتصال API." : raw;
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

        private static object? ReadIdentifier(JsonElement value) => value.ValueKind switch
        {
            JsonValueKind.Number when value.TryGetInt32(out var integer) => integer,
            JsonValueKind.Number when value.TryGetInt64(out var longInteger) => longInteger,
            JsonValueKind.String => value.GetString(),
            _ => null
        };

        /// <summary>
        /// يجلب قيمة JSON دون حساسية لحالة الأحرف. ASP.NET Core يحول أول حرف
        /// إلى صغير افتراضياً، بينما أسماء حقول ERP تستخدم شرطة سفلية وأحرفاً كبيرة.
        /// </summary>
        private static bool TryGetJsonValue(
            IReadOnlyDictionary<string, JsonElement> row,
            string propertyName,
            out JsonElement value)
        {
            if (row.TryGetValue(propertyName, out value))
                return true;

            foreach (var item in row)
            {
                if (string.Equals(item.Key, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = item.Value;
                    return true;
                }
            }

            value = default;
            return false;
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
