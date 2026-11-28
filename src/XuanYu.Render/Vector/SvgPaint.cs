using System.Globalization;
using System.Numerics;
using System.Xml.Linq;

namespace XuanYu.Render;

internal readonly record struct SvgPaint(Vector3? Fill, Vector3? Stroke, float StrokeWidth, bool EvenOdd, float FillOpacity)
{
    public static SvgPaint Default => new(Vector3.Zero, null, 1, false, 1);
    private const string StyleNames = "id fill stroke stroke-width fill-rule transform fill-opacity";

    public SvgPaint Apply(XElement e) => new(
        e.Attribute("fill") is { } f ? Color(f.Value) : Fill,
        e.Attribute("stroke") is { } s ? Color(s.Value) : Stroke,
        e.Attribute("stroke-width") is { } w ? SvgNumbers.NonNegative(w.Value) : StrokeWidth,
        e.Attribute("fill-rule") is { } r ? r.Value switch
        { "evenodd" => true, "nonzero" => false, _ => throw new InvalidDataException("无效 fill-rule。") } : EvenOdd, e.Attribute("fill-opacity") is { } alpha ? Math.Clamp(SvgNumbers.Value(alpha.Value),0,1) : FillOpacity);

    public static void CheckAttributes(XElement e, string geometry)
    {
        var allowed = (StyleNames + " " + geometry).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var a in e.Attributes())
            if (!a.IsNamespaceDeclaration && (a.Name.Namespace != XNamespace.None ||
                !allowed.Contains(a.Name.LocalName)))
                throw new InvalidDataException($"暂不支持 SVG 属性：{e.Name.LocalName}/{a.Name}。");
    }

    private static Vector3? Color(string text)
    {
        text = text.Trim().ToLowerInvariant();
        if (text == "none") return null;
        if (text.Length == 4 && text[0] == '#')
            text = "#" + string.Concat(text[1..].Select(c => new string(c, 2)));
        if (text.Length == 7 && text[0] == '#' && int.TryParse(text.AsSpan(1),
            NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int rgb))
            return new Vector3((rgb >> 16) & 255, (rgb >> 8) & 255, rgb & 255) / 255f;
        return text switch
        {
            "black" => Vector3.Zero, "white" => Vector3.One, "red" => Vector3.UnitX,
            "blue" => Vector3.UnitZ, "green" => new Vector3(0, 128/255f, 0),
            "yellow" => new Vector3(1,1,0), "gray" or "grey" => new Vector3(128/255f),
            _ => throw new InvalidDataException($"暂不支持 SVG 颜色：{text}。")
        };
    }
}