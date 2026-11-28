using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview;
using XuanYu.World;
using XuanYu.World.Ecs;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    public void TranslateG07Entity(Guid id, EnuPosition delta)
    {
        var (_, current) = EditableGeoEntity(id);
        Scene.SetTransform(id, Translated(current, delta));
    }

    public void RotateG07Entity(Guid id, Vector3 degrees)
    {
        var (_, current) = EditableGeoEntity(id);
        Scene.SetTransform(id, Rotated(current, degrees));
    }

    private static Transform Rotated(Transform current, Vector3 degrees)
    {
        if (degrees == Vector3.Zero) return current;
        var angle = degrees * (MathF.PI / 180f);
        var matrix = Matrix4x4.CreateFromQuaternion(current.Rotation) *
            Matrix4x4.CreateRotationX(angle.X) * Matrix4x4.CreateRotationY(angle.Y) *
            Matrix4x4.CreateRotationZ(angle.Z);
        return current with { Rotation = Quaternion.CreateFromRotationMatrix(matrix) };
    }

    public void ScaleG07Entity(Guid id, Vector3 factor)
    {
        var (_, current) = EditableGeoEntity(id);
        Scene.SetTransform(id, Scaled(current, factor));
    }

    private static Transform Scaled(Transform current, Vector3 factor)
    {
        if (factor == Vector3.One) return current;
        var scale = current.Scale * factor;
        if (!float.IsFinite(scale.X) || !float.IsFinite(scale.Y) || !float.IsFinite(scale.Z) ||
            scale.X <= 0f || scale.Y <= 0f || scale.Z <= 0f)
            throw new ArgumentException("缩放结果必须为有限正数。", nameof(factor));
        return current with { Scale = scale };
    }

    public GeoSampleStatus GroundG07Entity(GeoUnifiedLodSession cpu, Guid id)
    {
        var (item, current) = EditableGeoEntity(id);
        var status = ResolveGround(cpu, item, current, out var grounded);
        if (status == GeoSampleStatus.Valid) Scene.SetTransform(id, grounded);
        return status;
    }

    private GeoSampleStatus ResolveGround(GeoUnifiedLodSession cpu, PreviewObject item,
        Transform current, out Transform grounded)
    {
        grounded = current;
        var geo = current.Spatial.ToGeo();
        var sample = cpu.SampleElevation(geo);
        if (sample.Status != GeoSampleStatus.Valid) return sample.Status;
        double bottom = MeshBottom(item, current);
        // 贴地基准是实体中心经纬度的原始 DEM，而不是整个模型投影范围。
        var target = new GeoCoordinate(geo.LatitudeDegrees, geo.LongitudeDegrees, sample.HeightMeters!.Value - bottom);
        var position = G07PreviewSource!.ResolveG07Orthometric(target);
        grounded = Transform.FromSpatial(position, current.Rotation, current.Scale);
        return GeoSampleStatus.Valid;
    }

    private static double MeshBottom(PreviewObject item, Transform current)
    {
        if (item.Mesh is not { } mesh) return 0d;
        var matrix = Matrix4x4.CreateScale(current.Scale) * Matrix4x4.CreateFromQuaternion(current.Rotation);
        double bottom = double.PositiveInfinity;
        var vertices = mesh.Vertices.Span;
        for (int i = 0; i < vertices.Length; i += 7)
        {
            var point = Vector3.Transform(new Vector3(vertices[i], vertices[i + 1], vertices[i + 2]), matrix);
            bottom = Math.Min(bottom, point.Z);
        }
        return bottom;
    }

    private (PreviewObject Item, Transform Current) EditableGeoEntity(Guid id)
    {
        if (IsSimulating || G07PreviewSource is null)
            throw new InvalidOperationException("当前不是全球地形编辑态。");
        var item = Scene.Objects.FirstOrDefault(x => x.Id == id);
        if (item is not { Visible: true, IsGeographic: true })
            throw new InvalidOperationException("请选择可见全球实体。");
        return (item, World.Find(id)!.Transform);
    }
}