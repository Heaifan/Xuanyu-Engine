using System.Numerics;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private readonly Dictionary<GeoTileId, RenderMesh?> _contourMeshes = [];
    private GeoContourElevationTask? _contourBuild;
    private GeoContourStyle _activeContourStyle;
    private int _activeContourStep;
    private long _contourSourceLoads = -1;

    public bool ContoursEnabled { get; private set; } = true;
    public GeoContourStyle ContourStyle =>
        GeoContourStyle.AtDistance(Camera.DistanceMeters);

    public void SetContoursEnabled(bool enabled) => ContoursEnabled = enabled;

    private RenderDraw[] CreateContourDraws(Matrix4x4 projection)
    {
        if (!ContoursEnabled) return [];
        GeoContourStyle style = ContourStyle;
        int step = ContourPrecision(style);
        long loads = _source.Residency.LoadCount;
        if (style != _activeContourStyle || step != _activeContourStep ||
            loads != _contourSourceLoads)
        {
            _contourMeshes.Clear();
            _contourBuild = null;
            _activeContourStyle = style;
            _activeContourStep = step;
            _contourSourceLoads = loads;
        }
        HashSet<GeoTileId> visible = _visibleMeshes.Keys.ToHashSet();
        foreach (GeoTileId id in _contourMeshes.Keys.ToArray())
            if (!visible.Contains(id)) _contourMeshes.Remove(id);
        if (_contourBuild is not null && !visible.Contains(_contourBuild.TileId))
            _contourBuild = null;
        AdvanceContourBuild(style, step);
        var draws = new List<RenderDraw>();
        foreach (GeoTileId id in _visibleMeshes.Keys)
            if (_contourMeshes.TryGetValue(id, out RenderMesh? line) && line is not null)
                draws.Add(new RenderDraw(line, projection, true, false));
        return draws.ToArray();
    }
}