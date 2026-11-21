using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public static partial class GeoTerrainMeshBuilder
{
    public static RenderMesh? Build(GeoHeightTile tile, EnuFrame anchor) =>
        BuildCore(tile, anchor, null, null, default);

    public static RenderMesh? Build(GeoHeightTile tile, EnuFrame anchor,
        NasademElevationSource source, GeoTileBounds coverage) =>
        BuildCore(tile, anchor, source, coverage, default);

    public static RenderMesh? Build(GeoHeightTile tile, EnuFrame anchor,
        NasademElevationSource source, GeoTileBounds coverage,
        GeoTerrainEdgeNeighbors neighbors) =>
        BuildCore(tile, anchor, source, coverage, neighbors);

    private static RenderMesh? BuildCore(GeoHeightTile tile, EnuFrame anchor,
        NasademElevationSource? source, GeoTileBounds? coverage,
        GeoTerrainEdgeNeighbors neighbors)
    {
        GeoTileBounds bounds = tile.Bounds;
        if (coverage is { } area)
        {
            bounds = new GeoTileBounds(
                Math.Max(bounds.SouthLatitudeDegrees, area.SouthLatitudeDegrees),
                Math.Max(bounds.WestLongitudeDegrees, area.WestLongitudeDegrees),
                Math.Min(bounds.NorthLatitudeDegrees, area.NorthLatitudeDegrees),
                Math.Min(bounds.EastLongitudeDegrees, area.EastLongitudeDegrees));
            if (bounds.NorthLatitudeDegrees <= bounds.SouthLatitudeDegrees ||
                bounds.EastLongitudeDegrees <= bounds.WestLongitudeDegrees)
                return null;
        }
        int side = tile.SamplesPerSide;
        var points = new GeoTerrainEdgeVertex?[side * side];
        for (int row = 0; row < side; row++)
            for (int col = 0; col < side; col++)
            {
                double latitude = bounds.NorthLatitudeDegrees -
                    (bounds.NorthLatitudeDegrees - bounds.SouthLatitudeDegrees) *
                    row / tile.CellsPerEdge;
                double longitude = bounds.WestLongitudeDegrees +
                    (bounds.EastLongitudeDegrees - bounds.WestLongitudeDegrees) *
                    col / tile.CellsPerEdge;
                double? height = source is null ? tile.HeightAt(col, row) :
                    source.Sample(latitude, longitude);
                if (height is not { } elevation) continue;
                EnuPosition local = anchor.ToEnu(Wgs84.ToEcef(
                    new GeoCoordinate(latitude, longitude, 0d)));
                Vector3 position = new((float)local.East, (float)local.North,
                    (float)(local.Up + elevation));
                var vertex = GeoTerrainEdgeVertex.FromHeight(position, elevation,
                    source?.SampleSurfaceNormal(latitude, longitude));
                if (source is not null && coverage is { } region && !neighbors.IsEmpty)
                    vertex = GeoTerrainEdgeSnap.Align(vertex, row, col,
                        tile.CellsPerEdge, bounds, neighbors, source, anchor, region);
                points[row * side + col] = vertex;
            }

        var vertices = new List<float>();
        for (int y = 0; y < side - 1; y++)
            for (int x = 0; x < side - 1; x++)
            {
                int a = y * side + x;
                int b = (y + 1) * side + x;
                int c = a + 1;
                int d = b + 1;
                AddTriangle(vertices, points, a, b, c);
                AddTriangle(vertices, points, c, b, d);
            }
        return vertices.Count == 0 ? null :
            RenderMesh.FromLitTerrain($"terrain/g02/{tile.Id}", System.Runtime.InteropServices.CollectionsMarshal.AsSpan(vertices));
    }
}