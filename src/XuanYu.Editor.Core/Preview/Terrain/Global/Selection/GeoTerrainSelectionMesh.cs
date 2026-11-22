using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

internal static partial class GeoTerrainSelectionMesh
{
    public static (RenderMesh? Inner, RenderMesh? Outer) Split(
        RenderMesh mesh, EnuFrame anchor, GeoTileBounds selected)
    {
        float[] data = mesh.Vertices.ToArray();
        ReadOnlySpan<float> heights = mesh.TerrainElevations.Span;
        ReadOnlySpan<float> shaded = mesh.TerrainShadedColors.Span;
        ReadOnlySpan<double> geographic = mesh.TerrainGeographicCoordinates.Span;
        for (int i = 0; i < heights.Length; i++)
            data[i * 7 + 6] = heights[i];
        var inner = new List<float>();
        var outer = new List<float>();
        var innerGeographic = new List<double>();
        var outerGeographic = new List<double>();
        var points = new Dictionary<Vector3, GeoCoordinate>();
        for (int i = 0; i < data.Length; i += 21)
        {
            Vertex a = Read(data, shaded, geographic, i, anchor, points);
            Vertex b = Read(data, shaded, geographic, i + 7, anchor, points);
            Vertex c = Read(data, shaded, geographic, i + 14, anchor, points);
            ClipTriangle(inner, outer, innerGeographic, outerGeographic, a, b, c, selected);
        }
        if (inner.Count == 0) return (null, mesh);
        if (outer.Count == 0) return (mesh, null);
        return (Part("/dem-outline", inner, innerGeographic),
            Part("/dem-rest", outer, outerGeographic));

        RenderMesh Part(string suffix, List<float> packed, List<double> coordinates)
        {
            var span = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(packed);
            var geographicSpan = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(coordinates);
            string id = mesh.AssetId + suffix;
            if (!mesh.TerrainShadedColors.IsEmpty)
                return RenderMesh.FromLitTerrain(id, span, geographicSpan);
            return mesh.TerrainElevations.IsEmpty ? new RenderMesh(id, span) :
                RenderMesh.FromOpaqueTerrain(id, span, geographicSpan);
        }
    }

    private static Vertex Read(float[] data, ReadOnlySpan<float> shaded,
        ReadOnlySpan<double> geographic, int i, EnuFrame anchor,
        Dictionary<Vector3, GeoCoordinate> points)
    {
        Vector3 p = new(data[i], data[i + 1], data[i + 2]);
        int coordinate = i / 7 * 2;
        GeoCoordinate geo;
        if (!geographic.IsEmpty && !double.IsNaN(geographic[coordinate]))
            geo = new(geographic[coordinate + 1], geographic[coordinate], 0d);
        else if (!points.TryGetValue(p, out geo))
        {
            double z = p.Z;
            // 地形以椭球零高程的 ENU XY 加高程构建；反求零高程经纬度避免山顶偏移。
            for (int step = 0; step < 4; step++)
            {
                geo = Wgs84.ToGeo(anchor.ToEcef(new EnuPosition(p.X, p.Y, z)));
                z -= geo.HeightMeters;
            }
            points.Add(p, geo);
        }
        int s = i / 7 * 3;
        Vector3? shade = shaded.IsEmpty ? null : new Vector3(shaded[s], shaded[s + 1], shaded[s + 2]);
        return new(p, new(data[i + 3], data[i + 4], data[i + 5]), data[i + 6],
            geo.LatitudeDegrees, geo.LongitudeDegrees, shade);
    }

    private static void Write(List<float> output, Vertex v)
    {
        output.Add(v.Position.X); output.Add(v.Position.Y); output.Add(v.Position.Z);
        output.Add(v.Color.X); output.Add(v.Color.Y); output.Add(v.Color.Z); output.Add(v.Alpha);
        if (v.ShadedColor is { } shade)
        { output.Add(shade.X); output.Add(shade.Y); output.Add(shade.Z); }
    }

    private readonly record struct Vertex(Vector3 Position, Vector3 Color,
        float Alpha, double Latitude, double Longitude, Vector3? ShadedColor)
    {
        public static Vertex Lerp(Vertex a, Vertex b, double t) => new(
            Vector3.Lerp(a.Position, b.Position, (float)t),
            Vector3.Lerp(a.Color, b.Color, (float)t),
            a.Alpha + (b.Alpha - a.Alpha) * (float)t,
            a.Latitude + (b.Latitude - a.Latitude) * t,
            a.Longitude + (b.Longitude - a.Longitude) * t,
            a.ShadedColor is { } left && b.ShadedColor is { } right
                ? Vector3.Lerp(left, right, (float)t) : null);
    }
}