using AlTayerERP.Mobile.Office.DTOs;
using AlTayerERP.Mobile.Office.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AlTayerERP.Mobile.Office;

public partial class PaymentRequestDetailsPage : ContentPage
{
    private readonly PaymentRequestService _service;
    private readonly PaymentRequestAttachmentService _attachmentService;
    private readonly long _requestId;
    private PaymentRequestListItemDto? _request;

    public PaymentRequestDetailsPage(PaymentRequestService service, long requestId)
    {
        InitializeComponent();
        _service = service;
        _attachmentService = IPlatformApplication.Current.Services.GetRequiredService<PaymentRequestAttachmentService>();
        _requestId = requestId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        SetBusy(true);
        HideMessage();
        try
        {
            _request = await _service.GetByIdAsync(_requestId);
            RequestNoLabel.Text = _request.Request_No;
            StatusLabel.Text = $"الحالة: {_request.StatusDisplay}";
            BeneficiaryLabel.Text = $"المستفيد: {_request.Beneficiary_Name}";
            DateLabel.Text = $"التاريخ: {_request.Request_Date:yyyy/MM/dd}";
            DescriptionLabel.Text = string.IsNullOrWhiteSpace(_request.Description) ? "البيان: —" : $"البيان: {_request.Description}";
            TotalLabel.Text = $"الإجمالي المحلي: {_request.LocalTotal:N2}";

            LinesPanel.Children.Clear();
            foreach (var line in _request.Details.OrderBy(x => x.Line_No))
            {
                LinesPanel.Children.Add(new Border
                {
                    BackgroundColor = Colors.White,
                    Stroke = Color.FromArgb("#D9E2EC"),
                    Padding = 12,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                    Content = new VerticalStackLayout
                    {
                        Spacing = 5,
                        Children =
                        {
                            new Label { Text = $"الحساب: {line.Account_ID}", TextColor = Color.FromArgb("#17324D"), FontAttributes = FontAttributes.Bold },
                            new Label { Text = $"المبلغ المحلي: {line.Local_Amount:N2}", TextColor = Color.FromArgb("#35566F") },
                            new Label { Text = $"العملة: {line.Currency_ID} | السعر: {line.Exchange_Rate:N6}", TextColor = Color.FromArgb("#7A8896"), FontSize = 12 },
                            new Label { Text = string.IsNullOrWhiteSpace(line.Description) ? "" : line.Description, TextColor = Color.FromArgb("#35566F") }
                        }
                    }
                });
            }

            var editable = _request.Status is "DRAFT" or "RETURNED";
            EditButton.IsVisible = editable;
            SubmitButton.IsVisible = editable;
            ReviewButton.IsVisible = _request.Status == "PENDING_REVIEW";
            ApproveButton.IsVisible = _request.Status == "PENDING_APPROVAL";
            ReturnButton.IsVisible = _request.Status == "PENDING_APPROVAL";
            RejectButton.IsVisible = _request.Status == "PENDING_APPROVAL";
            ReasonEditor.IsVisible = ReviewButton.IsVisible || ApproveButton.IsVisible || ReturnButton.IsVisible || RejectButton.IsVisible;
        }
        catch (Exception ex)
        {
            ShowMessage(ex.Message);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void OnEditClicked(object? sender, EventArgs e)
    {
        if (_request == null || _request.Status is not ("DRAFT" or "RETURNED"))
        {
            ShowMessage("لا يمكن تعديل الطلب في حالته الحالية.");
            return;
        }
        await Navigation.PushAsync(new NewPaymentRequestPage(_service, _request));
    }

    private async void OnAttachmentsClicked(object? sender, EventArgs e) =>
        await Navigation.PushAsync(new PaymentRequestAttachmentsPage(_attachmentService, _requestId));

    private async void OnSubmitClicked(object? sender, EventArgs e) =>
        await ExecuteAsync(() => _service.SubmitAsync(_requestId), "تم إرسال الطلب للمراجعة.", false);
    private async void OnReviewClicked(object? sender, EventArgs e) =>
        await ExecuteAsync(() => _service.ReviewAsync(_requestId, ReasonEditor.Text?.Trim() ?? string.Empty), "تم تحويل الطلب للاعتماد.", true);
    private async void OnApproveClicked(object? sender, EventArgs e) =>
        await ExecuteAsync(() => _service.ApproveAsync(_requestId, ReasonEditor.Text?.Trim() ?? string.Empty), "تم اعتماد الطلب.", true);
    private async void OnReturnClicked(object? sender, EventArgs e) =>
        await ExecuteAsync(() => _service.ReturnAsync(_requestId, ReasonEditor.Text?.Trim() ?? string.Empty), "تمت إعادة الطلب.", true);
    private async void OnRejectClicked(object? sender, EventArgs e) =>
        await ExecuteAsync(() => _service.RejectAsync(_requestId, ReasonEditor.Text?.Trim() ?? string.Empty), "تم رفض الطلب.", true);

    private async Task ExecuteAsync(Func<Task> action, string successMessage, bool reasonRequired)
    {
        HideMessage();
        if (reasonRequired && string.IsNullOrWhiteSpace(ReasonEditor.Text))
        {
            ShowMessage("سبب الإجراء مطلوب.");
            return;
        }
        SetBusy(true);
        try
        {
            await action();
            await DisplayAlert("تمت العملية", successMessage, "موافق");
            await LoadAsync();
        }
        catch (Exception ex) { ShowMessage(ex.Message); }
        finally { SetBusy(false); }
    }

    private void SetBusy(bool busy)
    {
        BusyIndicator.IsVisible = busy;
        BusyIndicator.IsRunning = busy;
        EditButton.IsEnabled = !busy;
        AttachmentsButton.IsEnabled = !busy;
        SubmitButton.IsEnabled = !busy;
        ReviewButton.IsEnabled = !busy;
        ApproveButton.IsEnabled = !busy;
        ReturnButton.IsEnabled = !busy;
        RejectButton.IsEnabled = !busy;
    }

    private void ShowMessage(string message) { MessageLabel.Text = message; MessageLabel.IsVisible = true; }
    private void HideMessage() { MessageLabel.Text = string.Empty; MessageLabel.IsVisible = false; }
}
