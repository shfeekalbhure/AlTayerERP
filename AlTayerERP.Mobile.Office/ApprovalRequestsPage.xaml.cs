using AlTayerERP.Mobile.Office.DTOs;
using AlTayerERP.Mobile.Office.Services;

namespace AlTayerERP.Mobile.Office;

public partial class ApprovalRequestsPage : ContentPage
{
    private readonly PaymentRequestService _service;
    private bool _loaded;

    public ApprovalRequestsPage(PaymentRequestService service)
    {
        InitializeComponent();
        _service = service;
        StatusPicker.SelectedIndex = 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_loaded)
            await LoadAsync();
    }

    private async Task LoadAsync()
    {
        RequestsRefresh.IsRefreshing = true;
        try
        {
            var selectedStatus = StatusPicker.SelectedIndex switch
            {
                1 => "PENDING_REVIEW",
                2 => "PENDING_APPROVAL",
                _ => null
            };

            List<PaymentRequestListItemDto> rows;
            if (selectedStatus != null)
            {
                rows = await _service.GetAsync(selectedStatus);
            }
            else
            {
                var review = await _service.GetAsync("PENDING_REVIEW");
                var approval = await _service.GetAsync("PENDING_APPROVAL");
                rows = review.Concat(approval)
                    .OrderByDescending(x => x.Request_Date)
                    .ThenByDescending(x => x.Payment_Request_ID)
                    .ToList();
            }

            RequestsList.ItemsSource = rows;
            SummaryLabel.Text = $"طلبات معلقة: {rows.Count}";
            _loaded = true;
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

    private async void OnStatusChanged(object? sender, EventArgs e)
    {
        if (Handler == null) return;
        await LoadAsync();
    }

    private async void OnRefreshClicked(object? sender, EventArgs e) => await LoadAsync();

    private async void OnRefreshing(object? sender, EventArgs e) => await LoadAsync();

    private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not PaymentRequestListItemDto row)
            return;

        RequestsList.SelectedItem = null;
        await Navigation.PushAsync(new PaymentRequestDetailsPage(_service, row.Payment_Request_ID));
    }
}
