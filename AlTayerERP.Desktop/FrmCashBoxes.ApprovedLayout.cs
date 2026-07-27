using System;
using System.Drawing;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// التحسين المعتمد لشاشة الصناديق دون تعديل ملف المصمم الأصلي.
    /// </summary>
    public partial class FrmCashBoxes
    {
        private bool _approvedLayoutApplied;

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ApplyApprovedCashBoxesLayout();
        }

        private void ApplyApprovedCashBoxesLayout()
        {
            if (_approvedLayoutApplied || IsDisposed)
                return;

            _approvedLayoutApplied = true;
            SuspendLayout();

            Text = "تعريف الصناديق";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(1100, 720);
            Size = new Size(1280, 820);
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

            // بيانات الشركة والاتصال موجودة في الشاشة الرئيسية؛ لذلك لا تكرر هنا.
            pnlTopBar.Visible = false;
            pnlStatusBar.Visible = false;

            Controls.Remove(grpCashBox);
            Controls.Remove(grpAdditionalData);
            Controls.Remove(grpActions);
            Controls.Remove(groupBox1);

            var titlePanel = BuildTitlePanel();
            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(14),
                BackColor = BackColor,
                ColumnCount = 2,
                RowCount = 3
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62F));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 330F));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            content.Controls.Add(titlePanel, 0, 0);
            content.SetColumnSpan(titlePanel, 2);

            ConfigureDataGroup();
            ConfigureFinancialGroup();
            ConfigureGridGroup();
            ConfigureActions();

            content.Controls.Add(grpCashBox, 1, 1);
            content.Controls.Add(grpAdditionalData, 0, 1);
            content.Controls.Add(groupBox1, 0, 2);
            content.SetColumnSpan(groupBox1, 2);

            Controls.Add(content);
            content.BringToFront();

            txtSearch.TextChanged -= ApprovedSearch_TextChanged;
            txtSearch.TextChanged += ApprovedSearch_TextChanged;
            txtSearch.KeyDown -= ApprovedSearch_KeyDown;
            txtSearch.KeyDown += ApprovedSearch_KeyDown;
            dgvCashBoxes.CellDoubleClick -= ApprovedGrid_CellDoubleClick;
            dgvCashBoxes.CellDoubleClick += ApprovedGrid_CellDoubleClick;

            AcceptButton = btnSearch;
            CancelButton = btnClose;
            ResumeLayout(true);
        }

        private Panel BuildTitlePanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(16, 10, 16, 10),
                Margin = new Padding(0, 0, 0, 10)
            };

            var title = new Label
            {
                Text = "تعريف الصناديق",
                Dock = DockStyle.Right,
                Width = 320,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font(Font.FontFamily, 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 41, 55)
            };

            var hint = new Label
            {
                Text = "إدارة صناديق الفروع وربطها بالحسابات والعملات والحدود المالية",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Color.FromArgb(107, 114, 128)
            };

            panel.Controls.Add(hint);
            panel.Controls.Add(title);
            return panel;
        }

        private void ConfigureDataGroup()
        {
            grpCashBox.Text = "بيانات الصندوق";
            grpCashBox.Dock = DockStyle.Fill;
            grpCashBox.Margin = new Padding(6, 0, 0, 8);
            grpCashBox.Padding = new Padding(12, 28, 12, 12);
            grpCashBox.BackColor = Color.White;

            ArrangeLabeledControl(label6, txtCashBoxCode, 15);
            ArrangeLabeledControl(label7, txtCashBoxNameAR, 58);
            ArrangeLabeledControl(label8, txtCashBoxNameEN, 101);
            ArrangeLabeledControl(label4, cmbBranch, 144);
            ArrangeLabeledControl(label3, cmbCurrency, 187);
            ArrangeLabeledControl(label5, cmbAccount, 230);

            txtCashBoxCode.ReadOnly = true;
            txtCashBoxCode.TabStop = false;
            txtCashBoxCode.BackColor = Color.FromArgb(243, 244, 246);
            cmbBranch.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCurrency.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAccount.DropDownStyle = ComboBoxStyle.DropDownList;

            chkIsActive.Text = "صندوق فعال";
            chkIsActive.AutoSize = true;
            chkIsActive.Location = new Point(18, 276);
            chkIsActive.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        }

        private void ConfigureFinancialGroup()
        {
            grpAdditionalData.Text = "الحدود والبيانات المالية";
            grpAdditionalData.Dock = DockStyle.Fill;
            grpAdditionalData.Margin = new Padding(0, 0, 6, 8);
            grpAdditionalData.Padding = new Padding(12, 28, 12, 12);
            grpAdditionalData.BackColor = Color.White;

            ArrangeLabeledControl(label1, numOpeningBalance, 15, 210);
            ArrangeLabeledControl(label13, numMaximumLimit, 58, 210);
            ArrangeLabeledControl(label2, numMinimumLimit, 101, 210);

            ConfigureMoneyInput(numOpeningBalance);
            ConfigureMoneyInput(numMaximumLimit);
            ConfigureMoneyInput(numMinimumLimit);

            label11.Text = "ملاحظات";
            label11.AutoSize = false;
            label11.TextAlign = ContentAlignment.MiddleRight;
            label11.SetBounds(grpAdditionalData.ClientSize.Width - 120, 150, 100, 30);
            label11.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            txtNotes.Multiline = true;
            txtNotes.ScrollBars = ScrollBars.Vertical;
            txtNotes.SetBounds(18, 150, Math.Max(260, grpAdditionalData.ClientSize.Width - 150), 105);
            txtNotes.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            dgvCashBoxCurrencies.Visible = false;
        }

        private void ConfigureGridGroup()
        {
            groupBox1.Text = "قائمة الصناديق";
            groupBox1.Dock = DockStyle.Fill;
            groupBox1.Margin = new Padding(0, 4, 0, 0);
            groupBox1.Padding = new Padding(10, 54, 10, 10);
            groupBox1.BackColor = Color.White;

            label9.Text = "بحث بالكود أو الاسم";
            label9.AutoSize = false;
            label9.TextAlign = ContentAlignment.MiddleRight;
            label9.SetBounds(groupBox1.ClientSize.Width - 190, 20, 170, 28);
            label9.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            txtSearch.RightToLeft = RightToLeft.Yes;
            txtSearch.PlaceholderText = "اكتب كود الصندوق أو اسمه...";
            txtSearch.SetBounds(groupBox1.ClientSize.Width - 520, 20, 320, 30);
            txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            panel1.Dock = DockStyle.Fill;
            dgvCashBoxes.RightToLeft = RightToLeft.Yes;
            dgvCashBoxes.BackgroundColor = Color.White;
            dgvCashBoxes.BorderStyle = BorderStyle.None;
            dgvCashBoxes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCashBoxes.RowHeadersVisible = false;
            dgvCashBoxes.RowTemplate.Height = 32;
            dgvCashBoxes.ColumnHeadersHeight = 38;
            dgvCashBoxes.EnableHeadersVisualStyles = false;
            dgvCashBoxes.ColumnHeadersDefaultCellStyle.Font = new Font(Font, FontStyle.Bold);
            dgvCashBoxes.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        }

        private void ConfigureActions()
        {
            grpActions.Text = string.Empty;
            grpActions.Dock = DockStyle.Bottom;
            grpActions.Height = 58;
            grpActions.Padding = new Padding(8);
            grpActions.BackColor = Color.White;

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(4)
            };

            grpActions.Controls.Clear();
            AddActionButton(flow, btnNew, "＋  جديد");
            AddActionButton(flow, btnSave, "💾  حفظ");
            AddActionButton(flow, btnEdit, "✎  تعديل");
            AddActionButton(flow, btnDelete, "🗑  حذف");
            AddActionButton(flow, btnSearch, "⌕  بحث");
            AddActionButton(flow, btnRefresh, "↻  تحديث");
            AddActionButton(flow, btnPrint, "🖨  طباعة");
            AddActionButton(flow, btnClose, "✕  إغلاق");
            grpActions.Controls.Add(flow);

            groupBox1.Controls.Add(grpActions);
            grpActions.BringToFront();
        }

        private static void AddActionButton(FlowLayoutPanel panel, Button button, string text)
        {
            button.Text = text;
            button.AutoSize = false;
            button.Size = new Size(112, 38);
            button.Margin = new Padding(4);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.BackColor = Color.White;
            button.ForeColor = Color.FromArgb(31, 41, 55);
            button.Cursor = Cursors.Hand;
            panel.Controls.Add(button);
        }

        private void ArrangeLabeledControl(Label label, Control control, int top, int controlWidth = 245)
        {
            label.AutoSize = false;
            label.TextAlign = ContentAlignment.MiddleRight;
            label.SetBounds(grpCashBox.ClientSize.Width - 125, top, 105, 30);
            label.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            control.SetBounds(18, top, Math.Max(controlWidth, grpCashBox.ClientSize.Width - 160), 30);
            control.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        }

        private static void ConfigureMoneyInput(NumericUpDown input)
        {
            input.DecimalPlaces = 2;
            input.ThousandsSeparator = true;
            input.Maximum = 999999999999M;
            input.Minimum = -999999999999M;
            input.TextAlign = HorizontalAlignment.Left;
        }

        private void ApprovedSearch_TextChanged(object sender, EventArgs e)
        {
            btnSearch.PerformClick();
        }

        private void ApprovedSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                txtSearch.Clear();
                e.SuppressKeyPress = true;
            }
        }

        private void ApprovedGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
                txtCashBoxNameAR.Focus();
        }
    }
}
