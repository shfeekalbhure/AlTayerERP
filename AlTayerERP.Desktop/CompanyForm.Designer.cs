namespace AlTayerERP.Desktop
{
    partial class CompanyForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            pnlHeader = new Panel();
            lblScreenTitle = new Label();
            lblScreenSubTitle = new Label();
            pnlToolbar = new FlowLayoutPanel();
            btnNew = new Button();
            btnSaveCompany = new Button();
            btnEdit = new Button();
            btnDelete = new Button();
            btnApprove = new Button();
            btnUnApprove = new Button();
            btnSearch = new Button();
            btnRefresh = new Button();
            btnPreview = new Button();
            btnPrint = new Button();
            btnExport = new Button();
            btnImport = new Button();
            btnClose = new Button();
            splitMain = new SplitContainer();
            pnlListHeader = new Panel();
            lblListTitle = new Label();
            dgvCompanies = new DataGridView();
            pnlDetails = new Panel();
            grpBasic = new GroupBox();
            tblBasic = new TableLayoutPanel();
            label1 = new Label();
            cmbGroups = new ComboBox();
            label2 = new Label();
            txtCompanyNameAr = new TextBox();
            label3 = new Label();
            txtCompanyNameEn = new TextBox();
            label4 = new Label();
            txtCompanyPrefix = new TextBox();
            label8 = new Label();
            txtTaxNumber = new TextBox();
            chkIsActive = new CheckBox();
            grpContact = new GroupBox();
            tblContact = new TableLayoutPanel();
            label5 = new Label();
            txtPhone = new TextBox();
            label6 = new Label();
            txtEmail = new TextBox();
            label7 = new Label();
            txtAddress = new TextBox();
            label10 = new Label();
            txtMobile = new TextBox();
            label11 = new Label();
            txtActivityType = new TextBox();
            grpLogo = new GroupBox();
            picCompanyLogo = new PictureBox();
            label9 = new Label();
            pnlLogoButtons = new FlowLayoutPanel();
            btnBrowseLogo = new Button();
            btnRemoveLogo = new Button();
            pnlAudit = new Panel();
            lblCreatedByCaption = new Label();
            lblCreatedBy = new Label();
            lblCreatedAtCaption = new Label();
            lblCreatedAt = new Label();
            lblModifiedByCaption = new Label();
            lblModifiedBy = new Label();
            lblModifiedAtCaption = new Label();
            lblModifiedAt = new Label();
            lblEditCountCaption = new Label();
            lblEditCount = new Label();
            lblPrintCountCaption = new Label();
            lblPrintCount = new Label();
            pnlHeader.SuspendLayout();
            pnlToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            pnlListHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCompanies).BeginInit();
            pnlDetails.SuspendLayout();
            grpBasic.SuspendLayout();
            tblBasic.SuspendLayout();
            grpContact.SuspendLayout();
            tblContact.SuspendLayout();
            grpLogo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picCompanyLogo).BeginInit();
            pnlLogoButtons.SuspendLayout();
            pnlAudit.SuspendLayout();
            SuspendLayout();
            //
            // pnlHeader
            //
            pnlHeader.BackColor = Color.FromArgb(31, 78, 121);
            pnlHeader.Controls.Add(lblScreenSubTitle);
            pnlHeader.Controls.Add(lblScreenTitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Padding = new Padding(24, 10, 24, 8);
            pnlHeader.Size = new Size(1320, 74);
            pnlHeader.TabIndex = 0;
            //
            // lblScreenTitle
            //
            lblScreenTitle.AutoSize = true;
            lblScreenTitle.Dock = DockStyle.Top;
            lblScreenTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblScreenTitle.ForeColor = Color.White;
            lblScreenTitle.Location = new Point(24, 10);
            lblScreenTitle.Name = "lblScreenTitle";
            lblScreenTitle.Size = new Size(185, 41);
            lblScreenTitle.TabIndex = 0;
            lblScreenTitle.Text = "إدارة الشركات";
            //
            // lblScreenSubTitle
            //
            lblScreenSubTitle.AutoSize = true;
            lblScreenSubTitle.Dock = DockStyle.Bottom;
            lblScreenSubTitle.Font = new Font("Segoe UI", 9.5F);
            lblScreenSubTitle.ForeColor = Color.FromArgb(220, 232, 244);
            lblScreenSubTitle.Location = new Point(24, 43);
            lblScreenSubTitle.Name = "lblScreenSubTitle";
            lblScreenSubTitle.Size = new Size(349, 23);
            lblScreenSubTitle.TabIndex = 1;
            lblScreenSubTitle.Text = "تعريف الشركات التابعة وإدارة بياناتها الأساسية";
            //
            // pnlToolbar
            //
            pnlToolbar.BackColor = Color.White;
            pnlToolbar.Controls.Add(btnNew);
            pnlToolbar.Controls.Add(btnSaveCompany);
            pnlToolbar.Controls.Add(btnEdit);
            pnlToolbar.Controls.Add(btnDelete);
            pnlToolbar.Controls.Add(btnApprove);
            pnlToolbar.Controls.Add(btnUnApprove);
            pnlToolbar.Controls.Add(btnSearch);
            pnlToolbar.Controls.Add(btnRefresh);
            pnlToolbar.Controls.Add(btnPreview);
            pnlToolbar.Controls.Add(btnPrint);
            pnlToolbar.Controls.Add(btnExport);
            pnlToolbar.Controls.Add(btnImport);
            pnlToolbar.Controls.Add(btnClose);
            pnlToolbar.Dock = DockStyle.Top;
            pnlToolbar.FlowDirection = FlowDirection.RightToLeft;
            pnlToolbar.Location = new Point(0, 74);
            pnlToolbar.Name = "pnlToolbar";
            pnlToolbar.Padding = new Padding(12, 9, 12, 8);
            pnlToolbar.Size = new Size(1320, 58);
            pnlToolbar.TabIndex = 1;
            pnlToolbar.WrapContents = false;
            //
            // toolbar buttons
            //
            ConfigureToolbarButton(btnNew, "جديد");
            ConfigureToolbarButton(btnSaveCompany, "حفظ");
            ConfigureToolbarButton(btnEdit, "تعديل");
            ConfigureToolbarButton(btnDelete, "إيقاف");
            ConfigureToolbarButton(btnApprove, "إعادة تفعيل");
            ConfigureToolbarButton(btnUnApprove, "إلغاء الاعتماد");
            ConfigureToolbarButton(btnSearch, "بحث");
            ConfigureToolbarButton(btnRefresh, "تحديث");
            ConfigureToolbarButton(btnPreview, "معاينة");
            ConfigureToolbarButton(btnPrint, "طباعة");
            ConfigureToolbarButton(btnExport, "تصدير");
            ConfigureToolbarButton(btnImport, "استيراد");
            ConfigureToolbarButton(btnClose, "إغلاق");
            btnNew.Click += btnNew_Click;
            btnSaveCompany.Click += btnSaveCompany_Click;
            btnEdit.Click += btnEdit_Click;
            btnDelete.Click += btnDelete_Click;
            btnApprove.Click += btnApprove_Click;
            btnUnApprove.Click += btnUnApprove_Click;
            btnSearch.Click += btnSearch_Click;
            btnRefresh.Click += btnRefresh_Click;
            btnPreview.Click += btnPreview_Click;
            btnPrint.Click += btnPrint_Click;
            btnExport.Click += btnExport_Click;
            btnImport.Click += btnImport_Click;
            btnClose.Click += btnClose_Click;
            //
            // splitMain
            //
            splitMain.Dock = DockStyle.Fill;
            splitMain.FixedPanel = FixedPanel.Panel1;
            splitMain.Location = new Point(0, 132);
            splitMain.Name = "splitMain";
            splitMain.Panel1.Controls.Add(dgvCompanies);
            splitMain.Panel1.Controls.Add(pnlListHeader);
            splitMain.Panel1.Padding = new Padding(10);
            splitMain.Panel2.Controls.Add(pnlDetails);
            splitMain.Panel2.Padding = new Padding(10);
            splitMain.RightToLeft = RightToLeft.Yes;
            splitMain.Size = new Size(1320, 628);
            splitMain.SplitterDistance = 430;
            splitMain.TabIndex = 2;
            //
            // pnlListHeader
            //
            pnlListHeader.BackColor = Color.FromArgb(238, 244, 249);
            pnlListHeader.Controls.Add(lblListTitle);
            pnlListHeader.Dock = DockStyle.Top;
            pnlListHeader.Location = new Point(10, 10);
            pnlListHeader.Name = "pnlListHeader";
            pnlListHeader.Padding = new Padding(12);
            pnlListHeader.Size = new Size(410, 50);
            pnlListHeader.TabIndex = 0;
            //
            // lblListTitle
            //
            lblListTitle.AutoSize = true;
            lblListTitle.Dock = DockStyle.Right;
            lblListTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblListTitle.ForeColor = Color.FromArgb(31, 78, 121);
            lblListTitle.Location = new Point(281, 12);
            lblListTitle.Name = "lblListTitle";
            lblListTitle.Size = new Size(117, 25);
            lblListTitle.TabIndex = 0;
            lblListTitle.Text = "قائمة الشركات";
            //
            // dgvCompanies
            //
            dgvCompanies.AllowUserToAddRows = false;
            dgvCompanies.AllowUserToDeleteRows = false;
            dgvCompanies.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCompanies.BackgroundColor = Color.White;
            dgvCompanies.BorderStyle = BorderStyle.None;
            dgvCompanies.ColumnHeadersHeight = 38;
            dgvCompanies.Dock = DockStyle.Fill;
            dgvCompanies.Location = new Point(10, 60);
            dgvCompanies.MultiSelect = false;
            dgvCompanies.Name = "dgvCompanies";
            dgvCompanies.ReadOnly = true;
            dgvCompanies.RowHeadersVisible = false;
            dgvCompanies.RowHeadersWidth = 51;
            dgvCompanies.RowTemplate.Height = 34;
            dgvCompanies.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCompanies.Size = new Size(410, 558);
            dgvCompanies.TabIndex = 1;
            dgvCompanies.CellClick += dgvCompanies_CellClick;
            //
            // pnlDetails
            //
            pnlDetails.AutoScroll = true;
            pnlDetails.BackColor = Color.FromArgb(247, 249, 252);
            pnlDetails.Controls.Add(pnlAudit);
            pnlDetails.Controls.Add(grpLogo);
            pnlDetails.Controls.Add(grpContact);
            pnlDetails.Controls.Add(grpBasic);
            pnlDetails.Dock = DockStyle.Fill;
            pnlDetails.Location = new Point(10, 10);
            pnlDetails.Name = "pnlDetails";
            pnlDetails.Padding = new Padding(10);
            pnlDetails.Size = new Size(856, 608);
            pnlDetails.TabIndex = 0;
            //
            // grpBasic
            //
            grpBasic.Controls.Add(tblBasic);
            grpBasic.Dock = DockStyle.Top;
            grpBasic.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            grpBasic.ForeColor = Color.FromArgb(31, 78, 121);
            grpBasic.Location = new Point(10, 10);
            grpBasic.Name = "grpBasic";
            grpBasic.Padding = new Padding(14, 12, 14, 14);
            grpBasic.Size = new Size(836, 172);
            grpBasic.TabIndex = 0;
            grpBasic.TabStop = false;
            grpBasic.Text = "البيانات الأساسية";
            //
            // tblBasic
            //
            tblBasic.ColumnCount = 4;
            tblBasic.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16F));
            tblBasic.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            tblBasic.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16F));
            tblBasic.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            tblBasic.Controls.Add(label1, 0, 0);
            tblBasic.Controls.Add(cmbGroups, 1, 0);
            tblBasic.Controls.Add(label2, 2, 0);
            tblBasic.Controls.Add(txtCompanyNameAr, 3, 0);
            tblBasic.Controls.Add(label4, 0, 1);
            tblBasic.Controls.Add(txtCompanyPrefix, 1, 1);
            tblBasic.Controls.Add(chkIsActive, 3, 1);
            tblBasic.Dock = DockStyle.Fill;
            tblBasic.Location = new Point(14, 36);
            tblBasic.Name = "tblBasic";
            tblBasic.RowCount = 2;
            tblBasic.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tblBasic.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tblBasic.Size = new Size(808, 122);
            tblBasic.TabIndex = 0;
            StyleCaption(label1, "المجموعة التجارية");
            StyleCaption(label2, "اسم الشركة بالعربي");
            StyleCaption(label3, "اسم الشركة بالإنجليزي");
            StyleCaption(label4, "رمز الشركة");
            StyleCaption(label8, "الرقم الضريبي");
            StyleInput(cmbGroups);
            StyleInput(txtCompanyNameAr);
            StyleInput(txtCompanyNameEn);
            StyleInput(txtCompanyPrefix);
            StyleInput(txtTaxNumber);
            chkIsActive.AutoSize = true;
            chkIsActive.Dock = DockStyle.Right;
            chkIsActive.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            chkIsActive.ForeColor = Color.FromArgb(40, 40, 40);
            chkIsActive.Text = "شركة نشطة";
            chkIsActive.TextAlign = ContentAlignment.MiddleRight;
            //
            // grpContact
            //
            grpContact.Controls.Add(tblContact);
            grpContact.Dock = DockStyle.Top;
            grpContact.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            grpContact.ForeColor = Color.FromArgb(31, 78, 121);
            grpContact.Location = new Point(10, 182);
            grpContact.Name = "grpContact";
            grpContact.Padding = new Padding(14, 12, 14, 14);
            grpContact.Size = new Size(836, 235);
            grpContact.TabIndex = 1;
            grpContact.TabStop = false;
            grpContact.Text = "بيانات إضافية (اختيارية)";
            //
            // tblContact
            //
            tblContact.ColumnCount = 4;
            tblContact.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16F));
            tblContact.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            tblContact.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16F));
            tblContact.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            tblContact.Controls.Add(label3, 0, 0);
            tblContact.Controls.Add(txtCompanyNameEn, 1, 0);
            tblContact.Controls.Add(label11, 2, 0);
            tblContact.Controls.Add(txtActivityType, 3, 0);
            tblContact.Controls.Add(label8, 0, 1);
            tblContact.Controls.Add(txtTaxNumber, 1, 1);
            tblContact.Controls.Add(label10, 2, 1);
            tblContact.Controls.Add(txtMobile, 3, 1);
            tblContact.Controls.Add(label5, 0, 2);
            tblContact.Controls.Add(txtPhone, 1, 2);
            tblContact.Controls.Add(label6, 2, 2);
            tblContact.Controls.Add(txtEmail, 3, 2);
            tblContact.Controls.Add(label7, 0, 3);
            tblContact.Controls.Add(txtAddress, 1, 3);
            tblContact.SetColumnSpan(txtAddress, 3);
            tblContact.Dock = DockStyle.Fill;
            tblContact.Location = new Point(14, 36);
            tblContact.Name = "tblContact";
            tblContact.RowCount = 4;
            tblContact.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tblContact.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tblContact.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tblContact.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tblContact.Size = new Size(808, 185);
            tblContact.TabIndex = 0;
            StyleCaption(label5, "الهاتف");
            StyleCaption(label6, "البريد الإلكتروني");
            StyleCaption(label7, "العنوان");
            StyleCaption(label10, "الجوال");
            StyleCaption(label11, "نوع النشاط");
            StyleCaption(label3, "الاسم بالإنجليزي");
            StyleCaption(label8, "الرقم الضريبي");
            StyleInput(txtPhone);
            StyleInput(txtMobile);
            StyleInput(txtEmail);
            StyleInput(txtAddress);
            StyleInput(txtActivityType);
            StyleInput(txtCompanyNameEn);
            StyleInput(txtTaxNumber);
            //
            // grpLogo
            //
            grpLogo.Controls.Add(pnlLogoButtons);
            grpLogo.Controls.Add(picCompanyLogo);
            grpLogo.Controls.Add(label9);
            grpLogo.Dock = DockStyle.Top;
            grpLogo.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            grpLogo.ForeColor = Color.FromArgb(31, 78, 121);
            grpLogo.Location = new Point(10, 417);
            grpLogo.Name = "grpLogo";
            grpLogo.Padding = new Padding(14, 12, 14, 14);
            grpLogo.Size = new Size(836, 190);
            grpLogo.TabIndex = 2;
            grpLogo.TabStop = false;
            grpLogo.Text = "شعار الشركة";
            //
            // picCompanyLogo
            //
            picCompanyLogo.BackColor = Color.White;
            picCompanyLogo.BorderStyle = BorderStyle.FixedSingle;
            picCompanyLogo.Location = new Point(648, 34);
            picCompanyLogo.Name = "picCompanyLogo";
            picCompanyLogo.Size = new Size(160, 135);
            picCompanyLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picCompanyLogo.TabIndex = 0;
            picCompanyLogo.TabStop = false;
            //
            // label9
            //
            label9.AutoSize = true;
            label9.Font = new Font("Segoe UI", 9.5F);
            label9.ForeColor = Color.DimGray;
            label9.Location = new Point(24, 45);
            label9.Name = "label9";
            label9.Size = new Size(311, 21);
            label9.TabIndex = 1;
            label9.Text = "يفضل استخدام شعار بصيغة PNG بخلفية شفافة";
            //
            // pnlLogoButtons
            //
            pnlLogoButtons.Controls.Add(btnBrowseLogo);
            pnlLogoButtons.Controls.Add(btnRemoveLogo);
            pnlLogoButtons.FlowDirection = FlowDirection.RightToLeft;
            pnlLogoButtons.Location = new Point(22, 82);
            pnlLogoButtons.Name = "pnlLogoButtons";
            pnlLogoButtons.Size = new Size(318, 48);
            pnlLogoButtons.TabIndex = 2;
            ConfigureSmallButton(btnBrowseLogo, "اختيار الشعار");
            ConfigureSmallButton(btnRemoveLogo, "حذف الشعار");
            btnBrowseLogo.Click += btnBrowseLogo_Click;
            btnRemoveLogo.Click += btnRemoveLogo_Click;
            //
            // pnlAudit
            //
            pnlAudit.BackColor = Color.FromArgb(232, 238, 244);
            pnlAudit.Controls.Add(lblPrintCount);
            pnlAudit.Controls.Add(lblPrintCountCaption);
            pnlAudit.Controls.Add(lblEditCount);
            pnlAudit.Controls.Add(lblEditCountCaption);
            pnlAudit.Controls.Add(lblModifiedAt);
            pnlAudit.Controls.Add(lblModifiedAtCaption);
            pnlAudit.Controls.Add(lblModifiedBy);
            pnlAudit.Controls.Add(lblModifiedByCaption);
            pnlAudit.Controls.Add(lblCreatedAt);
            pnlAudit.Controls.Add(lblCreatedAtCaption);
            pnlAudit.Controls.Add(lblCreatedBy);
            pnlAudit.Controls.Add(lblCreatedByCaption);
            pnlAudit.Dock = DockStyle.Bottom;
            pnlAudit.Location = new Point(10, 548);
            pnlAudit.Name = "pnlAudit";
            pnlAudit.Padding = new Padding(10);
            pnlAudit.Size = new Size(836, 50);
            pnlAudit.TabIndex = 3;
            ConfigureAuditPair(lblCreatedByCaption, lblCreatedBy, "أنشئ بواسطة:", "—", 700);
            ConfigureAuditPair(lblCreatedAtCaption, lblCreatedAt, "تاريخ الإنشاء:", "—", 535);
            ConfigureAuditPair(lblModifiedByCaption, lblModifiedBy, "عُدل بواسطة:", "—", 370);
            ConfigureAuditPair(lblModifiedAtCaption, lblModifiedAt, "تاريخ التعديل:", "—", 205);
            ConfigureAuditPair(lblEditCountCaption, lblEditCount, "عدد التعديلات:", "0", 85);
            ConfigureAuditPair(lblPrintCountCaption, lblPrintCount, "عدد الطباعة:", "0", 0);
            //
            // CompanyForm
            //
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(247, 249, 252);
            ClientSize = new Size(1320, 760);
            Controls.Add(splitMain);
            Controls.Add(pnlToolbar);
            Controls.Add(pnlHeader);
            Font = new Font("Segoe UI", 9.5F);
            MinimumSize = new Size(1180, 700);
            Name = "CompanyForm";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            StartPosition = FormStartPosition.CenterParent;
            Text = "إدارة الشركات";
            Load += CompanyForm_Load;
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlToolbar.ResumeLayout(false);
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            pnlListHeader.ResumeLayout(false);
            pnlListHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCompanies).EndInit();
            pnlDetails.ResumeLayout(false);
            grpBasic.ResumeLayout(false);
            tblBasic.ResumeLayout(false);
            tblBasic.PerformLayout();
            grpContact.ResumeLayout(false);
            tblContact.ResumeLayout(false);
            tblContact.PerformLayout();
            grpLogo.ResumeLayout(false);
            grpLogo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picCompanyLogo).EndInit();
            pnlLogoButtons.ResumeLayout(false);
            pnlAudit.ResumeLayout(false);
            pnlAudit.PerformLayout();
            ResumeLayout(false);
        }

        private static void ConfigureToolbarButton(Button button, string text)
        {
            button.BackColor = Color.FromArgb(245, 248, 251);
            button.FlatAppearance.BorderColor = Color.FromArgb(190, 205, 220);
            button.FlatStyle = FlatStyle.Flat;
            button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            button.ForeColor = Color.FromArgb(31, 78, 121);
            button.Margin = new Padding(4, 0, 4, 0);
            button.Name = "btn" + text.Replace(" ", string.Empty);
            button.Size = new Size(88, 40);
            button.Text = text;
            button.UseVisualStyleBackColor = false;
        }

        private static void ConfigureSmallButton(Button button, string text)
        {
            button.BackColor = Color.White;
            button.FlatAppearance.BorderColor = Color.FromArgb(170, 190, 210);
            button.FlatStyle = FlatStyle.Flat;
            button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            button.ForeColor = Color.FromArgb(31, 78, 121);
            button.Margin = new Padding(5);
            button.Size = new Size(140, 36);
            button.Text = text;
            button.UseVisualStyleBackColor = false;
        }

        private static void StyleCaption(Label label, string text)
        {
            label.AutoSize = true;
            label.Dock = DockStyle.Right;
            label.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            label.ForeColor = Color.FromArgb(55, 65, 75);
            label.Margin = new Padding(5, 13, 5, 5);
            label.Text = text;
            label.TextAlign = ContentAlignment.MiddleRight;
        }

        private static void StyleInput(Control control)
        {
            control.Dock = DockStyle.Fill;
            control.Font = new Font("Segoe UI", 10F);
            control.Margin = new Padding(5, 9, 5, 9);
        }

        private static void ConfigureAuditPair(Label caption, Label value, string captionText, string valueText, int right)
        {
            caption.AutoSize = true;
            caption.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            caption.ForeColor = Color.FromArgb(70, 80, 90);
            caption.Location = new Point(right, 15);
            caption.Text = captionText;
            value.AutoSize = true;
            value.Font = new Font("Segoe UI", 8.5F);
            value.ForeColor = Color.FromArgb(31, 78, 121);
            value.Location = new Point(Math.Max(0, right - 50), 15);
            value.Text = valueText;
        }

        #endregion

        private Panel pnlHeader;
        private Label lblScreenTitle;
        private Label lblScreenSubTitle;
        private FlowLayoutPanel pnlToolbar;
        private SplitContainer splitMain;
        private Panel pnlListHeader;
        private Label lblListTitle;
        private Panel pnlDetails;
        private GroupBox grpBasic;
        private TableLayoutPanel tblBasic;
        private GroupBox grpContact;
        private TableLayoutPanel tblContact;
        private GroupBox grpLogo;
        private FlowLayoutPanel pnlLogoButtons;
        private Panel pnlAudit;
        private Label lblCreatedByCaption;
        private Label lblCreatedBy;
        private Label lblCreatedAtCaption;
        private Label lblCreatedAt;
        private Label lblModifiedByCaption;
        private Label lblModifiedBy;
        private Label lblModifiedAtCaption;
        private Label lblModifiedAt;
        private Label lblEditCountCaption;
        private Label lblEditCount;
        private Label lblPrintCountCaption;
        private Label lblPrintCount;
        private ComboBox cmbGroups;
        private TextBox txtCompanyNameAr;
        private TextBox txtCompanyNameEn;
        private Button btnSaveCompany;
        private Label label1;
        private Label label2;
        private Label label3;
        private TextBox txtCompanyPrefix;
        private Label label4;
        private Button btnRefresh;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnSearch;
        private TextBox txtEmail;
        private TextBox txtAddress;
        private TextBox txtPhone;
        private TextBox txtMobile;
        private Label label5;
        private Label label6;
        private Label label7;
        private TextBox txtTaxNumber;
        private Label label8;
        private Label label10;
        private Label label11;
        private TextBox txtActivityType;
        private CheckBox chkIsActive;
        private Button btnNew;
        private Button btnPrint;
        private Button btnClose;
        private Button btnExport;
        private Button btnImport;
        private Button btnPreview;
        private Button btnApprove;
        private Button btnUnApprove;
        private DataGridView dgvCompanies;
        private PictureBox picCompanyLogo;
        private Label label9;
        private Button btnBrowseLogo;
        private Button btnRemoveLogo;
    }
}
