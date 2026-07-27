using System.Globalization;
using AlTayerERP.Mobile.Office.DTOs;
using AlTayerERP.Mobile.Office.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AlTayerERP.Mobile.Office;

public partial class NewPaymentRequestPage : ContentPage
{
    private readonly PaymentRequestService _service;
    private readonly PaymentRequestReferenceService _referenceService;
    private bool _referencesLoaded;

    public NewPaymentRequestPage(PaymentRequestService service)
    {
        InitializeComponent();
        _service = service;
        _referenceService = IPlatformApplication.Current.Services
            .GetRequiredService<PaymentRequestReferenceService>();
        RequestDatePicker.Date = DateTime.Today;
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
            var data = await _referenceService.GetAsync();
            AccountPicker.ItemsSource = data.Accounts;
            CostCenterPicker.ItemsSource = data.CostCenters;
            CurrencyPicker.ItemsSource = data.Currencies;

            var defaultCurrency = data.Currencies.FirstOrDefault(x => x.IsDefault)
                                  ?? data.Currencies.FirstOrDefault(x => x.IsLocal)
                                  ?? data.Currencies.FirstOrDefault();
            CurrencyPicker.SelectedItem = defaultCurrency;
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

    private async void OnSaveDraftClicked(object? sender, EventArgs e) =>
        await SaveAsync(submitAfterSave: false);

    private async void OnSaveAndSubmitClicked(object? sender, EventArgs e) =>
        await SaveAsync(submitAfterSave: true);

    private void OnCurrencyChanged(object? sender, EventArgs e)
    {
        if (CurrencyPicker.SelectedItem is not PaymentRequestCurrencyDto currency)
            return;

        ExchangeRateEntry.Text = currency.IsLocal
            ? "1"
            : currency.ExchangeRate.ToString(CultureInfo.InvariantCulture);
        ExchangeRateEntry.IsReadOnly = currency.IsLocal;
        ForeignAmountEntry.IsEnabled = !currency.IsLocal;

        if (currency.IsLocal)
            ForeignAmountEntry.Text = "0";
        else
            RecalculateLocalAmount();
    }

    private void OnForeignAmountChanged(object? sender, TextChangedEventArgs e) =>
        RecalculateLocalAmount();

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

    private async Task SaveAsync(bool submitAfterSave)
    {
        HideStatus();
        if (!TryBuildDto(out var dto, out var validationMessage))
        {
            ShowStatus(validationMessage);
            return;
        }

        SetBusy(true);
        try
        {
            var saved = await _service.CreateAsync(dto!);
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

    private bool TryBuildDto(out CreatePaymentRequestDto? dto, out string message)
    {
        dto = null;
        message = string.Empty;

        if (string.IsNullOrWhiteSpace(BeneficiaryEntry.Text))
        {
            message = "اسم المستفيد مطلوب.";
            return false;
        }

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
        dto = new CreatePaymentRequestDto
        {
            Request_Date = RequestDatePicker.Date ?? DateTime.Today,
            Beneficiary_Name = BeneficiaryEntry.Text.Trim(),
            Party_ID = Clean(PartyIdEntry.Text),
            Header_Reference_No = Clean(ReferenceEntry.Text),
            Description = Clean(DescriptionEditor.Text),
            Lines =
            [
                new CreatePaymentRequestLineDto
                {
                    Account_ID = account.Id,
                    Cost_Center_ID = costCenter?.Id,
                    Currency_ID = currency.Id,
                    Exchange_Rate = exchangeRate,
                    Foreign_Amount = foreignAmount,
                    Local_Amount = localAmount,
                    Description = Clean(LineDescriptionEditor.Text)
                }
            ]
        };
        return true;
    }

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
}
