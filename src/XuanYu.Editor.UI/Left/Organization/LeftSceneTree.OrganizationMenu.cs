using Avalonia.Controls;
using XuanYu.Editor.Preview;

namespace XuanYu.Editor.UI.Left;

internal sealed partial class LeftSceneTree
{
    internal event Action? OrganizationFocusRequested;

    private MenuItem CreateOrganizationMenu(ContextMenu menu, Guid child)
    {
        var organization = LeftMenuStyle.Item("调整层级", "↳");
        menu.Opening += (_, _) => PopulateParentMenu(organization, child);
        organization.SubmenuOpened += (_, _) =>
        {
            _scene.Select(child);
            OrganizationFocusRequested?.Invoke();
        };
        return organization;
    }

    private void PopulateParentMenu(MenuItem menu, Guid child)
    {
        var info = _scene.GetOrganizationEditingInfo(child);
        menu.Items.Clear();
        foreach (OrganizationParentOption option in info.Parents)
        {
            if (option.Id is not null && menu.Items.Count == 1)
                menu.Items.Add(LeftMenuStyle.Divider());
            var choice = LeftMenuStyle.Item(option.Name, "↳");
            if (option.Id == info.ParentId)
                choice.Icon = new TextBlock { Text = "✓", Foreground = EditorTheme.Accent, FontSize = 13 };
            choice.Click += (_, _) => _scene.SetOrganizationParent(child, option.Id);
            menu.Items.Add(choice);
        }
    }
}