using AlTayerERP.Mobile.Office.DTOs;
using AlTayerERP.Mobile.Office.Services;

namespace AlTayerERP.Mobile.Office;

public partial class ApprovalRequestsPage : ContentPage
{
    private readonly ApprovalRequestsService _service;
    private bool _loaded;

    public ApprovalRequestsPage(ApprovalRequestsService service)
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
            var pending = await _service.GetAsync("Pending");
            var underReview = await _service.GetAsync("UnderReview");
            var approved = await _service.GetAsync("Approved");
            var rejected = await _service.GetAsync("Rejected");
            var returned = await _service.GetAsync("Returned");

            var selectedStatus = StatusPicker.SelectedIndex switch
            {
                1 => "UnderReview",
                2 => "Pending",
                _ => null
            };

            var rows = selectedStatus switch
            {
                "UnderReview" => underReview,
                "Pending" => pending,
                _ => pending.Concat(underReview)
                    .OrderByDescending(x => x.Requested_At)
                    .ThenByDescending(x => x.Approval_ID)
                    .ToList()
            };

            RequestsList.ItemsSource = rows;
            SummaryLabel.Text = $"طلبات معلقة: {rows.Count}";
            DiagnosticLabel.Text =
                $"بانتظار الاعتماد: {pending.Count} | تحت المراجعة: {underReview.Count}\n" +
                $"معتمد: {approved.Count} | معاد: {returned.Count} | مرفوض: {rejected.Count}";

            if (rows.Count == 0 && pending.Count == 0 && underReview.Count == 0)
            {
                EmptyTitleLabel.Text = "لا توجد طلبات اعتماد معلقة حالياً.";
            }
            else if (rows.Count == 0)
            {
                EmptyTitleLabel.Text = "لا توجد طلبات اعتماد معلقة.";
            }

            _loaded = true;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("تعذر التحميل", MobileApiErrorHandler.GetUserMessage(ex), "موافق");
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
        if (e.CurrentSelection.FirstOrDefault() is not ApprovalRequestListItemDto row)
            return;

        RequestsList.SelectedItem = null;
        await Navigation.PushAsync(new ApprovalRequestDetailsPage(_service, row.Approval_ID));
    }
}
