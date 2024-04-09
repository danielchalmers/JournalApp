using MudBlazor.Utilities;

namespace JournalApp;

public sealed class PreferenceService : IPreferences, IDisposable
{
    private readonly ILogger<PreferenceService> logger;
    private readonly IPreferences _preferenceStore;
    private readonly Application _application;
    private Dictionary<string, string> _moodColors;
    private AppTheme _theme;

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

        GenerateMoodColors();
    }

    public AppTheme SelectedAppTheme
    {
        get => _theme;
        set
        {
            logger.LogInformation($"Changing theme from {_theme} to {value}");
            _preferenceStore.Set("theme", value.ToString());
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

    public bool HideNotes
    {
        get => _preferenceStore.Get("hide_notes", false);
        set => _preferenceStore.Set("hide_notes", value);
    }

    public MudColor PrimaryColor
    {
        get
        {
            var palette = _preferenceStore.Get("mood_palette", string.Empty);

            if (string.IsNullOrEmpty(palette))
                palette = "#6bdbe7"; // Tetradic to our primary purple.

            return palette;
        }
        set
        {
            _preferenceStore.Set("mood_palette", value.Value[..^2]);
            GenerateMoodColors();
        }
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

    private void GenerateMoodColors()
    {
        var emojis = DataPoint.Moods.Where(x => x != "🤔").ToList();
        var primary = PrimaryColor.ToMauiColor();
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
