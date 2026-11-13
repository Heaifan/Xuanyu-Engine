using Avalonia.Controls;
using Avalonia.Media;
using SvgPath = Avalonia.Controls.Shapes.Path;

namespace XuanYu.Editor.UI;

internal static class TerrainUiGlyphs
{
    public static Control ChevronDown() => Create("M 2 3 L 5 6 L 8 3");
    public static Control ChevronRight() => Create("M 3 2 L 6 5 L 3 8");

    public static Control DemFile() => new SvgPath
    {
        Width = 14,
        Height = 14,
        Stretch = Stretch.Uniform,
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        Stroke = EditorTheme.Accent,
        StrokeThickness = 1.2,
        Data = PathGeometry.Parse(
            "M 3 1 H 9 L 12 4 V 12 H 3 Z " +
            "M 9 1 V 4 H 12 M 5 6 H 10 M 5 8 H 10 M 5 10 H 9")
    };

    private static Control Create(string data) => new SvgPath
    {
        Width = 10,
        Height = 10,
        Stretch = Stretch.Uniform,
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        Stroke = EditorTheme.TextMuted,
        StrokeThickness = 1.4,
        Data = PathGeometry.Parse(data)
    };
}