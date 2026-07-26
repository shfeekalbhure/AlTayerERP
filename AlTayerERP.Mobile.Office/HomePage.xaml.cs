namespace AlTayerERP.Mobile.Office;

public partial class HomePage : ContentPage
{
    public HomePage(string fullName, string companyId, int branchId, int yearId)
    {
        InitializeComponent();
        WelcomeLabel.Text = $"مرحباً {fullName}";
        ContextLabel.Text = $"الشركة: {companyId} | الفرع: {branchId} | السنة: {yearId}";
    }
}
