using AlTayerERP.Mobile.Office.Services;

namespace AlTayerERP.Mobile.Office;

public partial class PaymentVoucherDetailsPage : ContentPage
{
    private readonly PaymentVoucherService _service;
    private readonly long _voucherId;

    public PaymentVoucherDetailsPage(PaymentVoucherService service, long voucherId)
    {
        InitializeComponent();
        _service = service;
        _voucherId = voucherId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        BusyIndicator.IsVisible = BusyIndicator.IsRunning = true;
        MessageLabel.IsVisible = false;
        try
        {
            var result = await _service.GetByIdAsync(_voucherId);
            var header = result.Header;
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
            MessageLabel.Text = ex.Message;
            MessageLabel.IsVisible = true;
        }
        finally
        {
            BusyIndicator.IsVisible = BusyIndicator.IsRunning = false;
        }
    }
}
