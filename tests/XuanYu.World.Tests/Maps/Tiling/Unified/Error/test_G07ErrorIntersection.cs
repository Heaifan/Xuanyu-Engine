using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07ErrorIntersection
{
    [Fact]
    public void FoldedTriangleMustMeasureCrossingsBetweenFineGridVertices()
    {
        string folder = Path.Combine(Path.GetTempPath(), "XYE-G07-" + Guid.NewGuid());
        Directory.CreateDirectory(folder);
        string path = Path.Combine(folder, "n23e120.hgt");
        try
        {
            using (var file = File.Create(path))
            {
                file.SetLength(NasademTile.ByteLength);
                int column = 3554, rowFromSouth = 753;
                file.Position = ((long)(3600 - rowFromSouth) * 3601 + column) * 2;
                file.Write([0, 100]);
            }
            var catalog = new NasademTileCatalog([path]);
            var source = new GeoUnifiedSamples(catalog, "synthetic-test");
            source.Add(NasademHgtFile.Load(path));
            var tile = new GeoUnifiedTile(0, 33861, 12736);
            var page = GeoUnifiedHeightPage.Create(tile, source);
            var anchor = new EnuFrame(new GeoCoordinate(23.5, 121, 0));
            double measured = GeoUnifiedError.Measure(page, source, anchor, 1)!.Value;
            Assert.True(measured >= 49.9, $"交点误差至少约 50 m，实际 {measured:F3} m");
        }
        finally { Directory.Delete(folder, true); }
    }
}
