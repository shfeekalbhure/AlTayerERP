using System;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace AlTayerERP.Desktop;

/// <summary>
/// التصميم المرئي الموحد المعتمد لشاشات المرحلة الأولى.
/// يطبق تلقائياً على النوافذ المفتوحة دون تغيير المنطق التشغيلي لكل شاشة.
/// </summary>
internal static class UnifiedPhase1Theme
{
    private static readonly Color Navy = Color.FromArgb(8, 55, 112);
    private static readonly Color Primary = Color.FromArgb(14, 93, 216);
    private static readonly Color Surface = Color.White;
    private static readonly Color Canvas = Color.FromArgb(247, 249, 252);
    private static readonly Color Border = Color.FromArgb(216, 225, 238);
    private static readonly Color Header = Color.FromArgb(235, 241, 249);
    private static readonly Color Text = Color.FromArgb(31, 52, 82);

    [ModuleInitializer]
    internal static void Register()
    {
        Application.Idle += (_, _) =>
        {
            foreach (Form form in Application.OpenForms.Cast<Form>().ToArray())
                Apply(form);
        };
    }

    internal static void Apply(Form form)
    {
        if (form.Tag as string == "UnifiedPhase1Theme") return;
        form.Tag = "UnifiedPhase1Theme";
        form.RightToLeft = RightToLeft.Yes;
        form.RightToLeftLayout = true;
        form.Font = new Font("Segoe UI", 9.5F);
        form.BackColor = Canvas;
        form.MinimumSize = new Size(Math.Min(form.Width, 980), Math.Min(form.Height, 620));
        StyleTree(form.Controls);
    }

    private static void StyleTree(Control.ControlCollection controls)
    {
        foreach (Control control in controls)
        {
            switch (control)
            {
                case DataGridView grid:
                    StyleGrid(grid);
                    break;
                case Button button:
                    StyleButton(button);
                    break;
                case GroupBox group:
                    group.ForeColor = Navy;
                    group.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                    group.BackColor = Surface;
                    break;
                case Panel panel:
                    if (panel.BackColor == SystemColors.Control || panel.BackColor == Color.Transparent)
                        panel.BackColor = Surface;
                    break;
                case TextBox textBox:
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                    textBox.BackColor = Surface;
                    textBox.ForeColor = Text;
                    break;
                case ComboBox combo:
                    combo.FlatStyle = FlatStyle.Flat;
                    combo.BackColor = Surface;
                    combo.ForeColor = Text;
                    break;
                case Label label:
                    label.ForeColor = label.ForeColor == Color.White ? Color.White : Text;
                    break;
            }

            if (control.HasChildren)
                StyleTree(control.Controls);
        }
    }

    private static void StyleButton(Button button)
    {
        button.Height = Math.Max(button.Height, 34);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = Border;
        button.Cursor = Cursors.Hand;
        button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

        var text = button.Text.Trim();
        if (text.Contains("حفظ", StringComparison.OrdinalIgnoreCase))
        {
            button.BackColor = Primary;
            button.ForeColor = Color.White;
            button.FlatAppearance.BorderColor = Primary;
        }
        else if (text.Contains("حذف", StringComparison.OrdinalIgnoreCase) ||
                 text.Contains("إيقاف", StringComparison.OrdinalIgnoreCase))
        {
            button.BackColor = Surface;
            button.ForeColor = Color.FromArgb(211, 47, 47);
        }
        else
        {
            button.BackColor = Surface;
            button.ForeColor = Navy;
        }
    }

    private static void StyleGrid(DataGridView grid)
    {
        grid.EnableHeadersVisualStyles = false;
        grid.BackgroundColor = Surface;
        grid.BorderStyle = BorderStyle.FixedSingle;
        grid.GridColor = Border;
        grid.RowHeadersVisible = false;
        grid.RowTemplate.Height = Math.Max(grid.RowTemplate.Height, 32);
        grid.ColumnHeadersHeight = Math.Max(grid.ColumnHeadersHeight, 38);
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Header,
            ForeColor = Navy,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleCenter,
            WrapMode = DataGridViewTriState.True
        };
        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Surface,
            ForeColor = Text,
            SelectionBackColor = Color.FromArgb(224, 236, 251),
            SelectionForeColor = Navy,
            Alignment = DataGridViewContentAlignment.MiddleCenter
        };
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 252, 255);
    }
}