using System.Numerics;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private static readonly bool G06GeometryDebug =
        Environment.GetEnvironmentVariable("XYE_G06_GEOMETRY_DEBUG") == "1";
    private static readonly bool G06ShowWire =
        Environment.GetEnvironmentVariable("XYE_G06_WIRE") != "0";
    private static readonly bool G06ShowBoundary =
        Environment.GetEnvironmentVariable("XYE_G06_BOUNDARY") == "1";
    private static readonly string? G06Tile =
        Environment.GetEnvironmentVariable("XYE_G06_TILE");
    private RenderMesh? _g06Observed;
    private RenderMesh? _g06Wire;
    private RenderMesh? _g06Boundary;
    private long _g06NextBuildFrame;
    private bool _g06LightNotice;

    private RenderDraw[] CreateSeamDebugDraws(Matrix4x4 projection)
    {
        if (!G06GeometryDebug) return [];
        if (!_g06LightNotice)
        {
            _g06LightNotice = true;
            Notice?.Invoke($"[G06/轻诊断] 单瓦片；线框={G06ShowWire} " +
                $"开边={G06ShowBoundary}；指定瓦片={G06Tile ?? "自动"}");
        }
        KeyValuePair<GeoTileId, RenderMesh> chosen = string.IsNullOrWhiteSpace(G06Tile)
            ? _visibleMeshes.OrderBy(item => DistanceFromTarget(item.Key)).FirstOrDefault()
            : _visibleMeshes.FirstOrDefault(item => item.Key.ToString() == G06Tile);
        RenderMesh? mesh = chosen.Value;
        if (mesh is null) return [];
        if (!ReferenceEquals(_g06Observed, mesh))
        {
            if (_monitorFrame < _g06NextBuildFrame) return [];
            _g06NextBuildFrame = _monitorFrame + 60;
            _g06Observed = mesh;
            _g06Wire = G06ShowWire ? GeoTerrainSeamWire.Build(mesh) : null;
            _g06Boundary = G06ShowBoundary ? GeoTerrainSeamBoundary.Build(mesh) : null;
            Notice?.Invoke(GeoTerrainSeamAudit.Describe(chosen.Key, mesh));
            if (G06ShowBoundary && mesh.TriangleCount > GeoTerrainSeamBoundary.MaxTriangles)
                Notice?.Invoke($"[G06/轻诊断] {chosen.Key} 开边跳过：" +
                    $"网格{mesh.TriangleCount}三角形超过上限" +
                    $"{GeoTerrainSeamBoundary.MaxTriangles}；没有做开边推断");
        }
        var draws = new List<RenderDraw>(2);
        if (_g06Wire is { } wire)
            draws.Add(new RenderDraw(wire, projection, true, false));
        if (_g06Boundary is { } edge)
            draws.Add(new RenderDraw(edge, projection, true, false));
        return draws.ToArray();
    }
}