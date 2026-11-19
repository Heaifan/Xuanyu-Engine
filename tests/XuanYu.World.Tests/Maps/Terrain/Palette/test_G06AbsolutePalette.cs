using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G06AbsolutePalette
{
    [Theory]
    [InlineData(-100f, 0x5CBB6F)]
    [InlineData(0f, 0x5CBB6F)]
    [InlineData(200f, 0x72C96A)]
    [InlineData(800f, 0xA8D96D)]
    [InlineData(1600f, 0xD5DF83)]
    [InlineData(2800f, 0xEDCF79)]
    [InlineData(4200f, 0xBF9369)]
    [InlineData(6000f, 0xA9A5A0)]
    [InlineData(7000f, 0xA9A5A0)]
    public void LocalElevation_UsesAbsoluteColorStops(float meters, int hex)
    {
        var field = new TerrainHeightField(2, 2, 20f);
        for (int y = 0; y < 2; y++)
            for (int x = 0; x < 2; x++) field.SetHeight(x, y, meters);
        RenderMesh mesh = TerrainRenderMeshAdapter.Create(
            TerrainMeshBuilder.Build(field), TerrainDisplayMode.Elevation);
        Vector3 actual = Color(mesh, 0);
        Vector3 expected = Rgb(hex);
        Assert.InRange(Vector3.Distance(Vector3.Normalize(actual),
            Vector3.Normalize(expected)), 0f, 0.00002f);
    }

    [Fact]
    public void SameHeight_DifferentTileRanges_KeepSameBaseColor()
    {
        RenderMesh a = Make(200f, 0f);
        RenderMesh b = Make(200f, 5000f);
        Vector3 ca = Vector3.Normalize(Color(a, 0));
        Vector3 cb = Vector3.Normalize(Color(b, 0));
        Assert.InRange(Vector3.Distance(ca, cb), 0f, 0.00002f);
    }

    [Fact]
    public void GlobalNativeMesh_UsesAbsoluteAltitudeNotEnuZ()
    {
        using var fixture = new G02BHgtFixture("N24E121");
        fixture.SetHeight(1801, 1801, 2800);
        NasademTile tile = fixture.Load();
        var source = new NasademElevationSource(tile);
        var anchor = new EnuFrame(new GeoCoordinate(24.5, 121.5, 0));
        GeoTileId id = GeographicTileScheme.Find(
            new GeoCoordinate(24.5003, 121.5003, 0), 13);
        var coverage = new GeoTileBounds(24, 121, 25, 122);
        RenderMesh mesh = GeoTerrainNativeMeshBuilder.Build(
            tile, id, anchor, source, coverage, default)!;
        GeoCoordinate sample = new(24d + 1801d / 3600d,
            121d + 1801d / 3600d, 0d);
        EnuPosition flat = anchor.ToEnu(Wgs84.ToEcef(sample));
        Vector3 expectedPosition = new((float)flat.East, (float)flat.North,
            (float)(flat.Up + 2800d));
        ReadOnlySpan<float> values = mesh.Vertices.Span;
        bool matched = false;
        for (int i = 0; i < mesh.VertexCount; i++)
        {
            int at = i * 7;
            var position = new Vector3(values[at], values[at + 1], values[at + 2]);
            if (Vector3.Distance(position, expectedPosition) >= 0.1f) continue;
            Assert.InRange(Vector3.Distance(Color(mesh, i), Rgb(0xEDCF79)),
                0f, 0.00002f);
            matched = true;
        }
        Assert.True(matched, "未在原生 NASADEM 网格找到 2800m 采样点");
    }

    private static RenderMesh Make(float first, float other)
    {
        var field = new TerrainHeightField(2, 2, 20f);
        for (int y = 0; y < 2; y++)
            for (int x = 0; x < 2; x++) field.SetHeight(x, y, other);
        field.SetHeight(0, 0, first);
        return TerrainRenderMeshAdapter.Create(
            TerrainMeshBuilder.Build(field), TerrainDisplayMode.Elevation);
    }

    private static Vector3 Color(RenderMesh mesh, int vertex)
    {
        ReadOnlySpan<float> data = mesh.Vertices.Span;
        int i = vertex * 7 + 3;
        return new(data[i], data[i + 1], data[i + 2]);
    }

    private static Vector3 Rgb(int hex) => new(
        ((hex >> 16) & 255) / 255f,
        ((hex >> 8) & 255) / 255f,
        (hex & 255) / 255f);
}