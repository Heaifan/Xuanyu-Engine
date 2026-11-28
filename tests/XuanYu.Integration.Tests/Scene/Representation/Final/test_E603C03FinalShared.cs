using System.Numerics;
using XuanYu.Editor.Pages;
using XuanYu.Runtime;
using Xunit;

namespace XuanYu.World.Tests;

public sealed class test_E603C03FinalShared
{
    [Fact]
    public void ThousandUnitsShareGeometryAndRuntimeCopyPreservesRepresentation()
    {
        string path = Path.Combine(Path.GetTempPath(),$"{Guid.NewGuid():N}.svg");
        try
        {
            File.WriteAllText(path,"""
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 10 10"><rect width="10" height="10"/></svg>
                """);
            var wb = new Workbench(use3D: true);
            var scene = wb.Scene;
            var first = scene.CreateSymbolEntity("Unit",path);
            for (int i=1; i<1000; i++) scene.CreateSymbolEntity($"Unit{i}",path);
            scene.SetSymbolStyle(first.Id,new(80,true));
            var draws = scene.RenderWithProjection(Matrix4x4.Identity,800,600).Where(d=>d.IsBillboard).ToArray();
            Assert.Equal(1000,draws.Length);
            Assert.All(draws,d=>Assert.Same(draws[0].Mesh,d.Mesh));
            Assert.Equal(1,scene.SvgAssets.LoadedCount);
            wb.StartSimulation();
            Assert.Equal(1000,wb.RenderDraws.Count(d=>d.IsBillboard));
            wb.StopSimulation();
            Assert.Equal(new BillboardStyle(80,true),scene.GetSymbolStyle(first.Id));
        }
        finally { File.Delete(path); }
    }
}