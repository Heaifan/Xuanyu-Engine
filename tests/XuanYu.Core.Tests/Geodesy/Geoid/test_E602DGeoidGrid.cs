using XuanYu.Core.Geodesy;
using Xunit;

namespace XuanYu.Core.Tests.Geodesy;

public sealed class test_E602DGeoidGrid
{
    [Fact]
    public void RealGrid_MatchesIndependentReferenceAtNodesAndInterior()
    {
        var grid = Load();
        Assert.Equal(17.161579132080078, grid.UndulationMeters(new(0, 0, 0)), 5);
        Assert.Equal(21.251991271973314, grid.UndulationMeters(new(23.5, 120.5, 0)), 5);
        Assert.Equal(22.139996185303012, grid.UndulationMeters(new(23.55, 120.6, 0)), 5);
        Assert.Equal(grid.UndulationMeters(new(23.55, 120.6, 0)),
            grid.UndulationMeters(new(23.55, 120.6, 8000)));
    }

    [Fact]
    public void RealGrid_WrapsLongitudeAndIncludesBothPoles()
    {
        var grid = Load();
        Assert.Equal(21.264589309692383, grid.UndulationMeters(new(0, 179.875, 0)), 5);
        Assert.Equal(21.05011940002442, grid.UndulationMeters(new(0, -179.875, 0)), 5);
        Assert.Equal(grid.UndulationMeters(new(0, 180, 0)),
            grid.UndulationMeters(new(0, -180, 0)));
        Assert.Equal(13.606245040893555, grid.UndulationMeters(new(90, 0, 0)), 5);
        Assert.Equal(-29.533849716186523, grid.UndulationMeters(new(-90, 0, 0)), 5);
    }

    [Fact]
    public void InvalidGrid_IsRejectedAtLoad()
    {
        string folder = Path.Combine(Environment.CurrentDirectory, "artifacts", "tests");
        Directory.CreateDirectory(folder);
        string path = Path.Combine(folder, Guid.NewGuid().ToString("N") + ".gtx");
        try
        {
            File.WriteAllBytes(path, new byte[40]);
            Assert.Throws<InvalidDataException>(() => Egm96GeoidGrid.Load(path));
        }
        finally { File.Delete(path); }
    }

    private static Egm96GeoidGrid Load() => Egm96GeoidGrid.Load(
        Environment.GetEnvironmentVariable("XYE_EGM96_GRID") ??
        throw new InvalidOperationException("请设置 XYE_EGM96_GRID。"));
}