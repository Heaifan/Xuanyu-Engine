using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public static partial class GeoTerrainMeshBuilder
{
    public static RenderMesh? Build(GeoHeightTile tile, EnuFrame anchor)
    {
        int side = tile.SamplesPerSide;
        var points = new Vector3[side * side];
        var valid = new bool[side * side];
        var colors = new Vector3[side * side];
        GeoTileBounds bounds = tile.Bounds;
        for (int row = 0; row < side; row++)
            for (int col = 0; col < side; col++)
            {
                double? height = tile.HeightAt(col, row);
                if (height is not { } elevation) continue;
                double latitude = bounds.NorthLatitudeDegrees -
                    (bounds.NorthLatitudeDegrees - bounds.SouthLatitudeDegrees) *
                    row / tile.CellsPerEdge;
                double longitude = bounds.WestLongitudeDegrees +
                    (bounds.EastLongitudeDegrees - bounds.WestLongitudeDegrees) *
                    col / tile.CellsPerEdge;
                EnuPosition local = anchor.ToEnu(Wgs84.ToEcef(
                    new GeoCoordinate(latitude, longitude, 0d)));
                int index = row * side + col;
                points[index] = new Vector3((float)local.East,
                    (float)local.North, (float)(local.Up + elevation));
                valid[index] = true;
                float t = Math.Clamp((float)((elevation + 100d) / 3000d), 0f, 1f);
                colors[index] = Vector3.Lerp(new Vector3(0.21f, 0.35f, 0.24f),
                    new Vector3(0.66f, 0.64f, 0.58f), t);
            }

        var vertices = new List<float>();
        for (int y = 0; y < side - 1; y++)
            for (int x = 0; x < side - 1; x++)
            {
                int a = y * side + x;
                int b = (y + 1) * side + x;
                int c = a + 1;
                int d = b + 1;
                AddTriangle(vertices, points, colors, valid, a, b, c);
                AddTriangle(vertices, points, colors, valid, c, b, d);
            }
        return vertices.Count == 0 ? null :
            new RenderMesh($"terrain/g02/{tile.Id}", vertices.ToArray());
    }
}