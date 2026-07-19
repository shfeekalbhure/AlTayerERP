using AlTayerERP.Desktop.Common;
using System;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// الجزء المسؤول عن شاشات البحث المساعدة داخل سند القبض.
    ///
    /// يحتوي حاليًا على:
    /// 1- فتح شاشة البحث عن الحسابات بواسطة F9.
    /// 2- نقل جزء رقم الحساب أو اسم الحساب إلى شاشة البحث.
    /// 3- استقبال الحساب المختار.
    /// 4- تعبئة رقم الحساب واسم الحساب ومعرفه داخل سطر السند.
    ///
    /// ويمكن إضافة البحث عن مركز التكلفة أو الطرف أو المشروع
    /// داخل هذا الملف مستقبلًا.
    /// </summary>
    public partial class FrmReceiptVoucher
    {
        #region أسماء أعمدة الحساب في جدول تفاصيل السند

        /// <summary>
        /// اسم العمود المخفي الذي يحتفظ بمعرف الحساب الحقيقي.
        /// </summary>
        private const string AccountIdColumnName =
            "colAccountId";

        /// <summary>
        /// اسم عمود رقم الحساب.
        /// </summary>
        private const string AccountCodeColumnName =
            "colAccountCode";

        /// <summary>
        /// اسم عمود اسم الحساب.
        /// </summary>
        private const string AccountNameColumnName =
            "colAccountName";

        #endregion

        #region التقاط زر F9

        /// <summary>
        /// التقاط اختصارات لوحة المفاتيح داخل شاشة سند القبض.
        ///
        /// عند الضغط على F9 داخل عمود رقم الحساب أو اسم الحساب:
        /// يتم فتح شاشة البحث عن الحسابات.
        /// </summary>
        protected override bool ProcessCmdKey(
            ref Message msg,
            Keys keyData)
        {
            if (keyData == Keys.F9 &&
                IsFocusInsideVoucherGrid() &&
                IsCurrentAccountColumn())
            {
                OpenAccountLookupForCurrentRow();

                return true;
            }

            return base.ProcessCmdKey(
                ref msg,
                keyData);
        }

        /// <summary>
        /// التحقق من أن المؤشر موجود داخل جدول تفاصيل السند
        /// أو داخل خلية قيد التعديل في الجدول.
        /// </summary>
        private bool IsFocusInsideVoucherGrid()
        {
            if (dgvVoucherDetails.Focused ||
                dgvVoucherDetails.ContainsFocus)
            {
                return true;
            }

            Control? editingControl =
                dgvVoucherDetails.EditingControl;

            return editingControl != null &&
                   editingControl.ContainsFocus;
        }

        /// <summary>
        /// التحقق من أن الخلية الحالية هي:
        /// - رقم الحساب.
        /// - اسم الحساب.
        ///
        /// يدعم التحقق بالاسم البرمجي أو بالعنوان العربي.
        /// </summary>
        private bool IsCurrentAccountColumn()
        {
            if (dgvVoucherDetails.CurrentCell == null)
            {
                return false;
            }

            DataGridViewColumn currentColumn =
                dgvVoucherDetails.CurrentCell.OwningColumn;

            string columnName =
                currentColumn.Name?.Trim()
                ?? string.Empty;

            string headerText =
                currentColumn.HeaderText?.Trim()
                ?? string.Empty;

            return
                columnName.Equals(
                    AccountCodeColumnName,
                    StringComparison.OrdinalIgnoreCase) ||

                columnName.Equals(
                    AccountNameColumnName,
                    StringComparison.OrdinalIgnoreCase) ||

                headerText.Equals(
                    "رقم الحساب",
                    StringComparison.OrdinalIgnoreCase) ||

                headerText.Equals(
                    "اسم الحساب",
                    StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region فتح شاشة البحث عن الحساب

        /// <summary>
        /// فتح شاشة البحث عن الحساب للسطر الحالي.
        ///
        /// إذا كتب المستخدم جزءًا من رقم الحساب أو اسم الحساب،
        /// يتم نقله تلقائيًا إلى حقل البحث داخل شاشة الحسابات.
        /// </summary>
        private void OpenAccountLookupForCurrentRow()
        {
            if (dgvVoucherDetails.CurrentCell == null ||
                dgvVoucherDetails.CurrentRow == null)
            {
                return;
            }

            // الاحتفاظ بالسطر الحالي قبل فتح شاشة البحث.
            DataGridViewRow currentRow =
                dgvVoucherDetails.CurrentRow;

            // قراءة النص الذي كتبه المستخدم داخل الخلية الحالية.
            string initialSearchText =
                GetCurrentAccountSearchText();

            // إنهاء تحرير الخلية حتى تثبت القيمة داخل الجدول.
            dgvVoucherDetails.EndEdit();

            using FrmAccountLookup lookupForm =
                new FrmAccountLookup(initialSearchText);

            DialogResult result =
                lookupForm.ShowDialog(this);

            // إذا أغلق المستخدم الشاشة دون اختيار حساب.
            if (result != DialogResult.OK)
            {
                return;
            }

            // تعبئة بيانات الحساب المختار داخل السطر.
            SetSelectedAccountToRow(
                currentRow,
                lookupForm);
        }

        /// <summary>
        /// قراءة النص الحالي من خلية رقم الحساب أو اسم الحساب.
        ///
        /// إذا كانت الخلية ما زالت في وضع التحرير،
        /// تتم قراءة النص مباشرة من أداة التحرير.
        /// </summary>
        private string GetCurrentAccountSearchText()
        {
            if (dgvVoucherDetails.EditingControl
                is TextBox editingTextBox)
            {
                return editingTextBox.Text.Trim();
            }

            return dgvVoucherDetails.CurrentCell?
                       .Value?
                       .ToString()?
                       .Trim()
                   ?? string.Empty;
        }

        #endregion

        #region تعبئة الحساب داخل سطر السند

        /// <summary>
        /// تعبئة بيانات الحساب المختار داخل السطر الحالي.
        ///
        /// القيم التي يتم تعبئتها:
        /// - معرف الحساب الداخلي.
        /// - رقم الحساب.
        /// - اسم الحساب.
        /// </summary>
        private void SetSelectedAccountToRow(
            DataGridViewRow row,
            FrmAccountLookup lookupForm)
        {
            if (row == null)
            {
                return;
            }

            // التأكد من وجود الأعمدة المطلوبة داخل جدول السند.
            if (!ValidateVoucherAccountColumns())
            {
                return;
            }

            // حفظ معرف الحساب الحقيقي لاستخدامه عند الحفظ.
            row.Cells[AccountIdColumnName].Value =
                lookupForm.SelectedAccountId;

            // عرض رقم الحساب للمستخدم.
            row.Cells[AccountCodeColumnName].Value =
                lookupForm.SelectedAccountCode;

            // عرض اسم الحساب للمستخدم.
            row.Cells[AccountNameColumnName].Value =
                lookupForm.SelectedAccountName;

            // نقل المؤشر إلى الخلية التالية بعد اسم الحساب.
            MoveToNextCellAfterAccount(row);
        }

        /// <summary>
        /// التأكد من وجود أعمدة الحساب المطلوبة داخل الجدول.
        /// </summary>
        private bool ValidateVoucherAccountColumns()
        {
            bool hasAccountId =
                dgvVoucherDetails.Columns.Contains(
                    AccountIdColumnName);

            bool hasAccountCode =
                dgvVoucherDetails.Columns.Contains(
                    AccountCodeColumnName);

            bool hasAccountName =
                dgvVoucherDetails.Columns.Contains(
                    AccountNameColumnName);

            if (hasAccountId &&
                hasAccountCode &&
                hasAccountName)
            {
                return true;
            }

            string missingColumns =
                string.Empty;

            if (!hasAccountId)
            {
                missingColumns +=
                    "\n- colAccountId";
            }

            if (!hasAccountCode)
            {
                missingColumns +=
                    "\n- colAccountCode";
            }

            if (!hasAccountName)
            {
                missingColumns +=
                    "\n- colAccountName";
            }

            MessageBox.Show(
                "أعمدة الحساب التالية غير موجودة " +
                "داخل جدول تفاصيل السند:" +
                missingColumns +
                "\n\nيرجى مراجعة أسماء الأعمدة في Designer.",
                "أعمدة الحساب",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return false;
        }

        /// <summary>
        /// نقل المؤشر إلى العمود التالي بعد اختيار الحساب.
        ///
        /// يتم البحث عن أول عمود ظاهر وغير للقراءة فقط
        /// بعد عمود اسم الحساب.
        /// </summary>
        private void MoveToNextCellAfterAccount(
            DataGridViewRow row)
        {
            if (!dgvVoucherDetails.Columns.Contains(
                AccountNameColumnName))
            {
                return;
            }

            int accountNameColumnIndex =
                dgvVoucherDetails
                    .Columns[AccountNameColumnName]
                    .Index;

            for (int columnIndex =
                     accountNameColumnIndex + 1;
                 columnIndex <
                     dgvVoucherDetails.Columns.Count;
                 columnIndex++)
            {
                DataGridViewColumn nextColumn =
                    dgvVoucherDetails.Columns[columnIndex];

                if (!nextColumn.Visible ||
                    nextColumn.ReadOnly)
                {
                    continue;
                }

                dgvVoucherDetails.CurrentCell =
                    row.Cells[columnIndex];

                dgvVoucherDetails.Focus();

                return;
            }

            // إذا لم يوجد عمود مناسب بعد اسم الحساب،
            // يبقى المؤشر على خلية اسم الحساب.
            dgvVoucherDetails.CurrentCell =
                row.Cells[AccountNameColumnName];

            dgvVoucherDetails.Focus();
        }

        #endregion
    }
}