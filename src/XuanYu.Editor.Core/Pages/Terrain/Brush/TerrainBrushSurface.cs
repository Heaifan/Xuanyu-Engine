using System.Numerics;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.Editor.TerrainEditing;

public static class TerrainBrushSurface
{
    public static float? Sample(TerrainHeightField field, Vector2 point)
    {
        if (point.X < 0f || point.Y < 0f ||
            point.X > field.WidthMeters || point.Y > field.LengthMeters)
            return null;

        int x = Math.Min((int)(point.X / field.SpacingXMeters), field.SamplesX - 2);
        int y = Math.Min((int)(point.Y / field.SpacingYMeters), field.SamplesY - 2);
        float top = MathF.Max(
            MathF.Max(field.GetHeight(x, y), field.GetHeight(x + 1, y)),
            MathF.Max(field.GetHeight(x, y + 1), field.GetHeight(x + 1, y + 1)));
        // 从当前格元最高点上方向下拾取，沿用已有三角形对角线合同。
        return TerrainHeightRayPicker.TryPick(field,
            new Vector3(point, top + 1f), -Vector3.UnitZ, out var hit)
            ? hit.WorldPosition.Z : null;
    }
}