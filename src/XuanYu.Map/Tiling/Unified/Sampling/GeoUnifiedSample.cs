namespace XuanYu.World.Maps;

public enum GeoSampleStatus { Valid, NoData, NotLoaded, OutsideCoverage }

public readonly record struct GeoUnifiedSample(
    GeoSampleStatus Status, short? Height, string Version)
{
    public MapVerticalDatum Datum => MapVerticalDatum.Egm96Orthometric;
}
