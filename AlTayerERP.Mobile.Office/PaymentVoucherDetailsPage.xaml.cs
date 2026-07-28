using AlTayerERP.Mobile.Office.DTOs;
using AlTayerERP.Mobile.Office.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AlTayerERP.Mobile.Office;

public partial class PaymentVoucherDetailsPage : ContentPage
{
    private readonly PaymentVoucherService _service;
    private readonly VoucherWorkflowService _workflow;
    private readonly VoucherJournalService _journalService;
    private readonly VoucherAttachmentService _attachmentService;
    private readonly VoucherEntryService _entryService;
    private readonly IReceiptVoucherPrintService _printService;
    private readonly long _voucherId;
    private PaymentVoucherDetailsDto? _currentVoucher;
    private bool _isPosted;

    public PaymentVoucherDetailsPage(PaymentVoucherService service, long voucherId)
    {
        InitializeComponent();
        _service = service;
        _workflow = IPlatformApplication.Current.Services.GetRequiredService<VoucherWorkflowService>();
        _journalService = IPlatformApplication.Current.Services.GetRequiredService<VoucherJournalService>();
        _attachmentService = IPlatformApplication.Current.Services.GetRequiredService<VoucherAttachmentService>();
        _entryService = IPlatformApplication.Current.Services.GetRequiredService<VoucherEntryService>();
        _printService = IPlatformApplication.Current.Services.GetRequiredService<IReceiptVoucherPrintService>();
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
            _currentVoucher = await _service.GetByIdAsync(_voucherId);
            var header = _currentVoucher.Header;
            _isPosted = header.IsPosted;
            VoucherNoLabel.Text = header.VoucherNo;
            StatusLabel.Text = $"الحالة: {header.WorkflowStatus}";
            BeneficiaryLabel.Text = $"المستفيد: {header.BeneficiaryName ?? "—"}";
            DateLabel.Text = $"التاريخ: {header.VoucherDate:yyyy/MM/dd}";
            ReferenceLabel.Text = $"المرجع: {header.ReferenceNo ?? "—"}";
            CashAccountLabel.Text = $"الصندوق/البنك: {header.CashAccountDisplay}";
            SourceLabel.Text = string.IsNullOrWhiteSpace(header.SourceDocumentNo) ? "المستند المصدر: —" : $"المستند المصدر: {header.SourceDocumentNo}";
            DescriptionLabel.Text = $"البيان: {header.Description ?? "—"}";
            TotalLabel.Text = $"الإجمالي المحلي: {header.LocalTotal:N2}";
            AuditLabel.Text = $"التعديلات: {header.EditCount} | الطباعة: {header.PrintCount} | الإنشاء: {header.CreatedAt:yyyy/MM/dd HH:mm}";
            ApplyActionVisibility(header.IsPosted, header.ApprovalStatus, header.ReviewStatus);

            LinesPanel.Children.Clear();
            foreach (var line in _currentVoucher.Details.OrderBy(x => x.LineNo))
            {
                LinesPanel.Children.Add(new Border
                {
                    BackgroundColor = Colors.White, Stroke = Color.FromArgb("#D9E2EC"), Padding = 12,
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
        catch (Exception ex) { ShowMessage(ex.Message); }
        finally { BusyIndicator.IsVisible = BusyIndicator.IsRunning = false; }
    }

    private void ApplyActionVisibility(bool isPosted, byte approvalStatus, byte reviewStatus)
    {
        var isDraft = !isPosted && approvalStatus != 2 && reviewStatus == 0;
        var isReturned = !isPosted && approvalStatus != 2 && reviewStatus == 3;
        var isReviewed = !isPosted && approvalStatus != 2 && reviewStatus == 2;
        var isApproved = !isPosted && approvalStatus == 2;
        EditButton.IsVisible = DeleteButton.IsVisible = isDraft || isReturned;
        ReviewButton.IsVisible = isDraft;
        ReturnButton.IsVisible = ApproveButton.IsVisible = isReviewed;
        CancelApprovalButton.IsVisible = PostButton.IsVisible = isApproved;
        UnpostButton.IsVisible = JournalButton.IsVisible = isPosted;
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
        if (!string.IsNullOrWhiteSpace(reason)) await ExecuteAsync(() => _workflow.ReturnForCorrectionAsync(_voucherId, reason), "تمت إعادة السند للتصحيح.");
    }
    private async void OnCancelApprovalClicked(object? sender, EventArgs e)
    {
        var reason = await DisplayPromptAsync("إلغاء الاعتماد", "أدخل سبب إلغاء الاعتماد:", "تنفيذ", "إلغاء");
        if (!string.IsNullOrWhiteSpace(reason)) await ExecuteAsync(() => _workflow.CancelApprovalAsync(_voucherId, reason), "تم إلغاء اعتماد السند.");
    }
    private async void OnPostClicked(object? sender, EventArgs e) =>
        await ExecuteAsync(() => _workflow.PostVoucherAsync(_voucherId, "ترحيل من تطبيق الجوال"), "تم ترحيل السند.");
    private async void OnUnpostClicked(object? sender, EventArgs e)
    {
        if (!_isPosted) return;
        var reason = await DisplayPromptAsync("فك الترحيل", "أدخل سبب فك الترحيل:", "تنفيذ", "إلغاء");
        if (!string.IsNullOrWhiteSpace(reason)) await ExecuteAsync(() => _workflow.UnpostAsync(_voucherId, reason), "تم فك ترحيل السند.");
    }
    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (!await DisplayAlert("حذف سند الصرف", "هل تريد حذف هذه المسودة؟", "نعم", "لا")) return;
        await ExecuteAsync(async () => { await _service.DeleteAsync(_voucherId); await Navigation.PopAsync(); }, "تم حذف السند.", false);
    }
    private async void OnPrintClicked(object? sender, EventArgs e)
    {
        if (_currentVoucher == null) { ShowMessage("لم تكتمل بيانات سند الصرف للطباعة."); return; }
        await PrintAsync(false);
    }
    private async void OnExportPdfClicked(object? sender, EventArgs e)
    {
        if (_currentVoucher == null) { ShowMessage("لم تكتمل بيانات سند الصرف للتصدير."); return; }
        await PrintAsync(true);
    }
    private async Task PrintAsync(bool exportPdf)
    {
        BusyIndicator.IsVisible = BusyIndicator.IsRunning = true;
        try
        {
            var printable = ToPrintableVoucher(_currentVoucher!);
            if (exportPdf)
            {
                var result = await _printService.ExportPdfAsync(printable, VoucherPdfExportOptions.PaymentVoucher);
                if (await DisplayAlert("تم التصدير", $"تم حفظ {result.FileName} في Downloads/AlTayerERP/PaymentVouchers.", "فتح الملف", "موافق"))
                    await Launcher.Default.OpenAsync(new Uri(result.ContentUri));
            }
            else await _printService.PrintAsync(printable, VoucherPdfExportOptions.PaymentVoucher);
            await _service.RecordPrintAsync(_voucherId);
            await LoadAsync();
        }
        catch (Exception ex) { ShowMessage(ex.Message); }
        finally { BusyIndicator.IsVisible = BusyIndicator.IsRunning = false; }
    }
    private static ReceiptVoucherDetailsDto ToPrintableVoucher(PaymentVoucherDetailsDto source) => new()
    {
        Header = new ReceiptVoucherHeaderDto
        {
            DocumentTitle = "سند صرف", DocumentFilePrefix = "Payment",
            VoucherId = source.Header.VoucherId, VoucherNo = source.Header.VoucherNo, VoucherDate = source.Header.VoucherDate,
            ReceivedFromName = source.Header.BeneficiaryName, Description = source.Header.Description, ReferenceNo = source.Header.ReferenceNo,
            CashAccountId = source.Header.CashAccountId, CashAccountDisplay = source.Header.CashAccountDisplay, BranchName = source.Header.BranchName,
            CompanyName = source.Header.CompanyName, CompanyLogoDataUri = source.Header.CompanyLogoDataUri, CurrencyId = source.Header.CurrencyId,
            CurrencyDisplay = source.Header.CurrencyDisplay, ExchangeRate = source.Header.ExchangeRate, Amount = source.Header.Amount,
            ForeignTotal = source.Header.ForeignTotal, LocalTotal = source.Header.LocalTotal, IsPosted = source.Header.IsPosted,
            ApprovalStatus = source.Header.ApprovalStatus, ReviewStatus = source.Header.ReviewStatus, PrintCount = source.Header.PrintCount
        },
        Details = source.Details.Select(x => new ReceiptVoucherLineDto
        {
            VoucherDetailId = x.VoucherDetailId, LineNo = x.LineNo, AccountId = x.AccountId, AccountDisplay = x.AccountDisplay,
            CostCenterId = x.CostCenterId, CostCenterDisplay = x.CostCenterDisplay, CurrencyId = x.CurrencyId, CurrencyDisplay = x.CurrencyDisplay,
            ExchangeRate = x.ExchangeRate, ForeignAmount = x.ForeignAmount, LocalAmount = x.LocalAmount,
            DebitAmount = x.DebitAmount, CreditAmount = x.CreditAmount, LineType = x.LineType, Description = x.Description
        }).ToList()
    };
    private async void OnAttachmentsClicked(object? sender, EventArgs e) => await Navigation.PushAsync(new VoucherAttachmentsPage(_attachmentService, _voucherId));
    private async void OnJournalClicked(object? sender, EventArgs e) => await Navigation.PushAsync(new VoucherJournalPage(_journalService, _voucherId));
    private async Task ExecuteAsync(Func<Task> action, string successMessage, bool reloadAfter = true)
    {
        BusyIndicator.IsVisible = BusyIndicator.IsRunning = true; MessageLabel.IsVisible = false;
        try { await action(); await DisplayAlert("تمت العملية", successMessage, "موافق"); if (reloadAfter) await LoadAsync(); }
        catch (Exception ex) { ShowMessage(ex.Message); }
        finally { BusyIndicator.IsVisible = BusyIndicator.IsRunning = false; }
    }
    private void ShowMessage(string message) { MessageLabel.Text = message; MessageLabel.IsVisible = true; }
}
