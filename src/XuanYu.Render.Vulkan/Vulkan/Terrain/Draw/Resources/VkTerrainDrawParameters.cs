using System.Numerics;
using System.Runtime.InteropServices;
using XuanYu.Core.Geodesy;

namespace XuanYu.Render.Vulkan;

internal static class VkTerrainDrawParameters
{
    public static byte[] Create(int south, int west, int step, GeoCoordinate origin, Matrix4x4 mvp)
    {
        var bytes = new byte[160];
        MemoryMarshal.Write(bytes.AsSpan(), in mvp);
        var anchor = Wgs84.ToEcef(origin);
        double rad = Math.PI / 180d;
        double lat = (south / 3600d - 90d) * rad;
        double lon = (west / 3600d - 180d) * rad;
        double aLat = origin.LatitudeDegrees * rad, aLon = origin.LongitudeDegrees * rad;
        double[] values = [Math.Sin(lat), Math.Cos(lat), Math.Sin(lon), Math.Cos(lon),
            Math.Sin(aLat), Math.Cos(aLat), Math.Sin(aLon), Math.Cos(aLon),
            anchor.X, anchor.Y, anchor.Z, step * rad / 3600d];
        MemoryMarshal.AsBytes(values.AsSpan()).CopyTo(bytes.AsSpan(64));
        return bytes;
    }
}