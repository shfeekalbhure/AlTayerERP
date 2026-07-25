using System;
using System.Drawing;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    public partial class FrmMain
    {
        private TextBox? _txtMenuSearch;
        private Panel? _menuSearchPanel;

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
                TextAlign = HorizontalAlignment.Right,
                RightToLeft = RightToLeft.Yes,
                AccessibleName = "بحث شجرة النظام"
            };

            _txtMenuSearch.TextChanged += (_, _) => BuildMainMenu();
            _txtMenuSearch.KeyDown += (_, args) =>
            {
                if (args.KeyCode == Keys.Down && tvMainMenu.Nodes.Count > 0)
                {
                    tvMainMenu.Focus();
                    tvMainMenu.SelectedNode = tvMainMenu.Nodes[0].Nodes.Count > 0
                        ? tvMainMenu.Nodes[0].Nodes[0]
                        : tvMainMenu.Nodes[0];
                    args.SuppressKeyPress = true;
                    args.Handled = true;
                }
                else if (args.KeyCode == Keys.Escape)
                {
                    _txtMenuSearch.Clear();
                    tvMainMenu.Focus();
                    args.SuppressKeyPress = true;
                    args.Handled = true;
                }
            };

            _menuSearchPanel.Controls.Add(_txtMenuSearch);
            pnlSideMenu.Controls.Add(_menuSearchPanel);
            _menuSearchPanel.BringToFront();
        }
    }
}
