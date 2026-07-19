namespace AlTayerERP.Desktop.Accounting.ReceiptVoucher
{
    partial class FrmJournalEntryInquiry
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
            panel1 = new Panel();
            panel2 = new Panel();
            panel3 = new Panel();
            dgvJournalDetails = new DataGridView();
            label13 = new Label();
            txtDocumentName = new TextBox();
            label1 = new Label();
            txtDocumentNo = new TextBox();
            btnPrint = new Button();
            btnClose = new Button();
            button3 = new Button();
            label2 = new Label();
            txtTotalCredit = new TextBox();
            label3 = new Label();
            txtTotalDebit = new TextBox();
            colNo = new DataGridViewTextBoxColumn();
            colAccountCode = new DataGridViewTextBoxColumn();
            colAccountName = new DataGridViewTextBoxColumn();
            colDebit = new DataGridViewTextBoxColumn();
            colCredit = new DataGridViewTextBoxColumn();
            colCurrency = new DataGridViewTextBoxColumn();
            colExchangeRate = new DataGridViewTextBoxColumn();
            colLocalDebit = new DataGridViewTextBoxColumn();
            colLocalCredit = new DataGridViewTextBoxColumn();
            colForeignAmount = new DataGridViewTextBoxColumn();
            colCostCenter = new DataGridViewTextBoxColumn();
            colDescription = new DataGridViewTextBoxColumn();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvJournalDetails).BeginInit();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.FromArgb(128, 128, 255);
            panel1.Controls.Add(button3);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(1063, 52);
            panel1.TabIndex = 0;
            // 
            // panel2
            // 
            panel2.Controls.Add(btnClose);
            panel2.Controls.Add(btnPrint);
            panel2.Controls.Add(label1);
            panel2.Controls.Add(txtDocumentNo);
            panel2.Controls.Add(label13);
            panel2.Controls.Add(txtDocumentName);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(0, 52);
            panel2.Name = "panel2";
            panel2.Size = new Size(1063, 52);
            panel2.TabIndex = 1;
            // 
            // panel3
            // 
            panel3.Controls.Add(label3);
            panel3.Controls.Add(txtTotalDebit);
            panel3.Controls.Add(label2);
            panel3.Controls.Add(txtTotalCredit);
            panel3.Dock = DockStyle.Bottom;
            panel3.Location = new Point(0, 517);
            panel3.Name = "panel3";
            panel3.Size = new Size(1063, 51);
            panel3.TabIndex = 0;
            // 
            // dgvJournalDetails
            // 
            dgvJournalDetails.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvJournalDetails.Columns.AddRange(new DataGridViewColumn[] { colNo, colAccountCode, colAccountName, colDebit, colCredit, colCurrency, colExchangeRate, colLocalDebit, colLocalCredit, colForeignAmount, colCostCenter, colDescription });
            dgvJournalDetails.Dock = DockStyle.Fill;
            dgvJournalDetails.Location = new Point(0, 104);
            dgvJournalDetails.Name = "dgvJournalDetails";
            dgvJournalDetails.RightToLeft = RightToLeft.Yes;
            dgvJournalDetails.RowHeadersWidth = 51;
            dgvJournalDetails.Size = new Size(1063, 413);
            dgvJournalDetails.TabIndex = 2;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(589, 16);
            label13.Name = "label13";
            label13.Size = new Size(74, 20);
            label13.TabIndex = 69;
            label13.Text = "اسم السند";
            // 
            // txtDocumentName
            // 
            txtDocumentName.Location = new Point(422, 13);
            txtDocumentName.Name = "txtDocumentName";
            txtDocumentName.RightToLeft = RightToLeft.No;
            txtDocumentName.Size = new Size(164, 27);
            txtDocumentName.TabIndex = 70;
            txtDocumentName.TextAlign = HorizontalAlignment.Center;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(979, 16);
            label1.Name = "label1";
            label1.Size = new Size(70, 20);
            label1.TabIndex = 71;
            label1.Text = "رقم السند";
            // 
            // txtDocumentNo
            // 
            txtDocumentNo.Location = new Point(812, 13);
            txtDocumentNo.Name = "txtDocumentNo";
            txtDocumentNo.RightToLeft = RightToLeft.No;
            txtDocumentNo.Size = new Size(164, 27);
            txtDocumentNo.TabIndex = 72;
            txtDocumentNo.TextAlign = HorizontalAlignment.Center;
            // 
            // btnPrint
            // 
            btnPrint.Location = new Point(226, 11);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(94, 29);
            btnPrint.TabIndex = 73;
            btnPrint.Text = "طباعه القيد";
            btnPrint.UseVisualStyleBackColor = true;
            // 
            // btnClose
            // 
            btnClose.Location = new Point(91, 12);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(94, 29);
            btnClose.TabIndex = 74;
            btnClose.Text = "إغلاق";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            button3.Font = new Font("Segoe UI", 14F);
            button3.Location = new Point(406, 3);
            button3.Name = "button3";
            button3.Size = new Size(304, 49);
            button3.TabIndex = 74;
            button3.Text = "استعراض القيد المحاسبي";
            button3.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(549, 15);
            label2.Name = "label2";
            label2.Size = new Size(84, 20);
            label2.TabIndex = 71;
            label2.Text = "اجمالى دائن";
            // 
            // txtTotalCredit
            // 
            txtTotalCredit.Location = new Point(382, 12);
            txtTotalCredit.Name = "txtTotalCredit";
            txtTotalCredit.RightToLeft = RightToLeft.No;
            txtTotalCredit.Size = new Size(164, 27);
            txtTotalCredit.TabIndex = 72;
            txtTotalCredit.TextAlign = HorizontalAlignment.Center;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(825, 15);
            label3.Name = "label3";
            label3.Size = new Size(89, 20);
            label3.TabIndex = 73;
            label3.Text = "اجمالى مدين";
            // 
            // txtTotalDebit
            // 
            txtTotalDebit.Location = new Point(658, 12);
            txtTotalDebit.Name = "txtTotalDebit";
            txtTotalDebit.RightToLeft = RightToLeft.No;
            txtTotalDebit.Size = new Size(164, 27);
            txtTotalDebit.TabIndex = 74;
            txtTotalDebit.TextAlign = HorizontalAlignment.Center;
            // 
            // colNo
            // 
            colNo.DataPropertyName = "No";
            colNo.HeaderText = "م";
            colNo.MinimumWidth = 6;
            colNo.Name = "colNo";
            colNo.Width = 125;
            // 
            // colAccountCode
            // 
            colAccountCode.DataPropertyName = "lAccount_Code";
            colAccountCode.HeaderText = "رقم الحساب";
            colAccountCode.MinimumWidth = 6;
            colAccountCode.Name = "colAccountCode";
            colAccountCode.Width = 125;
            // 
            // colAccountName
            // 
            colAccountName.DataPropertyName = "lAccount_Name";
            colAccountName.HeaderText = "اسم الحساب";
            colAccountName.MinimumWidth = 6;
            colAccountName.Name = "colAccountName";
            colAccountName.Width = 125;
            // 
            // colDebit
            // 
            colDebit.DataPropertyName = "Debit";
            colDebit.HeaderText = "مدين";
            colDebit.MinimumWidth = 6;
            colDebit.Name = "colDebit";
            colDebit.Width = 125;
            // 
            // colCredit
            // 
            colCredit.DataPropertyName = "Credit";
            colCredit.HeaderText = "دائن";
            colCredit.MinimumWidth = 6;
            colCredit.Name = "colCredit";
            colCredit.Width = 125;
            // 
            // colCurrency
            // 
            colCurrency.HeaderText = "العمله";
            colCurrency.MinimumWidth = 6;
            colCurrency.Name = "colCurrency";
            colCurrency.Width = 125;
            // 
            // colExchangeRate
            // 
            colExchangeRate.DataPropertyName = "lExchange_Rate";
            colExchangeRate.HeaderText = "سعر الصرف";
            colExchangeRate.MinimumWidth = 6;
            colExchangeRate.Name = "colExchangeRate";
            colExchangeRate.Width = 125;
            // 
            // colLocalDebit
            // 
            colLocalDebit.DataPropertyName = "lLocal_Debit";
            colLocalDebit.HeaderText = "مدين محلي";
            colLocalDebit.MinimumWidth = 6;
            colLocalDebit.Name = "colLocalDebit";
            colLocalDebit.Width = 125;
            // 
            // colLocalCredit
            // 
            colLocalCredit.DataPropertyName = "lLocal_Credit";
            colLocalCredit.HeaderText = "دائن محلي";
            colLocalCredit.MinimumWidth = 6;
            colLocalCredit.Name = "colLocalCredit";
            colLocalCredit.Width = 125;
            // 
            // colForeignAmount
            // 
            colForeignAmount.HeaderText = "المبلغ اجنبي";
            colForeignAmount.MinimumWidth = 6;
            colForeignAmount.Name = "colForeignAmount";
            colForeignAmount.Width = 125;
            // 
            // colCostCenter
            // 
            colCostCenter.HeaderText = "مركز التكلفه";
            colCostCenter.MinimumWidth = 6;
            colCostCenter.Name = "colCostCenter";
            colCostCenter.Width = 125;
            // 
            // colDescription
            // 
            colDescription.HeaderText = "البيان";
            colDescription.MinimumWidth = 6;
            colDescription.Name = "colDescription";
            colDescription.Width = 125;
            // 
            // FrmJournalEntryInquiry
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1063, 568);
            Controls.Add(dgvJournalDetails);
            Controls.Add(panel3);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Name = "FrmJournalEntryInquiry";
            Text = "ستعراض القيود المحاسبية";
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvJournalDetails).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Panel panel2;
        private Panel panel3;
        private DataGridView dgvJournalDetails;
        private Label label1;
        private TextBox txtDocumentNo;
        private Label label13;
        private TextBox txtDocumentName;
        private Button btnClose;
        private Button btnPrint;
        private Button button3;
        private Label label3;
        private TextBox txtTotalDebit;
        private Label label2;
        private TextBox txtTotalCredit;
        private DataGridViewTextBoxColumn colNo;
        private DataGridViewTextBoxColumn colAccountCode;
        private DataGridViewTextBoxColumn colAccountName;
        private DataGridViewTextBoxColumn colDebit;
        private DataGridViewTextBoxColumn colCredit;
        private DataGridViewTextBoxColumn colCurrency;
        private DataGridViewTextBoxColumn colExchangeRate;
        private DataGridViewTextBoxColumn colLocalDebit;
        private DataGridViewTextBoxColumn colLocalCredit;
        private DataGridViewTextBoxColumn colForeignAmount;
        private DataGridViewTextBoxColumn colCostCenter;
        private DataGridViewTextBoxColumn colDescription;
    }
}