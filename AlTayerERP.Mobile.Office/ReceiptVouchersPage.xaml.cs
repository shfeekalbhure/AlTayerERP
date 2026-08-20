using AlTayerERP.Mobile.Office.DTOs;
using AlTayerERP.Mobile.Office.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AlTayerERP.Mobile.Office;

public partial class ReceiptVouchersPage : ContentPage
{
    private readonly ReceiptVoucherService _service;
    private readonly VoucherEntryService _entryService;

    public ReceiptVouchersPage(ReceiptVoucherService service)
    {
        InitializeComponent();
        _service = service;
        _entryService = (IPlatformApplication.Current?.Services ?? throw new InvalidOperationException("خدمات التطبيق غير مهيأة.")).GetRequiredService<VoucherEntryService>();
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
            await DisplayAlertAsync("تعذر التحميل", MobileApiErrorHandler.GetUserMessage(ex), "موافق");
        }
        finally
        {
            VouchersRefresh.IsRefreshing = false;
        }
    }

    private async void OnNewClicked(object? sender, EventArgs e) =>
        await Navigation.PushAsync(new NewVoucherPage(_entryService, "RECEIPT"));

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
