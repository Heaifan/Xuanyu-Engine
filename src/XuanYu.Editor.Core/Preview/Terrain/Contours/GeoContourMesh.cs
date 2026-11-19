using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Render;

namespace XuanYu.Editor.Preview.Terrain;

public static partial class GeoContourMesh
{
    internal readonly record struct Point(Vector3 Position, Vector3 Color, float Elevation);

    public static RenderMesh? Build(RenderMesh terrain, EnuFrame anchor,
        GeoContourStyle style)
    {
        var task = new GeoContourBuildTask(terrain, anchor, style);
        task.Advance(int.MaxValue);
        return task.Finish();
    }

    internal static Point Read(ReadOnlySpan<float> data, int index,
        EnuFrame anchor, Dictionary<Vector3, float> elevations)
    {
        Vector3 position = new(data[index], data[index + 1], data[index + 2]);
        if (!elevations.TryGetValue(position, out float elevation))
        {
            GeoCoordinate geo = Wgs84.ToGeo(anchor.ToEcef(new EnuPosition(
                position.X, position.Y, position.Z)));
            // 原采样是米级高程，消除 ENU float 逆变换带来的亚米抖动。
            elevation = MathF.Round((float)geo.HeightMeters);
            elevations.Add(position, elevation);
        }
        return new Point(position,
            new Vector3(data[index + 3], data[index + 4], data[index + 5]),
            elevation);
    }
}