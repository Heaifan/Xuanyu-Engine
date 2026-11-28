using System.Numerics;
using System.Runtime.InteropServices;
using XuanYu.Core.Geodesy;
using XuanYu.Render;

namespace XuanYu.Render.Vulkan;

internal static class VkTerrainDrawParameters
{
    public const int ByteSize = 608;

    public static byte[] Create(int south, int west, int step, GeoCoordinate origin, Matrix4x4 mvp,
        IReadOnlyList<Vector4>? clipRects = null, TerrainVisualStyle? style = null,
        RenderWaterAtlas? water = null, EnuPosition renderOrigin = default)
    {
        var bytes = new byte[ByteSize];
        MemoryMarshal.Write(bytes.AsSpan(), in mvp);
        var anchor = Wgs84.ToEcef(origin);
        double rad = Math.PI / 180d;
        double lat = (south / 3600d - 90d) * rad;
        double lon = (west / 3600d - 180d) * rad;
        double aLat = origin.LatitudeDegrees * rad, aLon = origin.LongitudeDegrees * rad;
        double[] values = [Math.Sin(lat), Math.Cos(lat), Math.Sin(lon), Math.Cos(lon),
            Math.Sin(aLat), Math.Cos(aLat), Math.Sin(aLon), Math.Cos(aLon),
            anchor.X, anchor.Y, anchor.Z, step * rad / 3600d];
        MemoryMarshal.AsBytes(values.AsSpan()).CopyTo(bytes.AsSpan(64));
        Span<Vector4> rectangles = stackalloc Vector4[4];
        rectangles.Fill(new Vector4(1, 1, -1, -1));
        if (clipRects is null) rectangles[0] = new Vector4(0, 0, 32, 32);
        else
        {
            if (clipRects.Count > 4) throw new ArgumentException("G07 页最多四个裁剪矩形。", nameof(clipRects));
            for (int i = 0; i < clipRects.Count; i++) rectangles[i] = clipRects[i];
        }
        MemoryMarshal.AsBytes(rectangles).CopyTo(bytes.AsSpan(160));
        var visual = new Vector4(water is null ? -1f : (float)((west - (water.WestLongitude + 180) * 3600) / 3600d),
            water is null ? -1f : (float)((south - (water.SouthLatitude + 90) * 3600) / 3600d),
            step / 3600f, style is null ? -1f : style.Hillshade ? 1f : 0f);
        MemoryMarshal.Write(bytes.AsSpan(224), in visual);
        if (style is not null)
        {
            for (int i = 0; i < 7; i++)
            {
                var color = style.Palette[i];
                MemoryMarshal.Write(bytes.AsSpan(240 + i * 16), in color);
            }
        }
        WriteOverlays(bytes, south, west, step, style);
        Span<double> eye = stackalloc double[4] { renderOrigin.East, renderOrigin.North, renderOrigin.Up, 0d };
        MemoryMarshal.AsBytes(eye).CopyTo(bytes.AsSpan(576));
        return bytes;
    }
    private static void WriteOverlays(byte[] bytes, int south, int west, int step,
        TerrainVisualStyle? style)
    {
        if (style is null) return;
        var contour = style.Contours;
        MemoryMarshal.Write(bytes.AsSpan(352), in contour);
        if (style.Grid is { } grid)
        {
            var config = new Vector4((float)grid.StepX, (float)grid.StepY,
                0f, grid.Geographic ? 1f : 2f);
            var phase = grid.Geographic
                ? new Vector4((float)((west / 3600d - 180d) / grid.StepX - grid.CenterX),
                    (float)((south / 3600d - 90d) / grid.StepY - grid.CenterY),
                    (float)(step / 3600d / grid.StepX), (float)(step / 3600d / grid.StepY))
                : new Vector4(grid.CenterX, grid.CenterY, 0, 0);
            MemoryMarshal.Write(bytes.AsSpan(368), in config);
            MemoryMarshal.Write(bytes.AsSpan(384), in phase);
            var next = new Vector4((float)(grid.CenterX * grid.NextRatio % 5d),
                (float)(grid.CenterY * grid.NextRatio % 5d), (float)grid.NextRatio, grid.Blend);
            MemoryMarshal.Write(bytes.AsSpan(560), in next);
        }
        if (style.Selection is { } bounds)
        {
            var rect = new Vector4((float)(((bounds.X + 180d) * 3600d - west) / step),
                (float)(((bounds.Y + 90d) * 3600d - south) / step),
                (float)(((bounds.Z + 180d) * 3600d - west) / step),
                (float)(((bounds.W + 90d) * 3600d - south) / step));
            MemoryMarshal.Write(bytes.AsSpan(400), in rect);
        }
        int count = Math.Min(8, style.LabelGaps.Count);
        var flags = new Vector4(style.Selection.HasValue ? 1 : 0, count,
            style.Grid is { } g ? g.CenterX % 5 : 0,
            style.Grid is { } h ? h.CenterY % 5 : 0);
        MemoryMarshal.Write(bytes.AsSpan(416), in flags);
        for (int i = 0; i < count; i++)
        {
            var gap = style.LabelGaps[i];
            MemoryMarshal.Write(bytes.AsSpan(432 + i * 16), in gap);
        }
    }
}