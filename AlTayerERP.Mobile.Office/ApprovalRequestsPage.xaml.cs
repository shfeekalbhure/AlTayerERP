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
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        RequestsRefresh.IsRefreshing = true;
        try
        {
            var draft = await _service.GetAsync("DRAFT");
            var pendingReview = await _service.GetAsync("PENDING_REVIEW");
            var pendingApproval = await _service.GetAsync("PENDING_APPROVAL");
            var approved = await _service.GetAsync("APPROVED");
            var rejected = await _service.GetAsync("REJECTED");
            var returned = await _service.GetAsync("RETURNED");

            var selectedStatus = StatusPicker.SelectedIndex switch
            {
                1 => "PENDING_REVIEW",
                2 => "PENDING_APPROVAL",
                _ => null
            };

            var rows = selectedStatus switch
            {
                "PENDING_REVIEW" => pendingReview,
                "PENDING_APPROVAL" => pendingApproval,
                _ => pendingReview.Concat(pendingApproval)
                    .OrderByDescending(x => x.Request_Date)
                    .ThenByDescending(x => x.Payment_Request_ID)
                    .ToList()
            };

            RequestsList.ItemsSource = rows;
            SummaryLabel.Text = $"طلبات معلقة: {rows.Count}";
            DiagnosticLabel.Text =
                $"مسودة: {draft.Count} | مراجعة: {pendingReview.Count} | اعتماد: {pendingApproval.Count}\n" +
                $"معتمد: {approved.Count} | معاد: {returned.Count} | مرفوض: {rejected.Count}";

            if (rows.Count == 0 && draft.Count > 0)
            {
                EmptyTitleLabel.Text = "توجد طلبات مسودة لكنها لم تُرسل للمراجعة بعد.";
            }
            else if (rows.Count == 0)
            {
                EmptyTitleLabel.Text = "لا توجد طلبات اعتماد معلقة.";
            }

            _loaded = true;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("تعذر التحميل", ex.Message, "موافق");
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
