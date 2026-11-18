using System.Numerics;
using XuanYu.Core.Geodesy;

namespace XuanYu.World.Maps;

public static partial class NasademBlockMeshBuilder
{
    public static TerrainMeshGeometry Build(NasademSampleBlock block, EnuFrame anchor)
    {
        ArgumentNullException.ThrowIfNull(block);
        ArgumentNullException.ThrowIfNull(anchor);

        int cells = NasademSampleBlock.CellsPerEdge;
        int side = cells + 1;
        var positions = new Vector3[side * side];
        var valid = new bool[side * side];
        var grid = new NasademGeoGrid(block.SouthLatitude, block.WestLongitude);

        for (int row = 0; row < side; row++)
            for (int column = 0; column < side; column++)
            {
                short? height = block.HeightAt(column, row);
                if (height is not { } meters) continue;
                GeoCoordinate location = grid.AtSample(
                    block.StartColumn + column,
                    block.StartRowFromSouth + row, 0d);
                EnuPosition ground = anchor.ToEnu(Wgs84.ToEcef(location));
                int index = row * side + column;
                positions[index] = new Vector3((float)ground.East,
                    (float)ground.North, (float)(ground.Up + meters));
                valid[index] = true;
            }

        var indices = new List<int>(cells * cells * 6);
        for (int row = 0; row < cells; row++)
            for (int column = 0; column < cells; column++)
            {
                int a = row * side + column;
                int b = a + 1;
                int c = a + side;
                int d = c + 1;
                AddTriangle(indices, valid, a, b, d);
                AddTriangle(indices, valid, a, d, c);
            }

        return new TerrainMeshGeometry(positions,
            CalculateNormals(positions, indices), indices.ToArray());
    }

    private static void AddTriangle(List<int> indices, bool[] valid,
        int a, int b, int c)
    {
        if (!valid[a] || !valid[b] || !valid[c]) return;
        indices.Add(a);
        indices.Add(b);
        indices.Add(c);
    }
}