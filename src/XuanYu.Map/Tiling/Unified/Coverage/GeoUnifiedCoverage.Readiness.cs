namespace XuanYu.World.Maps;

public sealed partial class GeoUnifiedCoverage
{
    public bool TryRefineWithEdges(GeoUnifiedTile parent, Func<GeoUnifiedTile, int, bool> ready,
        out GeoUnifiedCoverage result)
    {
        result = this;
        if (!TryRefine(parent, _ => true, out var candidate) ||
            candidate.Leaves.Any(tile => !ready(tile, candidate.EdgeMask(tile)))) return false;
        result = candidate;
        return true;
    }

    public bool TryCoarsenWithEdges(GeoUnifiedTile parent, Func<GeoUnifiedTile, int, bool> ready,
        out GeoUnifiedCoverage result)
    {
        result = this;
        if (!TryCoarsen(parent, _ => true, out var candidate) ||
            candidate.Leaves.Any(tile => !ready(tile, candidate.EdgeMask(tile)))) return false;
        result = candidate;
        return true;
    }
}
