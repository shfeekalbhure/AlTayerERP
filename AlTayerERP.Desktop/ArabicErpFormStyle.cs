using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// ثيم موحّد للشاشات القديمة: يحافظ على العناصر والمنطق الموجودين
    /// ويجعل الواجهة عربية، واضحة، ومتسقة دون إعادة بناء الـ Designer.
    /// </summary>
    internal static class ArabicErpFormStyle
    {
        private static readonly Color Primary = Color.FromArgb(24, 74, 119);
        private static readonly Color Accent = Color.FromArgb(0, 120, 170);
        private static readonly Color Surface = Color.White;
        private static readonly Color Background = Color.FromArgb(245, 248, 251);

        public static void Apply(Form form, string? subtitle = null)
        {
            form.RightToLeft = RightToLeft.Yes;
            form.RightToLeftLayout = true;
            form.Font = new Font("Segoe UI", 9F);
            form.BackColor = Background;
            form.StartPosition = FormStartPosition.CenterParent;
            form.KeyPreview = true;
            form.MinimumSize = new Size(920, 580);

            StyleControls(form);
            form.Shown += (_, _) => StyleControls(form);

            // اختصارات موحّدة تعمل بجانب الأزرار الموجودة في كل شاشة.
            form.KeyDown += (_, e) =>
            {
                if (e.Control && e.KeyCode == Keys.N)
                {
                    Click(form, "btnNew", "جديد");
                    e.SuppressKeyPress = true;
                }
                else if (e.Control && e.KeyCode == Keys.S)
                {
                    Click(form, "btnSave", "حفظ");
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.F5)
                {
                    Click(form, "btnRefresh", "تحديث");
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    Click(form, "btnClose", "إغلاق");
                    e.SuppressKeyPress = true;
                }
            };

            if (!string.IsNullOrWhiteSpace(subtitle))
                form.AccessibleDescription = subtitle;
        }

        private static void StyleControls(Control root)
        {
            foreach (Control control in root.Controls)
            {
                control.RightToLeft = RightToLeft.Yes;
                control.Font = new Font("Segoe UI", 9F);

                switch (control)
                {
                    case Button button:
                        button.FlatStyle = FlatStyle.Flat;
                        button.FlatAppearance.BorderColor = Color.FromArgb(190, 205, 220);
                        button.FlatAppearance.BorderSize = 1;
                        button.BackColor = button.Name.Contains("Delete", StringComparison.OrdinalIgnoreCase)
                            ? Color.FromArgb(253, 242, 242)
                            : Surface;
                        button.ForeColor = button.Name.Contains("Delete", StringComparison.OrdinalIgnoreCase)
                            ? Color.FromArgb(170, 40, 40)
                            : Primary;
                        button.Height = Math.Max(button.Height, 34);
                        button.Cursor = Cursors.Hand;
                        break;

                    case DataGridView grid:
                        ConfigureGrid(grid);
                        break;

                    case TextBox textBox:
                        textBox.BorderStyle = BorderStyle.FixedSingle;
                        textBox.BackColor = Surface;
                        break;

                    case ComboBox combo:
                        combo.FlatStyle = FlatStyle.Flat;
                        combo.BackColor = Surface;
                        break;

                    case GroupBox group:
                        group.ForeColor = Primary;
                        group.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                        break;

                    case Label label:
                        label.ForeColor = Color.FromArgb(50, 60, 70);
                        break;
                }

                if (control.HasChildren)
                    StyleControls(control);
            }
        }

        private static void ConfigureGrid(DataGridView grid)
        {
            grid.EnableHeadersVisualStyles = false;
            grid.BackgroundColor = Surface;
            grid.BorderStyle = BorderStyle.FixedSingle;
            grid.GridColor = Color.FromArgb(222, 230, 238);
            grid.RowHeadersVisible = false;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            grid.ColumnHeadersHeight = 38;
            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Primary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                WrapMode = DataGridViewTriState.False
            };
            grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Surface,
                ForeColor = Color.FromArgb(35, 45, 55),
                SelectionBackColor = Accent,
                SelectionForeColor = Color.White,
                Alignment = DataGridViewContentAlignment.MiddleRight
            };
        }

        private static void Click(Form form, string controlName, string caption)
        {
            var byName = form.Controls.Find(controlName, true).OfType<Button>().FirstOrDefault();
            var byCaption = form.Controls.OfType<Control>()
                .SelectMany(AllChildren)
                .OfType<Button>()
                .FirstOrDefault(button => string.Equals(button.Text.Trim(), caption, StringComparison.Ordinal));

            var target = byName ?? byCaption;
            if (target != null && target.Enabled && target.Visible)
                target.PerformClick();
        }

        private static System.Collections.Generic.IEnumerable<Control> AllChildren(Control root)
        {
            yield return root;
            foreach (Control child in root.Controls)
                foreach (Control nested in AllChildren(child))
                    yield return nested;
        }
    }
}
