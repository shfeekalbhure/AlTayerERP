using System;
using System.Drawing;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    public partial class FrmMain
    {
        private TextBox? _txtMenuSearch;
        private Panel? _menuSearchPanel;
        private Panel? _menuTreeHost;

        private void EnsureMenuSearchBox()
        {
            if (_menuSearchPanel != null || pnlSideMenu.IsDisposed)
                return;

            _menuSearchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 42,
                Padding = new Padding(6, 6, 6, 6),
                // شريط منفصل بصرياً، خارج بطاقة الشجرة.
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            _menuTreeHost = new Panel
            {
                Dock = DockStyle.Fill,
                // المسافة العلوية تُظهر أن الشريط السابق مستقل عن بطاقة الشجرة.
                Padding = new Padding(3, 8, 3, 3),
                BackColor = Color.FromArgb(5, 36, 69),
                BorderStyle = BorderStyle.FixedSingle
            };

            pnlSideMenu.SuspendLayout();
            pnlSideMenu.BackColor = Color.FromArgb(239, 243, 248);
            pnlSideMenu.Padding = new Padding(4);
            pnlSideMenu.Controls.Remove(tvMainMenu);
            tvMainMenu.Dock = DockStyle.Fill;
            _menuTreeHost.Controls.Add(tvMainMenu);

            _txtMenuSearch = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9F),
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

            // البحث فوق قالب الشجرة، والشجرة داخل قالب مستقل أسفله.
            pnlSideMenu.Controls.Add(_menuTreeHost);
            pnlSideMenu.Controls.Add(_menuSearchPanel);
            _menuSearchPanel.BringToFront();
            pnlSideMenu.ResumeLayout(true);
        }
    }
}
