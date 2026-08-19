using AlTayerERP.Mobile.Office.DTOs;
using AlTayerERP.Mobile.Office.Services;

namespace AlTayerERP.Mobile.Office;

public partial class ApprovalRequestDetailsPage : ContentPage
{
    private readonly ApprovalRequestsService _service;
    private readonly int _approvalId;
    private ApprovalRequestListItemDto? _request;
    private bool _loaded;

    public ApprovalRequestDetailsPage(ApprovalRequestsService service, int approvalId)
    {
        InitializeComponent();
        _service = service;
        _approvalId = approvalId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_loaded)
            await LoadAsync();
    }

    private async Task LoadAsync()
    {
        SetBusy(true);
        try
        {
            _request = await _service.GetByIdAsync(_approvalId);
            TitleLabel.Text = $"طلب اعتماد #{_request.Approval_ID}";
            StatusLabel.Text = $"الحالة: {_request.StatusDisplay}";
            RequestTypeLabel.Text = $"نوع الطلب: {_request.Request_Type}";
            ReferenceLabel.Text = $"المرجع: {_request.ReferenceDisplay}";
            EntityLabel.Text = $"الكيان: {(_request.Entity_Type ?? "—")} / {(_request.Entity_ID ?? "—")}";
            AmountLabel.Text = $"المبلغ: {_request.AmountDisplay}";
            RequestedByLabel.Text = $"أنشأه: {(_request.Requested_By ?? "—")}";
            RequestedAtLabel.Text = $"التاريخ: {_request.Requested_At:yyyy/MM/dd HH:mm}";
            ReasonLabel.Text = $"السبب: {(_request.Reason ?? "—")}";
            ApprovalNotesLabel.Text = $"ملاحظات الاعتماد: {(_request.Approval_Notes ?? "—")}";

            var open = _request.Status is "Pending" or "UnderReview";
            ReviewButton.IsEnabled = open && _request.Status == "Pending";
            ApproveButton.IsEnabled = open;
            ReturnButton.IsEnabled = open;
            RejectButton.IsEnabled = open;
            _loaded = true;
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void OnReviewClicked(object? sender, EventArgs e)
    {
        if (_request == null) return;
        await ExecuteAsync(
            () => _service.ReviewAsync(_approvalId, Clean(DecisionReasonEditor.Text)),
            "تم تحويل الطلب إلى حالة تحت المراجعة.");
    }

    private async void OnApproveClicked(object? sender, EventArgs e)
    {
        if (!RequireReason()) return;
        await ExecuteAsync(
            () => _service.ApproveAsync(_approvalId, Clean(DecisionReasonEditor.Text)!),
            "تم اعتماد الطلب بنجاح.");
    }

    private async void OnReturnClicked(object? sender, EventArgs e)
    {
        if (!RequireReason()) return;
        await ExecuteAsync(
            () => _service.ReturnAsync(_approvalId, Clean(DecisionReasonEditor.Text)!),
            "تمت إعادة الطلب للتعديل.");
    }

    private async void OnRejectClicked(object? sender, EventArgs e)
    {
        if (!RequireReason()) return;
        await ExecuteAsync(
            () => _service.RejectAsync(_approvalId, Clean(DecisionReasonEditor.Text)!),
            "تم رفض الطلب.");
    }

    private async Task ExecuteAsync(Func<Task> operation, string successMessage)
    {
        SetBusy(true);
        HideError();
        try
        {
            await operation();
            await DisplayAlertAsync("تم التنفيذ", successMessage, "موافق");
            DecisionReasonEditor.Text = string.Empty;
            _loaded = false;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private bool RequireReason()
    {
        if (!string.IsNullOrWhiteSpace(DecisionReasonEditor.Text)) return true;
        ShowError("أدخل سبب القرار قبل التنفيذ.");
        return false;
    }

    private void SetBusy(bool busy)
    {
        BusyIndicator.IsVisible = BusyIndicator.IsRunning = busy;
        if (busy)
        {
            ReviewButton.IsEnabled = false;
            ApproveButton.IsEnabled = false;
            ReturnButton.IsEnabled = false;
            RejectButton.IsEnabled = false;
        }
    }

    private void ShowError(string message)
    {
        StatusMessageLabel.Text = message;
        StatusMessageLabel.IsVisible = true;
    }

    private void HideError() => StatusMessageLabel.IsVisible = false;
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
