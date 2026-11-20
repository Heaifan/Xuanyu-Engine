using System.Numerics;
using XuanYu.Core.Geodesy;

namespace XuanYu.Editor.Camera;

public sealed partial class GlobalCamera
{
    private const double TerrainClearanceMeters = 3d;
    private Func<GeoCoordinate, double?>? _groundHeight;
    private Func<GeoCoordinate, double?>? _focusHeight;
    private Func<EcefPosition, EcefPosition, double, double>? _sweepTerrain;

    public void SetTerrainCollision(Func<GeoCoordinate, double?> groundSampler,
        Func<GeoCoordinate, double?>? focusSampler = null,
        Func<EcefPosition, EcefPosition, double, double>? sweepTerrain = null)
    {
        _groundHeight = groundSampler ?? throw new ArgumentNullException(nameof(groundSampler));
        _focusHeight = focusSampler ?? groundSampler;
        _sweepTerrain = sweepTerrain;
    }

    private bool IsAboveTerrain()
    {
        if (_groundHeight is null) return true;
        GeoCoordinate eye = Wgs84.ToGeo(PositionEcef);
        double? ground = _groundHeight(eye);
        return ground is null || eye.HeightMeters >= ground.Value + TerrainClearanceMeters;
    }

    private void ConstrainZoom(double previous) => ConstrainZoomPath(previous);

    public bool RestoreTerrainClearance()
    {
        if (IsAboveTerrain()) return false;
        double before = DistanceMeters;
        for (int i = 0; i < 32 && !IsAboveTerrain(); i++)
            DistanceMeters = Math.Min(MaxDistanceMeters,
                Math.Max(DistanceMeters + 1d, DistanceMeters * 1.4d));
        return DistanceMeters > before;
    }

    // 恢复正北朝上、正顶视，并按当前真实地图范围设置相机距离。
    public void FrameTop(GeoCoordinate center, double distance)
    {
        YawRadians = 0d;
        PitchRadians = Math.PI / 2d - 0.01d;
        DistanceMeters = Math.Clamp(distance, MinDistanceMeters, MaxDistanceMeters);
        Target = GroundedFocus(center);
        RestoreTerrainClearance();
    }

    public void SnapAxis(Vector3 axis)
    {
        if (axis.Z < -0.5f) return; // 正式地形不允许地下仰视。
        double yaw = axis.Z > 0.5f ? 0d :
            axis.X > 0.5f ? Math.PI / 2d :
            axis.X < -0.5f ? -Math.PI / 2d :
            axis.Y > 0.5f ? Math.PI : 0d;
        YawRadians = yaw;
        PitchRadians = axis.Z > 0.5f ? Math.PI / 2d - 0.01d : 0.01d;
        RestoreTerrainClearance();
    }
}
