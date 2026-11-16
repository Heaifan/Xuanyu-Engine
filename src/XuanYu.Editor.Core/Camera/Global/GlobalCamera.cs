
using System.Numerics;
using XuanYu.Core.Geodesy;

namespace XuanYu.Editor.Camera;

public sealed class GlobalCamera
{
    public const double MinDistanceMeters = 0.5d;
    public const double MaxDistanceMeters = 50_000_000d;

    public GeoCoordinate Target { get; private set; }
    public double DistanceMeters { get; private set; }
    public double YawRadians { get; private set; }
    public double PitchRadians { get; private set; } = Math.PI / 4d;
    public EcefPosition TargetEcef => Wgs84.ToEcef(Target);

    public GlobalCamera(GeoCoordinate target, double distanceMeters = 15_000_000d)
    {
        if (!double.IsFinite(distanceMeters) ||
            distanceMeters < MinDistanceMeters || distanceMeters > MaxDistanceMeters)
            throw new ArgumentOutOfRangeException(nameof(distanceMeters));
        Target = target;
        DistanceMeters = distanceMeters;
    }

    public EcefPosition PositionEcef
    {
        get
        {
            double horizontal = DistanceMeters * Math.Cos(PitchRadians);
            var offset = new EnuPosition(
                horizontal * Math.Sin(YawRadians),
                -horizontal * Math.Cos(YawRadians),
                DistanceMeters * Math.Sin(PitchRadians));
            return new EnuFrame(Target).ToEcef(offset);
        }
    }

    public void Focus(GeoCoordinate target) => Target = target;

    public void Orbit(double yawDeltaRadians, double pitchDeltaRadians)
    {
        if (!double.IsFinite(yawDeltaRadians) ||
            !double.IsFinite(pitchDeltaRadians))
            throw new ArgumentOutOfRangeException(nameof(yawDeltaRadians));
        YawRadians = (YawRadians + yawDeltaRadians) % (2d * Math.PI);
        PitchRadians = Math.Clamp(PitchRadians + pitchDeltaRadians,
            0.01d, Math.PI / 2d - 0.01d);
    }

    public void Zoom(double wheelSteps)
    {
        if (!double.IsFinite(wheelSteps))
            throw new ArgumentOutOfRangeException(nameof(wheelSteps));
        DistanceMeters = Math.Clamp(
            DistanceMeters * Math.Pow(0.85d, wheelSteps),
            MinDistanceMeters, MaxDistanceMeters);
    }

    public Vector3 ToCameraRelative(EcefPosition position)
    {
        var frame = new EnuFrame(Target);
        EnuPosition point = frame.ToEnu(position);
        EnuPosition eye = frame.ToEnu(PositionEcef);
        // 先以 double 相减，再转换 float，避免巨大 ECEF 坐标被截断。
        return new Vector3((float)(point.East - eye.East),
            (float)(point.North - eye.North),
            (float)(point.Up - eye.Up));
    }
}
