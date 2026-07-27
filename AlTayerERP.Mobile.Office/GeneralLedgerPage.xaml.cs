using AlTayerERP.Mobile.Office.DTOs;
using AlTayerERP.Mobile.Office.Services;

namespace AlTayerERP.Mobile.Office;

public partial class GeneralLedgerPage : ContentPage
{
    private readonly GeneralLedgerService _service;
    private bool _accountsLoaded;

    public GeneralLedgerPage(GeneralLedgerService service)
    {
        InitializeComponent();
        _service = service;
        FromDatePicker.Date = new DateTime(DateTime.Today.Year, 1, 1);
        ToDatePicker.Date = DateTime.Today;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_accountsLoaded)
            await LoadAccountsAsync();
    }

    private async Task LoadAccountsAsync()
    {
        SetBusy(true);
        HideMessage();
        try
        {
            AccountPicker.ItemsSource = await _service.GetAccountsAsync();
            if (AccountPicker.ItemsSource is IEnumerable<GeneralLedgerAccountDto> accounts)
                AccountPicker.SelectedItem = accounts.FirstOrDefault();
            _accountsLoaded = true;
        }
        catch (Exception ex)
        {
            ShowMessage(ex.Message);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void OnLoadClicked(object? sender, EventArgs e)
    {
        HideMessage();
        if (AccountPicker.SelectedItem is not GeneralLedgerAccountDto account)
        {
            ShowMessage("اختر الحساب أولاً.");
            return;
        }

        var from = FromDatePicker.Date ?? DateTime.Today;
        var to = ToDatePicker.Date ?? DateTime.Today;
        if (from.Date > to.Date)
        {
            ShowMessage("تاريخ البداية يجب ألا يتجاوز تاريخ النهاية.");
            return;
        }

        SetBusy(true);
        try
        {
            var result = await _service.GetAsync(account.Id, from, to);
            AccountLabel.Text = $"{result.AccountCode} - {result.AccountName}";
            OpeningLabel.Text = $"الرصيد الافتتاحي: مدين {result.OpeningDebit:N2} | دائن {result.OpeningCredit:N2}";
            MovementLabel.Text = $"حركة الفترة: مدين {result.PeriodDebit:N2} | دائن {result.PeriodCredit:N2}";
            ClosingLabel.Text = $"الرصيد الختامي: مدين {result.ClosingDebit:N2} | دائن {result.ClosingCredit:N2}";
            SummaryBorder.IsVisible = true;
            RowsList.ItemsSource = result.Rows;
        }
        catch (Exception ex)
        {
            ShowMessage(ex.Message);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool busy)
    {
        BusyIndicator.IsVisible = busy;
        BusyIndicator.IsRunning = busy;
        AccountPicker.IsEnabled = !busy;
        FromDatePicker.IsEnabled = !busy;
        ToDatePicker.IsEnabled = !busy;
    }

    private void ShowMessage(string message)
    {
        MessageLabel.Text = message;
        MessageLabel.IsVisible = true;
    }

    private void HideMessage()
    {
        MessageLabel.Text = string.Empty;
        MessageLabel.IsVisible = false;
    }
}
