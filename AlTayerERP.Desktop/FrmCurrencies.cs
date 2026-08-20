using AlTayerERP.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// شاشة إدارة العملات وأسعار الصرف.
    /// </summary>
    public partial class FrmCurrencies : Form
    {
        #region متغيرات الشاشة

        private readonly HttpClient _client = ApiService.Client;
        private readonly string _baseUrl = ApiService.BaseUrl;

        private int _selectedCurrencyId;

        private List<CurrencyModel> _currenciesCache = new();

        private bool _isBinding;

        #endregion

        #region Constructor

        public FrmCurrencies()
        {
            InitializeComponent();

            // تطبيق الثيم العربي الموحد والاختصارات على الشاشة القديمة.
RegisterEvents();
        }

        #endregion

        #region تسجيل الأحداث

        private void RegisterEvents()
        {
            Load -= FrmCurrencies_Load;
            Load += FrmCurrencies_Load;

            btnSave.Click -= btnSave_Click;
            btnSave.Click += btnSave_Click;

            btnEdit.Click -= btnEdit_Click;
            btnEdit.Click += btnEdit_Click;

            btnDelete.Click -= btnDelete_Click;
            btnDelete.Click += btnDelete_Click;

            btnNew.Click -= btnNew_Click;
            btnNew.Click += btnNew_Click;

            btnRefresh.Click -= btnRefresh_Click;
            btnRefresh.Click += btnRefresh_Click;

            btnSearch.Click -= btnSearch_Click;
            btnSearch.Click += btnSearch_Click;

            btnClose.Click -= btnClose_Click;
            btnClose.Click += btnClose_Click;

            dgvCurrencies.CellClick -= dgvCurrencies_CellClick;
            dgvCurrencies.CellClick += dgvCurrencies_CellClick;

            chkIsLocalCurrency.CheckedChanged -= chkIsLocalCurrency_CheckedChanged;
            chkIsLocalCurrency.CheckedChanged += chkIsLocalCurrency_CheckedChanged;
        }

        #endregion

        #region تحميل الشاشة

        private async void FrmCurrencies_Load(object? sender, EventArgs e)
        {
            try
            {
                _isBinding = true;

                SetupNumericControls();
                SetupCurrenciesGrid();

                await LoadCurrenciesAsync();

                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _isBinding = false;
            }
        }

        #endregion

        #region إعداد حقول الأرقام

        private void SetupNumericControls()
        {
            numDecimalPlaces.Minimum = 0;
            numDecimalPlaces.Maximum = 6;

            SetupExchangeRateControl(numExchangeRate);
            SetupExchangeRateControl(numMinExchangeRate);
            SetupExchangeRateControl(numMaxExchangeRate);
        }

        private static void SetupExchangeRateControl(NumericUpDown control)
        {
            control.DecimalPlaces = 6;
            control.Minimum = 0.000001M;
            control.Maximum = 999999999999M;
            control.ThousandsSeparator = true;
        }

        #endregion

        #region تجهيز الجدول

        private void SetupCurrenciesGrid()
        {
            dgvCurrencies.Columns.Clear();

            dgvCurrencies.AllowUserToAddRows = false;
            dgvCurrencies.AllowUserToDeleteRows = false;
            dgvCurrencies.ReadOnly = true;
            dgvCurrencies.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            dgvCurrencies.MultiSelect = false;
            dgvCurrencies.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            dgvCurrencies.Columns.Add(
                "Currency_ID",
                "رقم");

            dgvCurrencies.Columns.Add(
                "Currency_Code",
                "الكود");

            dgvCurrencies.Columns.Add(
                "Currency_Name_AR",
                "اسم العملة");

            dgvCurrencies.Columns.Add(
                "Currency_Name_EN",
                "الاسم الإنجليزي");

            dgvCurrencies.Columns.Add(
                "Currency_Symbol",
                "الرمز");

            dgvCurrencies.Columns.Add(
                "Decimal_Places",
                "المنازل");

            dgvCurrencies.Columns.Add(
                "Exchange_Rate",
                "سعر الصرف");

            dgvCurrencies.Columns.Add(
                "Min_Exchange_Rate",
                "أقل سعر");

            dgvCurrencies.Columns.Add(
                "Max_Exchange_Rate",
                "أعلى سعر");

            dgvCurrencies.Columns.Add(
            new DataGridViewCheckBoxColumn
            {
                Name = "Is_Local_Currency",
                HeaderText = "العملة المحلية"
             });

            dgvCurrencies.Columns.Add(
                new DataGridViewCheckBoxColumn
                {
                    Name = "Is_Default",
                    HeaderText = "العملة الافتراضية"
                });

            dgvCurrencies.Columns.Add(
                new DataGridViewCheckBoxColumn
                {
                    Name = "Is_Active",
                    HeaderText = "نشطة"
                });

            dgvCurrencies.Columns["Currency_ID"].Visible = false;

            dgvCurrencies.Columns["Exchange_Rate"]
                .DefaultCellStyle.Format = "N6";

            dgvCurrencies.Columns["Min_Exchange_Rate"]
                .DefaultCellStyle.Format = "N6";

            dgvCurrencies.Columns["Max_Exchange_Rate"]
                .DefaultCellStyle.Format = "N6";
        }

        #endregion

        #region تحميل العملات

        private async Task LoadCurrenciesAsync()
        {
            try
            {
                string companyId = CurrentSession.Company_ID;

                var data =
                    await _client.GetFromJsonAsync<List<CurrencyModel>>(
                        $"{_baseUrl}Currencies?companyId={companyId}");

                _currenciesCache = data ?? new List<CurrencyModel>();

                FillCurrenciesGrid(_currenciesCache);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(
                    $"تعذر الاتصال بالخادم.\n{ex.Message}",
                    "خطأ اتصال",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "خطأ تحميل العملات",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        #endregion

        #region تعبئة الجدول

        private void FillCurrenciesGrid(
            IEnumerable<CurrencyModel> currencies)
        {
            dgvCurrencies.Rows.Clear();

            foreach (CurrencyModel item in currencies)
            {
                dgvCurrencies.Rows.Add(
                    item.Currency_ID,
                    item.Currency_Code,
                    item.Currency_Name_AR,
                    item.Currency_Name_EN,
                    item.Currency_Symbol,
                    item.Decimal_Places,
                    item.Exchange_Rate,
                    item.Min_Exchange_Rate,
                    item.Max_Exchange_Rate,
                    item.Is_Local_Currency,
                   item.Is_Default,
                   item.Is_Active);

            }
        }

        #endregion

        #region حفظ

        private async void btnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!ValidateCurrencyData())
                    return;

                CurrencyRequest request = BuildCurrencyRequest();

                HttpResponseMessage response =
                    await _client.PostAsJsonAsync(
                        $"{_baseUrl}Currencies",
                        request);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        "تم حفظ العملة بنجاح.",
                        "نجاح",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    await LoadCurrenciesAsync();
                    ClearForm();
                }
                else
                {
                    await ShowApiErrorAsync(response);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "خطأ أثناء الحفظ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        #endregion

        #region تعديل

        private async void btnEdit_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_selectedCurrencyId == 0)
                {
                    MessageBox.Show(
                        "اختر العملة المراد تعديلها أولاً.",
                        "تنبيه",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                if (!ValidateCurrencyData())
                    return;

                CurrencyRequest request = BuildCurrencyRequest();

                HttpResponseMessage response =
                    await _client.PutAsJsonAsync(
                        $"{_baseUrl}Currencies/{_selectedCurrencyId}",
                        request);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        "تم تعديل العملة بنجاح.",
                        "نجاح",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    await LoadCurrenciesAsync();
                    ClearForm();
                }
                else
                {
                    await ShowApiErrorAsync(response);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "خطأ أثناء التعديل",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        #endregion

        #region حذف

        private async void btnDelete_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_selectedCurrencyId == 0)
                {
                    MessageBox.Show(
                        "اختر العملة المراد حذفها أولاً.",
                        "تنبيه",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                DialogResult result = MessageBox.Show(
                    "هل تريد حذف العملة المحددة؟",
                    "تأكيد الحذف",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                    return;

                HttpResponseMessage response =
                    await _client.DeleteAsync(
                        $"{_baseUrl}Currencies/{_selectedCurrencyId}");

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        "تم حذف العملة بنجاح.",
                        "نجاح",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    await LoadCurrenciesAsync();
                    ClearForm();
                }
                else
                {
                    await ShowApiErrorAsync(response);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "خطأ أثناء الحذف",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        #endregion

        #region التحقق من البيانات

        /// <summary>
        /// التحقق من صحة بيانات العملة قبل الحفظ أو التعديل.
        ///
        /// الاسم العربي: كود العملة.
        /// الاسم البرمجي: txtCurrencyCode.
        ///
        /// الاسم العربي: اسم العملة العربي.
        /// الاسم البرمجي: txtCurrencyNameAR.
        ///
        /// الاسم العربي: سعر الصرف.
        /// الاسم البرمجي: numExchangeRate.
        ///
        /// الاسم العربي: أقل سعر صرف.
        /// الاسم البرمجي: numMinExchangeRate.
        ///
        /// الاسم العربي: أعلى سعر صرف.
        /// الاسم البرمجي: numMaxExchangeRate.
        ///
        /// الاسم العربي: العملة المحلية.
        /// الاسم البرمجي: chkIsLocalCurrency.
        ///
        /// الاسم العربي: العملة الافتراضية.
        /// الاسم البرمجي: chkIsDefault.
        /// </summary>
        private bool ValidateCurrencyData()
        {
            #region التحقق من كود العملة

            if (string.IsNullOrWhiteSpace(
                txtCurrencyCode.Text))
            {
                MessageBox.Show(
                    "أدخل كود العملة.",
                    "تنبيه",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtCurrencyCode.Focus();

                return false;
            }

            #endregion

            #region التحقق من اسم العملة العربي

            if (string.IsNullOrWhiteSpace(
                txtCurrencyNameAR.Text))
            {
                MessageBox.Show(
                    "أدخل اسم العملة العربي.",
                    "تنبيه",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtCurrencyNameAR.Focus();

                return false;
            }

            #endregion

            #region التحقق من تكرار كود العملة

            string currencyCode =
                txtCurrencyCode.Text
                    .Trim();

            bool duplicateCode =
                _currenciesCache.Any(x =>
                    x.Currency_ID != _selectedCurrencyId &&
                    string.Equals(
                        x.Currency_Code?.Trim(),
                        currencyCode,
                        StringComparison.OrdinalIgnoreCase));

            if (duplicateCode)
            {
                MessageBox.Show(
                    "كود العملة مستخدم مسبقًا.",
                    "تنبيه",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtCurrencyCode.Focus();

                return false;
            }

            #endregion

            #region التحقق من العملة الافتراضية

            /*
             * عند تحديد العملة الحالية كعملة افتراضية،
             * نتحقق من عدم وجود عملة افتراضية أخرى
             * تابعة للشركة نفسها.
             *
             * عند التعديل يتم استثناء العملة المحددة حاليًا
             * بواسطة _selectedCurrencyId.
             */
            bool anotherDefaultCurrencyExists =
                chkIsDefault.Checked &&
                _currenciesCache.Any(x =>
                    x.Currency_ID != _selectedCurrencyId &&
                    x.Is_Default);

            if (anotherDefaultCurrencyExists)
            {
                MessageBox.Show(
                    "توجد عملة افتراضية أخرى بالفعل.\n\n" +
                    "ألغِ تحديد العملة الافتراضية من العملة السابقة أولًا، " +
                    "ثم عيّن هذه العملة كعملة افتراضية.",
                    "العملة الافتراضية",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                chkIsDefault.Focus();

                return false;
            }

            #endregion

            #region قراءة أسعار الصرف

            decimal exchangeRate =
                numExchangeRate.Value;

            decimal minRate =
                numMinExchangeRate.Value;

            decimal maxRate =
                numMaxExchangeRate.Value;

            #endregion

            #region التحقق من العملة المحلية

            if (chkIsLocalCurrency.Checked)
            {
                /*
                 * العملة المحلية يجب أن يكون:
                 *
                 * سعر الصرف = 1
                 * أقل سعر صرف = 1
                 * أعلى سعر صرف = 1
                 *
                 * الدالة ApplyLocalCurrencyRules
                 * تضبط هذه القيم تلقائيًا.
                 */
                if (exchangeRate != 1m ||
                    minRate != 1m ||
                    maxRate != 1m)
                {
                    MessageBox.Show(
                        "العملة المحلية يجب أن يكون سعر صرفها مساويًا للواحد.",
                        "العملة المحلية",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    ApplyLocalCurrencyRules();

                    return false;
                }
            }
            else
            {
                #region التحقق من أسعار العملة الأجنبية

                if (minRate > maxRate)
                {
                    MessageBox.Show(
                        "أقل سعر صرف لا يمكن أن يكون أكبر من أعلى سعر صرف.",
                        "خطأ في أسعار الصرف",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    numMinExchangeRate.Focus();

                    return false;
                }

                if (exchangeRate < minRate)
                {
                    MessageBox.Show(
                        $"سعر الصرف الحالي أقل من الحد الأدنى.\n\n" +
                        $"سعر الصرف الحالي: {exchangeRate:N6}\n" +
                        $"الحد الأدنى المسموح: {minRate:N6}",
                        "خطأ في سعر الصرف",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    numExchangeRate.Focus();

                    return false;
                }

                if (exchangeRate > maxRate)
                {
                    MessageBox.Show(
                        $"سعر الصرف الحالي أكبر من الحد الأعلى.\n\n" +
                        $"سعر الصرف الحالي: {exchangeRate:N6}\n" +
                        $"الحد الأعلى المسموح: {maxRate:N6}",
                        "خطأ في سعر الصرف",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    numExchangeRate.Focus();

                    return false;
                }

                #endregion
            }

            #endregion

            return true;
        }



        #endregion

        #region العملة المحلية

        private void chkIsLocalCurrency_CheckedChanged(
            object? sender,
            EventArgs e)
        {
            if (_isBinding)
                return;

            ApplyLocalCurrencyRules();
        }

        private void ApplyLocalCurrencyRules()
        {
            bool isLocal = chkIsLocalCurrency.Checked;

            if (isLocal)
            {
                numExchangeRate.Value = 1;
                numMinExchangeRate.Value = 1;
                numMaxExchangeRate.Value = 1;
            }

            /*
             * العملة المحلية سعرها دائمًا 1.
             * نمنع تغيير السعر في شاشة تعريف العملات
             * طالما تم تحديدها كعملة محلية.
             */
            numExchangeRate.Enabled = !isLocal;
            numMinExchangeRate.Enabled = !isLocal;
            numMaxExchangeRate.Enabled = !isLocal;
        }

        #endregion

        #region جديد

        private void btnNew_Click(object? sender, EventArgs e)
        {
            ClearForm();
        }

        #endregion

        #region تحديث

        private async void btnRefresh_Click(object? sender, EventArgs e)
        {
            await LoadCurrenciesAsync();
            ClearForm();
        }

        #endregion

        #region بحث

        private void btnSearch_Click(object? sender, EventArgs e)
        {
            string searchText =
                Microsoft.VisualBasic.Interaction.InputBox(
                    "أدخل اسم العملة أو كودها:",
                    "البحث عن عملة");

            if (string.IsNullOrWhiteSpace(searchText))
            {
                FillCurrenciesGrid(_currenciesCache);
                return;
            }

            searchText = searchText.Trim();

            List<CurrencyModel> result =
                _currenciesCache
                    .Where(x =>
                        (x.Currency_Code ?? string.Empty)
                            .Contains(
                                searchText,
                                StringComparison.OrdinalIgnoreCase) ||

                        (x.Currency_Name_AR ?? string.Empty)
                            .Contains(
                                searchText,
                                StringComparison.OrdinalIgnoreCase) ||

                        (x.Currency_Name_EN ?? string.Empty)
                            .Contains(
                                searchText,
                                StringComparison.OrdinalIgnoreCase))
                    .ToList();

            FillCurrenciesGrid(result);
        }

        #endregion

        #region اختيار صف من الجدول

        private void dgvCurrencies_CellClick(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            DataGridViewRow row =
                dgvCurrencies.Rows[e.RowIndex];

            if (!int.TryParse(
                    Convert.ToString(
                        row.Cells["Currency_ID"].Value),
                    out int currencyId))
            {
                return;
            }

            CurrencyModel? item =
                _currenciesCache.FirstOrDefault(
                    x => x.Currency_ID == currencyId);

            if (item == null)
                return;

            _isBinding = true;

            try
            {
                _selectedCurrencyId = item.Currency_ID;

                txtCurrencyCode.Text = item.Currency_Code;
                txtCurrencyNameAR.Text = item.Currency_Name_AR;
                txtCurrencyNameEN.Text = item.Currency_Name_EN;
                txtCurrencySymbol.Text = item.Currency_Symbol;

                numDecimalPlaces.Value =
                    ClampValue(
                        numDecimalPlaces,
                        item.Decimal_Places);

                numExchangeRate.Value =
                    ClampValue(
                        numExchangeRate,
                        item.Exchange_Rate);

                numMinExchangeRate.Value =
                    ClampValue(
                        numMinExchangeRate,
                        item.Min_Exchange_Rate);

                numMaxExchangeRate.Value =
                    ClampValue(
                        numMaxExchangeRate,
                        item.Max_Exchange_Rate);

                chkIsLocalCurrency.Checked =
                    item.Is_Local_Currency;
                // الاسم العربي: العملة الافتراضية.
                // الاسم البرمجي: chkIsDefault.
                chkIsDefault.Checked =
                    item.Is_Default;
                chkIsActive.Checked =
                    item.Is_Active;





                txtNotes.Text =
                    item.Notes ?? string.Empty;
            }
            finally
            {
                _isBinding = false;
            }

            ApplyLocalCurrencyRules();
        }

        #endregion

        #region تنظيف الشاشة

        private void ClearForm()
        {
            _isBinding = true;

            try
            {
                _selectedCurrencyId = 0;

                txtCurrencyCode.Clear();
                txtCurrencyNameAR.Clear();
                txtCurrencyNameEN.Clear();
                txtCurrencySymbol.Clear();
                txtNotes.Clear();

                numDecimalPlaces.Value = 2;

                numExchangeRate.Value = 1;
                numMinExchangeRate.Value = 1;
                numMaxExchangeRate.Value = 1;

                chkIsLocalCurrency.Checked = false;
                chkIsDefault.Checked = false;
                chkIsActive.Checked = true;
            }
            finally
            {
                _isBinding = false;
            }

            ApplyLocalCurrencyRules();

            txtCurrencyCode.Focus();
        }

        #endregion

        #region تجهيز طلب الحفظ

        private CurrencyRequest BuildCurrencyRequest()
        {
            return new CurrencyRequest
            {
                Company_ID = CurrentSession.Company_ID,

                Currency_Code =
                    txtCurrencyCode.Text.Trim().ToUpperInvariant(),

                Currency_Name_AR =
                    txtCurrencyNameAR.Text.Trim(),

                Currency_Name_EN =
                    txtCurrencyNameEN.Text.Trim(),

                Currency_Symbol =
                    txtCurrencySymbol.Text.Trim(),

                Decimal_Places =
                    (int)numDecimalPlaces.Value,

                Exchange_Rate =
                    chkIsLocalCurrency.Checked
                        ? 1
                        : numExchangeRate.Value,

                Min_Exchange_Rate =
                    chkIsLocalCurrency.Checked
                        ? 1
                        : numMinExchangeRate.Value,

                Max_Exchange_Rate =
                    chkIsLocalCurrency.Checked
                        ? 1
                        : numMaxExchangeRate.Value,

                Is_Local_Currency =
              chkIsLocalCurrency.Checked,

                Is_Default =
                chkIsDefault.Checked,
                Is_Active =
                chkIsActive.Checked,

                Notes =
                    txtNotes.Text.Trim(),

                Created_By =
                    CurrentSession.Username,

                Updated_By =
                    CurrentSession.Username
            };
        }

        #endregion

        #region أدوات مساعدة

        private static decimal ClampValue(
            NumericUpDown control,
            decimal value)
        {
            if (value < control.Minimum)
                return control.Minimum;

            if (value > control.Maximum)
                return control.Maximum;

            return value;
        }

        private static decimal ClampValue(
            NumericUpDown control,
            int value)
        {
            return ClampValue(control, Convert.ToDecimal(value));
        }

        private static async Task ShowApiErrorAsync(
            HttpResponseMessage response)
        {
            string errorMessage =
                await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                errorMessage =
                    $"تعذر تنفيذ العملية. رمز الخطأ: " +
                    $"{(int)response.StatusCode}";
            }

            MessageBox.Show(
                errorMessage,
                "خطأ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        #endregion

        #region إغلاق الشاشة

        private void btnClose_Click(object? sender, EventArgs e)
        {
            Close();
        }

        #endregion
    }

    #region Models

    public class CurrencyModel
    {
        public int Currency_ID { get; set; }

        public string Company_ID { get; set; } = string.Empty;

        public string Currency_Code { get; set; } = string.Empty;

        public string Currency_Name_AR { get; set; } = string.Empty;

        public string Currency_Name_EN { get; set; } = string.Empty;

        public string Currency_Symbol { get; set; } = string.Empty;

        public int Decimal_Places { get; set; }

        public decimal Exchange_Rate { get; set; }

        public decimal Min_Exchange_Rate { get; set; }

        public decimal Max_Exchange_Rate { get; set; }

        public bool Is_Local_Currency { get; set; }


        /// <summary>
        /// العملة الافتراضية.
        /// تحدد العملة التي تظهر تلقائيًا عند فتح السندات.
        /// </summary>
        public bool Is_Default { get; set; }
        public bool Is_Active { get; set; }

        public string? Notes { get; set; }
    }

    public class CurrencyRequest
    {
        public string Company_ID { get; set; } = string.Empty;

        public string Currency_Code { get; set; } = string.Empty;

        public string Currency_Name_AR { get; set; } = string.Empty;

        public string Currency_Name_EN { get; set; } = string.Empty;

        public string Currency_Symbol { get; set; } = string.Empty;

        public int Decimal_Places { get; set; }

        public decimal Exchange_Rate { get; set; }

        public decimal Min_Exchange_Rate { get; set; }

        public decimal Max_Exchange_Rate { get; set; }

        public bool Is_Local_Currency { get; set; }

     //   public bool Is_Default { get; set; }
        public bool Is_Active { get; set; }

        public string? Notes { get; set; }

        public string? Created_By { get; set; }

        public string? Updated_By { get; set; }
        /// <summary>
        /// العملة الافتراضية.
        /// </summary>
        public bool Is_Default { get; set; }






    }

    #endregion
}
