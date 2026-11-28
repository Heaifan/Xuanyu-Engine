using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    public GeoElevationSample CreateG07Cube(GeoUnifiedLodSession cpu,
        GeoCoordinate horizontal, double offsetMeters, out PreviewObject? cube)
    {
        if (IsSimulating) throw new InvalidOperationException("运行态不允许创建编辑实体。");
        var source = G07PreviewSource ?? throw new InvalidOperationException("尚未导入地理地形。");
        var sample = source.ResolveG07Ground(cpu, horizontal, offsetMeters, out var position);
        cube = sample.Status == GeoSampleStatus.Valid ? Scene.CreateGeographicCube(position) : null;
        return sample;
    }
}