using System.Numerics;

namespace XuanYu.Editor.Preview;

public sealed partial class GeoGizmoLayout
{
    private static GeoGizmoLayout CreateOrientation(Vector2 center, Vector2[] axes, GeoGizmoMode mode)
    {
        var handles = new List<Handle>(3);
        for (int i = 0; i < 3; i++)
        {
            var id = (GeoGizmoHandle)(1 << i);
            if (mode == GeoGizmoMode.Scale)
            {
                if (axes[i].Length() >= 16) handles.Add(new(id, [center, center + axes[i]]));
                continue;
            }
            var a = axes[(i + 1) % 3]; var b = axes[(i + 2) % 3];
            // 近乎侧视的环不能可靠反解角度。
            if (MathF.Abs(Cross(a, b)) < 650) continue;
            var points = new Vector2[65];
            for (int j = 0; j < 64; j++)
            {
                float angle = j * MathF.Tau / 64;
                points[j] = center + a * MathF.Cos(angle) + b * MathF.Sin(angle);
            }
            points[64] = points[0];
            handles.Add(new(id, points));
        }
        return new(center, handles, mode);
    }

    private GeoGizmoHandle PickOrientation(Vector2 point)
    {
        if (Mode == GeoGizmoMode.Scale && Vector2.DistanceSquared(point, Center) <= 81)
            return GeoGizmoHandle.All;
        GeoGizmoHandle best = GeoGizmoHandle.None;
        float distance = 49;
        foreach (var h in Handles)
            for (int i = 1; i < h.Points.Length; i++)
            {
                var start = h.Points[i - 1]; var axis = h.Points[i] - start;
                float t = Math.Clamp(Vector2.Dot(point - start, axis) / axis.LengthSquared(), 0, 1);
                float next = Vector2.DistanceSquared(point, start + t * axis);
                if (next < distance) { distance = next; best = h.Id; }
            }
        return best;
    }
}