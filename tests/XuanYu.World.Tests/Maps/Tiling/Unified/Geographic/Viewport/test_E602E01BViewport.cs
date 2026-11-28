using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Pages;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_E602E01BViewport
{
    [Fact]
    public async Task ConfiguredSourceUsesCorrectedPageAndCameraGround()
    {
        using var fixture = new E602CHgtFixture();
        string path = fixture.Create("N23E120");
        var wb = new Workbench(use3D: true);
        wb.ImportTerrain(path);
        var source = wb.G07PreviewSource!;
        var geoid = Egm96GeoidGrid.Load(Environment.GetEnvironmentVariable("XYE_EGM96_GRID")!);
        source.ConfigureG07Geoid(geoid);
        using var cpu = await source.LoadG07DatasetAsync(path, 512);
        var point = new GeoCoordinate(23.5, 120.5, 0d);
        double n = geoid.UndulationMeters(point);
        source.Camera.FrameTop(new GeoCoordinate(23.5, 120.5, n), 4d);
        Assert.InRange(Wgs84.ToGeo(source.Camera.PositionEcef).HeightMeters, n + 3d, n + 5d);
        source.Camera.Zoom(100);
        source.EnsureG07CameraClearance(cpu);
        Assert.True(Wgs84.ToGeo(source.Camera.PositionEcef).HeightMeters >= n + 2.99d);
        Assert.True(cpu.TryPickSurfaceHit(new Vector3(0, 0, 1000), -Vector3.UnitZ, out var hit));
        Assert.InRange((double)hit.Position.Z, n - 0.5d, n + 0.5d);
        Assert.Equal(0f, hit.Elevation);
        Assert.Equal(0d, cpu.SampleElevation(point).HeightMeters);
    }

    [Fact]
    public async Task CoarseValleyCoverageRestoresEyeAboveCorrectedVisibleSurface()
    {
        string folder = Path.Combine(Environment.CurrentDirectory, "artifacts", "tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        string path = Path.Combine(folder, "N23E120.hgt");
        try
        {
            var bytes = new byte[NasademTile.ByteLength];
            for (int i = 0; i < bytes.Length; i += 2) { bytes[i] = 3; bytes[i + 1] = 232; }
            for (int y = 1799; y <= 1801; y++)
                for (int x = 1799; x <= 1801; x++)
                {
                    int index = (y * 3601 + x) * 2;
                    bytes[index] = bytes[index + 1] = 0;
                }
            File.WriteAllBytes(path, bytes);
            var wb = new Workbench(use3D: true);
            wb.ImportTerrain(path);
            var source = wb.G07PreviewSource!;
            var geoid = Egm96GeoidGrid.Load(Environment.GetEnvironmentVariable("XYE_EGM96_GRID")!);
            source.ConfigureG07Geoid(geoid);
            using var cpu = await source.LoadG07DatasetAsync(path, 512);
            double n = geoid.UndulationMeters(new(23.5, 120.5, 0));
            source.Camera.FrameTop(new GeoCoordinate(23.5, 120.5, n), 4d);
            Assert.InRange(Wgs84.ToGeo(source.Camera.PositionEcef).HeightMeters, n + 3d, n + 5d);
            Assert.True(source.EnsureG07CameraClearance(cpu));
            var eye = source.G07Eye;
            Assert.True(cpu.TryPickSurface(new Vector3((float)eye.East, (float)eye.North, 32768f),
                -Vector3.UnitZ, out var hit));
            Assert.InRange(hit.Up, n + 990d, n + 1001d);
            Assert.True(eye.Up >= hit.Up + 2.99d);
            Assert.False(source.EnsureG07CameraClearance(cpu));
        }
        finally { Directory.Delete(folder, true); }
    }
}