using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// ADR-017: ترتيب شاشة إدارة المستخدمين المعتمد.
    /// يضع عوامل التصفية فوق جدول النتائج، ويمنع تداخل الأزرار والحقول.
    /// </summary>
    public partial class FrmUsers
    {
        private readonly GroupBox _grpUserFilters = new();
        private readonly GroupBox _grpUserAudit = new();
        private readonly TextBox _txtUserSearch = new();
        private readonly ComboBox _cmbFilterBranch = new();
        private readonly ComboBox _cmbFilterStatus = new();
        private readonly Button _btnResetPassword = new();
        private readonly Button _btnReactivate = new();

        private void ApplyApprovedUsersLayout()
        {
            SuspendLayout();

            Text = "إدارة المستخدمين والصلاحيات";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            MinimumSize = new Size(1180, 760);
            Font = new Font("Tahoma", 10F);

            ConfigureToolbar();
            ConfigureUserDataArea();
            ConfigureFilters();
            ConfigureUsersGrid();
            ConfigureAudit();

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = Color.White,
                Padding = new Padding(8),
                RightToLeft = RightToLeft.Yes
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 248F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 86F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));

            Controls.Remove(pnlToolbar);
            Controls.Remove(pnlData);
            Controls.Remove(pnlGrid);

            root.Controls.Add(pnlToolbar, 0, 0);
            root.Controls.Add(pnlData, 0, 1);
            root.Controls.Add(_grpUserFilters, 0, 2);
            root.Controls.Add(pnlGrid, 0, 3);
            root.Controls.Add(_grpUserAudit, 0, 4);
            Controls.Add(root);

            ResumeLayout(true);
        }

        private void ConfigureToolbar()
        {
            pnlToolbar.Controls.Clear();
            pnlToolbar.Dock = DockStyle.Fill;
            pnlToolbar.Padding = new Padding(10, 8, 10, 8);
            pnlToolbar.BackColor = Color.FromArgb(248, 250, 253);
            pnlToolbar.BorderStyle = BorderStyle.FixedSingle;
            pnlToolbar.RightToLeft = RightToLeft.Yes;

            btnNew.Text = "جديد";
            btnSave.Text = "حفظ";
            btnEdit.Text = "تعديل";
            btnDelete.Text = "إيقاف";
            btnRefresh.Text = "تحديث";
            btnSearch.Text = "بحث";
            btnPrint.Text = "طباعة";
            btnClose.Text = "إغلاق";
            _btnReactivate.Text = "إعادة تفعيل";
            _btnResetPassword.Text = "إعادة تعيين كلمة المرور";

            _btnReactivate.Click += async (_, _) => await ReactivateSelectedUserAsync();
            _btnResetPassword.Click += (_, _) => BeginPasswordReset();

            btnSearch.Click -= btnSearch_Click;
            btnSearch.Click += (_, _) =>
            {
                _txtUserSearch.Focus();
                ApplyUsersFilter();
            };

            var strip = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(2),
                BackColor = Color.Transparent
            };

            foreach (var button in new[]
                     {
                         btnNew, btnSave, btnEdit, btnDelete, _btnReactivate,
                         _btnResetPassword, btnRefresh, btnSearch, btnPrint, btnClose
                     })
            {
                button.Size = new Size(button == _btnResetPassword ? 150 : 100, 48);
                button.Margin = new Padding(4);
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderColor = Color.FromArgb(175, 199, 230);
                button.FlatAppearance.MouseOverBackColor = Color.FromArgb(232, 240, 252);
                button.BackColor = Color.White;
                button.ForeColor = Color.FromArgb(8, 49, 92);
                button.Font = new Font("Tahoma", 9.5F, FontStyle.Bold);
                button.TextAlign = ContentAlignment.MiddleCenter;
                strip.Controls.Add(button);
            }

            pnlToolbar.Controls.Add(strip);
        }

        private void ConfigureUserDataArea()
        {
            pnlData.Dock = DockStyle.Fill;
            pnlData.Padding = new Padding(4);
            pnlData.BackColor = Color.White;
            grpPermissions.Visible = false;

            grpUserData.Dock = DockStyle.Fill;
            grpUserData.Text = "بيانات المستخدم";
            grpUserData.ForeColor = Color.FromArgb(8, 49, 92);
            grpUserData.Font = new Font("Tahoma", 11F, FontStyle.Bold);
            grpUserData.Padding = new Padding(12);

            panel1.Controls.Clear();
            panel1.Dock = DockStyle.Fill;
            panel1.RightToLeft = RightToLeft.Yes;

            txtPassword.UseSystemPasswordChar = true;
            txtConfirmPassword.UseSystemPasswordChar = true;
            txtUserName.ReadOnly = true;
            txtUserName.Text = "(جديد)";
            txtNotes.Multiline = false;

            var fields = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RightToLeft = RightToLeft.Yes,
                ColumnCount = 4,
                RowCount = 6,
                Padding = new Padding(8)
            };
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32F));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32F));
            for (var row = 0; row < 6; row++)
                fields.RowStyles.Add(new RowStyle(SizeType.Percent, 16.66F));

            AddField(fields, 0, 0, "رقم المستخدم", txtUserName);
            AddField(fields, 2, 0, "اسم الدخول", txtLoginName);
            AddField(fields, 0, 1, "الاسم الكامل", txtFullName);
            AddField(fields, 2, 1, "الهاتف", txtPhone);
            AddField(fields, 0, 2, "الدور", cmbRole);
            AddField(fields, 2, 2, "نطاق الفرع", cmbBranch);
            AddField(fields, 0, 3, "الحالة", cmbStatus);
            AddField(fields, 2, 3, "سبب القفل / ملاحظات", txtNotes);
            AddField(fields, 0, 4, "كلمة المرور", txtPassword);
            AddField(fields, 2, 4, "تأكيد كلمة المرور", txtConfirmPassword);

            chkChangePassword.Text = "إجبار المستخدم على تغيير كلمة المرور عند أول دخول";
            chkChangePassword.AutoSize = true;
            chkChangePassword.ForeColor = Color.FromArgb(8, 49, 92);
            chkIsActive.Text = "الحساب نشط";
            chkIsActive.AutoSize = true;
            chkIsActive.ForeColor = Color.FromArgb(8, 49, 92);

            var accountFlags = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(4)
            };
            accountFlags.Controls.Add(chkChangePassword);
            accountFlags.Controls.Add(chkIsActive);
            fields.Controls.Add(accountFlags, 0, 5);
            fields.SetColumnSpan(accountFlags, 4);
            panel1.Controls.Add(fields);
        }

        private static void AddField(TableLayoutPanel table, int column, int row, string caption, Control control)
        {
            var label = new Label
            {
                Text = caption + ":",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Tahoma", 10F),
                ForeColor = Color.FromArgb(30, 41, 59)
            };
            control.Dock = DockStyle.Fill;
            control.Margin = new Padding(5);
            if (control is ComboBox combo) combo.DropDownStyle = ComboBoxStyle.DropDownList;
            table.Controls.Add(label, column, row);
            table.Controls.Add(control, column + 1, row);
        }

        private void ConfigureFilters()
        {
            _grpUserFilters.Controls.Clear();
            _grpUserFilters.Dock = DockStyle.Fill;
            _grpUserFilters.Text = "عوامل التصفية";
            _grpUserFilters.ForeColor = Color.FromArgb(8, 49, 92);
            _grpUserFilters.Font = new Font("Tahoma", 10F, FontStyle.Bold);
            _grpUserFilters.Padding = new Padding(12);

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RightToLeft = RightToLeft.Yes,
                ColumnCount = 6,
                RowCount = 1,
                Padding = new Padding(4)
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27.5F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27.5F));

            AddFilter(panel, 0, "بحث بالاسم أو رقم المستخدم", _txtUserSearch);
            AddFilter(panel, 2, "الفرع", _cmbFilterBranch);
            AddFilter(panel, 4, "الحالة", _cmbFilterStatus);

            _txtUserSearch.PlaceholderText = "اكتب الاسم أو اسم الدخول أو رقم المستخدم";
            _cmbFilterBranch.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbFilterStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbFilterStatus.Items.Clear();
            _cmbFilterStatus.Items.AddRange(new object[] { "الكل", "نشط", "موقوف" });
            _cmbFilterStatus.SelectedIndex = 0;
            _txtUserSearch.TextChanged += (_, _) => ApplyUsersFilter();
            _cmbFilterStatus.SelectedIndexChanged += (_, _) => ApplyUsersFilter();
            _cmbFilterBranch.SelectedIndexChanged += (_, _) => ApplyUsersFilter();

            _grpUserFilters.Controls.Add(panel);
        }

        private static void AddFilter(TableLayoutPanel table, int column, string caption, Control control)
        {
            var label = new Label
            {
                Text = caption + ":",
                AutoSize = true,
                Anchor = AnchorStyles.Right,
                Font = new Font("Tahoma", 9F)
            };
            control.Dock = DockStyle.Fill;
            control.Margin = new Padding(6, 4, 14, 4);
            table.Controls.Add(label, column, 0);
            table.Controls.Add(control, column + 1, 0);
        }

        private void ConfigureUsersGrid()
        {
            pnlGrid.Dock = DockStyle.Fill;
            pnlGrid.Padding = new Padding(4);
            pnlGrid.BackColor = Color.White;
            dgvUsers.Dock = DockStyle.Fill;
            dgvUsers.RightToLeft = RightToLeft.Yes;
            dgvUsers.BackgroundColor = Color.White;
            dgvUsers.BorderStyle = BorderStyle.FixedSingle;
            dgvUsers.EnableHeadersVisualStyles = false;
            dgvUsers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(232, 238, 245);
            dgvUsers.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(8, 49, 92);
            dgvUsers.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvUsers.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvUsers.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvUsers.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvUsers.ColumnHeadersHeight = 36;
            dgvUsers.RowTemplate.Height = 30;
        }

        private void ConfigureAudit()
        {
            _grpUserAudit.Controls.Clear();
            _grpUserAudit.Dock = DockStyle.Fill;
            _grpUserAudit.Text = "التدقيق";
            _grpUserAudit.ForeColor = Color.FromArgb(8, 49, 92);
            _grpUserAudit.Font = new Font("Tahoma", 10F, FontStyle.Bold);
            var text = new Label
            {
                Dock = DockStyle.Fill,
                Text = "أنشئ بواسطة: —     |     تاريخ الإنشاء: —     |     آخر تعديل: —",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Tahoma", 9F),
                ForeColor = Color.FromArgb(51, 65, 85)
            };
            _grpUserAudit.Controls.Add(text);
        }

        private void InitializeApprovedUserFilters()
        {
            var branches = cmbBranch.DataSource as IEnumerable<BranchLookupModel>;
            if (branches == null) return;
            var choices = branches.ToList();
            choices.Insert(0, new BranchLookupModel { Branch_ID = 0, Branch_Name = "الكل" });
            _cmbFilterBranch.DataSource = choices;
            _cmbFilterBranch.DisplayMember = "Branch_Name";
            _cmbFilterBranch.ValueMember = "Branch_ID";
            _cmbFilterBranch.SelectedIndex = 0;
        }

        private void ApplyUsersFilter()
        {
            if (_isBinding) return;
            var term = _txtUserSearch.Text.Trim();
            var branchId = _cmbFilterBranch.SelectedValue is int id ? id : 0;

            var filtered = _usersCache.Where(user =>
                (string.IsNullOrWhiteSpace(term) ||
                 user.Full_Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                 user.Login_Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                 user.User_ID.ToString().Contains(term, StringComparison.OrdinalIgnoreCase)) &&
                (branchId <= 0 || user.Branch_ID == branchId) &&
                (_cmbFilterStatus.SelectedIndex <= 0 ||
                 (_cmbFilterStatus.Text == "نشط" ? user.Is_Active : !user.Is_Active))).ToList();

            DisplayUsersInGrid(filtered);
        }

        private void BeginPasswordReset()
        {
            if (_selectedUserId == 0)
            {
                MessageBox.Show("اختر مستخدماً أولاً لإعادة تعيين كلمة المرور.", "إدارة المستخدمين", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            txtPassword.Clear();
            txtConfirmPassword.Clear();
            txtPassword.Focus();
            MessageBox.Show("أدخل كلمة المرور الجديدة وتأكيدها في بيانات المستخدم، ثم اضغط تعديل. لن تظهر كلمة المرور القديمة.", "إعادة تعيين كلمة المرور", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async System.Threading.Tasks.Task ReactivateSelectedUserAsync()
        {
            if (_selectedUserId == 0)
            {
                MessageBox.Show("اختر مستخدماً أولاً لإعادة تفعيله.", "إدارة المستخدمين", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            cmbStatus.Text = "نشط";
            chkIsActive.Checked = true;
            await ExecuteUpdateAsync();
        }
    }
}
