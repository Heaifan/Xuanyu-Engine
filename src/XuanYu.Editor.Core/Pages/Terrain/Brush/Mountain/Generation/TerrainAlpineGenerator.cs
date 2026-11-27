using XuanYu.World.Maps;

namespace XuanYu.Editor.TerrainEditing;

public static class TerrainAlpineGenerator
{
    public static float[] CreateRaw(int samplesX, int samplesY, int seed)
    {
        if (samplesX < 2 || samplesY < 2) throw new ArgumentOutOfRangeException(nameof(samplesX));
        var noise = new TerrainRotatedFractalNoise(seed);
        var raw = new float[checked(samplesX * samplesY)];
        for (int y = 0; y < samplesY; y++)
        for (int x = 0; x < samplesX; x++)
            raw[y * samplesX + x] = Sample(noise, x / (double)samplesX, y / (double)samplesY);
        return raw;
    }

    private static float Sample(TerrainRotatedFractalNoise noise, double nx, double nz)
    {
        // TerraForge STAGE 03：该分支覆盖前两阶段的高程，而不是继续累加。
        double cx = (nx - 0.5d) * 2d, cz = (nz - 0.5d) * 2d;
        double warpX = noise.Fbm(nx * 2.6d + 3.1d, nz * 2.6d + 8.4d, 3);
        double warpZ = noise.Fbm(nx * 2.6d - 5.2d, nz * 2.6d + 1.9d, 3);
        double wx = nx * 3.5d + warpX * 0.55d, wz = nz * 3.5d + warpZ * 0.55d;
        double ridge = noise.Ridged(wx, wz);
        double detail = noise.Fbm(wx * 2.2d, wz * 2.2d, 4) * 0.5d + 0.5d;
        double spine = 0.25d * Math.Sin(cx * 2.4d + warpX * 1.2d);
        double dist = Math.Abs((cx + cz) * 0.65d - spine);
        double mask = Math.Exp(-dist * dist * 3.4d);
        double rx = cx * 0.88d, rz = cz * 0.88d;
        double edge = Math.Max(0d, 1d - Math.Pow(Math.Sqrt(rx * rx + rz * rz), 2.6d));
        return (float)(2.5d + edge * (mask * 54d * ridge + 14d * detail * ridge));
    }

    internal static int Apply(TerrainHeightField field, float height, int seed)
    {
        var raw = CreateRaw(field.SamplesX, field.SamplesY, seed);
        // 仅此处做米制适配：移除演示基底 2.5，并令最大相对隆起等于指定高度。
        double scale = height / ((double)raw.Max() - 2.5d);
        int changed = 0;
        for (int y = 0; y < field.SamplesY; y++)
        for (int x = 0; x < field.SamplesX; x++)
        {
            float current = field.GetHeight(x, y);
            float next = current + (float)((raw[y * field.SamplesX + x] - 2.5d) * scale);
            if (next == current) continue;
            field.SetHeight(x, y, next);
            changed++;
        }
        return changed;
    }
}