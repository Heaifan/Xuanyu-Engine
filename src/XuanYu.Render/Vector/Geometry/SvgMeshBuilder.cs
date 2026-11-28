using System.Numerics;

namespace XuanYu.Render;

internal sealed partial class SvgMeshBuilder(Vector2 center, float extent)
{
    private readonly List<float> _vertices = [];
    public Matrix3x2 Transform { get; set; } = Matrix3x2.Identity;
    public float Opacity { get; set; } = 1;

    public RenderMesh Build(string id) => _vertices.Count > 0
        ? new RenderMesh(id, _vertices.ToArray())
        : throw new InvalidDataException("SVG 没有可绘制的图元。");

    private void Triangle(Vector2 a, Vector2 b, Vector2 c, Vector3 color)
    {
        Add(a); Add(b); Add(c);
        void Add(Vector2 p)
        {
            var v = (Vector2.Transform(p, Transform)-center)/extent;
            _vertices.AddRange([v.X,v.Y,0,color.X,color.Y,color.Z,Opacity]);
        }
    }

    private void Quad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector3 color)
    {
        Triangle(a,b,c,color); Triangle(a,c,d,color);
    }

    public void Stroke(SvgContour contour, Vector3 color, float width)
    {
        var p = contour.Points.DistinctAdjacent().ToArray();
        if (p.Length < 2) return;
        if (contour.Closed && p[0] == p[^1]) p = p[..^1];
        if (p.Length < 2) return;
        int segments = contour.Closed ? p.Length : p.Length-1;
        Vector2[] normals = new Vector2[segments];
        for (int i=0; i<segments; i++)
        {
            var a=p[i]; var b=p[(i+1)%p.Length];
            var delta=Vector2.Normalize(b-a);
            var n = normals[i] = new Vector2(-delta.Y,delta.X)*(width/2);
            Quad(a+n,b+n,b-n,a-n,color);
        }
        for (int i=contour.Closed ? 0 : 1; i<(contour.Closed ? p.Length : p.Length-1); i++)
        {
            var previous=normals[(i+segments-1)%segments]; var next=normals[i%segments];
            Join(p[i],previous,next,color);
            Join(p[i],-previous,-next,color);
        }
    }

    private void Join(Vector2 p, Vector2 a, Vector2 b, Vector3 color)
    {
        float denominator = Vector2.Dot(a+b,b);
        Triangle(p,p+a,p+b,color);
        if (denominator <= 0) return;
        Vector2 m = (a+b)*(b.LengthSquared()/denominator);
        if (m.LengthSquared() <= 16*b.LengthSquared()) Triangle(p+a,p+m,p+b,color);
    }
}

internal static class SvgPoints
{
    public static IEnumerable<Vector2> DistinctAdjacent(this IEnumerable<Vector2> points)
    {
        Vector2? previous = null;
        foreach (var point in points)
        {
            if (point == previous) continue;
            previous=point;
            yield return point;
        }
    }
}