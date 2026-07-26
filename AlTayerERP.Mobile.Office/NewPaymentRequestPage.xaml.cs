using System.Globalization;
using AlTayerERP.Mobile.Office.DTOs;
using AlTayerERP.Mobile.Office.Services;

namespace AlTayerERP.Mobile.Office;

public partial class NewPaymentRequestPage : ContentPage
{
    private readonly PaymentRequestService _service;

    public NewPaymentRequestPage(PaymentRequestService service)
    {
        InitializeComponent();
        _service = service;
        RequestDatePicker.Date = DateTime.Today;
    }

    private async void OnSaveDraftClicked(object? sender, EventArgs e) =>
        await SaveAsync(submitAfterSave: false);

    private async void OnSaveAndSubmitClicked(object? sender, EventArgs e) =>
        await SaveAsync(submitAfterSave: true);

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

        if (string.IsNullOrWhiteSpace(AccountIdEntry.Text))
        {
            message = "رقم الحساب المدين مطلوب.";
            return false;
        }

        if (!int.TryParse(CurrencyIdEntry.Text, out var currencyId) || currencyId <= 0)
        {
            message = "رقم العملة غير صحيح.";
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

        dto = new CreatePaymentRequestDto
        {
            Request_Date = RequestDatePicker.Date,
            Beneficiary_Name = BeneficiaryEntry.Text.Trim(),
            Party_ID = Clean(PartyIdEntry.Text),
            Header_Reference_No = Clean(ReferenceEntry.Text),
            Description = Clean(DescriptionEditor.Text),
            Lines =
            [
                new CreatePaymentRequestLineDto
                {
                    Account_ID = AccountIdEntry.Text.Trim(),
                    Cost_Center_ID = Clean(CostCenterIdEntry.Text),
                    Currency_ID = currencyId,
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
