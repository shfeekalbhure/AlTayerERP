using AlTayerERP.Mobile.Office.DTOs;
using AlTayerERP.Mobile.Office.Services;

namespace AlTayerERP.Mobile.Office;

public partial class ReceiptVouchersPage : ContentPage
{
    private readonly ReceiptVoucherService _service;

    public ReceiptVouchersPage(ReceiptVoucherService service)
    {
        InitializeComponent();
        _service = service;
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

    private async void OnSearchClicked(object? sender, EventArgs e) => await LoadAsync();
    private async void OnRefreshing(object? sender, EventArgs e) => await LoadAsync();

    private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not ReceiptVoucherListItemDto item)
            return;
        VouchersList.SelectedItem = null;
        await Navigation.PushAsync(new ReceiptVoucherDetailsPage(_service, item.VoucherId));
    }
}
