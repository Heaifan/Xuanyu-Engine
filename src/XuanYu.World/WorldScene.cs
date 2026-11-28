using System.Numerics;
using XuanYu.World.Ecs;

namespace XuanYu.World;

public sealed partial class WorldScene
{
    private EcsWorld _ecs = new();
    private Dictionary<Guid, string> _names = [];

    public IReadOnlyCollection<WorldEntity> Entities =>
        _names.Keys.Select(id => Read(id, Require(id))).ToArray();

    public WorldEntity Create(string name) => Create(name, default);

    public WorldEntity Create(string name, SpatialPosition position)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var entity = new WorldEntity(Guid.NewGuid(), name.Trim(),
            true, Transform.FromSpatial(position, Quaternion.Identity,
                Vector3.One));
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
        WriteTransform(handle, transform);
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
        _organization.Remove(id);
        _ecs.Destroy(handle);
        _names.Remove(id);
        return true;
    }

    public WorldScene Clone() => new()
    {
        _ecs = _ecs.Clone(),
        _names = new Dictionary<Guid, string>(_names),
        _organization = _organization.Clone()
    };

    public void Replace(IReadOnlyList<WorldEntity> entities,
        IReadOnlyDictionary<Guid, Guid>? organizationParents = null)
    {
        var replacement = new WorldScene();
        foreach (WorldEntity entity in entities) replacement.Insert(entity);
        if (organizationParents is not null)
            foreach (var (child, parent) in organizationParents)
                replacement.SetOrganizationParent(child, parent);
        _ecs = replacement._ecs;
        _names = replacement._names;
        _organization = replacement._organization;
    }

    private void Insert(WorldEntity entity)
    {
        EntityId handle = _ecs.Create(entity.Id);
        _ecs.Add(handle, entity.Transform.Spatial);
        _ecs.Add(handle, new SceneOrientation(entity.Transform.Rotation, entity.Transform.Scale));
        _ecs.Add(handle, new SceneVisibility(entity.IsVisible));
        _names.Add(entity.Id, entity.Name);
    }

    private EntityId Require(Guid id) =>
        _ecs.TryGetId(id, out EntityId handle) ? handle :
            throw new KeyNotFoundException($"场景实体不存在：{id}");

    private WorldEntity Read(Guid id, EntityId handle) =>
        new(id, _names[id], _ecs.Get<SceneVisibility>(handle).IsVisible,
            ReadTransform(handle));
}