using System.Numerics;
using Xunit;
using Xunit.Abstractions;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G06RawGeometry(ITestOutputHelper output)
{
    [G06GeometryFact]
    public void RealHgt_VisibleL13ImprovesWithoutChangingTopologyOrEdges()
    {
        string path = Environment.GetEnvironmentVariable("XYE_G06_R04_HGT")!;
        NasademTile raw = NasademHgtFile.Load(path);
        Assert.Equal("N23E120", raw.TileId);
        var session = new GeoTerrainRenderSession(new NasademTileCatalog([path]),
            16, useOverview: true, streamRaw: true) { RawGeometryEnabled = false };
        session.ToggleNative();
        var id = GeographicTileScheme.Find(new GeoCoordinate(23.5, 120.85, 0), 13);
        GeoTileBounds region = GeographicTileScheme.Bounds(id);
        var focus = new GeoCoordinate((region.SouthLatitudeDegrees + region.NorthLatitudeDegrees) / 2,
            (region.WestLongitudeDegrees + region.EastLongitudeDegrees) / 2, 0);
        session.Camera.FrameTop(focus, 5_000);
        session.Camera.Orbit(0, -0.55);
        RenderDraw before = default!, after = default!;
        Assert.True(SpinWait.SpinUntil(() =>
        {
            var draws = session.Draws(1280, 720);
            if (session.PendingCount != 0) return false;
            foreach (var draw in draws)
                if (draw.Mesh.AssetId == $"terrain/g02/{id}") { before = draw; return true; }
            return false;
        }, TimeSpan.FromSeconds(30)), "普通 L13 未就绪");
        Assert.Equal(1536, before.Mesh.VertexCount);
        session.RawGeometryEnabled = true;
        Assert.True(SpinWait.SpinUntil(() =>
        {
            foreach (var draw in session.Draws(1280, 720))
                if (draw.Mesh.AssetId == before.Mesh.AssetId && !ReferenceEquals(draw.Mesh, before.Mesh))
                { after = draw; return session.RawGeometryCount > 0; }
            return false;
        }, TimeSpan.FromSeconds(30)), "原始高程改良 L13 未就绪");
        AssertTopologyAndEdges(before.Mesh, after.Mesh);
        var samples = G06GeometryMeasure.Samples(raw, new EnuFrame(new(23.5, 120.5, 0)), focus);
        Assert.True(Error(after.Mesh, samples) < Error(before.Mesh, samples));
        output.WriteLine(G06GeometryMeasure.Report("Production-Middle", before, before.ModelToClip, samples));
        output.WriteLine(G06GeometryMeasure.Report("Production-Raw", after, before.ModelToClip, samples));
        var warm = Assert.Single(session.Draws(1280, 720),
            d => d.Mesh.AssetId == before.Mesh.AssetId);
        Assert.Same(after.Mesh, warm.Mesh);
        session.RawGeometryEnabled = false;
        var restored = Assert.Single(session.Draws(1280, 720),
            d => d.Mesh.AssetId == before.Mesh.AssetId);
        Assert.Equal(before.Mesh.Vertices.ToArray(), restored.Mesh.Vertices.ToArray());
    }

    private static double Error(RenderMesh mesh, IReadOnlyList<Vector3> samples)
    {
        double sum = 0;
        foreach (var p in samples)
        {
            double? z = GeoTerrainMeshSurface.HeightAt(mesh, p.X, p.Y);
            Assert.True(z.HasValue, "实际提交网格缺面");
            double delta = z!.Value - p.Z; sum += delta * delta;
        }
        return sum;
    }

    private static void AssertTopologyAndEdges(RenderMesh a, RenderMesh b)
    {
        Assert.Equal(a.VertexCount, b.VertexCount);
        var av = a.Vertices.Span; var bv = b.Vertices.Span;
        var geo = a.TerrainGeographicCoordinates.Span;
        Assert.True(geo.SequenceEqual(b.TerrainGeographicCoordinates.Span));
        for (int i = 0; i < a.VertexCount; i++)
        {
            int p = i * 7;
            Assert.Equal(av[p], bv[p]); Assert.Equal(av[p + 1], bv[p + 1]);
            if (!double.IsNaN(geo[i * 2])) continue;
            Assert.True(av.Slice(p, 7).SequenceEqual(bv.Slice(p, 7)));
            Assert.Equal(a.TerrainElevations.Span[i], b.TerrainElevations.Span[i]);
            Assert.True(a.TerrainShadedColors.Span.Slice(i * 3, 3)
                .SequenceEqual(b.TerrainShadedColors.Span.Slice(i * 3, 3)));
        }
    }
}