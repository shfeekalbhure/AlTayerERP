using AlTayerERP.Mobile.Office.Services;

namespace AlTayerERP.Mobile.Office;

public partial class TrialBalancePage : ContentPage
{
    private readonly TrialBalanceService _service;

    public TrialBalancePage(TrialBalanceService service)
    {
        InitializeComponent();
        _service = service;
        FromDatePicker.Date = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        ToDatePicker.Date = DateTime.Today;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async void OnLoadClicked(object? sender, EventArgs e) => await LoadAsync();

    private async Task LoadAsync()
    {
        MessageLabel.IsVisible = false;
        BusyIndicator.IsVisible = BusyIndicator.IsRunning = true;
        try
        {
            var from = FromDatePicker.Date ?? DateTime.Today;
            var to = ToDatePicker.Date ?? DateTime.Today;
            var result = await _service.GetAsync(from, to);
            RowsList.ItemsSource = result.Rows;
            PeriodTotalsLabel.Text = $"حركة الفترة: مدين {result.Totals.PeriodDebit:N2} | دائن {result.Totals.PeriodCredit:N2}";
            ClosingTotalsLabel.Text = $"الرصيد الختامي: مدين {result.Totals.ClosingDebit:N2} | دائن {result.Totals.ClosingCredit:N2}";
        }
        catch (Exception ex)
        {
            MessageLabel.Text = ex.Message;
            MessageLabel.IsVisible = true;
        }
        finally
        {
            BusyIndicator.IsRunning = BusyIndicator.IsVisible = false;
        }
    }
}
