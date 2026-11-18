using System.Numerics;

namespace XuanYu.Editor.Preview.Terrain;

internal readonly record struct GeoTerrainEdgeVertex(Vector3 Position, Vector3 Color)
{
    public static GeoTerrainEdgeVertex FromHeight(Vector3 position, double elevation)
    {
        float t = Math.Clamp((float)((elevation + 100d) / 3000d), 0f, 1f);
        return new(position, Vector3.Lerp(new(0.21f, 0.35f, 0.24f),
            new(0.66f, 0.64f, 0.58f), t));
    }

    public static GeoTerrainEdgeVertex Lerp(GeoTerrainEdgeVertex a,
        GeoTerrainEdgeVertex b, float t) => new(
            Vector3.Lerp(a.Position, b.Position, t), Vector3.Lerp(a.Color, b.Color, t));
}