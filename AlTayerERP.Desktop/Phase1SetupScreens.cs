using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// قاعدة موحدة لشاشات تهيئة المرحلة الأولى.
    /// تنشئ واجهة عربية كاملة: أدوات، بحث، حقول بيانات، جدول، ومعلومات النظام.
    /// </summary>
    public abstract class FrmPhase1SetupBase : Form
    {
        protected FrmPhase1SetupBase(string title, params SetupField[] fields)
        {
            Text = title;
            StartPosition = FormStartPosition.CenterParent;
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Segoe UI", 10);
            Width = 1220;
            Height = 760;
            MinimumSize = new Size(980, 650);

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 54, Padding = new Padding(10),
                FlowDirection = FlowDirection.RightToLeft, WrapContents = false
            };
            toolbar.Controls.AddRange(new Control[]
            {
                MakeButton("جديد", (_, _) => ClearInputs()),
                MakeButton("حفظ", (_, _) => MessageBox.Show("سيتم ربط الحفظ بالخدمة عند اعتماد تصميم الشاشة.", Text)),
                MakeButton("تعديل"), MakeButton("حذف"),
                MakeButton("بحث"), MakeButton("تحديث"),
                MakeButton("طباعة"), MakeButton("إغلاق", (_, _) => Close())
            });

            var search = new TextBox { PlaceholderText = "بحث بالكود أو الاسم أو الرقم المرجعي...", Dock = DockStyle.Top, Height = 36, Margin = new Padding(10) };
            var fieldsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(12),
                FlowDirection = FlowDirection.RightToLeft, WrapContents = true
            };

            foreach (var field in fields)
                fieldsPanel.Controls.Add(CreateField(field));

            var grid = new DataGridView
            {
                Dock = DockStyle.Fill, AllowUserToAddRows = false, ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false, BackgroundColor = Color.White
            };
            foreach (var field in fields)
                grid.Columns.Add(new DataGridViewTextBoxColumn { Name = field.Code, HeaderText = field.Caption });

            var status = new StatusStrip();
            status.Items.Add("الحالة: شاشة تهيئة");
            status.Items.Add(new ToolStripStatusLabel { Spring = true });
            status.Items.Add("الشركة والفرع والسنة من الجلسة الحالية");

            Controls.Add(grid);
            Controls.Add(fieldsPanel);
            Controls.Add(search);
            Controls.Add(toolbar);
            Controls.Add(status);
        }

        private static Button MakeButton(string text, EventHandler? click = null)
        {
            var button = new Button { Text = text, Width = 88, Height = 32, Margin = new Padding(4, 0, 4, 0) };
            if (click != null) button.Click += click;
            return button;
        }

        private Control CreateField(SetupField field)
        {
            var panel = new Panel { Width = field.Kind == SetupFieldKind.Notes ? 450 : 220, Height = field.Kind == SetupFieldKind.Notes ? 92 : 62, Margin = new Padding(5) };
            panel.Controls.Add(new Label { Text = field.Caption, Dock = DockStyle.Top, Height = 23 });
            Control input = field.Kind switch
            {
                SetupFieldKind.YesNo => new CheckBox { Text = "فعال", Dock = DockStyle.Fill, Checked = field.DefaultTrue },
                SetupFieldKind.Date => new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short },
                SetupFieldKind.Number => new NumericUpDown { Dock = DockStyle.Fill, DecimalPlaces = 2, Maximum = 999999999 },
                SetupFieldKind.Notes => new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical },
                _ => new TextBox { Dock = DockStyle.Fill, MaxLength = field.MaxLength }
            };
            input.Tag = field.Code;
            panel.Controls.Add(input);
            return panel;
        }

        private void ClearInputs()
        {
            foreach (Control control in Controls)
                ClearInputs(control);
        }

        private static void ClearInputs(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                if (child is TextBox textBox) textBox.Clear();
                if (child is CheckBox checkBox) checkBox.Checked = false;
                if (child.HasChildren) ClearInputs(child);
            }
        }
    }

    public enum SetupFieldKind { Text, Number, Date, YesNo, Notes }
    public sealed record SetupField(string Code, string Caption, SetupFieldKind Kind = SetupFieldKind.Text, int MaxLength = 150, bool DefaultTrue = false);

    public sealed class FrmParties : FrmPhase1SetupBase
    {
        public FrmParties() : base("إدارة الأطراف المالية",
            new("Party_Code", "كود الطرف"), new("Party_Name_AR", "الاسم العربي"),
            new("Party_Name_EN", "الاسم الإنجليزي"), new("Party_Type", "نوع الطرف"),
            new("Mobile_No", "الجوال"), new("Phone_No", "الهاتف"), new("Identity_No", "رقم الهوية"),
            new("Tax_No", "الرقم الضريبي"), new("Account_ID", "الحساب المرتبط"),
            new("Credit_Limit", "الحد الائتماني", SetupFieldKind.Number),
            new("Address", "العنوان", SetupFieldKind.Notes), new("Is_Active", "الحالة", SetupFieldKind.YesNo, DefaultTrue: true),
            new("Notes", "ملاحظات", SetupFieldKind.Notes)) { }
    }

    public sealed class FrmBanks : FrmPhase1SetupBase
    {
        public FrmBanks() : base("إدارة البنوك والحسابات البنكية",
            new("Bank_Code", "كود البنك"), new("Bank_Name_AR", "اسم البنك العربي"),
            new("Bank_Name_EN", "اسم البنك الإنجليزي"), new("Account_No", "رقم الحساب البنكي"),
            new("IBAN", "IBAN"), new("Currency", "العملة"), new("GL_Account", "الحساب المحاسبي"),
            new("Branch_Name", "فرع البنك"), new("Is_Active", "الحالة", SetupFieldKind.YesNo, DefaultTrue: true),
            new("Notes", "ملاحظات", SetupFieldKind.Notes)) { }
    }

    public sealed class FrmFiscalPeriods : FrmPhase1SetupBase
    {
        public FrmFiscalPeriods() : base("إدارة الفترات المالية",
            new("Period_Code", "كود الفترة"), new("Period_Name", "اسم الفترة"),
            new("Start_Date", "تاريخ البداية", SetupFieldKind.Date), new("End_Date", "تاريخ النهاية", SetupFieldKind.Date),
            new("Is_Closed", "مقفلة", SetupFieldKind.YesNo), new("Close_Date", "تاريخ الإقفال", SetupFieldKind.Date),
            new("Close_Reason", "سبب الإقفال", SetupFieldKind.Notes)) { }
    }

    public sealed class FrmExchangeRates : FrmPhase1SetupBase
    {
        public FrmExchangeRates() : base("إدارة أسعار الصرف",
            new("Rate_Date", "تاريخ السعر", SetupFieldKind.Date), new("Currency", "العملة"),
            new("Exchange_Rate", "سعر الصرف", SetupFieldKind.Number), new("Min_Rate", "الحد الأدنى", SetupFieldKind.Number),
            new("Max_Rate", "الحد الأعلى", SetupFieldKind.Number), new("Is_Default", "افتراضي", SetupFieldKind.YesNo),
            new("Notes", "ملاحظات", SetupFieldKind.Notes)) { }
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

    public sealed class FrmApprovalPolicies : FrmPhase1SetupBase
    {
        public FrmApprovalPolicies() : base("سياسات الاعتماد والسقوف المالية",
            new("Policy_Code", "كود السياسة"), new("Policy_Name", "اسم السياسة"),
            new("Document_Type", "نوع المستند"), new("Minimum_Amount", "من مبلغ", SetupFieldKind.Number),
            new("Maximum_Amount", "إلى مبلغ", SetupFieldKind.Number), new("Approver_Role", "دور المعتمد"),
            new("Allow_Creator_Approval", "يسمح باعتماد المنشئ", SetupFieldKind.YesNo),
            new("Is_Active", "فعال", SetupFieldKind.YesNo, DefaultTrue: true), new("Notes", "ملاحظات", SetupFieldKind.Notes)) { }
    }

    public sealed class FrmSystemScreens : FrmPhase1SetupBase
    {
        public FrmSystemScreens() : base("كتالوج شاشات النظام",
            new("Screen_Code", "كود الشاشة"), new("Screen_Name", "اسم الشاشة"),
            new("Module_Name", "النظام/الوحدة"), new("Sort_Order", "ترتيب الظهور", SetupFieldKind.Number),
            new("Is_Active", "فعالة", SetupFieldKind.YesNo, DefaultTrue: true)) { }
    }

    public sealed class FrmGeneralSettings : FrmPhase1SetupBase
    {
        public FrmGeneralSettings() : base("الإعدادات العامة والمالية",
            new("Setting_Key", "مفتاح الإعداد"), new("Setting_Name", "اسم الإعداد"),
            new("Setting_Value", "القيمة"), new("Scope", "النطاق: شركة/فرع/سنة"),
            new("Effective_Date", "تاريخ السريان", SetupFieldKind.Date),
            new("Is_Active", "فعال", SetupFieldKind.YesNo, DefaultTrue: true),
            new("Description", "الوصف", SetupFieldKind.Notes)) { }
    }
}
