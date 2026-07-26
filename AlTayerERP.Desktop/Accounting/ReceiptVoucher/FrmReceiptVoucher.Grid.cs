using AlTayerERP.Desktop.Common;
using System;
using System.Linq;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// التعامل مع جدول تفاصيل السند.
    /// </summary>
    public partial class FrmReceiptVoucher
    {
        #region === إعداد الجدول ===

        /// <summary>
        /// دالة تهيئة وإعداد جدول تفاصيل السند (dgvVoucherDetails)
        /// تقوم بضبط خصائص العرض، ومنع التكرار في الأحداث عبر إلغاء الاشتراك ثم إعادة الاشتراك بها.
        /// </summary>
        private void ConfigureVoucherGrid()
        {




            // منع التوليد التلقائي للأعمدة للاعتماد على الأعمدة المعرفة مسبقاً
            dgvVoucherDetails.AutoGenerateColumns = false;
            // السماح للمستخدم بإضافة صفوف جديدة
            dgvVoucherDetails.AllowUserToAddRows = true;
            // السماح للمستخدم بحذف الصفوف
            dgvVoucherDetails.AllowUserToDeleteRows = true;
            // منع تحديد أكثر من خلية في نفس الوقت
            dgvVoucherDetails.MultiSelect = false;
            // جعل نمط التحديد يعتمد على الخلية المفردة
            dgvVoucherDetails.SelectionMode = DataGridViewSelectionMode.CellSelect;
            // تفعيل نمط التعديل بمجرد الضغط والدخول إلى الخلية
            dgvVoucherDetails.EditMode = DataGridViewEditMode.EditOnEnter;
            // إيقاف ضبط الحجم التلقائي للأعمدة
            dgvVoucherDetails.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            // إيقاف ضبط الحجم التلقائي للأسطر
            dgvVoucherDetails.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            // إظهار شريطي تمرير واضحين عند زيادة أسطر أو أعمدة التوزيع المحاسبي.
            dgvVoucherDetails.ScrollBars = ScrollBars.Both;
            // منع المستخدم من تغيير ارتفاع ترويسة الأعمدة
            dgvVoucherDetails.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            // تحديد ارتفاع ترويسة الأعمدة بـ 35 بكسل
            dgvVoucherDetails.ColumnHeadersHeight = 35;
            // إظهار العمود الجانبي لعناوين الصفوف
            dgvVoucherDetails.RowHeadersVisible = true;
            // تحديد عرض ترويسة الصف الجانبي بـ 50 بكسل
            dgvVoucherDetails.RowHeadersWidth = 50;
            // تفعيل التنقل القياسي عبر زر Tab
            dgvVoucherDetails.StandardTab = true;

            // الأعمدة الناتجة عن الحساب لا تعدل يدويًا حتى لا تختلف عن المبلغ الأصلي.
            colNo.ReadOnly = true;
            colForeignAmount.ReadOnly = true;
            colLocalAmount.ReadOnly = true;

            // إلغاء التسجيل أولاً ثم إعادة التسجيل لمنع التكرار في الأحداث
            dgvVoucherDetails.CurrentCellDirtyStateChanged -= dgvVoucherDetails_CurrentCellDirtyStateChanged;
            dgvVoucherDetails.CellEndEdit -= dgvVoucherDetails_CellEndEdit;
            dgvVoucherDetails.CellValueChanged -= dgvVoucherDetails_CellValueChanged;
            dgvVoucherDetails.DefaultValuesNeeded -= dgvVoucherDetails_DefaultValuesNeeded;
            dgvVoucherDetails.DataError -= dgvVoucherDetails_DataError;
            dgvVoucherDetails.RowsRemoved -= dgvVoucherDetails_RowsRemoved;

            // إعادة تسجيل الأحداث لربطها بالدوال المعنية
            dgvVoucherDetails.CurrentCellDirtyStateChanged += dgvVoucherDetails_CurrentCellDirtyStateChanged;
            dgvVoucherDetails.CellEndEdit += dgvVoucherDetails_CellEndEdit;
            dgvVoucherDetails.CellValueChanged += dgvVoucherDetails_CellValueChanged;
            dgvVoucherDetails.DefaultValuesNeeded += dgvVoucherDetails_DefaultValuesNeeded;
            dgvVoucherDetails.DataError += dgvVoucherDetails_DataError;
            dgvVoucherDetails.RowsRemoved += dgvVoucherDetails_RowsRemoved;


           // أضف هذا السطر في آخر دالة ConfigureVoucherGrid()
            dgvVoucherDetails.EditingControlShowing -= dgvVoucherDetails_EditingControlShowing;
            dgvVoucherDetails.EditingControlShowing += dgvVoucherDetails_EditingControlShowing;

           // أضف هذه الأسطر في نهاية دالة ConfigureVoucherGrid()
           dgvVoucherDetails.CellDoubleClick -= dgvVoucherDetails_CellDoubleClick;
            dgvVoucherDetails.CellDoubleClick += dgvVoucherDetails_CellDoubleClick;

            dgvVoucherDetails.KeyDown -= dgvVoucherDetails_KeyDown;
            dgvVoucherDetails.KeyDown += dgvVoucherDetails_KeyDown;

        }

        #endregion

        #region === أحداث الجدول ===

        /// <summary>
        /// حدث يتم استدعاؤه فور تغير حالة الخلية الحالية (أصبحت "غير نظيفة" أو تم تعديل قيمتها).
        /// وظيفته: حفظ واعتماد القيمة المدخلة في الخلية فوراً دون الحاجة للانتقال لخلية أخرى.
        /// </summary>

        private void dgvVoucherDetails_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            // إذا كانت الخلية الحالية هي خلية عملة أو حساب، نقوم بإنهاء التعديل فوراً لتشغيل الحسبة دون انتظار الخروج
            if (dgvVoucherDetails.CurrentCell is DataGridViewComboBoxCell)
            {
                dgvVoucherDetails.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
            else if (dgvVoucherDetails.IsCurrentCellDirty)
            {
                dgvVoucherDetails.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        /// <summary>
        /// حدث يتم استدعاؤه عند تغير قيمة الخلية في الجدول.
        /// وظيفته: فحص الخلية المعدلة؛ فإذا كانت العمود الخاص بالعملة (colCurrency) يقوم بتحديث سعر الصرف.
        /// وإذا كانت الخلية المعدلة هي حقل المبلغ (colAmount) أو حقل سعر الصرف (colExchangeRate) يعيد احتساب المبالغ وتحديث الإجماليات.
        /// </summary>



        private void dgvVoucherDetails_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            // تجنب التنفيذ في حالات التحميل، أو عند النقر على الترويسات
            if (_isLoading || e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            // إذا كان التعديل ناتج عن دالة الحساب الحالية، نتخطى لمنع الحلقة اللانهائية
            if (_isCalculatingGridAmounts) return;

            DataGridViewRow row = dgvVoucherDetails.Rows[e.RowIndex];
            string columnName = dgvVoucherDetails.Columns[e.ColumnIndex].Name;

            try
            {
                _isCalculatingGridAmounts = true;

                // التحقق من العمود الذي تم تعديل قيمته
                if (columnName == colAccountCode.Name ||
                    columnName == colAccountName.Name)
                {
                    object? accountId = row.Cells[columnName].Value;
                    row.Cells[colAccountCode.Name].Value = accountId;
                    row.Cells[colAccountName.Name].Value = accountId;
                    return;
                }
                else if (columnName == colCurrency.Name)
                {
                    UpdateGridRowExchangeRate(row); // تحديث سعر الصرف للعملة الجديدة في الصف
                }
                else if (columnName == colAmount.Name || columnName == colExchangeRate.Name)
                {
                    // الحساب الفوري لسطر الجدول بدون قيود معطلة
                    CalculateGridRowCurrencyAmountsInternal(row);
                }
                else
                {
                    return;
                }

                UpdateVoucherTotals(); // تحديث إجماليات السند بالكامل
            }
            finally
            {
                _isCalculatingGridAmounts = false;
            }
        }







        /// <summary>
        /// حدث النقر المزدوج على الخلية لفتح شاشة البحث عن الحسابات
        /// </summary>
        private void dgvVoucherDetails_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            // التأكد من أن المستخدم نقر على سطر حقيقي وداخل عمود رقم الحساب colAccountCode
            if (e.RowIndex >= 0 && dgvVoucherDetails.Columns[e.ColumnIndex].Name == "colAccountCode")
            {
                OpenAccountLookupForm(e.RowIndex);
            }
        }

        /// <summary>
        /// حدث الضغط على الأزرار في الجدول (لتفعيل زر F9 للبحث السريع)
        /// </summary>
        private void dgvVoucherDetails_KeyDown(object? sender, KeyEventArgs e)
        {
            if (dgvVoucherDetails.CurrentCell == null) return;

            int rowIndex = dgvVoucherDetails.CurrentCell.RowIndex;
            string columnName = dgvVoucherDetails.CurrentCell.OwningColumn.Name;

            // F9 يفتح شاشة الاستعلام المناسبة للخلية الحالية.
            if (e.KeyCode != Keys.F9 || rowIndex < 0)
            {
                return;
            }

            if (columnName == "colAccountCode")
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                OpenAccountLookupForm(rowIndex);
            }
            else if (columnName == "colCostCenter")
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                OpenCostCenterLookupForm(rowIndex);
            }
        }

        /// <summary>
        /// فتح استعلام الحسابات لسطر التفاصيل.
        /// يعتمد على الحسابات المحملة مع السند، لذلك لا يفتح شاشة فارغة عند تعذر طلب إضافي للـ API.
        /// </summary>
        private void OpenAccountLookupForm(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgvVoucherDetails.Rows.Count)
            {
                return;
            }

            var items = _accountLookups
                .Select(account => new LookupDialogItem
                {
                    Id = account.Account_ID,
                    Code = account.Account_Code,
                    Name = account.Account_Name_AR
                })
                .Where(item => !string.IsNullOrWhiteSpace(item.Id))
                .ToList();

            if (items.Count == 0)
            {
                MessageBox.Show(
                    "لا توجد حسابات فعالة متاحة للشركة الحالية. راجع دليل الحسابات.",
                    "استعلام الحسابات",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            string currentSearchText =
                Convert.ToString(
                    dgvVoucherDetails.Rows[rowIndex]
                        .Cells["colAccountCode"].Value)
                ?? string.Empty;

            dgvVoucherDetails.EndEdit();

            using var lookupForm =
                new FrmReferenceLookup("استعلام دليل الحسابات", items, currentSearchText);

            if (lookupForm.ShowDialog(this) != DialogResult.OK ||
                lookupForm.SelectedItem == null)
            {
                return;
            }

            DataGridViewRow row = dgvVoucherDetails.Rows[rowIndex];
            row.Cells["colAccountCode"].Value = lookupForm.SelectedItem.Id;

            if (dgvVoucherDetails.Columns.Contains("colAccountName"))
            {
                row.Cells["colAccountName"].Value = lookupForm.SelectedItem.Id;
            }

            dgvVoucherDetails.CurrentCell = row.Cells["colAccountCode"];
            dgvVoucherDetails.Refresh();
        }











        /// <summary>
        /// حدث يتم استدعاؤه فور انتهاء المستخدم من تعديل خلية ما.
        /// وظيفته: التأكد من إعادة احتساب مبالغ الصف المعدل وتحديث إجمالي السند.
        /// </summary>
        private void dgvVoucherDetails_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (_isLoading || e.RowIndex < 0) return;
            UpdateVoucherRowAmounts(e.RowIndex);
            UpdateVoucherTotals();
        }

        /// <summary>
        /// حدث يتم استدعاؤه عند الحاجة لتعبئة قيم افتراضية لصف تم إنشاؤه حديثاً.
        /// وظيفته: تعبئة الصف بالعملة الافتراضية (colCurrency)، وسعر صرفها (colExchangeRate)، وتصفير حقول المبالغ (colAmount, colLocalAmount, colForeignAmount).
        /// </summary>
        private void dgvVoucherDetails_DefaultValuesNeeded(object? sender, DataGridViewRowEventArgs e)
        {
            CurrencyLookupModel? defaultCurrency = ResolveDefaultCurrency();
            if (defaultCurrency == null) return;

            e.Row.Cells[colNo.Name].Value = e.Row.Index + 1;
            e.Row.Cells[colCurrency.Name].Value = defaultCurrency.Currency_ID;
            e.Row.Cells[colExchangeRate.Name].Value = NormalizeExchangeRate(defaultCurrency.Exchange_Rate);
            e.Row.Cells[colAmount.Name].Value = 0m;
            e.Row.Cells[colLocalAmount.Name].Value = 0m;
            e.Row.Cells[colForeignAmount.Name].Value = 0m;
        }
        /// <summary>
        /// حدث يتم استدعاؤه عند دخول الخلية.
        /// وظيفته: فتح القائمة المنسدلة تلقائياً عند التنقل بالكيبورد، مع الحفاظ على إمكانية النقر المزدوج بالماوس.
        /// </summary>
        private void dgvVoucherDetails_CellEnter(object? sender, DataGridViewCellEventArgs e)
        {
            // التحقق من أن المستخدم يقف على عمود رقم الحساب وصحة الفهارس
            if (e.RowIndex >= 0 && dgvVoucherDetails.Columns[e.ColumnIndex].Name == "colAccountCode")
            {
                // 🎯 الشرط السحري: إذا دخل المستخدم الخلية بدون استخدام أزرار الماوس (أي بالأسهم أو Tab)
                if (Control.MouseButtons == MouseButtons.None)
                {
                    // يجبر الجدول على الدخول في طور التعديل وفتح القائمة فوراً
                    dgvVoucherDetails.BeginEdit(true);

                    if (dgvVoucherDetails.EditingControl is ComboBox combo)
                    {
                        combo.DroppedDown = true; // عرض القائمة المنسدلة فوراً
                    }
                }
            }
        }





        /// <summary>
        /// حدث يتم استدعاؤه عند طلب شاشة الحسابات في  الجدول (Data Error).
        /// وظيفته: ...................................
        /// </summary>

        //    private void dgvVoucherDetails_CellEnter(object? sender, DataGridViewCellEventArgs e)
        //     {
        // التحقق من أن المستخدم يقف على عمود رقم الحساب وصحت الفهارس
        //        if (e.RowIndex >= 0 && dgvVoucherDetails.Columns[e.ColumnIndex].Name == "colAccountCode")
        //        {
        // السطر السحري الذي يجبر الجدول على الدخول في طور التعديل وفتح القائمة فوراً
        //             dgvVoucherDetails.BeginEdit(true);

        //            if (dgvVoucherDetails.EditingControl is ComboBox combo)
        //            {
        //                 combo.DroppedDown = true; // ✅ عرض القائمة المنسدلة فوراً على طول!
        //             }
        //         }
        //     }
        /// <summary>
        /// حدث يتم استدعاؤه عند حدوث خطأ في صحة بيانات الجدول (Data Error).
        /// وظيفته: إيقاف رمي الاستثناءات وتفادي عرض شاشات الخطأ الافتراضية للمستخدم.
        /// </summary>
        private void dgvVoucherDetails_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
        }

        /// <summary>
        /// حدث يتم استدعاؤه فور حذف صف أو مجموعة صفوف من الجدول.
        /// وظيفته: إعادة احتساب وتحديث إجماليات السند بعد عملية الحذف.
        /// </summary>
        private void dgvVoucherDetails_RowsRemoved(object? sender, DataGridViewRowsRemovedEventArgs e)
        {
            if (!_isLoading) UpdateVoucherTotals();
        }

        /// <summary>
        /// حدث الضغط على محتويات خلايا الجدول.
        /// وظيفته: لا يحتوي على منطق برمجي حالياً ولكنه يتحقق من صحة فهرس الصف.
        /// </summary>
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
        }

        #endregion

        #region === تحديث صف الجدول ===

        /// <summary>
        /// دالة تقوم بتحديث سعر الصرف للصف بناءً على العملة المحددة فيه.
        /// وظيفته: البحث عن سعر صرف العملة المحددة ووضعه في خلية سعر الصرف (colExchangeRate)، ثم إعادة احتساب مبالغ الصف.
        /// </summary>
        private void UpdateGridRowExchangeRate(DataGridViewRow row)
        {
            int currencyId = GetGridIntValue(row.Cells[colCurrency.Name].Value);
            CurrencyLookupModel? currency = _currencyLookups.FirstOrDefault(x => x.Currency_ID == currencyId);

            if (currency == null)
            {
                currency = ResolveDefaultCurrency();
                if (currency == null) return;
                row.Cells[colCurrency.Name].Value = currency.Currency_ID;
            }

            row.Cells[colExchangeRate.Name].Value = NormalizeExchangeRate(currency.Exchange_Rate);
            CalculateGridRowCurrencyAmounts(row);
        }

        /// <summary>
        /// دالة تتحقق من صحة بيانات العملة المحددة لصف معين وإعادة تحديث واحتساب مبالغه.
        /// وظيفته: استرجاع العملة وتحديث سعر صرفها وقيمها إذا لزم الأمر بناءً على فهرس الصف (rowIndex).
        /// </summary>
        private void UpdateVoucherRowAmounts(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgvVoucherDetails.Rows.Count) return;

            DataGridViewRow row = dgvVoucherDetails.Rows[rowIndex];
            if (row.IsNewRow) return;

            int currencyId = GetGridIntValue(row.Cells[colCurrency.Name].Value);
            CurrencyLookupModel? selectedCurrency = _currencyLookups.FirstOrDefault(x => x.Currency_ID == currencyId);

            if (selectedCurrency == null)
            {
                selectedCurrency = ResolveDefaultCurrency();
                if (selectedCurrency == null) return;

                row.Cells[colCurrency.Name].Value = selectedCurrency.Currency_ID;
                row.Cells[colExchangeRate.Name].Value = NormalizeExchangeRate(selectedCurrency.Exchange_Rate);
            }

            CalculateGridRowCurrencyAmounts(row);
        }

        #endregion

        #region === العمليات الحسابية ===

        /// <summary>
        /// دالة تقوم باحتساب مبالغ الصف (المبلغ المحلي والمبلغ الأجنبي).
        /// وظيفته الحسابية: 
        /// 1. ضرب مبلغ الصف (colAmount) بسعر الصرف (colExchangeRate) للحصول على المبلغ المحلي (colLocalAmount).
        /// 2. تحديد قيمة المبلغ الأجنبي (colForeignAmount): فإذا كانت العملة محلية تكون القيمة صفرية (0m)، وغير ذلك يوضع نفس قيمة المبلغ.
        /// </summary>
        /// <summary>
        /// دالة تقوم باحتساب مبالغ الصف (المبلغ المحلي والمبلغ الأجنبي).
        /// </summary>
        private void CalculateGridRowCurrencyAmounts(DataGridViewRow row)
        {
            if (row == null || row.IsNewRow || _isCalculatingGridAmounts) return;

            try
            {
                _isCalculatingGridAmounts = true;
                CalculateGridRowCurrencyAmountsInternal(row);
            }
            finally
            {
                _isCalculatingGridAmounts = false;
            }
        }

        /// <summary>
        /// الدالة الداخلية التنفيذية لتطبيق منطق العملات (1 للمحلي و 0 للأجنبي) داخل الجدول
        /// </summary>
        private void CalculateGridRowCurrencyAmountsInternal(DataGridViewRow row)
        {
            if (row == null || row.IsNewRow) return;

            decimal amount = GetGridDecimalValue(row.Cells[colAmount.Name].Value);
            int currencyId = GetGridIntValue(row.Cells[colCurrency.Name].Value);

            CurrencyLookupModel? currency = _currencyLookups.FirstOrDefault(x => x.Currency_ID == currencyId);
            if (currency == null) return;

            // 1️⃣ حالة العملة المحلية (الريال اليمني)
            if (currency.Is_Local_Currency == true || currency.Currency_Code == "YER")
            {
                row.Cells[colExchangeRate.Name].Value = 1m;
                row.Cells[colLocalAmount.Name].Value = amount;

                // 🎯 التصفير الفوري والمباشر لخانة الأجنبي في سطر الجدول بناءً على طلبك
                row.Cells[colForeignAmount.Name].Value = 0m;
            }
            // 0️⃣ حالة العملة الأجنبية (سعودي أو دولار)
            else
            {
                decimal exchangeRate = NormalizeExchangeRate(GetGridDecimalValue(row.Cells[colExchangeRate.Name].Value));

                // احتساب المبلغ المحلي: المبلغ * سعر الصرف
                decimal localAmount = decimal.Round(amount * exchangeRate, 2, MidpointRounding.AwayFromZero);

                row.Cells[colExchangeRate.Name].Value = exchangeRate;
                row.Cells[colForeignAmount.Name].Value = amount; // الأجنبي يساوي الرئيسي المدخل
                row.Cells[colLocalAmount.Name].Value = localAmount;
            }
        }

        /// <summary>
        /// دالة تقوم باحتساب وتحديث الإجماليات النهائية وعرضها على واجهة المستخدم.
        /// وظيفته الحسابية:
        /// 1. جمع المبالغ المحلية (colLocalAmount) لجميع الصفوف ووضع الناتج في الحقل (txtTotalAmount).
        /// 2. جمع المبالغ الأجنبية (colForeignAmount) لجميع الصفوف ووضع الناتج في الحقل (txtTotalForeignAmount).
        /// 3. طرح مجموع المبالغ المحلية للصفوف من القيمة الإجمالية المدخلة للسند (numLocalAmount) لاستخراج الفارق وعرضه في (txtDifference).
        /// </summary>
        private void UpdateVoucherTotals()
        {
            decimal totalLocalAmount = 0m;
            decimal totalForeignAmount = 0m;

            // حلقة تكرارية لجمع قيم الخلايا من كافة الصفوف النشطة بالجدول
            foreach (DataGridViewRow row in dgvVoucherDetails.Rows)
            {
                if (row.IsNewRow) continue;
                totalLocalAmount += GetGridDecimalValue(row.Cells[colLocalAmount.Name].Value);
                totalForeignAmount += GetGridDecimalValue(row.Cells[colForeignAmount.Name].Value);
            }

            // تقريب القيم الإجمالية لخانتين عشريتين
            totalLocalAmount = decimal.Round(totalLocalAmount, 2);
            totalForeignAmount = decimal.Round(totalForeignAmount, 2);

            // جلب قيمة مبلغ السند الرئيسي من حقل الإدخال وتقريبها
            decimal receiptLocalAmount = decimal.Round(numLocalAmount.Value, 2);
            // احتساب الفارق: إجمالي السند - مجموع تفاصيل الصفوف المحلية
            decimal difference = decimal.Round(receiptLocalAmount - totalLocalAmount, 2);

            // عرض الإجماليات والفروقات مقربة ومنسقة بتنسيق الرقم القياسي المالي "N2"
            txtTotalAmount.Text = totalLocalAmount.ToString("N2");
            txtTotalForeignAmount.Text = totalForeignAmount.ToString("N2");
            txtDifference.Text = difference.ToString("N2");
        }

        #endregion

        private void dgvVoucherDetails_EditingControlShowing(object? sender, DataGridViewEditingControlShowingEventArgs e)
        {
            // ✅ تعديل اسم العمود هنا إلى colAccountCode ليطابق التصميم تماماً
            if (dgvVoucherDetails.CurrentCell == null || e.Control is not ComboBox combo)
            {
                return;
            }

            string columnName = dgvVoucherDetails.CurrentCell.OwningColumn.Name;
            if (columnName == "colAccountCode")
            {
                combo.DropDownStyle = ComboBoxStyle.DropDown;
                combo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                combo.AutoCompleteSource = AutoCompleteSource.ListItems;
                combo.KeyDown -= ComboAccount_KeyDown;
                combo.KeyDown += ComboAccount_KeyDown;
            }
            else if (columnName == "colCostCenter")
            {
                combo.KeyDown -= ComboCostCenter_KeyDown;
                combo.KeyDown += ComboCostCenter_KeyDown;
            }
        }

        private void ComboAccount_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F9 && dgvVoucherDetails.CurrentCell != null)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                OpenAccountLookupForm(dgvVoucherDetails.CurrentCell.RowIndex);
                return;
            }

            if (sender is ComboBox combo && (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back))
            {
                e.Handled = true;

                combo.SelectedIndex = -1;
                combo.Text = string.Empty;

                if (dgvVoucherDetails.CurrentCell != null)
                {
                    int rowIndex = dgvVoucherDetails.CurrentCell.RowIndex;
                    int columnIndex = dgvVoucherDetails.CurrentCell.ColumnIndex;

                    // تصفير الخلية الحالية
                    dgvVoucherDetails.Rows[rowIndex].Cells[columnIndex].Value = DBNull.Value;

                    // تصفير عمود اسم الحساب المقابل (تأكد من اسمه برمجياً أيضاً مثل colAccountName)
                    if (dgvVoucherDetails.Columns.Contains("colAccountName"))
                    {
                        dgvVoucherDetails.Rows[rowIndex].Cells["colAccountName"].Value = string.Empty;
                    }

                    dgvVoucherDetails.EndEdit();
                }
            }
        }


        #region === إضافة صف جديد ===

        /// <summary>
        /// دالة تقوم بإضافة صف تفاصيل جديد لجدول السند برمجياً.
        /// وظيفته: إدراج صف جديد وتعبئته بقيم العملة الافتراضية (colCurrency)، وسعر صرفها (colExchangeRate)، وتصفير المبالغ والعملات المقابلة.
        /// </summary>
        private void AddNewVoucherDetailRow()
        {
            CurrencyLookupModel? defaultCurrency = ResolveDefaultCurrency();
            if (defaultCurrency == null) return;

            // إضافة الصف للجدول واستخراج فهرس الصف الجديد
            int rowIndex = dgvVoucherDetails.Rows.Add();
            DataGridViewRow row = dgvVoucherDetails.Rows[rowIndex];

            // تعبئة خلايا الصف الجديد بالبيانات الافتراضية
            row.Cells[colNo.Name].Value = rowIndex + 1;
            row.Cells[colCurrency.Name].Value = defaultCurrency.Currency_ID;
            row.Cells[colExchangeRate.Name].Value = NormalizeExchangeRate(defaultCurrency.Exchange_Rate);
            row.Cells[colAmount.Name].Value = 0m;
            row.Cells[colLocalAmount.Name].Value = 0m;
            row.Cells[colForeignAmount.Name].Value = 0m;
        }

        #endregion
    }
}
