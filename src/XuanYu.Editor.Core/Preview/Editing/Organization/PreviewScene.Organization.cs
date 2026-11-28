namespace XuanYu.Editor.Preview;

public sealed partial class PreviewScene
{
    public event Action? OrganizationChanged;

    public bool SetOrganizationParent(Guid child, Guid? parent)
    {
        if (!World.SetOrganizationParent(child, parent)) return false;
        OrganizationChanged?.Invoke();
        Publish();
        return true;
    }
}