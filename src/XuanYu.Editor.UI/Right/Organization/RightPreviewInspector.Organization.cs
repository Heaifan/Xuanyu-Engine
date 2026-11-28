using System.Collections.Specialized;

namespace XuanYu.Editor.UI.Right;

public sealed partial class RightPreviewInspector
{
    private RightPropertyGroup? _organizationGroup;
    private RightOrganizationEditor? _organizationEditor;

    private void BindOrganization()
    {
        _scene.OrganizationChanged += RefreshOrganization;
        _scene.EntityRenamed += RefreshOrganization;
        ((INotifyCollectionChanged)_scene.Objects).CollectionChanged += (_, _) => RefreshOrganization();
    }

    private RightPropertyGroup CreateOrganizationGroup(Guid id)
    {
        _organizationEditor = new RightOrganizationEditor(_scene, id);
        _organizationGroup = CreateGroup("组织层级");
        _organizationGroup.AddControl(_organizationEditor);
        return _organizationGroup;
    }

    private void ClearOrganization()
    {
        _organizationEditor = null;
        _organizationGroup = null;
    }

    private void RefreshOrganization()
    {
        if (_organizationEditor is { } editor && _scene.SelectedId == editor.EntityId &&
            _scene.World.Find(editor.EntityId) is not null)
            editor.Refresh();
    }

    internal void RevealOrganization()
    {
        Refresh();
        _organizationGroup?.Reveal();
    }
}