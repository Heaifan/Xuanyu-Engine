using System.Numerics;
using System.Xml.Linq;

namespace XuanYu.Render;

internal sealed record SvgContour(Vector2[] Points, bool Closed);

internal static class SvgShapes
{
    public static SvgContour[] Read(XElement e)
    {
        string name = e.Name.LocalName;
        float N(string key, float fallback = 0) => e.Attribute(key) is { } a ? SvgNumbers.Value(a.Value) : fallback;
        float S(string key) => SvgNumbers.NonNegative((string?)e.Attribute(key) ?? "0");
        switch (name)
        {
            case "rect":
                SvgPaint.CheckAttributes(e, "x y width height rx ry");
                float x = N("x"), y = N("y"), w = S("width"), h = S("height");
                if (w == 0 || h == 0) return [];
                float rx = e.Attribute("rx") is null ? S("ry") : S("rx");
                float ry = e.Attribute("ry") is null ? rx : S("ry");
                return [RoundedRect(x, y, w, h, Math.Min(rx,w/2), Math.Min(ry,h/2))];
            case "circle":
                SvgPaint.CheckAttributes(e, "cx cy r");
                return Ellipse(N("cx"), N("cy"), S("r"), S("r"));
            case "ellipse":
                SvgPaint.CheckAttributes(e, "cx cy rx ry");
                return Ellipse(N("cx"), N("cy"), S("rx"), S("ry"));
            case "line":
                SvgPaint.CheckAttributes(e, "x1 y1 x2 y2");
                return [new([new(N("x1"),N("y1")),new(N("x2"),N("y2"))],false)];
            case "polygon": case "polyline":
                SvgPaint.CheckAttributes(e, "points");
                var values = SvgNumbers.Parse((string?)e.Attribute("points") ?? "");
                if (values.Length % 2 != 0) throw new InvalidDataException("无效 SVG points。");
                return [new(Enumerable.Range(0, values.Length/2)
                    .Select(i => new Vector2(values[i*2],values[i*2+1])).ToArray(),name == "polygon")];
            case "path":
                SvgPaint.CheckAttributes(e, "d");
                return SvgPathReader.Read((string?)e.Attribute("d") ?? "");
            default: throw new InvalidDataException($"暂不支持 SVG 图元：{name}。");
        }
    }

    private static SvgContour RoundedRect(float x, float y, float w, float h, float rx, float ry)
    {
        if (rx == 0 || ry == 0) return new([new(x,y),new(x+w,y),new(x+w,y+h),new(x,y+h)],true);
        List<Vector2> points = [];
        Vector2[] centers = [new(x+w-rx,y+ry), new(x+w-rx,y+h-ry), new(x+rx,y+h-ry), new(x+rx,y+ry)];
        for (int corner=0; corner<4; corner++)
            for (int i=0; i<=16; i++)
            {
                float angle = (corner-1+i/16f)*MathF.PI/2;
                points.Add(centers[corner]+new Vector2(rx*MathF.Cos(angle),ry*MathF.Sin(angle)));
            }
        return new(points.ToArray(),true);
    }

    private static SvgContour[] Ellipse(float x, float y, float rx, float ry)
    {
        if (rx == 0 || ry == 0) return [];
        return [new(Enumerable.Range(0, 64).Select(i =>
        {
            float angle = i * MathF.Tau / 64;
            return new Vector2(x + rx*MathF.Cos(angle), y + ry*MathF.Sin(angle));
        }).ToArray(),true)];
    }
}