
using System.Reflection;
using System.Xml.Linq;
using Xunit;
using XuanYu.Core;
using XuanYu.World;

namespace XuanYu.World.Tests;

public sealed class test_ArchitectureBaseline
{
    [Fact]
    public void Core_ShouldNotReferenceAnyEngineAssembly()
    {
        Assembly assembly = typeof(CoreAssemblyMarker).Assembly;
        string[] references = assembly.GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .Where(name => name.StartsWith("XuanYu.", StringComparison.Ordinal))
            .ToArray();

        Assert.Empty(references);
    }

    [Fact]
    public void World_ShouldNotReferenceRenderPlatformRuntimeOrEditor()
    {
        Assembly assembly = typeof(WorldAssemblyMarker).Assembly;
        string[] references = assembly.GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .Where(name => name is "XuanYu.Render"
                or "XuanYu.Platform"
                or "XuanYu.Runtime"
                or "XuanYu.Editor")
            .ToArray();

        Assert.Empty(references);
    }

    [Theory]
    [InlineData("XuanYu.Core", "")]
    [InlineData("XuanYu.Platform", "XuanYu.Core")]
    [InlineData("XuanYu.World", "XuanYu.Core")]
    [InlineData("XuanYu.Render", "XuanYu.Core;XuanYu.World;XuanYu.Platform")]
    [InlineData("XuanYu.Runtime", "XuanYu.Core;XuanYu.World;XuanYu.Platform;XuanYu.Render")]
    [InlineData("XuanYu.Editor", "XuanYu.Core;XuanYu.World;XuanYu.Platform;XuanYu.Render;XuanYu.Runtime")]
    [InlineData("XuanYu.World.Tests", "XuanYu.Core;XuanYu.World;XuanYu.Editor")]
    public void Projects_ShouldKeepApprovedReferences(string project, string allowed)
    {
        string group = project.EndsWith(".Tests", StringComparison.Ordinal)
            ? "tests" : "src";
        string file = Path.Combine(FindRoot(), group, project, $"{project}.csproj");

        string[] actual = XDocument.Load(file).Descendants("ProjectReference")
            .Select(element => Path.GetFileNameWithoutExtension(
                element.Attribute("Include")!.Value.Replace('\\', '/')))
            .Order(StringComparer.Ordinal)
            .ToArray();
        string[] expected = allowed.Split(
                ';', StringSplitOptions.RemoveEmptyEntries)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(expected, actual);
    }

    private static string FindRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "XuanYuEngine.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("找不到 XuanYuEngine.sln。");
    }
}
