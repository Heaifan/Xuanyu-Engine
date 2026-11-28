using System.Numerics;

namespace XuanYu.Render;

internal sealed partial class SvgMeshBuilder
{
    private readonly record struct Edge(Vector2 A, Vector2 B)
    {
        public float Min => Math.Min(A.Y,B.Y);
        public float Max => Math.Max(A.Y,B.Y);
        public int Winding => A.Y < B.Y ? 1 : -1;
        public float X(float y) => A.X + (B.X-A.X)*((y-A.Y)/(B.Y-A.Y));
    }

    public void Fill(SvgContour[] contours, Vector3 color, bool evenOdd)
    {
        List<Edge> edges = [];
        foreach (var contour in contours)
        {
            var p = contour.Points;
            if (p.Length < 3) continue;
            for (int i=0; i<p.Length; i++)
                if (p[i].Y != p[(i+1)%p.Length].Y) edges.Add(new(p[i],p[(i+1)%p.Length]));
        }
        // 扫描带在边交点处分段，凹多边形、洞及交叉路径遵守填充规则。
        SortedSet<float> levels = new(edges.SelectMany(e => new[] { e.Min,e.Max }));
        for (int i=0; i<edges.Count; i++)
            for (int j=i+1; j<edges.Count; j++) AddCrossing(edges[i],edges[j],levels);
        var ys = levels.ToArray();
        for (int i=0; i+1<ys.Length; i++)
        {
            float lo=ys[i], hi=ys[i+1], mid=lo+(hi-lo)/2;
            var active=edges.Where(e=>e.Min<mid && mid<e.Max).OrderBy(e=>e.X(mid)).ToArray();
            int winding=0;
            for (int j=0; j+1<active.Length; j++)
            {
                var a=active[j]; var b=active[j+1]; winding+=a.Winding;
                if (evenOdd ? (winding&1)==0 : winding==0) continue;
                Quad(new(a.X(lo),lo),new(b.X(lo),lo),new(b.X(hi),hi),new(a.X(hi),hi),color);
            }
        }
    }

    private static void AddCrossing(Edge a, Edge b, SortedSet<float> levels)
    {
        float lo=Math.Max(a.Min,b.Min), hi=Math.Min(a.Max,b.Max);
        if (lo>=hi) return;
        float left=a.X(lo)-b.X(lo), right=a.X(hi)-b.X(hi);
        if ((left<0 && right>0)||(left>0 && right<0))
            levels.Add(lo+(hi-lo)*(left/(left-right)));
    }
}