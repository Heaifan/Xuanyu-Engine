using System.Diagnostics;
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
    private readonly bool _r02PerfEnabled = Environment.GetEnvironmentVariable("XYE_G06_PERF") == "1";

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
        bool profile = _r02PerfEnabled && _monitorFrame % 120 == 0;
        long start = profile ? Stopwatch.GetTimestamp() : 0;
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
        long cover = profile ? Stopwatch.GetTimestamp() : 0;
        UpdateNativeDetails();
        UpdateRawGeometry();
        long native = profile ? Stopwatch.GetTimestamp() : 0;
        PrefetchCameraSources();
        if (Camera.RestoreTerrainClearance() && !_firstSafetyLogged)
        {
            _firstSafetyLogged = true;
            Notice?.Invoke("G-04-02R：最终可见网格已用于相机离地校正");
        }
        PendingCount = targets.Count(id => !_visibleMeshes.ContainsKey(id));
        Matrix4x4 projection = ViewProjection(width, height);
        long camera = profile ? Stopwatch.GetTimestamp() : 0;
        RenderDraw[] surface = PrepareSurface(projection);
        long prepared = profile ? Stopwatch.GetTimestamp() : 0;
        RenderDraw[] contours = CreateContourDraws(projection, surface);
        RenderDraw[] seams = CreateSeamDebugDraws(projection);
        RenderDraw[] draws = [..surface, ..contours, ..seams];
        DrawCount = surface.Length;
        TrimMeshCaches();
        if (!_firstSurfaceLogged && DrawCount > 0)
        {
            _firstSurfaceLogged = true;
            Notice?.Invoke("G-04-03B-R3：完整覆盖与局部父片兜底已启用");
            Notice?.Invoke($"G-04：首帧绘制 {DrawCount} 片，选片 {SelectedCount}，待细化 {PendingCount} 片");
        }
        CaptureMonitorHistory();
        if (profile) Notice?.Invoke($"[G06/R08-03B/帧] 帧={_monitorFrame} 覆盖={Stopwatch.GetElapsedTime(start, cover).TotalMilliseconds:F1}ms Native及Raw={Stopwatch.GetElapsedTime(cover, native).TotalMilliseconds:F1}ms " +
            $"相机={Stopwatch.GetElapsedTime(native, camera).TotalMilliseconds:F1}ms 地表准备={Stopwatch.GetElapsedTime(camera, prepared).TotalMilliseconds:F1}ms 尾段={Stopwatch.GetElapsedTime(prepared).TotalMilliseconds:F1}ms 总计={Stopwatch.GetElapsedTime(start).TotalMilliseconds:F1}ms");
        return draws;
    }

    private RenderDraw[] PrepareSurface(Matrix4x4 projection)
    {
        long start = _r02PerfEnabled ? Stopwatch.GetTimestamp() : 0;
        RenderDraw[] surface = CreateSelectionDraws(projection);
        double selectionMs = _r02PerfEnabled ? Stopwatch.GetElapsedTime(start).TotalMilliseconds : 0;
        surface = BindResidentDem(surface);
        start = _r02PerfEnabled ? Stopwatch.GetTimestamp() : 0;
        BindDemUvs(surface);
        double uvMs = _r02PerfEnabled ? Stopwatch.GetElapsedTime(start).TotalMilliseconds : 0;
        if (_r02PerfEnabled && (_r02UvBuilds > 0 || selectionMs + uvMs >= 5d))
            Notice?.Invoke($"[G06/R02] 帧={_monitorFrame} 选区={selectionMs:F1}ms UV={uvMs:F1}ms " +
                $"新建UV={_r02UvBuilds} UV唯一反算={_r02UvReverseCount} Draw={surface.Length}");
        return surface;
    }
}