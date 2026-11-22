using System.Numerics;
using XuanYu.Core.Geodesy;

namespace XuanYu.Editor.Preview.Terrain;

internal static class GeoContourLabelGrid
{
    private const int Cells = 12;

    public static GeoContourLabel[] Build(GeoCoordinate center, double distance,
        int majorInterval, Matrix4x4 projection, EnuFrame anchor,
        uint width, uint height, Func<double, double, double?> sample)
    {
        var frame = new EnuFrame(new GeoCoordinate(
            center.LatitudeDegrees, center.LongitudeDegrees, 0d));
        double radius = Math.Clamp(distance * 0.9d, 400d, 18_000d);
        var grid = new GeoContourLabelSample[Cells + 1, Cells + 1];
        for (int row = 0; row <= Cells; row++)
            for (int col = 0; col <= Cells; col++)
            {
                double east = (col * 2d / Cells - 1d) * radius;
                double north = (row * 2d / Cells - 1d) * radius;
                GeoCoordinate geo = Wgs84.ToGeo(
                    frame.ToEcef(new EnuPosition(east, north, 0d)));
                grid[row, col] = new(geo.LatitudeDegrees, geo.LongitudeDegrees,
                    sample(geo.LatitudeDegrees, geo.LongitudeDegrees));
            }
        var candidates = new List<GeoContourLabel>();
        for (int row = 0; row <= Cells; row++)
            for (int col = 0; col <= Cells; col++)
            {
                var point = grid[row, col];
                if (col > 0 && GeoContourLabelProjection.TryCrossing(
                    grid[row, col - 1], point, majorInterval, sample,
                    anchor, projection, width, height, out var horizontal))
                    candidates.Add(horizontal);
                if (row > 0 && GeoContourLabelProjection.TryCrossing(
                    grid[row - 1, col], point, majorInterval, sample,
                    anchor, projection, width, height, out var vertical))
                    candidates.Add(vertical);
            }
        return GeoContourLabelLayout.Select(candidates, width, height);
    }
}

internal readonly record struct GeoContourLabelSample(
    double Latitude, double Longitude, double? Height);