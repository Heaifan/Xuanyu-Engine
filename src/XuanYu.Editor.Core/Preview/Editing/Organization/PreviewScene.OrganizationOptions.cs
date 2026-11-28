namespace XuanYu.Editor.Preview;

public sealed record OrganizationParentOption(Guid? Id, string Name)
{
    public override string ToString() => Name;
}

public sealed record OrganizationEditingInfo(Guid? ParentId, int Depth,
    int DirectChildren, int Descendants, IReadOnlyList<OrganizationParentOption> Parents);

public sealed partial class PreviewScene
{
    public OrganizationEditingInfo GetOrganizationEditingInfo(Guid child)
    {
        Guid? parent = World.GetOrganizationParent(child);
        var children = World.GetOrganizationChildren(child);
        var excluded = new HashSet<Guid> { child };
        var pending = new Stack<Guid>(children);
        while (pending.TryPop(out Guid id))
        {
            excluded.Add(id);
            foreach (Guid descendant in World.GetOrganizationChildren(id))
                pending.Push(descendant);
        }
        int depth = 0;
        for (Guid? upper = parent; upper is Guid upperId; upper = World.GetOrganizationParent(upperId))
            depth++;
        var options = new List<OrganizationParentOption> { new(null, "无上级（独立实体）") };
        options.AddRange(World.Entities.Where(entity => !excluded.Contains(entity.Id))
            .OrderBy(entity => entity.Name, StringComparer.Ordinal)
            .Select(entity => new OrganizationParentOption(entity.Id, entity.Name)));
        return new(parent, depth, children.Count, excluded.Count - 1, options);
    }
}