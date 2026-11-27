using System.Buffers.Binary;
using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07ContourLabelCache
{
    [Theory]
    [InlineData(1800, 420f, 0f, 1f, true)]
    [InlineData(1800, 450f, 0f, 1f, false)]
    [InlineData(1900, 100f, 0f, 1f, false)]
    [InlineData(1900, 50f, 10f, 1f, true)]
    [InlineData(1800, 840f, 0f, 2f, true)]
    public void LocalLabelConflicts(int elevation, float x, float y, float scale, bool expected)
    {
        var a = new GeoContourLabel(0, 0, 1800, 0, 0, 0, 0, 0, 0, 0, 0);
        var b = a with { X = x, Y = y, ElevationMeters = elevation };
        Assert.Equal(expected, GeoTerrainRenderSession.G07LabelsConflict(a, b, scale));
    }

    [Fact]
    public async Task LabelsRemainVisibleDuringCameraAndCoverageChanges()
    {
        string folder = Path.Combine(Environment.CurrentDirectory,
            "artifacts", "tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        string path = Path.Combine(folder, "N23E120.hgt");
        try
        {
            WriteSlope(path);
            var wb = new Workbench(use3D: true);
            wb.ImportTerrain(path);
            var source = wb.G07PreviewSource!;
            using var cpu = await source.LoadG07DatasetAsync(path, 512);
            var anchor = new EnuFrame(source.G07Origin);
            var center = anchor.ToEnu(Wgs84.ToEcef(new GeoCoordinate(23.5, 120.5, 0)));
            var mvp = Matrix4x4.CreateTranslation(-(float)center.East, -(float)center.North,
                -(float)center.Up) * Matrix4x4.CreateScale(1f / 20000, 1f / 15000, -1f / 20000) *
                Matrix4x4.CreateTranslation(0, 0, 0.5f);
            var cache = new G07ContourLabelCache();
            var first = cache.Update(source, cpu, mvp, 800, 600, 20, 1, 0);
            Assert.NotEmpty(first);
            Assert.Same(first, cache.Update(source, cpu, mvp, 800, 600, 20, 1, 1));
            var moved = mvp * Matrix4x4.CreateTranslation(0.005f, 0, 0);
            var second = cache.Update(source, cpu, moved, 800, 600, 20, 1, 16);
            Assert.NotEmpty(second);
            Assert.Contains(second, label => first.Any(old => old.ElevationMeters == label.ElevationMeters &&
                MathF.Abs(label.X - old.X) < 16 && MathF.Abs(label.Y - old.Y) < 16));
            await Refine(cpu, new EnuPosition(center.East, center.North, center.Up + 3600));
            var refined = cache.Update(source, cpu, moved, 800, 600, 20, 1, 32);
            Assert.NotEmpty(refined);
            var refreshed = cache.Update(source, cpu, moved, 800, 600, 20, 1, 220);
            Assert.NotEmpty(refreshed);
            foreach (var labels in new[] { first, second, refined, refreshed })
                for (int i = 0; i < labels.Length; i++)
                    for (int j = i + 1; j < labels.Length; j++)
                        Assert.False(GeoTerrainRenderSession.G07LabelsConflict(labels[i], labels[j], 1f));
            Assert.Same(refreshed, cache.Update(source, cpu, moved, 800, 600, 20, 1, 221));
            var outside = moved * Matrix4x4.CreateTranslation(10, 0, 0);
            Assert.Empty(cache.Update(source, cpu, outside, 800, 600, 20, 1, 222));
            Assert.Empty(cache.Update(source, cpu, moved, 800, 600, 0, 1, 223));
            Assert.NotEmpty(cache.Update(source, cpu, moved, 800, 600, 20, 1, 224));
            cache.Clear();
            Assert.NotEmpty(cache.Update(source, cpu, moved, 800, 600, 20, 1, 225));
        }
        finally { Directory.Delete(folder, recursive: true); }
    }

    private static void WriteSlope(string path)
    {
        using var stream = File.Create(path);
        var row = new byte[3601 * 2];
        for (int y = 0; y < 3601; y++)
        {
            short height = (short)((3600 - y) * 2);
            for (int x = 0; x < 3601; x++)
                BinaryPrimitives.WriteInt16BigEndian(row.AsSpan(x * 2, 2), height);
            stream.Write(row);
        }
    }

    private static async Task Refine(GeoUnifiedLodSession cpu, EnuPosition eye)
    {
        var initial = cpu.Coverage;
        for (int i = 0; i < 1500; i++)
        {
            cpu.Step(eye, 1080, Math.PI / 3, (_, _) => true);
            if (!ReferenceEquals(initial, cpu.Coverage)) return;
            await Task.Delay(10);
        }
        Assert.Fail($"未观察到真实 CPU 覆盖细化：{cpu.Diagnostic}");
    }
}