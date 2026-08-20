using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace AlTayerERP.Desktop.Services;

/// <summary>
/// توحيد مرئي محدود لشاشات المجموعات والشركات والفروع والدول.
/// لا يغير قواعد الحفظ أو البيانات، ويعمل بعد إنشاء عناصر الشاشة الفعلية.
/// </summary>
internal static class CorporateMasterScreensPolishService
{
    private const string AppliedMarker = "CorporateMasterScreensPolished";
    private static readonly string[] TargetForms =
    {
        "FrmTenantGroups", "CompanyForm", "BranchForm", "FrmCountries", "CountriesForm"
    };

    private static readonly string[] ToolbarOrder =
    {
        "جديد", "حفظ", "تعديل", "إيقاف", "إعادة تفعيل", "بحث", "تحديث", "طباعة", "إغلاق"
    };

    /// <summary>يراقب النماذج المفتوحة ويطبق الضبط مرة واحدة دون ربط أي حدث فتح جديد.</summary>
    [ModuleInitializer]
    internal static void Register()
    {
        Application.Idle += (_, _) =>
        {
            foreach (Form form in Application.OpenForms.Cast<Form>().ToArray())
            {
                if (!TargetForms.Contains(form.GetType().Name, StringComparer.Ordinal) ||
                    string.Equals(form.Tag?.ToString(), AppliedMarker, StringComparison.Ordinal))
                    continue;

                Apply(form);
                form.Tag = AppliedMarker;
            }
        };
    }

    private static void Apply(Form form)
    {
        form.RightToLeft = RightToLeft.Yes;
        form.RightToLeftLayout = true;
        form.AutoScroll = false;

        NormalizeInputs(form);
        NormalizeToolbars(form);
        NormalizeGrids(form);
        NormalizeCards(form);
        NormalizeRootRows(form);

        if (form.GetType().Name is "FrmCountries" or "CountriesForm")
            CompleteCountriesGrid(form);

        form.PerformLayout();
        form.Invalidate(true);
    }

    /// <summary>يوحد ارتفاعات الحقول ومحاذاتها العربية.</summary>
    private static void NormalizeInputs(Control root)
    {
        foreach (var textBox in FindControls<TextBox>(root))
        {
            if (!textBox.Multiline)
                textBox.Height = 30;
            textBox.Margin = new Padding(3, 2, 3, 2);
            textBox.TextAlign = HorizontalAlignment.Right;
        }

        foreach (var combo in FindControls<ComboBox>(root))
        {
            combo.Height = 30;
            combo.Margin = new Padding(3, 2, 3, 2);
            combo.RightToLeft = RightToLeft.Yes;
        }

        foreach (var check in FindControls<CheckBox>(root))
        {
            check.AutoSize = true;
            check.Margin = new Padding(8, 6, 8, 4);
            check.TextAlign = ContentAlignment.MiddleRight;
        }
    }

    /// <summary>يرتب أزرار الإجراءات من أقصى اليمين ويمنع أسهم التمرير.</summary>
    private static void NormalizeToolbars(Control root)
    {
        foreach (var flow in FindControls<FlowLayoutPanel>(root))
        {
            var buttons = flow.Controls.OfType<Button>().ToList();
            if (buttons.Count < 5)
                continue;

            flow.SuspendLayout();
            flow.FlowDirection = FlowDirection.RightToLeft;
            flow.RightToLeft = RightToLeft.Yes;
            flow.WrapContents = false;
            flow.AutoScroll = false;
            flow.Padding = new Padding(3, 3, 3, 2);

            foreach (var button in buttons)
            {
                button.Text = NormalizeButtonText(button.Text);
                button.Height = 31;
                button.Width = button.Text == "إعادة تفعيل" ? 94 : 78;
                button.Margin = new Padding(2);
                button.AutoSize = false;
            }

            var ordered = buttons
                .OrderBy(button => Array.IndexOf(ToolbarOrder, button.Text) is var index && index >= 0 ? index : 99)
                .ToList();
            for (var index = 0; index < ordered.Count; index++)
                flow.Controls.SetChildIndex(ordered[index], index);

            flow.ResumeLayout(true);
        }
    }

    private static string NormalizeButtonText(string? text)
    {
        var value = (text ?? string.Empty).Trim();
        foreach (var approved in ToolbarOrder)
        {
            if (value.Contains(approved, StringComparison.Ordinal))
                return approved;
        }
        return value;
    }

    /// <summary>يكبر مساحة البيانات ويزيل التمرير الأفقي من الجداول ذات الأعمدة المرنة.</summary>
    private static void NormalizeGrids(Control root)
    {
        foreach (var grid in FindControls<DataGridView>(root))
        {
            grid.Dock = DockStyle.Fill;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.ScrollBars = ScrollBars.Vertical;
            grid.RowHeadersVisible = false;
            grid.AllowUserToResizeRows = false;
            grid.RowTemplate.Height = 31;
            grid.ColumnHeadersHeight = 35;
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.BackgroundColor = Color.White;
        }
    }

    /// <summary>يضبط بطاقات التدقيق ويمنع قص النصوص في أسفل الشاشة.</summary>
    private static void NormalizeCards(Control root)
    {
        foreach (var table in FindControls<TableLayoutPanel>(root))
        {
            if (table.ColumnCount == 3 && table.Controls.OfType<Control>().Any(control =>
                    control.Text.Contains("العدادات", StringComparison.Ordinal) ||
                    control.Text.Contains("الإنشاء", StringComparison.Ordinal)))
            {
                table.MinimumSize = new Size(0, 76);
                table.Padding = new Padding(2);
            }
        }
    }

    /// <summary>يقلل الصفوف العلوية الثابتة ويترك النسبة الأكبر للجدول.</summary>
    private static void NormalizeRootRows(Form form)
    {
        var root = form.Controls.OfType<TableLayoutPanel>().FirstOrDefault(table => table.Dock == DockStyle.Fill);
        if (root is null || root.RowCount < 5)
            return;

        EnsureRowStyles(root);
        var formName = form.GetType().Name;
        if (formName == "FrmTenantGroups")
        {
            SetAbsolute(root, 0, 58);
            SetAbsolute(root, 1, 40);
            SetAbsolute(root, 2, 176);
            SetAbsolute(root, 3, 50);
            SetPercent(root, 4, 100);
            SetAbsolute(root, 5, 80);
        }
        else if (formName is "FrmCountries" or "CountriesForm")
        {
            SetAbsolute(root, 0, 58);
            SetAbsolute(root, 1, 40);
            SetAbsolute(root, 2, 285);
            SetAbsolute(root, 3, 42);
            SetPercent(root, 4, 100);
            SetAbsolute(root, 5, 80);
        }
        else if (formName == "BranchForm")
        {
            SetAbsolute(root, 0, 42);
            SetAbsolute(root, 1, 42);
            SetAbsolute(root, 2, 330);
            SetAbsolute(root, 3, 36);
            SetPercent(root, 4, 100);
        }
    }

    /// <summary>يضيف أعمدة ISO المفقودة إلى جدول الدول من البيانات المحملة نفسها.</summary>
    private static void CompleteCountriesGrid(Form form)
    {
        var grid = FindControls<DataGridView>(form).FirstOrDefault();
        if (grid is null)
            return;

        grid.DataBindingComplete += (_, _) => FillCountryReferenceColumns(form, grid);
        FillCountryReferenceColumns(form, grid);
    }

    private static void FillCountryReferenceColumns(Form form, DataGridView grid)
    {
        AddTextColumn(grid, "ISO2", "ISO2", 58);
        AddTextColumn(grid, "ISO3", "ISO3", 65);

        var rowsField = FindField(form.GetType(), "_rows");
        if (rowsField?.GetValue(form) is not IDictionary rows)
            return;

        foreach (DataGridViewRow gridRow in grid.Rows)
        {
            if (!int.TryParse(gridRow.Cells["ID"]?.Value?.ToString(), out var id) || !rows.Contains(id))
                continue;

            var row = rows[id];
            gridRow.Cells["ISO2"].Value = ReadDictionaryValue(row, "ISO2");
            gridRow.Cells["ISO3"].Value = ReadDictionaryValue(row, "ISO3");
        }
    }

    private static object? ReadDictionaryValue(object? dictionary, string key)
    {
        if (dictionary is null) return null;
        var tryGet = dictionary.GetType().GetMethod("TryGetValue");
        if (tryGet is null) return null;
        var args = new object?[] { key, null };
        return tryGet.Invoke(dictionary, args) is true ? args[1]?.ToString() : null;
    }

    private static void AddTextColumn(DataGridView grid, string name, string header, int minimumWidth)
    {
        if (grid.Columns.Contains(name)) return;
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = name,
            HeaderText = header,
            MinimumWidth = minimumWidth,
            FillWeight = minimumWidth,
            ReadOnly = true
        });
    }

    private static FieldInfo? FindField(Type? type, string name)
    {
        while (type is not null)
        {
            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field is not null) return field;
            type = type.BaseType;
        }
        return null;
    }

    private static void EnsureRowStyles(TableLayoutPanel table)
    {
        while (table.RowStyles.Count < table.RowCount)
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
    }

    private static void SetAbsolute(TableLayoutPanel table, int row, float height)
    {
        if (row < table.RowCount) table.RowStyles[row] = new RowStyle(SizeType.Absolute, height);
    }

    private static void SetPercent(TableLayoutPanel table, int row, float percent)
    {
        if (row < table.RowCount) table.RowStyles[row] = new RowStyle(SizeType.Percent, percent);
    }

    private static IEnumerable<T> FindControls<T>(Control root) where T : Control
    {
        foreach (Control child in root.Controls)
        {
            if (child is T match) yield return match;
            foreach (var nested in FindControls<T>(child)) yield return nested;
        }
    }
}
