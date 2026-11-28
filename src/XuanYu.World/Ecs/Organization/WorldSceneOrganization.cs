using XuanYu.World.Ecs;

namespace XuanYu.World;

public sealed partial class WorldScene
{
    private OrganizationGraph _organization = new();

    public bool HasOrganizationRelations => _organization.HasRelations;

    public Guid? GetOrganizationParent(Guid id)
    {
        Require(id);
        return _organization.Parent(id);
    }

    public IReadOnlyList<Guid> GetOrganizationChildren(Guid id)
    {
        Require(id);
        return _organization.Children(id);
    }

    public bool SetOrganizationParent(Guid child, Guid? parent)
    {
        Require(child);
        if (parent is Guid id) Require(id);
        return _organization.SetParent(child, parent);
    }
}