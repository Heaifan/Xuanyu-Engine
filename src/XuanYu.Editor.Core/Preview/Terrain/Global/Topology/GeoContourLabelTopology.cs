namespace XuanYu.Editor.Preview.Terrain;

internal readonly record struct GeoContourGridEdge(
    int Row, int Column, bool Vertical, int Elevation);

internal sealed partial class GeoContourLabelTopology
{
    private readonly Dictionary<GeoContourGridEdge, GeoContourGridEdge> _parents = [];
    private readonly List<(GeoContourGridEdge Edge, GeoContourLabel Label)> _points = [];

    public void Add(GeoContourGridEdge edge, GeoContourLabel? label)
    {
        _parents.Add(edge, edge);
        if (label is { } point) _points.Add((edge, point));
    }

    public GeoContourLabel[] Candidates()
    {
        var ids = new Dictionary<GeoContourGridEdge, int>();
        var result = new GeoContourLabel[_points.Count];
        for (int i = 0; i < _points.Count; i++)
        {
            var (edge, label) = _points[i];
            GeoContourGridEdge root = Root(edge);
            if (!ids.TryGetValue(root, out int id))
            {
                id = ids.Count + 1;
                ids.Add(root, id);
            }
            result[i] = label with { ContourId = id };
        }
        return result;
    }

    private GeoContourGridEdge Root(GeoContourGridEdge edge)
    {
        GeoContourGridEdge parent = _parents[edge];
        if (parent != edge) _parents[edge] = parent = Root(parent);
        return parent;
    }

    private void Join(GeoContourGridEdge a, GeoContourGridEdge b)
    {
        if (_parents.ContainsKey(a) && _parents.ContainsKey(b))
        {
            GeoContourGridEdge rootA = Root(a), rootB = Root(b);
            if (rootA != rootB) _parents[rootB] = rootA;
        }
    }
}