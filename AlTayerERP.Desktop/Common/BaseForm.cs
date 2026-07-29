using AlTayerERP.Desktop.Services;
using System.Drawing;
using System.Windows.Forms;

namespace AlTayerERP.Desktop.Common;

/// <summary>
/// القالب المركزي لشاشات المرحلة الأولى. يطبق هوية نظام الطائر السعيد،
/// والتنسيق المتجاوب، وبطاقة التدقيق وسياق الجلسة دون التدخل في منطق الأعمال.
/// </summary>
public abstract class BaseForm : Form
{
    private readonly Label _lblAuditSummary = new();
    private readonly Panel _pnlAuditBody = new();
    private readonly Button _btnToggleAudit = new();
    private bool _showPrintAudit;

    // يمنع حلقة Resize → إعادة تنسيق → Resize التي كانت تسبب اهتزاز وتعليق بعض الشاشات.
    private bool _baseStyleApplied;
    private bool _responsiveRefreshQueued;
    private bool _applyingResponsiveLayout;

    private RequiredFieldStyleService? _requiredFieldStyle;

    protected void ApplyBaseFormStyle()
    {
        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = true;
        Font = new Font("Segoe UI", 9.5F);
        BackColor = Color.FromArgb(244, 247, 251);
        KeyPreview = true;
        DoubleBuffered = true;
        AutoScaleMode = AutoScaleMode.Dpi;

        // بعض الشاشات تستدعي القالب أكثر من مرة؛ نربط Resize مرة واحدة فقط.
        if (_baseStyleApplied)
            return;

        _baseStyleApplied = true;
        Resize += (_, _) => ScheduleResponsiveRefresh();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        ScheduleResponsiveRefresh();
    }

    private void ScheduleResponsiveRefresh()
    {
        if (!IsHandleCreated || IsDisposed || _applyingResponsiveLayout || _responsiveRefreshQueued)
            return;

        // نؤجل دورة واحدة فقط بعد اكتمال Resize؛ لا ننشئ عشرات BeginInvoke عند سحب النافذة.
        _responsiveRefreshQueued = true;
        BeginInvoke(new Action(() =>
        {
            _responsiveRefreshQueued = false;
            if (IsDisposed || _applyingResponsiveLayout)
                return;

            _applyingResponsiveLayout = true;
            try
            {
                var insideWorkspace = !TopLevel || Parent is TabPage;
                if (insideWorkspace)
                {
                    MinimumSize = Size.Empty;
                    MaximumSize = Size.Empty;
                    AutoScroll = false;
                    Dock = DockStyle.Fill;
                }

                ApplyPremiumVisualIdentity(this);
                ApplyResponsiveLayout(this, insideWorkspace);
                UnifiedScreenLayoutService.Apply(this);
                Invalidate(true);
            }
            finally
            {
                // أي Resize ناتج أثناء التنسيق يتجاهل، فتتوقف حلقة الاهتزاز.
                _applyingResponsiveLayout = false;
            }
        }));
    }

    private void ApplyPremiumVisualIdentity(Control root)
    {
        ReplaceLegacyHeader(root);
        StyleControlTree(root);
        _requiredFieldStyle?.RefreshAppearance();
    }

    private void ReplaceLegacyHeader(Control root)
    {
        foreach (var table in FindControls<TableLayoutPanel>(root))
        {
            if (table.RowCount == 0) continue;
            var current = table.GetControlFromPosition(0, 0);
            if (current is BrandHeaderControl) continue;
            if (current is not Panel panel || panel.Height > 100) continue;

            var title = panel.Controls.OfType<Label>()
                .Select(x => x.Text?.Trim())
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
            if (string.IsNullOrWhiteSpace(title)) continue;

            var color = panel.BackColor;
            var looksLikeHeader = color.B < 190 && color.R < 70 && color.G < 135;
            if (!looksLikeHeader) continue;

            table.Controls.Remove(panel);
            panel.Dispose();
            var header = new BrandHeaderControl(title)
            {
                Name = "brandHeader",
                Margin = new Padding(0, 0, 0, 4)
            };
            table.Controls.Add(header, 0, 0);
            table.SetColumnSpan(header, Math.Max(1, table.ColumnCount));
            if (table.RowStyles.Count > 0) table.RowStyles[0].Height = 68;
            break;
        }
    }

    private void ApplyResponsiveLayout(Control root, bool insideWorkspace)
    {
        var availableWidth = Math.Max(700, ClientSize.Width);
        var compact = insideWorkspace || availableWidth < 1450;
        var veryCompact = availableWidth < 1080;
        var geographicScreen = GetType().Name is "FrmCountries" or "FrmGovernorates" or "FrmCities";

        if (compact) Padding = new Padding(0);

        foreach (var table in FindControls<TableLayoutPanel>(root))
        {
            if (compact && table.Padding.Left > 8)
                table.Padding = new Padding(6);

            if (table.ColumnCount == 4 && !DirectChildrenAreFieldCards(table) && table.ColumnStyles.Count >= 4)
            {
                if (table.ColumnStyles[0].SizeType == SizeType.Absolute && table.ColumnStyles[0].Width > 108)
                    table.ColumnStyles[0].Width = compact ? 96 : 120;
                if (table.ColumnStyles[2].SizeType == SizeType.Absolute && table.ColumnStyles[2].Width > 108)
                    table.ColumnStyles[2].Width = compact ? 96 : 120;
            }

            // المراجع الجغرافية تعتمد عمودين دائماً داخل النظام؛ التخطيط السابق
            // بأربعة أعمدة هو سبب قص العناوين والحقول الظاهر في صور التشغيل.
            if ((compact || geographicScreen) && table.ColumnCount == 4 &&
                DirectChildrenAreFieldCards(table) && table.Tag?.ToString() != "REFLOWED_2COL")
            {
                ReflowFieldCardsToTwoColumns(table);
            }

            if (table.RowCount >= 5 && table.GetControlFromPosition(0, 0) is BrandHeaderControl && table.RowStyles.Count > 2)
            {
                var editor = table.GetControlFromPosition(0, 2);
                if (editor is not null && ContainsReflowedTable(editor))
                    table.RowStyles[2].Height = compact ? 292 : 270;
            }
        }

        foreach (var toolbar in FindControls<FlowLayoutPanel>(root))
        {
            var buttons = toolbar.Controls.OfType<Button>().ToList();
            if (buttons.Count < 3) continue;
            toolbar.WrapContents = false;
            toolbar.AutoScroll = veryCompact;
            foreach (var button in buttons)
            {
                button.Tag ??= button.Text;
                var original = button.Tag?.ToString() ?? button.Text;
                button.Text = compact ? RemoveShortcut(original) : original;
                button.Width = veryCompact ? 78 : compact ? 90 : Math.Max(106, button.Width);
                button.Height = 36;
                button.Margin = new Padding(3, 4, 3, 4);
            }
        }

        foreach (var label in FindControls<Label>(root))
        {
            if (compact && label.Font.Size > 10.5F)
                label.AutoEllipsis = true;
        }
    }

    private static bool DirectChildrenAreFieldCards(TableLayoutPanel table)
    {
        var children = table.Controls.Cast<Control>().ToList();
        return children.Count >= 4 && children.All(x => x is Panel panel && panel.BorderStyle == BorderStyle.FixedSingle);
    }

    private static void ReflowFieldCardsToTwoColumns(TableLayoutPanel table)
    {
        var items = table.Controls.Cast<Control>()
            .Select(c => new
            {
                Control = c,
                Position = table.GetPositionFromControl(c),
                Span = table.GetColumnSpan(c)
            })
            .OrderBy(x => x.Position.Row)
            .ThenBy(x => x.Position.Column)
            .ToList();

        table.SuspendLayout();
        table.Controls.Clear();
        table.ColumnStyles.Clear();
        table.RowStyles.Clear();
        table.ColumnCount = 2;
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        var row = 0;
        var col = 0;
        foreach (var item in items)
        {
            var fullWidth = item.Span > 1;
            if (fullWidth)
            {
                if (col != 0) { row++; col = 0; }
                table.Controls.Add(item.Control, 0, row);
                table.SetColumnSpan(item.Control, 2);
                row++;
                continue;
            }

            table.Controls.Add(item.Control, col, row);
            col++;
            if (col >= 2) { col = 0; row++; }
        }
        if (col != 0) row++;

        table.RowCount = Math.Max(1, row);
        for (var i = 0; i < table.RowCount; i++)
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));

        table.Tag = "REFLOWED_2COL";
        table.ResumeLayout(true);
    }

    private static bool ContainsReflowedTable(Control root) =>
        root is TableLayoutPanel t && t.Tag?.ToString() == "REFLOWED_2COL" ||
        root.Controls.Cast<Control>().Any(ContainsReflowedTable);

    private static string RemoveShortcut(string text)
    {
        var separators = new[] { "  Ctrl+", " Ctrl+", "  F5", " F5", "  Esc", " Esc" };
        foreach (var separator in separators)
        {
            var index = text.IndexOf(separator, StringComparison.OrdinalIgnoreCase);
            if (index > 0) return text[..index].Trim();
        }
        return text.Trim();
    }

    private static void StyleControlTree(Control root)
    {
        foreach (Control control in root.Controls)
        {
            switch (control)
            {
                case Button button:
                    StyleButton(button);
                    break;
                case TextBox textBox:
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                    textBox.BackColor = textBox.ReadOnly ? Color.FromArgb(246, 248, 251) : Color.White;
                    textBox.Margin = new Padding(4, 6, 4, 6);
                    break;
                case ComboBox combo:
                    combo.FlatStyle = FlatStyle.Flat;
                    combo.BackColor = combo.Enabled ? Color.White : Color.FromArgb(246, 248, 251);
                    combo.Margin = new Padding(4, 6, 4, 6);
                    break;
                case DateTimePicker dateTimePicker:
                    dateTimePicker.CalendarMonthBackground = dateTimePicker.Enabled ? Color.White : Color.FromArgb(246, 248, 251);
                    break;
                case NumericUpDown numeric:
                    numeric.BorderStyle = BorderStyle.FixedSingle;
                    numeric.BackColor = numeric.Enabled ? Color.White : Color.FromArgb(246, 248, 251);
                    break;
                case DataGridView grid:
                    StyleGrid(grid);
                    break;
                case Panel panel when panel.BorderStyle == BorderStyle.FixedSingle:
                    panel.BackColor = Color.White;
                    panel.Padding = new Padding(Math.Max(panel.Padding.Left, 8));
                    break;
            }
            StyleControlTree(control);
        }
    }

    private static void StyleButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = Color.FromArgb(205, 216, 230);
        button.Cursor = Cursors.Hand;
        button.Font = new Font("Segoe UI", 9.2F, FontStyle.Bold);
        button.Height = Math.Max(34, button.Height);
        button.Padding = new Padding(6, 0, 6, 0);

        var text = button.Text ?? string.Empty;
        if (text.Contains("حفظ", StringComparison.OrdinalIgnoreCase))
        {
            button.BackColor = Color.FromArgb(15, 103, 208);
            button.ForeColor = Color.White;
            button.FlatAppearance.BorderColor = Color.FromArgb(15, 103, 208);
        }
        else if (text.Contains("إيقاف", StringComparison.OrdinalIgnoreCase) && !text.Contains("إعادة"))
        {
            button.BackColor = Color.FromArgb(255, 246, 246);
            button.ForeColor = Color.FromArgb(174, 35, 35);
            button.FlatAppearance.BorderColor = Color.FromArgb(238, 188, 188);
        }
        else if (text.Contains("إغلاق", StringComparison.OrdinalIgnoreCase))
        {
            button.BackColor = Color.FromArgb(247, 249, 252);
            button.ForeColor = Color.FromArgb(71, 85, 105);
        }
        else
        {
            if (button.BackColor == SystemColors.Control || button.BackColor == Color.White)
                button.BackColor = Color.White;
            if (button.ForeColor == SystemColors.ControlText)
                button.ForeColor = Color.FromArgb(16, 65, 112);
        }
    }

    private static void StyleGrid(DataGridView grid)
    {
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.None;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(231, 239, 249);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(19, 61, 103);
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        grid.ColumnHeadersHeight = 38;
        grid.DefaultCellStyle.BackColor = Color.White;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(216, 232, 249);
        grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(12, 52, 92);
        grid.DefaultCellStyle.Padding = new Padding(4);
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 253);
        grid.RowTemplate.Height = 34;
        grid.GridColor = Color.FromArgb(226, 232, 240);
    }

    private static IEnumerable<T> FindControls<T>(Control root) where T : Control
    {
        foreach (Control child in root.Controls)
        {
            if (child is T match) yield return match;
            foreach (var nested in FindControls<T>(child)) yield return nested;
        }
    }

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

        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 2, Padding = new Padding(0, 5, 0, 0) };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        AddAuditField(table, 0, 0, "أنشئ بواسطة", "—", out var createdBy);
        AddAuditField(table, 1, 0, "تاريخ الإنشاء", "—", out var createdAt);
        AddAuditField(table, 0, 1, "آخر تعديل بواسطة", "—", out var updatedBy);
        AddAuditField(table, 1, 1, "آخر تعديل", "—", out var updatedAt);
        createdBy.Name = "auditCreatedBy"; createdAt.Name = "auditCreatedAt";
        updatedBy.Name = "auditUpdatedBy"; updatedAt.Name = "auditUpdatedAt";

        _pnlAuditBody.Controls.Add(table);
        _pnlAuditBody.Dock = DockStyle.Fill;
        container.Controls.Add(_pnlAuditBody);
        container.Controls.Add(_lblAuditSummary);
        container.Controls.Add(header);
        SetAuditInfo(null);
        return container;
    }

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

    protected void SetAuditInfo(AuditInfoView? audit)
    {
        var values = audit ?? AuditInfoView.Empty;
        _lblAuditSummary.Text = values.IsAvailable ? BuildAuditSummary(values) : "تظهر بيانات التدقيق بعد تحميل سجل محفوظ من الخادم.";
        SetAuditLabel("auditCreatedBy", values.CreatedBy);
        SetAuditLabel("auditCreatedAt", FormatDate(values.CreatedAt));
        SetAuditLabel("auditUpdatedBy", values.UpdatedBy);
        SetAuditLabel("auditUpdatedAt", FormatDate(values.UpdatedAt));
    }

    /// <summary>تطبيق قاعدة الحقول الإلزامية الموحدة دون تلوين يدوي داخل الشاشة.</summary>
    protected void ApplyRequiredFieldStyle(params Control[] requiredFields)
    {
        _requiredFieldStyle ??= new RequiredFieldStyleService(this);
        _requiredFieldStyle.ApplyRequiredFieldStyle(requiredFields);
    }

    /// <summary>يعرض حدًا أحمر ورسالة عربية عند غياب قيمة إلزامية، ويعيد الحد تلقائياً بعد الإدخال.</summary>
    protected bool ValidateRequiredField(Control field, string message, Func<bool>? hasValue = null)
    {
        _requiredFieldStyle ??= new RequiredFieldStyleService(this);
        return _requiredFieldStyle.ValidateRequiredField(field, message, hasValue);
    }

    protected void ClearRequiredFieldValidation(Control field) =>
        _requiredFieldStyle?.ClearValidation(field);

    protected void EnablePrintAudit() => _showPrintAudit = true;

    private string BuildAuditSummary(AuditInfoView values)
    {
        var summary = $"الحالة: {values.RecordStatus} | عدد التعديلات: {values.EditCount}";
        return _showPrintAudit ? $"{summary} | الطباعة: {values.PrintCount}" : summary;
    }

    private void ToggleAuditPanel()
    {
        _pnlAuditBody.Visible = !_pnlAuditBody.Visible;
        _btnToggleAudit.Text = _pnlAuditBody.Visible ? "إخفاء التفاصيل" : "إظهار التفاصيل";
    }

    private static void AddAuditField(TableLayoutPanel table, int pairColumn, int row, string caption, string value, out Label output)
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

    private void SetAuditLabel(string name, string value)
    {
        var label = _pnlAuditBody.Controls.Find(name, true).OfType<Label>().FirstOrDefault();
        if (label is not null) label.Text = value;
    }

    private static string FormatDate(DateTime? value) => value?.ToLocalTime().ToString("yyyy/MM/dd HH:mm") ?? "—";
}

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
    public static AuditInfoView Empty { get; } = new("—", null, "—", null, 0, 0, "جديد", false);
}
