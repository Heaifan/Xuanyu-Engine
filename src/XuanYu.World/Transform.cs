using System.Numerics;
using System.Text.Json.Serialization;
using XuanYu.World.Ecs;

namespace XuanYu.World;

[JsonConverter(typeof(TransformJsonConverter))]
public readonly record struct Transform
{
    public SpatialPosition Spatial { get; private init; }
    public Quaternion Rotation { get; init; }
    public Vector3 Scale { get; init; }

    public Vector3 Position
    {
        get
        {
            RequireLocal();
            return new Vector3((float)Spatial.X, (float)Spatial.Y, (float)Spatial.Z);
        }
        init
        {
            RequireLocal();
            Spatial = SpatialPosition.FromLocal(value.X, value.Y, value.Z);
        }
    }

    public Transform(Vector3 Position, Quaternion Rotation, Vector3 Scale)
        : this(SpatialPosition.FromLocal(Position.X, Position.Y, Position.Z), Rotation, Scale)
    {
    }

    private Transform(SpatialPosition spatial, Quaternion rotation, Vector3 scale)
    {
        Spatial = spatial;
        Rotation = rotation;
        Scale = scale;
    }

    public static Transform FromSpatial(SpatialPosition spatial, Quaternion rotation, Vector3 scale) =>
        new(spatial, rotation, scale);

    public static Transform Identity => new(Vector3.Zero, Quaternion.Identity, Vector3.One);

    public void Deconstruct(out Vector3 position, out Quaternion rotation, out Vector3 scale) =>
        (position, rotation, scale) = (Position, Rotation, Scale);

    public Matrix4x4 ToMatrix() =>
        Matrix4x4.CreateScale(Scale) *
        Matrix4x4.CreateFromQuaternion(Rotation) *
        Matrix4x4.CreateTranslation(Position);

    public override string ToString() =>
        $"Transform {{ Spatial = {Spatial}, Rotation = {Rotation}, Scale = {Scale} }}";

    private void RequireLocal()
    {
        if (Spatial.Space != SpatialSpace.Local)
            throw new InvalidOperationException("全球坐标需要地理参考帧，不能直接转换为本地变换。");
    }
}