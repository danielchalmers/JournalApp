using MaterialColorUtilities.Maui;
using MaterialColorUtilities.Palettes;
using MaterialColorUtilities.Schemes;
using MudBlazor;

namespace JournalApp.Data;

/// <summary>
/// Service that manages dynamic Material You theming for MudBlazor.
/// Generates color palettes from system colors (Android API 27+) or fallback seed.
/// </summary>
public sealed class ThemeService
{
    private readonly ILogger<ThemeService> _logger;
    private readonly MaterialColorService _materialColorService;

    public ThemeService(ILogger<ThemeService> logger, MaterialColorService materialColorService)
    {
        _logger = logger;
        _materialColorService = materialColorService;
    }

    /// <summary>
    /// Event raised when the theme palette changes.
    /// </summary>
    public event EventHandler PaletteChanged;

    /// <summary>
    /// Creates a MudBlazor theme with dynamically generated Material You colors.
    /// </summary>
    public MudTheme CreateMudTheme()
    {
        // Generate both light and dark color schemes manually from a seed
        // This approach ensures we have both palettes regardless of current system theme
        uint seed = ThemeConstants.DefaultSeedColor;
        
        try
        {
            if (_materialColorService.Seed != 0)
            {
                seed = _materialColorService.Seed;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing MaterialColorService.Seed, using fallback");
        }
        
        _logger.LogInformation("Creating MudTheme from seed: #{Seed:X6}", seed);

        // Create core palette from seed
        var corePalette = CorePalette.Of(seed);
        
        // Generate light and dark schemes from the core palette
        var lightScheme = new LightSchemeMapper().Map(corePalette);
        var darkScheme = new DarkSchemeMapper().Map(corePalette);

        // Convert to MAUI colors
        var lightSchemeMaui = lightScheme.Convert(x => Microsoft.Maui.Graphics.Color.FromUint(x));
        var darkSchemeMaui = darkScheme.Convert(x => Microsoft.Maui.Graphics.Color.FromUint(x));

        _logger.LogInformation("MudTheme created. Light Primary: {LightPrimary}, Dark Primary: {DarkPrimary}",
            lightSchemeMaui.Primary.ToHex(), darkSchemeMaui.Primary.ToHex());

        return new MudTheme
        {
            PaletteLight = new PaletteLight()
            {
                Primary = lightSchemeMaui.Primary.ToHex(),
                TextPrimary = lightSchemeMaui.OnSurface.ToHex(),
                PrimaryContrastText = lightSchemeMaui.OnPrimary.ToHex(),
                Secondary = lightSchemeMaui.Secondary.ToHex(),
                TextSecondary = lightSchemeMaui.OnSecondary.ToHex(),
                SecondaryContrastText = lightSchemeMaui.OnSecondary.ToHex(),
                Error = lightSchemeMaui.Error.ToHex(),
                Background = lightSchemeMaui.Background.ToHex(),
                Surface = lightSchemeMaui.Surface.ToHex(),

                HoverOpacity = 0.1,
            },

            PaletteDark = new PaletteDark()
            {
                Primary = darkSchemeMaui.Primary.ToHex(),
                TextPrimary = darkSchemeMaui.OnSurface.ToHex(),
                PrimaryContrastText = darkSchemeMaui.OnPrimary.ToHex(),
                Secondary = darkSchemeMaui.Secondary.ToHex(),
                TextSecondary = darkSchemeMaui.OnSecondary.ToHex(),
                Error = darkSchemeMaui.Error.ToHex(),
                Background = darkSchemeMaui.Background.ToHex(),
                Surface = darkSchemeMaui.Surface.ToHex(),

                HoverOpacity = 0.1,
            },

            LayoutProperties = new()
            {
                DefaultBorderRadius = "4px",
            },

            Typography = new()
            {
                Button = new ButtonTypography()
                {
                    TextTransform = "none",
                },

                Caption = new CaptionTypography()
                {
                    LineHeight = "1",
                },
            },
        };
    }

    /// <summary>
    /// Updates the seed color and regenerates the palette.
    /// </summary>
    public void SetSeedColor(uint seed)
    {
        _logger.LogInformation("Setting seed color to: #{Seed:X6}", seed);
        _materialColorService.Seed = seed;
        OnPaletteChanged();
    }

    private void OnPaletteChanged()
    {
        PaletteChanged?.Invoke(this, EventArgs.Empty);
    }
}
