using XuanYu.World.Ecs;

namespace XuanYu.World;

public sealed partial class WorldScene
{
    public bool HasComponent<T>(Guid id) where T : unmanaged
    {
        EntityId handle = Require(id);
        return typeof(T) == typeof(Transform) || _ecs.Has<T>(handle);
    }

    public T GetComponent<T>(Guid id) where T : unmanaged
    {
        EntityId handle = Require(id);
        if (typeof(T) == typeof(Transform)) return (T)(object)ReadTransform(handle);
        return _ecs.Get<T>(handle);
    }

    public void SetComponent<T>(Guid id, T value) where T : unmanaged
    {
        EntityId handle = Require(id);
        if (typeof(T) == typeof(Transform))
            WriteTransform(handle, (Transform)(object)value);
        else if (typeof(T) == typeof(SpatialPosition))
            WritePosition(handle, (SpatialPosition)(object)value);
        else
            _ecs.Set(handle, value);
    }

    public bool AddComponent<T>(Guid id, T value = default) where T : unmanaged
    {
        EntityId handle = Require(id);
        return typeof(T) != typeof(Transform) && _ecs.Add(handle, value);
    }

    public bool RemoveComponent<T>(Guid id) where T : unmanaged
    {
        EntityId handle = Require(id);
        if (typeof(T) == typeof(Transform) || typeof(T) == typeof(SpatialPosition) ||
            typeof(T) == typeof(SceneOrientation) || typeof(T) == typeof(SceneVisibility))
            throw new InvalidOperationException("不能移除场景必需组件。");
        return _ecs.Remove<T>(handle);
    }
}