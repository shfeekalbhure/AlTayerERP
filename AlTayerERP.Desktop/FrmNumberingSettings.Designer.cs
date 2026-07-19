namespace AlTayerERP.Desktop
{
    partial class FrmNumberingSettings
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
            pnlButtons = new Panel();
            btnClose = new Button();
            btnRefresh = new Button();
            btnEdit = new Button();
            btnDelete = new Button();
            btnSave = new Button();
            btnNew = new Button();
            grpNumbering = new GroupBox();
            chkIsActive = new CheckBox();
            chkUseYear = new CheckBox();
            chkUseBranch = new CheckBox();
            chkUseCompany = new CheckBox();
            numLastNumber = new NumericUpDown();
            numDigitsCount = new NumericUpDown();
            txtPrefix = new TextBox();
            cmbResetType = new ComboBox();
            label8 = new Label();
            cmbDocumentType = new ComboBox();
            label3 = new Label();
            label4 = new Label();
            label2 = new Label();
            label1 = new Label();
            dgvNumberingSettings = new DataGridView();
            pnlButtons.SuspendLayout();
            grpNumbering.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numLastNumber).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numDigitsCount).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvNumberingSettings).BeginInit();
            SuspendLayout();
            // 
            // pnlButtons
            // 
            pnlButtons.Controls.Add(btnClose);
            pnlButtons.Controls.Add(btnRefresh);
            pnlButtons.Controls.Add(btnEdit);
            pnlButtons.Controls.Add(btnDelete);
            pnlButtons.Controls.Add(btnSave);
            pnlButtons.Controls.Add(btnNew);
            pnlButtons.Dock = DockStyle.Top;
            pnlButtons.Location = new Point(0, 0);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(1382, 92);
            pnlButtons.TabIndex = 0;
            // 
            // btnClose
            // 
            btnClose.Location = new Point(618, 12);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(90, 40);
            btnClose.TabIndex = 1;
            btnClose.Text = "اغلاق";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(725, 12);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(90, 40);
            btnRefresh.TabIndex = 1;
            btnRefresh.Text = "تحديث";
            btnRefresh.UseVisualStyleBackColor = true;
            // 
            // btnEdit
            // 
            btnEdit.Location = new Point(945, 12);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(90, 40);
            btnEdit.TabIndex = 1;
            btnEdit.Text = "تعديل";
            btnEdit.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(841, 12);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(90, 40);
            btnDelete.TabIndex = 1;
            btnDelete.Text = "حذف";
            btnDelete.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(1052, 12);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(90, 40);
            btnSave.TabIndex = 1;
            btnSave.Text = "حفظ";
            btnSave.UseVisualStyleBackColor = true;
            // 
            // btnNew
            // 
            btnNew.Location = new Point(1168, 12);
            btnNew.Name = "btnNew";
            btnNew.Size = new Size(90, 40);
            btnNew.TabIndex = 1;
            btnNew.Text = "جديد";
            btnNew.UseVisualStyleBackColor = true;
            // 
            // grpNumbering
            // 
            grpNumbering.Controls.Add(chkIsActive);
            grpNumbering.Controls.Add(chkUseYear);
            grpNumbering.Controls.Add(chkUseBranch);
            grpNumbering.Controls.Add(chkUseCompany);
            grpNumbering.Controls.Add(numLastNumber);
            grpNumbering.Controls.Add(numDigitsCount);
            grpNumbering.Controls.Add(txtPrefix);
            grpNumbering.Controls.Add(cmbResetType);
            grpNumbering.Controls.Add(label8);
            grpNumbering.Controls.Add(cmbDocumentType);
            grpNumbering.Controls.Add(label3);
            grpNumbering.Controls.Add(label4);
            grpNumbering.Controls.Add(label2);
            grpNumbering.Controls.Add(label1);
            grpNumbering.Dock = DockStyle.Top;
            grpNumbering.Location = new Point(0, 92);
            grpNumbering.Name = "grpNumbering";
            grpNumbering.Size = new Size(1382, 220);
            grpNumbering.TabIndex = 1;
            grpNumbering.TabStop = false;
            grpNumbering.Text = "بيانات اعدادات الترقيم";
            grpNumbering.Enter += grpNumbering_Enter;
            // 
            // chkIsActive
            // 
            chkIsActive.AutoSize = true;
            chkIsActive.Checked = true;
            chkIsActive.CheckState = CheckState.Checked;
            chkIsActive.Location = new Point(915, 176);
            chkIsActive.Name = "chkIsActive";
            chkIsActive.Size = new Size(66, 27);
            chkIsActive.TabIndex = 2;
            chkIsActive.Text = "فغال";
            chkIsActive.UseVisualStyleBackColor = true;
            // 
            // chkUseYear
            // 
            chkUseYear.AutoSize = true;
            chkUseYear.Location = new Point(866, 145);
            chkUseYear.Name = "chkUseYear";
            chkUseYear.Size = new Size(116, 27);
            chkUseYear.TabIndex = 2;
            chkUseYear.Text = "حسب السنة";
            chkUseYear.UseVisualStyleBackColor = true;
            // 
            // chkUseBranch
            // 
            chkUseBranch.AutoSize = true;
            chkUseBranch.Location = new Point(865, 113);
            chkUseBranch.Name = "chkUseBranch";
            chkUseBranch.Size = new Size(113, 27);
            chkUseBranch.TabIndex = 2;
            chkUseBranch.Text = "حسب الفرع";
            chkUseBranch.UseVisualStyleBackColor = true;
            // 
            // chkUseCompany
            // 
            chkUseCompany.AutoSize = true;
            chkUseCompany.Location = new Point(855, 80);
            chkUseCompany.Name = "chkUseCompany";
            chkUseCompany.Size = new Size(124, 27);
            chkUseCompany.TabIndex = 2;
            chkUseCompany.Text = "حسب الشركة";
            chkUseCompany.UseVisualStyleBackColor = true;
            // 
            // numLastNumber
            // 
            numLastNumber.Location = new Point(1140, 158);
            numLastNumber.Maximum = new decimal(new int[] { 1410065407, 2, 0, 0 });
            numLastNumber.Name = "numLastNumber";
            numLastNumber.Size = new Size(120, 30);
            numLastNumber.TabIndex = 2;
            numLastNumber.Value = new decimal(new int[] { 4, 0, 0, 0 });
            // 
            // numDigitsCount
            // 
            numDigitsCount.Location = new Point(1154, 122);
            numDigitsCount.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            numDigitsCount.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numDigitsCount.Name = "numDigitsCount";
            numDigitsCount.Size = new Size(106, 30);
            numDigitsCount.TabIndex = 2;
            numDigitsCount.Value = new decimal(new int[] { 4, 0, 0, 0 });
            // 
            // txtPrefix
            // 
            txtPrefix.Location = new Point(1143, 84);
            txtPrefix.Name = "txtPrefix";
            txtPrefix.Size = new Size(120, 30);
            txtPrefix.TabIndex = 2;
            // 
            // cmbResetType
            // 
            cmbResetType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbResetType.FormattingEnabled = true;
            cmbResetType.Location = new Point(785, 43);
            cmbResetType.Name = "cmbResetType";
            cmbResetType.Size = new Size(180, 31);
            cmbResetType.TabIndex = 2;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(967, 46);
            label8.Name = "label8";
            label8.Size = new Size(116, 23);
            label8.TabIndex = 2;
            label8.Text = "طريقة التصفير";
            // 
            // cmbDocumentType
            // 
            cmbDocumentType.FormattingEnabled = true;
            cmbDocumentType.Location = new Point(1143, 43);
            cmbDocumentType.Name = "cmbDocumentType";
            cmbDocumentType.Size = new Size(120, 31);
            cmbDocumentType.TabIndex = 2;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(1266, 46);
            label3.Name = "label3";
            label3.Size = new Size(95, 23);
            label3.TabIndex = 2;
            label3.Text = "نوع المستند";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(1266, 160);
            label4.Name = "label4";
            label4.Size = new Size(61, 23);
            label4.TabIndex = 2;
            label4.Text = "اخر رقم";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(1266, 88);
            label2.Name = "label2";
            label2.Size = new Size(54, 23);
            label2.TabIndex = 2;
            label2.Text = "البادئة";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(1266, 124);
            label1.Name = "label1";
            label1.Size = new Size(86, 23);
            label1.TabIndex = 2;
            label1.Text = "عدد الارقام";
            // 
            // dgvNumberingSettings
            // 
            dgvNumberingSettings.AllowUserToResizeRows = false;
            dgvNumberingSettings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvNumberingSettings.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvNumberingSettings.Dock = DockStyle.Fill;
            dgvNumberingSettings.Location = new Point(0, 312);
            dgvNumberingSettings.Name = "dgvNumberingSettings";
            dgvNumberingSettings.RowHeadersWidth = 51;
            dgvNumberingSettings.RowTemplate.ReadOnly = true;
            dgvNumberingSettings.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvNumberingSettings.Size = new Size(1382, 338);
            dgvNumberingSettings.TabIndex = 2;
            // 
            // FrmNumberingSettings
            // 
            AutoScaleDimensions = new SizeF(9F, 23F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1382, 650);
            Controls.Add(dgvNumberingSettings);
            Controls.Add(grpNumbering);
            Controls.Add(pnlButtons);
            Font = new Font("Segoe UI", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Name = "FrmNumberingSettings";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "صلاحياة الترقيم";
            WindowState = FormWindowState.Maximized;
            pnlButtons.ResumeLayout(false);
            grpNumbering.ResumeLayout(false);
            grpNumbering.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numLastNumber).EndInit();
            ((System.ComponentModel.ISupportInitialize)numDigitsCount).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvNumberingSettings).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlButtons;
        private Button button1;
        private Button button5;
        private Button button4;
        private Button button2;
        private Button button3;
        private Button btnSave;
        private Button btnNew;
        private Button btnClose;
        private Button btnRefresh;
        private Button btnEdit;
        private Button btnDelete;
        private GroupBox grpNumbering;
        private TextBox textBox1;
        private ComboBox cmbDocumentType;
        private Label label2;
        private Label label1;
        private TextBox txtPrefix;
        private NumericUpDown numericUpDown1;
        private NumericUpDown numDigitsCount;
        private Label label3;
        private Label label4;
        private NumericUpDown numLastNumber;
        private NumericUpDown numericUpDown2;
        private ComboBox comboBox1;
        private Label label8;
        private Label label7;
        private Label label6;
        private Label label5;
        private CheckBox chkIsActive;
        private CheckBox checkBox3;
        private CheckBox checkBox2;
        private CheckBox checkBox1;
        private CheckBox chkUseYear;
        private CheckBox chkUseBranch;
        private CheckBox chkUseCompany;
        private ComboBox cmbResetType;
        private DataGridView dgvNumberingSettings;
    }
}