using System.Numerics;
using XuanYu.Render;

namespace XuanYu.Editor.Preview.Terrain;

internal static class GeoContourLabelSpan
{
    public static bool Fits(RenderMesh mesh, Matrix4x4 projection, Vector2 center,
        float radians, float halfWidth, uint width, uint height)
    {
        Vector2 offset = new Vector2(MathF.Cos(radians), MathF.Sin(radians)) * halfWidth;
        return GeoTerrainCursorPicker.TryPick(projection, center - offset, width, height,
                   [mesh], out _) &&
               GeoTerrainCursorPicker.TryPick(projection, center + offset, width, height,
                   [mesh], out _);
    }
}
