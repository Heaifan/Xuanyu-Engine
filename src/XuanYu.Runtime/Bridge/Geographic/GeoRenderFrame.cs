using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.World;

namespace XuanYu.Runtime;

public sealed class GeoRenderFrame
{
    private readonly EnuFrame _anchor;
    public EnuPosition Eye { get; }
    public Matrix4x4 RelativeProjection { get; }

    public GeoRenderFrame(EnuFrame anchor, EcefPosition eye, EcefPosition target,
        float aspect, float near, float far)
    {
        _anchor = anchor;
        Eye = anchor.ToEnu(eye);
        Matrix4x4 view = Matrix4x4.CreateLookAt(Vector3.Zero, ToRelative(target), Vector3.UnitZ);
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 3f, aspect, near, far);
        projection.M22 = -projection.M22;
        RelativeProjection = view * projection;
    }

    // 仅供仍以数据集 ENU float 顶点存储的外部网格兼容使用。
    public Matrix4x4 AnchorProjection => Matrix4x4.CreateTranslation(
        -(float)Eye.East, -(float)Eye.North, -(float)Eye.Up) * RelativeProjection;

    public Vector3 ToRelative(EcefPosition position)
    {
        var p = _anchor.ToEnu(position);
        return new((float)(p.East - Eye.East), (float)(p.North - Eye.North), (float)(p.Up - Eye.Up));
    }

    public Matrix4x4 ModelToClip(Transform transform)
    {
        var position = transform.Spatial.ToEcef();
        var local = new EnuFrame(Wgs84.ToGeo(position));
        var origin = _anchor.ToEnu(local.OriginEcef);
        Vector3 Axis(EnuPosition axis)
        {
            var p = _anchor.ToEnu(local.ToEcef(axis));
            return new((float)(p.East - origin.East), (float)(p.North - origin.North), (float)(p.Up - origin.Up));
        }
        var east = Axis(new(1, 0, 0));
        var north = Axis(new(0, 1, 0));
        var up = Axis(new(0, 0, 1));
        var basis = new Matrix4x4(east.X, east.Y, east.Z, 0,
            north.X, north.Y, north.Z, 0, up.X, up.Y, up.Z, 0, 0, 0, 0, 1);
        // 全球 Rotation 的局部轴定义为实体所在地 ENU，而非数据集中心 ENU。
        return Matrix4x4.CreateScale(transform.Scale) * Matrix4x4.CreateFromQuaternion(transform.Rotation) *
            basis * Matrix4x4.CreateTranslation(ToRelative(position)) * RelativeProjection;
    }
}