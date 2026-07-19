namespace AlTayerERP.Desktop
{
    partial class FrmMain
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
            pnlTopBar = new Panel();
            btnLogout = new Button();
            btnAboutSystem = new Button();
            btnSettings = new Button();
            btnNotifications = new Button();
            picCompanyLogo = new PictureBox();
            lblCurrentUser = new Label();
            lblCompanyName = new Label();
            lblFiscalYear = new Label();
            lblCurrentBranch = new Label();
            pnlStatusBar = new Panel();
            lblStatusTime = new Label();
            lblStatusLicense = new Label();
            lblStatusApi = new Label();
            lblVersion = new Label();
            lblStatusDatabase = new Label();
            pnlSideMenu = new Panel();
            tvMainMenu = new TreeView();
            pnlWorkspace = new Panel();
            pnlTopBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picCompanyLogo).BeginInit();
            pnlStatusBar.SuspendLayout();
            pnlSideMenu.SuspendLayout();
            SuspendLayout();
            // 
            // pnlTopBar
            // 
            pnlTopBar.BackColor = Color.White;
            pnlTopBar.Controls.Add(btnLogout);
            pnlTopBar.Controls.Add(btnAboutSystem);
            pnlTopBar.Controls.Add(btnSettings);
            pnlTopBar.Controls.Add(btnNotifications);
            pnlTopBar.Controls.Add(picCompanyLogo);
            pnlTopBar.Controls.Add(lblCurrentUser);
            pnlTopBar.Controls.Add(lblCompanyName);
            pnlTopBar.Controls.Add(lblFiscalYear);
            pnlTopBar.Controls.Add(lblCurrentBranch);
            pnlTopBar.Dock = DockStyle.Top;
            pnlTopBar.Location = new Point(0, 0);
            pnlTopBar.Name = "pnlTopBar";
            pnlTopBar.Size = new Size(1136, 75);
            pnlTopBar.TabIndex = 0;
            // 
            // btnLogout
            // 
            btnLogout.Location = new Point(11, 23);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(141, 30);
            btnLogout.TabIndex = 4;
            btnLogout.Text = "تسجيل الخروج";
            btnLogout.UseVisualStyleBackColor = true;
            // 
            // btnAboutSystem
            // 
            btnAboutSystem.Location = new Point(158, 0);
            btnAboutSystem.Name = "btnAboutSystem";
            btnAboutSystem.Size = new Size(94, 54);
            btnAboutSystem.TabIndex = 3;
            btnAboutSystem.Text = "حول\r\nالنظام";
            btnAboutSystem.UseVisualStyleBackColor = true;
            // 
            // btnSettings
            // 
            btnSettings.Location = new Point(257, 19);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(94, 30);
            btnSettings.TabIndex = 3;
            btnSettings.Text = "الاعدادات";
            btnSettings.UseVisualStyleBackColor = true;
            // 
            // btnNotifications
            // 
            btnNotifications.Location = new Point(355, 19);
            btnNotifications.Name = "btnNotifications";
            btnNotifications.Size = new Size(94, 30);
            btnNotifications.TabIndex = 2;
            btnNotifications.Text = "الاشعارات";
            btnNotifications.UseVisualStyleBackColor = true;
            // 
            // picCompanyLogo
            // 
            picCompanyLogo.Location = new Point(999, 6);
            picCompanyLogo.Name = "picCompanyLogo";
            picCompanyLogo.Size = new Size(125, 62);
            picCompanyLogo.TabIndex = 1;
            picCompanyLogo.TabStop = false;
            // 
            // lblCurrentUser
            // 
            lblCurrentUser.AutoSize = true;
            lblCurrentUser.Location = new Point(462, 8);
            lblCurrentUser.Name = "lblCurrentUser";
            lblCurrentUser.Size = new Size(96, 46);
            lblCurrentUser.TabIndex = 0;
            lblCurrentUser.Text = "المستخدم:\r\n مدير النظام";
            // 
            // lblCompanyName
            // 
            lblCompanyName.AutoSize = true;
            lblCompanyName.Location = new Point(844, 4);
            lblCompanyName.Name = "lblCompanyName";
            lblCompanyName.Size = new Size(139, 46);
            lblCompanyName.TabIndex = 0;
            lblCompanyName.Text = "شركة\r\n الطائر للنقل البري";
            // 
            // lblFiscalYear
            // 
            lblFiscalYear.AutoSize = true;
            lblFiscalYear.Location = new Point(566, 8);
            lblFiscalYear.Name = "lblFiscalYear";
            lblFiscalYear.Size = new Size(108, 46);
            lblFiscalYear.TabIndex = 0;
            lblFiscalYear.Text = "السنة المالية :\r\n 2026";
            // 
            // lblCurrentBranch
            // 
            lblCurrentBranch.AutoSize = true;
            lblCurrentBranch.Location = new Point(696, 3);
            lblCurrentBranch.Name = "lblCurrentBranch";
            lblCurrentBranch.Size = new Size(114, 46);
            lblCurrentBranch.TabIndex = 0;
            lblCurrentBranch.Text = "الفرع \r\nعدن_المنصورة";
            // 
            // pnlStatusBar
            // 
            pnlStatusBar.BackColor = Color.White;
            pnlStatusBar.Controls.Add(lblStatusTime);
            pnlStatusBar.Controls.Add(lblStatusLicense);
            pnlStatusBar.Controls.Add(lblStatusApi);
            pnlStatusBar.Controls.Add(lblVersion);
            pnlStatusBar.Controls.Add(lblStatusDatabase);
            pnlStatusBar.Dock = DockStyle.Bottom;
            pnlStatusBar.Location = new Point(0, 756);
            pnlStatusBar.Name = "pnlStatusBar";
            pnlStatusBar.Size = new Size(1136, 35);
            pnlStatusBar.TabIndex = 1;
            // 
            // lblStatusTime
            // 
            lblStatusTime.AutoSize = true;
            lblStatusTime.Location = new Point(82, 7);
            lblStatusTime.Name = "lblStatusTime";
            lblStatusTime.Size = new Size(112, 23);
            lblStatusTime.TabIndex = 6;
            lblStatusTime.Text = "الوقت والتاريخ\r\n";
            lblStatusTime.Click += lblStatusTime_Click;
            // 
            // lblStatusLicense
            // 
            lblStatusLicense.AutoSize = true;
            lblStatusLicense.Location = new Point(285, 3);
            lblStatusLicense.Name = "lblStatusLicense";
            lblStatusLicense.Size = new Size(115, 23);
            lblStatusLicense.TabIndex = 5;
            lblStatusLicense.Text = "الترخيص: فعال";
            // 
            // lblStatusApi
            // 
            lblStatusApi.AutoSize = true;
            lblStatusApi.Location = new Point(482, 7);
            lblStatusApi.Name = "lblStatusApi";
            lblStatusApi.Size = new Size(134, 23);
            lblStatusApi.TabIndex = 4;
            lblStatusApi.Text = "API: غير مفحوص";
            // 
            // lblVersion
            // 
            lblVersion.AutoSize = true;
            lblVersion.Location = new Point(876, 3);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(61, 23);
            lblVersion.TabIndex = 2;
            lblVersion.Text = "الإصدار\t";
            // 
            // lblStatusDatabase
            // 
            lblStatusDatabase.AutoSize = true;
            lblStatusDatabase.Location = new Point(633, 6);
            lblStatusDatabase.Name = "lblStatusDatabase";
            lblStatusDatabase.Size = new Size(206, 23);
            lblStatusDatabase.TabIndex = 3;
            lblStatusDatabase.Text = "قاعدة البيانات: غير مفحوص";
            lblStatusDatabase.Click += label4_Click;
            // 
            // pnlSideMenu
            // 
            pnlSideMenu.BackColor = Color.Navy;
            pnlSideMenu.Controls.Add(tvMainMenu);
            pnlSideMenu.Dock = DockStyle.Right;
            pnlSideMenu.Location = new Point(876, 75);
            pnlSideMenu.Name = "pnlSideMenu";
            pnlSideMenu.Size = new Size(260, 681);
            pnlSideMenu.TabIndex = 2;
            // 
            // tvMainMenu
            // 
            tvMainMenu.BorderStyle = BorderStyle.None;
            tvMainMenu.Dock = DockStyle.Fill;
            tvMainMenu.FullRowSelect = true;
            tvMainMenu.HideSelection = false;
            tvMainMenu.Location = new Point(0, 0);
            tvMainMenu.Name = "tvMainMenu";
            tvMainMenu.Size = new Size(260, 681);
            tvMainMenu.TabIndex = 0;
            tvMainMenu.AfterSelect += tvMainMenu_AfterSelect_1;
            // 
            // pnlWorkspace
            // 
            pnlWorkspace.Dock = DockStyle.Fill;
            pnlWorkspace.Location = new Point(0, 75);
            pnlWorkspace.Name = "pnlWorkspace";
            pnlWorkspace.Size = new Size(876, 681);
            pnlWorkspace.TabIndex = 3;
            pnlWorkspace.Paint += pnlWorkspace_Paint;
            // 
            // FrmMain
            // 
            AutoScaleDimensions = new SizeF(9F, 23F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.WhiteSmoke;
            ClientSize = new Size(1136, 791);
            Controls.Add(pnlWorkspace);
            Controls.Add(pnlSideMenu);
            Controls.Add(pnlStatusBar);
            Controls.Add(pnlTopBar);
            Font = new Font("Segoe UI", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Name = "FrmMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "AlTayerERP - الشاشة الرئيسية";
            WindowState = FormWindowState.Maximized;
            pnlTopBar.ResumeLayout(false);
            pnlTopBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picCompanyLogo).EndInit();
            pnlStatusBar.ResumeLayout(false);
            pnlStatusBar.PerformLayout();
            pnlSideMenu.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlTopBar;
        private Panel pnlStatusBar;
        private Panel pnlSideMenu;
        private Panel pnlWorkspace;
        private Button btnLogout;
        private Button btnSettings;
        private Button btnNotifications;
        private PictureBox picCompanyLogo;
        private Label lblCurrentUser;
        private Label lblCompanyName;
        private Label lblFiscalYear;
        private Label lblCurrentBranch;
        private Button btnHome;
        private Button btnUsers;
        private Button btnSettingsCenter;
        private Button btnReportsCenter;
        private Button btnDriversCenter;
        private Button btnCustomersCenter;
        private Button btnWarehouseCenter;
        private Button btnShippingCenter;
        private Button btnTicketsCenter;
        private Button btnAccountingCenter;
        private Panel pnlAccountingMenu;
        private Button btnChartOfAccounts;
        private Panel pnlTicketsMenu;
        private Button button1;
        private Panel pnlShippingMenu;
        private Button button2;
        private Panel pnlDriversMenu;
        private Button button3;
        private Panel panel4;
        private Button button4;
        private Panel pnlWarehouseMenu;
        private Button button5;
        private Panel panel6;
        private Button button6;
        private Panel pnlCustomersMenu;
        private Button button7;
        private Button btnAboutSystem;
        private Label lblVersion;
        private Label lblStatusDatabase;
        private Label lblStatusTime;
        private Label lblStatusLicense;
        private Label lblStatusApi;
        private TreeView tvMainMenu;
    }
}