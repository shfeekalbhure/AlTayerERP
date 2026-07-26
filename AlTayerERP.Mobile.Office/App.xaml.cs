namespace AlTayerERP.Mobile.Office;

public partial class App : Application
{
    private readonly MainPage _mainPage;

    public App(MainPage mainPage)
    {
        InitializeComponent();
        _mainPage = mainPage;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new NavigationPage(_mainPage)
        {
            FlowDirection = FlowDirection.RightToLeft,
            BarBackgroundColor = Color.FromArgb("#17324D"),
            BarTextColor = Colors.White
        });
    }
}
