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
            Assert.True(NasademWaterFile.HasMask(path, NasademHgtFile.ReadInfo(path)));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Catalog_ReadsSidecarByCoordinatesWithoutDecodingHgt()
    {
        string hgt = Path.Combine(AppContext.BaseDirectory,
            $"XYE-SWB-{Guid.NewGuid():N}-n24e121.hgt");
        string swb = Path.ChangeExtension(hgt, ".swb");
        try
        {
            using (var output = File.Create(hgt))
                output.SetLength(NasademTile.ByteLength);
            var catalog = new NasademTileCatalog([hgt]);
            var info = catalog.Entries.Single().Info;
            Assert.False(NasademWaterFile.HasMask(hgt, info));
            Assert.Null(catalog.LoadWaterMask(24, 121));
            using (var output = File.Create(swb))
            {
                output.SetLength(NasademTile.SampleCount);
                output.WriteByte(255);
            }
            // 索引完成后移除 HGT：掩膜读取不能依赖完整高程解码。
            File.Delete(hgt);
            Assert.True(NasademWaterFile.HasMask(hgt, info));
            var water = Assert.IsType<TerrainWaterMask>(catalog.LoadWaterMask(24, 121));
            Assert.True(water.IsWater(0, 3600));
            Assert.False(water.IsWater(0, 0));
            Assert.Null(catalog.LoadWaterMask(24, 122));
        }
        finally
        {
            File.Delete(hgt);
            File.Delete(swb);
        }
    }
}