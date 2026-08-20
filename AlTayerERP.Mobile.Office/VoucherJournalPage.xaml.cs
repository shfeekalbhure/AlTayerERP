using AlTayerERP.Mobile.Office.Services;

namespace AlTayerERP.Mobile.Office;

public partial class VoucherJournalPage : ContentPage
{
    private readonly VoucherJournalService _service;
    private readonly long _voucherId;

    public VoucherJournalPage(VoucherJournalService service, long voucherId)
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
            var data = await _service.GetAsync(_voucherId);
            EntryNoLabel.Text = data.Header.EntryNo;
            StatusLabel.Text = data.Header.IsCancelled ? "ملغي" : data.Header.IsPosted ? "مرحّل" : "غير مرحّل";
            VoucherLabel.Text = $"السند: {data.VoucherNo}";
            DateLabel.Text = $"التاريخ: {data.Header.EntryDate:yyyy/MM/dd}";
            DescriptionLabel.Text = string.IsNullOrWhiteSpace(data.Header.Description)
                ? "البيان: —"
                : $"البيان: {data.Header.Description}";
            TotalsLabel.Text = $"المدين: {data.Header.TotalDebit:N2} | الدائن: {data.Header.TotalCredit:N2}";

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
                            new Label { Text = line.AccountDisplay, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#17324D") },
                            new Label { Text = $"مدين: {line.Debit:N2} | دائن: {line.Credit:N2}", TextColor = Color.FromArgb("#35566F") },
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
            MessageLabel.Text = MobileApiErrorHandler.GetUserMessage(ex);
            MessageLabel.IsVisible = true;
        }
        finally
        {
            BusyIndicator.IsVisible = BusyIndicator.IsRunning = false;
        }
    }
}
