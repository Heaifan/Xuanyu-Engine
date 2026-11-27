using Xunit;
using XuanYu.Editor.Pages;
using XuanYu.Editor.TerrainEditing;

namespace XuanYu.World.Tests;

public sealed class test_E5MR01AlpineGeneration
{
    [Theory]
    [InlineData(7429, 61.477669, 20.828845, 25.058133, 146, 127, 45.130299, 24.362711, 28.611998, 33.839970, 22.181055)]
    [InlineData(7, 60.997433, 19.600055, 23.921610, 117, 137, 53.598629, 22.377911, 20.651571, 24.108217, 9.007910)]
    [InlineData(8, 61.131958, 19.667102, 23.701074, 136, 140, 43.713512, 21.216780, 25.268522, 32.882214, 19.296438)]
    public void RawField_MatchesOriginalStage3(int seed, double peak, double mean, double rms,
        int px, int py, double center, double a, double b, double c, double d)
    {
        var raw = TerrainAlpineGenerator.CreateRaw(256, 256, seed);
        Assert.Equal(65536, raw.Length);
        Assert.All(raw, h => Assert.True(float.IsFinite(h) && h >= 2.5f));
        Near(raw.Max(), peak);
        Near(raw.Average(h => (double)h), mean);
        Near(Math.Sqrt(raw.Average(h => (double)h * h)), rms);
        Assert.Equal(py * 256 + px, Array.IndexOf(raw, raw.Max()));
        Near(raw[128 * 256 + 128], center);
        Near(raw[64 * 256 + 64], a);
        Near(raw[192 * 256 + 64], b);
        Near(raw[64 * 256 + 192], c);
        Near(raw[192 * 256 + 192], d);
        Assert.Equal(2.5f, raw[0]);
        Assert.Equal(2.5f, raw[^1]);
        Assert.Equal(raw, TerrainAlpineGenerator.CreateRaw(256, 256, seed));
    }

    [Fact]
    public void DefaultSeed_RegionalMeansMatchOriginalStage3()
    {
        double[] expected = [5.452974, 15.833426, 16.831060, 9.262403,
            17.444097, 37.617348, 39.996639, 24.829919,
            23.902796, 38.227540, 38.967876, 18.404502,
            9.359502, 14.795328, 16.692811, 5.643304];
        var raw = TerrainAlpineGenerator.CreateRaw(256, 256, 7429);
        for (int by = 0; by < 4; by++)
        for (int bx = 0; bx < 4; bx++)
        {
            double sum = 0d;
            for (int y = by * 64; y < (by + 1) * 64; y++)
            for (int x = bx * 64; x < (bx + 1) * 64; x++) sum += raw[y * 256 + x];
            Near(sum / 4096d, expected[by * 4 + bx]);
        }
    }

    [Fact]
    public void Workbench_AlpineUsesMeterScaleAndExistingHistory()
    {
        var wb = new Workbench(use3D: true);
        wb.ResizeLocalTerrain(200, 200, 1);
        wb.SwitchTo(PageId.Terrain);
        wb.SetTerrainMode(TerrainWorkspaceMode.Edit);
        wb.SelectTerrainMountainTool();
        wb.SetTerrainMountainHeight(60f);
        wb.SetTerrainNaturalPeakSeed(7429);
        var field = wb.TerrainMap.HeightField;
        Assert.Equal(201, field.SamplesX);
        Assert.Equal(201, field.SamplesY);
        var before = field.Samples.ToArray();
        Assert.True(wb.GenerateAlpineTerrain());
        var after = field.Samples.ToArray();
        Near(after.Zip(before, (a, b) => a - b).Max(), 60d);
        Near(field.GetHeight(100, 100) - before[100 * 201 + 100], 41.837713d);
        Assert.Equal(before[0], after[0]);
        Assert.True(wb.CanUndoTerrain);
        wb.UndoTerrain(); Assert.Equal(before, field.Samples.ToArray());
        wb.RedoTerrain(); Assert.Equal(after, field.Samples.ToArray());
    }

    private static void Near(double actual, double expected) =>
        Assert.InRange(Math.Abs(actual - expected), 0d, 0.0002d);
}