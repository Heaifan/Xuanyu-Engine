using System.Numerics;

namespace XuanYu.Editor.Camera;

public sealed partial class ViewportCamera
{
    private (double X, double Y, double Z) Backward => (
        Math.Cos(_pitch) * Math.Cos(_yaw), Math.Cos(_pitch) * Math.Sin(_yaw), Math.Sin(_pitch));
    private (double X, double Y, double Z) Right => (-Math.Sin(_yaw), Math.Cos(_yaw), 0d);
    private (double X, double Y, double Z) Up => (
        -Math.Sin(_pitch) * Math.Cos(_yaw), -Math.Sin(_pitch) * Math.Sin(_yaw), Math.Cos(_pitch));

    private static Vector3 ToVector((double X, double Y, double Z) p) =>
        new((float)p.X, (float)p.Y, (float)p.Z);

    private static double Dot((double X, double Y, double Z) a,
        (double X, double Y, double Z) b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    public Vector3 ViewRight => ToVector(Right);
    public Vector3 ViewUp => ToVector(Up);

    public Matrix4x4 ViewProjection(uint width, uint height)
    {
        var eye = PositionMeters;
        var r = Right;
        var u = Up;
        var b = Backward;
        // 先以 double 完成平移投影，避免由两个大 float 坐标相减求相机方向。
        Matrix4x4 view = new(
            (float)r.X, (float)u.X, (float)b.X, 0f,
            (float)r.Y, (float)u.Y, (float)b.Y, 0f,
            (float)r.Z, (float)u.Z, (float)b.Z, 0f,
            (float)(-Dot(eye, r)), (float)(-Dot(eye, u)), (float)(-Dot(eye, b)), 1f);
        var (near, far) = ClipDepthRange();
        float h = (float)(2d * _distance * Math.Tan(FieldOfView / 2d));
        Matrix4x4 projection = IsOrthographic
            ? Matrix4x4.CreateOrthographic(h * width / height, h, near, far)
            : Matrix4x4.CreatePerspectiveFieldOfView((float)FieldOfView, (float)width / height, near, far);
        projection.M22 = -projection.M22;
        return view * projection;
    }

    internal bool TryCreateScreenRay(Vector2 pixel, uint width, uint height,
        out Vector3 origin, out Vector3 direction)
    {
        origin = direction = default;
        if (width == 0 || height == 0 || !float.IsFinite(pixel.X) || !float.IsFinite(pixel.Y) ||
            pixel.X < 0 || pixel.Y < 0 || pixel.X >= width || pixel.Y >= height)
            return false;

        var eye = PositionMeters;
        var b = Backward;
        var r = Right;
        var u = Up;
        double tan = Math.Tan(FieldOfView / 2d);
        double x = (pixel.X * 2d / width - 1d) * tan * width / height;
        double y = (1d - pixel.Y * 2d / height) * tan;
        var offset = (X: r.X * x + u.X * y, Y: r.Y * x + u.Y * y, Z: r.Z * x + u.Z * y);
        var (near, _) = ClipDepthRange();
        double scale = IsOrthographic ? _distance : near;
        // 原点位于近裁剪面；正交射线平行，透视射线从相机发散。
        origin = ToVector((eye.X - b.X * near + offset.X * scale,
            eye.Y - b.Y * near + offset.Y * scale, eye.Z - b.Z * near + offset.Z * scale));
        var ray = IsOrthographic ? (-b.X, -b.Y, -b.Z) :
            (offset.X - b.X, offset.Y - b.Y, offset.Z - b.Z);
        double length = Math.Sqrt(Dot(ray, ray));
        direction = ToVector((ray.Item1 / length, ray.Item2 / length, ray.Item3 / length));
        return true;
    }
}