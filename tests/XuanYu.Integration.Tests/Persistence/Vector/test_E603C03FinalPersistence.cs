using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview;
using XuanYu.Editor.Preview.Persistence;
using XuanYu.Runtime;
using XuanYu.World.Ecs;
using Xunit;

namespace XuanYu.World.Tests;

public sealed class test_E603C03FinalPersistence
{
    [Fact]
    public void LocalAndGlobalSymbolsRestoreSharedResourceAndInactiveBinding()
    {
        string folder = Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            string svg = Path.Combine(folder,"unit.svg"), file = Path.Combine(folder,"test.xyscene");
            File.WriteAllText(svg,"""
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 10 10"><rect width="10" height="10"/></svg>
                """);
            var scene = new PreviewScene();
            var a = scene.CreateSymbolEntity("Unit",svg);
            var b = scene.CreateSymbolEntity("Unit",svg,SpatialPosition.FromGeo(new GeoCoordinate(23,121,40)));
            scene.SetSymbolStyle(a.Id,new(100,false));
            scene.SetRepresentation(b.Id,EntityRepresentationKind.None);
            scene.SetOrganizationParent(b.Id,a.Id);
            SceneFile.Save(scene,file);
            Assert.Contains("unit.svg",File.ReadAllText(file));
            var target = new PreviewScene(); SceneFile.Load(target,file);
            Assert.Equal(scene.World.Entities.ToArray(),target.World.Entities.ToArray());
            Assert.Equal(a.Id,target.World.GetOrganizationParent(b.Id));
            Assert.Equal(new BillboardStyle(100,false),target.GetSymbolStyle(a.Id));
            Assert.Equal(EntityRepresentationKind.None,target.GetRepresentation(b.Id));
            target.SetRepresentation(b.Id,EntityRepresentationKind.Symbol);
            Assert.Same(target.GetActiveMesh(a.Id),target.GetActiveMesh(b.Id));
            Assert.Equal(1,target.SvgAssets.LoadedCount);
            File.Delete(svg);
            var before = target.World.Entities.ToArray();
            Assert.Throws<InvalidDataException>(()=>SceneFile.Load(target,file));
            Assert.Equal(before,target.World.Entities.ToArray());
        }
        finally { Directory.Delete(folder,true); }
    }
}