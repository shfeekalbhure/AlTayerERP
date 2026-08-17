using AlTayerERP.Mobile.Office.DTOs;
using AlTayerERP.Mobile.Office.Services;

namespace AlTayerERP.Mobile.Office;

public partial class PaymentRequestsPage : ContentPage
{
    private readonly PaymentRequestService _service;
    private readonly List<(string Text, string? Value)> _statuses =
    [
        ("كل الحالات", null),
        ("مسودة", "DRAFT"),
        ("بانتظار المراجعة", "PENDING_REVIEW"),
        ("بانتظار الاعتماد", "PENDING_APPROVAL"),
        ("معتمد", "APPROVED"),
        ("مرفوض", "REJECTED"),
        ("معاد", "RETURNED")
    ];

    public PaymentRequestsPage(PaymentRequestService service)
    {
        InitializeComponent();
        _service = service;
        StatusPicker.ItemsSource = _statuses.Select(x => x.Text).ToList();
        StatusPicker.SelectedIndex = 0;
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
            var status = StatusPicker.SelectedIndex >= 0
                ? _statuses[StatusPicker.SelectedIndex].Value
                : null;
            RequestsList.ItemsSource = await _service.GetAsync(status, RequestNoEntry.Text);
        }
        catch (Exception ex)
        {
            await DisplayAlert("تعذر التحميل", ex.Message, "موافق");
        }
        finally
        {
            RequestsRefresh.IsRefreshing = false;
        }
    }

    private async void OnNewClicked(object? sender, EventArgs e) =>
        await Navigation.PushAsync(new NewPaymentRequestPage(_service));

    private async void OnRequestSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not PaymentRequestListItemDto selected)
            return;

        RequestsList.SelectedItem = null;
        await Navigation.PushAsync(new PaymentRequestDetailsPage(_service, selected.Payment_Request_ID));
    }

    private async void OnSearchClicked(object? sender, EventArgs e) => await LoadAsync();
    private async void OnStatusChanged(object? sender, EventArgs e) => await LoadAsync();
    private async void OnRefreshing(object? sender, EventArgs e) => await LoadAsync();
}
