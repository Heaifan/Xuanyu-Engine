using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G05OfflineCodec
{
    [Fact]
    public void RealHgt_RoundTripsEverySampleIncludingNoData()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        hgt.SetHeight(0, 0, short.MinValue);
        hgt.SetHeight(3600, 3600, -32767);
        hgt.SetHeight(1800, 1800, 1234);
        string output = Path.Combine(AppContext.BaseDirectory,
            $"XYE-G05D04-{Guid.NewGuid():N}.xyt");
        try
        {
            var result = XYTerrainWriter.Write(hgt.Path, output);
            Assert.True(result.PackBytes > 0);
            Assert.Equal(NasademTile.ByteLength, result.RawBytes);
            Assert.Equal(31, result.RawBlocks + result.DeflateBlocks +
                result.BrotliBlocks + result.DeltaBrotliBlocks);
            NasademTile original = hgt.Load();
            NasademTile restored = XYTerrainReader.ReadTile(output);
            Assert.Equal(original.TileId, restored.TileId);
            Assert.Equal(original.NoDataCount, restored.NoDataCount);
            Assert.Equal(original.MinElevation, restored.MinElevation);
            Assert.Equal(original.MaxElevation, restored.MaxElevation);
            for (int row = 0; row < NasademTile.Side; row++)
                for (int col = 0; col < NasademTile.Side; col++)
                    Assert.Equal(original.HeightAt(col, row), restored.HeightAt(col, row));
            foreach (int row in new[] { 0, 1, 119, 120, 239, 240, 3599, 3600 })
                foreach (int col in new[] { 0, 1, 1800, 3600 })
                    Assert.Equal(original.HeightAt(col, row),
                        XYTerrainReader.ReadHeightAt(output, col, row));
        }
        finally { if (File.Exists(output)) File.Delete(output); }
    }

    [Fact]
    public void CorruptedHeader_IsRejectedAtReaderEntry()
    {
        string output = Path.Combine(AppContext.BaseDirectory,
            $"XYE-G05D04-Bad-{Guid.NewGuid():N}.xyt");
        try
        {
            File.WriteAllBytes(output, new byte[28]);
            Assert.Throws<InvalidDataException>(() => XYTerrainReader.ReadInfo(output));
        }
        finally { File.Delete(output); }
    }
}