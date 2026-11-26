using System.Numerics;
using XuanYu.Core.Geodesy;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe partial class VkTerrainCoordinateProbe
{
    public Vector3[] Run(int south, int west, int step, GeoCoordinate origin,
        ReadOnlySpan<short> heights)
    {
        WriteInput(south, west, step, origin, heights);
        Dispatch();
        var result = new Vector3[1089];
        void* mapped = null;
        Check(_vk.MapMemory(_device, _outputMemory, 0, 1089 * 16, 0, &mapped), "映射坐标诊断结果");
        try
        {
            var values = (Vector4*)mapped;
            for (int i = 0; i < result.Length; i++) result[i] = new Vector3(values[i].X, values[i].Y, values[i].Z);
        }
        finally { _vk.UnmapMemory(_device, _outputMemory); }
        return result;
    }

    private void WriteInput(int south, int west, int step, GeoCoordinate origin,
        ReadOnlySpan<short> heights)
    {
        var anchor = Wgs84.ToEcef(origin);
        double lat = south / 3600d - 90d, lon = west / 3600d - 180d;
        double r = Math.PI / (180d * 3600d);
        double* values = stackalloc double[12] { Math.Sin(lat * Math.PI / 180), Math.Cos(lat * Math.PI / 180),
            Math.Sin(lon * Math.PI / 180), Math.Cos(lon * Math.PI / 180),
            Math.Sin(origin.LatitudeDegrees * Math.PI / 180), Math.Cos(origin.LatitudeDegrees * Math.PI / 180),
            Math.Sin(origin.LongitudeDegrees * Math.PI / 180), Math.Cos(origin.LongitudeDegrees * Math.PI / 180),
            anchor.X, anchor.Y, anchor.Z, step * r };
        void* mapped = null;
        Check(_vk.MapMemory(_device, _inputMemory, 0, 96 + 1089 * 4, 0, &mapped), "映射坐标诊断输入");
        try
        {
            System.Buffer.MemoryCopy(values, mapped, 96, 96);
            int* target = (int*)((byte*)mapped + 96);
            for (int i = 0; i < heights.Length; i++) target[i] = heights[i];
        }
        finally { _vk.UnmapMemory(_device, _inputMemory); }
    }
}
