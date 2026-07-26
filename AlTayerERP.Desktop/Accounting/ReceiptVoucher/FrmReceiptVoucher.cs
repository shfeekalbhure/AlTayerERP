using AlTayerERP.Desktop.Accounting.ReceiptVoucher;
using AlTayerERP.Desktop.Common;
using AlTayerERP.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// شاشة سند القبض - الملف الرئيسي.
    /// يحتوي على: المتغيرات العامة، النماذج المشتركة، الدوال المساعدة، المنطق الأساسي.
    /// </summary>
    public partial class FrmReceiptVoucher : BaseForm
    {
        #region === المتغيرات العامة ===

        // عميل HTTP لإرسال واستقبال طلبات الواجهة البرمجية (API)
        private readonly HttpClient _client = ApiService.Client;
        // الرابط الأساسي للواجهة البرمجية (API) الخاصة بالنظام
        private readonly string _baseUrl = ApiService.BaseUrl;

        // متغير لتخزين معرف السند المحدد حالياً (0 تعني سند جديد)
        private long _selectedVoucherId = 0;
        // متغيرات منطقية للتحكم في حالة النظام ومنع التكرار اللانهائي للأحداث أثناء التحميل أو الحساب
        private bool _isLoading = false;
        private bool _isCalculatingAmounts = false;
        private bool _isCalculatingGridAmounts = false;
        private bool _isSynchronizingReference = false;

        // حالة السند المحملة للتحكم الصحيح في دورة المراجعة والاعتماد والترحيل.
        private byte _currentReviewStatus = 0;
        private byte _currentApprovalStatus = 0;
        private string _loadedVoucherBranchId = string.Empty;
        private int _loadedFiscalYearId = 0;
        private string? _loadedPartyId;
        private string _loadedReceivedFromName = string.Empty;
        private bool _isCrossContextVoucher = false;
        private readonly Label _lblReviewStatus = new Label();
        private readonly ToolTip _workflowToolTip = new ToolTip();
        private readonly string _voucherTypeCode;
        private readonly string _voucherCaption;

        // قوائم لتخزين بيانات العملات والحسابات المسترجعة من قاعدة البيانات
        private List<CurrencyLookupModel> _currencyLookups = new();
        private List<AccountLookupModel> _accountLookups = new();
        // قوائم مرجعية تستخدمها شاشات الاستعلام باختصار F9.
        private List<CashBoxLookupModel> _cashBoxLookups = new();
        private List<CostCenterLookupModel> _costCenterLookups = new();

        #endregion

        #region === المشيد ===

        // مشيد الشاشة الرئيسي المسؤول عن تهيئة المكونات وتسجيل الأحداث وإعداد الخصائص
        public FrmReceiptVoucher() : this("RECEIPT", "سند القبض")
        {
        }

        /// <summary>
        /// قاعدة موحدة لسندات القبض والصرف والقيد اليومي. نوع السند يحدد من الشاشة
        /// ولا يمكن للمستخدم تغييره يدوياً أثناء الإدخال.
        /// </summary>
        protected FrmReceiptVoucher(string voucherTypeCode, string voucherCaption)
        {
            _voucherTypeCode = voucherTypeCode?.Trim().ToUpperInvariant()
                ?? throw new ArgumentNullException(nameof(voucherTypeCode));
            _voucherCaption = voucherCaption?.Trim()
                ?? throw new ArgumentNullException(nameof(voucherCaption));

            InitializeComponent();
            ApplyBaseFormStyle();
            Text = _voucherCaption;
            RegisterEvents();
            ConfigureScreen();
        }

        #endregion

        #region === تسجيل الأحداث ===

        // دالة مخصصة لربط الأحداث بالدوال الخاصة بها والتأكد من عدم تكرار الربط
        private void RegisterEvents()
        {
            // ربط حدث تحميل الشاشة
            Load -= FrmReceiptVoucher_Load;
            Load += FrmReceiptVoucher_Load;

            // ربط حدث النقر على زر "جديد"
            btnNew.Click -= btnNew_Click;
            btnNew.Click += btnNew_Click;

            // ربط حدث النقر على زر "حفظ"
            btnSave.Click -= btnSave_Click;
            btnSave.Click += btnSave_Click;

            // ربط حدث النقر على زر "بحث"
            btnSearch.Click -= btnSearch_Click;
            btnSearch.Click += btnSearch_Click;

            // ربط حدث النقر على زر "إغلاق"
            btnClose.Click -= btnClose_Click;
            btnClose.Click += btnClose_Click;

            // ربط حدث تغير العملة المحددة في القائمة المنسدلة
            cmbCurrency.SelectedIndexChanged -= cmbCurrency_SelectedIndexChanged;
            cmbCurrency.SelectedIndexChanged += cmbCurrency_SelectedIndexChanged;

            // ربط حدث تغير قيمة المبلغ الرئيسي
            numAmount.ValueChanged -= numAmount_ValueChanged;
            numAmount.ValueChanged += numAmount_ValueChanged;

            // ربط حدث تغير قيمة سعر الصرف
            numExchangeRate.ValueChanged -= numExchangeRate_ValueChanged;
            numExchangeRate.ValueChanged += numExchangeRate_ValueChanged;

            // حقلا رقم المرجع يعرضان نفس قيمة قاعدة البيانات؛ نبقيهما متزامنين
            // حتى لا تضيع قيمة أحدهما عند الحفظ.
            txtReference.TextChanged -= txtReference_TextChanged;
            txtReference.TextChanged += txtReference_TextChanged;
            txtReferenceNo.TextChanged -= txtReferenceNo_TextChanged;
            txtReferenceNo.TextChanged += txtReferenceNo_TextChanged;

            // اختصار F9 يفتح شاشة استعلام للصندوق ومركز التكلفة بدلاً من البحث اليدوي فقط.
            RegisterLookupShortcutEvents();

            // استدعاء دالة لتسجيل أحداث العمليات الإضافية على السند
            RegisterVoucherActionEvents();

            KeyDown -= FrmReceiptVoucher_KeyDown;
            KeyDown += FrmReceiptVoucher_KeyDown;
        }

        #endregion

        #region === إعداد الشاشة ===

        // دالة لضبط الخصائص الافتراضية لعناصر الواجهة (مثل القراءة فقط والاتجاه)
        private void ConfigureScreen()
        {
            KeyPreview = true; // تفعيل التقاط الأحداث من لوحة المفاتيح على مستوى النموذج
            RightToLeft = RightToLeft.Yes; // ضبط الاتجاه من اليمين إلى اليسار
            RightToLeftLayout = true; // ضبط تخطيط الواجهة ليدعم اللغة العربية

            // جعل الحقول التعريفية وحقول النظام للقراءة فقط لحمايتها من التعديل اليدوي
            txtVoucherNo.ReadOnly = true;
            txtCreatedBy.ReadOnly = true;
            txtCreatedDate.ReadOnly = true;
            txtUpdatedBy.ReadOnly = true;
            txtUpdatedDate.ReadOnly = true;
            txtTotalAmount.ReadOnly = true;
            txtTotalForeignAmount.ReadOnly = true;
            txtDifference.ReadOnly = true;
            txtJournalNo.ReadOnly = true;
            chkPosted.Enabled = false;

            // زرا الاستيراد والتصدير غير منفذين في هذه الشاشة؛ نستفيد من مكانهما
            // لإظهار عمليتي المراجعة الفعليتين ونخفي الأزرار غير الجاهزة.
            btnImport.Text = "تمت المراجعة";
            btnImport.Location = new System.Drawing.Point(350, 12);
            btnImport.Size = new System.Drawing.Size(90, 29);
            btnExport.Text = "إعادة للتصحيح";
            btnExport.Location = new System.Drawing.Point(443, 12);
            btnExport.Size = new System.Drawing.Size(110, 29);
            btnAttachments.Visible = true;
            button1.Visible = false;

            ConfigureReceiptToolbar();

            _lblReviewStatus.AutoSize = true;
            _lblReviewStatus.Location = new System.Drawing.Point(1278, 12);
            _lblReviewStatus.Text = "المراجعة: غير مراجع";
            _lblReviewStatus.ForeColor = System.Drawing.Color.DarkRed;
            pnlTotals.Controls.Add(_lblReviewStatus);

            // إعداد عناصر التحكم الرقمية وشبكة البيانات وحقول المبالغ
            ConfigureNumericControls();
            ConfigureVoucherGrid();
            ConfigureHeaderAmountFields();
        }

        /// <summary>
        /// يوحد شريط أوامر سند القبض: مقاسات وألوان ومحاذاة RTL ثابتة.
        /// يعاد ترتيبه تلقائياً عند تغيير حجم نافذة الشاشة.
        /// </summary>
        private void ConfigureReceiptToolbar()
        {
            pnlToolbar.BackColor = System.Drawing.Color.FromArgb(245, 248, 252);
            pnlToolbar.Padding = new Padding(12, 8, 12, 8);
            pnlToolbar.Resize -= pnlToolbar_Resize;
            pnlToolbar.Resize += pnlToolbar_Resize;

            ApplyReceiptToolbarLayout();
        }

        private void pnlToolbar_Resize(object? sender, EventArgs e) =>
            ApplyReceiptToolbarLayout();

        private void ApplyReceiptToolbarLayout()
        {
            if (pnlToolbar.ClientSize.Width <= 0)
            {
                return;
            }

            var orderedButtons = new[]
            {
                btnNew, btnSave, btnEdit, btnDelete, btnPrint, btnSearch, btnRefresh,
                btnImport, btnExport, btnApprove, btnCancelApprove, btnPost, btnUnPost,
                btnViewJournalEntry, btnAttachments, btnUndo
            };

            int right = pnlToolbar.ClientSize.Width - 12;
            const int top = 8;
            const int height = 34;
            const int gap = 4;

            pnlToolbar.SuspendLayout();
            try
            {
                foreach (Button button in orderedButtons)
                {
                    bool wideButton = button == btnExport || button == btnCancelApprove ||
                        button == btnUnPost || button == btnViewJournalEntry;
                    int width = wideButton ? 98 : 78;

                    right -= width;
                    button.Location = new System.Drawing.Point(right, top);
                    button.Size = new System.Drawing.Size(width, height);
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderSize = 0;
                    button.ForeColor = System.Drawing.Color.White;
                    button.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
                    button.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                    button.UseVisualStyleBackColor = false;
                    button.BackColor = GetReceiptToolbarColor(button);
                    right -= gap;
                }
            }
            finally
            {
                pnlToolbar.ResumeLayout();
            }
        }

        private static System.Drawing.Color GetReceiptToolbarColor(Button button)
        {
            if (button == btnNew) return System.Drawing.Color.FromArgb(37, 99, 235);
            if (button == btnSave) return System.Drawing.Color.FromArgb(22, 135, 79);
            if (button == btnEdit) return System.Drawing.Color.FromArgb(217, 119, 6);
            if (button == btnDelete || button == btnUnPost) return System.Drawing.Color.FromArgb(198, 40, 40);
            if (button == btnPrint) return System.Drawing.Color.FromArgb(109, 40, 217);
            if (button == btnSearch) return System.Drawing.Color.FromArgb(14, 116, 144);
            if (button == btnRefresh) return System.Drawing.Color.FromArgb(71, 85, 105);
            if (button == btnImport) return System.Drawing.Color.FromArgb(91, 33, 182);
            if (button == btnExport || button == btnCancelApprove) return System.Drawing.Color.FromArgb(194, 65, 12);
            if (button == btnApprove || button == btnPost) return System.Drawing.Color.FromArgb(5, 120, 87);
            if (button == btnViewJournalEntry) return System.Drawing.Color.FromArgb(29, 78, 216);
            if (button == btnAttachments) return System.Drawing.Color.FromArgb(3, 105, 161);
            return System.Drawing.Color.FromArgb(100, 116, 139);
        }

        // دالة لضبط حدود وخصائص حقول الإدخال الرقمية الخاصة بالمبالغ وسعر الصرف
        private void ConfigureNumericControls()
        {
            numAmount.DecimalPlaces = 2;
            numAmount.Minimum = 0m;
            numAmount.Maximum = 999999999999m;
            numAmount.Increment = 1m;
            numAmount.ThousandsSeparator = true;

            // إعداد حقل سعر الصرف (عدد الخانات العشرية، الحدود الدنيا والعليا، قيمة الزيادة، والقيمة الافتراضية)
            numExchangeRate.DecimalPlaces = 6;
            numExchangeRate.Minimum = 0.000001m;
            numExchangeRate.Maximum = 999999999999m;
            numExchangeRate.Increment = 0.000001m;
            numExchangeRate.Value = 1m;
            numExchangeRate.ThousandsSeparator = true;

            // إعداد حقل المبلغ بالعملة الأجنبية
            numForeignAmount.DecimalPlaces = 2;
            numForeignAmount.Minimum = 0m;
            numForeignAmount.Maximum = 999999999999m;
            numForeignAmount.Increment = 1m;
            numForeignAmount.ThousandsSeparator = true;

            // إعداد حقل المبلغ بالعملة المحلية (للقراءة فقط بشكل افتراضي)
            numLocalAmount.DecimalPlaces = 2;
            numLocalAmount.Minimum = 0m;
            numLocalAmount.Maximum = 999999999999m;
            numLocalAmount.Increment = 1m;
            numLocalAmount.ThousandsSeparator = true;
            numLocalAmount.ReadOnly = true;
            numLocalAmount.Enabled = true;
        }

        // دالة لتحديد حالة الحقول الرقمية في الترويسة (من منها متاح للإدخال ومن للقراءة فقط)
        private void ConfigureHeaderAmountFields()
        {
            numExchangeRate.ReadOnly = true;
            numLocalAmount.ReadOnly = true;
            numForeignAmount.ReadOnly = true;
            numAmount.ReadOnly = false; // حقل المبلغ الرئيسي هو المتاح للإدخال بشكل أساسي
        }

        #endregion

        #region === أحداث المبالغ ===

        // حدث يتم استدعاؤه عند تغيير العملة المحددة
        private void cmbCurrency_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isLoading || _isCalculatingAmounts) return;

            CurrencyLookupModel? selectedCurrency = GetSelectedCurrency();
            if (selectedCurrency == null) return;

            _isCalculatingAmounts = true;
            try
            {
                bool isLocal = selectedCurrency.Is_Local_Currency ||
                    selectedCurrency.Currency_Code.Equals("YER", StringComparison.OrdinalIgnoreCase);
                SetNumericValueSafe(
                    numExchangeRate,
                    isLocal ? 1m : NormalizeExchangeRate(selectedCurrency.Exchange_Rate));
            }
            finally
            {
                _isCalculatingAmounts = false;
            }

            CalculateHeaderCurrencyAmounts();
        }

        // حدث يتم استدعاؤه عند تغيير قيمة المبلغ الرئيسي
        private void numAmount_ValueChanged(object? sender, EventArgs e)
        {
            if (_isLoading || _isCalculatingAmounts) return;
            CalculateHeaderCurrencyAmounts(); // إعادة حساب المبالغ بناءً على القيمة الجديدة
        }

        // حدث يتم استدعاؤه عند تغيير قيمة سعر الصرف
        private void numExchangeRate_ValueChanged(object? sender, EventArgs e)
        {
            if (_isLoading || _isCalculatingAmounts) return;
            CalculateHeaderCurrencyAmounts();
        }

        private void txtReference_TextChanged(object? sender, EventArgs e)
        {
            SynchronizeReferenceText(txtReference, txtReferenceNo);
        }

        private void txtReferenceNo_TextChanged(object? sender, EventArgs e)
        {
            SynchronizeReferenceText(txtReferenceNo, txtReference);
        }

        private void SynchronizeReferenceText(TextBox source, TextBox target)
        {
            if (_isLoading || _isSynchronizingReference || target.Text == source.Text)
            {
                return;
            }

            try
            {
                _isSynchronizingReference = true;
                target.Text = source.Text;
            }
            finally
            {
                _isSynchronizingReference = false;
            }
        }

        #endregion


        private void CalculateHeaderCurrencyAmounts()
        {
            if (_isLoading || _isCalculatingAmounts) return;

            // جلب بيانات العملة المحددة حالياً
            CurrencyLookupModel? selectedCurrency = GetSelectedCurrency();
            if (selectedCurrency == null) return;

            try
            {
                _isCalculatingAmounts = true; // رفع الراية لمنع التداخل اللانهائي للأحداث
                decimal enteredAmount = numAmount.Value;

                // ✅ تحويل حالة العملة إلى رقم (1 للمحلي، 0 للأجنبي) ليفحصها البرنامج بدقة
                int isLocal = selectedCurrency.Is_Local_Currency ? 1 : 0;

                if (isLocal == 1 || selectedCurrency.Currency_Code == "YER")
                {
                    // 1️⃣ إذا كانت القيمة تساوي 1 (عملة محلية - ريال يمني)
                    SetNumericValueSafe(numExchangeRate, 1m);
                    SetNumericValueSafe(numLocalAmount, enteredAmount);
                    SetNumericValueSafe(numForeignAmount, 0m); // تصفير حقل العملة الأجنبية فوراً

                    numForeignAmount.ReadOnly = true;
                    numExchangeRate.ReadOnly = true;
                }
                else if (isLocal == 0)
                {
                    // 0️⃣ إذا كانت القيمة تساوي 0 (عملة أجنبية - دولار أو سعودي)
                    decimal exchangeRate = NormalizeExchangeRate(numExchangeRate.Value);
                    SetNumericValueSafe(numExchangeRate, exchangeRate);
                    SetNumericValueSafe(numForeignAmount, enteredAmount); // الأجنبي يساوي الرئيسي

                    decimal localAmount = decimal.Round(enteredAmount * exchangeRate, 2);
                    SetNumericValueSafe(numLocalAmount, localAmount); // المحلي = حاصل الضرب

                    numForeignAmount.ReadOnly = true;
                    numExchangeRate.ReadOnly = false;
                }
            }
            finally
            {
                _isCalculatingAmounts = false; // خفض الراية
            }

            UpdateVoucherTotals(); // تحديث إجماليات السند بالكامل
        }
        #region === حساب المبالغ ===

        // الدالة المسؤولة عن احتساب المبالغ المحلية والأجنبية في ترويسة السند طبقاً للعملة المحددة وسعر صرفها



        #endregion

        #region === دوال العملة المساعدة ===

        // دالة لاستخراج كائن العملة المحدد حالياً من القائمة المنسدلة بشتى الطرق الممكنة (كائن مباشر أو عن طريق القيمة)
        private CurrencyLookupModel? GetSelectedCurrency()
        {
            if (cmbCurrency.SelectedItem is CurrencyLookupModel selectedCurrency)
                return selectedCurrency;

            if (cmbCurrency.SelectedValue == null) return null;

            if (cmbCurrency.DataSource is IEnumerable<CurrencyLookupModel> currencies)
            {
                string selectedCurrencyId = cmbCurrency.SelectedValue.ToString() ?? string.Empty;
                return currencies.FirstOrDefault(c => c.Currency_ID.ToString() == selectedCurrencyId);
            }

            return null;
        }

        // دالة لتعيين العملة الافتراضية للنظام داخل القائمة المنسدلة عند فتح شاشة جديدة
        private void SetDefaultCurrency()
        {
            if (cmbCurrency.Items.Count == 0)
            {
                SetNumericValueSafe(numExchangeRate, 1m);
                return;
            }

            CurrencyLookupModel? defaultCurrency = null;
            foreach (object item in cmbCurrency.Items)
            {
                if (item is CurrencyLookupModel currency && currency.Is_Default)
                {
                    defaultCurrency = currency;
                    break;
                }
            }

            try
            {
                _isCalculatingAmounts = true;
                if (defaultCurrency != null)
                    cmbCurrency.SelectedValue = defaultCurrency.Currency_ID;
                else
                    cmbCurrency.SelectedIndex = 0;
            }
            finally
            {
                _isCalculatingAmounts = false;
            }

            CalculateHeaderCurrencyAmounts();
        }

        // دالة للتأكد من أن سعر الصرف ليس صفراً أو سالباً، وفي حال كان كذلك يتم إرجاع القيمة الاحتياطية (1)
        private static decimal NormalizeExchangeRate(decimal exchangeRate, decimal fallback = 1m)
        {
            return exchangeRate <= 0m ? fallback : exchangeRate;
        }


        // دالة لتعيين قيمة رقمية لعنصر تحكم NumericUpDown بشكل آمن دون تخطي حدوده الدنيا أو العليا لتفادي الأخطاء البرمجية
        private static void SetNumericValueSafe(NumericUpDown control, decimal value)
        {
            if (value < control.Minimum) value = control.Minimum;
            if (value > control.Maximum) value = control.Maximum;
            control.Value = value;
        }

        // دالة للبحث عن العملة الافتراضية أو العملة المحلية ضمن القائمة المخزنة، أو إرجاع أول عملة متاحة
        private CurrencyLookupModel? ResolveDefaultCurrency()
        {
            return _currencyLookups.FirstOrDefault(x => x.Is_Default)
                ?? _currencyLookups.FirstOrDefault(x => x.Is_Local_Currency)
                ?? _currencyLookups.FirstOrDefault();
        }

        #endregion

        #region === دوال مساعدة موحدة ===

        // دالة آمنة لتحويل أي قيمة مستخرجة من خلايا شبكة البيانات (Grid) إلى قيمة عشرية Decimal مع معالجة نصوص الفواصل وغيرها
        private static decimal GetGridDecimalValue(object? value)
        {
            if (value == null || value == DBNull.Value) return 0m;
            if (value is decimal d) return d;
            if (value is int i) return i;
            if (value is long l) return l;

            string text = value.ToString()?.Trim() ?? string.Empty;
            text = text.Replace(",", string.Empty).Replace("٬", string.Empty);

            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal result))
                return result;

            return decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out result) ? result : 0m;
        }

        // دالة آمنة لتحويل قيمة من خلايا الشبكة إلى عدد صحيح int
        private static int GetGridIntValue(object? value)
        {
            if (value == null || value == DBNull.Value) return 0;
            return int.TryParse(value.ToString(), out int result) ? result : 0;
        }

        // دالة لمحاولة تحويل القيمة إلى عدد صحيح يقبل القيمة الفارغة (Nullable Int)
        private static int? TryConvertNullableInt(object? value)
        {
            if (value == null || value == DBNull.Value) return null;
            return int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int result) ? result : null;
        }

        // دالة لإرجاع قيمة فارغة Null إذا كان النص يحتوي على مسافات فقط، وإلا تقوم بعمل Trim للنص
        private static string? NullIfWhiteSpace(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        // دالة للتأكد من بقاء القيمة الرقمية ضمن النطاق المسموح به لعنصر تحكم رقمي معين وإرجاع القيمة المصححة
        private static decimal LimitNumericValue(NumericUpDown control, decimal value)
        {
            if (value < control.Minimum) return control.Minimum;
            if (value > control.Maximum) return control.Maximum;
            return value;
        }

        // دالة لتعيين قيمة القائمة المنسدلة ComboBox بشكل آمن ومعالجة القيم الفارغة
        private static void SetComboBoxValue(ComboBox comboBox, object? value)
        {
            if (value == null)
            {
                comboBox.SelectedIndex = -1;
                return;
            }
            if (comboBox.DataSource != null || comboBox.Items.Count > 0)
                comboBox.SelectedValue = value;
        }

        // دالة لتعيين قيمة أداة اختيار التاريخ DateTimePicker بشكل آمن مع التحقق من تواجده ضمن الحدود المسموحة للأداة
        private static void SetDatePickerValue(DateTimePicker picker, DateTime? date)
        {
            if (date.HasValue && date.Value >= picker.MinDate && date.Value <= picker.MaxDate)
                picker.Value = date.Value;
        }

        // دالة لتنسيق التاريخ والوقت ليظهر بشكل نصي موحد بصيغة (السنة/الشهر/اليوم ساعة:دقيقة ص/م)
        private static string FormatDateTime(DateTime? dateTime)
        {
            return dateTime?.ToString("yyyy/MM/dd hh:mm tt") ?? string.Empty;
        }

        // دالة آمنة لتعيين نص داخل مربع نص TextBox مع التحقق من أن الكائن غير فارغ لمنع الأخطاء
        private static void SafeSetText(TextBox? textBox, string text)
        {
            if (textBox != null) textBox.Text = text;
        }

        // دالة لتعيين قيمة لخلية محددة داخل سطر وعمود معين في شبكة البيانات DataGridView
        private static void SetCellValue(DataGridViewRow row, DataGridViewColumn column, object? value)
        {
            if (column == null) return;
            int columnIndex = column.Index;
            if (columnIndex < 0 || columnIndex >= row.Cells.Count) return;
            row.Cells[columnIndex].Value = value ?? string.Empty;
        }

        // دالة آمنة إضافية للتحقق من وجود العمود قبل استدعاء دالة تعيين قيمة الخلية
        private static void SafeSetCellValue(DataGridViewRow row, DataGridViewColumn? column, object? value)
        {
            if (column == null) return;
            SetCellValue(row, column, value);
        }

        #endregion

        #region === أحداث مصمم الشاشة ===

        // أحداث فارغة تم إنشاؤها تلقائياً بواسطة مصمم النموذج (Designer) لعناصر واجهة مختلفة
        private void panel1_Paint(object? sender, PaintEventArgs e) { }
        private void flowLayoutPanel1_Paint(object? sender, PaintEventArgs e) { }
        private void lblCompanyName_Click(object? sender, EventArgs e) { }
        private void pnlUserInfo_Paint(object? sender, PaintEventArgs e) { }
        private void label36_Click(object? sender, EventArgs e) { }
        private void pnlToolbar_Paint(object? sender, PaintEventArgs e) { }
        private void dgvVoucherDetails_CellContentClick(object? sender, DataGridViewCellEventArgs e) { }

        private void groupBox1_Enter(object sender, EventArgs e) { }

        #endregion

        #region === الإغلاق ===

        // حدث النقر على زر الإغلاق لإنهاء الشاشة الحالية وحذفها من الذاكرة
        private void btnClose_Click(object? sender, EventArgs e)
        {
            Close();
        }

        #region === عرض قيد اليومية ===

        // حدث النقر على زر "عرض قيد اليومية" المرتبط بالسند الحالي لفتح شاشة استعلام القيود
        private void btnViewJournalEntry_Click(object? sender, EventArgs e)
        {
            string voucherNo = txtVoucherNo.Text.Trim();

            // التحقق من أن حقل رقم السند ليس فارغاً (أي أن هناك سند معروض بالفعل)
            if (string.IsNullOrWhiteSpace(voucherNo))
            {
                MessageBox.Show(
                    "يجب فتح أو حفظ السند أولًا.",
                    "تنبيه",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            // إنشاء وفتح شاشة الاستعلام عن قيد اليومية وتمرير رقم السند لها كشاشة حوارية (Dialog)
            using var form = new FrmJournalEntryInquiry(voucherNo);
            form.ShowDialog(this);
        }

        #endregion

        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            // فتح شاشة البحث كشاشة منبثقة (Modal) تمنع التعديل خلفها حتى تختار الحساب

            using (Form2 lookup = new Form2())
            {
                lookup.ShowDialog();
            }

         //   using (FrmAccountLookup lookup = new FrmAccountLookup())
         //   {
         //       lookup.ShowDialog();
         //   }
        }
    }
}
