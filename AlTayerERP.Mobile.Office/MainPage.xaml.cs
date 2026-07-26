using AlTayerERP.Mobile.Office.DTOs;
using AlTayerERP.Mobile.Office.Services;

namespace AlTayerERP.Mobile.Office;

public partial class MainPage : ContentPage
{
    private readonly AuthenticationService _authentication;
    private LoginOptionsResponseDto? _loginOptions;

    public MainPage(AuthenticationService authentication)
    {
        InitializeComponent();
        _authentication = authentication;
    }

    private async void OnLoadOptionsClicked(object? sender, EventArgs e)
    {
        HideError();

        if (string.IsNullOrWhiteSpace(CompanyEntry.Text) ||
            string.IsNullOrWhiteSpace(LoginNameEntry.Text) ||
            string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            ShowError("أدخل رمز الشركة واسم المستخدم وكلمة المرور.");
            return;
        }

        SetBusy(true);
        try
        {
            _loginOptions = await _authentication.GetLoginOptionsAsync(new LoginOptionsRequestDto
            {
                Company_ID = CompanyEntry.Text.Trim(),
                Login_Name = LoginNameEntry.Text.Trim(),
                Password = PasswordEntry.Text,
                Device_ID = GetDeviceId()
            });

            BranchPicker.ItemsSource = _loginOptions.Branches;
            YearPicker.ItemsSource = _loginOptions.Years;

            BranchPicker.SelectedItem = _loginOptions.Branches.FirstOrDefault(x => x.Is_Default)
                                        ?? _loginOptions.Branches.FirstOrDefault();
            YearPicker.SelectedItem = _loginOptions.Years.FirstOrDefault(x => x.Is_Default)
                                      ?? _loginOptions.Years.FirstOrDefault();

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
            await DisplayAlert("تم تسجيل الدخول", $"مرحباً {result.Full_Name}", "موافق");
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
        LoadOptionsButton.IsEnabled = !isBusy;
        LoginButton.IsEnabled = !isBusy;
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
