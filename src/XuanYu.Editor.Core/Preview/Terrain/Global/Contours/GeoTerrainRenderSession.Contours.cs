using System.Numerics;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private readonly Dictionary<GeoTileId,
        (RenderMesh Surface, RenderMesh? Lines, GeoContourStyle Style)> _contourMeshes = [];
    private GeoContourElevationTask? _contourBuild;
    private RenderMesh? _contourBuildingSurface;
    private GeoContourStyle _activeContourStyle;

    public bool ContoursEnabled { get; private set; } = true;
    public GeoContourStyle ContourStyle =>
        GeoContourStyle.AtDistance(Camera.DistanceMeters);

    public void SetContoursEnabled(bool enabled) => ContoursEnabled = enabled;

    private RenderDraw[] CreateContourDraws(Matrix4x4 projection, RenderDraw[] terrain)
    {
        ReportG06Frame();
        if (!ContoursEnabled) return [];
        GeoContourStyle style = ContourStyle;
        var visibleLods = new Dictionary<RenderMesh, int>();
        foreach (var (id, visibleMesh) in _visibleMeshes)
            visibleLods.TryAdd(visibleMesh, id.Level);
        bool needsRibbon = false;
        for (int i = 0; i < terrain.Length; i++)
        {
            if (terrain[i].Mesh.TerrainElevations.IsEmpty) needsRibbon = true;
            else terrain[i] = terrain[i] with
            {
                ContourIntervalMeters = style.IntervalMeters,
                ContourNextIntervalMeters = style.NextIntervalMeters,
                ContourIntervalBlend = style.IntervalBlend,
                ContourOpacity = visibleLods.TryGetValue(terrain[i].Mesh, out int level)
                    ? style.OpacityAtLod(level) : style.Opacity
            };
        }
        if (!needsRibbon)
        {
            if (_g06DiagnosticEnabled && _monitorFrame % 120 == 0)
                Notice?.Invoke($"[G06/线条] 可见地表={terrain.Length} 等高距={style.IntervalMeters}m 像素线宽=1/1.5");
            return [];
        }
        if (style != _activeContourStyle)
        {
            _activeContourStyle = style;
            _contourBuild = null;
            _contourBuildingSurface = null;
        }
        foreach (GeoTileId id in _contourMeshes.Keys.ToArray())
            if (!_visibleMeshes.TryGetValue(id, out RenderMesh? visible) ||
                !ReferenceEquals(visible, _contourMeshes[id].Surface))
                _contourMeshes.Remove(id);
        if (_contourBuild is not null &&
            (!_visibleMeshes.TryGetValue(_contourBuild.TileId, out RenderMesh? mesh) ||
             !ReferenceEquals(mesh, _contourBuildingSurface)))
        {
            _contourBuild = null;
            _contourBuildingSurface = null;
        }
        AdvanceContourBuild(style);
        var draws = new List<RenderDraw>();
        foreach (GeoTileId id in _visibleMeshes.Keys)
            if (_contourMeshes.TryGetValue(id, out var entry) && entry.Lines is { } line)
                draws.Add(new RenderDraw(line, projection, true, false));
        if (_g06DiagnosticEnabled && _monitorFrame % 120 == 0)
        {
            int missing = _visibleMeshes.Keys.Count(id => !_contourMeshes.ContainsKey(id));
            int stale = _visibleMeshes.Keys.Count(id =>
                _contourMeshes.TryGetValue(id, out var entry) && entry.Style != style);
            Notice?.Invoke($"[G06/缓存] 可见={_visibleMeshes.Count} 可绘={draws.Count} " +
                $"待建={missing} 待换样={stale} 构建中={(_contourBuild is null ? 0 : 1)}");
        }
        return draws.ToArray();
    }
}
