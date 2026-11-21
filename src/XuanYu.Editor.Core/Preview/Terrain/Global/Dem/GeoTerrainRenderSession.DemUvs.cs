using XuanYu.Core.Geodesy;
using XuanYu.Render;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private readonly Dictionary<(RenderMesh Mesh, int South, int West), float[]> _demUvs = [];

    private void BindDemUvs(RenderDraw[] draws)
    {
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
        var uv = new float[mesh.VertexCount * 2];
        for (int i = 0; i < mesh.VertexCount; i++)
        {
            int p = i * 7;
            double zeroUp = vertices[p + 2] - elevations[i];
            GeoCoordinate geo = default;
            for (int j = 0; j < 4; j++)
            {
                geo = Wgs84.ToGeo(_anchor.ToEcef(new EnuPosition(
                    vertices[p], vertices[p + 1], zeroUp)));
                zeroUp -= geo.HeightMeters;
            }
            uv[i * 2] = (float)(geo.LongitudeDegrees - west);
            uv[i * 2 + 1] = (float)(south + 1d - geo.LatitudeDegrees);
        }
        _demUvs.Add(key, uv);
        return uv;
    }

    private void TrimDemUvs(RenderDraw[] draws)
    {
        var visible = draws.Select(d => d.Mesh).ToHashSet();
        foreach (var key in _demUvs.Keys.ToArray())
            if (!visible.Contains(key.Mesh)) _demUvs.Remove(key);
    }
}