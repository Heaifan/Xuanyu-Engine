using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview;
using XuanYu.World.Ecs;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    public PreviewObject CreateLocalUnit(string path)
    {
        if (IsSimulating || IsGeoTerrainPreview)
            throw new InvalidOperationException("当前不是本地实体编辑态。");
        var camera = Scene.Camera;
        var target = camera?.TargetMeters ?? (0d, 0d, 0d);
        var (x, y, z) = target;
        float? ground = _terrainPreview is { } terrain && ReferenceEquals(terrain.Scene, Scene)
            ? terrain.SampleSurface(new Vector2((float)x, (float)y)) : null;
        double height = Math.Max(z, ground is float h ? h + 1d : z);
        var item = Scene.CreateSymbolEntity("单位", path,
            SpatialPosition.FromLocal(x, y, height));
        camera?.Focus(x, y, height);
        Log.Write($"[C03/二维] 创建本地单位：{item.Name}，图标={Scene.UnitIcons.NameFor(path)}，CPU缓存={Scene.SvgAssets.LoadedCount}");
        return item;
    }

    public GeoElevationSample CreateG07Unit(GeoUnifiedLodSession cpu,
        GeoCoordinate horizontal, string path, out PreviewObject? item)
    {
        if (IsSimulating) throw new InvalidOperationException("运行态不允许创建编辑实体。");
        var source = G07PreviewSource ?? throw new InvalidOperationException("尚未导入地理地形。");
        var sample = source.ResolveG07Ground(cpu, horizontal, 100d, out var position);
        item = null;
        if (sample.Status != GeoSampleStatus.Valid) return sample;
        item = Scene.CreateSymbolEntity("单位", path, position);
        source.Camera.FrameTop(position.ToGeo(), Math.Max(100d, source.Camera.DistanceMeters));
        Log.Write($"[C03/二维] 创建全球单位：{item.Name}，图标={Scene.UnitIcons.NameFor(path)}，离地100m，CPU缓存={Scene.SvgAssets.LoadedCount}");
        return sample;
    }
}