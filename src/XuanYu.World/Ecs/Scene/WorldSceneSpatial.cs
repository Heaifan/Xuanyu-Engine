using XuanYu.World.Ecs;

namespace XuanYu.World;

public sealed partial class WorldScene
{
    public SpatialPosition GetSpatialPosition(Guid id) =>
        _ecs.Get<SpatialPosition>(Require(id));

    public WorldEntity SetSpatialPosition(Guid id, SpatialPosition position)
    {
        EntityId handle = Require(id);
        WritePosition(handle, position);
        return Read(id, handle);
    }

    private void WritePosition(EntityId handle, SpatialPosition position)
    {
        if (_ecs.Get<SpatialPosition>(handle).Space != position.Space)
            throw new InvalidOperationException("不能直接改变实体的坐标空间。");
        _ecs.Set(handle, position);
    }

    private void WriteTransform(EntityId handle, Transform transform)
    {
        WritePosition(handle, transform.Spatial);
        _ecs.Set(handle, new SceneOrientation(transform.Rotation, transform.Scale));
    }

    private Transform ReadTransform(EntityId handle)
    {
        var orientation = _ecs.Get<SceneOrientation>(handle);
        return Transform.FromSpatial(_ecs.Get<SpatialPosition>(handle),
            orientation.Rotation, orientation.Scale);
    }
}