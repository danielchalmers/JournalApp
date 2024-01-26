namespace JournalApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new MainPage();
    }

    public static Window Window { get; private set; }

    public static string ActivatedFilePath { get; set; }

    public static (DateTimeOffset LeftAt, DateOnly LastDate)? IndexDateState { get; set; }

    public static event EventHandler NewIntent;

    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = base.CreateWindow(activationState);
        Window = window;

        return window;
    }

    public static void OnNewIntent(object sender)
    {
        NewIntent?.Invoke(sender, EventArgs.Empty);
    }
}