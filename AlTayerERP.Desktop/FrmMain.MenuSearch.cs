using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    public partial class FrmMain
    {
        private TextBox? _txtMenuSearch;
        private Panel? _menuSearchPanel;

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            EnsureMenuSearchBox();
        }

        private void EnsureMenuSearchBox()
        {
            if (_menuSearchPanel != null || pnlSideMenu.IsDisposed)
                return;

            _menuSearchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                Padding = new Padding(8, 7, 8, 7),
                BackColor = Color.FromArgb(5, 36, 69)
            };

            _txtMenuSearch = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.5F),
                PlaceholderText = "ابحث في شجرة النظام...",
                RightToLeft = RightToLeft.Yes,
                AccessibleName = "بحث شجرة النظام"
            };

            _txtMenuSearch.TextChanged += (_, _) => FilterMainMenuTree(_txtMenuSearch.Text);
            _txtMenuSearch.KeyDown += (_, args) =>
            {
                if (args.KeyCode == Keys.Down && tvMainMenu.Nodes.Count > 0)
                {
                    tvMainMenu.Focus();
                    tvMainMenu.SelectedNode = tvMainMenu.Nodes[0].Nodes.Count > 0
                        ? tvMainMenu.Nodes[0].Nodes[0]
                        : tvMainMenu.Nodes[0];
                    args.Handled = true;
                }
                else if (args.KeyCode == Keys.Escape)
                {
                    _txtMenuSearch.Clear();
                    tvMainMenu.Focus();
                    args.Handled = true;
                }
            };

            _menuSearchPanel.Controls.Add(_txtMenuSearch);
            pnlSideMenu.Controls.Add(_menuSearchPanel);
            _menuSearchPanel.BringToFront();
        }

        private void FilterMainMenuTree(string? searchText)
        {
            if (!_permissionsLoaded)
                return;

            var query = (searchText ?? string.Empty).Trim();
            tvMainMenu.BeginUpdate();
            try
            {
                tvMainMenu.Nodes.Clear();
                var rows = _allowedScreens
                    .Where(x => x.Is_Active && IsSupportedScreen(x.Screen_Code))
                    .Where(x => string.IsNullOrWhiteSpace(query) ||
                                x.Screen_Name.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                                x.Screen_Code.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                x.Module_Name.Contains(query, StringComparison.CurrentCultureIgnoreCase))
                    .OrderBy(x => x.Module_Name)
                    .ThenBy(x => x.Sort_Order)
                    .ThenBy(x => x.Screen_Name)
                    .ToList();

                foreach (var module in rows.GroupBy(x => string.IsNullOrWhiteSpace(x.Module_Name)
                             ? "شاشات النظام"
                             : x.Module_Name))
                {
                    var root = new TreeNode(module.Key);
                    foreach (var screen in module)
                        root.Nodes.Add(screen.Screen_Code, screen.Screen_Name);

                    if (root.Nodes.Count > 0)
                    {
                        root.Expand();
                        tvMainMenu.Nodes.Add(root);
                    }
                }
            }
            finally
            {
                tvMainMenu.EndUpdate();
            }
        }
    }
}
