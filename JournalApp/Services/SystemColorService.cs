using Microsoft.Maui.Graphics;

namespace JournalApp;

public sealed class SystemColorService
{
    private const string DefaultSourceColorHex = "#5B5B5B";

    private readonly ILogger<SystemColorService> _logger;
    private string _cachedSourceHex;

    public SystemColorService(ILogger<SystemColorService> logger)
    {
        _logger = logger;
    }

    public string GetSourceColorHex()
    {
        if (!string.IsNullOrWhiteSpace(_cachedSourceHex))
            return _cachedSourceHex;

        var sourceColor = TryGetSystemAccentColor();
        _cachedSourceHex = sourceColor?.ToHex() ?? DefaultSourceColorHex;

        _logger.LogInformation("System source color: {SourceColor}", _cachedSourceHex);
        return _cachedSourceHex;
    }

    private static Color? TryGetSystemAccentColor()
    {
#if ANDROID
        if (OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            try
            {
                var context = Android.App.Application.Context;
                var colorInt = context.GetColor(Android.Resource.Color.SystemAccent1_500);
                return Color.FromArgb(unchecked((uint)colorInt));
            }
            catch
            {
                // Ignore and fallback to the default.
            }
        }
#endif
        return null;
    }
}
