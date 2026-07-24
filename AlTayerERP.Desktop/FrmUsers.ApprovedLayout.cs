using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// ADR-017: التصميم النهائي المغلق لشاشة إدارة المستخدمين.
    /// ترتيب ثابت RTL، عوامل التصفية فوق النتائج، شريط أدوات مرن، وجدول واضح قابل للطباعة.
    /// </summary>
    public partial class FrmUsers
    {
        private readonly GroupBox _grpUserFilters = new();
        private readonly GroupBox _grpUserAudit = new();
        private readonly TextBox _txtUserSearch = new();
        private readonly ComboBox _cmbFilterBranch = new();
        private readonly ComboBox _cmbFilterRole = new();
        private readonly ComboBox _cmbFilterStatus = new();
        private readonly Button _btnClearFilters = new();
        private readonly Button _btnResetPassword = new();
        private readonly Button _btnReactivate = new();
        private readonly Label _lblResultCount = new();
        private readonly Label _lblSelectionState = new();

        private void ApplyApprovedUsersLayout()
        {
            SuspendLayout();

            Text = "إدارة المستخدمين والصلاحيات";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            MinimumSize = new Size(1120, 720);
            Font = new Font("Tahoma", 9.5F);
            BackColor = Color.FromArgb(244, 247, 251);

            ConfigureToolbar();
            ConfigureUserDataArea();
            ConfigureFilters();
            ConfigureUsersGrid();
            ConfigureAudit();

            var header = BuildScreenHeader();
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                BackColor = Color.FromArgb(244, 247, 251),
                Padding = new Padding(8),
                RightToLeft = RightToLeft.Yes
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 270F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 82F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));

            Controls.Remove(pnlToolbar);
            Controls.Remove(pnlData);
            Controls.Remove(pnlGrid);
            Controls.Clear();

            root.Controls.Add(header, 0, 0);
            root.Controls.Add(pnlToolbar, 0, 1);
            root.Controls.Add(pnlData, 0, 2);
            root.Controls.Add(_grpUserFilters, 0, 3);
            root.Controls.Add(pnlGrid, 0, 4);
            root.Controls.Add(_grpUserAudit, 0, 5);
            Controls.Add(root);

            ResumeLayout(true);
        }

        private Control BuildScreenHeader()
        {
            var header = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(16, 7, 16, 7),
                Margin = new Padding(0, 0, 0, 6)
            };

            var title = new Label
            {
                Dock = DockStyle.Right,
                Width = 420,
                Text = "إدارة المستخدمين والصلاحيات",
                Font = new Font("Tahoma", 15F, FontStyle.Bold),
                ForeColor = Color.FromArgb(8, 49, 92),
                TextAlign = ContentAlignment.MiddleRight
            };
            var subtitle = new Label
            {
                Dock = DockStyle.Fill,
                Text = "إدارة الحسابات والأدوار ونطاق الفروع وحالة الوصول",
                Font = new Font("Tahoma", 9F),
                ForeColor = Color.FromArgb(90, 105, 125),
                TextAlign = ContentAlignment.MiddleRight
            };
            _lblSelectionState.Dock = DockStyle.Left;
            _lblSelectionState.Width = 210;
            _lblSelectionState.Text = "الوضع: مستخدم جديد";
            _lblSelectionState.Font = new Font("Tahoma", 9F, FontStyle.Bold);
            _lblSelectionState.ForeColor = Color.FromArgb(37, 99, 235);
            _lblSelectionState.TextAlign = ContentAlignment.MiddleLeft;

            header.Controls.Add(subtitle);
            header.Controls.Add(title);
            header.Controls.Add(_lblSelectionState);
            return header;
        }

        private void ConfigureToolbar()
        {
            pnlToolbar.Controls.Clear();
            pnlToolbar.Dock = DockStyle.Fill;
            pnlToolbar.Padding = new Padding(10, 7, 10, 7);
            pnlToolbar.BackColor = Color.White;
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
            btnSearch.Click += (_, _) => { _txtUserSearch.Focus(); ApplyUsersFilter(); };
            btnPrint.Click += (_, _) => PrintUsersGrid();

            var strip = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(2),
                BackColor = Color.Transparent
            };

            ConfigureActionButton(btnNew, Color.FromArgb(37, 99, 235), Color.White, 92);
            ConfigureActionButton(btnSave, Color.FromArgb(22, 163, 74), Color.White, 92);
            ConfigureActionButton(btnEdit, Color.FromArgb(245, 158, 11), Color.White, 92);
            ConfigureActionButton(btnDelete, Color.FromArgb(220, 38, 38), Color.White, 92);
            ConfigureActionButton(_btnReactivate, Color.FromArgb(13, 148, 136), Color.White, 112);
            ConfigureActionButton(_btnResetPassword, Color.FromArgb(109, 40, 217), Color.White, 158);
            ConfigureActionButton(btnRefresh, Color.White, Color.FromArgb(8, 49, 92), 92);
            ConfigureActionButton(btnSearch, Color.White, Color.FromArgb(8, 49, 92), 92);
            ConfigureActionButton(btnPrint, Color.White, Color.FromArgb(8, 49, 92), 92);
            ConfigureActionButton(btnClose, Color.FromArgb(71, 85, 105), Color.White, 92);

            foreach (var button in new[]
                     {
                         btnNew, btnSave, btnEdit, btnDelete, _btnReactivate,
                         _btnResetPassword, btnRefresh, btnSearch, btnPrint, btnClose
                     })
                strip.Controls.Add(button);

            pnlToolbar.Controls.Add(strip);
        }

        private static void ConfigureActionButton(Button button, Color backColor, Color foreColor, int width)
        {
            button.Size = new Size(width, 46);
            button.Margin = new Padding(4);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = backColor == Color.White ? 1 : 0;
            button.FlatAppearance.BorderColor = Color.FromArgb(185, 200, 218);
            button.FlatAppearance.MouseOverBackColor = backColor == Color.White
                ? Color.FromArgb(235, 242, 252)
                : ControlPaint.Light(backColor, 0.08F);
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.Font = new Font("Tahoma", 9F, FontStyle.Bold);
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.Cursor = Cursors.Hand;
        }

        private void ConfigureUserDataArea()
        {
            pnlData.Dock = DockStyle.Fill;
            pnlData.Padding = new Padding(3);
            pnlData.BackColor = Color.Transparent;
            grpPermissions.Visible = false;

            grpUserData.Dock = DockStyle.Fill;
            grpUserData.Text = "بيانات المستخدم";
            grpUserData.ForeColor = Color.FromArgb(8, 49, 92);
            grpUserData.Font = new Font("Tahoma", 10.5F, FontStyle.Bold);
            grpUserData.Padding = new Padding(12, 8, 12, 10);
            grpUserData.BackColor = Color.White;

            panel1.Controls.Clear();
            panel1.Dock = DockStyle.Fill;
            panel1.RightToLeft = RightToLeft.Yes;
            panel1.BackColor = Color.White;

            txtPassword.UseSystemPasswordChar = true;
            txtConfirmPassword.UseSystemPasswordChar = true;
            txtUserName.ReadOnly = true;
            txtUserName.BackColor = Color.FromArgb(241, 245, 249);
            txtUserName.Text = "(جديد)";
            txtNotes.Multiline = false;

            var fields = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RightToLeft = RightToLeft.Yes,
                ColumnCount = 4,
                RowCount = 7,
                Padding = new Padding(6)
            };
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16F));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16F));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            for (var row = 0; row < 7; row++) fields.RowStyles.Add(new RowStyle(SizeType.Percent, 14.285F));

            AddField(fields, 0, 0, "رقم المستخدم", txtUserName);
            AddField(fields, 2, 0, "اسم الدخول", txtLoginName);
            AddField(fields, 0, 1, "الاسم الكامل", txtFullName);
            AddField(fields, 2, 1, "الهاتف", txtPhone);
            AddField(fields, 0, 2, "البريد الإلكتروني", txtEmail);
            AddField(fields, 2, 2, "الدور", cmbRole);
            AddField(fields, 0, 3, "نطاق الفرع", cmbBranch);
            AddField(fields, 2, 3, "الحالة", cmbStatus);
            AddField(fields, 0, 4, "الملاحظات / سبب القفل", txtNotes);
            fields.SetColumnSpan(txtNotes, 3);
            AddField(fields, 0, 5, "كلمة المرور", txtPassword);
            AddField(fields, 2, 5, "تأكيد كلمة المرور", txtConfirmPassword);

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
            fields.Controls.Add(accountFlags, 0, 6);
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
                Font = new Font("Tahoma", 9.5F),
                ForeColor = Color.FromArgb(30, 41, 59),
                Margin = new Padding(4)
            };
            control.Dock = DockStyle.Fill;
            control.Margin = new Padding(5, 3, 5, 3);
            control.Font = new Font("Tahoma", 9.5F);
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
            _grpUserFilters.Font = new Font("Tahoma", 9.5F, FontStyle.Bold);
            _grpUserFilters.Padding = new Padding(10, 6, 10, 8);
            _grpUserFilters.BackColor = Color.White;

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RightToLeft = RightToLeft.Yes,
                ColumnCount = 10,
                RowCount = 1,
                Padding = new Padding(2)
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 104F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));

            AddFilter(panel, 0, "بحث", _txtUserSearch);
            AddFilter(panel, 2, "الفرع", _cmbFilterBranch);
            AddFilter(panel, 4, "الدور", _cmbFilterRole);
            AddFilter(panel, 6, "الحالة", _cmbFilterStatus);

            _btnClearFilters.Text = "مسح التصفية";
            ConfigureActionButton(_btnClearFilters, Color.White, Color.FromArgb(8, 49, 92), 100);
            _btnClearFilters.Dock = DockStyle.Fill;
            _btnClearFilters.Margin = new Padding(4, 2, 4, 2);
            _btnClearFilters.Click += (_, _) => ClearUserFilters();
            panel.Controls.Add(_btnClearFilters, 8, 0);

            _lblResultCount.Dock = DockStyle.Fill;
            _lblResultCount.Text = "النتائج: 0";
            _lblResultCount.TextAlign = ContentAlignment.MiddleCenter;
            _lblResultCount.Font = new Font("Tahoma", 9F, FontStyle.Bold);
            _lblResultCount.ForeColor = Color.FromArgb(51, 65, 85);
            panel.Controls.Add(_lblResultCount, 9, 0);

            _txtUserSearch.PlaceholderText = "الاسم أو اسم الدخول أو الرقم";
            _cmbFilterBranch.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbFilterRole.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbFilterStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbFilterStatus.Items.Clear();
            _cmbFilterStatus.Items.AddRange(new object[] { "الكل", "نشط", "موقوف" });
            _cmbFilterStatus.SelectedIndex = 0;
            _txtUserSearch.TextChanged += (_, _) => ApplyUsersFilter();
            _cmbFilterStatus.SelectedIndexChanged += (_, _) => ApplyUsersFilter();
            _cmbFilterBranch.SelectedIndexChanged += (_, _) => ApplyUsersFilter();
            _cmbFilterRole.SelectedIndexChanged += (_, _) => ApplyUsersFilter();

            _grpUserFilters.Controls.Add(panel);
        }

        private static void AddFilter(TableLayoutPanel table, int column, string caption, Control control)
        {
            var label = new Label
            {
                Text = caption + ":",
                AutoSize = true,
                Anchor = AnchorStyles.Right,
                Font = new Font("Tahoma", 9F),
                ForeColor = Color.FromArgb(51, 65, 85)
            };
            control.Dock = DockStyle.Fill;
            control.Margin = new Padding(5, 3, 10, 3);
            table.Controls.Add(label, column, 0);
            table.Controls.Add(control, column + 1, 0);
        }

        private void ConfigureUsersGrid()
        {
            pnlGrid.Dock = DockStyle.Fill;
            pnlGrid.Padding = new Padding(3);
            pnlGrid.BackColor = Color.White;
            dgvUsers.Dock = DockStyle.Fill;
            dgvUsers.RightToLeft = RightToLeft.Yes;
            dgvUsers.BackgroundColor = Color.White;
            dgvUsers.BorderStyle = BorderStyle.FixedSingle;
            dgvUsers.EnableHeadersVisualStyles = false;
            dgvUsers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(8, 49, 92);
            dgvUsers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvUsers.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 9F, FontStyle.Bold);
            dgvUsers.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvUsers.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvUsers.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvUsers.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvUsers.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgvUsers.GridColor = Color.FromArgb(220, 228, 238);
            dgvUsers.ColumnHeadersHeight = 38;
            dgvUsers.RowTemplate.Height = 31;
            dgvUsers.MultiSelect = false;
            dgvUsers.AllowUserToResizeRows = false;
            dgvUsers.RowsAdded += (_, _) => UpdateResultCount();
            dgvUsers.RowsRemoved += (_, _) => UpdateResultCount();
            dgvUsers.SelectionChanged += (_, _) => UpdateSelectionVisualState();
        }

        private void ConfigureAudit()
        {
            _grpUserAudit.Controls.Clear();
            _grpUserAudit.Dock = DockStyle.Fill;
            _grpUserAudit.Text = "حالة السجل";
            _grpUserAudit.ForeColor = Color.FromArgb(8, 49, 92);
            _grpUserAudit.Font = new Font("Tahoma", 9F, FontStyle.Bold);
            _grpUserAudit.BackColor = Color.White;
            var text = new Label
            {
                Dock = DockStyle.Fill,
                Text = "حدد مستخدماً من الجدول لعرض بياناته. جميع عمليات الحفظ والتعديل والإيقاف مسجلة في سجل التدقيق.",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Tahoma", 8.5F),
                ForeColor = Color.FromArgb(71, 85, 105)
            };
            _grpUserAudit.Controls.Add(text);
        }

        private void InitializeApprovedUserFilters()
        {
            var branches = cmbBranch.DataSource as IEnumerable<BranchLookupModel>;
            if (branches != null)
            {
                var choices = branches.ToList();
                choices.Insert(0, new BranchLookupModel { Branch_ID = 0, Branch_Name = "الكل" });
                _cmbFilterBranch.DataSource = choices;
                _cmbFilterBranch.DisplayMember = "Branch_Name";
                _cmbFilterBranch.ValueMember = "Branch_ID";
                _cmbFilterBranch.SelectedIndex = 0;
            }

            var roles = cmbRole.DataSource as IEnumerable<RoleLookupModel>;
            if (roles != null)
            {
                var roleChoices = roles.ToList();
                roleChoices.Insert(0, new RoleLookupModel { Role_ID = 0, Role_Name = "الكل" });
                _cmbFilterRole.DataSource = roleChoices;
                _cmbFilterRole.DisplayMember = "Role_Name";
                _cmbFilterRole.ValueMember = "Role_ID";
                _cmbFilterRole.SelectedIndex = 0;
            }
        }

        private void ApplyUsersFilter()
        {
            if (_isBinding) return;
            var term = _txtUserSearch.Text.Trim();
            var branchId = _cmbFilterBranch.SelectedValue is int id ? id : 0;
            var roleId = _cmbFilterRole.SelectedValue is int selectedRoleId ? selectedRoleId : 0;

            var filtered = _usersCache.Where(user =>
                (string.IsNullOrWhiteSpace(term) ||
                 user.Full_Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                 user.Login_Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                 user.User_ID.ToString().Contains(term, StringComparison.OrdinalIgnoreCase)) &&
                (branchId <= 0 || user.Branch_ID == branchId) &&
                (roleId <= 0 || user.Role_ID == roleId) &&
                (_cmbFilterStatus.SelectedIndex <= 0 ||
                 (_cmbFilterStatus.Text == "نشط" ? user.Is_Active : !user.Is_Active))).ToList();

            DisplayUsersInGrid(filtered);
            UpdateResultCount();
        }

        private void ClearUserFilters()
        {
            _isBinding = true;
            try
            {
                _txtUserSearch.Clear();
                if (_cmbFilterBranch.Items.Count > 0) _cmbFilterBranch.SelectedIndex = 0;
                if (_cmbFilterRole.Items.Count > 0) _cmbFilterRole.SelectedIndex = 0;
                if (_cmbFilterStatus.Items.Count > 0) _cmbFilterStatus.SelectedIndex = 0;
            }
            finally { _isBinding = false; }
            DisplayUsersInGrid(_usersCache);
            UpdateResultCount();
        }

        private void UpdateResultCount() => _lblResultCount.Text = $"النتائج: {dgvUsers.Rows.Count}";

        private void UpdateSelectionVisualState()
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                _lblSelectionState.Text = "الوضع: مستخدم جديد";
                _lblSelectionState.ForeColor = Color.FromArgb(37, 99, 235);
                return;
            }

            var row = dgvUsers.SelectedRows[0];
            var name = row.Cells["Full_Name"].Value?.ToString() ?? "مستخدم";
            var active = string.Equals(row.Cells["Is_Active"].Value?.ToString(), "نشط", StringComparison.OrdinalIgnoreCase);
            _lblSelectionState.Text = $"المحدد: {name} — {(active ? "نشط" : "موقوف")}";
            _lblSelectionState.ForeColor = active ? Color.FromArgb(22, 163, 74) : Color.FromArgb(220, 38, 38);
            btnDelete.Enabled = active;
            _btnReactivate.Enabled = !active;
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
            MessageBox.Show("أدخل كلمة المرور الجديدة وتأكيدها، ثم اضغط تعديل. لا يعرض النظام كلمة المرور القديمة.", "إعادة تعيين كلمة المرور", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void PrintUsersGrid()
        {
            if (dgvUsers.Rows.Count == 0)
            {
                MessageBox.Show("لا توجد بيانات مستخدمين للطباعة.", "الطباعة", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var printDocument = new PrintDocument { DocumentName = "قائمة المستخدمين" };
            printDocument.DefaultPageSettings.Landscape = true;
            var nextRow = 0;
            printDocument.PrintPage += (_, e) =>
            {
                using var titleFont = new Font("Tahoma", 14F, FontStyle.Bold);
                using var headerFont = new Font("Tahoma", 8.5F, FontStyle.Bold);
                using var rowFont = new Font("Tahoma", 8F);
                var bounds = e.MarginBounds;
                e.Graphics.DrawString("قائمة المستخدمين", titleFont, Brushes.Black, bounds.Right - 180, bounds.Top);
                var y = bounds.Top + 42;
                var visibleColumns = dgvUsers.Columns.Cast<DataGridViewColumn>().Where(c => c.Visible).ToList();
                var cellWidth = Math.Max(75, bounds.Width / Math.Max(1, visibleColumns.Count));
                var x = bounds.Right - cellWidth;
                foreach (var column in visibleColumns)
                {
                    e.Graphics.DrawRectangle(Pens.Gray, x, y, cellWidth, 28);
                    e.Graphics.DrawString(column.HeaderText, headerFont, Brushes.Black, new RectangleF(x + 2, y + 5, cellWidth - 4, 20));
                    x -= cellWidth;
                }
                y += 28;

                while (nextRow < dgvUsers.Rows.Count)
                {
                    if (y + 25 > bounds.Bottom)
                    {
                        e.HasMorePages = true;
                        return;
                    }
                    x = bounds.Right - cellWidth;
                    foreach (var column in visibleColumns)
                    {
                        var value = dgvUsers.Rows[nextRow].Cells[column.Index].FormattedValue?.ToString() ?? string.Empty;
                        e.Graphics.DrawRectangle(Pens.LightGray, x, y, cellWidth, 25);
                        e.Graphics.DrawString(value, rowFont, Brushes.Black, new RectangleF(x + 2, y + 4, cellWidth - 4, 18));
                        x -= cellWidth;
                    }
                    y += 25;
                    nextRow++;
                }
                e.HasMorePages = false;
            };

            using var preview = new PrintPreviewDialog
            {
                Document = printDocument,
                Width = 1100,
                Height = 750,
                RightToLeft = RightToLeft.Yes,
                RightToLeftLayout = true
            };
            preview.ShowDialog(this);
        }
    }
}
