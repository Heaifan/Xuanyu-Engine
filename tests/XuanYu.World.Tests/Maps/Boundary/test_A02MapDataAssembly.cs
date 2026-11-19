using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps.Boundary;

public sealed class test_A02MapDataAssembly
{
    [Fact]
    public void MapDataContracts_ShouldBelongToMapAssembly()
    {
        Type[] contracts =
        [
            typeof(MapDocument),
            typeof(TerrainHeightField),
            typeof(TerrainWaterMask),
            typeof(MapGeoReference)
        ];

        foreach (Type contract in contracts)
            Assert.Equal("XuanYu.Map", contract.Assembly.GetName().Name);
    }

    [Fact]
    public void MapAssembly_ShouldNotReferenceWorldOrEditor()
    {
        string[] forbidden =
        [
            "XuanYu.World", "XuanYu.Editor", "XuanYu.Editor.Core",
            "XuanYu.Editor.UI", "XuanYu.Render", "XuanYu.Render.Vulkan"
        ];
        var references = typeof(MapDocument).Assembly.GetReferencedAssemblies();

        foreach (var reference in references)
            Assert.DoesNotContain(reference.Name ?? string.Empty, forbidden);
    }
}