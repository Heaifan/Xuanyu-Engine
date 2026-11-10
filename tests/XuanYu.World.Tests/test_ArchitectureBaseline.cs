using System.Reflection;
using Xunit;
using XuanYu.Core;
using XuanYu.World;

namespace XuanYu.World.Tests;

public sealed class test_ArchitectureBaseline
{
    [Fact]
    public void Core_ShouldNotReferenceAnyEngineAssembly()
    {
        Assembly coreAssembly = typeof(CoreAssemblyMarker).Assembly;
        string[] engineRefs = coreAssembly
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .Where(name => name.StartsWith("XuanYu.", StringComparison.Ordinal))
            .ToArray();

        Assert.Empty(engineRefs);
    }

    [Fact]
    public void World_ShouldNotReferenceRenderPlatformRuntimeOrEditor()
    {
        Assembly worldAssembly = typeof(WorldAssemblyMarker).Assembly;
        string[] forbiddenRefs = worldAssembly
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .Where(name =>
                name is "XuanYu.Render"
                     or "XuanYu.Platform"
                     or "XuanYu.Runtime"
                     or "XuanYu.Editor")
            .ToArray();

        Assert.Empty(forbiddenRefs);
    }
}
