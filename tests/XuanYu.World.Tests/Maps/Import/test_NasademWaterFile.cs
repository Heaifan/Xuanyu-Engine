using System.IO.Compression;
using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class TestNasademWaterFile
{
    [Fact]
    public void ZipSwb_IsReadNorthToSouthAndOrientedToSouthFirst()
    {
        string path = Path.Combine(AppContext.BaseDirectory,
            $"XYE-SWB-{Guid.NewGuid():N}-n24e121.zip");
        try
        {
            using (var archive = ZipFile.Open(path, ZipArchiveMode.Create))
            {
                var hgt = archive.CreateEntry("n24e121.hgt");
                using (var output = hgt.Open())
                {
                    byte[] zeros = new byte[NasademTile.SampleCount * 2];
                    output.Write(zeros);
                }
                var swb = archive.CreateEntry("n24e121.swb");
                using (var output = swb.Open())
                {
                    byte[] values = new byte[NasademTile.SampleCount];
                    values[0] = 255;
                    values[^1] = 255;
                    output.Write(values);
                }
            }
            NasademTile tile = NasademHgtFile.Load(path);
            TerrainWaterMask water = Assert.IsType<TerrainWaterMask>(
                NasademWaterFile.Load(path, tile));
            Assert.True(water.IsWater(0, 3600));
            Assert.True(water.IsWater(3600, 0));
            Assert.False(water.IsWater(0, 0));
        }
        finally { File.Delete(path); }
    }
}