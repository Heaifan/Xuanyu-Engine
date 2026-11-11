using Avalonia.Controls;
using Avalonia.Input;

namespace XuanYu.Editor.UI.Left;

internal sealed partial class LeftSceneTree
{
    internal ContextMenu CreateSceneMenu()
    {
        ContextMenu menu = LeftMenuStyle.Create();
        menu.Items.Add(CreateItem());
        return menu;
    }

    private ContextMenu CreateNodeMenu(LeftTreeNode node, LeftTreeRow row)
    {
        ContextMenu menu = CreateSceneMenu();
        if (node.Item is not { } item)
            return menu;

        MenuItem rename = LeftMenuStyle.Item(
            "重命名", "✎", new KeyGesture(Key.F2));
        rename.Click += (_, _) => row.BeginRename(
            name => _scene.Rename(item.Id, name));
        MenuItem delete = LeftMenuStyle.Item(
            "删除", "×", new KeyGesture(Key.Delete));
        delete.Click += (_, _) => _scene.Delete(item.Id);
        menu.Items.Add(LeftMenuStyle.Divider());
        menu.Items.Add(rename);
        menu.Items.Add(delete);
        return menu;
    }

    private MenuItem CreateItem()
    {
        MenuItem create = LeftMenuStyle.Item("创建", "＋",
            new KeyGesture(Key.A, KeyModifiers.Shift));
        MenuItem cube = LeftMenuStyle.Item("立方体", "□");
        cube.Click += (_, _) => _scene.CreateCube();
        create.Items.Add(cube);
        return create;
    }
}