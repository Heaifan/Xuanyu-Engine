using System.Numerics;

namespace XuanYu.Editor.Preview;

[Flags]
public enum GeoGizmoHandle { None = 0, E = 1, N = 2, U = 4, EN = 3, EU = 5, NU = 6, All = 7 }
public enum GeoGizmoMode { Select, Translate, Rotate, Scale }

public sealed partial class GeoGizmoLayout
{
    public sealed record Handle(GeoGizmoHandle Id, Vector2[] Points);
    public Vector2 Center { get; }
    public IReadOnlyList<Handle> Handles { get; }
    public GeoGizmoMode Mode { get; }
    private GeoGizmoLayout(Vector2 center, List<Handle> handles, GeoGizmoMode mode)
    { Center = center; Handles = handles; Mode = mode; }

    public static GeoGizmoLayout? Create(Matrix4x4 matrix, uint width, uint height, GeoGizmoMode mode = GeoGizmoMode.Translate)
    {
        if (mode == GeoGizmoMode.Select) return null;
        var clip = Vector4.Transform(new Vector4(0, 0, 0, 1), matrix);
        if (clip.W <= 0 || clip.Z < 0 || clip.Z > clip.W || width == 0 || height == 0) return null;
        var center = Screen(clip, width, height);
        if (center.X < 0 || center.Y < 0 || center.X >= width || center.Y >= height) return null;
        Vector2 Axis(int i)
        {
            var row = i == 0 ? new Vector4(matrix.M11, matrix.M12, matrix.M13, matrix.M14) :
                i == 1 ? new(matrix.M21, matrix.M22, matrix.M23, matrix.M24) :
                new(matrix.M31, matrix.M32, matrix.M33, matrix.M34);
            return new((row.X - clip.X / clip.W * row.W) / clip.W * width / 2,
                (row.Y - clip.Y / clip.W * row.W) / clip.W * height / 2);
        }
        Vector2[] axes = [Axis(0), Axis(1), Axis(2)];
        float largest = axes.Max(x => x.Length());
        if (!float.IsFinite(largest) || largest <= 0) return null;
        var handles = new List<Handle>(6);
        for (int i = 0; i < 3; i++) axes[i] *= 80 / largest;
        if (mode != GeoGizmoMode.Translate) return CreateOrientation(center, axes, mode);
        for (int i = 0; i < 3; i++)
            if (axes[i].Length() >= 16) handles.Add(new((GeoGizmoHandle)(1 << i), [center, center + axes[i]]));
        for (int i = 0; i < 3; i++)
            for (int j = i + 1; j < 3; j++)
            {
                var a = axes[i]; var b = axes[j];
                if (MathF.Abs(Cross(a, b)) * .23f * .23f < 65) continue;
                handles.Add(new((GeoGizmoHandle)((1 << i) | (1 << j)),
                    [center + .22f*(a+b), center + .45f*a + .22f*b,
                     center + .45f*(a+b), center + .22f*a + .45f*b]));
            }
        return new(center, handles, mode);
    }

    public GeoGizmoHandle Pick(Vector2 point)
    {
        if (Mode != GeoGizmoMode.Translate) return PickOrientation(point);
        if (Vector2.DistanceSquared(point, Center) <= 49 && Handles.Any(h => h.Id == GeoGizmoHandle.EN))
            return GeoGizmoHandle.EN;
        GeoGizmoHandle best = GeoGizmoHandle.None;
        float distance = 36;
        foreach (var h in Handles.Where(h => h.Points.Length == 2))
        {
            var axis = h.Points[1] - Center;
            float t = Math.Clamp(Vector2.Dot(point - Center, axis) / axis.LengthSquared(), 0, 1);
            float next = Vector2.DistanceSquared(point, Center + t * axis);
            if (next < distance) { distance = next; best = h.Id; }
        }
        if (best != GeoGizmoHandle.None) return best;
        foreach (var h in Handles.Where(h => h.Points.Length == 4))
        {
            float first = Cross(h.Points[1] - h.Points[0], point - h.Points[0]);
            if (Enumerable.Range(1, 3).All(i => first * Cross(h.Points[(i+1)%4] - h.Points[i], point - h.Points[i]) > 0))
                return h.Id;
        }
        return GeoGizmoHandle.None;
    }

    public static Vector2 Screen(Vector4 clip, uint width, uint height) =>
        new((clip.X / clip.W + 1) * width / 2, (clip.Y / clip.W + 1) * height / 2);
    private static float Cross(Vector2 a, Vector2 b) => a.X*b.Y - a.Y*b.X;
}