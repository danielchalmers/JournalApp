namespace JournalApp;

public sealed class AppThemeService : IDisposable
{
    private readonly ILogger<AppThemeService> _logger;
    private readonly Application _application;
    private AppTheme _theme;

    public AppThemeService(ILogger<AppThemeService> logger)
    {
        _logger = logger;
        _application = Application.Current;
        if (_application != null) // Unit testing?
            _application.RequestedThemeChanged += Application_RequestedThemeChanged;
    }

    public AppTheme SelectedAppTheme
    {
        get => _theme;
        set
        {
            _logger.LogInformation($"Changing theme from {_theme} to {value}");
            _theme = value;
            OnThemeChanged();
        }
    }

    public bool IsDarkMode => _theme switch
    {
        AppTheme.Unspecified => Application.Current.RequestedTheme != AppTheme.Light,
        AppTheme.Light => false,
        _ => true,
    };

    public event EventHandler<bool> ThemeChanged;

    private void Application_RequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
    {
        _theme = e.RequestedTheme;
        OnThemeChanged();
    }

    private void OnThemeChanged()
    {
        ThemeChanged?.Invoke(this, IsDarkMode);
    }

    public void Dispose()
    {
        if (_application != null) // Unit testing?
            _application.RequestedThemeChanged -= Application_RequestedThemeChanged;
    }
}
