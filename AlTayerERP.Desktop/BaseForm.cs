using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AlTayerERP.Desktop;

/// <summary>
/// النموذج الأساسي الموحد لشاشات سطح المكتب.
/// يحتوي وظائف العرض والتحقق المشتركة فقط، ولا ينفذ اتصالاً أو منطق أعمال.
/// </summary>
public class BaseForm : Form
{
    private readonly Dictionary<Control, Color> _originalBackColors = new();
    private Label? _auditCreatedBy;
    private Label? _auditCreatedAt;
    private Label? _auditUpdatedBy;
    private Label? _auditUpdatedAt;
    private Label? _auditEditCount;
    private Label? _auditPrintCount;

    protected BaseForm()
    {
        ApplyBaseFormStyle();
    }

    /// <summary>يعيد تطبيق النمط العام عند الحاجة من النماذج المشتقة.</summary>
    protected void ApplyBaseFormStyle()
    {
        StartPosition = FormStartPosition.CenterParent;
        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = true;
        Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        BackColor = SystemColors.Control;
        AutoScaleMode = AutoScaleMode.Font;
    }

    /// <summary>
    /// يتحقق من أن الحقل الإلزامي يحتوي قيمة، ويعرض تنبيهًا عربيًا آمنًا عند عدم اكتماله.
    /// </summary>
    protected bool ValidateRequiredField(Control control, string fieldName = "الحقل المطلوب")
    {
        ArgumentNullException.ThrowIfNull(control);

        var hasValue = control switch
        {
            TextBoxBase textBox => !string.IsNullOrWhiteSpace(textBox.Text),
            ComboBox comboBox => comboBox.SelectedIndex >= 0 && comboBox.SelectedValue is not null,
            ListControl listControl => listControl.SelectedIndex >= 0,
            NumericUpDown numeric => numeric.Value != 0,
            DateTimePicker => true,
            CheckBox checkBox => checkBox.Checked,
            _ => !string.IsNullOrWhiteSpace(control.Text)
        };

        RememberOriginalColor(control);
        control.BackColor = hasValue ? _originalBackColors[control] : Color.MistyRose;

        if (hasValue)
            return true;

        MessageBox.Show($"{fieldName} مطلوب.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        control.Focus();
        return false;
    }

    /// <summary>يطبق تنسيقًا بصريًا موحدًا على الحقول الإلزامية.</summary>
    protected void ApplyRequiredFieldsStyle(params Control[] controls)
    {
        if (controls is null)
            return;

        foreach (var control in controls.Where(control => control is not null))
        {
            RememberOriginalColor(control);
            control.BackColor = Color.LightYellow;
        }
    }

    /// <summary>ينشئ لوحة التدقيق المشتركة التي يمكن إضافتها إلى أي شاشة.</summary>
    protected Panel CreateAuditInfoPanel()
    {
        var panel = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = true,
            Padding = new Padding(8),
            RightToLeft = RightToLeft.Yes,
            BackColor = SystemColors.ControlLight
        };

        _auditCreatedBy = CreateAuditLabel("أنشئ بواسطة: غير متاح");
        _auditCreatedAt = CreateAuditLabel("تاريخ الإنشاء: غير متاح");
        _auditUpdatedBy = CreateAuditLabel("عدل بواسطة: غير متاح");
        _auditUpdatedAt = CreateAuditLabel("تاريخ التعديل: غير متاح");
        _auditEditCount = CreateAuditLabel("عدد التعديلات: 0");
        _auditPrintCount = CreateAuditLabel("عدد مرات الطباعة: 0");

        panel.Controls.AddRange(new Control[]
        {
            _auditCreatedBy,
            _auditCreatedAt,
            _auditUpdatedBy,
            _auditUpdatedAt,
            _auditEditCount,
            _auditPrintCount
        });

        return panel;
    }

    /// <summary>
    /// يحدث لوحة التدقيق المشتركة. استخدام params يحافظ على التوافق مع الشاشات الحالية
    /// التي تمرر أربع أو ست قيم دون إنشاء خدمة تدقيق موازية.
    /// </summary>
    protected void SetAuditInfo(params object?[] values)
    {
        values ??= Array.Empty<object?>();

        SetLabel(_auditCreatedBy, "أنشئ بواسطة", ValueAt(values, 0));
        SetLabel(_auditCreatedAt, "تاريخ الإنشاء", FormatAuditValue(ValueAt(values, 1)));
        SetLabel(_auditUpdatedBy, "عدل بواسطة", ValueAt(values, 2));
        SetLabel(_auditUpdatedAt, "تاريخ التعديل", FormatAuditValue(ValueAt(values, 3)));
        SetLabel(_auditEditCount, "عدد التعديلات", ValueAt(values, 4) ?? 0);
        SetLabel(_auditPrintCount, "عدد مرات الطباعة", ValueAt(values, 5) ?? 0);
    }

    private static Label CreateAuditLabel(string text) => new()
    {
        AutoSize = true,
        Text = text,
        Margin = new Padding(8, 4, 8, 4)
    };

    private void RememberOriginalColor(Control control)
    {
        if (!_originalBackColors.ContainsKey(control))
            _originalBackColors[control] = control.BackColor;
    }

    private static object? ValueAt(IReadOnlyList<object?> values, int index) =>
        index >= 0 && index < values.Count ? values[index] : null;

    private static string FormatAuditValue(object? value) => value switch
    {
        null => "غير متاح",
        DateTime date => date.ToString("yyyy/MM/dd HH:mm"),
        DateTimeOffset date => date.ToString("yyyy/MM/dd HH:mm"),
        _ => value.ToString() ?? "غير متاح"
    };

    private static void SetLabel(Label? label, string caption, object? value)
    {
        if (label is not null)
            label.Text = $"{caption}: {value ?? "غير متاح"}";
    }
}
