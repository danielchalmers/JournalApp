using Color = Microsoft.Maui.Graphics.Color;

namespace JournalApp;

public static class ColorUtil
{
    public static Color GetComplementary(this Color color)
    {
        var hue = color.GetHue();
        return color.WithHue((hue + 0.5f) % 1f);
    }

    public static Color GetGradientColor(Color start, Color end, double percentage)
    {
        var r = start.Red + ((end.Red - start.Red) * percentage);
        var g = start.Green + ((end.Green - start.Green) * percentage);
        var b = start.Blue + ((end.Blue - start.Blue) * percentage);

        return Color.FromRgb(r, g, b);
    }
}
