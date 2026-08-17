using AlTayerERP.Mobile.Office.DTOs;
using AlTayerERP.Mobile.Office.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AlTayerERP.Mobile.Office;

public partial class PaymentVouchersPage : ContentPage
{
    private readonly PaymentVoucherService _service;
    private readonly VoucherEntryService _entryService;

    public PaymentVouchersPage(PaymentVoucherService service)
    {
        InitializeComponent();
        _service = service;
        _entryService = IPlatformApplication.Current.Services.GetRequiredService<VoucherEntryService>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            VouchersList.ItemsSource = await _service.GetAsync(VoucherNoEntry.Text);
        }
        catch (Exception ex)
        {
            await DisplayAlert("تعذر التحميل", ex.Message, "موافق");
        }
        finally
        {
            VouchersRefresh.IsRefreshing = false;
        }
    }

    private async void OnNewClicked(object? sender, EventArgs e) =>
        await Navigation.PushAsync(new NewVoucherPage(_entryService, "PAYMENT"));

    private async void OnSearchClicked(object? sender, EventArgs e) => await LoadAsync();
    private async void OnRefreshing(object? sender, EventArgs e) => await LoadAsync();

    private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not PaymentVoucherListItemDto item)
            return;

        VouchersList.SelectedItem = null;
        await Navigation.PushAsync(new PaymentVoucherDetailsPage(_service, item.VoucherId));
    }
}
