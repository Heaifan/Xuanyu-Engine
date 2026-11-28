namespace XuanYu.World.Maps;

public readonly record struct GeoElevationSample(
    GeoSampleStatus Status, double? HeightMeters, string Version)
{
    public MapVerticalDatum Datum => MapVerticalDatum.Egm96Orthometric;
}