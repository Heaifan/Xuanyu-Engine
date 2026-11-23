namespace XuanYu.Editor.Preview.Terrain;

public readonly record struct GeoTerrainHudScale(double Meters, double WidthDip)
{
    public static GeoTerrainHudScale? From(double? metersPerPixel, double scaling)
    {
        if (metersPerPixel is not > 0d || !double.IsFinite(metersPerPixel.Value) ||
            !double.IsFinite(scaling) || scaling <= 0d) return null;
        double target = metersPerPixel.Value * scaling * 110d;
        double power = Math.Pow(10d, Math.Floor(Math.Log10(target)));
        double ratio = target / power;
        double step = ratio < 1.5d ? 1d : ratio < 3.5d ? 2d :
            ratio < 7.5d ? 5d : 10d;
        double meters = step * power;
        return new GeoTerrainHudScale(meters,
            meters / metersPerPixel.Value / scaling);
    }
}