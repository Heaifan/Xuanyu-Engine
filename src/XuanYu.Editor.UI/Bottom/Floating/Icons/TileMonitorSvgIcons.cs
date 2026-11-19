using System.Xml.Linq;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using SvgPath = Avalonia.Controls.Shapes.Path;

namespace XuanYu.Editor.UI;

internal static class TileMonitorSvgIcons
{
    private const string Root = "avares://XuanYu.Editor.UI/Assets/Monitor/";
    private static readonly Dictionary<string, Geometry> Cache = [];

    public static Control Create(string name)
    {
        if (!Cache.TryGetValue(name, out Geometry? geometry))
        {
            using var stream = AssetLoader.Open(new Uri(Root + name + ".svg"));
            XNamespace svg = "http://www.w3.org/2000/svg";
            var paths = XDocument.Load(stream).Descendants(svg + "path")
                .Select(p => p.Attribute("d")!.Value);
            geometry = StreamGeometry.Parse(string.Join(" ", paths));
            Cache.Add(name, geometry);
        }
        return new SvgPath
        {
            Data = geometry, Width = 17, Height = 17,
            Stroke = EditorTheme.TextMain, StrokeThickness = 1.8,
            StrokeLineCap = PenLineCap.Round,
            StrokeJoin = PenLineJoin.Round,
            Stretch = Stretch.Uniform,
            Fill = Brushes.Transparent,
            IsHitTestVisible = false
        };
    }
}