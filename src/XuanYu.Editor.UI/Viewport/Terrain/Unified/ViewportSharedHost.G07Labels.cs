using System.Numerics;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportSharedHost
{
    private readonly G07ContourLabelCache _g07LabelCache = new();

    private GeoContourLabel[] G07Labels(GeoTerrainRenderSession source, GeoUnifiedLodSession cpu,
        Matrix4x4 mvp, uint width, uint height, int interval, float scale) =>
        _g07LabelCache.Update(source, cpu, mvp, width, height, interval, scale, Environment.TickCount64);

    private static Vector4 LabelGap(GeoContourLabel label, float scale)
    {
        float radians = label.AngleDegrees * MathF.PI / 180f;
        uint packed = (uint)BitConverter.HalfToUInt16Bits((Half)radians) |
            ((uint)BitConverter.HalfToUInt16Bits((Half)(34f * scale)) << 16);
        return new(label.X, label.Y, BitConverter.UInt32BitsToSingle(packed), label.ElevationMeters);
    }
}