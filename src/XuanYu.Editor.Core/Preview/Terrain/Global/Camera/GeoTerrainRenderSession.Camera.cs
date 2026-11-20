using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Camera;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private static readonly bool G06ShortNear =
        Environment.GetEnvironmentVariable("XYE_G06_NEAR_TEST") == "1";

    public void ResetCamera()
    {
        Camera = new GlobalCamera(_anchor.Origin, InitialDistanceMeters);
        Camera.SetTerrainCollision(SampleGroundHeight, SampleFocusHeight, SweepZoom);
    }

    public void Navigate(float dx, float dy, float wheel, bool pan,
        uint viewportHeight)
    {
        if (wheel != 0f)
            Camera.Zoom(wheel);
        else if (pan)
        {
            double meters = 2d * Camera.DistanceMeters * Math.Tan(Math.PI / 6d) /
                Math.Max(1u, viewportHeight);
            double yaw = Camera.YawRadians;
            double east = (-dx * Math.Cos(yaw) - dy * Math.Sin(yaw)) * meters;
            double north = (-dx * Math.Sin(yaw) + dy * Math.Cos(yaw)) * meters;
            var origin = new EnuFrame(Camera.Target);
            GeoCoordinate geo = Wgs84.ToGeo(origin.ToEcef(
                new EnuPosition(east, north, 0d)));
            Camera.FocusForCursorZoom(new GeoCoordinate(geo.LatitudeDegrees,
                geo.LongitudeDegrees, 0d));
        }
        else
            Camera.Orbit(-dx * 0.006d, dy * 0.006d);
    }

    public float NearPlaneMeters
    {
        get
        {
            GeoCoordinate eye = Wgs84.ToGeo(Camera.PositionEcef);
            // 远景只取分级高程，禁止诊断时触发每帧同步解码 HGT。
            var sampler = StreamingEnabled && Camera.DistanceMeters > 20_000d
                ? _source.CoarseFor(9) : _source;
            double ground = sampler.Sample(eye.LatitudeDegrees,
                eye.LongitudeDegrees) ?? 0d;
            double clearance = Math.Max(0d, eye.HeightMeters - ground);
            double near = Math.Min(Camera.DistanceMeters * 0.01d,
                clearance * 0.25d);
            float baseline = (float)Math.Max(0.5d, near);
            return G06ShortNear ? Math.Max(0.05f, baseline * 0.1f) : baseline;
        }
    }

    private Matrix4x4 ViewProjection(uint width, uint height)
    {
        EnuPosition eye = _anchor.ToEnu(Camera.PositionEcef);
        EnuPosition target = _anchor.ToEnu(Camera.TargetEcef);
        Matrix4x4 view = Matrix4x4.CreateLookAt(ToFloat(eye),
            ToFloat(target), Vector3.UnitZ);
        float near = NearPlaneMeters;
        float far = (float)Math.Max(500_000d, Camera.DistanceMeters * 8d);
        Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfView(
            MathF.PI / 3f, (float)width / height, near, far);
        projection.M22 = -projection.M22;
        return view * projection;
    }

    private static Vector3 ToFloat(EnuPosition p) =>
        new((float)p.East, (float)p.North, (float)p.Up);
}
