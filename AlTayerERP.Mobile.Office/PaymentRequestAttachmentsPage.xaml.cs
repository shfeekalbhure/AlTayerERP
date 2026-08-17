using AlTayerERP.Mobile.Office.DTOs;
using AlTayerERP.Mobile.Office.Services;

namespace AlTayerERP.Mobile.Office;

public partial class PaymentRequestAttachmentsPage : ContentPage
{
    private readonly PaymentRequestAttachmentService _service;
    private readonly long _requestId;

    public PaymentRequestAttachmentsPage(PaymentRequestAttachmentService service, long requestId)
    {
        InitializeComponent();
        _service = service;
        _requestId = requestId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        HideStatus();
        try
        {
            AttachmentsList.ItemsSource = await _service.GetAsync(_requestId);
        }
        catch (Exception ex)
        {
            ShowStatus(ex.Message);
        }
        finally
        {
            AttachmentsRefresh.IsRefreshing = false;
        }
    }

    private async void OnUploadClicked(object? sender, EventArgs e)
    {
        HideStatus();
        try
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "اختر مرفق طلب الصرف"
            });
            if (file == null)
                return;

            UploadButton.IsEnabled = false;
            await _service.UploadAsync(_requestId, file);
            await DisplayAlert("تم الرفع", $"تم رفع الملف {file.FileName}.", "موافق");
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ShowStatus(ex.Message);
        }
        finally
        {
            UploadButton.IsEnabled = true;
        }
    }

    private async void OnOpenClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.CommandParameter is not PaymentRequestAttachmentDto attachment)
            return;

        HideStatus();
        try
        {
            var path = await _service.DownloadAsync(_requestId, attachment);
            await Launcher.Default.OpenAsync(new OpenFileRequest
            {
                Title = attachment.Original_File_Name,
                File = new ReadOnlyFile(path)
            });
        }
        catch (Exception ex)
        {
            ShowStatus(ex.Message);
        }
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.CommandParameter is not PaymentRequestAttachmentDto attachment)
            return;

        var reason = await DisplayPromptAsync(
            "حذف المرفق",
            $"اكتب سبب حذف {attachment.Original_File_Name}",
            "حذف",
            "إلغاء",
            "سبب الحذف",
            maxLength: 300);

        if (string.IsNullOrWhiteSpace(reason))
            return;

        HideStatus();
        try
        {
            await _service.DeleteAsync(_requestId, attachment.Payment_Request_Attachment_ID, reason);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ShowStatus(ex.Message);
        }
    }

    private async void OnRefreshing(object? sender, EventArgs e) => await LoadAsync();

    private void ShowStatus(string message)
    {
        StatusLabel.Text = message;
        StatusLabel.IsVisible = true;
    }

    private void HideStatus()
    {
        StatusLabel.Text = string.Empty;
        StatusLabel.IsVisible = false;
    }
}
