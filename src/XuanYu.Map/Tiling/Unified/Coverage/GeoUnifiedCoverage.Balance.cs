namespace XuanYu.World.Maps;

public sealed partial class GeoUnifiedCoverage
{
    internal GeoUnifiedCoverage PrepareBalancedRefinement(GeoUnifiedTile target)
    {
        var split = new HashSet<GeoUnifiedTile> { target };
        var pending = new Queue<GeoUnifiedTile>();
        pending.Enqueue(target);
        while (pending.TryDequeue(out var tile))
            foreach (var neighbor in _leaves)
                if (neighbor.Level > tile.Level && Adjacent(tile, neighbor) && split.Add(neighbor))
                    pending.Enqueue(neighbor);
        var next = new HashSet<GeoUnifiedTile>(_leaves);
        foreach (var tile in split)
        {
            next.Remove(tile);
            next.UnionWith(Children(tile));
        }
        return new GeoUnifiedCoverage(next);
    }
}