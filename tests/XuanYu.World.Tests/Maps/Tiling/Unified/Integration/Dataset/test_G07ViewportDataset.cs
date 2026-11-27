using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07ViewportDataset
{
    [G07DataFact]
    public async Task FullCatalogInitialCoverageAndBlockedRefinement()
    {
        var catalog = new NasademTileCatalog([Environment.GetEnvironmentVariable("XYE_G07_WEST")!,
            Environment.GetEnvironmentVariable("XYE_G07_EAST")!]);
        var anchor = new EnuFrame(new GeoCoordinate(23.5, 121, 0));
        using var cpu = await GeoUnifiedLodSession.CreateDatasetAsync(catalog, anchor, 512);
        Assert.NotNull(cpu.Domain);
        Assert.NotNull(cpu.PreparationProfile);
        Assert.Equal(cpu.Domain!.Roots.Count, cpu.Coverage.Leaves.Count);
        Assert.True(cpu.Coverage.Leaves.Count > 1);
        foreach (var root in cpu.Coverage.Leaves)
        {
            using var lease = cpu.AcquirePage(root, cpu.Coverage.EdgeMask(root));
            Assert.NotNull(lease);
            for (int y = 0; y < 33; y++)
                for (int x = 0; x < 33; x++) Assert.Equal(GeoSampleStatus.Valid, lease!.Page.StatusAt(x, y));
        }
        var initial = cpu.Coverage;
        var outside = cpu.CreateViewportVisibility(Matrix4x4.CreateScale(0.000001f) *
            Matrix4x4.CreateTranslation(10f, 0f, 0.5f));
        Assert.All(initial.Leaves, tile => Assert.False(outside(tile)));
        var inside = cpu.CreateViewportVisibility(Matrix4x4.CreateScale(0.000001f) *
            Matrix4x4.CreateTranslation(0f, 0f, 0.5f));
        Assert.All(initial.Leaves, tile => Assert.True(inside(tile)));
        var hidden = cpu.Step(new EnuPosition(0, 0, 1000), 1080, Math.PI / 3,
            (_, _) => true, outside);
        Assert.Same(initial, hidden.Coverage);
        Assert.Null(hidden.PendingCoverage);
        var blocked = cpu.Step(new EnuPosition(0, 0, 1000), 1080, Math.PI / 3, (_, _) => false);
        Assert.Same(initial, blocked.Coverage);
    }
}