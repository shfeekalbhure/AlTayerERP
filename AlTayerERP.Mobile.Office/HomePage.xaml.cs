using AlTayerERP.Mobile.Office.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AlTayerERP.Mobile.Office;

public partial class HomePage : ContentPage
{
    private readonly MobileHomeService _mobileHomeService;
    private readonly AuthenticationService _authenticationService;
    private readonly PaymentRequestService _paymentRequestService;
    private readonly PaymentVoucherService _paymentVoucherService;
    private readonly ReceiptVoucherService _receiptVoucherService;
    private readonly JournalVoucherService _journalVoucherService;
    private bool _permissionsLoaded;

    public HomePage(
        MobileHomeService mobileHomeService,
        AuthenticationService authenticationService,
        string fullName,
        string companyId,
        int branchId,
        int yearId)
    {
        InitializeComponent();
        _mobileHomeService = mobileHomeService;
        _authenticationService = authenticationService;
        _paymentRequestService = IPlatformApplication.Current.Services.GetRequiredService<PaymentRequestService>();
        _paymentVoucherService = IPlatformApplication.Current.Services.GetRequiredService<PaymentVoucherService>();
        _receiptVoucherService = IPlatformApplication.Current.Services.GetRequiredService<ReceiptVoucherService>();
        _journalVoucherService = IPlatformApplication.Current.Services.GetRequiredService<JournalVoucherService>();
        WelcomeLabel.Text = $"مرحباً {fullName}";
        ContextLabel.Text = $"الشركة: {companyId} | الفرع: {branchId} | السنة: {yearId}";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_permissionsLoaded)
            return;
        await LoadPermissionsAsync();
    }

    private async Task LoadPermissionsAsync()
    {
        PermissionsBusy.IsVisible = true;
        PermissionsBusy.IsRunning = true;
        PermissionsStatusLabel.IsVisible = true;
        PermissionsStatusLabel.Text = "جاري تحميل الصلاحيات...";
        NoPermissionsLabel.IsVisible = false;

        try
        {
            var response = await _mobileHomeService.GetPermissionsAsync();
            var allowed = response.Permissions.Where(x => x.CanView)
                .Select(x => x.ScreenCode)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            PaymentRequestButton.IsVisible = allowed.Contains("PaymentRequest");
            ReceiptVoucherButton.IsVisible = allowed.Contains("ReceiptVoucher");
            PaymentVoucherButton.IsVisible = allowed.Contains("PaymentVoucher");
            JournalVoucherButton.IsVisible = allowed.Contains("JournalVoucher");
            DocumentSearchButton.IsVisible = allowed.Contains("DocumentSearch");
            ApprovalRequestsButton.IsVisible = allowed.Contains("ApprovalRequests");
            TrialBalanceButton.IsVisible = allowed.Contains("TrialBalance");
            GeneralLedgerButton.IsVisible = allowed.Contains("GeneralLedger");

            ServicesPanel.IsVisible = allowed.Count > 0;
            NoPermissionsLabel.IsVisible = allowed.Count == 0;
            PermissionsStatusLabel.IsVisible = false;
            _permissionsLoaded = true;
        }
        catch (Exception ex)
        {
            PermissionsStatusLabel.Text = ex.Message;
            PermissionsStatusLabel.TextColor = Color.FromArgb("#B42318");
        }
        finally
        {
            PermissionsBusy.IsRunning = false;
            PermissionsBusy.IsVisible = false;
        }
    }

    private async void OnPaymentRequestsClicked(object? sender, EventArgs e) =>
        await Navigation.PushAsync(new PaymentRequestsPage(_paymentRequestService));

    private async void OnReceiptVouchersClicked(object? sender, EventArgs e) =>
        await Navigation.PushAsync(new ReceiptVouchersPage(_receiptVoucherService));

    private async void OnPaymentVouchersClicked(object? sender, EventArgs e) =>
        await Navigation.PushAsync(new PaymentVouchersPage(_paymentVoucherService));

    private async void OnJournalVouchersClicked(object? sender, EventArgs e) =>
        await Navigation.PushAsync(new JournalVouchersPage(_journalVoucherService));

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        var confirmed = await DisplayAlert("تسجيل الخروج", "هل تريد إنهاء الجلسة؟", "نعم", "لا");
        if (!confirmed)
            return;

        try
        {
            await _authenticationService.LogoutAsync();
        }
        finally
        {
            if (Application.Current?.Windows.FirstOrDefault() is Window window)
            {
                window.Page = new NavigationPage(new MainPage(_authenticationService, _mobileHomeService))
                {
                    FlowDirection = FlowDirection.RightToLeft,
                    BarBackgroundColor = Color.FromArgb("#17324D"),
                    BarTextColor = Colors.White
                };
            }
        }
    }
}
