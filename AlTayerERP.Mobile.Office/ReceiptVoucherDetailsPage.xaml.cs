using AlTayerERP.Mobile.Office.DTOs;
using AlTayerERP.Mobile.Office.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AlTayerERP.Mobile.Office;

public partial class ReceiptVoucherDetailsPage : ContentPage
{
    private readonly ReceiptVoucherService _service;
    private readonly VoucherWorkflowService _workflow;
    private readonly VoucherJournalService _journalService;
    private readonly VoucherAttachmentService _attachmentService;
    private readonly VoucherEntryService _entryService;
    private readonly IReceiptVoucherPrintService _printService;
    private readonly long _voucherId;
    private ReceiptVoucherDetailsDto? _currentVoucher;
    private bool _isPosted;

    public ReceiptVoucherDetailsPage(ReceiptVoucherService service, long voucherId)
    {
        InitializeComponent();
        _service = service;
        _workflow = (IPlatformApplication.Current?.Services ?? throw new InvalidOperationException("خدمات التطبيق غير مهيأة.")).GetRequiredService<VoucherWorkflowService>();
        _journalService = (IPlatformApplication.Current?.Services ?? throw new InvalidOperationException("خدمات التطبيق غير مهيأة.")).GetRequiredService<VoucherJournalService>();
        _attachmentService = (IPlatformApplication.Current?.Services ?? throw new InvalidOperationException("خدمات التطبيق غير مهيأة.")).GetRequiredService<VoucherAttachmentService>();
        _entryService = (IPlatformApplication.Current?.Services ?? throw new InvalidOperationException("خدمات التطبيق غير مهيأة.")).GetRequiredService<VoucherEntryService>();
        _printService = (IPlatformApplication.Current?.Services ?? throw new InvalidOperationException("خدمات التطبيق غير مهيأة.")).GetRequiredService<IReceiptVoucherPrintService>();
        _voucherId = voucherId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        BusyIndicator.IsVisible = BusyIndicator.IsRunning = true;
        MessageLabel.IsVisible = false;

        try
        {
            var data = await _service.GetByIdAsync(_voucherId);
            _currentVoucher = data;
            var header = data.Header;
            _isPosted = header.IsPosted;

            VoucherNoLabel.Text = header.VoucherNo;
            PostingStatusLabel.Text = $"الحالة: {header.WorkflowStatus}";
            ReceivedFromLabel.Text = $"استلمنا من: {header.ReceivedFromName ?? "—"}";
            DateLabel.Text = $"التاريخ: {header.VoucherDate:yyyy/MM/dd}";
            ReferenceLabel.Text = $"المرجع: {header.ReferenceNo ?? "—"}";
            CashAccountLabel.Text = $"الصندوق/البنك: {header.CashAccountDisplay}";
            DescriptionLabel.Text = $"البيان: {header.Description ?? "—"}";
            TotalLabel.Text = $"الإجمالي المحلي: {header.LocalTotal:N2}";
            AuditLabel.Text = $"التعديلات: {header.EditCount} | الطباعة: {header.PrintCount} | الإنشاء: {header.CreatedAt:yyyy/MM/dd HH:mm}";

            ApplyActionVisibility(header.IsPosted, header.ApprovalStatus, header.ReviewStatus);

            LinesPanel.Children.Clear();
            foreach (var line in data.Details.OrderBy(x => x.LineNo))
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
                            new Label { Text = $"الحساب: {line.AccountDisplay}", FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#17324D") },
                            new Label { Text = $"مدين: {line.DebitAmount:N2} | دائن: {line.CreditAmount:N2}", TextColor = Color.FromArgb("#35566F") },
                            new Label { Text = $"العملة: {line.CurrencyDisplay} | السعر: {line.ExchangeRate:N6}", FontSize = 12, TextColor = Color.FromArgb("#7A8896") },
                            new Label { Text = string.IsNullOrWhiteSpace(line.CostCenterDisplay) ? "بدون مركز تكلفة" : $"مركز التكلفة: {line.CostCenterDisplay}", FontSize = 12, TextColor = Color.FromArgb("#7A8896") },
                            new Label { Text = line.Description ?? string.Empty, TextColor = Color.FromArgb("#35566F") }
                        }
                    }
                });
            }
        }
        catch (Exception ex)
        {
            ShowMessage(ex.Message);
        }
        finally
        {
            BusyIndicator.IsRunning = false;
            BusyIndicator.IsVisible = false;
        }
    }

    private void ApplyActionVisibility(bool isPosted, byte approvalStatus, byte reviewStatus)
    {
        var isDraft = !isPosted && approvalStatus != 2 && reviewStatus == 0;
        var isReturned = !isPosted && approvalStatus != 2 && reviewStatus == 3;
        var isReviewed = !isPosted && approvalStatus != 2 && reviewStatus == 2;
        var isApproved = !isPosted && approvalStatus == 2;

        EditButton.IsVisible = isDraft || isReturned;
        DeleteButton.IsVisible = isDraft || isReturned;
        ReviewButton.IsVisible = isDraft;
        ReturnButton.IsVisible = isReviewed;
        ApproveButton.IsVisible = isReviewed;
        CancelApprovalButton.IsVisible = isApproved;
        PostButton.IsVisible = isApproved;
        UnpostButton.IsVisible = isPosted;
        JournalButton.IsVisible = isPosted;
        AttachmentsButton.IsVisible = true;
        PrintButton.IsVisible = true;
    }

    private async void OnEditClicked(object? sender, EventArgs e) =>
        await Navigation.PushAsync(new NewVoucherPage(_entryService, _service, _voucherId));

    private async void OnReviewClicked(object? sender, EventArgs e) =>
        await ExecuteAsync(() => _workflow.ReviewAsync(_voucherId, "مراجعة من تطبيق الجوال"), "تمت مراجعة السند.");

    private async void OnApproveClicked(object? sender, EventArgs e) =>
        await ExecuteAsync(() => _workflow.ApproveAsync(_voucherId, "اعتماد من تطبيق الجوال"), "تم اعتماد السند.");

    private async void OnReturnClicked(object? sender, EventArgs e)
    {
        var reason = await DisplayPromptAsync("إعادة للتصحيح", "أدخل سبب الإعادة:", "تنفيذ", "إلغاء");
        if (string.IsNullOrWhiteSpace(reason)) return;
        await ExecuteAsync(() => _workflow.ReturnForCorrectionAsync(_voucherId, reason), "تمت إعادة السند للتصحيح.");
    }

    private async void OnCancelApprovalClicked(object? sender, EventArgs e)
    {
        var reason = await DisplayPromptAsync("إلغاء الاعتماد", "أدخل سبب إلغاء الاعتماد:", "تنفيذ", "إلغاء");
        if (string.IsNullOrWhiteSpace(reason)) return;
        await ExecuteAsync(() => _workflow.CancelApprovalAsync(_voucherId, reason), "تم إلغاء اعتماد السند.");
    }

    private async void OnPostClicked(object? sender, EventArgs e) =>
        await ExecuteAsync(() => _workflow.PostVoucherAsync(_voucherId, "ترحيل من تطبيق الجوال"), "تم ترحيل السند.");

    private async void OnUnpostClicked(object? sender, EventArgs e)
    {
        if (!_isPosted) return;
        var reason = await DisplayPromptAsync("فك الترحيل", "أدخل سبب فك الترحيل:", "تنفيذ", "إلغاء");
        if (string.IsNullOrWhiteSpace(reason)) return;
        await ExecuteAsync(() => _workflow.UnpostAsync(_voucherId, reason), "تم فك ترحيل السند.");
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        var confirmed = await DisplayAlertAsync("حذف سند القبض", "هل تريد حذف هذه المسودة؟", "نعم", "لا");
        if (!confirmed) return;
        await ExecuteAsync(async () =>
        {
            await _service.DeleteAsync(_voucherId);
            await Navigation.PopAsync();
        }, "تم حذف السند.", reloadAfter: false);
    }

    private async void OnPrintClicked(object? sender, EventArgs e)
    {
        if (_currentVoucher == null)
        {
            ShowMessage("لم تكتمل بيانات سند القبض للطباعة.");
            return;
        }

        BusyIndicator.IsVisible = BusyIndicator.IsRunning = true;
        MessageLabel.IsVisible = false;
        try
        {
            await _printService.PrintAsync(_currentVoucher);
            await _service.RecordPrintAsync(_voucherId);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ShowMessage(ex.Message);
        }
        finally
        {
            BusyIndicator.IsVisible = BusyIndicator.IsRunning = false;
        }
    }

    private async void OnExportPdfClicked(object? sender, EventArgs e)
    {
        if (_currentVoucher == null)
        {
            ShowMessage("لم تكتمل بيانات سند القبض للتصدير.");
            return;
        }

        BusyIndicator.IsVisible = BusyIndicator.IsRunning = true;
        MessageLabel.IsVisible = false;
        try
        {
            var result = await _printService.ExportPdfAsync(_currentVoucher);
            var openFile = await DisplayAlertAsync(
                "تم التصدير",
                $"تم حفظ {result.FileName} في مجلد التنزيلات / AlTayerERP / سندات القبض.",
                "فتح الملف",
                "موافق");

            if (openFile)
                await Launcher.Default.OpenAsync(new Uri(result.ContentUri));
        }
        catch (Exception ex)
        {
            ShowMessage(ex.Message);
        }
        finally
        {
            BusyIndicator.IsVisible = BusyIndicator.IsRunning = false;
        }
    }

    private async void OnAttachmentsClicked(object? sender, EventArgs e) =>
        await Navigation.PushAsync(new VoucherAttachmentsPage(_attachmentService, _voucherId));

    private async void OnJournalClicked(object? sender, EventArgs e) =>
        await Navigation.PushAsync(new VoucherJournalPage(_journalService, _voucherId));

    private async Task ExecuteAsync(Func<Task> action, string successMessage, bool reloadAfter = true)
    {
        BusyIndicator.IsVisible = BusyIndicator.IsRunning = true;
        MessageLabel.IsVisible = false;
        try
        {
            await action();
            await DisplayAlertAsync("تمت العملية", successMessage, "موافق");
            if (reloadAfter) await LoadAsync();
        }
        catch (Exception ex)
        {
            ShowMessage(ex.Message);
        }
        finally
        {
            BusyIndicator.IsVisible = BusyIndicator.IsRunning = false;
        }
    }

    private void ShowMessage(string message)
    {
        MessageLabel.Text = message;
        MessageLabel.IsVisible = true;
    }
}
