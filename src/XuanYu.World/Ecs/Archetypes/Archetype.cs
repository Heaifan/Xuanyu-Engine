using System.Runtime.InteropServices;

namespace XuanYu.World.Ecs;

public sealed class Archetype
{
    private readonly List<EntityId> _entities = [];
    private readonly Dictionary<Type, IComponentColumn> _columns = [];
    private readonly ComponentType[] _types;

    public int Count => _entities.Count;
    public int ComponentCount => _columns.Count;
    internal ReadOnlySpan<ComponentType> Types => _types;

    // 结构变化后须重新获取全部 Span 和行号。
    public ReadOnlySpan<EntityId> Entities => CollectionsMarshal.AsSpan(_entities);

    public Archetype(params ComponentType[] components)
    {
        ArgumentNullException.ThrowIfNull(components);
        _types = (ComponentType[])components.Clone();
        foreach (ComponentType component in _types)
        {
            ArgumentNullException.ThrowIfNull(component);
            if (!_columns.TryAdd(component.ValueType, component.CreateColumn()))
                throw new ArgumentException("Archetype 组件类型重复。", nameof(components));
        }
    }

    public bool Has<T>() where T : unmanaged => Has(typeof(T));
    internal bool Has(Type type) => _columns.ContainsKey(type);

    public Span<T> GetColumn<T>() where T : unmanaged =>
        ((ComponentColumn<T>)_columns[typeof(T)]).Values;

    // 调用方负责确保实体存活、属于当前世界且尚未占据任何行。
    public int Append(EntityId entity)
    {
        int row = Count;
        foreach (IComponentColumn column in _columns.Values)
            column.AppendDefault();
        _entities.Add(entity);
        return row;
    }

    internal void CopySharedTo(int row, Archetype target, int targetRow)
    {
        foreach (var pair in _columns)
            if (target._columns.TryGetValue(pair.Key, out var column))
                pair.Value.CopyTo(row, column, targetRow);
    }

    public EntityId? RemoveAtSwapBack(int row)
    {
        if ((uint)row >= (uint)Count)
            throw new ArgumentOutOfRangeException(nameof(row));

        int last = Count - 1;
        EntityId? moved = null;
        if (row != last)
        {
            moved = _entities[last];
            _entities[row] = moved.Value;
        }
        foreach (IComponentColumn column in _columns.Values)
            column.RemoveAtSwapBack(row);
        _entities.RemoveAt(last);
        return moved;
    }
}