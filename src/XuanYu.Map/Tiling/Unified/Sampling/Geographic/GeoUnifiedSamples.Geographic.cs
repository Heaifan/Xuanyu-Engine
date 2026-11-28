using XuanYu.Core.Geodesy;

namespace XuanYu.World.Maps;

public sealed partial class GeoUnifiedSamples
{
    // 只使用经纬度；输入的椭球高不参与 EGM96 正高查询。
    public GeoElevationSample SampleElevation(GeoCoordinate position)
    {
        using var pin = _loader.PinSample(_generation);
        double x = SnapArcsecond((position.LongitudeDegrees + 180d) * 3600d);
        double y = SnapArcsecond((position.LatitudeDegrees + 90d) * 3600d);
        int left = (int)Math.Floor(x), right = (int)Math.Ceiling(x);
        int bottom = (int)Math.Floor(y), top = (int)Math.Ceiling(y);
        var a = SampleRaw(left, bottom);
        var b = right == left ? a : SampleRaw(right, bottom);
        var c = top == bottom ? a : SampleRaw(left, top);
        var d = top == bottom ? b : right == left ? c : SampleRaw(right, top);
        var status = Merge(Merge(a.Status, b.Status), Merge(c.Status, d.Status));
        if (status != GeoSampleStatus.Valid) return new(status, null, _version);
        double u = x - left, v = y - bottom;
        double height = (a.Height!.Value * (1d - u) + b.Height!.Value * u) * (1d - v) +
            (c.Height!.Value * (1d - u) + d.Height!.Value * u) * v;
        return new(status, height, _version);
    }

    // 枚举顺序恰为 Valid < NoData < NotLoaded < OutsideCoverage；
    // 缺覆盖不可修复，未驻留可重试，只有已驻留的缺值才报告 NoData。
    private static GeoSampleStatus Merge(GeoSampleStatus a, GeoSampleStatus b) =>
        (GeoSampleStatus)Math.Max((int)a, (int)b);

    private static double SnapArcsecond(double value)
    {
        double rounded = Math.Round(value);
        return Math.Abs(value - rounded) < 1e-8d ? rounded : value;
    }
}