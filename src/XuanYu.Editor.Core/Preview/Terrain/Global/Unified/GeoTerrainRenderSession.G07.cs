using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;
using XuanYu.Render;
using XuanYu.Editor.Camera;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private readonly NasademTileCatalog? _g07Catalog;
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
        ViewProjection(width, height, _g07NearPlane);

    // 在渲染工作线程启动之前查询上一帧已提交覆盖；不与 Step 并发读写页缓存。
    public bool EnsureG07CameraClearance(GeoUnifiedLodSession cpu)
    {
        var key = (Camera, cpu, cpu.Coverage, Camera.PositionEcef, Camera.DistanceMeters);
        if (_g07ClearanceKey is { } previous && previous.Equals(key)) return false;
        double? Ground(GeoCoordinate position) => G07CollisionHeight(cpu, position);
        bool corrected = Camera.RestoreTerrainClearance(Ground);
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
        return GeoUnifiedLodSession.CreateDatasetAsync(catalog, _anchor, gpuCapacity);
    }
}