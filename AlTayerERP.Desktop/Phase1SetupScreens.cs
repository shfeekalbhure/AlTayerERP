using AlTayerERP.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// قالب مرئي موحّد لشاشات تهيئة المرحلة الأولى.
    /// يحافظ على اتجاه العربية، ويعرض سياق الجلسة، وشريط أدوات واضح،
    /// وحقولاً منظمة، وحالة فارغة مفهومة بدلاً من جدول أبيض غير مفسر.
    /// </summary>
    public abstract class FrmPhase1SetupBase : Form
    {
        private readonly DataGridView _grid = new();
        private readonly TextBox _searchBox = new();
        private readonly Label _emptyState = new();
        private readonly Dictionary<string, Control> _inputs = new();
        private readonly IReadOnlyList<SetupField> _fields;

        protected FrmPhase1SetupBase(string title, params SetupField[] fields)
        {
            _fields = fields;
            Text = title;
            StartPosition = FormStartPosition.CenterParent;
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Segoe UI", 9.5F);
            BackColor = SystemColors.Control;
            Width = 1220;
            Height = 760;
            MinimumSize = new Size(980, 650);
            KeyPreview = true;

            var shell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = BackColor,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(10)
            };
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 0));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            shell.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            shell.Controls.Add(CreateHeader(title), 0, 0);
            shell.Controls.Add(CreateToolbar(), 0, 1);
            shell.Controls.Add(CreateSearchPanel(), 0, 2);
            shell.Controls.Add(CreateEditorCard(), 0, 3);
            shell.Controls.Add(CreateGridCard(), 0, 4);
            Controls.Add(shell);

            KeyDown += FrmPhase1SetupBase_KeyDown;
        }

        private Control CreateHeader(string title)
        {
            var header = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = SystemColors.Control,
                Height = 0,
                Visible = false,
                Padding = Padding.Empty,
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
                BackColor = SystemColors.Control,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(8),
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Margin = new Padding(0, 0, 0, 6)
            };

            toolbar.Controls.Add(CreateButton("إغلاق  Esc", Color.FromArgb(107, 114, 128), (_, _) => Close()));
            toolbar.Controls.Add(CreateButton("تحديث  F5", Color.FromArgb(36, 99, 168), (_, _) => _grid.Refresh()));
            toolbar.Controls.Add(CreateButton("بحث  Ctrl+F", Color.FromArgb(72, 118, 161), (_, _) => _searchBox.Focus()));

            var save = CreateButton("حفظ  Ctrl+S", Color.FromArgb(22, 125, 84), null);
            save.Enabled = false;
            new ToolTip().SetToolTip(save, "هذه الشاشة مرئية في المرحلة الحالية؛ الربط التشغيلي الخاص بها لم يُعتمد بعد.");
            toolbar.Controls.Add(save);

            toolbar.Controls.Add(CreateButton("جديد  F2", Color.FromArgb(28, 125, 184), (_, _) => ClearInputs()));
            return toolbar;
        }

        private Control CreateSearchPanel()
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10, 6, 10, 6),
                Margin = new Padding(0, 0, 0, 6)
            };

            _searchBox.PlaceholderText = "بحث سريع بالكود أو الاسم أو الرقم المرجعي…";
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
                Text = "بيانات السجل",
                Dock = DockStyle.Top,
                Height = 27,
                ForeColor = Color.FromArgb(27, 62, 104),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight
            };

            var fieldsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = true,
                Padding = new Padding(0, 4, 0, 0)
            };

            foreach (var field in _fields)
                fieldsPanel.Controls.Add(CreateField(field));

            card.Controls.Add(fieldsPanel);
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
            foreach (var field in _fields)
            {
                _grid.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = field.Code,
                    HeaderText = field.Caption,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                });
            }

            _emptyState.Text = "لا توجد بيانات للعرض حالياً\nاستخدم «جديد» لإضافة سجل بعد تفعيل خدمة هذه الشاشة.";
            _emptyState.Dock = DockStyle.Fill;
            _emptyState.TextAlign = ContentAlignment.MiddleCenter;
            _emptyState.Font = new Font("Segoe UI", 10F);
            _emptyState.ForeColor = Color.FromArgb(107, 114, 128);
            _emptyState.BackColor = Color.White;

            card.Controls.Add(_grid);
            card.Controls.Add(_emptyState);
            _emptyState.BringToFront();
            return card;
        }

        private void ConfigureGrid()
        {
            _grid.Dock = DockStyle.Fill;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.ReadOnly = true;
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.MultiSelect = false;
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

        private Control CreateField(SetupField field)
        {
            var isNotes = field.Kind == SetupFieldKind.Notes;
            var panel = new Panel
            {
                // إطار مستقل لكل حقل يمنع ظهور الحقول المسطحة أو غير الواضحة.
                Width = isNotes ? 520 : 250,
                Height = isNotes ? 126 : 86,
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
                SetupFieldKind.YesNo => new CheckBox
                {
                    Text = "نعم",
                    Dock = DockStyle.Fill,
                    Checked = field.DefaultTrue,
                    TextAlign = ContentAlignment.MiddleRight
                },
                SetupFieldKind.Date => new DateTimePicker
                {
                    Dock = DockStyle.Fill,
                    Format = DateTimePickerFormat.Short,
                    CalendarFont = new Font("Segoe UI", 9F)
                },
                SetupFieldKind.Number => new NumericUpDown
                {
                    Dock = DockStyle.Fill,
                    DecimalPlaces = 2,
                    Maximum = 999999999,
                    ThousandsSeparator = true
                },
                SetupFieldKind.Notes => new TextBox
                {
                    Dock = DockStyle.Fill,
                    Multiline = true,
                    ScrollBars = ScrollBars.Vertical,
                    BorderStyle = BorderStyle.FixedSingle
                },
                _ => new TextBox
                {
                    Dock = DockStyle.Fill,
                    MaxLength = field.MaxLength,
                    BorderStyle = BorderStyle.FixedSingle
                }
            };

            ConfigureSetupInput(input, isNotes);

            input.Tag = field;
            _inputs[field.Code] = input;
            panel.Controls.Add(input);
            return panel;
        }

        /// <summary>
        /// يرفع وضوح حقول التهيئة والمنسدلات والتواريخ في جميع شاشات الإعداد.
        /// </summary>
        private static void ConfigureSetupInput(Control input, bool isNotes)
        {
            input.Dock = DockStyle.Bottom;
            input.Height = isNotes ? 82 : 34;
            input.Margin = new Padding(0, 4, 0, 0);
            input.Font = new Font("Segoe UI", 10F);
            input.BackColor = Color.White;

            switch (input)
            {
                case DateTimePicker date:
                    date.Height = 34;
                    break;
                case NumericUpDown number:
                    number.Height = 34;
                    number.TextAlign = HorizontalAlignment.Right;
                    break;
                case TextBox text when !isNotes:
                    text.MinimumSize = new Size(0, 30);
                    text.BorderStyle = BorderStyle.FixedSingle;
                    text.TextAlign = HorizontalAlignment.Right;
                    break;
                case CheckBox check:
                    check.Height = 34;
                    break;
            }
        }

        private static Button CreateButton(string text, Color color, EventHandler? click)
        {
            var button = new Button
            {
                Text = text,
                AutoSize = false,
                Width = 118,
                Height = 32,
                Margin = new Padding(4, 0, 4, 0),
                FlatStyle = FlatStyle.Standard,
                UseVisualStyleBackColor = true,
                BackColor = SystemColors.Control,
                ForeColor = SystemColors.ControlText,
                Cursor = Cursors.Default,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };

            if (click != null)
                button.Click += click;

            return button;
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
        }

        private void ClearInputs()
        {
            foreach (var pair in _inputs)
            {
                var defaultValue = pair.Value.Tag as SetupField;
                switch (pair.Value)
                {
                    case TextBox textBox:
                        textBox.Clear();
                        break;
                    case CheckBox checkBox:
                        checkBox.Checked = defaultValue?.DefaultTrue ?? false;
                        break;
                    case NumericUpDown number:
                        number.Value = 0;
                        break;
                    case DateTimePicker date:
                        date.Value = DateTime.Today;
                        break;
                }
            }

            _searchBox.Clear();
            _grid.ClearSelection();
        }

        private void FrmPhase1SetupBase_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                ClearInputs();
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
    }

    public enum SetupFieldKind { Text, Number, Date, YesNo, Notes }
    public sealed record SetupField(string Code, string Caption, SetupFieldKind Kind = SetupFieldKind.Text, int MaxLength = 150, bool DefaultTrue = false);

    /// <summary>
    /// إدارة فعلية للأطراف المالية ضمن الشركة الحالية، لا تقبل نطاق شركة من الواجهة.
    /// </summary>
    public sealed class FrmParties : FrmVoucherReferenceEditor
    {
        public FrmParties() : base("إدارة الأطراف المالية", "Parties", "Party_ID",
            new("Party_Code", "كود الطرف"),
            new("Party_Name_AR", "الاسم العربي"),
            new("Party_Name_EN", "الاسم الإنجليزي"),
            new("Party_Type", "نوع الطرف", ReferenceEditorFieldKind.Choice,
                Options: new[] { "عميل", "مورد", "موظف", "مندوب", "وكيل", "جهة حكومية", "أخرى" }),
            new("Mobile_No", "الجوال"),
            new("Phone_No", "الهاتف"),
            new("Identity_No", "رقم الهوية"),
            new("Tax_No", "الرقم الضريبي"),
            new("Account_ID", "الحساب المرتبط"),
            new("Credit_Limit", "الحد الائتماني", ReferenceEditorFieldKind.Number),
            new("Address", "العنوان"),
            new("Notes", "ملاحظات"),
            new("Is_Active", "فعال", ReferenceEditorFieldKind.Boolean, DefaultBoolean: true)) { }
    }

    /// <summary>
    /// إدارة فعلية للحسابات البنكية ضمن الشركة الحالية.
    /// </summary>
    public sealed class FrmBanks : FrmVoucherReferenceEditor
    {
        public FrmBanks() : base("إدارة البنوك والحسابات البنكية", "BankAccounts", "Bank_Account_ID",
            new("Bank_Code", "كود البنك"),
            new("Bank_Name_AR", "اسم البنك العربي"),
            new("Bank_Name_EN", "اسم البنك الإنجليزي"),
            new("Account_No", "رقم الحساب البنكي"),
            new("IBAN", "IBAN"),
            new("Currency_Code", "كود العملة"),
            new("GL_Account", "الحساب المحاسبي"),
            new("Branch_Name", "فرع البنك"),
            new("Notes", "ملاحظات"),
            new("Is_Active", "الحالة", ReferenceEditorFieldKind.Boolean, DefaultBoolean: true)) { }
    }

    /// <summary>
    /// فترات فعلية للسنة والفرع الحاليين؛ يمنع الخادم التداخل والإقفال بلا سبب.
    /// </summary>
    public sealed class FrmFiscalPeriods : FrmVoucherReferenceEditor
    {
        public FrmFiscalPeriods() : base("إدارة الفترات المالية", "FiscalPeriods", "Fiscal_Period_ID",
            new("Period_Code", "كود الفترة"),
            new("Period_Name", "اسم الفترة"),
            new("Start_Date", "تاريخ البداية", ReferenceEditorFieldKind.Date),
            new("End_Date", "تاريخ النهاية", ReferenceEditorFieldKind.Date),
            new("Is_Closed", "الحالة", ReferenceEditorFieldKind.Boolean),
            new("Close_Date", "تاريخ الإقفال", ReferenceEditorFieldKind.Date),
            new("Close_Reason", "سبب الإقفال"),
            new("Is_Active", "فعالة", ReferenceEditorFieldKind.Boolean, DefaultBoolean: true)) { }
    }

    /// <summary>
    /// سجل تاريخي فعلي لأسعار الصرف؛ يتحقق الخادم من حدود السعر والعملة المحلية.
    /// </summary>
    public sealed class FrmExchangeRates : FrmVoucherReferenceEditor
    {
        public FrmExchangeRates() : base("إدارة أسعار الصرف", "ExchangeRates", "Exchange_Rate_ID",
            new("Rate_Date", "تاريخ السريان", ReferenceEditorFieldKind.Date),
            new("Currency_Code", "كود العملة"),
            new("Exchange_Rate", "سعر الصرف", ReferenceEditorFieldKind.Number, DecimalPlaces: 6),
            new("Min_Rate", "الحد الأدنى", ReferenceEditorFieldKind.Number, DecimalPlaces: 6),
            new("Max_Rate", "الحد الأعلى", ReferenceEditorFieldKind.Number, DecimalPlaces: 6),
            new("Is_Default", "السعر الافتراضي", ReferenceEditorFieldKind.Boolean),
            new("Notes", "ملاحظات"),
            new("Is_Active", "فعال", ReferenceEditorFieldKind.Boolean, DefaultBoolean: true)) { }
    }

    public sealed class FrmPaymentMethods : FrmVoucherReferenceEditor
    {
        public FrmPaymentMethods() : base("إدارة طرق السداد", "VoucherReferenceData/PaymentMethods", "Payment_Method_ID",
            new("Payment_Method_Code", "الكود"),
            new("Payment_Method_Name_AR", "الاسم العربي"),
            new("Payment_Method_Name_EN", "الاسم الإنجليزي"),
            new("Requires_Reference", "يتطلب مرجع", ReferenceEditorFieldKind.Boolean),
            new("Requires_Reference_Date", "يتطلب تاريخ مرجع", ReferenceEditorFieldKind.Boolean),
            new("Is_Cash", "نقدي", ReferenceEditorFieldKind.Boolean),
            new("Is_Bank", "بنكي", ReferenceEditorFieldKind.Boolean),
            new("Sort_Order", "ترتيب الظهور", ReferenceEditorFieldKind.Number),
            new("Is_Active", "فعال", ReferenceEditorFieldKind.Boolean, DefaultBoolean: true)) { }
    }

    public sealed class FrmVoucherTypes : FrmVoucherReferenceEditor
    {
        public FrmVoucherTypes() : base("إدارة أنواع السندات", "VoucherReferenceData/VoucherTypes", "Voucher_Type_ID",
            new("Voucher_Type_Code", "الكود"),
            new("Voucher_Type_Name_AR", "الاسم العربي"),
            new("Voucher_Type_Name_EN", "الاسم الإنجليزي"),
            new("Sort_Order", "ترتيب الظهور", ReferenceEditorFieldKind.Number),
            new("Is_Active", "فعال", ReferenceEditorFieldKind.Boolean, DefaultBoolean: true)) { }
    }

    public sealed class FrmVoucherStatuses : FrmVoucherReferenceEditor
    {
        public FrmVoucherStatuses() : base("إدارة حالات السندات", "VoucherReferenceData/VoucherStatuses", "Voucher_Status_ID",
            new("Voucher_Status_Code", "الكود"),
            new("Voucher_Status_Name_AR", "الاسم العربي"),
            new("Voucher_Status_Name_EN", "الاسم الإنجليزي"),
            new("Sort_Order", "ترتيب الظهور", ReferenceEditorFieldKind.Number),
            new("Is_Active", "فعال", ReferenceEditorFieldKind.Boolean, DefaultBoolean: true)) { }
    }

    /// <summary>
    /// سياسات وسقوف مالية فعلية للشركة الحالية؛ النطاق يفرضه الخادم من الجلسة.
    /// </summary>
    public sealed class FrmApprovalPolicies : FrmVoucherReferenceEditor
    {
        public FrmApprovalPolicies() : base("سياسات الاعتماد والسقوف المالية", "FinancialPolicies", "Limit_ID",
            new("Entity_Type", "نوع الجهة", ReferenceEditorFieldKind.Choice,
                Options: new[] { "حساب", "صندوق", "بنك", "عميل", "مورد", "فرع" }),
            new("Entity_ID", "رقم الجهة"),
            new("Limit_Type", "نوع السقف", ReferenceEditorFieldKind.Choice,
                Options: new[] { "قبض", "صرف", "مديونية", "خصم" }),
            new("Currency_Code", "كود العملة"),
            new("Limit_Amount", "قيمة السقف", ReferenceEditorFieldKind.Number),
            new("Period_Type", "فترة السقف", ReferenceEditorFieldKind.Choice,
                Options: new[] { "Daily", "Monthly", "Yearly" }),
            new("Requires_Approval", "يتطلب اعتماد", ReferenceEditorFieldKind.Boolean, DefaultBoolean: true),
            new("Is_Active", "فعال", ReferenceEditorFieldKind.Boolean, DefaultBoolean: true)) { }
    }

    /// <summary>
    /// كتالوج فعلي للشاشات: يحفظ عبر API ويغذي صلاحيات الأدوار وشجرة النظام.
    /// </summary>
    public sealed class FrmSystemScreens : FrmVoucherReferenceEditor
    {
        public FrmSystemScreens() : base("كتالوج شاشات النظام", "SystemScreens", "Screen_ID",
            new("Screen_Code", "كود الشاشة"),
            new("Screen_Name", "اسم الشاشة"),
            new("Module_Name", "النظام/الوحدة"),
            new("Sort_Order", "ترتيب الظهور", ReferenceEditorFieldKind.Number),
            new("Is_Active", "فعالة", ReferenceEditorFieldKind.Boolean, DefaultBoolean: true)) { }
    }

    /// <summary>
    /// إعدادات فعلية متعددة المستويات: عام، شركة، فرع، أو سنة مالية.
    /// يفرض API سياق الشركة والفرع والسنة من الجلسة الحالية.
    /// </summary>
    public sealed class FrmGeneralSettings : FrmVoucherReferenceEditor
    {
        public FrmGeneralSettings() : base("الإعدادات العامة والمالية", "SystemSettings", "Setting_ID",
            new("Setting_Key", "مفتاح الإعداد"),
            new("Setting_Name", "اسم الإعداد"),
            new("Setting_Value", "القيمة"),
            new("Scope", "نطاق الإعداد", ReferenceEditorFieldKind.Choice,
                Options: new[]
                {
                    "SYSTEM | عام للنظام",
                    "COMPANY | خاص بالشركة",
                    "BRANCH | خاص بالفرع",
                    "FISCAL_YEAR | خاص بالسنة المالية"
                }),
            new("Effective_Date", "تاريخ السريان", ReferenceEditorFieldKind.Date),
            new("Description", "الوصف"),
            new("Is_Active", "فعال", ReferenceEditorFieldKind.Boolean, DefaultBoolean: true)) { }
    }
}
