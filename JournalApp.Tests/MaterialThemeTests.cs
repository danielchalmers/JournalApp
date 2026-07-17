using MudBlazor.Utilities;

namespace JournalApp.Tests;

public class MaterialThemeTests
{
    private static string Hex(MudColor color) => color.ToString(MudColorOutputFormats.Hex).ToUpperInvariant();

    [Fact]
    public void DefaultSeedReproducesOrchidPalette()
    {
        // The runtime generator must match the palette that was previously generated offline with material-color-utilities, so a regression here means the HCT math or tone mapping drifted.
        var theme = MaterialTheme.FromSeed(MaterialTheme.DefaultSeed);

        Hex(theme.PaletteLight.Primary).Should().Be("#844C72");
        Hex(theme.PaletteLight.PrimaryLighten).Should().Be("#FFD8EE");
        Hex(theme.PaletteLight.PrimaryDarken).Should().Be("#69345A");
        Hex(theme.PaletteLight.Secondary).Should().Be("#705766");
        Hex(theme.PaletteLight.Tertiary).Should().Be("#81533F");
        Hex(theme.PaletteLight.Error).Should().Be("#BA1A1A");
        Hex(theme.PaletteLight.Info).Should().Be("#1A59C2");
        Hex(theme.PaletteLight.Success).Should().Be("#426900");
        Hex(theme.PaletteLight.Warning).Should().Be("#994704");
        Hex(theme.PaletteLight.Background).Should().Be("#FFF8F9");
        Hex(theme.PaletteLight.BackgroundGray).Should().Be("#FEF0F5");
        Hex(theme.PaletteLight.Surface).Should().Be("#F8EAF0");
        Hex(theme.PaletteLight.GrayLighter).Should().Be("#F2E5EA");
        Hex(theme.PaletteLight.GrayLight).Should().Be("#EDDFE4");
        Hex(theme.PaletteLight.TextPrimary).Should().Be("#201A1D");
        Hex(theme.PaletteLight.TextSecondary).Should().Be("#4F444A");
        Hex(theme.PaletteLight.LinesDefault).Should().Be("#D3C2CA");
        Hex(theme.PaletteLight.LinesInputs).Should().Be("#81737A");
        Hex(theme.PaletteLight.Dark).Should().Be("#362E32");
        Hex(theme.PaletteLight.DarkContrastText).Should().Be("#FBEDF3");

        Hex(theme.PaletteDark.Primary).Should().Be("#F7B1DE");
        Hex(theme.PaletteDark.PrimaryContrastText).Should().Be("#4F1E42");
        Hex(theme.PaletteDark.PrimaryLighten).Should().Be("#69345A");
        Hex(theme.PaletteDark.PrimaryDarken).Should().Be("#FFD8EE");
        Hex(theme.PaletteDark.Secondary).Should().Be("#DDBECF");
        Hex(theme.PaletteDark.Tertiary).Should().Be("#F4B9A0");
        Hex(theme.PaletteDark.Error).Should().Be("#FFB4AB");
        Hex(theme.PaletteDark.Info).Should().Be("#B0C6FF");
        Hex(theme.PaletteDark.Success).Should().Be("#A0D756");
        Hex(theme.PaletteDark.Warning).Should().Be("#FFB68C");
        Hex(theme.PaletteDark.Background).Should().Be("#181215");
        Hex(theme.PaletteDark.BackgroundGray).Should().Be("#201A1D");
        Hex(theme.PaletteDark.Surface).Should().Be("#251E22");
        Hex(theme.PaletteDark.GrayLighter).Should().Be("#2F282C");
        Hex(theme.PaletteDark.GrayLight).Should().Be("#3B3337");
        Hex(theme.PaletteDark.TextPrimary).Should().Be("#EDDFE4");
        Hex(theme.PaletteDark.TextSecondary).Should().Be("#D3C2CA");
        Hex(theme.PaletteDark.LinesDefault).Should().Be("#4F444A");
        Hex(theme.PaletteDark.LinesInputs).Should().Be("#9B8D94");
        Hex(theme.PaletteDark.Dark).Should().Be("#EDDFE4");
        Hex(theme.PaletteDark.DarkContrastText).Should().Be("#362E32");
    }

    [Fact]
    public void OtherSeedsProduceDistinctPalettes()
    {
        var blue = MaterialTheme.FromSeed(0xFF4285F4);

        Hex(blue.PaletteLight.Primary).Should().NotBe("#844C72");
        Hex(blue.PaletteLight.Background).Should().NotBe(Hex(blue.PaletteLight.Surface), "the surface container ladder should keep distinct tones");
    }
}
