using System.Numerics;

namespace XuanYu.World.Maps;

public static class GeoUnifiedFrustum
{
    public static bool Intersects(GeoUnifiedBounds bounds, Matrix4x4 viewProjection)
    {
        float[] xs = [MathF.BitDecrement((float)bounds.Min.East), MathF.BitIncrement((float)bounds.Max.East)];
        float[] ys = [MathF.BitDecrement((float)bounds.Min.North), MathF.BitIncrement((float)bounds.Max.North)];
        float[] zs = [MathF.BitDecrement((float)bounds.Min.Up), MathF.BitIncrement((float)bounds.Max.Up)];
        int outside = 63;
        foreach (float x in xs)
            foreach (float y in ys)
                foreach (float z in zs)
                {
                    Vector4 p = Vector4.Transform(new Vector4(x, y, z, 1), viewProjection);
                    if (!float.IsFinite(p.X) || !float.IsFinite(p.Y) ||
                        !float.IsFinite(p.Z) || !float.IsFinite(p.W)) return true;
                    float margin = 1e-5f * (MathF.Abs(p.X) + MathF.Abs(p.Y) + MathF.Abs(p.Z) + MathF.Abs(p.W) + 1);
                    int mask = 0;
                    if (p.X < -p.W - margin) mask |= 1;
                    if (p.X > p.W + margin) mask |= 2;
                    if (p.Y < -p.W - margin) mask |= 4;
                    if (p.Y > p.W + margin) mask |= 8;
                    if (p.Z < -margin) mask |= 16;
                    if (p.Z > p.W + margin) mask |= 32;
                    outside &= mask;
                    if (outside == 0) return true;
                }
        return false;
    }
}
