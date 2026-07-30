using System.Collections.ObjectModel;
using System.Globalization;
using AlTayerERP.Mobile.Office.DTOs;
using AlTayerERP.Mobile.Office.Services;

namespace AlTayerERP.Mobile.Office;

public partial class NewVoucherPage : ContentPage
{
    private readonly VoucherEntryService _service;
    private readonly string _type;
    private readonly ObservableCollection<VoucherDraftLine> _lines = [];
    private VoucherEntryReferencesDto? _references;
    private bool _loaded;

    public NewVoucherPage(VoucherEntryService service, string type)
    {
        InitializeComponent();
        _service = service;
        _type = type.Trim().ToUpperInvariant();
        Title = _type == "RECEIPT" ? "سند قبض جديد" : "سند صرف جديد";
        PartyNameEntry.Placeholder = _type == "RECEIPT" ? "استلمنا من" : "اسم المستفيد";
        LinesList.ItemsSource = _lines;
        VoucherDatePicker.Date = DateTime.Today;
        OnLookupPageLoaded(this, EventArgs.Empty);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_loaded) await LoadReferencesAsync();
    }

    private async Task LoadReferencesAsync()
    {
        SetBusy(true);
        HideStatus();
        try
        {
            _references = await _service.GetReferencesAsync(_type);
            SourcePicker.ItemsSource = _references.Sources;
            PartyPicker.ItemsSource = _references.Parties;
            PaymentMethodPicker.ItemsSource = _references.PaymentMethods;
            AccountPicker.ItemsSource = _references.Accounts;
            CostCenterPicker.ItemsSource = _references.CostCenters;
            CurrencyPicker.ItemsSource = _references.Currencies;

            CurrencyPicker.SelectedItem = _references.Currencies.FirstOrDefault(x => x.IsDefault)
                                          ?? _references.Currencies.FirstOrDefault(x => x.IsLocal)
                                          ?? _references.Currencies.FirstOrDefault();
            RefreshLookupButtonTexts();
            ApplyReferencesDiagnostics();

            var today = DateTime.Today;
            var period = _references.OpenPeriods.FirstOrDefault(x => today >= x.StartDate.Date && today <= x.EndDate.Date)
                         ?? _references.OpenPeriods.FirstOrDefault();
            if (period == null)
            {
                PeriodLabel.Text = "لا توجد فترة مالية مفتوحة.";
                SaveButton.IsEnabled = AddLineButton.IsEnabled = false;
            }
            else
            {
                VoucherDatePicker.MinimumDate = period.StartDate.Date;
                VoucherDatePicker.MaximumDate = period.EndDate.Date;
                VoucherDatePicker.Date = today < period.StartDate.Date || today > period.EndDate.Date ? period.StartDate.Date : today;
                PeriodLabel.Text = $"الفترة المفتوحة: {period.StartDate:yyyy/MM/dd} - {period.EndDate:yyyy/MM/dd}";
            }

            _loaded = true;
        }
        catch (Exception ex)
        {
            ShowStatus(MobileApiErrorHandler.GetUserMessage(ex));
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void OnPartyChanged(object? sender, EventArgs e)
    {
        if (PartyPicker.SelectedItem is VoucherEntryPartyDto party)
        {
            PartyNameEntry.Text = party.Name;
            PartySearchButton.Text = party.DisplayName;
        }
        else
        {
            PartySearchButton.Text = "اختيار الطرف - اختياري 🔍";
        }
    }

    private void OnCurrencyChanged(object? sender, EventArgs e)
    {
        if (CurrencyPicker.SelectedItem is not VoucherEntryCurrencyDto currency)
        {
            CurrencySearchButton.Text = "اختيار العملة 🔍";
            return;
        }

        CurrencySearchButton.Text = currency.DisplayName;
        ExchangeRateEntry.Text = (currency.IsLocal ? 1m : currency.ExchangeRate).ToString(CultureInfo.InvariantCulture);
        ExchangeRateEntry.IsReadOnly = currency.IsLocal;
        ForeignAmountEntry.IsEnabled = !currency.IsLocal;
        LocalAmountEntry.IsReadOnly = !currency.IsLocal;
        if (currency.IsLocal) ForeignAmountEntry.Text = "0";
        else RecalculateLocal();
    }

    private void OnForeignAmountChanged(object? sender, TextChangedEventArgs e) => RecalculateLocal();

    private void RecalculateLocal()
    {
        if (CurrencyPicker.SelectedItem is not VoucherEntryCurrencyDto currency || currency.IsLocal) return;
        if (TryDecimal(ForeignAmountEntry.Text, out var foreign) &&
            TryDecimal(ExchangeRateEntry.Text, out var rate) && foreign >= 0 && rate > 0)
            LocalAmountEntry.Text = decimal.Round(foreign * rate, 2).ToString(CultureInfo.InvariantCulture);
    }

    private void OnAddLineClicked(object? sender, EventArgs e)
    {
        HideStatus();
        if (AccountPicker.SelectedItem is not VoucherEntryLookupDto account)
        {
            ShowStatus("اختر الحساب المقابل.");
            return;
        }
        if (SourcePicker.SelectedItem is VoucherEntrySourceDto source &&
            string.Equals(account.Id, source.AccountId, StringComparison.Ordinal))
        {
            ShowStatus("لا يمكن استخدام حساب الصندوق أو البنك نفسه كحساب مقابل.");
            return;
        }
        if (CurrencyPicker.SelectedItem is not VoucherEntryCurrencyDto currency)
        {
            ShowStatus("اختر العملة.");
            return;
        }
        if (!TryDecimal(ExchangeRateEntry.Text, out var rate) || rate <= 0)
        {
            ShowStatus("سعر الصرف غير صحيح.");
            return;
        }
        if (!TryDecimal(ForeignAmountEntry.Text, out var foreign) || foreign < 0)
        {
            ShowStatus("المبلغ الأجنبي غير صحيح.");
            return;
        }
        if (!TryDecimal(LocalAmountEntry.Text, out var local) || local <= 0)
        {
            ShowStatus("المبلغ المحلي يجب أن يكون أكبر من صفر.");
            return;
        }
        if (currency.IsLocal && (rate != 1m || foreign != 0m))
        {
            ShowStatus("للعملة المحلية يجب أن يكون سعر الصرف 1 والمبلغ الأجنبي صفراً.");
            return;
        }

        var center = CostCenterPicker.SelectedItem as VoucherEntryLookupDto;
        _lines.Add(new VoucherDraftLine
        {
            AccountId = account.Id,
            AccountDisplay = account.DisplayName,
            CostCenterId = center?.Id,
            CostCenterDisplay = center?.DisplayName ?? "بدون مركز تكلفة",
            CurrencyId = currency.Id,
            CurrencyDisplay = currency.DisplayName,
            ExchangeRate = rate,
            ForeignAmount = foreign,
            LocalAmount = local,
            Description = Clean(LineDescriptionEditor.Text)
        });
        ClearLineEditor();
        UpdateTotal();
    }

    private void OnRemoveLineClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is VoucherDraftLine line)
        {
            _lines.Remove(line);
            UpdateTotal();
        }
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        HideStatus();
        if (_references == null) { ShowStatus("لم يتم تحميل بيانات السند."); return; }
        if (SourcePicker.SelectedItem is not VoucherEntrySourceDto source) { ShowStatus("اختر الصندوق أو البنك."); return; }
        if (string.IsNullOrWhiteSpace(PartyNameEntry.Text)) { ShowStatus(_type == "RECEIPT" ? "اسم المستلم منه مطلوب." : "اسم المستفيد مطلوب."); return; }
        if (PaymentMethodPicker.SelectedItem is not VoucherEntryPaymentMethodDto method) { ShowStatus("اختر طريقة السداد."); return; }
        var accountingText = Clean(DescriptionEditor.Text);
        if (accountingText == null) { ShowStatus("البيان المحاسبي مطلوب."); return; }
        if (_lines.Count == 0) { ShowStatus("أضف سطراً محاسبياً واحداً على الأقل."); return; }
        if (_lines.Any(x => string.Equals(x.AccountId, source.AccountId, StringComparison.Ordinal)))
        {
            ShowStatus("لا يمكن استخدام حساب الصندوق أو البنك نفسه كحساب مقابل.");
            return;
        }
        if (!_references.OpenPeriods.Any(x => VoucherDatePicker.Date >= x.StartDate.Date && VoucherDatePicker.Date <= x.EndDate.Date))
        {
            ShowStatus("تاريخ السند لا يقع داخل فترة مالية مفتوحة.");
            return;
        }

        var localCurrency = _references.Currencies.FirstOrDefault(x => x.IsLocal);
        if (localCurrency == null) { ShowStatus("العملة المحلية غير مهيأة."); return; }

        var total = _lines.Sum(x => x.LocalAmount);
        var details = new List<CreateMobileVoucherLineDto>
        {
            new()
            {
                Line_No = 1,
                Account_ID = source.AccountId,
                Currency_ID = localCurrency.Id,
                Exchange_Rate = 1m,
                Foreign_Amount = 0m,
                Local_Amount = total,
                Debit_Amount = _type == "RECEIPT" ? total : 0m,
                Credit_Amount = _type == "PAYMENT" ? total : 0m,
                Description = accountingText,
                Line_Type = 1
            }
        };

        details.AddRange(_lines.Select((x, index) => new CreateMobileVoucherLineDto
        {
            Line_No = index + 2,
            Account_ID = x.AccountId,
            Cost_Center_ID = x.CostCenterId,
            Currency_ID = x.CurrencyId,
            Exchange_Rate = x.ExchangeRate,
            Foreign_Amount = x.ForeignAmount,
            Local_Amount = x.LocalAmount,
            Debit_Amount = _type == "PAYMENT" ? x.LocalAmount : 0m,
            Credit_Amount = _type == "RECEIPT" ? x.LocalAmount : 0m,
            Description = x.Description,
            Line_Type = 2
        }));

        if (details.Sum(x => x.Debit_Amount) != details.Sum(x => x.Credit_Amount))
        {
            ShowStatus("السند غير متوازن محاسبياً.");
            return;
        }

        var party = PartyPicker.SelectedItem as VoucherEntryPartyDto;
        var voucherDate = VoucherDatePicker.Date ?? DateTime.Today;
        var dto = new CreateMobileVoucherDto
        {
            Voucher_Type_ID = _references.VoucherType.Id,
            Voucher_Status_ID = _references.DraftStatus.Id,
            Voucher_Date = voucherDate.Date,
            Transaction_Date = voucherDate,
            Cash_Account_ID = source.AccountId,
            Party_ID = party?.Id,
            Received_From_Name = PartyNameEntry.Text.Trim(),
            Payment_Method_ID = method.Id,
            Currency_ID = localCurrency.Id,
            Exchange_Rate = 1m,
            Amount = total,
            Foreign_Total = 0m,
            Local_Total = total,
            Reference_No = Clean(ReferenceEntry.Text),
            Against_Text = accountingText,
            Description = accountingText,
            Requires_Approval = true,
            Details = details
        };

        SetBusy(true);
        try
        {
            var result = await _service.CreateAsync(dto);
            await DisplayAlert("تم الحفظ", $"تم حفظ السند {result.Voucher_No} كمسودة.", "موافق");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            ShowStatus(MobileApiErrorHandler.GetUserMessage(ex));
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void ClearLineEditor()
    {
        AccountPicker.SelectedItem = null;
        CostCenterPicker.SelectedItem = null;
        CurrencyPicker.SelectedItem = _references?.Currencies.FirstOrDefault(x => x.IsDefault)
                                      ?? _references?.Currencies.FirstOrDefault(x => x.IsLocal)
                                      ?? _references?.Currencies.FirstOrDefault();
        AccountSearchButton.Text = "اختيار الحساب المقابل 🔍";
        CostCenterSearchButton.Text = "اختيار مركز التكلفة - اختياري 🔍";
        RefreshLookupButtonTexts();
        ForeignAmountEntry.Text = "0";
        LocalAmountEntry.Text = string.Empty;
        LineDescriptionEditor.Text = string.Empty;
    }

    private void UpdateTotal() => TotalLabel.Text = $"الإجمالي: {_lines.Sum(x => x.LocalAmount):N2}";
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static bool TryDecimal(string? value, out decimal result) =>
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out result) ||
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result);

    private void SetBusy(bool busy)
    {
        BusyIndicator.IsVisible = BusyIndicator.IsRunning = busy;
        SaveButton.IsEnabled = !busy && (_references?.OpenPeriods.Count > 0);
        AddLineButton.IsEnabled = !busy && (_references?.OpenPeriods.Count > 0);
    }

    private void ShowStatus(string message) { StatusLabel.Text = message; StatusLabel.IsVisible = true; }
    private void HideStatus() { StatusLabel.Text = string.Empty; StatusLabel.IsVisible = false; }

    private sealed class VoucherDraftLine
    {
        public string AccountId { get; init; } = string.Empty;
        public string AccountDisplay { get; init; } = string.Empty;
        public string? CostCenterId { get; init; }
        public string CostCenterDisplay { get; init; } = string.Empty;
        public int CurrencyId { get; init; }
        public string CurrencyDisplay { get; init; } = string.Empty;
        public decimal ExchangeRate { get; init; }
        public decimal ForeignAmount { get; init; }
        public decimal LocalAmount { get; init; }
        public string? Description { get; init; }
    }
}
