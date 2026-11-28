namespace XuanYu.World.Ecs;

public readonly struct ArchetypeView
{
    private readonly Archetype _table;

    internal ArchetypeView(Archetype table) => _table = table;

    public int Count => _table.Count;
    public ReadOnlySpan<EntityId> Entities => _table.Entities;
    public bool Has<T>() where T : unmanaged => _table.Has<T>();

    public Span<T> GetColumn<T>() where T : unmanaged => _table.GetColumn<T>();
}