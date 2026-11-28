namespace XuanYu.World.Ecs;

internal sealed class OrganizationGraph
{
    private readonly Dictionary<Guid, Guid> _parents = [];
    private readonly Dictionary<Guid, HashSet<Guid>> _children = [];

    public bool HasRelations => _parents.Count != 0;

    public Guid? Parent(Guid id) =>
        _parents.TryGetValue(id, out Guid parent) ? parent : null;

    public Guid[] Children(Guid id) =>
        _children.TryGetValue(id, out var children) ? children.ToArray() : [];

    public bool SetParent(Guid child, Guid? parent)
    {
        if (Parent(child) == parent) return false;
        for (Guid? ancestor = parent; ancestor is Guid id; ancestor = Parent(id))
            if (id == child)
                throw new InvalidOperationException("组织关系不能形成环。");

        Detach(child);
        if (parent is Guid next) Attach(child, next);
        return true;
    }

    public void Remove(Guid id)
    {
        Detach(id);
        if (!_children.Remove(id, out var children)) return;
        foreach (Guid child in children) _parents.Remove(child);
    }

    public OrganizationGraph Clone()
    {
        var copy = new OrganizationGraph();
        foreach (var (child, parent) in _parents) copy.Attach(child, parent);
        return copy;
    }

    private void Attach(Guid child, Guid parent)
    {
        _parents.Add(child, parent);
        if (!_children.TryGetValue(parent, out var children))
        {
            children = [];
            _children.Add(parent, children);
        }
        children.Add(child);
    }

    private void Detach(Guid child)
    {
        if (!_parents.Remove(child, out Guid parent)) return;
        var children = _children[parent];
        children.Remove(child);
        if (children.Count == 0) _children.Remove(parent);
    }
}