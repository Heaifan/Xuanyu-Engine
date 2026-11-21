using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class TestG06WaterAtlas
{
    [Fact]
    public void RealSwb_PacksNorthSouthAndAttachesAtFarDistance()
    {
        using var west = new G02BHgtFixture("n24e121");
        using var east = new G02BHgtFixture("n24e122");
        string swb = Path.ChangeExtension(west.Path, ".swb");
        try
        {
            var samples = new byte[NasademTile.SampleCount];
            samples[0] = 255;
            samples[^1] = 255;
            samples[(NasademTile.Side - 1) * NasademTile.Side + 15] = 255;
            File.WriteAllBytes(swb, samples);
            var catalog = new NasademTileCatalog([west.Path, east.Path]);
            var geo = new GeoTerrainRenderSession(catalog);
            geo.LoadWater(catalog, [new("N24E121", 24, 121)]);
            Assert.Equal(1, geo.LoadedWaterMaskCount);
            var draws = geo.Draws(776, 532).Where(d => !d.Mesh.TerrainElevations.IsEmpty).ToArray();
            Assert.NotEmpty(draws);
            RenderWaterAtlas atlas = Assert.IsType<RenderWaterAtlas>(draws[0].WaterAtlas);
            Assert.All(draws, d =>
            {
                Assert.Same(atlas, d.WaterAtlas);
                Assert.Equal(d.Mesh.VertexCount * 2, d.DemUV.Length);
            });
            ReadOnlySpan<short> words = atlas.PackedSamples.Span;
            Assert.Equal((short)2, words[0]);
            Assert.Equal((short)1, words[1]);
            Assert.Equal((short)1, words[2]);
            Assert.Equal((short)0, words[3]);
            Assert.Equal(short.MinValue, words[4]);
            int north = 3600 * NasademTile.Side;
            Assert.NotEqual(0, words[4 + north / 16] & (1 << (north % 16)));
            int lastSouth = 3600;
            Assert.NotEqual(0, words[4 + lastSouth / 16] & (1 << (lastSouth % 16)));
        }
        finally { File.Delete(swb); }
    }
}