
using System.Xml.Linq;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using SvgPath = Avalonia.Controls.Shapes.Path;

namespace XuanYu.Editor.UI.Left;

internal static class LeftSvgIcon
{
    private const string Root = "avares://XuanYu.Editor.UI/Assets/Left/";

    private static readonly Dictionary<string, Geometry> Cache = [];

    private static readonly IBrush Standard =
        new SolidColorBrush(Color.Parse("#58738D"));

    private static readonly IBrush Folder =
        new SolidColorBrush(Color.Parse("#BA8738"));

    public static SvgPath Create(string key, int size = 16)
    {
        bool chevron = key.StartsWith("Controls/chevron-", StringComparison.Ordinal);
        double scale = chevron ? 0.74 : 0.86;
        return new SvgPath
        {
            Data = GeometryFor(key),
            Width = size,
            Height = size,
            Stretch = Stretch.Uniform,
            Stroke = key.Contains("folder", StringComparison.Ordinal)
                ? Folder : Standard,
            StrokeThickness = 1.7,
            StrokeLineCap = PenLineCap.Round,
            Fill = Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            // 保持布局占位，缩小图形本身，避免 Left 树节点的缩进发生变化。
            RenderTransform = new ScaleTransform(scale, scale),
            RenderTransformOrigin = RelativePoint.Center,
            IsHitTestVisible = false
        };
    }

    public static string ForFileName(string name) =>
        System.IO.Path.GetExtension(name).ToLowerInvariant() switch
        {
            ".cs" or ".csproj" or ".axaml" or ".xml" => "Types/code",
            ".json" or ".yaml" or ".yml" or ".dem" or ".tif" => "Types/data",
            ".xyscene" or ".xyproject" => "Nodes/scene",
            ".png" or ".jpg" or ".jpeg" or ".webp" or ".svg" => "Types/image",
            ".glb" or ".gltf" or ".obj" or ".fbx" or ".stl" => "Types/model",
            _ => "Nodes/file"
        };

    private static Geometry GeometryFor(string key)
    {
        if (Cache.TryGetValue(key, out Geometry? cached))
            return cached;

        using var stream = AssetLoader.Open(new Uri(Root + key + ".svg"));
        var document = XDocument.Load(stream);
        XNamespace ns = "http://www.w3.org/2000/svg";

        string data = string.Join(" ", document.Descendants(ns + "path")
            .Select(element => element.Attribute("d")!.Value));

        Geometry geometry = StreamGeometry.Parse(data);
        Cache.Add(key, geometry);
        return geometry;
    }
}
