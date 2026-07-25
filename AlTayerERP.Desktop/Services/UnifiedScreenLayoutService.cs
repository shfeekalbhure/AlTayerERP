using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AlTayerERP.Desktop.Services
{
    /// <summary>
    /// يطبق عقد التخطيط الداخلي الموحد على شاشات المرحلة الأولى بعد استضافتها
    /// داخل مساحة العمل، دون تغيير منطق الأعمال أو أسماء الأدوات أو مصادر البيانات.
    /// </summary>
    internal static class UnifiedScreenLayoutService
    {
        public static void Apply(Form form)
        {
            if (form.IsDisposed)
                return;

            form.SuspendLayout();
            try
            {
                NormalizeForm(form);
                NormalizeRoot(form);
                NormalizeSplitContainers(form);
                NormalizeTables(form);
                NormalizeDataGrids(form);
                NormalizeSearchInputs(form);
                NormalizeToolbars(form);
            }
            finally
            {
                form.ResumeLayout(true);
                form.PerformLayout();
                form.Invalidate(true);
            }
        }

        private static void NormalizeForm(Form form)
        {
            form.AutoScroll = false;
            form.MinimumSize = Size.Empty;
            form.MaximumSize = Size.Empty;
            form.Margin = Padding.Empty;
            form.Padding = Padding.Empty;
            form.Dock = DockStyle.Fill;
        }

        private static void NormalizeRoot(Form form)
        {
            var roots = form.Controls.Cast<Control>()
                .Where(control => control.Visible && control is not MenuStrip && control is not StatusStrip)
                .ToList();

            if (roots.Count == 1)
            {
                roots[0].Dock = DockStyle.Fill;
                roots[0].Margin = Padding.Empty;
            }

            foreach (var root in roots)
            {
                if (root is TableLayoutPanel or SplitContainer or Panel)
                {
                    root.Margin = Padding.Empty;
                    if (root.Dock == DockStyle.None && root.Width >= form.ClientSize.Width * 0.80)
                        root.Dock = DockStyle.Fill;
                }
            }
        }

        private static void NormalizeSplitContainers(Control root)
        {
            foreach (var split in FindControls<SplitContainer>(root))
            {
                split.Dock = DockStyle.Fill;
                split.Margin = Padding.Empty;
                split.Panel1.Padding = Padding.Empty;
                split.Panel2.Padding = Padding.Empty;

                if (split.Orientation == Orientation.Horizontal)
                {
                    var minimumTop = Math.Min(320, Math.Max(180, split.Height / 3));
                    if (split.SplitterDistance < minimumTop)
                        split.SplitterDistance = minimumTop;
                }
            }
        }

        private static void NormalizeTables(Control root)
        {
            foreach (var table in FindControls<TableLayoutPanel>(root))
            {
                table.Margin = Padding.Empty;
                if (table.Parent is Form or TabPage or Panel && table.Dock == DockStyle.None)
                    table.Dock = DockStyle.Fill;

                StretchGridRows(table);
            }
        }

        private static void StretchGridRows(TableLayoutPanel table)
        {
            var gridRows = table.Controls.Cast<Control>()
                .Where(control => control is DataGridView || ContainsDataGrid(control))
                .Select(control => table.GetPositionFromControl(control).Row)
                .Where(row => row >= 0)
                .Distinct()
                .ToList();

            if (gridRows.Count == 0 || table.RowCount == 0)
                return;

            EnsureRowStyles(table);

            foreach (var row in gridRows)
                table.RowStyles[row] = new RowStyle(SizeType.Percent, 100F / gridRows.Count);

            for (var row = 0; row < table.RowCount; row++)
            {
                if (gridRows.Contains(row))
                    continue;

                var controls = table.Controls.Cast<Control>()
                    .Where(control => table.GetPositionFromControl(control).Row == row)
                    .ToList();

                if (controls.Count == 0)
                    continue;

                var preferred = controls.Max(control => control.PreferredSize.Height + control.Margin.Vertical);
                var current = table.RowStyles[row];
                if (current.SizeType == SizeType.Percent)
                    table.RowStyles[row] = new RowStyle(SizeType.Absolute, Math.Max(34, preferred));
            }
        }

        private static void EnsureRowStyles(TableLayoutPanel table)
        {
            while (table.RowStyles.Count < table.RowCount)
                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

        private static void NormalizeDataGrids(Control root)
        {
            foreach (var grid in FindControls<DataGridView>(root))
            {
                grid.Margin = Padding.Empty;
                grid.AutoSizeColumnsMode = grid.AutoSizeColumnsMode == DataGridViewAutoSizeColumnsMode.None
                    ? DataGridViewAutoSizeColumnsMode.Fill
                    : grid.AutoSizeColumnsMode;
                grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
                grid.RowHeadersVisible = false;
                grid.AllowUserToResizeRows = false;
                grid.BackgroundColor = Color.White;

                // لا نملأ النموذج مباشرة عندما توجد حقول أعلى الجدول؛
                // ذلك كان يجعل الجدول يغطي حقول الإدخال في الشاشات القديمة.
                // داخل حاوية مخصصة للجدول يظل Dock.Fill هو السلوك الصحيح،
                // أما الجدول المباشر فيتمدد بالـ Anchor مع أبعاد الشاشة.
                if (CanFillGrid(grid))
                {
                    grid.Dock = DockStyle.Fill;
                }
                else
                {
                    grid.Dock = DockStyle.None;
                    grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom |
                                  AnchorStyles.Left | AnchorStyles.Right;
                }

                if (grid.Parent is Panel panel)
                {
                    panel.Padding = Padding.Empty;
                    panel.Margin = Padding.Empty;
                }
            }
        }

        private static bool CanFillGrid(DataGridView grid)
        {
            if (grid.Parent is TableLayoutPanel or SplitterPanel or TabPage)
                return true;

            if (grid.Parent is not Panel panel)
                return false;

            // Panel مخصص للجدول وحده، أو يحتوي عناصر Docked فقط.
            return panel.Controls.Count == 1 ||
                   panel.Controls.Cast<Control>().All(control =>
                       control == grid || control.Dock != DockStyle.None);
        }

        private static void NormalizeSearchInputs(Control root)
        {
            foreach (var textBox in FindControls<TextBox>(root))
            {
                var name = textBox.Name ?? string.Empty;
                var looksLikeSearch = name.Contains("search", StringComparison.OrdinalIgnoreCase) ||
                                      name.Contains("بحث", StringComparison.OrdinalIgnoreCase) ||
                                      (textBox.PlaceholderText?.Contains("بحث", StringComparison.OrdinalIgnoreCase) ?? false);

                if (!looksLikeSearch)
                    continue;

                textBox.Height = Math.Max(34, textBox.Height);
                textBox.Margin = new Padding(4);
                if (textBox.Parent is Panel panel && textBox.Dock == DockStyle.None)
                    textBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            }
        }

        private static void NormalizeToolbars(Control root)
        {
            foreach (var flow in FindControls<FlowLayoutPanel>(root))
            {
                if (flow.Controls.OfType<Button>().Count() < 2)
                    continue;

                flow.AutoSize = false;
                flow.WrapContents = false;
                flow.AutoScroll = true;
                flow.Height = Math.Max(46, flow.Height);
                flow.Padding = new Padding(4);
            }
        }

        private static bool ContainsDataGrid(Control root) =>
            root is DataGridView || root.Controls.Cast<Control>().Any(ContainsDataGrid);

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
}
