using System.IO.Compression;
using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G05SourceResidencyZip
{
    [Fact]
    public void ZipLoadsOnDemand_AndSummaryDoesNotRequireAnotherDecode()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        hgt.SetHeight(1800, 1800, 1234);
        string zip = Path.Combine(AppContext.BaseDirectory, $"XYE-G05D-{Guid.NewGuid():N}.zip");
        try
        {
            using (var archive = ZipFile.Open(zip, ZipArchiveMode.Create))
                archive.CreateEntryFromFile(hgt.Path, "NASADEM_HGT_n24e121.hgt");
            var source = new NasademElevationSource(new NasademTileCatalog([zip]), 1);
            Assert.Equal(0, source.Residency.ResidentCount);
            Assert.Equal(1234d, source.Sample(24.5, 121.5)!.Value);
            Assert.Equal((short)1234, source.ReadSummary(24, 121)!.Value.MaxElevation);
            Assert.Equal(1L, source.Residency.LoadCount);
            Assert.Equal(1, source.Residency.ResidentCount);
        }
        finally { File.Delete(zip); }
    }
}