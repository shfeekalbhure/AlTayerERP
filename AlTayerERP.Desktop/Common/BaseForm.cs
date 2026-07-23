using AlTayerERP.Desktop.Services;
using System.Drawing;
using System.Windows.Forms;

namespace AlTayerERP.Desktop.Common;

/// <summary>
/// القالب المركزي لشاشات المرحلة الأولى (BaseForm).
/// مسؤول عن هوية الواجهة، بطاقة التدقيق، وسياق الجلسة فقط.
/// لا يحتوي حفظاً أو ترحيلاً أو اعتماداً أو فحص صلاحيات فعلياً.
/// </summary>
public abstract class BaseForm : Form
{
    private readonly Label _lblAuditSummary = new();
    private readonly Panel _pnlAuditBody = new();
    private readonly Button _btnToggleAudit = new();

    /// <summary>ينشئ الإعدادات البصرية المشتركة: RTL والخط والخلفية والاختصارات.</summary>
    protected void ApplyBaseFormStyle()
    {
        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = true;
        Font = new Font("Segoe UI", 9.5F);
        BackColor = Color.FromArgb(247, 249, 252);
        KeyPreview = true;
    }

    /// <summary>
    /// بطاقة تدقيق قابلة للطي. القيم للعرض فقط وتُمرر من نتيجة API الموثوقة،
    /// ولا تُبنى من قيم مرسلة من المستخدم أو عناصر الإدخال في الشاشة.
    /// </summary>
    protected Control CreateAuditInfoPanel()
    {
        var container = new Panel
        {
            Name = "pnlAuditInfo",
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(10)
        };

        var header = new Panel { Dock = DockStyle.Top, Height = 30 };
        header.Controls.Add(new Label
        {
            Text = "معلومات النظام والتدقيق",
            Dock = DockStyle.Right,
            Width = 250,
            ForeColor = Color.FromArgb(8, 55, 112),
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleRight
        });

        _btnToggleAudit.Text = "إخفاء التفاصيل";
        _btnToggleAudit.Dock = DockStyle.Left;
        _btnToggleAudit.Width = 120;
        _btnToggleAudit.FlatStyle = FlatStyle.Flat;
        _btnToggleAudit.FlatAppearance.BorderColor = Color.FromArgb(208, 220, 235);
        _btnToggleAudit.Click += (_, _) => ToggleAuditPanel();
        header.Controls.Add(_btnToggleAudit);

        _lblAuditSummary.Dock = DockStyle.Top;
        _lblAuditSummary.Height = 25;
        _lblAuditSummary.ForeColor = Color.FromArgb(75, 85, 99);
        _lblAuditSummary.TextAlign = ContentAlignment.MiddleRight;

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 2,
            Padding = new Padding(0, 5, 0, 0)
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        AddAuditField(table, 0, 0, "أنشئ بواسطة", "—", out var createdBy);
        AddAuditField(table, 1, 0, "تاريخ الإنشاء", "—", out var createdAt);
        AddAuditField(table, 0, 1, "آخر تعديل بواسطة", "—", out var updatedBy);
        AddAuditField(table, 1, 1, "آخر تعديل", "—", out var updatedAt);

        // العلامات Name تستخدمها SetAuditInfo لتحديث العرض فقط.
        createdBy.Name = "auditCreatedBy";
        createdAt.Name = "auditCreatedAt";
        updatedBy.Name = "auditUpdatedBy";
        updatedAt.Name = "auditUpdatedAt";

        _pnlAuditBody.Controls.Add(table);
        _pnlAuditBody.Dock = DockStyle.Fill;

        container.Controls.Add(_pnlAuditBody);
        container.Controls.Add(_lblAuditSummary);
        container.Controls.Add(header);
        SetAuditInfo(null);
        return container;
    }

    /// <summary>
    /// شريط سياق العمل والاتصال للعرض فقط. لا يغير الشركة أو الفرع أو السنة
    /// لأن تغيير النطاق يتم من شاشة الدخول أو النافذة المعتمدة.
    /// </summary>
    protected Control CreateSessionStatusStrip()
    {
        var status = new StatusStrip
        {
            Name = "statusStripSession",
            Dock = DockStyle.Fill,
            SizingGrip = false,
            BackColor = Color.FromArgb(249, 250, 252)
        };

        status.Items.Add(new ToolStripStatusLabel($"الشركة: {CurrentSession.Company_ID}") { Spring = false });
        status.Items.Add(new ToolStripStatusLabel($" | الفرع: {CurrentSession.Branch_ID}"));
        status.Items.Add(new ToolStripStatusLabel($" | السنة: {CurrentSession.Year_ID}"));
        status.Items.Add(new ToolStripStatusLabel($" | المستخدم: {CurrentSession.Username}"));
        status.Items.Add(new ToolStripStatusLabel(" | API: يُفحص من الشاشة الرئيسية"));
        return status;
    }

    /// <summary>
    /// يحدّث بطاقة التدقيق من DTO صادر عن الخادم. لا يوجد Setter عام للحقول
    /// حتى لا تتسلل قيم التدقيق من عناصر WinForms أو من المستخدم.
    /// </summary>
    protected void SetAuditInfo(AuditInfoView? audit)
    {
        var values = audit ?? AuditInfoView.Empty;
        _lblAuditSummary.Text = values.IsAvailable
            ? $"الحالة: {values.RecordStatus} | عدد التعديلات: {values.EditCount} | الطباعة: {values.PrintCount}"
            : "تظهر بيانات التدقيق بعد تحميل سجل محفوظ من الخادم.";

        SetAuditLabel("auditCreatedBy", values.CreatedBy);
        SetAuditLabel("auditCreatedAt", FormatDate(values.CreatedAt));
        SetAuditLabel("auditUpdatedBy", values.UpdatedBy);
        SetAuditLabel("auditUpdatedAt", FormatDate(values.UpdatedAt));
    }

    /// <summary>قلب حالة بطاقة التدقيق دون تغيير أي بيانات أو منطق أعمال.</summary>
    private void ToggleAuditPanel()
    {
        _pnlAuditBody.Visible = !_pnlAuditBody.Visible;
        _btnToggleAudit.Text = _pnlAuditBody.Visible ? "إخفاء التفاصيل" : "إظهار التفاصيل";
    }

    /// <summary>يبني حقلاً للقراءة فقط داخل بطاقة التدقيق.</summary>
    private static void AddAuditField(TableLayoutPanel table, int pairColumn, int row,
        string caption, string value, out Label output)
    {
        int column = pairColumn * 2;
        table.Controls.Add(new Label
        {
            Text = caption,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = Color.FromArgb(31, 52, 82)
        }, column, row);

        output = new Label
        {
            Text = value,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = Color.FromArgb(75, 85, 99),
            BorderStyle = BorderStyle.FixedSingle
        };
        table.Controls.Add(output, column + 1, row);
    }

    /// <summary>يبحث عن حقل التدقيق بالمعرف الداخلي ويحدث نصه للعرض فقط.</summary>
    private void SetAuditLabel(string name, string value)
    {
        var label = _pnlAuditBody.Controls.Find(name, true).OfType<Label>().FirstOrDefault();
        if (label is not null) label.Text = value;
    }

    /// <summary>تنسيق التاريخ بشكل عربي موحد، مع إظهار شرطة للقيمة غير المتاحة.</summary>
    private static string FormatDate(DateTime? value) =>
        value?.ToLocalTime().ToString("yyyy/MM/dd HH:mm") ?? "—";
}

/// <summary>
/// DTO عرضي لبطاقة التدقيق. مصدره Backend API ولا يحتوي بيانات اعتماد أو أسرار.
/// </summary>
public sealed record AuditInfoView(
    string CreatedBy,
    DateTime? CreatedAt,
    string UpdatedBy,
    DateTime? UpdatedAt,
    int EditCount,
    int PrintCount,
    string RecordStatus,
    bool IsAvailable)
{
    /// <summary>القيمة الافتراضية قبل تحميل سجل محفوظ.</summary>
    public static AuditInfoView Empty { get; } = new("—", null, "—", null, 0, 0, "جديد", false);
}