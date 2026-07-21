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
    /// تحميل بيانات جلسة المستخدم والقوائم المساعدة.
    /// </summary>
    public partial class FrmReceiptVoucher
    {
        #region === نماذج البيانات القادمة من API ===

        private sealed class FinancialVoucherLookupsModel
        {
            public List<BranchLookupModel> Branches { get; set; } = new();
            public List<VoucherTypeLookupModel> VoucherTypes { get; set; } = new();
            public List<VoucherStatusLookupModel> VoucherStatuses { get; set; } = new();
            public List<CashBoxLookupModel> CashBoxes { get; set; } = new();
            public List<CurrencyLookupModel> Currencies { get; set; } = new();
            public List<CostCenterLookupModel> CostCenters { get; set; } = new();
            public List<PaymentMethodLookupModel> PaymentMethods { get; set; } = new();
            public List<PartyLookupModel> Parties { get; set; } = new();
            public List<AccountLookupModel> Accounts { get; set; } = new();
        }

        private sealed class BranchLookupModel
        {
            public int Branch_ID { get; set; }
            public string Branch_Name { get; set; } = string.Empty;
        }

        private sealed class VoucherTypeLookupModel
        {
            public int Voucher_Type_ID { get; set; }
            public string Voucher_Type_Code { get; set; } = string.Empty;
            public string Voucher_Type_Name_AR { get; set; } = string.Empty;
        }

        private sealed class VoucherStatusLookupModel
        {
            public int Voucher_Status_ID { get; set; }
            public string Voucher_Status_Code { get; set; } = string.Empty;
            public string Voucher_Status_Name_AR { get; set; } = string.Empty;
        }

        private sealed class CashBoxLookupModel
        {
            public string Cash_Box_ID { get; set; } = string.Empty;
            public string Cash_Box_Code { get; set; } = string.Empty;
            public string Cash_Box_Name { get; set; } = string.Empty;
            public string Account_ID { get; set; } = string.Empty;
            public int Branch_ID { get; set; }

            public string Display_Name =>
                string.IsNullOrWhiteSpace(Cash_Box_Code) ? Cash_Box_Name : $"{Cash_Box_Code} - {Cash_Box_Name}";
        }

        private sealed class CurrencyLookupModel
        {
            public int Currency_ID { get; set; }
            public string Currency_Code { get; set; } = string.Empty;
            public string Currency_Name_AR { get; set; } = string.Empty;
            public decimal Exchange_Rate { get; set; } = 1.000000m;
            public bool Is_Default { get; set; }
            public bool Is_Local_Currency { get; set; }
            public bool Is_Active { get; set; }

            public string Display_Name =>
                string.IsNullOrWhiteSpace(Currency_Code) ? Currency_Name_AR : $"{Currency_Name_AR} - {Currency_Code}";
        }

        private sealed class CostCenterLookupModel
        {
            public string Cost_Center_ID { get; set; } = string.Empty;
            public string Cost_Center_Code { get; set; } = string.Empty;
            public string Cost_Center_Name_AR { get; set; } = string.Empty;

            public string Display_Name =>
                string.IsNullOrWhiteSpace(Cost_Center_Code) ? Cost_Center_Name_AR : $"{Cost_Center_Code} - {Cost_Center_Name_AR}";
        }

        private sealed class PaymentMethodLookupModel
        {
            public int Payment_Method_ID { get; set; }
            public string Payment_Method_Code { get; set; } = string.Empty;
            public string Payment_Method_Name_AR { get; set; } = string.Empty;
        }

        private sealed class PartyLookupModel
        {
            public string Party_ID { get; set; } = string.Empty;
            public string Party_Code { get; set; } = string.Empty;
            public string Party_Name_AR { get; set; } = string.Empty;

            public string Display_Name =>
                string.IsNullOrWhiteSpace(Party_Code) ? Party_Name_AR : $"{Party_Code} - {Party_Name_AR}";
        }

        private sealed class AccountLookupModel
        {
            public string Account_ID { get; set; } = string.Empty;
            public string Account_Code { get; set; } = string.Empty;
            public string Account_Name_AR { get; set; } = string.Empty;

            public string Display_Name =>
                string.IsNullOrWhiteSpace(Account_Code) ? Account_Name_AR : $"{Account_Code} - {Account_Name_AR}";
        }

        #endregion

        #region === فتح الشاشة ===

        /*      private async void FrmReceiptVoucher_Load(object? sender, EventArgs e)
              {
                  _isLoading = true;
                  UseWaitCursor = true;

                  try
                  {
                      LoadCurrentSession();
                      await LoadFinancialVoucherLookupsAsync();
                      NewVoucher();
                      await GenerateVoucherNumberAsync();
                      UpdateStatusBar("متصل", "متصلة", "فعال");
                      SetViewMode();
                  }
                  catch (HttpRequestException ex)
                  {
                      UpdateStatusBar("غير متصل", "غير مفحوصة", "فعال");
                      ShowError("تعذر الاتصال بالـ API", ex);
                  }
                  catch (Exception ex)
                  {
                      UpdateStatusBar("خطأ", "غير مفحوصة", "غير معروف");
                      ShowError("حدث خطأ أثناء تحميل الشاشة", ex);
                  }
                  finally
                  {


                      _isLoading = false;
                      UseWaitCursor = false;

                      if (cmbCurrency.SelectedIndex >= 0)
                          CalculateHeaderCurrencyAmounts();
                  }
              }
        */
        private async void FrmReceiptVoucher_Load(object? sender, EventArgs e)
        {
            _isLoading = true;
            UseWaitCursor = true;

            try
            {
                // تحميل بيانات جلسة المستخدم
                LoadCurrentSession();

                // تحميل القوائم المساعدة من الـ API
                await LoadFinancialVoucherLookupsAsync();

                // تهيئة الشاشة لتكون في وضع العرض بدون سند مفتوح
                _selectedVoucherId = 0;

                txtVoucherNo.Clear();
                txtJournalNo.Clear();

                txtReference.Clear();
                txtReferenceNo.Clear();
                txtAgainst.Clear();
                txtHeaderNotes.Clear();

                SetNumericValueSafe(numAmount, 0m);
                SetNumericValueSafe(numLocalAmount, 0m);
                SetNumericValueSafe(numForeignAmount, 0m);

                dgvVoucherDetails.Rows.Clear();

                txtTotalAmount.Text = "0.00";
                txtTotalForeignAmount.Text = "0.00";
                txtDifference.Text = "0.00";

                chkPosted.Checked = false;

                UpdateStatusBar("متصل", "متصلة", "فعال");

                // فتح الشاشة في وضع العرض (الحقول مقفلة)
                SetViewMode();
            }
            catch (HttpRequestException ex)
            {
                UpdateStatusBar(
                    "غير متصل",
                    "غير مفحوصة",
                    "فعال");

                ShowError(
                    "تعذر الاتصال بالـ API",
                    ex);
            }
            catch (Exception ex)
            {
                UpdateStatusBar(
                    "خطأ",
                    "غير مفحوصة",
                    "غير معروف");

                ShowError(
                    "حدث خطأ أثناء تحميل الشاشة",
                    ex);
            }
            finally
            {
                _isLoading = false;
                UseWaitCursor = false;

                if (cmbCurrency.SelectedIndex >= 0)
                {
                    CalculateHeaderCurrencyAmounts();
                }
            }
        }

        #endregion

        #region === بيانات الجلسة ===

        private void LoadCurrentSession()
        {
            lblCompanyName.Text = $"الشركة : {CurrentSession.Company_Name}";
            lblCurrentBranch.Text = $"الفرع : {CurrentSession.Branch_Name}";
            lblFiscalYear.Text = $"السنة المالية : {CurrentSession.Year_Name}";
            lblCurrentUser.Text = $"المستخدم : {CurrentSession.Full_Name}";
            txtCreatedBy.Text = CurrentSession.Full_Name;
            txtCreatedDate.Text = DateTime.Now.ToString("yyyy/MM/dd hh:mm tt");
        }

        #endregion

        #region === تحميل القوائم المساعدة ===

        private async Task LoadFinancialVoucherLookupsAsync()
        {
            ValidateSession();

            var requestUrl = BuildLookupUrl();
            var lookups = await _client.GetFromJsonAsync<FinancialVoucherLookupsModel>(requestUrl)
                ?? throw new InvalidOperationException("لم يرجع السيرفر بيانات القوائم المساعدة.");

            lookups.Branches ??= new();
            lookups.VoucherTypes ??= new();
            lookups.VoucherStatuses ??= new();
            lookups.CashBoxes ??= new();
            lookups.Currencies ??= new();
            lookups.CostCenters ??= new();
            lookups.PaymentMethods ??= new();
            lookups.Parties ??= new();
            lookups.Accounts ??= new();

            BindBranches(lookups.Branches);
            BindVoucherTypes(lookups.VoucherTypes);
            BindVoucherStatuses(lookups.VoucherStatuses);
            BindCashBoxes(lookups.CashBoxes);
            BindCurrencies(lookups.Currencies);
            BindCostCenters(lookups.CostCenters);
            BindPaymentMethods(lookups.PaymentMethods);
            BindParties(lookups.Parties);
            BindAccountsToGrid(lookups.Accounts);
            BindCostCentersToGrid(lookups.CostCenters);
            BindCurrenciesToGrid(lookups.Currencies);

            SetInitialSelections(lookups);
        }

        // لا تُحمَّل قوائم السند قبل اكتمال سياق المستخدم والشركة والفرع والسنة.
        private void ValidateSession()
        {
            if (!CurrentSession.IsLoggedIn)
            {
                throw new InvalidOperationException(
                    "جلسة المستخدم غير مكتملة. سجل الدخول وحدد الشركة والفرع والسنة المالية من جديد.");
            }
        }

        private string BuildLookupUrl()
        {
            var companyId = Uri.EscapeDataString(CurrentSession.Company_ID);
            return $"{_baseUrl}FinancialVoucherLookups?companyId={companyId}&branchId={CurrentSession.Branch_ID}";
        }

        #endregion

        #region === ربط القوائم ===

        private static void BindCombo<T>(ComboBox combo, List<T> data, string displayMember, string valueMember, int? defaultIndex = null) where T : class
        {
            combo.DataSource = null;
            combo.DisplayMember = displayMember;
            combo.ValueMember = valueMember;
            combo.DataSource = data.ToList();
            combo.SelectedIndex = defaultIndex ?? (data.Count > 0 ? 0 : -1);
        }

        private static List<T> AddEmptyOption<T>(List<T> items, T emptyItem) where T : class
        {
            var list = new List<T> { emptyItem };
            list.AddRange(items ?? new List<T>());
            return list;
        }

        private void BindBranches(List<BranchLookupModel> branches)
        {
            BindCombo(cmbBranch, branches, nameof(BranchLookupModel.Branch_Name), nameof(BranchLookupModel.Branch_ID));
            var currentBranch = branches.FirstOrDefault(b => b.Branch_ID == CurrentSession.Branch_ID);
            if (currentBranch != null) cmbBranch.SelectedValue = currentBranch.Branch_ID;
            cmbBranch.Enabled = false;
        }

        private void BindVoucherTypes(List<VoucherTypeLookupModel> voucherTypes)
        {
            BindCombo(cmbVoucherType, voucherTypes, nameof(VoucherTypeLookupModel.Voucher_Type_Name_AR), nameof(VoucherTypeLookupModel.Voucher_Type_ID));
        }

        private void BindVoucherStatuses(List<VoucherStatusLookupModel> voucherStatuses)
        {
            BindCombo(cmbStatus, voucherStatuses, nameof(VoucherStatusLookupModel.Voucher_Status_Name_AR), nameof(VoucherStatusLookupModel.Voucher_Status_ID));
        }

        private void BindCashBoxes(List<CashBoxLookupModel> cashBoxes)
        {
            BindCombo(cmbCashAccount, cashBoxes, nameof(CashBoxLookupModel.Display_Name), nameof(CashBoxLookupModel.Account_ID));
        }

        private void BindCurrencies(List<CurrencyLookupModel> currencies)
        {
            var activeCurrencies = currencies.Where(c => c.Is_Active).ToList();
            if (activeCurrencies.Count == 0 && currencies.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine("[تحذير] لا توجد عملات نشطة، يتم عرض جميع العملات.");
                activeCurrencies = currencies.ToList();
            }
            _currencyLookups = activeCurrencies;
            BindCombo(cmbCurrency, _currencyLookups, nameof(CurrencyLookupModel.Display_Name), nameof(CurrencyLookupModel.Currency_ID));
        }

        private void BindCostCenters(List<CostCenterLookupModel> costCenters)
        {
            var list = AddEmptyOption(costCenters, new CostCenterLookupModel
            {
                Cost_Center_ID = string.Empty,
                Cost_Center_Code = string.Empty,
                Cost_Center_Name_AR = "بدون مركز تكلفة"
            });
            BindCombo(cmbCostCenter, list, nameof(CostCenterLookupModel.Display_Name), nameof(CostCenterLookupModel.Cost_Center_ID), 0);
        }

        private void BindPaymentMethods(List<PaymentMethodLookupModel> paymentMethods)
        {
            BindCombo(cmbPaymentMethod, paymentMethods, nameof(PaymentMethodLookupModel.Payment_Method_Name_AR), nameof(PaymentMethodLookupModel.Payment_Method_ID));
            var cashMethod = paymentMethods.FirstOrDefault(m =>
                m.Payment_Method_Code.Equals("CASH", StringComparison.OrdinalIgnoreCase) ||
                m.Payment_Method_Name_AR.Contains("نقد"));
            if (cashMethod != null) cmbPaymentMethod.SelectedValue = paymentMethods.FirstOrDefault()?.Payment_Method_ID ?? 0; // تم الحفاظ عليها برمجياً كما وردت بالملف الأصلي دون تدخل لتغيير المنطق
            if (cashMethod != null) cmbPaymentMethod.SelectedValue = cashMethod.Payment_Method_ID;
        }

        private void BindParties(List<PartyLookupModel> parties)
        {
            var list = AddEmptyOption(parties, new PartyLookupModel
            {
                Party_ID = string.Empty,
                Party_Code = string.Empty,
                Party_Name_AR = "بدون طرف محدد"
            });
            BindCombo(cmbParty, list, nameof(PartyLookupModel.Display_Name), nameof(PartyLookupModel.Party_ID), 0);
        }

        #endregion

        #region === ربط الجدول ===

        private void BindAccountsToGrid(List<AccountLookupModel> accounts)
        {
            _accountLookups = accounts.ToList();
            var data = _accountLookups.ToList();

            colAccountCode.DataSource = null;
            colAccountCode.DisplayMember = nameof(AccountLookupModel.Account_Code);
            colAccountCode.ValueMember = nameof(AccountLookupModel.Account_ID);
            colAccountCode.DataSource = data;

            colAccountName.DataSource = null;
            colAccountName.DisplayMember = nameof(AccountLookupModel.Account_Name_AR);
            colAccountName.ValueMember = nameof(AccountLookupModel.Account_ID);
            colAccountName.DataSource = data;
        }

        private void BindCostCentersToGrid(List<CostCenterLookupModel> costCenters)
        {
            var list = AddEmptyOption(costCenters, new CostCenterLookupModel
            {
                Cost_Center_ID = string.Empty,
                Cost_Center_Code = string.Empty,
                Cost_Center_Name_AR = "بدون مركز تكلفة"
            });
            colCostCenter.DataSource = null;
            colCostCenter.DisplayMember = nameof(CostCenterLookupModel.Display_Name);
            colCostCenter.ValueMember = nameof(CostCenterLookupModel.Cost_Center_ID);
            colCostCenter.DataSource = list;
        }

        private void BindCurrenciesToGrid(List<CurrencyLookupModel> currencies)
        {
            var gridCurrencies = _currencyLookups.Count > 0 ? _currencyLookups.ToList() : currencies.Where(c => c.Is_Active).ToList();
            colCurrency.DataSource = null;
            colCurrency.DisplayMember = nameof(CurrencyLookupModel.Display_Name);
            colCurrency.ValueMember = nameof(CurrencyLookupModel.Currency_ID);
            colCurrency.DataSource = gridCurrencies;
        }

        #endregion

        #region === Values الافتراضية ===

        private void SetInitialSelections(FinancialVoucherLookupsModel lookups)
        {
            var receiptType = lookups.VoucherTypes.FirstOrDefault(t =>
                t.Voucher_Type_Code.Equals("RECEIPT", StringComparison.OrdinalIgnoreCase) ||
                t.Voucher_Type_Code.Equals("RECEIPT_VOUCHER", StringComparison.OrdinalIgnoreCase) ||
                t.Voucher_Type_Name_AR.Contains("قبض"));

            if (receiptType != null) cmbVoucherType.SelectedValue = receiptType.Voucher_Type_ID;
            cmbVoucherType.Enabled = false;

            var draftStatus = lookups.VoucherStatuses.FirstOrDefault(s =>
                s.Voucher_Status_Code.Equals("DRAFT", StringComparison.OrdinalIgnoreCase) ||
                s.Voucher_Status_Name_AR.Contains("مسودة"));

            if (draftStatus != null) cmbStatus.SelectedValue = draftStatus.Voucher_Status_ID;
            cmbStatus.Enabled = false;

            SetDefaultCurrencyAndExchangeRate();
        }

        private void SetDefaultCurrencyAndExchangeRate()
        {
            if (_currencyLookups.Count == 0)
            {
                cmbCurrency.SelectedIndex = -1;
                SetNumericValueSafe(numExchangeRate, 1m);
                SetNumericValueSafe(numLocalAmount, 0m);
                SetNumericValueSafe(numForeignAmount, 0m);
                return;
            }

            var defaultCurrency = _currencyLookups.FirstOrDefault(c => c.Is_Default) ?? _currencyLookups.FirstOrDefault();
            if (defaultCurrency == null)
            {
                cmbCurrency.SelectedIndex = -1;
                return;
            }

            _isCalculatingAmounts = true;
            try
            {
                cmbCurrency.SelectedValue = defaultCurrency.Currency_ID;
                SetNumericValueSafe(numExchangeRate, defaultCurrency.Exchange_Rate);
            }
            finally
            {
                _isCalculatingAmounts = false;
            }

            CalculateHeaderCurrencyAmounts();
        }

        #endregion

        #region === مساعدات ===

        private void UpdateStatusBar(string apiStatus, string dbStatus, string licenseStatus)
        {
            lblStatusApi.Text = $"API: {apiStatus}";
            lblStatusDatabase.Text = $"قاعدة البيانات: {dbStatus}";
            lblStatusLicense.Text = $"الترخيص: {licenseStatus}";
            lblVersion.Text = "الإصدار: 1.0.0";
            lblStatusTime.Text = DateTime.Now.ToString("yyyy/MM/dd hh:mm tt");
        }

        private static void ShowError(string message, Exception ex)
        {
            MessageBox.Show($"{message}\n\n{ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        #endregion
    }
}