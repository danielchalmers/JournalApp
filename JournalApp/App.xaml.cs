namespace JournalApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new MainPage();
    }

    public static Window Window { get; private set; }
    
    public static event EventHandler<string> NewIntent;

    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = base.CreateWindow(activationState);
        Window = window;

        Window.Destroying += App_Destroying;

        return window;
    }

    private void App_Destroying(object sender, EventArgs e)
    {
        Preferences.Set("index_left_at", null);
    }

    public static void OnNewIntent(object sender, string activatedFilePath)
    {
        NewIntent?.Invoke(sender, activatedFilePath);
    }
}