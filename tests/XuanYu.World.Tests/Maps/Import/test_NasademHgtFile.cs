
using System.IO;
using System.IO.Compression;
using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class TestNasademHgtFile
{
    [Fact]
    public void Hgt_ReadsBigEndianAndSouthNorthDirection()
    {
        string path = CreateTile();
        try
        {
            NasademTile tile = NasademHgtFile.Load(path);
            Assert.Equal("N24E121", tile.TileId);
            Assert.Equal(24, tile.SouthLatitude);
            Assert.Equal(121, tile.WestLongitude);
            Assert.Equal((short)300, tile.HeightAt(0, 0));
            Assert.Equal((short)-20, tile.HeightAt(0, 3600));
            Assert.Equal(0, tile.NoDataCount);
            Assert.Equal((short)-20, tile.MinElevation);
            Assert.Equal((short)300, tile.MaxElevation);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Zip_ReadsHgtEntry()
    {
        string source = CreateTile();
        string zip = NewPath(".zip");
        try
        {
            using (var archive = ZipFile.Open(zip, ZipArchiveMode.Create))
            {
                var entry = archive.CreateEntry("NASADEM_HGT_n24e121.hgt");
                using var output = entry.Open();
                using var input = File.OpenRead(source);
                input.CopyTo(output);
            }
            var tile = NasademHgtFile.Load(zip);
            Assert.Equal("N24E121", tile.TileId);
            Assert.Equal((short)300, tile.HeightAt(0, 0));
        }
        finally
        {
            File.Delete(source);
            File.Delete(zip);
        }
    }

    [Fact]
    public void NoData_IsNotTreatedAsElevation()
    {
        string path = CreateTile();
        try
        {
            using (var stream = File.OpenWrite(path))
            {
                stream.Position = 0;
                stream.Write([0x80, 0x00]);
            }
            var tile = NasademHgtFile.Load(path);
            Assert.Null(tile.HeightAt(0, 3600));
            Assert.Equal(1, tile.NoDataCount);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void InvalidByteLength_IsRejected()
    {
        string path = NewPath(".hgt");
        try
        {
            File.WriteAllBytes(path, [0, 1]);
            Assert.Throws<InvalidDataException>(() => NasademHgtFile.Load(path));
        }
        finally { File.Delete(path); }
    }

    private static string CreateTile()
    {
        string path = NewPath(".hgt");
        using var stream = File.Create(path);
        stream.SetLength(NasademTile.ByteLength);
        stream.Position = 0;
        stream.Write([0xFF, 0xEC]);
        stream.Position = (long)(NasademTile.Side - 1) * NasademTile.Side * 2;
        stream.Write([0x01, 0x2C]);
        return path;
    }

    private static string NewPath(string extension) => Path.Combine(
        AppContext.BaseDirectory, $"XYE-{Guid.NewGuid():N}-n24e121{extension}");
}
