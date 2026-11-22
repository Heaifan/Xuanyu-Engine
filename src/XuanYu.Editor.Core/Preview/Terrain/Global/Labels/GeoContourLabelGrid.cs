using System.Numerics;
using XuanYu.Core.Geodesy;

namespace XuanYu.Editor.Preview.Terrain;

internal static class GeoContourLabelGrid
{
    private const int Cells = 28;

    public static GeoContourLabel[] Build(GeoCoordinate center, double distance,
        int majorInterval, Matrix4x4 projection, EnuFrame anchor,
        uint width, uint height, Func<double, double, double?> sample)
    {
        var frame = new EnuFrame(new GeoCoordinate(
            center.LatitudeDegrees, center.LongitudeDegrees, 0d));
        double radius = Math.Clamp(distance * 1.15d, 400d, 18_000d);
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
        var topology = new GeoContourLabelTopology();
        for (int row = 0; row <= Cells; row++)
            for (int col = 0; col <= Cells; col++)
            {
                if (col > 0) Scan(grid[row, col - 1], grid[row, col], row, col - 1, false);
                if (row > 0) Scan(grid[row - 1, col], grid[row, col], row - 1, col, true);
            }
        for (int row = 0; row < Cells; row++)
            for (int col = 0; col < Cells; col++)
                topology.ConnectCell(grid, row, col, majorInterval);
        return GeoContourLabelLayout.Select(topology.Candidates(), width, height);

        void Scan(GeoContourLabelSample a, GeoContourLabelSample b,
            int row, int col, bool vertical)
        {
            if (a.Height is not { } ah || b.Height is not { } bh) return;
            int first = (int)Math.Floor(Math.Min(ah, bh) / majorInterval) + 1;
            int last = (int)Math.Ceiling(Math.Max(ah, bh) / majorInterval) - 1;
            for (int k = first; k <= last; k++)
            {
                int elevation = k * majorInterval;
                bool found = GeoContourLabelProjection.TryCrossing(a, b, elevation,
                    sample, anchor, projection, width, height, out var label);
                topology.Add(new(row, col, vertical, elevation), found ? label : null);
            }
        }
    }
}

internal readonly record struct GeoContourLabelSample(
    double Latitude, double Longitude, double? Height);