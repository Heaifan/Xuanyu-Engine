using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview;
using XuanYu.Render;
using XuanYu.Runtime;
using Xunit;

namespace XuanYu.World.Tests;

public sealed class test_E603C03R02UnitIcons
{
    [Fact]
    public void PresetsAreRealSvgSourcesAndSearchDoesNotParse()
    {
        var catalog = new SvgAssetCatalog();
        var icons = new UnitIconLibrary(catalog, Root());
        var land = icons.Search("陆军", "");
        Assert.Equal(9, land.Count);
        Assert.Equal(0, catalog.LoadedCount);
        Assert.Empty(icons.Search("海军", "坦克"));
        Assert.Equal("坦克营", Assert.Single(icons.Search("陆军", "tank")).Name);
        foreach (var entry in land)
            Assert.True(catalog.Resolve(catalog.Register(entry.SourcePath).Id).TriangleCount > 0);
        Assert.Equal(9, catalog.LoadedCount);
    }

    [Fact]
    public void ReplacingIconPreservesIdentityOrganizationAndStyleAndUsesRecent()
    {
        var wb = new Workbench(use3D: true);
        var scene = wb.Scene;
        var icons = new UnitIconLibrary(scene.SvgAssets, Root());
        var first = wb.CreateLocalUnit(icons.RecentPath);
        var parent = scene.CreateLogicEntity();
        scene.SetOrganizationParent(first.Id, parent.Id);
        scene.Select(first.Id);
        scene.SetSymbolStyle(first.Id, new(80, false));
        var before = scene.World.Find(first.Id);
        var tank = Assert.Single(icons.Search("陆军", "tank"));
        scene.BindSymbol(first.Id, tank.SourcePath);
        Assert.Equal(before, scene.World.Find(first.Id));
        Assert.Equal(parent.Id, scene.World.GetOrganizationParent(first.Id));
        Assert.Equal(new BillboardStyle(80, false), scene.GetSymbolStyle(first.Id));
        Assert.Equal(tank.SourcePath, scene.UnitIcons.RecentPath);
        var second = wb.CreateLocalUnit(scene.UnitIcons.RecentPath);
        Assert.Same(scene.GetActiveMesh(first.Id), scene.GetActiveMesh(second.Id));
        Assert.Equal(2, scene.SvgAssets.LoadedCount);
        scene.ClearSymbol(first.Id);
        Assert.Null(scene.GetSymbolAsset(first.Id));
        Assert.Equal(EntityRepresentationKind.None, scene.GetRepresentation(first.Id));
        Assert.NotNull(scene.GetActiveMesh(second.Id));
    }

    [Fact]
    public void CustomIconsAppearAfterSuccessfulImport()
    {
        var scene = new PreviewScene();
        var item = scene.CreateLogicEntity();
        string path = Path.Combine(Path.GetTempPath(), $"custom-{Guid.NewGuid():N}.svg");
        try
        {
            File.WriteAllText(path, "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 10 10\"><rect width=\"10\" height=\"10\"/></svg>");
            scene.BindSymbol(item.Id, path);
            Assert.Equal(path, Assert.Single(scene.UnitIcons.Search("自定义", "custom-")).SourcePath);
        }
        finally { File.Delete(path); }
    }

    private static string Root()
    {
        DirectoryInfo? folder = new(AppContext.BaseDirectory);
        while (folder is not null && !File.Exists(Path.Combine(folder.FullName, "XuanYuEngine.sln"))) folder = folder.Parent;
        return folder?.FullName ?? throw new DirectoryNotFoundException("仓库根目录未找到");
    }
}