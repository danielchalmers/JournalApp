using MaterialColorUtilities.Blend;
using MaterialColorUtilities.ColorAppearance;
using MaterialColorUtilities.Palettes;
using MudBlazor;

namespace JournalApp;

/// <summary>
/// Builds the app's MudTheme from a Material 3 seed color at runtime, replacing the palette that used to be generated offline with material-color-utilities.
/// </summary>
public static class MaterialTheme
{
    // The stock "Orchid" seed used when device colors are off or unavailable.
    public const uint DefaultSeed = 0xFF854C73;

    /// <summary>
    /// Generates the full theme with the same SchemeTonalSpot palettes and slot conventions the hardcoded Orchid palette was built from.
    /// Slot conventions (same in both modes, so CSS can rely on them):
    ///   Background = M3 surface, Surface = surfaceContainer (cards), BackgroundGray = surfaceContainerLow, GrayLighter = surfaceContainerHigh, GrayLight = surfaceContainerHighest.
    ///   X.Lighten = xContainer, X.Darken = onXContainer, LinesDefault = outlineVariant, LinesInputs = outline, Dark = inverseSurface, DarkContrastText = inverseOnSurface.
    /// </summary>
    public static MudTheme FromSeed(uint seed)
    {
        var seedHct = Hct.FromInt(seed);

        // SchemeTonalSpot palettes: fixed chromas at the seed hue, with the tertiary rotated 60 degrees.
        var primary = TonalPalette.FromHueAndChroma(seedHct.Hue, 36);
        var secondary = TonalPalette.FromHueAndChroma(seedHct.Hue, 16);
        var tertiary = TonalPalette.FromHueAndChroma(seedHct.Hue + 60, 24);
        var neutral = TonalPalette.FromHueAndChroma(seedHct.Hue, 6);
        var neutralVariant = TonalPalette.FromHueAndChroma(seedHct.Hue, 8);
        var error = TonalPalette.FromHueAndChroma(25, 84);

        // Info/Success/Warning are the stock Material hues harmonized toward the seed.
        var info = TonalPalette.FromInt(Blender.Harmonize(0xFF2196F3, seed));
        var success = TonalPalette.FromInt(Blender.Harmonize(0xFF4CAF50, seed));
        var warning = TonalPalette.FromInt(Blender.Harmonize(0xFFFF9800, seed));

        return new MudTheme()
        {
            PaletteLight = new PaletteLight()
            {
                Primary = Hex(primary[40]),
                PrimaryContrastText = "#FFFFFF",
                PrimaryLighten = Hex(primary[90]),
                PrimaryDarken = Hex(primary[30]),
                Secondary = Hex(secondary[40]),
                SecondaryContrastText = "#FFFFFF",
                SecondaryLighten = Hex(secondary[90]),
                SecondaryDarken = Hex(secondary[30]),
                Tertiary = Hex(tertiary[40]),
                TertiaryContrastText = "#FFFFFF",
                TertiaryLighten = Hex(tertiary[90]),
                TertiaryDarken = Hex(tertiary[30]),
                Error = Hex(error[40]),
                ErrorContrastText = "#FFFFFF",
                ErrorLighten = Hex(error[90]),
                ErrorDarken = Hex(error[30]),
                Info = Hex(info[40]),
                InfoContrastText = "#FFFFFF",
                InfoLighten = Hex(info[90]),
                InfoDarken = Hex(info[10]),
                Success = Hex(success[40]),
                SuccessContrastText = "#FFFFFF",
                SuccessLighten = Hex(success[90]),
                SuccessDarken = Hex(success[10]),
                Warning = Hex(warning[40]),
                WarningContrastText = "#FFFFFF",
                WarningLighten = Hex(warning[90]),
                WarningDarken = Hex(warning[10]),
                Background = Hex(neutral[98]),
                BackgroundGray = Hex(neutral[96]),
                Surface = Hex(neutral[94]),
                GrayLighter = Hex(neutral[92]),
                GrayLight = Hex(neutral[90]),
                DrawerBackground = Hex(neutral[96]),
                AppbarBackground = Hex(neutral[98]),
                AppbarText = Hex(neutral[10]),
                TextPrimary = Hex(neutral[10]),
                TextSecondary = Hex(neutralVariant[30]),
                LinesDefault = Hex(neutralVariant[80]),
                LinesInputs = Hex(neutralVariant[50]),
                Divider = Hex(neutralVariant[80]),
                TableLines = Hex(neutralVariant[80]),
                Dark = Hex(neutral[20]),
                DarkContrastText = Hex(neutral[95]),

                HoverOpacity = 0.08,
            },

            PaletteDark = new PaletteDark()
            {
                Primary = Hex(primary[80]),
                PrimaryContrastText = Hex(primary[20]),
                PrimaryLighten = Hex(primary[30]),
                PrimaryDarken = Hex(primary[90]),
                Secondary = Hex(secondary[80]),
                SecondaryContrastText = Hex(secondary[20]),
                SecondaryLighten = Hex(secondary[30]),
                SecondaryDarken = Hex(secondary[90]),
                Tertiary = Hex(tertiary[80]),
                TertiaryContrastText = Hex(tertiary[20]),
                TertiaryLighten = Hex(tertiary[30]),
                TertiaryDarken = Hex(tertiary[90]),
                Error = Hex(error[80]),
                ErrorContrastText = Hex(error[20]),
                ErrorLighten = Hex(error[30]),
                ErrorDarken = Hex(error[90]),
                Info = Hex(info[80]),
                InfoContrastText = Hex(info[20]),
                InfoLighten = Hex(info[30]),
                InfoDarken = Hex(info[90]),
                Success = Hex(success[80]),
                SuccessContrastText = Hex(success[20]),
                SuccessLighten = Hex(success[30]),
                SuccessDarken = Hex(success[90]),
                Warning = Hex(warning[80]),
                WarningContrastText = Hex(warning[20]),
                WarningLighten = Hex(warning[30]),
                WarningDarken = Hex(warning[90]),
                Background = Hex(neutral[6]),
                BackgroundGray = Hex(neutral[10]),
                Surface = Hex(neutral[12]),
                GrayLighter = Hex(neutral[17]),
                GrayLight = Hex(neutral[22]),
                DrawerBackground = Hex(neutral[10]),
                AppbarBackground = Hex(neutral[6]),
                AppbarText = Hex(neutral[90]),
                TextPrimary = Hex(neutral[90]),
                TextSecondary = Hex(neutralVariant[80]),
                LinesDefault = Hex(neutralVariant[30]),
                LinesInputs = Hex(neutralVariant[60]),
                Divider = Hex(neutralVariant[30]),
                TableLines = Hex(neutralVariant[30]),
                Dark = Hex(neutral[90]),
                DarkContrastText = Hex(neutral[20]),

                HoverOpacity = 0.08,
            },

            LayoutProperties = new()
            {
                DefaultBorderRadius = "8px",
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
    /// The tone 50 primary of the device's Material You palette on Android 12+, or null when unsupported.
    /// </summary>
    public static uint? GetDeviceSeed()
    {
#if ANDROID
        if (OperatingSystem.IsAndroidVersionAtLeast(31))
            return (uint)Android.App.Application.Context.GetColor(Android.Resource.Color.SystemAccent1500);
#endif
        return null;
    }

    private static string Hex(uint argb) => "#" + (argb & 0xFFFFFF).ToString("X6");
}
