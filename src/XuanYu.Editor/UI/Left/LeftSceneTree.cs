using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using XuanYu.Editor.Preview;

namespace XuanYu.Editor.UI.Left;

internal sealed partial class LeftSceneTree : StackPanel
{
    private readonly PreviewScene _scene;
    private readonly StackPanel _rows = new();
    private readonly LeftTreeNode _root = new("当前场景", "Nodes/scene");
    private readonly LeftTreeNode _meshes = new("网格对象", "Nodes/folder");
    private readonly Dictionary<Guid, LeftTreeRow> _objectRows = [];

    public LeftSceneTree(PreviewScene scene)
    {
        _scene = scene;
        Spacing = 5;
        _root.Children.Add(_meshes);
        Children.Add(new TextBlock
        {
            Text = "场景结构", FontFamily = new FontFamily("Microsoft YaHei UI"),
            FontWeight = FontWeight.SemiBold, FontSize = 11,
            Foreground = EditorTheme.TextSub,
            Margin = new Thickness(4, 8, 0, 4)
        });
        Children.Add(_rows);
        ContextMenu = CreateSceneMenu();
        ((INotifyCollectionChanged)_scene.Objects).CollectionChanged +=
            (_, _) => Rebuild();
        _scene.EntityRenamed += Rebuild;
        _scene.Changed += Refresh;
        KeyDown += OnTreeKeyDown;
        Rebuild();
    }

    public void Refresh()
    {
        foreach ((Guid id, LeftTreeRow row) in _objectRows)
            row.Refresh(_scene.SelectedId == id);
    }

    private void Rebuild()
    {
        _meshes.Children.Clear();
        foreach (PreviewObject item in _scene.Objects)
            _meshes.Children.Add(new LeftTreeNode(item.Name, "Nodes/mesh", item));
        _rows.Children.Clear();
        _objectRows.Clear();
        AddNode(_root, 0, [], true);
        Refresh();
    }

    private void AddNode(LeftTreeNode node, int depth,
        bool[] continuations, bool isLast)
    {
        var row = new LeftTreeRow(node, depth, continuations, isLast);
        _rows.Children.Add(row);
        row.ContextMenu = CreateNodeMenu(node, row);
        if (node.Item is { } item)
        {
            _objectRows.Add(item.Id, row);
            row.PointerPressed += (_, e) =>
            {
                if (e.GetCurrentPoint(row).Properties.IsRightButtonPressed)
                {
                    _scene.Select(item.Id);
                    row.Focus();
                }
            };
        }
        row.Click += (_, _) =>
        {
            if (node.Children.Count == 0 && node.Item is { } target)
                _scene.Select(target.Id);
            else
            {
                node.IsExpanded = !node.IsExpanded;
                Rebuild();
            }
        };
        if (!node.IsExpanded)
            return;
        bool[] next = depth == 0 ? [] : [.. continuations, !isLast];
        for (int i = 0; i < node.Children.Count; i++)
            AddNode(node.Children[i], depth + 1, next,
                i == node.Children.Count - 1);
    }
}