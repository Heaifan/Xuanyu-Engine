using System.IO.Compression;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07ReloadVersion
{
    [G07DataFact]
    public async Task SameVersionRejectsChangedSourceAfterEviction()
    {
        string folder = Path.Combine(Path.GetTempPath(), "XYE-G07-" + Guid.NewGuid());
        Directory.CreateDirectory(folder);
        string path = Path.Combine(folder, "n23e120.hgt");
        try
        {
            using (var zip = ZipFile.OpenRead(Environment.GetEnvironmentVariable("XYE_G07_WEST")!))
            using (var input = zip.GetEntry("n23e120.hgt")!.Open())
            using (var output = File.Create(path)) input.CopyTo(output);
            var loader = new GeoUnifiedTileLoader(new NasademTileCatalog([path]), 80_000_000);
            Assert.NotNull(await loader.Request(23, 120));
            Assert.True(loader.Release(23, 120));
            using (var file = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
            {
                file.Position = 1; int value = file.ReadByte();
                file.Position = 1; file.WriteByte((byte)(value ^ 1));
            }
            await Assert.ThrowsAsync<InvalidDataException>(async () => await loader.Request(23, 120));
            Assert.Equal(0, loader.ReservedBytes);
        }
        finally { Directory.Delete(folder, true); }
    }
}
