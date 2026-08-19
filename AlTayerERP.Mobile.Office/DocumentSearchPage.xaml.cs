using AlTayerERP.Mobile.Office.DTOs;
using AlTayerERP.Mobile.Office.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AlTayerERP.Mobile.Office;

public partial class DocumentSearchPage : ContentPage
{
    private readonly DocumentSearchService _searchService;
    private readonly PaymentRequestService _paymentRequestService;
    private readonly PaymentVoucherService _paymentVoucherService;
    private readonly ReceiptVoucherService _receiptVoucherService;
    private readonly JournalVoucherService _journalVoucherService;

    public DocumentSearchPage(DocumentSearchService searchService)
    {
        InitializeComponent();
        _searchService = searchService;
        var services = IPlatformApplication.Current?.Services
            ?? throw new InvalidOperationException("خدمات التطبيق غير مهيأة.");
        _paymentRequestService = services.GetRequiredService<PaymentRequestService>();
        _paymentVoucherService = services.GetRequiredService<PaymentVoucherService>();
        _receiptVoucherService = services.GetRequiredService<ReceiptVoucherService>();
        _journalVoucherService = services.GetRequiredService<JournalVoucherService>();
    }

    private async void OnSearchClicked(object? sender, EventArgs e)
    {
        MessageLabel.IsVisible = false;
        if (string.IsNullOrWhiteSpace(QueryEntry.Text))
        {
            ShowMessage("أدخل رقم المستند أو المرجع.");
            return;
        }

        BusyIndicator.IsVisible = BusyIndicator.IsRunning = true;
        try
        {
            var type = TypePicker.SelectedIndex switch
            {
                1 => "PAYMENT_REQUEST",
                2 => "RECEIPT",
                3 => "PAYMENT",
                4 => "JOURNAL",
                _ => "ALL"
            };
            ResultsList.ItemsSource = await _searchService.SearchAsync(QueryEntry.Text, type);
        }
        catch (Exception ex)
        {
            ShowMessage(ex.Message);
        }
        finally
        {
            BusyIndicator.IsRunning = BusyIndicator.IsVisible = false;
        }
    }

    private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not DocumentSearchResultDto item) return;
        ResultsList.SelectedItem = null;

        Page page = item.DocumentType switch
        {
            "PAYMENT_REQUEST" => new PaymentRequestDetailsPage(_paymentRequestService, item.DocumentId),
            "RECEIPT" => new ReceiptVoucherDetailsPage(_receiptVoucherService, item.DocumentId),
            "PAYMENT" => new PaymentVoucherDetailsPage(_paymentVoucherService, item.DocumentId),
            "JOURNAL" => new JournalVoucherDetailsPage(_journalVoucherService, item.DocumentId),
            _ => throw new InvalidOperationException("نوع المستند غير مدعوم.")
        };

        await Navigation.PushAsync(page);
    }

    private void ShowMessage(string message)
    {
        MessageLabel.Text = message;
        MessageLabel.IsVisible = true;
    }
}
