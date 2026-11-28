using System.Numerics;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class TerrainPreviewSession
{
    internal float? SampleSurface(Vector2 scaledPoint)
    {
        TerrainHeightField field = Map.HeightField;
        float u = (scaledPoint.X / DisplayScale + OriginMeters.X)
            / field.SpacingXMeters;
        float v = (scaledPoint.Y / DisplayScale + OriginMeters.Y)
            / field.SpacingYMeters;
        if (u < 0f || v < 0f ||
            u > field.SamplesX - 1 || v > field.SamplesY - 1)
            return null;

        int x = Math.Min((int)u, field.SamplesX - 2);
        int y = Math.Min((int)v, field.SamplesY - 2);
        float dx = u - x, dy = v - y;
        float a = field.GetHeight(x, y);
        float b = field.GetHeight(x + 1, y);
        float c = field.GetHeight(x, y + 1);
        float d = field.GetHeight(x + 1, y + 1);
        float height = dx >= dy
            ? a + (b - a) * dx + (d - b) * dy
            : a + (c - a) * dy + (d - c) * dx;
        return height * DisplayScale;
    }
}