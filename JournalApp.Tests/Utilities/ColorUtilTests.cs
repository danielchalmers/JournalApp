using Color = Microsoft.Maui.Graphics.Color;

namespace JournalApp.Tests;

/// <summary>
/// ColorUtil supplies the channel-wise gradient math that builds the mood palette.
/// A naive refactor of either method (dropping the hue wraparound, or lerping the wrong channels)
/// would silently shift every mood colour.
/// </summary>
public class ColorUtilTests
{
    private static readonly Color Red = new(1f, 0f, 0f);
    private static readonly Color Blue = new(0f, 0f, 1f);

    [Fact]
    public void GetGradientColor_AtZero_ReturnsStart()
    {
        var result = ColorUtil.GetGradientColor(Red, Blue, 0f);
        result.Red.Should().BeApproximately(1f, 0.001f);
        result.Green.Should().BeApproximately(0f, 0.001f);
        result.Blue.Should().BeApproximately(0f, 0.001f);
    }

    [Fact]
    public void GetGradientColor_AtOne_ReturnsEnd()
    {
        var result = ColorUtil.GetGradientColor(Red, Blue, 1f);
        result.Red.Should().BeApproximately(0f, 0.001f);
        result.Green.Should().BeApproximately(0f, 0.001f);
        result.Blue.Should().BeApproximately(1f, 0.001f);
    }

    [Fact]
    public void GetGradientColor_AtHalf_InterpolatesEachChannel()
    {
        var result = ColorUtil.GetGradientColor(Red, Blue, 0.5f);
        result.Red.Should().BeApproximately(0.5f, 0.001f);
        result.Green.Should().BeApproximately(0f, 0.001f);
        result.Blue.Should().BeApproximately(0.5f, 0.001f);
    }

    [Fact]
    public void GetComplementary_ShiftsHueByHalf()
    {
        var complementary = Red.GetComplementary(); // hue 0 -> 0.5
        complementary.GetHue().Should().BeApproximately(0.5f, 0.01f);
    }

    [Fact]
    public void GetComplementary_WrapsHueAroundOne()
    {
        var start = Red.WithHue(0.75f);
        var complementary = start.GetComplementary(); // 0.75 + 0.5 = 1.25 -> 0.25
        complementary.GetHue().Should().BeApproximately(0.25f, 0.01f);
    }
}
