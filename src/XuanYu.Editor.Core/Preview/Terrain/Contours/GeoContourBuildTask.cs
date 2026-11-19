using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Render;

namespace XuanYu.Editor.Preview.Terrain;

public sealed class GeoContourBuildTask
{
    private readonly EnuFrame _anchor;
    private readonly GeoContourStyle _style;
    private readonly Dictionary<Vector3, float> _elevations = [];
    private readonly List<float> _output = [];
    private int _next;

    public RenderMesh Terrain { get; }

    public GeoContourBuildTask(RenderMesh terrain, EnuFrame anchor, GeoContourStyle style)
    {
        Terrain = terrain;
        _anchor = anchor;
        _style = style;
    }

    public bool Advance(int triangles)
    {
        ReadOnlySpan<float> vertices = Terrain.Vertices.Span;
        long end = Math.Min((long)vertices.Length, _next + 21L * triangles);
        while (_next < end)
        {
            var a = GeoContourMesh.Read(vertices, _next, _anchor, _elevations);
            var b = GeoContourMesh.Read(vertices, _next + 7, _anchor, _elevations);
            var c = GeoContourMesh.Read(vertices, _next + 14, _anchor, _elevations);
            GeoContourMesh.TraceTriangle(_output, a, b, c, _style);
            _next += 21;
        }
        return _next == vertices.Length;
    }

    public RenderMesh? Finish() => _output.Count == 0 ? null :
        new RenderMesh($"{Terrain.AssetId}/contour/{_style.IntervalMeters}/{_style.WidthMeters}",
            _output.ToArray());
}