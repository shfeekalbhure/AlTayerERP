using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace AlTayerERP.Desktop.Services;

/// <summary>
/// تحسينات تشغيلية ومرئية لشاشة المجموعات التجارية فقط، دون تغيير منطق الحفظ
/// أو بنية قاعدة البيانات. جميع مسميات الأدوات المعروضة للمستخدم عربية وواضحة.
/// </summary>
internal static class TenantGroupsScreenPolishService
{
    private const string MarkerName = "TenantGroupsPolishApplied";

    [ModuleInitializer]
    internal static void Register()
    {
        Application.Idle += (_, _) =>
        {
            foreach (var form in Application.OpenForms.OfType<FrmTenantGroups>().ToArray())
                Apply(form);
        };
    }

    private static void Apply(FrmTenantGroups form)
    {
        if (form.IsDisposed || form.Controls.Find(MarkerName, true).Length > 0)
            return;

        form.Controls.Add(new Label { Name = MarkerName, Visible = false });
        form.RightToLeft = RightToLeft.Yes;
        form.RightToLeftLayout = true;

        ConfigureToolbar(form);
        ConfigureGridAndEmptyState(form);
        ConfigureLayout(form);
    }

    /// <summary>يثبت نص كل زر ويلغي أسهم التمرير غير اللازمة في شريط الإجراءات.</summary>
    private static void ConfigureToolbar(Control root)
    {
        var expected = new[]
        {
            "جديد", "حفظ", "تعديل", "إيقاف", "إعادة تفعيل",
            "طباعة", "بحث", "تحديث", "إغلاق"
        };

        foreach (var flow in FindControls<FlowLayoutPanel>(root))
        {
            var buttons = flow.Controls.OfType<Button>().ToList();
            if (buttons.Count < expected.Length)
                continue;

            flow.AutoScroll = false;
            flow.WrapContents = false;
            flow.FlowDirection = FlowDirection.RightToLeft;
            flow.RightToLeft = RightToLeft.Yes;
            flow.Padding = new Padding(4, 5, 4, 3);

            for (var index = 0; index < expected.Length && index < buttons.Count; index++)
            {
                var button = buttons[index];
                button.Text = expected[index];
                button.Width = expected[index] == "إعادة تفعيل" ? 112 : 92;
                button.Height = 34;
                button.TextAlign = ContentAlignment.MiddleCenter;
                button.UseCompatibleTextRendering = true;
                new ToolTip().SetToolTip(button, expected[index]);
            }

            break;
        }
    }

    /// <summary>يضبط الجدول ويعرض حالة عربية واضحة عندما لا توجد سجلات.</summary>
    private static void ConfigureGridAndEmptyState(Control root)
    {
        var grid = FindControls<DataGridView>(root).FirstOrDefault();
        if (grid is null)
            return;

        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.ScrollBars = ScrollBars.Vertical;
        grid.RowHeadersVisible = false;
        grid.RowTemplate.Height = 34;
        grid.ColumnHeadersHeight = 38;
        grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
        grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

        var emptyState = new Label
        {
            Name = "lblTenantGroupsEmptyState",
            Text = "لا توجد بيانات للمجموعات التجارية.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(100, 116, 139),
            BackColor = Color.White,
            Visible = false
        };

        var parent = grid.Parent;
        if (parent is null)
            return;

        parent.Controls.Add(emptyState);
        emptyState.BringToFront();

        void UpdateEmptyState()
        {
            var empty = grid.Rows.Cast<DataGridViewRow>().All(row => row.IsNewRow || !row.Visible);
            emptyState.Visible = empty;
            if (empty) emptyState.BringToFront();
        }

        grid.DataBindingComplete += (_, _) => UpdateEmptyState();
        grid.RowsAdded += (_, _) => UpdateEmptyState();
        grid.RowsRemoved += (_, _) => UpdateEmptyState();
        UpdateEmptyState();
    }

    /// <summary>يزيد مساحة الجدول ويمنع قص بطاقات الإنشاء والتعديل والعدادات.</summary>
    private static void ConfigureLayout(Control root)
    {
        var shell = FindControls<TableLayoutPanel>(root)
            .FirstOrDefault(table => table.RowCount == 6 && table.ColumnCount == 1);
        if (shell is null || shell.RowStyles.Count < 6)
            return;

        shell.Padding = new Padding(6);
        shell.RowStyles[0] = new RowStyle(SizeType.Absolute, 68);
        shell.RowStyles[1] = new RowStyle(SizeType.Absolute, 48);
        shell.RowStyles[2] = new RowStyle(SizeType.Absolute, 190);
        shell.RowStyles[3] = new RowStyle(SizeType.Absolute, 62);
        shell.RowStyles[4] = new RowStyle(SizeType.Percent, 100);
        shell.RowStyles[5] = new RowStyle(SizeType.Absolute, 86);
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
