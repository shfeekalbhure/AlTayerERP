namespace AlTayerERP.Desktop
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtGroupNameAr = new TextBox();
            txtGroupNameEn = new TextBox();
            btnSaveGroup = new Button();
            SuspendLayout();
            // 
            // txtGroupNameAr
            // 
            txtGroupNameAr.Location = new Point(565, 62);
            txtGroupNameAr.Name = "txtGroupNameAr";
            txtGroupNameAr.Size = new Size(125, 27);
            txtGroupNameAr.TabIndex = 0;
            // 
            // txtGroupNameEn
            // 
            txtGroupNameEn.Location = new Point(402, 62);
            txtGroupNameEn.Name = "txtGroupNameEn";
            txtGroupNameEn.Size = new Size(125, 27);
            txtGroupNameEn.TabIndex = 0;
            // 
            // btnSaveGroup
            // 
            btnSaveGroup.Location = new Point(370, 12);
            btnSaveGroup.Name = "btnSaveGroup";
            btnSaveGroup.Size = new Size(157, 29);
            btnSaveGroup.TabIndex = 1;
            btnSaveGroup.Text = "حفظ المجموعة";
            btnSaveGroup.UseVisualStyleBackColor = true;
            btnSaveGroup.Click += btnSaveGroup_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnSaveGroup);
            Controls.Add(txtGroupNameEn);
            Controls.Add(txtGroupNameAr);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtGroupNameAr;
        private TextBox txtGroupNameEn;
        private Button btnSaveGroup;
    }
}
