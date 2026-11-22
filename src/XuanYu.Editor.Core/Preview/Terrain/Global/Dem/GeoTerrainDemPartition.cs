using XuanYu.Core.Geodesy;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public static class GeoTerrainDemPartition
{
    public readonly record struct Part(RenderMesh Mesh, int South, int West);

    public static Part[] Split(RenderMesh mesh, EnuFrame anchor,
        int south, int west, int north, int east)
    {
        var parts = new List<Part>(4);
        RenderMesh? remainder = mesh;
        for (int y = south; y <= north; y++)
            for (int x = west; x <= east; x++)
            {
                if (remainder is null) return parts.ToArray();
                if (y == north && x == east)
                {
                    parts.Add(new(remainder, y, x));
                    return parts.ToArray();
                }
                // 只裁剪尚未归属的三角形，避免同一区域重复绘制。
                var (inside, outside) = GeoTerrainSelectionMesh.Split(remainder, anchor,
                    new GeoTileBounds(y, x, y + 1d, x + 1d));
                if (inside is not null) parts.Add(new(inside, y, x));
                remainder = outside;
            }
        return parts.ToArray();
    }
}