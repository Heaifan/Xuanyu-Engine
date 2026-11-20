using System.Numerics;

namespace XuanYu.Editor.Preview.Terrain;

internal readonly record struct GeoTerrainEdgeVertex(Vector3 Position, Vector3 Color, float Elevation)
{
    public static GeoTerrainEdgeVertex FromHeight(Vector3 position, double elevation)
    {
        return new(position, TerrainColorPalette.Elevation(elevation), (float)elevation);
    }

    public static GeoTerrainEdgeVertex Lerp(GeoTerrainEdgeVertex a,
        GeoTerrainEdgeVertex b, float t) => new(
            Vector3.Lerp(a.Position, b.Position, t), Vector3.Lerp(a.Color, b.Color, t),
            float.Lerp(a.Elevation, b.Elevation, t));
}
