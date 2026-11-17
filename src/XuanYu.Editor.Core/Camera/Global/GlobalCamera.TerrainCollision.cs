using System.Numerics;
using XuanYu.Core.Geodesy;

namespace XuanYu.Editor.Camera;

public sealed partial class GlobalCamera
{
    private const double TerrainClearanceMeters = 3d;
    private Func<GeoCoordinate, double?>? _groundHeight;

    public void SetTerrainCollision(Func<GeoCoordinate, double?> sampler) =>
        _groundHeight = sampler ?? throw new ArgumentNullException(nameof(sampler));

    private bool IsAboveTerrain()
    {
        if (_groundHeight is null) return true;
        GeoCoordinate eye = Wgs84.ToGeo(PositionEcef);
        double? ground = _groundHeight(eye);
        return ground is null || eye.HeightMeters >= ground.Value + TerrainClearanceMeters;
    }

    private void ConstrainZoom(double previous)
    {
        if (DistanceMeters >= previous || IsAboveTerrain()) return;
        double blocked = DistanceMeters;
        double allowed = previous;
        for (int i = 0; i < 18; i++)
        {
            DistanceMeters = (blocked + allowed) * 0.5d;
            if (IsAboveTerrain()) allowed = DistanceMeters;
            else blocked = DistanceMeters;
        }
        DistanceMeters = allowed;
    }

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
        Target = center;
        YawRadians = 0d;
        PitchRadians = Math.PI / 2d - 0.01d;
        DistanceMeters = Math.Clamp(distance, MinDistanceMeters, MaxDistanceMeters);
        RestoreTerrainClearance();
    }

    public void FocusForCursorZoom(GeoCoordinate target)
    {
        Target = target;
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