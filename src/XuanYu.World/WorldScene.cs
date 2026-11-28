using XuanYu.World.Ecs;

namespace XuanYu.World;

public sealed class WorldScene
{
    private EcsWorld _ecs = new();
    private Dictionary<Guid, string> _names = [];

    public IReadOnlyCollection<WorldEntity> Entities =>
        _names.Keys.Select(id => Read(id, Require(id))).ToArray();

    public WorldEntity Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var entity = new WorldEntity(Guid.NewGuid(), name.Trim(),
            true, Transform.Identity);
        Insert(entity);
        return entity;
    }

    public WorldEntity? Find(Guid id) =>
        _ecs.TryGetId(id, out EntityId handle) ? Read(id, handle) : null;

    public WorldEntity Rename(Guid id, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        EntityId handle = Require(id);
        string trimmed = name.Trim();
        if (_names.Any(x => x.Key != id && x.Value == trimmed))
            throw new InvalidOperationException("场景中已经存在同名实体。");
        _names[id] = trimmed;
        return Read(id, handle);
    }

    public WorldEntity SetTransform(Guid id, Transform transform)
    {
        EntityId handle = Require(id);
        _ecs.Set(handle, transform);
        return Read(id, handle);
    }

    public WorldEntity SetVisible(Guid id, bool visible)
    {
        EntityId handle = Require(id);
        _ecs.Set(handle, new SceneVisibility(visible));
        return Read(id, handle);
    }

    public bool Delete(Guid id)
    {
        if (!_ecs.TryGetId(id, out EntityId handle)) return false;
        _ecs.Destroy(handle);
        _names.Remove(id);
        return true;
    }

    public void Replace(IReadOnlyList<WorldEntity> entities)
    {
        var replacement = new WorldScene();
        foreach (WorldEntity entity in entities) replacement.Insert(entity);
        _ecs = replacement._ecs;
        _names = replacement._names;
    }

    private void Insert(WorldEntity entity)
    {
        EntityId handle = _ecs.Create(entity.Id);
        _ecs.Add(handle, entity.Transform);
        _ecs.Add(handle, new SceneVisibility(entity.IsVisible));
        _names.Add(entity.Id, entity.Name);
    }

    private EntityId Require(Guid id) =>
        _ecs.TryGetId(id, out EntityId handle) ? handle :
            throw new KeyNotFoundException($"场景实体不存在：{id}");

    private WorldEntity Read(Guid id, EntityId handle) =>
        new(id, _names[id], _ecs.Get<SceneVisibility>(handle).IsVisible,
            _ecs.Get<Transform>(handle));
}