using System.Numerics;

namespace XuanYu.Editor.Preview.Terrain;

internal readonly record struct GeoTerrainEdgeVertex(Vector3 Position, Vector3 Color, float Elevation)
{
    private static readonly Vector3 Sun = new(-0.5f, 0.5f, 0.70710678f);
    public Vector3 ShadedColor { get; init; } = Color;

    public static GeoTerrainEdgeVertex FromHeight(Vector3 position, double elevation,
        Vector3? normal = null)
    {
        Vector3 color = TerrainColorPalette.Elevation(elevation);
        float light = 0.75f + 0.25f * MathF.Max(0f, Vector3.Dot(normal ?? Vector3.UnitZ, Sun));
        return new(position, color, (float)elevation) { ShadedColor = color * light };
    }

    public static GeoTerrainEdgeVertex Lerp(GeoTerrainEdgeVertex a,
        GeoTerrainEdgeVertex b, float t) => new(
            Vector3.Lerp(a.Position, b.Position, t), Vector3.Lerp(a.Color, b.Color, t),
            float.Lerp(a.Elevation, b.Elevation, t))
        { ShadedColor = Vector3.Lerp(a.ShadedColor, b.ShadedColor, t) };

    public void AppendTo(List<float> output)
    {
        output.Add(Position.X); output.Add(Position.Y); output.Add(Position.Z);
        output.Add(Color.X); output.Add(Color.Y); output.Add(Color.Z); output.Add(Elevation);
        output.Add(ShadedColor.X); output.Add(ShadedColor.Y); output.Add(ShadedColor.Z);
    }
}