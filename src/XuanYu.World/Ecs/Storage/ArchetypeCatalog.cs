namespace XuanYu.World.Ecs;

internal sealed class ArchetypeCatalog
{
    private readonly List<Archetype> _tables = [new Archetype()];
    public Archetype Empty => _tables[0];

    public Archetype Transition<T>(Archetype source, bool adding) where T : unmanaged
    {
        Type changed = typeof(T);
        int count = source.ComponentCount + (adding ? 1 : -1);
        foreach (Archetype table in _tables)
        {
            if (table.ComponentCount == count && table.Has(changed) == adding &&
                MatchesTransition(source, table, changed))
                return table;
        }

        var types = new ComponentType[count];
        int next = 0;
        foreach (ComponentType type in source.Types)
            if (type.ValueType != changed) types[next++] = type;
        if (adding) types[next] = ComponentType.Of<T>();
        var target = new Archetype(types);
        _tables.Add(target);
        return target;
    }

    public IEnumerable<ArchetypeView> Query(ComponentType[] required)
    {
        ArgumentNullException.ThrowIfNull(required);
        foreach (ComponentType type in required)
            ArgumentNullException.ThrowIfNull(type);

        foreach (Archetype table in _tables)
            if (table.Count != 0 && MatchesQuery(table, required))
                yield return new ArchetypeView(table);
    }

    private static bool MatchesTransition(Archetype source, Archetype target, Type changed)
    {
        foreach (ComponentType type in source.Types)
            if (type.ValueType != changed && !target.Has(type.ValueType)) return false;
        return true;
    }

    private static bool MatchesQuery(Archetype table, ComponentType[] required)
    {
        foreach (ComponentType type in required)
            if (!table.Has(type.ValueType)) return false;
        return true;
    }
}