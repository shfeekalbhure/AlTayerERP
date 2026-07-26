using AlTayerERP.Mobile.Office.DTOs;
using AlTayerERP.Mobile.Office.Services;

namespace AlTayerERP.Mobile.Office;

public partial class MainPage : ContentPage
{
    private readonly AuthenticationService _authentication;
    private LoginOptionsResponseDto? _loginOptions;
    private bool _companiesLoaded;

    public MainPage(AuthenticationService authentication)
    {
        InitializeComponent();
        _authentication = authentication;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_companiesLoaded)
            return;

        await LoadCompaniesAsync();
    }

    private async Task LoadCompaniesAsync()
    {
        HideError();
        CompaniesStatusLabel.IsVisible = true;
        CompaniesStatusLabel.Text = "جاري تحميل الشركات...";
        LoadOptionsButton.IsEnabled = false;

        try
        {
            var companies = await _authentication.GetLoginCompaniesAsync();
            CompanyPicker.ItemsSource = companies;

            if (companies.Count == 1)
                CompanyPicker.SelectedItem = companies[0];

            CompaniesStatusLabel.Text = companies.Count == 0
                ? "لا توجد شركات نشطة متاحة."
                : companies.Count == 1
                    ? "تم تحديد الشركة تلقائياً."
                    : "اختر الشركة من القائمة.";

            LoadOptionsButton.IsEnabled = companies.Count > 0;
            _companiesLoaded = true;
        }
        catch (Exception ex)
        {
            CompaniesStatusLabel.Text = "تعذر تحميل الشركات.";
            ShowError(ex.Message);
        }
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

        SetBusy(true);
        try
        {
            _loginOptions = await _authentication.GetLoginOptionsAsync(new LoginOptionsRequestDto
            {
                Company_ID = company.Company_ID,
                Login_Name = LoginNameEntry.Text.Trim(),
                Password = PasswordEntry.Text,
                Device_ID = GetDeviceId()
            });

            BranchPicker.ItemsSource = _loginOptions.Branches;
            YearPicker.ItemsSource = _loginOptions.Years;

            var defaultBranch = _loginOptions.Branches.FirstOrDefault(x => x.Is_Default)
                                ?? _loginOptions.Branches.FirstOrDefault();
            var defaultYear = _loginOptions.Years.FirstOrDefault(x => x.Is_Default)
                              ?? _loginOptions.Years.FirstOrDefault();

            BranchPicker.SelectedItem = defaultBranch;
            YearPicker.SelectedItem = defaultYear;

            // عند وجود خيار واحد فقط في كل قائمة لا نعرض خطوة إضافية للمستخدم.
            if (_loginOptions.Branches.Count == 1 &&
                _loginOptions.Years.Count == 1 &&
                defaultBranch != null &&
                defaultYear != null)
            {
                await CompleteLoginAsync(defaultBranch, defaultYear);
                return;
            }

            UserWelcomeLabel.Text = $"مرحباً {_loginOptions.Full_Name}";
            CredentialsPanel.IsVisible = false;
            ContextPanel.IsVisible = true;
            StepTitleLabel.Text = "اختر الفرع والسنة المالية";
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
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

        SetBusy(true);
        try
        {
            await CompleteLoginAsync(branch, year);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
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
        await Navigation.PushAsync(new HomePage(
            result.Full_Name,
            result.Company_ID,
            result.Branch_ID,
            result.Year_ID));
    }

    private void OnBackClicked(object? sender, EventArgs e)
    {
        _loginOptions = null;
        BranchPicker.ItemsSource = null;
        YearPicker.ItemsSource = null;
        ContextPanel.IsVisible = false;
        CredentialsPanel.IsVisible = true;
        StepTitleLabel.Text = "أدخل بيانات الحساب أولاً";
        HideError();
    }

    private void SetBusy(bool isBusy)
    {
        BusyIndicator.IsVisible = isBusy;
        BusyIndicator.IsRunning = isBusy;
        LoadOptionsButton.IsEnabled = !isBusy && CompanyPicker.ItemsSource != null;
        LoginButton.IsEnabled = !isBusy;
        CompanyPicker.IsEnabled = !isBusy;
        LoginNameEntry.IsEnabled = !isBusy;
        PasswordEntry.IsEnabled = !isBusy;
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
}
