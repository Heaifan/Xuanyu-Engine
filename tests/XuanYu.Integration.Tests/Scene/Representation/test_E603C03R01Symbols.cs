using System.Numerics;
using System.Text;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview;
using XuanYu.Render;
using XuanYu.Runtime;
using XuanYu.World.Ecs;
using Xunit;

namespace XuanYu.World.Tests;

public sealed class test_E603C03R01Symbols
{
    private const string Svg="""
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 50">
          <rect width="100" height="50" fill="#80b0e0"/>
          <path d="M10 10 L90 40 M90 10 L10 40" fill="none" stroke="black" stroke-width="2"/>
        </svg>
        """;

    [Fact]
    public void CatalogRegistersLazilyAndSharesParsedMesh()
    {
        string path=Path.Combine(Path.GetTempPath(),$"{Guid.NewGuid():N}.svg");
        try
        {
            var catalog=new SvgAssetCatalog(); var entry=catalog.Register(path);
            Assert.Equal(0,catalog.LoadedCount);
            File.WriteAllText(path,Svg);
            var mesh=catalog.Resolve(entry.Id);
            Assert.Same(entry,catalog.Register(path));
            Assert.Same(mesh,catalog.Resolve(entry.Id));
            Assert.Equal(1,catalog.LoadedCount);
            Assert.True(mesh.TriangleCount>2);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void SceneBindsSameAssetToTwoEntitiesWithIndependentSizesAndPicking()
    {
        string path=Path.Combine(Path.GetTempPath(),$"{Guid.NewGuid():N}.svg");
        try
        {
            File.WriteAllText(path,Svg); var scene=new PreviewScene();
            var first=scene.CreateLogicEntity(); var second=scene.CreateLogicEntity(SpatialPosition.FromLocal(.7,0,0));
            scene.BindSymbol(first.Id,path); scene.BindSymbol(second.Id,path);
            scene.SetSymbolStyle(first.Id,new(40,true));
            var draws=scene.RenderWithProjection(Matrix4x4.Identity,200,100);
            Assert.Same(draws[0].Mesh,draws[1].Mesh);
            Assert.True(draws[0].IsBillboard);
            Assert.Equal(.4f,draws[0].ModelToClip.M11,5);
            Assert.Equal(.8f,draws[0].ModelToClip.M22,5);
            Assert.Equal(64f,scene.GetSymbolStyle(second.Id).Size);
            Assert.Equal(first.Id,PreviewMeshPicker.Pick(scene,100,50,200,100,Matrix4x4.Identity));
            scene.SetRepresentation(first.Id,EntityRepresentationKind.Point);
            Assert.Equal("builtin/point",scene.GetActiveMesh(first.Id)!.AssetId);
            scene.SetRepresentation(first.Id,EntityRepresentationKind.None);
            Assert.Null(scene.GetActiveMesh(first.Id));
            scene.SetRepresentation(first.Id,EntityRepresentationKind.Symbol);
            Assert.Same(draws[0].Mesh,scene.GetActiveMesh(first.Id));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void GlobalBillboardRemainsFrontFacingAtDifferentAzimuths()
    {
        string path=Path.Combine(Path.GetTempPath(),$"{Guid.NewGuid():N}.svg");
        try
        {
            File.WriteAllText(path,Svg); var scene=new PreviewScene();
            var anchor=new EnuFrame(new GeoCoordinate(23,120,10));
            var item=scene.CreateLogicEntity(SpatialPosition.FromEcef(anchor.OriginEcef));
            scene.BindSymbol(item.Id,path);
            foreach (var eye in new EnuPosition[]{new(0,-20,10),new(20,0,10),new(0,20,10),new(-20,0,10)})
            {
                var frame=new GeoRenderFrame(anchor,anchor.ToEcef(eye),anchor.OriginEcef,2,.1f,1000);
                var draw=Assert.Single(scene.RenderGeographic(frame,200,100));
                var m=draw.ModelToClip;
                Assert.Equal(.64f,m.M11/m.M44,5);
                Assert.Equal(1.28f,m.M22/m.M44,5);
                Assert.Equal(0f,m.M12); Assert.Equal(0f,m.M21);
            }
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void UnsupportedSvgFailsInsteadOfSilentlyChangingAppearance()
    {
        using var stream=new MemoryStream(Encoding.UTF8.GetBytes(Svg.Replace("<rect ","<rect clip-path=\"url(#clip)\" ")));
        Assert.Throws<InvalidDataException>(()=>SvgMeshReader.Read("test/unsupported",stream));
    }
}