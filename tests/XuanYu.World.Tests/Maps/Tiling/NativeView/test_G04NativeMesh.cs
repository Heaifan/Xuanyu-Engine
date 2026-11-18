using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G04NativeMesh
{
    [Fact]
    public void NativeDetail_RetainsExactInteriorHeight()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        hgt.SetHeight(1801, 1801, 3500);
        NasademTile tile = hgt.Load();
        var source = new NasademElevationSource(tile);
        var anchor = new EnuFrame(new GeoCoordinate(24.5, 121.5, 0));
        GeoTileId id = GeographicTileScheme.Find(
            new GeoCoordinate(24.5003, 121.5003, 0), 13);
        var coverage = new GeoTileBounds(24, 121, 25, 122);
        var mesh = GeoTerrainNativeMeshBuilder.Build(tile, id,
            anchor, source, coverage, default)!;
        GeoCoordinate geo = new(24d + 1801d / 3600d,
            121d + 1801d / 3600d, 0d);
        EnuPosition flat = anchor.ToEnu(Wgs84.ToEcef(geo));
        Vector3 target = new((float)flat.East, (float)flat.North,
            (float)(flat.Up + 3500d));
        float[] data = mesh.Vertices.ToArray();
        Assert.True(mesh.TriangleCount > 512);
        Assert.Contains(Enumerable.Range(0, mesh.VertexCount), i =>
            Vector3.Distance(new Vector3(data[i * 7], data[i * 7 + 1],
                data[i * 7 + 2]), target) < 0.1f);
    }

    [Fact]
    public void NoData_RemovesAffectedNativeTriangles()
    {
        using var normalFile = new G02BHgtFixture("N24E121");
        using var missingFile = new G02BHgtFixture("N24E121");
        missingFile.SetHeight(1801, 1801, short.MinValue);
        var anchor = new EnuFrame(new GeoCoordinate(24.5, 121.5, 0));
        GeoTileId id = GeographicTileScheme.Find(
            new GeoCoordinate(24.5003, 121.5003, 0), 13);
        var coverage = new GeoTileBounds(24, 121, 25, 122);
        int normal = Build(normalFile.Load()).TriangleCount;
        int missing = Build(missingFile.Load()).TriangleCount;
        Assert.True(missing < normal);

        XuanYu.Render.RenderMesh Build(NasademTile tile) =>
            GeoTerrainNativeMeshBuilder.Build(tile, id, anchor,
                new NasademElevationSource(tile), coverage, default)!;
    }
}