namespace AlTayerERP.Desktop.Services;

/// <summary>
/// تحسينات مرئية محدودة لشاشة الشركات فقط دون تغيير منطقها أو تخطيط الشاشات الأخرى.
/// </summary>
internal static class CompanyScreenPolishService
{
    public static void Apply(Form form)
    {
        if (!string.Equals(form.GetType().Name, "CompanyForm", StringComparison.Ordinal))
            return;

        foreach (var grid in FindControls<DataGridView>(form))
        {
            // الجدول يملأ العرض، لذلك لا حاجة لشريط تمرير أفقي يستهلك مساحة الشاشة.
            grid.RowTemplate.Height = 32;
            grid.ColumnHeadersHeight = 36;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            grid.DefaultCellStyle.ForeColor = Color.FromArgb(31, 41, 55);
            grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.ScrollBars = ScrollBars.Vertical;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.RowHeadersVisible = false;
            grid.AllowUserToResizeRows = false;
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
