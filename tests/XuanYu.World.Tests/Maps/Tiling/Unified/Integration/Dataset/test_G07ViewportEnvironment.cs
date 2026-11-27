using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Pages;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07ViewportEnvironment
{
    [Fact]
    public async Task ImportedTerrain_EnvironmentAndSurfaceUseG07()
    {
        string folder = Path.Combine(Environment.CurrentDirectory,
            "artifacts", "tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        string path = Path.Combine(folder, "N23E120.hgt");
        try
        {
            using (var stream = File.Create(path)) stream.SetLength(NasademTile.ByteLength);
            var wb = new Workbench(use3D: true);
            wb.ImportTerrain(path);
            var source = wb.G07PreviewSource!;
            using var cpu = await source.LoadG07DatasetAsync(path, 512);
            Assert.Equal(0d, cpu.SampleImportedHeight(23.5, 120.5));
            Assert.Null(cpu.SampleImportedHeight(23.5, 119.99));
            Assert.Equal(0d, cpu.SampleGridHeight(23.5, 119.99));
            var anchor = new EnuFrame(source.G07Origin);
            Assert.True(Pick(23.5, 120.5));
            Assert.False(Pick(23.5, 119.99));
            wb.SetSkyEnabled(true);
            wb.SetTerrainGridEnabled(false);
            var sky = Assert.Single(wb.G07EnvironmentDraws(source, cpu, 800, 600));
            Assert.Equal("editor/sky-gradient", sky.Mesh.AssetId);
            wb.SetSkyEnabled(false);
            Assert.Empty(wb.G07EnvironmentDraws(source, cpu, 800, 600));
            wb.SetTerrainGridEnabled(true);
            var grid = Assert.Single(wb.G07EnvironmentDraws(source, cpu, 800, 600));
            Assert.Equal("editor/geo-grid", grid.Mesh.AssetId);
            var vertices = grid.Mesh.Vertices.Span.ToArray();
            Assert.Contains(Enumerable.Range(0, vertices.Length / 7), i =>
            {
                int j = i * 7;
                var p = Wgs84.ToGeo(anchor.ToEcef(new EnuPosition(vertices[j], vertices[j+1], vertices[j+2])));
                return p.LongitudeDegrees < 120 || p.LongitudeDegrees > 121 ||
                    p.LatitudeDegrees < 23 || p.LatitudeDegrees > 24;
            });
            Assert.DoesNotContain(Enumerable.Range(0, vertices.Length / 7), i =>
            {
                int j = i * 7;
                var p = Wgs84.ToGeo(anchor.ToEcef(new EnuPosition(vertices[j], vertices[j+1], vertices[j+2])));
                return p.LongitudeDegrees > 120.05 && p.LongitudeDegrees < 120.95 &&
                    p.LatitudeDegrees > 23.05 && p.LatitudeDegrees < 23.95;
            });
            wb.SwitchTo(PageId.World);
            wb.SetWorldGridEnabled(false);
            Assert.Empty(wb.G07EnvironmentDraws(source, cpu, 800, 600));

            bool Pick(double latitude, double longitude)
            {
                var p = anchor.ToEnu(Wgs84.ToEcef(new GeoCoordinate(latitude, longitude, 0)));
                return cpu.TryPickSurface(new Vector3((float)p.East, (float)p.North,
                    (float)p.Up + 5000f), -Vector3.UnitZ, out _);
            }
        }
        finally { Directory.Delete(folder, recursive: true); }
    }
}