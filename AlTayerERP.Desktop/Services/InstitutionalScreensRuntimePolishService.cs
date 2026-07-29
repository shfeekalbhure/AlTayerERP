using System;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace AlTayerERP.Desktop.Services;

/// <summary>
/// تحسينات تشغيلية مرئية لشاشتي المجموعات التجارية والشركات فقط.
/// لا تفتح الشاشات ولا تضيف أحداث تنقل؛ وظيفتها ضبط الأدوات الموجودة بعد إنشائها.
/// </summary>
internal static class InstitutionalScreensRuntimePolishService
{
    private const string EmptyLabelName = "lblRuntimeEmptyState";

    [ModuleInitializer]
    internal static void Register()
    {
        Application.Idle += (_, _) =>
        {
            foreach (Form form in Application.OpenForms.Cast<Form>().ToArray())
            {
                if (form.IsDisposed) continue;
                if (form.GetType().Name == "FrmTenantGroups") ApplyTenantGroups(form);
                else if (form.GetType().Name == "CompanyForm") ApplyCompanies(form);
            }
        };
    }

    private static void ApplyTenantGroups(Form form)
    {
        foreach (var toolbar in FindControls<FlowLayoutPanel>(form).Where(x => x.Controls.OfType<Button>().Count() >= 5))
        {
            toolbar.AutoScroll = false;
            toolbar.WrapContents = false;
            toolbar.FlowDirection = FlowDirection.RightToLeft;
            toolbar.Padding = new Padding(4, 5, 4, 3);

            foreach (var button in toolbar.Controls.OfType<Button>())
            {
                button.Text = NormalizeButtonText(button.Text);
                button.Width = Math.Max(button.Width, 94);
                button.Height = 34;
                button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                button.TextAlign = ContentAlignment.MiddleCenter;
            }
        }

        var grid = FindControls<DataGridView>(form).FirstOrDefault();
        if (grid is null) return;

        ConfigureGrid(grid);
        EnsureEmptyState(grid, "لا توجد مجموعات تجارية للعرض حالياً.");
        ImproveAuditLabels(form);
    }

    private static void ApplyCompanies(Form form)
    {
        var grid = FindControls<DataGridView>(form).FirstOrDefault();
        if (grid is not null)
        {
            ConfigureGrid(grid);
            EnsureEmptyState(grid, "لا توجد شركات للعرض حالياً.");
        }

        foreach (var flow in FindControls<FlowLayoutPanel>(form))
        {
            flow.AutoScroll = false;
            if (flow.Controls.OfType<Button>().Any(x =>
                    x.Text.Contains("شعار", StringComparison.CurrentCultureIgnoreCase) ||
                    x.Text.Contains("اختيار", StringComparison.CurrentCultureIgnoreCase) ||
                    x.Text.Contains("حذف", StringComparison.CurrentCultureIgnoreCase)))
            {
                flow.WrapContents = false;
                foreach (var button in flow.Controls.OfType<Button>())
                {
                    button.Width = Math.Max(button.Width, 104);
                    button.Height = 32;
                    button.Font = new Font("Segoe UI", 8.8F, FontStyle.Bold);
                }
            }
        }

        foreach (var panel in FindControls<Panel>(form))
            panel.AutoScroll = false;

        ImproveAuditLabels(form);
    }

    private static void ConfigureGrid(DataGridView grid)
    {
        grid.Dock = DockStyle.Fill;
        grid.RightToLeft = RightToLeft.Yes;
        grid.ScrollBars = ScrollBars.Vertical;
        grid.RowHeadersVisible = false;
        grid.AllowUserToResizeRows = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.FixedSingle;
        grid.RowTemplate.Height = 32;
        grid.ColumnHeadersHeight = 38;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.2F, FontStyle.Bold);
        grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.2F, FontStyle.Regular);
        grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        grid.DefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(37, 99, 235);
        grid.DefaultCellStyle.SelectionForeColor = Color.White;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
    }

    private static void EnsureEmptyState(DataGridView grid, string message)
    {
        var parent = grid.Parent;
        if (parent is null) return;

        var label = parent.Controls.Find(EmptyLabelName, false).OfType<Label>().FirstOrDefault();
        if (label is null)
        {
            label = new Label
            {
                Name = EmptyLabelName,
                Text = message,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139),
                BackColor = Color.White,
                Visible = false
            };
            parent.Controls.Add(label);
            label.BringToFront();

            grid.DataBindingComplete += (_, _) => RefreshEmptyState(grid, label);
            grid.RowsAdded += (_, _) => RefreshEmptyState(grid, label);
            grid.RowsRemoved += (_, _) => RefreshEmptyState(grid, label);
        }

        RefreshEmptyState(grid, label);
    }

    private static void RefreshEmptyState(DataGridView grid, Label label)
    {
        var empty = grid.Rows.Cast<DataGridViewRow>().All(x => x.IsNewRow);
        label.Visible = empty;
        if (empty) label.BringToFront();
        else grid.BringToFront();
    }

    private static void ImproveAuditLabels(Control root)
    {
        foreach (var label in FindControls<Label>(root))
        {
            if (label.BorderStyle != BorderStyle.FixedSingle) continue;
            label.AutoEllipsis = true;
            label.TextAlign = ContentAlignment.MiddleRight;
            label.Padding = new Padding(4, 0, 4, 0);
        }
    }

    private static string NormalizeButtonText(string? text)
    {
        var value = (text ?? string.Empty).Trim();
        if (value.Contains("جديد")) return "جديد";
        if (value.Contains("حفظ")) return "حفظ";
        if (value.Contains("تعديل")) return "تعديل";
        if (value.Contains("إيقاف")) return "إيقاف";
        if (value.Contains("إعادة تفعيل")) return "إعادة تفعيل";
        if (value.Contains("طباعة")) return "طباعة";
        if (value.Contains("بحث")) return "بحث";
        if (value.Contains("تحديث")) return "تحديث";
        if (value.Contains("إغلاق")) return "إغلاق";
        return value;
    }

    private static System.Collections.Generic.IEnumerable<T> FindControls<T>(Control root) where T : Control
    {
        foreach (Control child in root.Controls)
        {
            if (child is T match) yield return match;
            foreach (var nested in FindControls<T>(child)) yield return nested;
        }
    }
}
