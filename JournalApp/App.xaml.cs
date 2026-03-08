namespace JournalApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    public static Window Window { get; private set; }

    public static (DateTimeOffset LeftAt, DateOnly LastDate)? IndexDateState { get; set; }

    public int LaunchCount
    {
        get => Preferences.Get("launches", 0);
        set => Preferences.Set("launches", value);
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = new Window(new MainPage());
        Window = window;

        LaunchCount++;

        return window;
    }
}
