using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace AlTayerERP.Desktop.Services;

/// <summary>
/// تحسينات مرئية خاصة بشاشة المجموعات التجارية فقط، دون تغيير منطق الحفظ
/// أو إضافة أي مسار فتح مستقل خارج مساحة العمل الرئيسية.
/// </summary>
internal static class TenantGroupsScreenClosureService
{
    private const string AppliedMarker = "TenantGroupsScreenClosureApplied";

    [ModuleInitializer]
    internal static void Register()
    {
        Application.Idle += (_, _) =>
        {
            foreach (var form in Application.OpenForms.Cast<Form>().ToArray())
            {
                if (!string.Equals(form.GetType().Name, "FrmTenantGroups", StringComparison.Ordinal) ||
                    form.Controls.Find(AppliedMarker, true).Length > 0)
                    continue;

                Apply(form);
            }
        };
    }

    private static void Apply(Form form)
    {
        form.SuspendLayout();
        try
        {
            form.Controls.Add(new Label { Name = AppliedMarker, Visible = false });
            form.RightToLeft = RightToLeft.Yes;
            form.RightToLeftLayout = true;
            form.AutoScroll = false;

            ConfigureToolbar(form);
            ConfigureGrid(form);
            ConfigureSearch(form);
        }
        finally
        {
            form.ResumeLayout(true);
        }
    }

    private static void ConfigureToolbar(Control root)
    {
        var toolbar = FindControls<FlowLayoutPanel>(root)
            .FirstOrDefault(flow => flow.Controls.OfType<Button>().Count() >= 7);
        if (toolbar is null)
            return;

        toolbar.AutoScroll = false;
        toolbar.WrapContents = false;
        toolbar.FlowDirection = FlowDirection.RightToLeft;
        toolbar.Padding = new Padding(4, 5, 4, 5);

        var approvedTexts = new[]
        {
            "جديد", "حفظ", "تعديل", "إيقاف", "إعادة تفعيل", "طباعة", "بحث", "تحديث", "إغلاق"
        };

        var buttons = toolbar.Controls.OfType<Button>().ToList();
        for (var index = 0; index < buttons.Count && index < approvedTexts.Length; index++)
        {
            var button = buttons[index];
            button.Text = approvedTexts[index];
            button.Width = approvedTexts[index] == "إعادة تفعيل" ? 112 : 94;
            button.Height = 34;
            button.Margin = new Padding(3, 0, 3, 0);
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }
    }

    private static void ConfigureGrid(Control root)
    {
        var grid = FindControls<DataGridView>(root).FirstOrDefault();
        if (grid is null)
            return;

        grid.Dock = DockStyle.Fill;
        grid.ScrollBars = ScrollBars.Vertical;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.RowHeadersVisible = false;
        grid.AllowUserToResizeRows = false;
        grid.RowTemplate.Height = 34;
        grid.ColumnHeadersHeight = 38;
        grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
        grid.DefaultCellStyle.ForeColor = Color.FromArgb(31, 41, 55);
        grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        grid.BackgroundColor = Color.White;

        var host = grid.Parent;
        if (host is null)
            return;

        var empty = host.Controls.Find("lblTenantGroupsEmpty", false).OfType<Label>().FirstOrDefault();
        if (empty is null)
        {
            empty = new Label
            {
                Name = "lblTenantGroupsEmpty",
                Text = "لا توجد بيانات للمجموعات التجارية",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139),
                BackColor = Color.White,
                Visible = false
            };
            host.Controls.Add(empty);
        }

        void RefreshEmptyState()
        {
            empty.Visible = grid.Rows.Count == 0;
            if (empty.Visible) empty.BringToFront(); else grid.BringToFront();
        }

        grid.DataBindingComplete += (_, _) => RefreshEmptyState();
        grid.RowsAdded += (_, _) => RefreshEmptyState();
        grid.RowsRemoved += (_, _) => RefreshEmptyState();
        RefreshEmptyState();
    }

    private static void ConfigureSearch(Control root)
    {
        foreach (var textBox in FindControls<TextBox>(root))
        {
            if (!(textBox.PlaceholderText?.Contains("ابحث", StringComparison.OrdinalIgnoreCase) ?? false))
                continue;

            textBox.TextAlign = HorizontalAlignment.Right;
            textBox.Font = new Font("Segoe UI", 9.5F);
            textBox.Height = 32;
        }
    }

    private static IEnumerable<T> FindControls<T>(Control root) where T : Control
    {
        foreach (Control child in root.Controls)
        {
            if (child is T match)
                yield return match;

            foreach (var nested in FindControls<T>(child))
                yield return nested;
        }
    }
}
