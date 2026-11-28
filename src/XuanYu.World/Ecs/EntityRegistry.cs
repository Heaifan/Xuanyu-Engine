namespace XuanYu.World.Ecs;

public sealed class EntityRegistry
{
    private readonly List<Slot> _slots = [];
    private readonly Stack<int> _free = new();
    private readonly Dictionary<Guid, EntityId> _ids = [];

    public int Count => _ids.Count;

    public EntityId Create() => Create(Guid.NewGuid());

    public EntityId Create(Guid guid)
    {
        if (guid == Guid.Empty)
            throw new ArgumentException("实体永久身份不能为空。", nameof(guid));
        if (_ids.ContainsKey(guid))
            throw new ArgumentException("实体永久身份已存在。", nameof(guid));

        EntityId id = Allocate(guid);
        _ids.Add(guid, id);
        return id;
    }

    public bool TryGetId(Guid guid, out EntityId id) =>
        _ids.TryGetValue(guid, out id);

    public bool IsAlive(EntityId id) =>
        (uint)id.Index < (uint)_slots.Count &&
        _slots[id.Index].Generation == id.Generation &&
        _slots[id.Index].Guid != Guid.Empty;

    public bool TryGetGuid(EntityId id, out Guid guid)
    {
        if (!IsAlive(id))
        {
            guid = Guid.Empty;
            return false;
        }
        guid = _slots[id.Index].Guid;
        return true;
    }

    public bool Destroy(EntityId id)
    {
        if (!TryGetGuid(id, out Guid guid)) return false;
        // 代数先递增，旧句柄不能访问复用后的实体。
        uint generation = checked(id.Generation + 1);
        _ids.Remove(guid);
        _slots[id.Index] = new Slot(Guid.Empty, generation);
        _free.Push(id.Index);
        return true;
    }

    private EntityId Allocate(Guid guid)
    {
        if (_free.TryPop(out int index))
        {
            uint generation = _slots[index].Generation;
            _slots[index] = new Slot(guid, generation);
            return new EntityId(index, generation);
        }

        var id = new EntityId(_slots.Count, 1);
        _slots.Add(new Slot(guid, id.Generation));
        return id;
    }

    private readonly record struct Slot(Guid Guid, uint Generation);
}