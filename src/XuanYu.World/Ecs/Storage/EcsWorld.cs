namespace XuanYu.World.Ecs;

public sealed partial class EcsWorld
{
    private readonly EntityRegistry _registry = new();
    private readonly ArchetypeCatalog _catalog = new();
    private readonly List<EntityLocation> _locations = [];

    public int Count => _registry.Count;
    public bool IsAlive(EntityId id) => _registry.IsAlive(id);
    public bool TryGetId(Guid guid, out EntityId id) => _registry.TryGetId(guid, out id);
    public bool TryGetGuid(EntityId id, out Guid guid) => _registry.TryGetGuid(id, out guid);

    public EntityId Create() => Create(Guid.NewGuid());

    public EntityId Create(Guid guid) => Create(guid, _catalog.Empty);

    private EntityId Create(Guid guid, Archetype table)
    {
        EntityId id = _registry.Create(guid);
        var location = new EntityLocation(table, table.Append(id));
        if (id.Index == _locations.Count) _locations.Add(location);
        else _locations[id.Index] = location;
        return id;
    }

    public bool Destroy(EntityId id)
    {
        if (!_registry.Destroy(id)) return false;
        RemoveRow(_locations[id.Index]);
        _locations[id.Index] = default;
        return true;
    }

    public bool Has<T>(EntityId id) where T : unmanaged => Locate(id).Table.Has<T>();

    public T Get<T>(EntityId id) where T : unmanaged
    {
        var location = Locate(id);
        return location.Table.GetColumn<T>()[location.Row];
    }

    public void Set<T>(EntityId id, T value) where T : unmanaged
    {
        var location = Locate(id);
        location.Table.GetColumn<T>()[location.Row] = value;
    }

    public bool Add<T>(EntityId id, T value = default) where T : unmanaged
    {
        var source = Locate(id);
        if (source.Table.Has<T>()) return false;
        var target = _catalog.Transition<T>(source.Table, true);
        int row = Move(id, source, target);
        target.GetColumn<T>()[row] = value;
        return true;
    }

    public bool Remove<T>(EntityId id) where T : unmanaged
    {
        var source = Locate(id);
        if (!source.Table.Has<T>()) return false;
        Move(id, source, _catalog.Transition<T>(source.Table, false));
        return true;
    }

    // 查询视图与 Span 只能在世界结构稳定期间使用。
    public IEnumerable<ArchetypeView> Query(params ComponentType[] required) =>
        _catalog.Query(required);

    private EntityLocation Locate(EntityId id)
    {
        if (!_registry.IsAlive(id)) throw new KeyNotFoundException("实体句柄已失效。");
        return _locations[id.Index];
    }

    private int Move(EntityId id, EntityLocation source, Archetype target)
    {
        int row = target.Append(id);
        source.Table.CopySharedTo(source.Row, target, row);
        RemoveRow(source);
        _locations[id.Index] = new EntityLocation(target, row);
        return row;
    }

    private void RemoveRow(EntityLocation source)
    {
        EntityId? moved = source.Table.RemoveAtSwapBack(source.Row);
        if (moved is EntityId id) _locations[id.Index] = source;
    }
}