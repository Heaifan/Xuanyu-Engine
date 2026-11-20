using System.Numerics;

namespace XuanYu.Editor.Preview.Terrain;

public static class GeoContourSurfacePath
{
    public static Vector3[] Densify(IReadOnlyList<Vector3> path,
        bool closed, float spacingMeters)
    {
        var result = new List<Vector3>();
        int edges = closed ? path.Count : path.Count - 1;
        for (int i = 0; i < edges; i++)
        {
            Vector3 a = path[i], b = path[(i + 1) % path.Count];
            float dx = b.X - a.X, dy = b.Y - a.Y;
            int divisions = Math.Max(1,
                (int)MathF.Ceiling(MathF.Sqrt(dx * dx + dy * dy) / spacingMeters));
            for (int j = 0; j < divisions; j++)
                result.Add(Vector3.Lerp(a, b, j / (float)divisions));
        }
        if (!closed) result.Add(path[^1]);
        return result.ToArray();
    }
}