using XuanYu.Core.Geodesy;

namespace XuanYu.Editor.Camera;

public sealed partial class GlobalCamera
{
    public GlobalCamera Copy() => (GlobalCamera)MemberwiseClone();

    public void MoveGround(double forward, double right)
    {
        if (forward == 0d && right == 0d) return;
        var previous = Target;
        var eye = PositionEcef;
        double yaw = YawRadians;
        var frame = new EnuFrame(previous);
        var moved = Wgs84.ToGeo(frame.ToEcef(new EnuPosition(
            right * Math.Cos(yaw) - forward * Math.Sin(yaw),
            right * Math.Sin(yaw) + forward * Math.Cos(yaw), 0d)));
        Target = GroundedFocus(new GeoCoordinate(moved.LatitudeDegrees,
            moved.LongitudeDegrees, previous.HeightMeters));
        if (!IsAboveTerrain() || (_sweepTerrain is not null &&
            _sweepTerrain(eye, PositionEcef, TerrainClearanceMeters) < 1d))
            Target = previous;
    }
}