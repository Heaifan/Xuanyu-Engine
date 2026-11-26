namespace XuanYu.World.Maps;

public sealed partial class GeoUnifiedCoverage
{
    private readonly HashSet<GeoUnifiedTile> _leaves;
    public IReadOnlyList<GeoUnifiedTile> Leaves { get; }

    public GeoUnifiedCoverage(GeoUnifiedTile root) : this([root]) { }

    private GeoUnifiedCoverage(HashSet<GeoUnifiedTile> leaves)
    {
        _leaves = leaves;
        Leaves = Array.AsReadOnly(leaves.ToArray());
    }

    public bool TryRefine(GeoUnifiedTile parent, Func<GeoUnifiedTile, bool> ready,
        out GeoUnifiedCoverage result)
    {
        result = this;
        if (parent.Level == 0 || !_leaves.Contains(parent)) return false;
        var children = Children(parent);
        var next = new HashSet<GeoUnifiedTile>(_leaves);
        next.Remove(parent);
        next.UnionWith(children);
        if (next.Any(tile => !ready(tile)) || !Balanced(next)) return false;
        result = new GeoUnifiedCoverage(next);
        return true;
    }

    public bool TryCoarsen(GeoUnifiedTile parent, Func<GeoUnifiedTile, bool> ready,
        out GeoUnifiedCoverage result)
    {
        result = this;
        if (parent.Level == 0) return false;
        var children = Children(parent);
        if (children.Any(child => !_leaves.Contains(child))) return false;
        var next = new HashSet<GeoUnifiedTile>(_leaves);
        next.ExceptWith(children);
        next.Add(parent);
        if (next.Any(tile => !ready(tile)) || !Balanced(next)) return false;
        result = new GeoUnifiedCoverage(next);
        return true;
    }

    private static GeoUnifiedTile[] Children(GeoUnifiedTile parent) =>
        [parent.Child(0, 0), parent.Child(1, 0), parent.Child(0, 1), parent.Child(1, 1)];

    private static bool Balanced(HashSet<GeoUnifiedTile> leaves)
    {
        var tiles = leaves.ToArray();
        for (int i = 0; i < tiles.Length; i++)
            for (int j = i + 1; j < tiles.Length; j++)
                if (Math.Abs(tiles[i].Level - tiles[j].Level) > 1 && Adjacent(tiles[i], tiles[j]))
                    return false;
        return true;
    }

    private static bool Adjacent(GeoUnifiedTile a, GeoUnifiedTile b)
    {
        bool xOverlap = a.West < b.West + b.Span && b.West < a.West + a.Span;
        bool yOverlap = a.South < b.South + b.Span && b.South < a.South + a.Span;
        return (a.West + a.Span == b.West || b.West + b.Span == a.West) && yOverlap ||
            (a.South + a.Span == b.South || b.South + b.Span == a.South) && xOverlap;
    }
}
