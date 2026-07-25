using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /*
     * دليل عربي لشاشة المستخدمين:
     * txtUserName = رقم المستخدم، txtLoginName = اسم الدخول، txtFullName = الاسم الكامل،
     * txtPhone = الهاتف، txtEmail = البريد الإلكتروني، cmbRole = الدور، cmbBranch = الفرع،
     * cmbStatus = الحالة، txtPassword = كلمة المرور، txtConfirmPassword = تأكيد كلمة المرور،
     * txtNotes = الملاحظات، dgvUsers = جدول المستخدمين.
     * الأزرار: btnNew جديد، btnSave حفظ، btnEdit تعديل، btnDelete إيقاف،
     * btnRefresh تحديث، btnSearch بحث، btnPrint طباعة، btnClose إغلاق.
     * تبقى الأسماء البرمجية بالإنجليزية لضمان استمرار الربط مع API، والنصوص والتوضيح بالعربية.
     */

    /// <summary>
    /// التصميم المعتمد لشاشة إدارة المستخدمين داخل مساحة العمل الرئيسية.
    /// يمنع قص العناوين، ويستجيب لاختلاف دقة العرض، ويعطي الجدول المساحة الأكبر.
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

            Text = "إدارة المستخدمين";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            AutoScroll = false;
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = new Font("Segoe UI", 9F);
            BackColor = Color.FromArgb(244, 247, 251);

            // تجهيز شريط الأوامر وحقول المستخدم بالعربية قبل عرض الشاشة.
            ConfigureToolbar();
            ConfigureUserDataArea();
            ApplyArabicControlDescriptions();
            ConfigureFilters();
            ConfigureUsersGrid();
            ConfigureAudit();

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(8),
                Margin = Padding.Empty,
                BackColor = Color.FromArgb(244, 247, 251),
                RightToLeft = RightToLeft.Yes
            };
            // تخطيط مضغوط: البيانات واضحة، الجدول قصير، وتذييل التدقيق ثابت أسفل الشاشة.
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 274F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));

            Controls.Remove(pnlToolbar);
            Controls.Remove(pnlData);
            Controls.Remove(pnlGrid);
            Controls.Clear();

            root.Controls.Add(pnlToolbar, 0, 0);
            root.Controls.Add(pnlData, 0, 1);
            root.Controls.Add(_grpUserFilters, 0, 2);
            root.Controls.Add(pnlGrid, 0, 3);
            root.Controls.Add(_grpUserAudit, 0, 4);
            Controls.Add(root);

            ResumeLayout(true);
        }

        /// <summary>يضع وصفاً عربياً لكل أداة لسهولة فهم الكود ودعم قارئات الشاشة.</summary>
        private void ApplyArabicControlDescriptions()
        {
            Describe(txtUserName, "رقم المستخدم", "رقم السجل الداخلي للمستخدم.");
            Describe(txtLoginName, "اسم الدخول", "الاسم الذي يستخدمه الموظف لتسجيل الدخول.");
            Describe(txtFullName, "الاسم الكامل", "اسم المستخدم الظاهر في النظام.");
            Describe(txtPhone, "الهاتف", "رقم هاتف المستخدم.");
            Describe(txtEmail, "البريد الإلكتروني", "البريد الإلكتروني الرسمي للمستخدم.");
            Describe(cmbRole, "الدور", "الدور الذي يحدد صلاحيات المستخدم.");
            Describe(cmbBranch, "الفرع", "الفرع المسموح للمستخدم بالعمل فيه.");
            Describe(cmbStatus, "الحالة", "حالة الحساب: نشط أو موقوف.");
            Describe(txtPassword, "كلمة المرور", "كلمة مرور الحساب عند الإنشاء أو إعادة التعيين.");
            Describe(txtConfirmPassword, "تأكيد كلمة المرور", "إعادة إدخال كلمة المرور للتحقق منها.");
            Describe(txtNotes, "الملاحظات", "ملاحظات إدارية عن المستخدم.");
            Describe(dgvUsers, "جدول المستخدمين", "يعرض المستخدمين المسجلين وبياناتهم غير الحساسة.");
        }

        private static void Describe(Control control, string arabicName, string description)
        {
            control.AccessibleName = arabicName;
            control.AccessibleDescription = description;
        }

        private void ConfigureToolbar()
        {
            pnlToolbar.Controls.Clear();
            pnlToolbar.Dock = DockStyle.Fill;
            pnlToolbar.Padding = new Padding(8, 6, 8, 6);
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
            btnNew.Click += (_, _) => SetNewUserVisualState();

            // Dock.Right مع اتجاه صريح: أول زر مضاف يظهر في أقصى اليمين دائماً.
            var strip = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.RightToLeft,
                RightToLeft = RightToLeft.No,
                WrapContents = false,
                AutoScroll = false,
                Padding = new Padding(1),
                BackColor = Color.Transparent
            };

            ConfigureActionButton(btnNew, Color.FromArgb(37, 99, 235), Color.White, 70);
            ConfigureActionButton(btnSave, Color.FromArgb(22, 163, 74), Color.White, 70);
            ConfigureActionButton(btnEdit, Color.FromArgb(245, 158, 11), Color.White, 70);
            ConfigureActionButton(btnDelete, Color.FromArgb(220, 38, 38), Color.White, 70);
            ConfigureActionButton(_btnReactivate, Color.FromArgb(13, 148, 136), Color.White, 92);
            ConfigureActionButton(_btnResetPassword, Color.FromArgb(109, 40, 217), Color.White, 132);
            ConfigureActionButton(btnPrint, Color.White, Color.FromArgb(8, 49, 92), 70);
            ConfigureActionButton(btnSearch, Color.White, Color.FromArgb(8, 49, 92), 70);
            ConfigureActionButton(btnRefresh, Color.White, Color.FromArgb(8, 49, 92), 70);
            ConfigureActionButton(btnClose, Color.FromArgb(51, 65, 85), Color.White, 70);

            foreach (var button in new[]
                     {
                         btnNew, btnSave, btnEdit, btnDelete, _btnReactivate,
                         _btnResetPassword, btnPrint, btnSearch, btnRefresh, btnClose
                     })
                strip.Controls.Add(button);

            pnlToolbar.Controls.Add(strip);
        }

        private static void ConfigureActionButton(Button button, Color backColor, Color foreColor, int width)
        {
            button.Size = new Size(width, 38);
            button.Margin = new Padding(3);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = backColor == Color.White ? 1 : 0;
            button.FlatAppearance.BorderColor = Color.FromArgb(185, 200, 218);
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.Cursor = Cursors.Hand;
        }

        private void ConfigureUserDataArea()
        {
            pnlData.Dock = DockStyle.Fill;
            pnlData.Padding = new Padding(2);
            pnlData.BackColor = Color.Transparent;
            grpPermissions.Visible = false;
            pnlData.Controls.Clear();

            txtUserName.ReadOnly = true;
            txtUserName.BackColor = Color.FromArgb(241, 245, 249);
            txtUserName.Text = "(جديد)";
            txtPassword.UseSystemPasswordChar = true;
            txtConfirmPassword.UseSystemPasswordChar = true;
            txtNotes.Multiline = false;
            txtNotes.PlaceholderText = "ملاحظات إدارية أو سبب الإيقاف";
            txtNotes.ScrollBars = ScrollBars.None;

            chkIsActive.Text = "الحساب نشط";
            chkChangePassword.Text = "إجبار تغيير كلمة المرور عند أول دخول";
            foreach (var check in new[] { chkIsActive, chkChangePassword })
            {
                check.AutoSize = true;
                check.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                check.ForeColor = Color.FromArgb(8, 49, 92);
                check.Margin = new Padding(10, 2, 18, 2);
            }

            // جروب واحد فقط: صفوف متقابلة في اليمين واليسار، بلا تداخل أو قص.
            var userAndSecurity = CreateUsersSection("بيانات وأمان المستخدم");
            var fields = CreateUsersFieldsTable(7);
            AddUserField(fields, "رقم المستخدم", txtUserName, 0, 0);
            AddUserField(fields, "اسم الدخول", txtLoginName, 2, 0);
            AddUserField(fields, "الاسم الكامل", txtFullName, 0, 1);
            AddUserField(fields, "الجوال", txtPhone, 2, 1);
            AddUserField(fields, "الدور", cmbRole, 0, 2);
            AddUserField(fields, "الفرع", cmbBranch, 2, 2);
            AddUserField(fields, "كلمة المرور", txtPassword, 0, 3);
            AddUserField(fields, "إعادة كلمة المرور", txtConfirmPassword, 2, 3);
            AddUserField(fields, "البريد الإلكتروني", txtEmail, 0, 4);
            AddUserField(fields, "الحالة", cmbStatus, 2, 4);

            fields.Controls.Add(CreateInlineUserLabel("الملاحظات"), 0, 5);
            fields.Controls.Add(PrepareInlineUserInput(txtNotes), 1, 5);
            fields.SetColumnSpan(txtNotes, 3);

            var flags = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                RightToLeft = RightToLeft.No,
                WrapContents = false,
                Padding = new Padding(6, 1, 0, 0)
            };
            flags.Controls.Add(chkIsActive);
            flags.Controls.Add(chkChangePassword);
            fields.Controls.Add(flags, 0, 6);
            fields.SetColumnSpan(flags, 4);

            userAndSecurity.Controls.Add(fields);
            pnlData.Controls.Add(userAndSecurity);
        }

        private static GroupBox CreateUsersSection(string title) => new()
        {
            Dock = DockStyle.Fill,
            Text = title,
            RightToLeft = RightToLeft.Yes,
            ForeColor = Color.FromArgb(8, 49, 92),
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            BackColor = Color.White,
            Padding = new Padding(10, 21, 10, 6),
            Margin = new Padding(2)
        };

        private static TableLayoutPanel CreateUsersFieldsTable(int rows)
        {
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = rows,
                RightToLeft = RightToLeft.Yes,
                Padding = new Padding(4, 1, 4, 1)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            for (var row = 0; row < rows; row++)
                table.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / rows));
            return table;
        }

        private static void AddUserField(TableLayoutPanel table, string caption, Control input, int column, int row)
        {
            table.Controls.Add(CreateInlineUserLabel(caption), column, row);
            table.Controls.Add(PrepareInlineUserInput(input), column + 1, row);
        }

        private static Label CreateInlineUserLabel(string text) => new()
        {
            Text = text + ":",
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(55, 65, 81),
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleRight,
            Margin = new Padding(4, 0, 2, 0)
        };

        private static Control PrepareInlineUserInput(Control input)
        {
            input.Dock = DockStyle.Fill;
            input.Margin = new Padding(2, 3, 8, 3);
            input.Font = new Font("Segoe UI", 9F);
            if (input is ComboBox combo)
                combo.DropDownStyle = ComboBoxStyle.DropDownList;
            return input;
        }

        private static Control BuildVerticalField(string caption, Control control)
        {
            var cell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                RightToLeft = RightToLeft.Yes,
                Margin = new Padding(5, 2, 5, 2)
            };
            cell.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            cell.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            cell.Controls.Add(new Label
            {
                Text = caption + ":",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                AutoSize = false,
                AutoEllipsis = false
            }, 0, 0);

            control.Dock = DockStyle.Fill;
            control.Margin = new Padding(0, 1, 0, 1);
            control.Font = new Font("Segoe UI", 9F);
            if (control is ComboBox combo) combo.DropDownStyle = ComboBoxStyle.DropDownList;
            cell.Controls.Add(control, 0, 1);
            return cell;
        }

        private static Control BuildLabeledPanel(string caption, Control control)
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                RightToLeft = RightToLeft.Yes
            };
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            panel.Controls.Add(new Label
            {
                Text = caption + ":",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85)
            }, 0, 0);
            panel.Controls.Add(control, 0, 1);
            return panel;
        }

        private void ConfigureFilters()
        {
            _grpUserFilters.Controls.Clear();
            _grpUserFilters.Dock = DockStyle.Fill;
            _grpUserFilters.Text = "البحث والتصفية";
            _grpUserFilters.ForeColor = Color.FromArgb(8, 49, 92);
            _grpUserFilters.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _grpUserFilters.Padding = new Padding(8, 18, 8, 4);
            _grpUserFilters.BackColor = Color.White;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RightToLeft = RightToLeft.Yes,
                ColumnCount = 6,
                RowCount = 1,
                Padding = new Padding(2)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92F));

            layout.Controls.Add(BuildCompactFilter("بحث", _txtUserSearch), 0, 0);
            layout.Controls.Add(BuildCompactFilter("الفرع", _cmbFilterBranch), 1, 0);
            layout.Controls.Add(BuildCompactFilter("الدور", _cmbFilterRole), 2, 0);
            layout.Controls.Add(BuildCompactFilter("الحالة", _cmbFilterStatus), 3, 0);

            _btnClearFilters.Text = "مسح التصفية";
            ConfigureActionButton(_btnClearFilters, Color.White, Color.FromArgb(8, 49, 92), 104);
            _btnClearFilters.Dock = DockStyle.Fill;
            _btnClearFilters.Click += (_, _) => ClearUserFilters();
            layout.Controls.Add(_btnClearFilters, 4, 0);

            _lblResultCount.Dock = DockStyle.Fill;
            _lblResultCount.Text = "النتائج: 0";
            _lblResultCount.TextAlign = ContentAlignment.MiddleCenter;
            _lblResultCount.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            _lblResultCount.ForeColor = Color.FromArgb(51, 65, 85);
            layout.Controls.Add(_lblResultCount, 5, 0);

            _txtUserSearch.PlaceholderText = "بحث بالرقم أو الاسم أو اسم الدخول";
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
            cmbBranch.DataSourceChanged += (_, _) => RefreshBranchFilter();
            cmbRole.DataSourceChanged += (_, _) => RefreshRoleFilter();

            _grpUserFilters.Controls.Add(layout);
        }

        private static Control BuildCompactFilter(string caption, Control control)
        {
            // صف واحد؛ يمنع اختفاء حقل البحث عندما يكون ارتفاع المجموعة محدوداً.
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                RightToLeft = RightToLeft.Yes,
                Margin = new Padding(3)
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            panel.Controls.Add(new Label
            {
                Text = caption + ":",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85)
            }, 0, 0);
            control.Dock = DockStyle.Fill;
            control.Margin = new Padding(1);
            panel.Controls.Add(control, 1, 0);
            return panel;
        }

        private void ConfigureUsersGrid()
        {
            pnlGrid.Dock = DockStyle.Fill;
            pnlGrid.Padding = new Padding(2);
            pnlGrid.BackColor = Color.Transparent;
            // لا تمدد الشبكة على كامل المساحة الفارغة عندما تكون النتائج قليلة.
            dgvUsers.Dock = DockStyle.Top;
            dgvUsers.Height = 190;
            dgvUsers.MinimumSize = new Size(0, 190);
            dgvUsers.RightToLeft = RightToLeft.Yes;
            dgvUsers.BackgroundColor = Color.White;
            dgvUsers.BorderStyle = BorderStyle.FixedSingle;
            dgvUsers.EnableHeadersVisualStyles = false;
            dgvUsers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(8, 49, 92);
            dgvUsers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvUsers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            dgvUsers.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvUsers.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvUsers.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvUsers.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvUsers.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgvUsers.GridColor = Color.FromArgb(220, 228, 238);
            dgvUsers.ColumnHeadersHeight = 38;
            dgvUsers.RowTemplate.Height = 30;
            dgvUsers.MultiSelect = false;
            dgvUsers.AllowUserToResizeRows = false;
            dgvUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvUsers.RowsAdded += (_, _) => UpdateResultCount();
            dgvUsers.RowsRemoved += (_, _) => UpdateResultCount();
            dgvUsers.SelectionChanged += (_, _) => UpdateSelectionVisualState();
        }

        private void ConfigureAudit()
        {
            _grpUserAudit.Controls.Clear();
            _grpUserAudit.Dock = DockStyle.Fill;
            _grpUserAudit.Text = "بيانات الإنشاء والتعديل";
            _grpUserAudit.ForeColor = Color.FromArgb(8, 49, 92);
            _grpUserAudit.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            _grpUserAudit.BackColor = Color.White;
            _grpUserAudit.Padding = new Padding(8, 18, 8, 4);

            // صفّان قصيران بدلاً من سبعة أعمدة ضيقة، حتى تبقى بيانات التدقيق مقروءة.
            var audit = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 2,
                RightToLeft = RightToLeft.Yes
            };
            for (var i = 0; i < 4; i++)
                audit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            audit.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            audit.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            _lblSelectionState.Dock = DockStyle.Fill;
            _lblSelectionState.Text = "الوضع: مستخدم جديد";
            _lblSelectionState.TextAlign = ContentAlignment.MiddleRight;
            _lblSelectionState.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            _lblSelectionState.ForeColor = Color.FromArgb(37, 99, 235);
            audit.Controls.Add(_lblSelectionState, 0, 0);
            audit.Controls.Add(CreateAuditLabel("أنشئ بواسطة: —"), 1, 0);
            audit.Controls.Add(CreateAuditLabel("تاريخ الإنشاء: —"), 2, 0);
            audit.Controls.Add(CreateAuditLabel("عدّل بواسطة: —"), 3, 0);
            audit.Controls.Add(CreateAuditLabel("تاريخ التعديل: —"), 0, 1);
            audit.Controls.Add(CreateAuditLabel("عدد التعديلات: —"), 1, 1);
            audit.Controls.Add(CreateAuditLabel("عدد الطباعة: —"), 2, 1);
            audit.Controls.Add(CreateAuditLabel("آخر طباعة: —"), 3, 1);
            _grpUserAudit.Controls.Add(audit);
        }

        private static Label CreateAuditLabel(string text) => new()
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 8F),
            ForeColor = Color.FromArgb(71, 85, 105)
        };

        private void RefreshBranchFilter()
        {
            if (cmbBranch.DataSource is not IEnumerable<BranchLookupModel> branches) return;
            var choices = branches.ToList();
            choices.Insert(0, new BranchLookupModel { Branch_ID = 0, Branch_Name = "الكل" });
            _cmbFilterBranch.DataSource = choices;
            _cmbFilterBranch.DisplayMember = "Branch_Name";
            _cmbFilterBranch.ValueMember = "Branch_ID";
            _cmbFilterBranch.SelectedIndex = 0;
        }

        private void RefreshRoleFilter()
        {
            if (cmbRole.DataSource is not IEnumerable<RoleLookupModel> roles) return;
            var choices = roles.ToList();
            choices.Insert(0, new RoleLookupModel { Role_ID = 0, Role_Name = "الكل" });
            _cmbFilterRole.DataSource = choices;
            _cmbFilterRole.DisplayMember = "Role_Name";
            _cmbFilterRole.ValueMember = "Role_ID";
            _cmbFilterRole.SelectedIndex = 0;
        }

        private void InitializeApprovedUserFilters()
        {
            RefreshBranchFilter();
            RefreshRoleFilter();
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

        private void SetNewUserVisualState()
        {
            txtUserName.Text = "(جديد)";
            _lblSelectionState.Text = "الوضع: مستخدم جديد";
            _lblSelectionState.ForeColor = Color.FromArgb(37, 99, 235);
            btnDelete.Enabled = false;
            _btnReactivate.Enabled = false;
        }

        private void UpdateSelectionVisualState()
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                SetNewUserVisualState();
                return;
            }

            var row = dgvUsers.SelectedRows[0];
            var id = row.Cells["User_ID"].Value?.ToString() ?? string.Empty;
            var name = row.Cells["Full_Name"].Value?.ToString() ?? "مستخدم";
            var active = string.Equals(row.Cells["Is_Active"].Value?.ToString(), "نشط", StringComparison.OrdinalIgnoreCase);
            txtUserName.Text = id;
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
                using var titleFont = new Font("Segoe UI", 14F, FontStyle.Bold);
                using var headerFont = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                using var rowFont = new Font("Segoe UI", 8F);
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
