using Xunit;
using XuanYu.World;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps.Migration;

public sealed class test_A02FullMapMigration
{
    [Fact]
    public void MapTypes_ShouldBelongToMapAssembly()
    {
        var map = typeof(GeoTileId).Assembly;
        Assert.Equal("XuanYu.Map", map.GetName().Name);
        Assert.Same(map, typeof(MapDocument).Assembly);
        Assert.Same(map, typeof(NasademTile).Assembly);
        Assert.Same(map, typeof(NasademElevationSource).Assembly);
        Assert.Same(map, typeof(GeoTerrainViewSession).Assembly);
    }

    [Fact]
    public void WorldAssembly_ShouldNotOwnMapTypes()
    {
        var world = typeof(WorldAssemblyMarker).Assembly;
        Assert.Null(world.GetType("XuanYu.World.Maps.MapDocument"));
        Assert.Null(world.GetType("XuanYu.World.Maps.GeoTileId"));
    }

    [Fact]
    public void MapAssembly_ShouldOnlyDependOnCore()
    {
        string[] refs = typeof(MapDocument).Assembly.GetReferencedAssemblies()
            .Select(a => a.Name ?? "")
            .Where(name => name.StartsWith("XuanYu.", StringComparison.Ordinal))
            .Order(StringComparer.Ordinal).ToArray();
        Assert.Equal(new[] { "XuanYu.Core" }, refs);
    }
}
