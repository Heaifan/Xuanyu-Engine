using System.Numerics;
using XuanYu.World.Maps;

namespace XuanYu.Editor.TerrainEditing;

public static class TerrainFlattenStamp
{
    public static int Apply(TerrainHeightField field, Vector2 center,
        TerrainBrushSettings brush, float target) =>
        Apply(field, center, brush, target, brush.Strength);

    internal static int Apply(TerrainHeightField field, Vector2 center,
        TerrainBrushSettings brush, float target, float strength) => strength == 0f ? 0 :
        TerrainHeightStamp.Visit(field, center, brush, (x, y, weight) =>
        {
            float before = field.GetHeight(x, y);
            float blend = strength * weight;
            return blend == 1f ? target : float.Lerp(before, target, blend);
        });
}