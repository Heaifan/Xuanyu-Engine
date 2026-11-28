using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Pages;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_E602GR03WheelClearance
{
    [Fact]
    public async Task SideWheel_KeepsTargetAndStopsAtGround()
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
        source.Camera.FrameTop(new GeoCoordinate(23.5, 120.5, geoid.UndulationMeters(point)), 1000d);
        source.Camera.SnapAxis(Vector3.UnitX);
        source.EnsureG07CameraClearance(cpu);
        var target = source.Camera.Target;
        source.ZoomAtCursor(1f, new Vector2(530, 430), 800, 600);
        source.EnsureG07CameraClearance(cpu);
        Assert.Equal(target, source.Camera.Target);
        Assert.InRange(source.Camera.DistanceMeters, 849.99d, 850.01d);
        source.ZoomAtCursor(100f, new Vector2(530, 430), 800, 600);
        source.EnsureG07CameraClearance(cpu);
        double stopped = source.Camera.DistanceMeters;
        Assert.InRange(stopped, 250d, 400d);
        source.ZoomAtCursor(100f, new Vector2(530, 430), 800, 600);
        source.EnsureG07CameraClearance(cpu);
        Assert.Equal(target, source.Camera.Target);
        Assert.True(source.Camera.DistanceMeters <= stopped);
        var eye = Wgs84.ToGeo(source.Camera.PositionEcef);
        Assert.True(eye.HeightMeters >= geoid.UndulationMeters(eye) + 2.99d);
        source.ZoomAtCursor(-1f, new Vector2(530, 430), 800, 600);
        source.EnsureG07CameraClearance(cpu);
        Assert.True(source.Camera.DistanceMeters > stopped);
    }

    [Fact]
    public async Task CoarseValley_InwardCorrectionStaysWithinPreviousSafeDistance()
    {
        string folder = Path.Combine(Environment.CurrentDirectory, "artifacts", "tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        string path = Path.Combine(folder, "N23E120.hgt");
        try
        {
            var bytes = new byte[NasademTile.ByteLength];
            for (int i = 0; i < bytes.Length; i += 2) { bytes[i] = 3; bytes[i + 1] = 232; }
            for (int y = 1798; y <= 1802; y++)
                for (int x = 1798; x <= 1802; x++)
                {
                    int offset = (y * 3601 + x) * 2;
                    bytes[offset] = bytes[offset + 1] = 0;
                }
            File.WriteAllBytes(path, bytes);
            var wb = new Workbench(use3D: true);
            wb.ImportTerrain(path);
            var source = wb.G07PreviewSource!;
            var geoid = Egm96GeoidGrid.Load(Environment.GetEnvironmentVariable("XYE_EGM96_GRID")!);
            source.ConfigureG07Geoid(geoid);
            using var cpu = await source.LoadG07DatasetAsync(path, 512);
            var point = new GeoCoordinate(23.5, 120.5, 0d);
            double n = geoid.UndulationMeters(point);
            source.Camera.FrameTop(new GeoCoordinate(23.5, 120.5, n), 1100d);
            Assert.False(source.EnsureG07CameraClearance(cpu));
            source.Camera.Zoom(1d);
            Assert.InRange(source.Camera.DistanceMeters, 934.99d, 935.01d);
            Assert.True(source.EnsureG07CameraClearance(cpu));
            Assert.InRange(source.Camera.DistanceMeters, 990d, 1030d);
            var eye = Wgs84.ToGeo(source.Camera.PositionEcef);
            Assert.True(eye.HeightMeters >= n + 990d);
            Assert.False(source.EnsureG07CameraClearance(cpu));
        }
        finally { Directory.Delete(folder, true); }
    }
}