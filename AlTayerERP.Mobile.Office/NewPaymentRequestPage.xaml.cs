using System.Collections.ObjectModel;
using System.Globalization;
using AlTayerERP.Mobile.Office.DTOs;
using AlTayerERP.Mobile.Office.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AlTayerERP.Mobile.Office;

public partial class NewPaymentRequestPage : ContentPage
{
    private readonly PaymentRequestService _service;
    private readonly PaymentRequestReferenceService _referenceService;
    private readonly ObservableCollection<PaymentRequestDraftLine> _lines = [];
    private PaymentRequestReferencesDto? _references;
    private bool _referencesLoaded;

    public NewPaymentRequestPage(PaymentRequestService service)
    {
        InitializeComponent();
        _service = service;
        _referenceService = IPlatformApplication.Current.Services.GetRequiredService<PaymentRequestReferenceService>();
        RequestDatePicker.Date = DateTime.Today;
        LinesList.ItemsSource = _lines;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_referencesLoaded)
            await LoadReferencesAsync();
    }

    private async Task LoadReferencesAsync()
    {
        SetBusy(true);
        HideStatus();
        try
        {
            _references = await _referenceService.GetAsync();
            AccountPicker.ItemsSource = _references.Accounts;
            CostCenterPicker.ItemsSource = _references.CostCenters;
            CurrencyPicker.ItemsSource = _references.Currencies;
            CurrencyPicker.SelectedItem = _references.Currencies.FirstOrDefault(x => x.IsDefault)
                                          ?? _references.Currencies.FirstOrDefault(x => x.IsLocal)
                                          ?? _references.Currencies.FirstOrDefault();
            _referencesLoaded = true;
        }
        catch (Exception ex)
        {
            ShowStatus(ex.Message);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void OnSaveDraftClicked(object? sender, EventArgs e) => await SaveAsync(false);
    private async void OnSaveAndSubmitClicked(object? sender, EventArgs e) => await SaveAsync(true);

    private void OnCurrencyChanged(object? sender, EventArgs e)
    {
        if (CurrencyPicker.SelectedItem is not PaymentRequestCurrencyDto currency)
            return;

        ExchangeRateEntry.Text = currency.IsLocal ? "1" : currency.ExchangeRate.ToString(CultureInfo.InvariantCulture);
        ExchangeRateEntry.IsReadOnly = currency.IsLocal;
        ForeignAmountEntry.IsEnabled = !currency.IsLocal;
        LocalAmountEntry.IsReadOnly = !currency.IsLocal;

        if (currency.IsLocal)
            ForeignAmountEntry.Text = "0";
        else
            RecalculateLocalAmount();
    }

    private void OnForeignAmountChanged(object? sender, TextChangedEventArgs e) => RecalculateLocalAmount();

    private void RecalculateLocalAmount()
    {
        if (CurrencyPicker.SelectedItem is not PaymentRequestCurrencyDto currency || currency.IsLocal)
            return;

        if (TryDecimal(ForeignAmountEntry.Text, out var foreignAmount) &&
            TryDecimal(ExchangeRateEntry.Text, out var exchangeRate) &&
            foreignAmount >= 0 && exchangeRate > 0)
        {
            LocalAmountEntry.Text = decimal.Round(foreignAmount * exchangeRate, 2)
                .ToString(CultureInfo.InvariantCulture);
        }
    }

    private void OnAddLineClicked(object? sender, EventArgs e)
    {
        HideStatus();
        if (!TryBuildCurrentLine(out var line, out var message))
        {
            ShowStatus(message);
            return;
        }

        _lines.Add(line!);
        ClearLineEditor();
        UpdateTotal();
    }

    private void OnRemoveLineClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is PaymentRequestDraftLine line)
        {
            _lines.Remove(line);
            UpdateTotal();
        }
    }

    private bool TryBuildCurrentLine(out PaymentRequestDraftLine? line, out string message)
    {
        line = null;
        message = string.Empty;

        if (AccountPicker.SelectedItem is not PaymentRequestReferenceItemDto account)
        {
            message = "اختر الحساب المدين.";
            return false;
        }
        if (CurrencyPicker.SelectedItem is not PaymentRequestCurrencyDto currency)
        {
            message = "اختر العملة.";
            return false;
        }
        if (!TryDecimal(ExchangeRateEntry.Text, out var exchangeRate) || exchangeRate <= 0)
        {
            message = "سعر الصرف غير صحيح.";
            return false;
        }
        if (!TryDecimal(ForeignAmountEntry.Text, out var foreignAmount) || foreignAmount < 0)
        {
            message = "المبلغ الأجنبي غير صحيح.";
            return false;
        }
        if (!TryDecimal(LocalAmountEntry.Text, out var localAmount) || localAmount <= 0)
        {
            message = "المبلغ المحلي يجب أن يكون أكبر من صفر.";
            return false;
        }
        if (currency.IsLocal && (foreignAmount != 0 || exchangeRate != 1))
        {
            message = "في العملة المحلية يجب أن يكون المبلغ الأجنبي صفراً وسعر الصرف 1.";
            return false;
        }

        var costCenter = CostCenterPicker.SelectedItem as PaymentRequestReferenceItemDto;
        line = new PaymentRequestDraftLine
        {
            AccountId = account.Id,
            AccountDisplay = account.DisplayName,
            CostCenterId = costCenter?.Id,
            CostCenterDisplay = costCenter?.DisplayName ?? "بدون مركز تكلفة",
            CurrencyId = currency.Id,
            CurrencyDisplay = currency.DisplayName,
            ExchangeRate = exchangeRate,
            ForeignAmount = foreignAmount,
            LocalAmount = localAmount,
            Description = Clean(LineDescriptionEditor.Text)
        };
        return true;
    }

    private async Task SaveAsync(bool submitAfterSave)
    {
        HideStatus();
        if (string.IsNullOrWhiteSpace(BeneficiaryEntry.Text))
        {
            ShowStatus("اسم المستفيد مطلوب.");
            return;
        }
        if (_lines.Count == 0)
        {
            ShowStatus("أضف سطر صرف واحداً على الأقل.");
            return;
        }

        var dto = new CreatePaymentRequestDto
        {
            Request_Date = RequestDatePicker.Date ?? DateTime.Today,
            Beneficiary_Name = BeneficiaryEntry.Text.Trim(),
            Party_ID = Clean(PartyIdEntry.Text),
            Header_Reference_No = Clean(ReferenceEntry.Text),
            Description = Clean(DescriptionEditor.Text),
            Lines = _lines.Select(x => new CreatePaymentRequestLineDto
            {
                Account_ID = x.AccountId,
                Cost_Center_ID = x.CostCenterId,
                Currency_ID = x.CurrencyId,
                Exchange_Rate = x.ExchangeRate,
                Foreign_Amount = x.ForeignAmount,
                Local_Amount = x.LocalAmount,
                Description = x.Description
            }).ToList()
        };

        SetBusy(true);
        try
        {
            var saved = await _service.CreateAsync(dto);
            if (submitAfterSave)
                await _service.SubmitAsync(saved.Payment_Request_ID);

            await DisplayAlert(
                submitAfterSave ? "تم الإرسال" : "تم الحفظ",
                submitAfterSave
                    ? $"تم حفظ طلب الصرف {saved.Request_No} وإرساله للمراجعة."
                    : $"تم حفظ طلب الصرف {saved.Request_No} كمسودة.",
                "موافق");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            ShowStatus(ex.Message);
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
        var defaultCurrency = _references?.Currencies.FirstOrDefault(x => x.IsDefault)
                              ?? _references?.Currencies.FirstOrDefault(x => x.IsLocal)
                              ?? _references?.Currencies.FirstOrDefault();
        CurrencyPicker.SelectedItem = defaultCurrency;
        ForeignAmountEntry.Text = "0";
        LocalAmountEntry.Text = string.Empty;
        LineDescriptionEditor.Text = string.Empty;
    }

    private void UpdateTotal() =>
        TotalLabel.Text = $"الإجمالي: {_lines.Sum(x => x.LocalAmount):N2}";

    private static bool TryDecimal(string? value, out decimal result) =>
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out result) ||
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result);

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private void SetBusy(bool busy)
    {
        BusyIndicator.IsVisible = busy;
        BusyIndicator.IsRunning = busy;
        SaveDraftButton.IsEnabled = !busy;
        SaveAndSubmitButton.IsEnabled = !busy;
        AddLineButton.IsEnabled = !busy;
        AccountPicker.IsEnabled = !busy;
        CostCenterPicker.IsEnabled = !busy;
        CurrencyPicker.IsEnabled = !busy;
    }

    private void ShowStatus(string message)
    {
        StatusLabel.Text = message;
        StatusLabel.IsVisible = true;
    }

    private void HideStatus()
    {
        StatusLabel.Text = string.Empty;
        StatusLabel.IsVisible = false;
    }

    private sealed class PaymentRequestDraftLine
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
