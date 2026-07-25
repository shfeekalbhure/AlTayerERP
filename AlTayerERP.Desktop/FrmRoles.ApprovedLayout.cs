using System.Drawing;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /*
     * دليل عربي لشاشة الأدوار:
     * txtRoleCode = كود الدور، txtRoleName = اسم الدور، txtDescription = وصف الدور،
     * cmbStatus = حالة الدور، dgvRoles = جدول الأدوار.
     * btnNew جديد، btnSave حفظ، btnEdit تعديل، btnDelete إيقاف،
     * btnSearch بحث، btnRefresh تحديث، btnClose إغلاق، btnPermissions صلاحيات الدور.
     */

    public partial class FrmRoles
    {
        private readonly Label _lblRolesFooter = new();

        /// <summary>
        /// تخطيط شاشة الأدوار المعتمد: شريط أدوات، عنوان، بيانات الدور، جدول مرن، وتذييل ثابت.
        /// لا يعتمد على الإحداثيات الثابتة حتى تعمل الشاشة داخل مساحة FrmMain.
        /// </summary>
        private void ApplyApprovedRolesLayout()
        {
            SuspendLayout();

            MinimumSize = Size.Empty;
            MaximumSize = Size.Empty;
            AutoScroll = false;
            BackColor = Color.FromArgb(244, 247, 251);
            Font = new Font("Segoe UI", 9F);
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;

            ConfigureRolesToolbar();
            ConfigureRolesDataCard();
            ConfigureRolesGrid();

            Controls.Clear();

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(12),
                BackColor = BackColor
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 220F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));

            root.Controls.Add(pnlToolbar, 0, 0);
            root.Controls.Add(CreateRolesHeader(), 0, 1);
            root.Controls.Add(pnlData, 0, 2);
            root.Controls.Add(pnlGrid, 0, 3);
            root.Controls.Add(CreateRolesFooter(), 0, 4);

            Controls.Add(root);

            dgvRoles.RowsAdded += (_, _) => UpdateRolesFooter();
            dgvRoles.RowsRemoved += (_, _) => UpdateRolesFooter();
            UpdateRolesFooter();

            ResumeLayout(true);
        }

        private void ConfigureRolesToolbar()
        {
            pnlToolbar.Controls.Clear();
            pnlToolbar.Dock = DockStyle.Fill;
            pnlToolbar.BackColor = Color.White;
            pnlToolbar.BorderStyle = BorderStyle.FixedSingle;
            pnlToolbar.Padding = new Padding(8);

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Margin = Padding.Empty
            };

            ConfigureRolesButton(btnNew, "جديد  Ctrl+N", Color.FromArgb(36, 99, 168));
            ConfigureRolesButton(btnSave, "حفظ  Ctrl+S", Color.FromArgb(22, 125, 84));
            ConfigureRolesButton(btnEdit, "تعديل", Color.FromArgb(14, 116, 144));
            ConfigureRolesButton(btnDelete, "إيقاف", Color.FromArgb(180, 83, 9));
            ConfigureRolesButton(btnSearch, "بحث  Ctrl+F", Color.FromArgb(75, 85, 99));
            ConfigureRolesButton(btnRefresh, "تحديث  F5", Color.FromArgb(75, 85, 99));
            ConfigureRolesButton(_btnPermissions, "صلاحيات الدور", Color.FromArgb(79, 70, 229));
            ConfigureRolesButton(btnClose, "إغلاق  Esc", Color.FromArgb(107, 114, 128));

            _txtSearch.Width = 220;
            _txtSearch.Height = 32;
            _txtSearch.Margin = new Padding(12, 0, 4, 0);
            _txtSearch.PlaceholderText = "بحث بالكود أو اسم الدور";
            _txtSearch.Anchor = AnchorStyles.None;

            toolbar.Controls.Add(btnNew);
            toolbar.Controls.Add(btnSave);
            toolbar.Controls.Add(btnEdit);
            toolbar.Controls.Add(btnDelete);
            toolbar.Controls.Add(btnSearch);
            toolbar.Controls.Add(_txtSearch);
            toolbar.Controls.Add(btnRefresh);
            toolbar.Controls.Add(_btnPermissions);
            toolbar.Controls.Add(btnClose);
            pnlToolbar.Controls.Add(toolbar);
        }

        private static void ConfigureRolesButton(Button button, string text, Color color)
        {
            button.Text = text;
            button.AutoSize = false;
            button.Width = 108;
            button.Height = 32;
            button.Margin = new Padding(3, 0, 3, 0);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = color;
            button.ForeColor = Color.White;
            button.Cursor = Cursors.Hand;
            button.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            button.RightToLeft = RightToLeft.Yes;
        }

        private Control CreateRolesHeader()
        {
            var header = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 64, 112),
                Padding = new Padding(14, 4, 14, 4),
                Margin = new Padding(0, 6, 0, 6)
            };

            header.Controls.Add(new Label
            {
                Text = "إدارة الأدوار",
                Dock = DockStyle.Right,
                Width = 260,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight
            });
            header.Controls.Add(new Label
            {
                Text = "إدارة الأدوار النظامية وحالة تفعيلها",
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(220, 232, 247),
                Font = new Font("Segoe UI", 8.5F),
                TextAlign = ContentAlignment.MiddleLeft
            });
            return header;
        }

        private void ConfigureRolesDataCard()
        {
            pnlData.Dock = DockStyle.Fill;
            pnlData.Padding = Padding.Empty;
            pnlData.BackColor = Color.Transparent;

            grpRoleData.Controls.Clear();
            grpRoleData.Dock = DockStyle.Fill;
            grpRoleData.Text = "بيانات الدور";
            grpRoleData.Padding = new Padding(14, 24, 14, 12);
            grpRoleData.BackColor = Color.White;
            grpRoleData.ForeColor = Color.FromArgb(31, 58, 92);
            grpRoleData.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            txtDescription.Multiline = true;
            txtDescription.ScrollBars = ScrollBars.Vertical;

            var fields = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(4),
                BackColor = Color.White
            };
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 66F));
            fields.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            fields.Controls.Add(CreateRoleField("كود الدور", txtRoleCode, 34), 0, 0);
            fields.Controls.Add(CreateRoleField("اسم الدور", txtRoleName, 34), 1, 0);
            fields.Controls.Add(CreateRoleField("الوصف", txtDescription, 74), 0, 1);
            fields.Controls.Add(CreateRoleField("الحالة", cmbStatus, 34), 1, 1);

            grpRoleData.Controls.Add(fields);
            pnlData.Controls.Clear();
            pnlData.Controls.Add(grpRoleData);
        }

        private static Control CreateRoleField(string title, Control input, int inputHeight)
        {
            var field = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(8, 2, 8, 2)
            };
            field.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            field.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var label = new Label
            {
                Text = title,
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(55, 65, 81),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight
            };

            input.Dock = DockStyle.Fill;
            input.Height = inputHeight;
            input.Margin = Padding.Empty;
            input.Font = new Font("Segoe UI", 9F);
            if (input is ComboBox combo)
                combo.DropDownStyle = ComboBoxStyle.DropDownList;

            field.Controls.Add(label, 0, 0);
            field.Controls.Add(input, 0, 1);
            return field;
        }

        private void ConfigureRolesGrid()
        {
            pnlGrid.Dock = DockStyle.Fill;
            pnlGrid.Padding = new Padding(1);
            pnlGrid.BackColor = Color.White;
            pnlGrid.BorderStyle = BorderStyle.FixedSingle;

            dgvRoles.Dock = DockStyle.Fill;
            dgvRoles.EnableHeadersVisualStyles = false;
            dgvRoles.BackgroundColor = Color.White;
            dgvRoles.BorderStyle = BorderStyle.None;
            dgvRoles.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvRoles.GridColor = Color.FromArgb(226, 232, 240);
            dgvRoles.RowTemplate.Height = 30;
            dgvRoles.ColumnHeadersHeight = 36;
            dgvRoles.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(30, 64, 112),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };
            dgvRoles.DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleRight,
                SelectionBackColor = Color.FromArgb(218, 232, 247),
                SelectionForeColor = Color.FromArgb(20, 44, 75)
            };
            dgvRoles.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        }

        private Control CreateRolesFooter()
        {
            var footer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4, 3, 4, 0) };
            _lblRolesFooter.Dock = DockStyle.Right;
            _lblRolesFooter.Width = 180;
            _lblRolesFooter.ForeColor = Color.FromArgb(75, 85, 99);
            _lblRolesFooter.TextAlign = ContentAlignment.MiddleRight;

            footer.Controls.Add(_lblRolesFooter);
            footer.Controls.Add(new Label
            {
                Text = "يُمنع حذف أو إيقاف دور مدير النظام.",
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(75, 85, 99),
                TextAlign = ContentAlignment.MiddleLeft
            });
            return footer;
        }

        private void UpdateRolesFooter() =>
            _lblRolesFooter.Text = $"عدد الأدوار: {dgvRoles.Rows.Count}";
    }
}
