using Color = Microsoft.Maui.Graphics.Color;

namespace JournalApp;

public static class ColorUtil
{
    public static Color GetComplementary(this Color color)
    {
        var hue = color.GetHue();
        return color.WithHue((hue + 0.5f) % 1f);
    }

    public static IEnumerable<Color> RgbGradientTo(this Color start, Color end, int steps)
    {
        var rStep = (end.Red - start.Red) / (steps - 1);
        var gStep = (end.Green - start.Green) / (steps - 1);
        var bStep = (end.Blue - start.Blue) / (steps - 1);

        for (var i = 0; i < steps; i++)
        {
            var r = start.Red + (i * rStep);
            var g = start.Green + (i * gStep);
            var b = start.Blue + (i * bStep);

            yield return Color.FromRgb(r, g, b);
        }
    }
}
