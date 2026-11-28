using System;
using System.IO;
using Xunit;
using XuanYu.Editor.Pages;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.ViewportReference;

public sealed class test_E5UX01R02GeoViewport
{
    [Fact]
    public void ImportedDem_FivePagesShareCameraProjectionAndSurface()
    {
        string folder = Path.Combine(Environment.CurrentDirectory,
            "artifacts", "tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        string path = Path.Combine(folder, "N24E121.hgt");
        try
        {
            using (var stream = File.Create(path))
                stream.SetLength(NasademTile.ByteLength);
            var wb = new Workbench(use3D: true);
            var localMesh = wb.Scene.Objects[0].Mesh;
            wb.ImportTerrain(path);
            wb.SetSkyEnabled(false);
            wb.SetWorldGridEnabled(false);
            wb.SetTerrainGridEnabled(false);
            var geo = wb.GeoTerrainCamera!;
            Assert.NotNull(geo);
            Assert.NotEmpty(wb.ViewportDraws(800, 600));
            foreach (var page in Enum.GetValues<PageId>())
            {
                var target = geo.Target;
                var eye = geo.PositionEcef;
                wb.SwitchTo(page);
                Assert.Same(geo, wb.GeoTerrainCamera);
                Assert.Null(wb.ActiveCamera);
                Assert.Equal(target, geo.Target);
                Assert.Equal(eye, geo.PositionEcef);
                var draws = wb.ViewportDraws(800, 600);
                Assert.NotEmpty(draws);
                Assert.DoesNotContain(draws, d => ReferenceEquals(d.Mesh, localMesh));
                var projection = wb.EditorProjection(800, 600);
                Assert.Contains(draws, d => d.ModelToClip == projection);
            }
            wb.SwitchTo(PageId.Project);
            var yaw = geo.YawRadians;
            wb.NavigateGeoTerrain(20f, 0f, 0f, false, 600);
            Assert.NotEqual(yaw, geo.YawRadians);
            wb.SwitchTo(PageId.Terrain);
            Assert.Same(geo, wb.GeoTerrainCamera);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }
}