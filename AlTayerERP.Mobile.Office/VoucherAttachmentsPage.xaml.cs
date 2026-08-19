using AlTayerERP.Mobile.Office.DTOs;
using AlTayerERP.Mobile.Office.Services;

namespace AlTayerERP.Mobile.Office;

public partial class VoucherAttachmentsPage : ContentPage
{
    private readonly VoucherAttachmentService _service;
    private readonly long _voucherId;

    public VoucherAttachmentsPage(VoucherAttachmentService service, long voucherId)
    {
        InitializeComponent();
        _service = service;
        _voucherId = voucherId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        SetBusy(true);
        try
        {
            AttachmentsList.ItemsSource = (await _service.GetAsync(_voucherId))
                .Where(x => x.Active)
                .OrderByDescending(x => x.UploadedAtUtc)
                .ToList();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("تعذر التحميل", ex.Message, "موافق");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void OnUploadClicked(object? sender, EventArgs e)
    {
        try
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "اختر مرفق سند القبض"
            });

            if (file == null)
                return;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new[] { ".pdf", ".png", ".jpg", ".jpeg", ".xlsx", ".docx" };
            if (!allowed.Contains(extension))
            {
                await DisplayAlertAsync("نوع غير مسموح", "الأنواع المسموحة: PDF وصور وExcel وWord.", "موافق");
                return;
            }

            var notes = await DisplayPromptAsync("ملاحظات المرفق", "أدخل وصفًا اختياريًا:", "رفع", "إلغاء");
            SetBusy(true);
            await _service.UploadAsync(_voucherId, file, notes);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("تعذر الرفع", ex.Message, "موافق");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void OnOpenClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: VoucherAttachmentDto item })
            return;

        try
        {
            SetBusy(true);
            var bytes = await _service.DownloadAsync(_voucherId, item.Id);
            var safeName = string.Concat(item.Name.Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch));
            var path = Path.Combine(FileSystem.CacheDirectory, safeName);
            await File.WriteAllBytesAsync(path, bytes);
            await Launcher.Default.OpenAsync(new OpenFileRequest(item.Name, new ReadOnlyFile(path)));
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("تعذر الفتح", ex.Message, "موافق");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: VoucherAttachmentDto item })
            return;

        var reason = await DisplayPromptAsync("حذف المرفق", "سبب الحذف إلزامي:", "حذف", "إلغاء");
        if (string.IsNullOrWhiteSpace(reason))
            return;

        try
        {
            SetBusy(true);
            await _service.DeleteAsync(_voucherId, item.Id, reason);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("تعذر الحذف", ex.Message, "موافق");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool busy)
    {
        BusyIndicator.IsVisible = busy;
        BusyIndicator.IsRunning = busy;
    }
}
