using AlTayerERP.Mobile.Office.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AlTayerERP.Mobile.Office;

public partial class PaymentVoucherDetailsPage : ContentPage
{
    private readonly PaymentVoucherService _service;
    private readonly VoucherWorkflowService _workflow;
    private readonly long _voucherId;
    private bool _isPosted;

    public PaymentVoucherDetailsPage(PaymentVoucherService service, long voucherId)
    {
        InitializeComponent();
        _service = service;
        _workflow = IPlatformApplication.Current.Services.GetRequiredService<VoucherWorkflowService>();
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
            var result = await _service.GetByIdAsync(_voucherId);
            var header = result.Header;
            _isPosted = header.IsPosted;
            VoucherNoLabel.Text = header.VoucherNo;
            StatusLabel.Text = header.IsPosted ? "مرحّل" : "غير مرحّل";
            BeneficiaryLabel.Text = $"المستفيد: {header.BeneficiaryName ?? "—"}";
            DateLabel.Text = $"التاريخ: {header.VoucherDate:yyyy/MM/dd}";
            SourceLabel.Text = string.IsNullOrWhiteSpace(header.SourceDocumentNo)
                ? "المستند المصدر: —"
                : $"المستند المصدر: {header.SourceDocumentNo}";
            DescriptionLabel.Text = string.IsNullOrWhiteSpace(header.Description)
                ? "البيان: —"
                : $"البيان: {header.Description}";
            TotalLabel.Text = $"الإجمالي المحلي: {header.LocalTotal:N2}";
            PostButton.IsVisible = !header.IsPosted;
            UnpostButton.IsVisible = header.IsPosted;

            LinesPanel.Children.Clear();
            foreach (var line in result.Details.OrderBy(x => x.LineNo))
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
                            new Label { Text = $"الحساب: {line.AccountId}", FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#17324D") },
                            new Label { Text = $"مدين: {line.DebitAmount:N2} | دائن: {line.CreditAmount:N2}", TextColor = Color.FromArgb("#35566F") },
                            new Label { Text = $"العملة: {line.CurrencyId} | السعر: {line.ExchangeRate:N6}", FontSize = 12, TextColor = Color.FromArgb("#7A8896") },
                            new Label { Text = string.IsNullOrWhiteSpace(line.Description) ? string.Empty : line.Description, TextColor = Color.FromArgb("#35566F") }
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
            BusyIndicator.IsVisible = BusyIndicator.IsRunning = false;
        }
    }

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

    private async void OnPostClicked(object? sender, EventArgs e) =>
        await ExecuteAsync(() => _workflow.PostVoucherAsync(_voucherId, "ترحيل من تطبيق الجوال"), "تم ترحيل السند.");

    private async void OnUnpostClicked(object? sender, EventArgs e)
    {
        if (!_isPosted) return;
        var reason = await DisplayPromptAsync("فك الترحيل", "أدخل سبب فك الترحيل:", "تنفيذ", "إلغاء");
        if (string.IsNullOrWhiteSpace(reason)) return;
        await ExecuteAsync(() => _workflow.UnpostAsync(_voucherId, reason), "تم فك ترحيل السند.");
    }

    private async Task ExecuteAsync(Func<Task> action, string successMessage)
    {
        BusyIndicator.IsVisible = BusyIndicator.IsRunning = true;
        MessageLabel.IsVisible = false;
        try
        {
            await action();
            await DisplayAlert("تمت العملية", successMessage, "موافق");
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

    private void ShowMessage(string message)
    {
        MessageLabel.Text = message;
        MessageLabel.IsVisible = true;
    }
}