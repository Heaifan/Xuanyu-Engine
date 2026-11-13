
using System.Numerics;

namespace XuanYu.Editor.Camera;

public static class ViewportScreenRay
{
    public static bool TryCreate(Matrix4x4 viewProjection,
        Vector2 pixel, uint width, uint height,
        out Vector3 origin, out Vector3 direction)
    {
        origin = default;
        direction = default;

        if (width == 0 || height == 0 ||
            !float.IsFinite(pixel.X) || !float.IsFinite(pixel.Y) ||
            pixel.X < 0 || pixel.Y < 0 ||
            pixel.X >= width || pixel.Y >= height)
            return false;

        if (!Matrix4x4.Invert(viewProjection, out var inverse))
            return false;

        float x = pixel.X * 2f / width - 1f;
        float y = pixel.Y * 2f / height - 1f;

        // Vulkan 正高度 viewport，且相机投影已翻转 Y 轴。
        Vector4 near = Vector4.Transform(
            new Vector4(x, y, 0f, 1f), inverse);
        Vector4 far = Vector4.Transform(
            new Vector4(x, y, 1f, 1f), inverse);

        if (MathF.Abs(near.W) < 1e-7f ||
            MathF.Abs(far.W) < 1e-7f)
            return false;

        Vector3 start = new(near.X / near.W,
            near.Y / near.W, near.Z / near.W);
        Vector3 end = new(far.X / far.W,
            far.Y / far.W, far.Z / far.W);

        Vector3 delta = end - start;
        if (delta.LengthSquared() < 1e-12f)
            return false;

        origin = start;
        direction = Vector3.Normalize(delta);
        return true;
    }
}
