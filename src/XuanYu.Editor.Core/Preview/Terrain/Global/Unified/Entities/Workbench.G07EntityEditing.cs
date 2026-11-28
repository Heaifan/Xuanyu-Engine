using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview;
using XuanYu.World;
using XuanYu.World.Ecs;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    public GeoElevationSample CreateG07InspectionCube(GeoUnifiedLodSession cpu,
        out PreviewObject? cube)
    {
        var source = G07PreviewSource ?? throw new InvalidOperationException("尚未导入地理地形。");
        const double edge = 200d;
        var sample = CreateG07Cube(cpu, source.Camera.Target, edge / 2d, out cube);
        if (cube is null) return sample;
        // builtin/cube 的原始边长为 1.4m，验收对象的缩放是真实空间尺寸。
        var transform = World.Find(cube.Id)!.Transform;
        Scene.SetTransform(cube.Id, transform with { Scale = new Vector3((float)(edge / 1.4d)) }, recordHistory: false);
        Scene.Select(cube.Id);
        var geo = transform.Spatial.ToGeo();
        Log.Write($"[E6/全球实体] 创建 {cube.Name} | 纬度={geo.LatitudeDegrees:F6} 经度={geo.LongitudeDegrees:F6} | 边长=200m 原点离地=100m");
        FocusG07Entity();
        return sample;
    }

    public GeoSampleStatus MoveG07Entity(GeoUnifiedLodSession cpu, Guid id,
        GeoCoordinate target, bool aboveGround)
    {
        if (IsSimulating) throw new InvalidOperationException("运行态不允许修改编辑实体。");
        var source = G07PreviewSource ?? throw new InvalidOperationException("尚未导入地理地形。");
        var current = World.Find(id)!.Transform;
        if (current.Spatial.Space != SpatialSpace.Ecef)
            throw new InvalidOperationException("只能编辑全球实体的地理位置。");
        SpatialPosition position;
        if (aboveGround)
        {
            var sample = source.ResolveG07Ground(cpu, target, target.HeightMeters, out position);
            if (sample.Status != GeoSampleStatus.Valid) return sample.Status;
        }
        else position = source.ResolveG07Orthometric(target);
        Scene.SetTransform(id, Transform.FromSpatial(position, current.Rotation, current.Scale));
        return GeoSampleStatus.Valid;
    }

    public bool FocusG07Entity()
    {
        if (IsSimulating || G07PreviewSource is not { } source ||
            Scene.Selected is not { Visible: true } item || !item.IsGeographic) return false;
        var transform = World.Find(item.Id)!.Transform;
        double radius = EntityRadius(item, transform);
        source.Camera.FrameTop(transform.Spatial.ToGeo(), Math.Max(8d, radius * 4d));
        Log.Write($"[E6/全球实体] F：聚焦 {item.Name}");
        return true;
    }

    private static double EntityRadius(PreviewObject item, Transform transform)
    {
        if (item.Mesh is not { } mesh) return 0d;
        double radius = 0d;
        var vertices = mesh.Vertices.Span;
        for (int i = 0; i < vertices.Length; i += 7)
        {
            double x = vertices[i] * (double)transform.Scale.X;
            double y = vertices[i + 1] * (double)transform.Scale.Y;
            double z = vertices[i + 2] * (double)transform.Scale.Z;
            radius = Math.Max(radius, Math.Sqrt(x * x + y * y + z * z));
        }
        return radius;
    }
}