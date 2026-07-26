using AlTayerERP.Mobile.Office.Services;

namespace AlTayerERP.Mobile.Office;

public partial class HomePage : ContentPage
{
    private readonly MobileHomeService _mobileHomeService;
    private bool _permissionsLoaded;

    public HomePage(
        MobileHomeService mobileHomeService,
        string fullName,
        string companyId,
        int branchId,
        int yearId)
    {
        InitializeComponent();
        _mobileHomeService = mobileHomeService;
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
            var allowed = response.Permissions
                .Where(x => x.CanView)
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
}
