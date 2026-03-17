using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Platform;

namespace JournalApp;

public sealed partial class PreferenceService : IPreferences, IDisposable
{
    private readonly ILogger<PreferenceService> logger;
    private readonly IPreferences _preferenceStore;
    private readonly Application _application;
    private Dictionary<string, string> _moodColors;
    private AppTheme? _theme;

    public PreferenceService(ILogger<PreferenceService> logger, IPreferences preferenceStore)
    {
        this.logger = logger;
        _preferenceStore = preferenceStore;

        // Not available in unit tests.
        if (Application.Current != null)
        {
            _application = Application.Current;
            _application.RequestedThemeChanged += Application_RequestedThemeChanged;
        }

        UpdateStatusBar();
        GenerateMoodColors();
    }

    public AppTheme SelectedAppTheme
    {
        get
        {
            if (_theme == null)
            {
                if (Enum.TryParse<AppTheme>(_preferenceStore.Get("theme", string.Empty), out var parsed))
                    _theme = parsed;
                else
                    _theme = AppTheme.Unspecified;
            }

            return _theme.Value;
        }
        set
        {
            logger.LogInformation($"Changing theme from {_theme} to {value}");
            _preferenceStore.Set("theme", value.ToString());
            _theme = value;
            OnThemeChanged();
        }
    }

    public bool IsDarkMode => SelectedAppTheme switch
    {
        AppTheme.Unspecified => Application.Current?.RequestedTheme != AppTheme.Light,
        AppTheme.Light => false,
        _ => true,
    };

    public bool HideNotes
    {
        get => _preferenceStore.Get("hide_notes", false);
        set => _preferenceStore.Set("hide_notes", value);
    }

    public SafetyPlan SafetyPlan
    {
        get => _preferenceStore.GetJson<SafetyPlan>("safety_plan");
        set
        {
            // Save the safety plan or discard if it's unchanged so it won't show in the main menu.
            if (value == new SafetyPlan())
                _preferenceStore.Set<string>("safety_plan", null);
            else
                _preferenceStore.SetJson("safety_plan", value);
        }
    }

    public DateTimeOffset LastExportDate
    {
        get
        {
            var lastExportString = _preferenceStore.Get<string>("last_export", null);

            if (DateTimeOffset.TryParse(lastExportString, out var parsed))
            {
                return parsed;
            }
            else
            {
                // We haven't tracked this or it's malformed so set it to now.
                LastExportDate = DateTimeOffset.Now;
                return DateTimeOffset.Now;
            }
        }
        set => _preferenceStore.Set("last_export", value.ToString("O"));
    }

    public event EventHandler<bool> ThemeChanged;

    private void Application_RequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
    {
        _theme = e.RequestedTheme;
        OnThemeChanged();
    }

    private void OnThemeChanged()
    {
        UpdateStatusBar();
        ThemeChanged?.Invoke(this, IsDarkMode);
    }

    private void UpdateStatusBar()
    {
        if (OperatingSystem.IsAndroid())
        {
            logger.LogDebug("Updating status bar");
#pragma warning disable CS0618 // Type or member is obsolete
            if (IsDarkMode)
            {
                StatusBar.SetColor(Color.FromHex("#EAB8D6"));
                StatusBar.SetStyle(StatusBarStyle.DarkContent);
            }
            else
            {
                StatusBar.SetColor(Color.FromHex("#854C73"));
                StatusBar.SetStyle(StatusBarStyle.LightContent);
            }
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }

    private void GenerateMoodColors()
    {
        var emojis = DataPoint.Moods.Where(x => x != "🤔").ToList();
#pragma warning disable CS0618 // Type or member is obsolete
        var primary = Color.FromHex("#FF9FDF");
#pragma warning restore CS0618 // Type or member is obsolete
        var complementary = primary.GetComplementary();

        _moodColors = [];
        for (var i = 0; i < emojis.Count; i++)
        {
            var p = i / (emojis.Count - 1f);
            var c = ColorUtil.GetGradientColor(primary, complementary, p);

            _moodColors.Add(emojis[i], c.ToHex());
        }

        logger.LogInformation($"Primary color: {primary.ToHex()}");
        logger.LogInformation($"Palette: {string.Join(",", _moodColors)}");
    }

    public string GetMoodColor(string emoji)
    {
        if (string.IsNullOrEmpty(emoji) || !_moodColors.TryGetValue(emoji, out var color))
            return "transparent";
        else
            return color;
    }

    public void Set(IReadOnlyDictionary<string, string> preferences)
    {
        Clear();
        foreach (var (key, value) in preferences)
        {
            Set(key, value);
            logger.LogInformation($"Preference restored: {key}");
        }
    }

    public IEnumerable<KeyValuePair<string, string>> Get(params string[] keys)
    {
        foreach (var key in keys)
        {
            yield return new(key, Get(key, string.Empty));
        }
    }

    public void Dispose()
    {
        if (_application != null)
            _application.RequestedThemeChanged -= Application_RequestedThemeChanged;
    }

    public bool ContainsKey(string key, string sharedName = null) => _preferenceStore.ContainsKey(key, sharedName);
    public void Remove(string key, string sharedName = null) => _preferenceStore.Remove(key, sharedName);
    public void Clear(string sharedName = null) => _preferenceStore.Clear(sharedName);
    public void Set<T>(string key, T value, string sharedName = null) => _preferenceStore.Set(key, value, sharedName);
    public T Get<T>(string key, T defaultValue, string sharedName = null) => _preferenceStore.Get(key, defaultValue, sharedName);
}
