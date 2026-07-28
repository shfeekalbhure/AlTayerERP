using AlTayerERP.Desktop.Services;
using System.Drawing.Printing;
using System.Net.Http.Json;
using System.Reflection;

namespace AlTayerERP.Desktop.Services;

/// <summary>
/// إغلاق واجهة شاشة الدول فقط دون التأثير على المحافظات أو المدن المشتركة معها.
/// </summary>
internal static class CountriesScreenClosureService
{
    private const string ClosedPrintButtonName = "btnCountriesClosedPrint";

    public static void Apply(Form form)
    {
        if (!string.Equals(form.GetType().Name, "FrmCountries", StringComparison.Ordinal))
            return;

        RemoveDuplicateResetButton(form);
        ReplacePrintButton(form);
    }

    private static void RemoveDuplicateResetButton(Control root)
    {
        foreach (var flow in FindControls<FlowLayoutPanel>(root))
        {
            var duplicate = flow.Controls.OfType<Button>()
                .FirstOrDefault(button => string.Equals(button.Text?.Trim(), "إعادة", StringComparison.Ordinal));
            if (duplicate is null) continue;

            flow.Controls.Remove(duplicate);
            duplicate.Dispose();
        }
    }

    private static void ReplacePrintButton(Form form)
    {
        if (FindControls<Button>(form).Any(button => button.Name == ClosedPrintButtonName))
            return;

        foreach (var flow in FindControls<FlowLayoutPanel>(form))
        {
            var original = flow.Controls.OfType<Button>()
                .FirstOrDefault(button => string.Equals(button.Text?.Trim(), "طباعة", StringComparison.Ordinal));
            if (original is null) continue;

            var index = flow.Controls.GetChildIndex(original);
            var replacement = new Button
            {
                Name = ClosedPrintButtonName,
                Text = "طباعة",
                Width = original.Width,
                Height = original.Height,
                Margin = original.Margin,
                FlatStyle = original.FlatStyle,
                BackColor = original.BackColor,
                ForeColor = original.ForeColor,
                Font = original.Font,
                UseVisualStyleBackColor = original.UseVisualStyleBackColor
            };
            replacement.FlatAppearance.BorderColor = original.FlatAppearance.BorderColor;
            replacement.Click += async (_, _) => await PrintCountryAsync(form);

            flow.Controls.Remove(original);
            original.Dispose();
            flow.Controls.Add(replacement);
            flow.Controls.SetChildIndex(replacement, index);
            return;
        }
    }

    private static async Task PrintCountryAsync(Form form)
    {
        var selectedId = ReadField<int>(form, "_selectedId");
        if (selectedId <= 0)
        {
            MessageBox.Show("اختر دولة أولاً.", form.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var response = await ApiService.Client.PostAsync($"GeographicReferences/countries/{selectedId}/print", null);
        if (!response.IsSuccessStatusCode)
        {
            MessageBox.Show(await response.Content.ReadAsStringAsync(), "تعذر تسجيل الطباعة", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var audit = await ApiService.Client.GetFromJsonAsync<CountryAuditInfo>(
            $"GeographicReferences/countries/{selectedId}/audit-info") ?? new CountryAuditInfo();

        WriteLabel(form, "_auditCreatedBy", audit.Created_By ?? "—");
        WriteLabel(form, "_auditCreatedAt", FormatDate(audit.Created_At));
        WriteLabel(form, "_auditUpdatedBy", audit.Updated_By ?? "—");
        WriteLabel(form, "_auditUpdatedAt", FormatDate(audit.Updated_At));
        WriteLabel(form, "_auditEditCount", audit.Edit_Count.ToString());
        WriteLabel(form, "_auditPrintCount", audit.Print_Count.ToString());

        var code = ReadText(form, "_code");
        var nameAr = ReadText(form, "_nameAr");
        var nameEn = ReadText(form, "_nameEn");
        var iso2 = ReadText(form, "_iso2");
        var iso3 = ReadText(form, "_iso3");
        var phoneCode = ReadText(form, "_phoneCode");
        var currencyCode = ReadText(form, "_currencyCode");
        var nationality = ReadText(form, "_nationalityAr");
        var notes = ReadText(form, "_notes");
        var active = ReadField<CheckBox>(form, "_active")?.Checked == true ? "نشطة" : "موقوفة";

        using var document = new PrintDocument { DocumentName = $"بيانات الدولة - {nameAr}" };
        document.PrintPage += (_, e) =>
        {
            using var titleFont = new Font("Segoe UI", 16, FontStyle.Bold);
            using var bodyFont = new Font("Segoe UI", 10.5F);
            using var smallFont = new Font("Segoe UI", 9F);

            e.Graphics.DrawString("بيانات الدولة", titleFont, Brushes.Navy, 80, 70);
            var body =
                $"الكود: {code}\n" +
                $"الاسم بالعربية: {nameAr}\n" +
                $"الاسم بالإنجليزية: {nameEn}\n" +
                $"ISO2: {iso2}\n" +
                $"ISO3: {iso3}\n" +
                $"مفتاح الاتصال: {phoneCode}\n" +
                $"رمز العملة: {currencyCode}\n" +
                $"الجنسية: {nationality}\n" +
                $"الحالة: {active}\n" +
                $"الملاحظات: {notes}";
            e.Graphics.DrawString(body, bodyFont, Brushes.Black, new RectangleF(80, 120, 680, 340));

            var auditText =
                $"أنشئ بواسطة: {audit.Created_By ?? "—"}    تاريخ الإنشاء: {FormatDate(audit.Created_At)}\n" +
                $"عُدّل بواسطة: {audit.Updated_By ?? "—"}    تاريخ التعديل: {FormatDate(audit.Updated_At)}\n" +
                $"عدد التعديلات: {audit.Edit_Count}    عدد مرات الطباعة: {audit.Print_Count}";
            e.Graphics.DrawString(auditText, smallFont, Brushes.DimGray, new RectangleF(80, 485, 680, 120));
        };

        using var preview = new PrintPreviewDialog
        {
            Document = document,
            Width = 900,
            Height = 700,
            RightToLeft = RightToLeft.Yes
        };
        preview.ShowDialog(form);
    }

    private static string ReadText(Form form, string fieldName) =>
        ReadField<TextBox>(form, fieldName)?.Text?.Trim() ?? string.Empty;

    private static T? ReadField<T>(Form form, string fieldName)
    {
        var type = form.GetType();
        while (type is not null)
        {
            var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field is not null)
            {
                var value = field.GetValue(form);
                if (value is T typed) return typed;
                return default;
            }
            type = type.BaseType;
        }
        return default;
    }

    private static void WriteLabel(Form form, string fieldName, string value)
    {
        var label = ReadField<Label>(form, fieldName);
        if (label is not null) label.Text = value;
    }

    private static string FormatDate(DateTime? value) =>
        value?.ToString("yyyy/MM/dd HH:mm") ?? "—";

    private static IEnumerable<T> FindControls<T>(Control root) where T : Control
    {
        foreach (Control child in root.Controls)
        {
            if (child is T match) yield return match;
            foreach (var nested in FindControls<T>(child)) yield return nested;
        }
    }

    private sealed class CountryAuditInfo
    {
        public string? Created_By { get; set; }
        public DateTime? Created_At { get; set; }
        public string? Updated_By { get; set; }
        public DateTime? Updated_At { get; set; }
        public int Edit_Count { get; set; }
        public int Print_Count { get; set; }
    }
}
