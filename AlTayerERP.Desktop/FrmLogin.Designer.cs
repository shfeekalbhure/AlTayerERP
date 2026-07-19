namespace AlTayerERP.Desktop
{
    partial class FrmLogin
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pnlHeader = new Panel();
            lblSystemSubtitle = new Label();
            lblSystemTitle = new Label();
            picSystemLogo = new PictureBox();
            pnlStatusBar = new Panel();
            lblDateTime = new Label();
            lblVersion = new Label();
            lblLicenseStatus = new Label();
            lblApiStatus = new Label();
            lblDatabaseStatus = new Label();
            pnlCompanyInfo = new Panel();
            lblCompanyEmail = new Label();
            lblCompanyPhone = new Label();
            lblCompanyAddress = new Label();
            lblCompanyName = new Label();
            lblCompanyTitle = new Label();
            picCompanyLogo = new PictureBox();
            pnlLogin = new Panel();
            grpLogin = new GroupBox();
            btnExit = new Button();
            btnAboutSystem = new Button();
            btnConnectionSettings = new Button();
            btnLogin = new Button();
            chkRememberMe = new CheckBox();
            txtPassword = new TextBox();
            cmbUsername = new ComboBox();
            lblPassword = new Label();
            lblUsername = new Label();
            cmbFiscalYear = new ComboBox();
            lblFiscalYear = new Label();
            cmbBranch = new ComboBox();
            lblBranch = new Label();
            cmbCompany = new ComboBox();
            lblCompany = new Label();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picSystemLogo).BeginInit();
            pnlStatusBar.SuspendLayout();
            pnlCompanyInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picCompanyLogo).BeginInit();
            pnlLogin.SuspendLayout();
            grpLogin.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.WhiteSmoke;
            pnlHeader.Controls.Add(lblSystemSubtitle);
            pnlHeader.Controls.Add(lblSystemTitle);
            pnlHeader.Controls.Add(picSystemLogo);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(900, 70);
            pnlHeader.TabIndex = 0;
            // 
            // lblSystemSubtitle
            // 
            lblSystemSubtitle.AutoSize = true;
            lblSystemSubtitle.Font = new Font("Microsoft Sans Serif", 8.25F);
            lblSystemSubtitle.Location = new Point(494, 26);
            lblSystemSubtitle.Name = "lblSystemSubtitle";
            lblSystemSubtitle.Size = new Size(143, 17);
            lblSystemSubtitle.TabIndex = 2;
            lblSystemSubtitle.Text = " AlTayer ERP System";
            // 
            // lblSystemTitle
            // 
            lblSystemTitle.AutoSize = true;
            lblSystemTitle.Font = new Font("Microsoft Sans Serif", 8.25F);
            lblSystemTitle.Location = new Point(485, 9);
            lblSystemTitle.Name = "lblSystemTitle";
            lblSystemTitle.Size = new Size(152, 17);
            lblSystemTitle.TabIndex = 1;
            lblSystemTitle.Text = "نظام الطائر لإدارة النقل والشحن";
            // 
            // picSystemLogo
            // 
            picSystemLogo.Location = new Point(82, 0);
            picSystemLogo.Name = "picSystemLogo";
            picSystemLogo.Size = new Size(64, 64);
            picSystemLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picSystemLogo.TabIndex = 0;
            picSystemLogo.TabStop = false;
            // 
            // pnlStatusBar
            // 
            pnlStatusBar.Controls.Add(lblDateTime);
            pnlStatusBar.Controls.Add(lblVersion);
            pnlStatusBar.Controls.Add(lblLicenseStatus);
            pnlStatusBar.Controls.Add(lblApiStatus);
            pnlStatusBar.Controls.Add(lblDatabaseStatus);
            pnlStatusBar.Dock = DockStyle.Bottom;
            pnlStatusBar.Location = new Point(0, 483);
            pnlStatusBar.Name = "pnlStatusBar";
            pnlStatusBar.Size = new Size(900, 35);
            pnlStatusBar.TabIndex = 2;
            // 
            // lblDateTime
            // 
            lblDateTime.AutoSize = true;
            lblDateTime.Location = new Point(22, 3);
            lblDateTime.Name = "lblDateTime";
            lblDateTime.Size = new Size(112, 23);
            lblDateTime.TabIndex = 4;
            lblDateTime.Text = "التاريخ والوقت";
            // 
            // lblVersion
            // 
            lblVersion.AutoSize = true;
            lblVersion.Location = new Point(158, 3);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(96, 23);
            lblVersion.TabIndex = 3;
            lblVersion.Text = "الإصدار: 1.0.0\t";
            // 
            // lblLicenseStatus
            // 
            lblLicenseStatus.AutoSize = true;
            lblLicenseStatus.Location = new Point(283, 3);
            lblLicenseStatus.Name = "lblLicenseStatus";
            lblLicenseStatus.Size = new Size(170, 23);
            lblLicenseStatus.TabIndex = 2;
            lblLicenseStatus.Text = "الترخيص: غير مفحوص";
            // 
            // lblApiStatus
            // 
            lblApiStatus.AutoSize = true;
            lblApiStatus.Location = new Point(462, 3);
            lblApiStatus.Name = "lblApiStatus";
            lblApiStatus.Size = new Size(129, 23);
            lblApiStatus.TabIndex = 1;
            lblApiStatus.Text = "غير مفحوص:API";
            // 
            // lblDatabaseStatus
            // 
            lblDatabaseStatus.AutoSize = true;
            lblDatabaseStatus.Location = new Point(658, 3);
            lblDatabaseStatus.Name = "lblDatabaseStatus";
            lblDatabaseStatus.Size = new Size(206, 23);
            lblDatabaseStatus.TabIndex = 0;
            lblDatabaseStatus.Text = "قاعدة البيانات: غير مفحوص";
            // 
            // pnlCompanyInfo
            // 
            pnlCompanyInfo.Controls.Add(lblCompanyEmail);
            pnlCompanyInfo.Controls.Add(lblCompanyPhone);
            pnlCompanyInfo.Controls.Add(lblCompanyAddress);
            pnlCompanyInfo.Controls.Add(lblCompanyName);
            pnlCompanyInfo.Controls.Add(lblCompanyTitle);
            pnlCompanyInfo.Controls.Add(picCompanyLogo);
            pnlCompanyInfo.Dock = DockStyle.Left;
            pnlCompanyInfo.Location = new Point(0, 70);
            pnlCompanyInfo.Name = "pnlCompanyInfo";
            pnlCompanyInfo.Size = new Size(330, 413);
            pnlCompanyInfo.TabIndex = 0;
            // 
            // lblCompanyEmail
            // 
            lblCompanyEmail.AutoSize = true;
            lblCompanyEmail.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblCompanyEmail.Location = new Point(198, 360);
            lblCompanyEmail.Name = "lblCompanyEmail";
            lblCompanyEmail.Size = new Size(119, 25);
            lblCompanyEmail.TabIndex = 3;
            lblCompanyEmail.Text = "البريد الإلكتروني";
            // 
            // lblCompanyPhone
            // 
            lblCompanyPhone.AutoSize = true;
            lblCompanyPhone.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblCompanyPhone.Location = new Point(262, 324);
            lblCompanyPhone.Name = "lblCompanyPhone";
            lblCompanyPhone.Size = new Size(54, 25);
            lblCompanyPhone.TabIndex = 3;
            lblCompanyPhone.Text = "الهاتف";
            // 
            // lblCompanyAddress
            // 
            lblCompanyAddress.AutoSize = true;
            lblCompanyAddress.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblCompanyAddress.Location = new Point(197, 280);
            lblCompanyAddress.Name = "lblCompanyAddress";
            lblCompanyAddress.Size = new Size(115, 25);
            lblCompanyAddress.TabIndex = 3;
            lblCompanyAddress.Text = "عدن- المنصوره";
            // 
            // lblCompanyName
            // 
            lblCompanyName.AutoSize = true;
            lblCompanyName.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblCompanyName.Location = new Point(94, 237);
            lblCompanyName.Name = "lblCompanyName";
            lblCompanyName.Size = new Size(223, 25);
            lblCompanyName.TabIndex = 2;
            lblCompanyName.Text = "مكتب الطائر السعيد للنقل والشحن";
            // 
            // lblCompanyTitle
            // 
            lblCompanyTitle.AutoSize = true;
            lblCompanyTitle.Font = new Font("Microsoft Sans Serif", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblCompanyTitle.Location = new Point(215, 206);
            lblCompanyTitle.Name = "lblCompanyTitle";
            lblCompanyTitle.Size = new Size(90, 22);
            lblCompanyTitle.TabIndex = 1;
            lblCompanyTitle.Text = "بيانات الشركه";
            // 
            // picCompanyLogo
            // 
            picCompanyLogo.BorderStyle = BorderStyle.FixedSingle;
            picCompanyLogo.Location = new Point(94, 19);
            picCompanyLogo.Name = "picCompanyLogo";
            picCompanyLogo.Size = new Size(180, 180);
            picCompanyLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picCompanyLogo.TabIndex = 0;
            picCompanyLogo.TabStop = false;
            // 
            // pnlLogin
            // 
            pnlLogin.Controls.Add(grpLogin);
            pnlLogin.Dock = DockStyle.Fill;
            pnlLogin.Location = new Point(330, 70);
            pnlLogin.Name = "pnlLogin";
            pnlLogin.Size = new Size(570, 413);
            pnlLogin.TabIndex = 3;
            // 
            // grpLogin
            // 
            grpLogin.Controls.Add(btnExit);
            grpLogin.Controls.Add(btnAboutSystem);
            grpLogin.Controls.Add(btnConnectionSettings);
            grpLogin.Controls.Add(btnLogin);
            grpLogin.Controls.Add(chkRememberMe);
            grpLogin.Controls.Add(txtPassword);
            grpLogin.Controls.Add(cmbUsername);
            grpLogin.Controls.Add(lblPassword);
            grpLogin.Controls.Add(lblUsername);
            grpLogin.Controls.Add(cmbFiscalYear);
            grpLogin.Controls.Add(lblFiscalYear);
            grpLogin.Controls.Add(cmbBranch);
            grpLogin.Controls.Add(lblBranch);
            grpLogin.Controls.Add(cmbCompany);
            grpLogin.Controls.Add(lblCompany);
            grpLogin.Dock = DockStyle.Fill;
            grpLogin.Location = new Point(0, 0);
            grpLogin.Name = "grpLogin";
            grpLogin.Size = new Size(570, 413);
            grpLogin.TabIndex = 0;
            grpLogin.TabStop = false;
            grpLogin.Text = "بيانات الدخول";
            // 
            // btnExit
            // 
            btnExit.Font = new Font("Microsoft Sans Serif", 8.25F);
            btnExit.Location = new Point(387, 340);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(180, 40);
            btnExit.TabIndex = 5;
            btnExit.Text = "خروج Exit";
            btnExit.UseVisualStyleBackColor = true;
            // 
            // btnAboutSystem
            // 
            btnAboutSystem.Font = new Font("Microsoft Sans Serif", 8.25F);
            btnAboutSystem.Location = new Point(9, 340);
            btnAboutSystem.Name = "btnAboutSystem";
            btnAboutSystem.Size = new Size(180, 40);
            btnAboutSystem.TabIndex = 4;
            btnAboutSystem.Text = "حول النظام";
            btnAboutSystem.UseVisualStyleBackColor = true;
            // 
            // btnConnectionSettings
            // 
            btnConnectionSettings.Font = new Font("Microsoft Sans Serif", 8.25F);
            btnConnectionSettings.Location = new Point(197, 340);
            btnConnectionSettings.Name = "btnConnectionSettings";
            btnConnectionSettings.Size = new Size(180, 40);
            btnConnectionSettings.TabIndex = 4;
            btnConnectionSettings.Text = "إعدادات الإتصال";
            btnConnectionSettings.UseVisualStyleBackColor = true;
            // 
            // btnLogin
            // 
            btnLogin.Font = new Font("Microsoft Sans Serif", 8.25F);
            btnLogin.Location = new Point(197, 277);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(180, 45);
            btnLogin.TabIndex = 0;
            btnLogin.Text = "تسجيل الدخول";
            btnLogin.UseVisualStyleBackColor = true;
            // 
            // chkRememberMe
            // 
            chkRememberMe.AutoSize = true;
            chkRememberMe.Location = new Point(430, 259);
            chkRememberMe.Name = "chkRememberMe";
            chkRememberMe.Size = new Size(82, 27);
            chkRememberMe.TabIndex = 3;
            chkRememberMe.Text = "تذكرني";
            chkRememberMe.UseVisualStyleBackColor = true;
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(155, 206);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '*';
            txtPassword.Size = new Size(294, 30);
            txtPassword.TabIndex = 2;
            // 
            // cmbUsername
            // 
            cmbUsername.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbUsername.FormattingEnabled = true;
            cmbUsername.Location = new Point(155, 160);
            cmbUsername.Name = "cmbUsername";
            cmbUsername.Size = new Size(294, 31);
            cmbUsername.TabIndex = 1;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(455, 206);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(91, 23);
            lblPassword.TabIndex = 0;
            lblPassword.Text = "كلمة المرور";
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Location = new Point(455, 165);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(79, 23);
            lblUsername.TabIndex = 0;
            lblUsername.Text = "المستخدم";
            // 
            // cmbFiscalYear
            // 
            cmbFiscalYear.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFiscalYear.FormattingEnabled = true;
            cmbFiscalYear.Location = new Point(155, 123);
            cmbFiscalYear.Name = "cmbFiscalYear";
            cmbFiscalYear.Size = new Size(294, 31);
            cmbFiscalYear.TabIndex = 1;
            // 
            // lblFiscalYear
            // 
            lblFiscalYear.AutoSize = true;
            lblFiscalYear.Location = new Point(455, 128);
            lblFiscalYear.Name = "lblFiscalYear";
            lblFiscalYear.Size = new Size(99, 23);
            lblFiscalYear.TabIndex = 0;
            lblFiscalYear.Text = "السنة المالية";
            // 
            // cmbBranch
            // 
            cmbBranch.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBranch.FormattingEnabled = true;
            cmbBranch.Location = new Point(155, 86);
            cmbBranch.Name = "cmbBranch";
            cmbBranch.Size = new Size(294, 31);
            cmbBranch.TabIndex = 1;
            // 
            // lblBranch
            // 
            lblBranch.AutoSize = true;
            lblBranch.Location = new Point(455, 91);
            lblBranch.Name = "lblBranch";
            lblBranch.Size = new Size(46, 23);
            lblBranch.TabIndex = 0;
            lblBranch.Text = "الفرع";
            // 
            // cmbCompany
            // 
            cmbCompany.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCompany.FormattingEnabled = true;
            cmbCompany.Location = new Point(155, 49);
            cmbCompany.Name = "cmbCompany";
            cmbCompany.Size = new Size(294, 31);
            cmbCompany.TabIndex = 1;
            cmbCompany.SelectedIndexChanged += cmbCompany_SelectedIndexChanged;
            // 
            // lblCompany
            // 
            lblCompany.AutoSize = true;
            lblCompany.Location = new Point(455, 54);
            lblCompany.Name = "lblCompany";
            lblCompany.Size = new Size(57, 23);
            lblCompany.TabIndex = 0;
            lblCompany.Text = "الشركة";
            // 
            // FrmLogin
            // 
            AutoScaleDimensions = new SizeF(9F, 23F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(900, 518);
            Controls.Add(pnlLogin);
            Controls.Add(pnlCompanyInfo);
            Controls.Add(pnlStatusBar);
            Controls.Add(pnlHeader);
            Font = new Font("Segoe UI", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmLogin";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "تسجيل الدخول";
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picSystemLogo).EndInit();
            pnlStatusBar.ResumeLayout(false);
            pnlStatusBar.PerformLayout();
            pnlCompanyInfo.ResumeLayout(false);
            pnlCompanyInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picCompanyLogo).EndInit();
            pnlLogin.ResumeLayout(false);
            grpLogin.ResumeLayout(false);
            grpLogin.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlHeader;
        private Panel pnlStatusBar;
        private Panel pnlCompanyInfo;
        private Panel pnlLogin;
        private PictureBox picSystemLogo;
        private Label lblSystemTitle;
        private Label lblSystemSubtitle;
        private Label lblCompanyTitle;
        private PictureBox picCompanyLogo;
        private Label lblCompanyPhone;
        private Label lblCompanyAddress;
        private Label lblCompanyName;
        private Label lblCompanyEmail;
        private GroupBox grpLogin;
        private Label lblCompany;
        private TextBox txtPassword;
        private ComboBox cmbCompany;
        private ComboBox cmbUsername;
        private Label lblPassword;
        private Label lblUsername;
        private ComboBox cmbFiscalYear;
        private Label lblFiscalYear;
        private ComboBox cmbBranch;
        private Label lblBranch;
        private CheckBox chkRememberMe;
        private Button btnLogin;
        private Button btnExit;
        private Button btnConnectionSettings;
        private Button btnAboutSystem;
        private Label lblDateTime;
        private Label lblVersion;
        private Label lblLicenseStatus;
        private Label lblApiStatus;
        private Label lblDatabaseStatus;

    }
}