using AlTayerERP.Mobile.Office.DTOs;
using AlTayerERP.Mobile.Office.Services;

namespace AlTayerERP.Mobile.Office;

public partial class JournalVouchersPage : ContentPage
{
    private readonly JournalVoucherService _service;

    public JournalVouchersPage(JournalVoucherService service)
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
            await DisplayAlert("تعذر التحميل", MobileApiErrorHandler.GetUserMessage(ex), "موافق");
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
        if (e.CurrentSelection.FirstOrDefault() is not JournalVoucherListItemDto item)
            return;

        VouchersList.SelectedItem = null;
        await Navigation.PushAsync(new JournalVoucherDetailsPage(_service, item.VoucherId));
    }
}
