using AlTayerERP.Mobile.Office.Services;

namespace AlTayerERP.Mobile.Office;

public partial class ReceiptVoucherDetailsPage : ContentPage
{
    private readonly ReceiptVoucherService _service;
    private readonly long _voucherId;

    public ReceiptVoucherDetailsPage(ReceiptVoucherService service, long voucherId)
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
        BusyIndicator.IsVisible = true;
        BusyIndicator.IsRunning = true;
        MessageLabel.IsVisible = false;

        try
        {
            var data = await _service.GetByIdAsync(_voucherId);
            var header = data.Header;
            VoucherNoLabel.Text = header.VoucherNo;
            PostingStatusLabel.Text = header.IsPosted ? "الحالة: مرحّل" : "الحالة: غير مرحّل";
            ReceivedFromLabel.Text = $"استلمنا من: {header.ReceivedFromName ?? "—"}";
            DateLabel.Text = $"التاريخ: {header.VoucherDate:yyyy/MM/dd}";
            ReferenceLabel.Text = $"المرجع: {header.ReferenceNo ?? "—"}";
            CashAccountLabel.Text = $"الصندوق/البنك: {header.CashAccountId}";
            DescriptionLabel.Text = $"البيان: {header.Description ?? "—"}";
            TotalLabel.Text = $"الإجمالي المحلي: {header.LocalTotal:N2}";

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
                            new Label { Text = $"الحساب: {line.AccountId}", FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#17324D") },
                            new Label { Text = $"مدين: {line.DebitAmount:N2} | دائن: {line.CreditAmount:N2}", TextColor = Color.FromArgb("#35566F") },
                            new Label { Text = $"العملة: {line.CurrencyId} | السعر: {line.ExchangeRate:N6}", FontSize = 12, TextColor = Color.FromArgb("#7A8896") },
                            new Label { Text = string.IsNullOrWhiteSpace(line.CostCenterId) ? "بدون مركز تكلفة" : $"مركز التكلفة: {line.CostCenterId}", FontSize = 12, TextColor = Color.FromArgb("#7A8896") },
                            new Label { Text = line.Description ?? string.Empty, TextColor = Color.FromArgb("#35566F") }
                        }
                    }
                });
            }
        }
        catch (Exception ex)
        {
            MessageLabel.Text = ex.Message;
            MessageLabel.IsVisible = true;
        }
        finally
        {
            BusyIndicator.IsRunning = false;
            BusyIndicator.IsVisible = false;
        }
    }
}
