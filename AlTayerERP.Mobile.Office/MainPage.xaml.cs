using AlTayerERP.Mobile.Office.DTOs;
using AlTayerERP.Mobile.Office.Services;

namespace AlTayerERP.Mobile.Office;

public partial class MainPage : ContentPage
{
    private readonly AuthenticationService _authentication;

    public MainPage(AuthenticationService authentication)
    {
        InitializeComponent();
        _authentication = authentication;
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        StatusLabel.IsVisible = false;

        if (!int.TryParse(BranchEntry.Text, out var branchId) ||
            !int.TryParse(YearEntry.Text, out var yearId) ||
            string.IsNullOrWhiteSpace(CompanyEntry.Text) ||
            string.IsNullOrWhiteSpace(LoginNameEntry.Text) ||
            string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            ShowError("أدخل الشركة والفرع والسنة واسم المستخدم وكلمة المرور.");
            return;
        }

        SetBusy(true);
        try
        {
            var result = await _authentication.LoginAsync(new LoginRequestDto
            {
                Company_ID = CompanyEntry.Text.Trim(),
                Branch_ID = branchId,
                Year_ID = yearId,
                Login_Name = LoginNameEntry.Text.Trim(),
                Password = PasswordEntry.Text,
                Device_ID = $"{DeviceInfo.Current.Platform}-{DeviceInfo.Current.Model}"
            });

            PasswordEntry.Text = string.Empty;
            await DisplayAlertAsync("تم تسجيل الدخول", $"مرحباً {result.Full_Name}", "موافق");
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

    private void SetBusy(bool isBusy)
    {
        BusyIndicator.IsVisible = isBusy;
        BusyIndicator.IsRunning = isBusy;
        LoginButton.IsEnabled = !isBusy;
    }

    private void ShowError(string message)
    {
        StatusLabel.Text = message;
        StatusLabel.IsVisible = true;
    }
}
