using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;
using XuanYu.Render;
using XuanYu.Editor.Camera;
using XuanYu.Runtime;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private readonly NasademTileCatalog? _g07Catalog;
    private Egm96GeoidGrid? _g07Geoid;

    public void ConfigureG07Geoid(Egm96GeoidGrid geoid)
    {
        _g07Geoid = geoid;
        _g07ClearanceKey = null;
    }

    private double G07EllipsoidHeight(GeoCoordinate position, double orthometric) =>
        orthometric + (_g07Geoid?.UndulationMeters(position) ?? 0d);
    public static IReadOnlyList<Vector4> G07ElevationPalette => TerrainColorPalette.GpuElevation;
    public RenderWaterAtlas? G07WaterAtlas => _waterAtlas;
    public Vector4? G07Selection => _selectedDem is { } selected
        ? new Vector4((float)selected.WestLongitudeDegrees, (float)selected.SouthLatitudeDegrees,
            (float)selected.EastLongitudeDegrees, (float)selected.NorthLatitudeDegrees) : null;
    public GeoCoordinate G07Origin => _anchor.Origin;
    public EnuPosition G07Eye => _anchor.ToEnu(Camera.PositionEcef);

    private (GlobalCamera Camera, GeoUnifiedLodSession Cpu, GeoUnifiedCoverage Cover,
        EcefPosition Eye, double Distance)? _g07ClearanceKey;
    private float? _g07NearPlane;

    public Matrix4x4 G07Projection(uint width, uint height) =>
        G07RenderFrame(width, height).AnchorProjection;

    public GeoRenderFrame G07RenderFrame(uint width, uint height) =>
        new(_anchor, Camera.PositionEcef, Camera.TargetEcef, (float)width / height,
            _g07NearPlane ?? NearPlaneMeters, (float)Math.Max(500_000d, Camera.DistanceMeters * 8d));

    // 在渲染工作线程启动之前查询上一帧已提交覆盖；不与 Step 并发读写页缓存。
    public bool EnsureG07CameraClearance(GeoUnifiedLodSession cpu)
    {
        var key = (Camera, cpu, cpu.Coverage, Camera.PositionEcef, Camera.DistanceMeters);
        if (_g07ClearanceKey is { } previous && previous.Equals(key)) return false;
        double? Ground(GeoCoordinate position) => G07CollisionHeight(cpu, position);
        (EcefPosition Eye, double Distance)? previousZoom =
            _g07ClearanceKey is { } last && ReferenceEquals(last.Camera, Camera) &&
            ReferenceEquals(last.Cpu, cpu) ? (last.Eye, last.Distance) : null;
        double requested = Camera.DistanceMeters;
        bool corrected = Camera.RestoreTerrainClearance(Ground, previousZoom);
        if (corrected && previousZoom is { } prior && requested < prior.Distance)
            Notice?.Invoke($"[G07/相机净空] 请求={requested:F3}m 上帧={prior.Distance:F3}m 修正={Camera.DistanceMeters:F3}m");
        GeoCoordinate eye = Wgs84.ToGeo(Camera.PositionEcef);
        double clearance = Math.Max(0d, eye.HeightMeters - (Ground(eye) ?? 0d));
        _g07NearPlane = NearPlaneForClearance(clearance);
        _g07ClearanceKey = (Camera, cpu, cpu.Coverage, Camera.PositionEcef, Camera.DistanceMeters);
        return corrected;
    }

    private double? G07CollisionHeight(GeoUnifiedLodSession cpu, GeoCoordinate position)
    {
        if (position.HeightMeters > 100_000d) return 0d;
        double? raw = cpu.SampleImportedHeight(position.LatitudeDegrees, position.LongitudeDegrees);
        if (_g07Geoid is not null) return CorrectedG07CollisionHeight(cpu, position, raw);
        EnuPosition eye = _anchor.ToEnu(Wgs84.ToEcef(position));
        // GPU 将高程加在数据集 ENU Up 上；按实际眼点 XY 垂直查询同一地表。
        float top = (float)Math.Max(eye.Up + 1d, short.MaxValue + 1d);
        if (!cpu.TryPickSurface(new Vector3((float)eye.East, (float)eye.North, top),
            -Vector3.UnitZ, out var hit)) return raw ?? 0d;
        double rendered = position.HeightMeters + hit.Up - eye.Up;
        return raw is { } height ? Math.Max(height, rendered) : rendered;
    }

    public Task<GeoUnifiedLodSession> LoadG07DatasetAsync(string? singleSourcePath, int gpuCapacity)
    {
        var catalog = _g07Catalog ?? (SourceCount == 1 && singleSourcePath is not null
            ? new NasademTileCatalog([singleSourcePath])
            : throw new InvalidOperationException("G07 缺少当前导入数据目录。"));
        return GeoUnifiedLodSession.CreateDatasetAsync(catalog, _anchor, gpuCapacity, geoid: _g07Geoid);
    }
}