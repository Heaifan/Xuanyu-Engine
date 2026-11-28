using XuanYu.Core.Geodesy;

namespace XuanYu.Editor.Camera;

public sealed partial class GlobalCamera
{
    private GeoCoordinate GroundedFocus(GeoCoordinate target)
    {
        if (DistanceMeters > 20_000d || _focusHeight is null) return target;
        double? height = _focusHeight(target);
        if (height is not { } ground || target.HeightMeters >= ground)
            return target;
        return new GeoCoordinate(target.LatitudeDegrees,
            target.LongitudeDegrees, ground);
    }

    public void FocusForCursorZoom(GeoCoordinate target) => FocusForCursorZoom(target, true);

    internal void FocusForCursorZoom(GeoCoordinate target, bool recoverClearance)
    {
        GeoCoordinate next = GroundedFocus(target);
        EcefPosition previousEye = PositionEcef;
        EcefPosition previousTarget = TargetEcef;
        EcefPosition nextTarget = Wgs84.ToEcef(next);
        // 平移 ECEF 视线向量，不绕新目标重新旋转观察方向。
        var shiftedEye = new EcefPosition(
            nextTarget.X + previousEye.X - previousTarget.X,
            nextTarget.Y + previousEye.Y - previousTarget.Y,
            nextTarget.Z + previousEye.Z - previousTarget.Z);
        EnuPosition local = new EnuFrame(next).ToEnu(shiftedEye);
        double pitch = Math.Atan2(local.Up,
            Math.Sqrt(local.East * local.East + local.North * local.North));
        // 不强行钳位：若迁移会造成地下仰视，取消此次目标重定位。
        if (pitch < 0.01d || pitch > Math.PI / 2d - 0.01d) return;

        GeoCoordinate oldTarget = Target;
        double oldYaw = YawRadians, oldPitch = PitchRadians;
        double oldDistance = DistanceMeters;
        Target = next;
        YawRadians = Math.Atan2(local.East, -local.North);
        PitchRadians = pitch;
        if (recoverClearance && !IsAboveTerrain()) RestoreTerrainClearance();
        if (IsAboveTerrain() && (_sweepTerrain is null ||
            _sweepTerrain(previousEye, PositionEcef, TerrainClearanceMeters) == 1d)) return;
        Target = oldTarget;
        YawRadians = oldYaw;
        PitchRadians = oldPitch;
        DistanceMeters = oldDistance;
    }
}