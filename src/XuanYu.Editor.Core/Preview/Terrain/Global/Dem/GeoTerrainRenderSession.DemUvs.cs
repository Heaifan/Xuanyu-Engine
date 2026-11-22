using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Render;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private readonly Dictionary<(RenderMesh Mesh, int South, int West), float[]> _demUvs = [];

    private int _r02UvBuilds;
    private int _r02UvReverseCount;

    private void BindDemUvs(RenderDraw[] draws)
    {
        _r02UvBuilds = 0;
        _r02UvReverseCount = 0;
        if (_waterAtlas is null && Camera.DistanceMeters > 20_000d)
        {
            _demUvs.Clear();
            return;
        }
        for (int i = 0; i < draws.Length; i++)
        {
            RenderDraw draw = draws[i];
            if (draw.Mesh.TerrainElevations.IsEmpty) continue;
            if (_waterAtlas is { } water)
                draws[i] = draw with { WaterAtlas = water,
                    DemUV = GetDemUvs(draw.Mesh, water.SouthLatitude, water.WestLongitude) };
            else if (draw.ResidentDem is { } dem)
                draws[i] = draw with { DemUV = GetDemUvs(draw.Mesh, dem.SouthLatitude, dem.WestLongitude) };
        }
        TrimDemUvs(draws);
    }

    private ReadOnlyMemory<float> GetDemUvs(RenderMesh mesh, int south, int west)
    {
        var key = (mesh, south, west);
        if (_demUvs.TryGetValue(key, out float[]? saved)) return saved;
        ReadOnlySpan<float> vertices = mesh.Vertices.Span;
        ReadOnlySpan<float> elevations = mesh.TerrainElevations.Span;
        ReadOnlySpan<double> geographic = mesh.TerrainGeographicCoordinates.Span;
        var uv = new float[mesh.VertexCount * 2];
        // 仅对完全相同的反算输入去重，不量化坐标，也不按 AssetId 复用。
        var unique = new Dictionary<Vector3, Vector2>();
        for (int i = 0; i < mesh.VertexCount; i++)
        {
            int g = i * 2;
            if (!geographic.IsEmpty && !double.IsNaN(geographic[g]))
            {
                uv[g] = (float)(geographic[g] - west);
                uv[g + 1] = (float)(south + 1d - geographic[g + 1]);
                continue;
            }
            int p = i * 7;
            var point = new Vector3(vertices[p], vertices[p + 1], vertices[p + 2] - elevations[i]);
            if (!unique.TryGetValue(point, out Vector2 value))
            {
                value = SolveDemUv(point, south, west);
                unique.Add(point, value);
            }
            uv[g] = value.X;
            uv[g + 1] = value.Y;
        }
        _demUvs.Add(key, uv);
        _r02UvBuilds++;
        _r02UvReverseCount += unique.Count;
        return uv;
    }

    private Vector2 SolveDemUv(Vector3 point, int south, int west)
    {
        double zeroUp = point.Z;
        GeoCoordinate geo = default;
        for (int j = 0; j < 4; j++)
        {
            geo = Wgs84.ToGeo(_anchor.ToEcef(new EnuPosition(point.X, point.Y, zeroUp)));
            zeroUp -= geo.HeightMeters;
        }
        return new Vector2((float)(geo.LongitudeDegrees - west),
            (float)(south + 1d - geo.LatitudeDegrees));
    }

    private void TrimDemUvs(RenderDraw[] draws)
    {
        var visible = draws.Select(d => d.Mesh).ToHashSet();
        foreach (var key in _demUvs.Keys.ToArray())
            if (!visible.Contains(key.Mesh)) _demUvs.Remove(key);
    }
}