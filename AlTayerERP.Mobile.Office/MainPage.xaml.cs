using AlTayerERP.Mobile.Office.DTOs;
using AlTayerERP.Mobile.Office.Services;

namespace AlTayerERP.Mobile.Office;

public partial class MainPage : ContentPage
{
    private readonly AuthenticationService _authentication;
    private readonly MobileHomeService _mobileHomeService;
    private readonly ApiConnectionDiagnosticsService _diagnostics;
    private readonly ApiClientConfiguration _connectionConfiguration;
    private LoginOptionsResponseDto? _loginOptions;
    private bool _companiesLoaded;
    private bool _sessionChecked;
    private bool _isBusy;
    private bool _isLoadingConnectionSettings;
    private string? _optionsCompanyId;
    private string? _optionsLoginName;

    public MainPage(
        AuthenticationService authentication,
        MobileHomeService mobileHomeService,
        ApiConnectionDiagnosticsService diagnostics,
        ApiClientConfiguration connectionConfiguration)
    {
        InitializeComponent();
        _authentication = authentication;
        _mobileHomeService = mobileHomeService;
        _diagnostics = diagnostics;
        _connectionConfiguration = connectionConfiguration;
        InitializeDevelopmentConnectionSettings();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_sessionChecked)
        {
            _sessionChecked = true;
            SetBusy(true);
            CompaniesStatusLabel.Text = "جاري التحقق من الجلسة...";
            CompaniesStatusLabel.IsVisible = true;

            try
            {
                var stored = await _authentication.RestoreSessionAsync();
                if (stored != null)
                {
                    OpenHome(stored.FullName, stored.CompanyId, stored.BranchId, stored.YearId);
                    return;
                }
            }
            catch
            {
                // عند تعذر الاستعادة نعرض شاشة الدخول المعتادة دون كشف تفاصيل تقنية.
            }
            finally
            {
                SetBusy(false);
            }
        }

        if (!_companiesLoaded)
            await LoadCompaniesAsync();
    }

    private async Task LoadCompaniesAsync()
    {
        HideError();
        CompaniesStatusLabel.IsVisible = true;
        CompaniesStatusLabel.Text = "جاري تحميل الشركات...";
        ResetContextOptions();
        SetBusy(true);

        try
        {
            var health = await _diagnostics.CheckHealthAsync();
            if (health.ErrorType != ApiErrorType.None)
                throw new ApiDiagnosticException(health);

            var companies = await _authentication.GetLoginCompaniesAsync();
            CompanyPicker.ItemsSource = companies;
            CompanyPicker.SelectedItem = companies.Count == 1 ? companies[0] : null;

            CompaniesStatusLabel.Text = companies.Count switch
            {
                0 => "لا توجد شركات مهيأة للظهور في شاشة الدخول. فعّل الشركة ومجموعة الشركات وخيار الظهور عند الدخول.",
                1 => "تم تحديد الشركة تلقائياً.",
                _ => "اختر الشركة من القائمة."
            };

            _companiesLoaded = true;
        }
        catch (Exception ex)
        {
            CompanyPicker.ItemsSource = null;
            CompanyPicker.SelectedItem = null;
            CompaniesStatusLabel.Text = "تعذر تحميل الشركات.";
            ShowError(MobileApiErrorHandler.GetUserMessage(ex));
        }
        finally
        {
            SetBusy(false);
            UpdateCredentialButtons();
        }
    }

    private void OnCredentialsChanged(object? sender, EventArgs e)
    {
        if (_isBusy)
            return;

        var currentCompany = (CompanyPicker.SelectedItem as LoginCompanyOptionDto)?.Company_ID?.Trim();
        var currentLogin = LoginNameEntry.Text?.Trim();

        if (_loginOptions != null &&
            (!string.Equals(currentCompany, _optionsCompanyId, StringComparison.Ordinal) ||
             !string.Equals(currentLogin, _optionsLoginName, StringComparison.OrdinalIgnoreCase)))
        {
            ResetContextOptions();
            CredentialsPanel.IsVisible = true;
            ContextPanel.IsVisible = false;
            StepTitleLabel.Text = "أدخل بيانات الحساب أولاً";
        }

        UpdateCredentialButtons();
    }

    private void OnContextChanged(object? sender, EventArgs e) => UpdateContextButton();

    private void OnConnectionModeChanged(object? sender, EventArgs e)
    {
#if DEBUG
        // لا نغيّر أي إعداد أو عنصر مرئي أثناء تعبئة القيم الأولية للواجهة.
        if (_isLoadingConnectionSettings || ConnectionModePicker.SelectedIndex < 0)
            return;

        // الحدث لا يعيد إنشاء عناصر الـPicker، ولا يختبر الاتصال تلقائياً.
        SaveDevelopmentConnectionSettings();
        RefreshWifiConnectionFields();
#endif
    }

    private async void OnTestConnectionClicked(object? sender, EventArgs e)
    {
#if DEBUG
        SaveDevelopmentConnectionSettings();
        SetBusy(true);
        try
        {
            var result = await _diagnostics.CheckHealthAsync(forceRetry: true);
            ConnectionStatusLabel.Text = result.ErrorType == ApiErrorType.None
                ? result.ConnectionKind == ApiConnectionKind.USB
                    ? "تم الاتصال بالخادم عبر USB."
                    : "تم الاتصال بالخادم عبر شبكة Wi-Fi."
                : result.ErrorType switch
                {
                    ApiErrorType.ConnectionRefused => "لم يتم العثور على API. شغّل الخادم وتحقق من المنفذ 5021.",
                    ApiErrorType.Timeout => "انتهت مهلة الاتصال. تحقق من الشبكة والجدار الناري والمنفذ 5021.",
                    ApiErrorType.Dns => "عنوان IP غير صحيح أو غير قابل للوصول من الهاتف.",
                    ApiErrorType.ServerError => "تم الوصول إلى API، لكن قاعدة البيانات غير جاهزة. افحص /api/health.",
                    _ => "فشل فحص API. راجع العنوان وإعدادات الاتصال."
                };
            DevelopmentDatabaseLabel.Text = result.ErrorType == ApiErrorType.None && !string.IsNullOrWhiteSpace(result.DatabaseName)
                ? $"قاعدة التطوير: {result.DatabaseName}"
                : !string.IsNullOrWhiteSpace(result.BaseAddress)
                    ? $"العنوان المختبر: {result.BaseAddress}"
                    : string.Empty;

            if (result.ErrorType == ApiErrorType.None && !_companiesLoaded)
                await LoadCompaniesAsync();
        }
        finally
        {
            SetBusy(false);
        }
#endif
    }

    private async void OnLoadOptionsClicked(object? sender, EventArgs e)
    {
        HideError();

        if (CompanyPicker.SelectedItem is not LoginCompanyOptionDto company ||
            string.IsNullOrWhiteSpace(LoginNameEntry.Text) ||
            string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            ShowError("اختر الشركة وأدخل اسم المستخدم وكلمة المرور.");
            return;
        }

        ResetContextOptions();
        SetBusy(true);
        try
        {
            var requestedLoginName = LoginNameEntry.Text.Trim();
            _loginOptions = await _authentication.GetLoginOptionsAsync(new LoginOptionsRequestDto
            {
                Company_ID = company.Company_ID,
                Login_Name = requestedLoginName,
                Password = PasswordEntry.Text,
                Device_ID = GetDeviceId()
            });

            if (_loginOptions.Branches.Count == 0 || _loginOptions.Years.Count == 0)
                throw new InvalidOperationException("لا توجد فروع أو سنوات مالية متاحة لهذا المستخدم.");

            _optionsCompanyId = company.Company_ID.Trim();
            _optionsLoginName = requestedLoginName;

            BranchPicker.ItemsSource = _loginOptions.Branches;
            YearPicker.ItemsSource = _loginOptions.Years;

            var defaultBranch = _loginOptions.Branches.FirstOrDefault(x => x.Is_Default)
                                ?? _loginOptions.Branches.FirstOrDefault();
            var defaultYear = _loginOptions.Years.FirstOrDefault(x => x.Is_Default)
                              ?? _loginOptions.Years.FirstOrDefault();

            BranchPicker.SelectedItem = defaultBranch;
            YearPicker.SelectedItem = defaultYear;

            UserWelcomeLabel.Text = $"مرحباً {_loginOptions.Full_Name}";
            CredentialsPanel.IsVisible = false;
            ContextPanel.IsVisible = true;
            StepTitleLabel.Text = "اختر الفرع والسنة المالية";
            UpdateContextButton();

            if (_loginOptions.Branches.Count == 1 &&
                _loginOptions.Years.Count == 1 &&
                defaultBranch != null &&
                defaultYear != null)
            {
                await CompleteLoginAsync(defaultBranch, defaultYear);
            }
        }
        catch (Exception ex)
        {
            ResetContextOptions();
            CredentialsPanel.IsVisible = true;
            ContextPanel.IsVisible = false;
            StepTitleLabel.Text = "أدخل بيانات الحساب أولاً";
            ShowError(MobileApiErrorHandler.GetUserMessage(ex));
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        HideError();

        if (_loginOptions == null ||
            BranchPicker.SelectedItem is not LoginBranchOptionDto branch ||
            YearPicker.SelectedItem is not LoginYearOptionDto year)
        {
            ShowError("اختر الفرع والسنة المالية.");
            return;
        }

        var currentCompany = (CompanyPicker.SelectedItem as LoginCompanyOptionDto)?.Company_ID?.Trim();
        var currentLogin = LoginNameEntry.Text?.Trim();
        if (!string.Equals(currentCompany, _optionsCompanyId, StringComparison.Ordinal) ||
            !string.Equals(currentLogin, _optionsLoginName, StringComparison.OrdinalIgnoreCase))
        {
            ResetContextOptions();
            CredentialsPanel.IsVisible = true;
            ContextPanel.IsVisible = false;
            StepTitleLabel.Text = "أعد تحميل الفروع والسنوات بعد تعديل بيانات الحساب";
            ShowError("تم تغيير بيانات الحساب. اضغط متابعة لتحميل الفروع والسنوات من جديد.");
            return;
        }

        SetBusy(true);
        try
        {
            await CompleteLoginAsync(branch, year);
        }
        catch (Exception ex)
        {
            ShowError(MobileApiErrorHandler.GetUserMessage(ex));
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task CompleteLoginAsync(LoginBranchOptionDto branch, LoginYearOptionDto year)
    {
        if (_loginOptions == null)
            throw new InvalidOperationException("لم يتم تحميل سياق الدخول.");

        var result = await _authentication.LoginAsync(new LoginRequestDto
        {
            Company_ID = _loginOptions.Company_ID,
            Branch_ID = branch.Branch_ID,
            Year_ID = year.Year_ID,
            User_ID = _loginOptions.User_ID,
            Login_Name = LoginNameEntry.Text.Trim(),
            Password = PasswordEntry.Text,
            Device_ID = GetDeviceId()
        });

        PasswordEntry.Text = string.Empty;
        OpenHome(result.Full_Name, result.Company_ID, result.Branch_ID, result.Year_ID);
    }

    private void OpenHome(string fullName, string companyId, int branchId, int yearId)
    {
        if (Application.Current?.Windows.FirstOrDefault() is not Window window)
            return;

        window.Page = new NavigationPage(new HomePage(
            _mobileHomeService,
            _authentication,
            fullName,
            companyId,
            branchId,
            yearId))
        {
            FlowDirection = FlowDirection.RightToLeft,
            BarBackgroundColor = Color.FromArgb("#17324D"),
            BarTextColor = Colors.White
        };
    }

    private void OnBackClicked(object? sender, EventArgs e)
    {
        ResetContextOptions();
        ContextPanel.IsVisible = false;
        CredentialsPanel.IsVisible = true;
        StepTitleLabel.Text = "أدخل بيانات الحساب أولاً";
        HideError();
        UpdateCredentialButtons();
    }

    private void ResetContextOptions()
    {
        _loginOptions = null;
        _optionsCompanyId = null;
        _optionsLoginName = null;
        BranchPicker.ItemsSource = null;
        BranchPicker.SelectedItem = null;
        YearPicker.ItemsSource = null;
        YearPicker.SelectedItem = null;
        LoginButton.IsEnabled = false;
    }

    private void UpdateCredentialButtons()
    {
        var ready = CompanyPicker.SelectedItem is LoginCompanyOptionDto &&
                    !string.IsNullOrWhiteSpace(LoginNameEntry.Text) &&
                    !string.IsNullOrWhiteSpace(PasswordEntry.Text);
        LoadOptionsButton.IsEnabled = !_isBusy && ready;
    }

    private void UpdateContextButton()
    {
        LoginButton.IsEnabled = !_isBusy &&
                                _loginOptions != null &&
                                BranchPicker.SelectedItem is LoginBranchOptionDto &&
                                YearPicker.SelectedItem is LoginYearOptionDto;
    }

    private void SetBusy(bool isBusy)
    {
        _isBusy = isBusy;
        BusyIndicator.IsVisible = isBusy;
        BusyIndicator.IsRunning = isBusy;

        CompanyPicker.IsEnabled = !isBusy;
        LoginNameEntry.IsEnabled = !isBusy;
        PasswordEntry.IsEnabled = !isBusy;
        BranchPicker.IsEnabled = !isBusy;
        YearPicker.IsEnabled = !isBusy;

        UpdateCredentialButtons();
        UpdateContextButton();
    }

    private void ShowError(string message)
    {
        StatusLabel.Text = message;
        StatusLabel.IsVisible = true;
    }

    private void HideError()
    {
        StatusLabel.Text = string.Empty;
        StatusLabel.IsVisible = false;
    }

    private static string GetDeviceId() =>
        $"{DeviceInfo.Current.Platform}-{DeviceInfo.Current.Model}";

    private void InitializeDevelopmentConnectionSettings()
    {
#if DEBUG
        DevelopmentConnectionPanel.IsVisible = true;
        _isLoadingConnectionSettings = true;
        try
        {
            ConnectionModePicker.SelectedIndex = (int)_connectionConfiguration.ConnectionMode;
            WifiBaseAddressEntry.Text = _connectionConfiguration.WifiBaseAddress;
            WifiPortEntry.Text = _connectionConfiguration.Port.ToString();
        }
        finally
        {
            _isLoadingConnectionSettings = false;
        }
        RefreshWifiConnectionFields();
        ConnectionStatusLabel.Text = "لم يتم اختبار الاتصال بعد.";
#endif
    }

    private void SaveDevelopmentConnectionSettings()
    {
#if DEBUG
        var port = int.TryParse(WifiPortEntry.Text, out var parsedPort) ? parsedPort : 5021;
        var mode = (ApiConnectionMode)Math.Clamp(ConnectionModePicker.SelectedIndex, 0, 2);
        _connectionConfiguration.SaveSettings(mode, WifiBaseAddressEntry.Text, port);
        DevelopmentDatabaseLabel.Text = string.Empty;
        _companiesLoaded = false;
#endif
    }

    private void RefreshWifiConnectionFields()
    {
#if DEBUG
        // تبقى الحاوية والـPicker ثابتين؛ يتغير فقط ما يلزم لطريقة USB.
        WifiConnectionFields.IsVisible = ConnectionModePicker.SelectedIndex != (int)ApiConnectionMode.USB;
#endif
    }
}
