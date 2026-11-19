using XuanYu.Core.Geodesy;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public readonly record struct GeoContourCoordinate(double Latitude, double Longitude);
public readonly record struct GeoContourSegment(int Level, GeoContourCoordinate A,
    GeoContourCoordinate B);

public sealed partial class GeoContourElevationTask
{
    private readonly NasademElevationSource _source;
    private readonly EnuFrame _anchor;
    private readonly GeoContourStyle _style;
    private readonly double[] _xs;
    private readonly double[] _ys;
    private readonly List<GeoContourSegment> _segments = [];
    private double?[]? _previous;
    private int _row;

    public GeoTileId TileId { get; }

    public GeoContourElevationTask(GeoTileId id, GeoTileBounds coverage,
        NasademElevationSource source, EnuFrame anchor, GeoContourStyle style, int step)
    {
        TileId = id;
        _source = source;
        _anchor = anchor;
        _style = style;
        GeoTileBounds bounds = GeographicTileScheme.Bounds(id);
        _xs = Axis(Math.Max(bounds.WestLongitudeDegrees, coverage.WestLongitudeDegrees),
            Math.Min(bounds.EastLongitudeDegrees, coverage.EastLongitudeDegrees), step);
        _ys = Axis(Math.Max(bounds.SouthLatitudeDegrees, coverage.SouthLatitudeDegrees),
            Math.Min(bounds.NorthLatitudeDegrees, coverage.NorthLatitudeDegrees), step);
    }

    public bool Advance(int rows)
    {
        if (_xs.Length < 2 || _ys.Length < 2) return true;
        double?[] previous = _previous ??= SampleRow(0);
        int end = Math.Min(_ys.Length - 1, _row + rows);
        while (_row < end)
        {
            double?[] next = SampleRow(_row + 1);
            for (int x = 0; x < _xs.Length - 1; x++)
                if (previous[x] is { } a && previous[x + 1] is { } b &&
                    next[x + 1] is { } c && next[x] is { } d)
                    TraceCell(x, a, b, c, d);
            _previous = next;
            previous = next;
            _row++;
        }
        return _row == _ys.Length - 1;
    }

    public RenderMesh? Finish() => GeoContourPolyline.Build(
        TileId, _segments, _anchor, _style);
}