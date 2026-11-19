using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Camera;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private readonly GeoTileSelector _selector;
    private readonly NasademElevationSource _source;
    private readonly EnuFrame _anchor;
    private readonly GeoTileBounds _coverage;
    private readonly GeoTileId[] _coverRoots;
    private readonly GeoTerrainFrameCache<GeoTileId, RenderMesh?> _meshes = new();
    private bool _firstSurfaceLogged;
    private bool _firstSafetyLogged;

    public GlobalCamera Camera { get; private set; }
    public int SelectedCount { get; private set; }
    public int DrawCount { get; private set; }
    public int PendingCount { get; private set; }
    public bool BootstrapActive => PendingCount > 0;
    public GeoTileBounds Coverage => _coverage;
    public int SourceCount { get; }
    public bool IsMultiSource => SourceCount > 1;
    public string SourceLabel => _nativeSourceId;
    public event Action<string>? Notice;

    private double InitialDistanceMeters =>
        150_000d * Math.Max(_coverage.NorthLatitudeDegrees - _coverage.SouthLatitudeDegrees,
            _coverage.EastLongitudeDegrees - _coverage.WestLongitudeDegrees);

    public IReadOnlyList<RenderDraw> Draws(uint width, uint height)
    {
        if (width < 2 || height < 2) return [];
        _monitorFrame++;
        if (StreamingEnabled) _source.PumpRawPrefetch();
        var view = new GeoTileView(Camera.PositionEcef, Camera.TargetEcef,
            Math.PI / 3d, (int)width, (int)height);
        IReadOnlyList<GeoTileId> targets = _selector.Select(view, _coverage, InCoverage,
            Camera.DistanceMeters <= 20_000d ? Camera.Target : null);
        _monitorTargets = targets;
        SelectedCount = targets.Count;
        BeginMeshCacheFrame();
        UpdateVisibleCover(targets);
        StitchVisibleTiles();
        UpdateNativeDetails();
        PrefetchCameraSources();
        if (Camera.RestoreTerrainClearance() && !_firstSafetyLogged)
        {
            _firstSafetyLogged = true;
            Notice?.Invoke("G-04-02R：最终可见网格已用于相机离地校正");
        }
        PendingCount = targets.Count(id => !_visibleMeshes.ContainsKey(id));
        Matrix4x4 projection = ViewProjection(width, height);
        RenderDraw[] surface = CreateSelectionDraws(projection);
        RenderDraw[] contours = CreateContourDraws(projection);
        RenderDraw[] draws = [..surface, ..contours];
        DrawCount = surface.Length;
        TrimMeshCaches();
        if (!_firstSurfaceLogged && DrawCount > 0)
        {
            _firstSurfaceLogged = true;
            Notice?.Invoke("G-04-03B-R3：完整覆盖与局部父片兜底已启用");
            Notice?.Invoke($"G-04：首帧绘制 {DrawCount} 片，选片 {SelectedCount}，待细化 {PendingCount} 片");
        }
        CaptureMonitorHistory();
        return draws;
    }
}