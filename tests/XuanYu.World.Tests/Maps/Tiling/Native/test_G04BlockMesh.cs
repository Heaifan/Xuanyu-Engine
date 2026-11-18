using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G04BlockMesh
{
    [Fact]
    public void StepOne_UsesRealSamplesAndIndexedTriangles()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        hgt.SetHeight(3, 7, 1250);
        hgt.SetHeight(297, 299, -13);
        NasademTile source = hgt.Load();
        var block = new NasademSampleBlock(source, 0, 0);
        var anchor = new EnuFrame(new GeoCoordinate(24.5d, 121.5d, 0d));
        TerrainMeshGeometry mesh = NasademBlockMeshBuilder.Build(block, anchor);

        Assert.Equal(301 * 301, mesh.VertexCount);
        Assert.Equal(300 * 300 * 2, mesh.TriangleCount);
        Assert.Equal(new[] { 0, 1, 302, 0, 302, 301 },
            mesh.Indices.Take(6).ToArray());
        VerifyPosition(source, anchor, mesh, 0, 0);
        VerifyPosition(source, anchor, mesh, 3, 7);
        VerifyPosition(source, anchor, mesh, 297, 299);
        VerifyPosition(source, anchor, mesh, 300, 300);
        Assert.All(mesh.Normals, normal =>
            Assert.InRange(normal.Length(), 0.999f, 1.001f));
        Assert.True(mesh.Normals[0].Z > 0f);
    }

    [Fact]
    public void NeighborMeshes_KeepIdenticalBoundaryPositions()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        hgt.SetHeight(300, 120, 412);
        NasademTile source = hgt.Load();
        var anchor = new EnuFrame(new GeoCoordinate(24.5d, 121.5d, 0d));
        TerrainMeshGeometry west = NasademBlockMeshBuilder.Build(
            new NasademSampleBlock(source, 0, 0), anchor);
        TerrainMeshGeometry east = NasademBlockMeshBuilder.Build(
            new NasademSampleBlock(source, 1, 0), anchor);
        foreach (int row in new[] { 0, 120, 300 })
            Assert.Equal(west.Positions[row * 301 + 300],
                east.Positions[row * 301]);
    }

    private static void VerifyPosition(NasademTile source, EnuFrame anchor,
        TerrainMeshGeometry mesh, int column, int rowFromSouth)
    {
        var geo = new GeoCoordinate(24d + rowFromSouth / 3600d,
            121d + column / 3600d, 0d);
        EnuPosition flat = anchor.ToEnu(Wgs84.ToEcef(geo));
        Vector3 vertex = mesh.Positions[rowFromSouth * 301 + column];
        double elevation = source.HeightAt(column, rowFromSouth)!.Value;
        Assert.InRange((double)vertex.X - flat.East, -0.02d, 0.02d);
        Assert.InRange((double)vertex.Y - flat.North, -0.02d, 0.02d);
        Assert.InRange((double)vertex.Z - (flat.Up + elevation),
            -0.02d, 0.02d);
    }
}