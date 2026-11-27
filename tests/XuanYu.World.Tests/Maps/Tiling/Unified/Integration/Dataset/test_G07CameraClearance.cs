using XuanYu.Core.Geodesy;
using XuanYu.Editor.Pages;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07CameraClearance
{
    [Fact]
    public async Task CommittedCoarseSurface_RestoresEyeAboveRawValley()
    {
        string folder = Path.Combine(Environment.CurrentDirectory,
            "artifacts", "tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        string path = Path.Combine(folder, "N23E120.hgt");
        try
        {
            var bytes = new byte[NasademTile.ByteLength];
            for (int i = 0; i < bytes.Length; i += 2)
            {
                bytes[i] = 3; bytes[i + 1] = 232;
            }
            // 3×3 原始低谷不落在 L6 顶点上，已提交粗三角形跨过低谷。
            for (int y = 1799; y <= 1801; y++)
                for (int x = 1799; x <= 1801; x++)
                {
                    int offset = (y * 3601 + x) * 2;
                    bytes[offset] = bytes[offset + 1] = 0;
                }
            File.WriteAllBytes(path, bytes);
            var wb = new Workbench(use3D: true);
            wb.ImportTerrain(path);
            var source = wb.G07PreviewSource!;
            using var cpu = await source.LoadG07DatasetAsync(path, 512);
            Assert.Equal(0d, cpu.SampleImportedHeight(23.5, 120.5));
            source.Camera.SetTerrainCollision(p => cpu.SampleImportedHeight(
                p.LatitudeDegrees, p.LongitudeDegrees));
            source.Camera.FrameTop(new GeoCoordinate(23.5, 120.5, 0d), 4d);
            Assert.InRange(Wgs84.ToGeo(source.Camera.PositionEcef).HeightMeters, 3d, 5d);
            Assert.True(source.EnsureG07CameraClearance(cpu));
            var eye = source.G07Eye;
            Assert.True(cpu.TryPickSurface(new System.Numerics.Vector3((float)eye.East,
                (float)eye.North, 32768f), -System.Numerics.Vector3.UnitZ, out var hit));
            Assert.InRange(hit.Up, 990d, 1001d);
            Assert.True(eye.Up >= hit.Up + 3d);
            Assert.False(source.EnsureG07CameraClearance(cpu));
        }
        finally { Directory.Delete(folder, recursive: true); }
    }
}