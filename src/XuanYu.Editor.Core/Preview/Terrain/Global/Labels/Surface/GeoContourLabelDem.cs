using System.Numerics;
using XuanYu.Render;

namespace XuanYu.Editor.Preview.Terrain;

internal static class GeoContourLabelDem
{
    public static bool TryTangent(RenderDemTile dem, Vector2 uv, out Vector2 tangent)
    {
        tangent = default;
        const int cells = RenderDemTile.Side - 1;
        Vector2 grid = uv * cells;
        if (grid.X < 0 || grid.Y < 0 || grid.X > cells || grid.Y > cells) return false;
        int x = Math.Min((int)grid.X, cells - 1), y = Math.Min((int)grid.Y, cells - 1);
        ReadOnlySpan<short> data = dem.SamplesNorthToSouth.Span;
        if (data.Length < RenderDemTile.Side * RenderDemTile.Side) return false;
        int i = y * RenderDemTile.Side + x;
        float a = data[i], b = data[i + 1], c = data[i + RenderDemTile.Side];
        float d = data[i + RenderDemTile.Side + 1];
        if (a == RenderDemTile.NoData || b == RenderDemTile.NoData ||
            c == RenderDemTile.NoData || d == RenderDemTile.NoData) return false;
        float tx = grid.X - x, ty = grid.Y - y;
        float gx = (b - a) * (1 - ty) + (d - c) * ty;
        float gy = (c - a) * (1 - tx) + (d - b) * tx;
        tangent = new Vector2(gy, -gx);
        return tangent.LengthSquared() > 1e-10f;
    }
}
