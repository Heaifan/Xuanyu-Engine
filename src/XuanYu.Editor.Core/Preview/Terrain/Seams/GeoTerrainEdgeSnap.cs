using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

internal static class GeoTerrainEdgeSnap
{
    public static Vector3 Align(Vector3 point, int row, int col, int cells,
        GeoTileBounds fine, GeoTerrainEdgeNeighbors neighbors,
        NasademElevationSource source, EnuFrame anchor, GeoTileBounds coverage)
    {
        bool horizontal;
        GeoTileId other;
        if (row == 0 && neighbors.North is { } north)
        { other = north; horizontal = true; }
        else if (row == cells && neighbors.South is { } south)
        { other = south; horizontal = true; }
        else if (col == 0 && neighbors.West is { } west)
        { other = west; horizontal = false; }
        else if (col == cells && neighbors.East is { } east)
        { other = east; horizontal = false; }
        else return point;

        GeoTileBounds tile = GeographicTileScheme.Bounds(other);
        var coarse = new GeoTileBounds(
            Math.Max(tile.SouthLatitudeDegrees, coverage.SouthLatitudeDegrees),
            Math.Max(tile.WestLongitudeDegrees, coverage.WestLongitudeDegrees),
            Math.Min(tile.NorthLatitudeDegrees, coverage.NorthLatitudeDegrees),
            Math.Min(tile.EastLongitudeDegrees, coverage.EastLongitudeDegrees));
        double[] layout = GeoTerrainEdgeLayout.Fit(fine, coarse, cells, horizontal);
        double position = layout[horizontal ? col : row];
        double latitude = horizontal ? (row == 0 ? fine.NorthLatitudeDegrees :
            fine.SouthLatitudeDegrees) : fine.NorthLatitudeDegrees -
            (fine.NorthLatitudeDegrees - fine.SouthLatitudeDegrees) * position;
        double longitude = horizontal ? fine.WestLongitudeDegrees +
            (fine.EastLongitudeDegrees - fine.WestLongitudeDegrees) * position :
            (col == 0 ? fine.WestLongitudeDegrees : fine.EastLongitudeDegrees);
        double span = horizontal ? coarse.EastLongitudeDegrees - coarse.WestLongitudeDegrees
            : coarse.NorthLatitudeDegrees - coarse.SouthLatitudeDegrees;
        double t = horizontal ? (longitude - coarse.WestLongitudeDegrees) / span
            : (coarse.NorthLatitudeDegrees - latitude) / span;
        double scaled = Math.Clamp(t * cells, 0d, cells);
        int first = Math.Min(cells - 1, (int)Math.Floor(scaled));
        float fraction = (float)(scaled - first);
        Vector3? a = Sample(first);
        Vector3? b = Sample(first + 1);
        return a is { } left && b is { } right
            ? Vector3.Lerp(left, right, fraction) : point;

        Vector3? Sample(int step)
        {
            double u = (double)step / cells;
            double lat = horizontal ? latitude : coarse.NorthLatitudeDegrees - span * u;
            double lon = horizontal ? coarse.WestLongitudeDegrees + span * u : longitude;
            double? elevation = source.Sample(lat, lon);
            if (elevation is null) return null;
            EnuPosition p = anchor.ToEnu(Wgs84.ToEcef(
                new GeoCoordinate(lat, lon, 0d)));
            return new Vector3((float)p.East, (float)p.North,
                (float)(p.Up + elevation.Value));
        }
    }
}