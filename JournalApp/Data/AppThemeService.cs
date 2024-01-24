namespace JournalApp;

public class AppThemeService : IDisposable
{
    private readonly Application _application;
    private AppTheme _theme;

    public AppThemeService()
    {
        _application = Application.Current;
        _application.RequestedThemeChanged += Application_RequestedThemeChanged;
    }

    public AppTheme SelectedAppTheme
    {   
        get => _theme;
        set
        {
            _theme = value;
            OnThemeChanged();
        }
    }

    public event EventHandler<bool> ThemeChanged;

    private void Application_RequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
    {
        _theme = e.RequestedTheme;
        OnThemeChanged();
    }

    private void OnThemeChanged()
    {
        var isDarkMode = _theme switch
        {
            AppTheme.Unspecified => Application.Current.RequestedTheme != AppTheme.Light,
            AppTheme.Light => false,
            _ => true,
        };

        ThemeChanged?.Invoke(this, isDarkMode);
    }

    public void Dispose()
    {
        _application.RequestedThemeChanged -= Application_RequestedThemeChanged;
    }
}
