using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AlTayerERP.Desktop.Common;

/// <summary>
/// قاعدة موحدة لتمييز الحقول الإلزامية والتحقق منها في شاشات النظام.
/// لا تستخدم لتلوين أزرار الاختيار أو بيانات التدقيق أو الحقول المحسوبة.
/// </summary>
internal sealed class RequiredFieldStyleService : IDisposable
{
    internal static readonly Color RequiredBackColor = Color.FromArgb(255, 252, 220);
    private static readonly Color OptionalBackColor = Color.White;
    private static readonly Color ReadOnlyBackColor = Color.FromArgb(246, 248, 251);
    private static readonly Color ErrorBorderColor = Color.FromArgb(205, 55, 55);

    private readonly Form _form;
    private readonly ErrorProvider _errorProvider;
    private readonly ToolTip _toolTip;
    private readonly HashSet<Control> _requiredFields = new();
    private readonly HashSet<Control> _invalidFields = new();
    private readonly HashSet<Control> _wiredParents = new();

    public RequiredFieldStyleService(Form form)
    {
        _form = form;
        _errorProvider = new ErrorProvider
        {
            ContainerControl = form,
            BlinkStyle = ErrorBlinkStyle.NeverBlink
        };
        _toolTip = new ToolTip();
        _toolTip.SetToolTip(form, "الحقول ذات الخلفية الصفراء مطلوبة");
        form.Disposed += (_, _) => Dispose();
    }

    public void ApplyRequiredFieldStyle(params Control[] fields)
    {
        foreach (var field in fields.Where(IsSupportedInput))
        {
            if (!_requiredFields.Add(field))
                continue;

            _errorProvider.SetIconAlignment(field, ErrorIconAlignment.MiddleLeft);
            _errorProvider.SetIconPadding(field, 6);
            WireInput(field);
            WireParent(field.Parent);
        }

        RefreshAppearance();
    }

    public bool ValidateRequiredField(Control field, string message, Func<bool>? hasValue = null)
    {
        var valid = (hasValue ?? (() => HasValue(field))).Invoke();
        SetValidationState(field, valid ? null : message);
        return valid;
    }

    public void ClearValidation(Control field) => SetValidationState(field, null);

    public void RefreshAppearance()
    {
        foreach (var field in _requiredFields.Where(field => !field.IsDisposed))
        {
            var readOnly = field is TextBox textBox && textBox.ReadOnly;
            field.BackColor = !field.Enabled || readOnly
                ? ReadOnlyBackColor
                : RequiredBackColor;
            field.ForeColor = Color.FromArgb(30, 35, 42);
            WireParent(field.Parent);
        }

        foreach (var parent in _wiredParents.Where(parent => !parent.IsDisposed))
            parent.Invalidate();
    }

    private void WireInput(Control field)
    {
        field.EnabledChanged += (_, _) => RefreshAppearance();
        field.ParentChanged += (_, _) => WireParent(field.Parent);

        switch (field)
        {
            case TextBox textBox:
                textBox.ReadOnlyChanged += (_, _) => RefreshAppearance();
                textBox.TextChanged += (_, _) => ClearValidation(field);
                break;
            case ComboBox comboBox:
                comboBox.SelectedIndexChanged += (_, _) => ClearValidation(field);
                comboBox.TextChanged += (_, _) => ClearValidation(field);
                break;
            case DateTimePicker dateTimePicker:
                dateTimePicker.ValueChanged += (_, _) => ClearValidation(field);
                break;
            case NumericUpDown numeric:
                numeric.ValueChanged += (_, _) => ClearValidation(field);
                break;
        }
    }

    private void WireParent(Control? parent)
    {
        if (parent is null || !_wiredParents.Add(parent))
            return;

        parent.Paint += DrawValidationBorders;
    }

    private void DrawValidationBorders(object? sender, PaintEventArgs e)
    {
        if (sender is not Control parent)
            return;

        using var pen = new Pen(ErrorBorderColor, 2);
        foreach (var field in _invalidFields.Where(field => !field.IsDisposed && field.Parent == parent))
        {
            var bounds = field.Bounds;
            bounds.Inflate(1, 1);
            e.Graphics.DrawRectangle(pen, bounds);
        }
    }

    private void SetValidationState(Control field, string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            _invalidFields.Remove(field);
            _errorProvider.SetError(field, string.Empty);
        }
        else
        {
            _invalidFields.Add(field);
            _errorProvider.SetError(field, message);
        }

        field.Parent?.Invalidate();
    }

    private static bool IsSupportedInput(Control field) =>
        field is TextBox or ComboBox or DateTimePicker or NumericUpDown;

    private static bool HasValue(Control field) => field switch
    {
        TextBox textBox => !string.IsNullOrWhiteSpace(textBox.Text),
        ComboBox comboBox => comboBox.SelectedIndex >= 0 || comboBox.SelectedValue is not null,
        DateTimePicker => true,
        NumericUpDown => true,
        _ => true
    };

    public void Dispose()
    {
        _errorProvider.Dispose();
        _toolTip.Dispose();
    }
}
