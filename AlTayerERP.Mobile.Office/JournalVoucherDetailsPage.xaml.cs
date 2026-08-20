using AlTayerERP.Mobile.Office.Services;

namespace AlTayerERP.Mobile.Office;

public partial class JournalVoucherDetailsPage : ContentPage
{
    private readonly JournalVoucherService _service;
    private readonly long _voucherId;

    public JournalVoucherDetailsPage(JournalVoucherService service, long voucherId)
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
            StatusLabel.Text = header.IsPosted ? "الحالة: مرحّل" : "الحالة: غير مرحّل";
            DateLabel.Text = $"التاريخ: {header.VoucherDate:yyyy/MM/dd}";
            DescriptionLabel.Text = string.IsNullOrWhiteSpace(header.Description) ? "البيان: —" : $"البيان: {header.Description}";
            ReferenceLabel.Text = string.IsNullOrWhiteSpace(header.ReferenceNo) ? "المرجع: —" : $"المرجع: {header.ReferenceNo}";
            DebitTotalLabel.Text = $"إجمالي المدين: {header.DebitTotal:N2}";
            CreditTotalLabel.Text = $"إجمالي الدائن: {header.CreditTotal:N2}";
            var balanced = decimal.Round(header.DebitTotal, 2) == decimal.Round(header.CreditTotal, 2);
            BalanceLabel.Text = balanced ? "القيد متوازن" : "القيد غير متوازن";
            BalanceLabel.TextColor = Color.FromArgb(balanced ? "#198754" : "#B42318");
            LinesList.ItemsSource = data.Details;
        }
        catch (Exception ex)
        {
            MessageLabel.Text = MobileApiErrorHandler.GetUserMessage(ex);
            MessageLabel.IsVisible = true;
        }
        finally
        {
            BusyIndicator.IsRunning = false;
            BusyIndicator.IsVisible = false;
        }
    }
}
