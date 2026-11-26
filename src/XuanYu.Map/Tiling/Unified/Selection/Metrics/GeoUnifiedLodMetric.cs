using XuanYu.Core.Geodesy;

namespace XuanYu.World.Maps;

public readonly record struct GeoUnifiedLodMetric(
    GeoUnifiedTile Tile, int EdgeMask, double ErrorMeters, GeoUnifiedBounds Bounds)
{
    internal (Guid, long) Identity { get; init; }

    public static GeoUnifiedLodMetric? Measure(GeoUnifiedHeightPage page, GeoUnifiedSamples source,
        EnuFrame anchor, int edgeMask)
    {
        using var pin = source.PinResident();
        double? error = GeoUnifiedError.Measure(page, source, anchor, edgeMask);
        if (error is null) return null;
        var bounds = GeoUnifiedBounds.Measure(page.Tile, source, anchor);
        return bounds is { } value ? new GeoUnifiedLodMetric(page.Tile, edgeMask, error.Value, value)
            { Identity = page.Identity } : null;
    }

    public bool Matches(GeoUnifiedHeightPage page, int edgeMask) =>
        page.Tile == Tile && page.Identity == Identity && edgeMask == EdgeMask;

    public double ScreenError(EnuPosition camera, int viewportHeight, double verticalFov) =>
        GeoUnifiedScreenError.Estimate(ErrorMeters, viewportHeight, Bounds.DistanceTo(camera), verticalFov);
}
