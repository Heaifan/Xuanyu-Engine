using XuanYu.Editor.Preview;

namespace XuanYu.Editor.UI.Left;

internal sealed partial class LeftSceneTree
{
    private void PopulateOrganization()
    {
        var nodes = new Dictionary<Guid, LeftTreeNode>();
        foreach (PreviewObject item in _scene.Objects)
            nodes.Add(item.Id, new LeftTreeNode(item.Name, item.Mesh is null ? "Nodes/file" : "Nodes/mesh", item)
            { IsExpanded = !_collapsed.Contains(item.Id) });
        _collapsed.RemoveWhere(id => !nodes.ContainsKey(id));
        foreach (var (id, node) in nodes)
        {
            var parent = _scene.World.GetOrganizationParent(id);
            if (parent is Guid parentId && nodes.TryGetValue(parentId, out var upper))
                upper.Children.Add(node);
            else
                (node.Item!.IsGeographic ? _geoMeshes : _meshes).Children.Add(node);
        }
    }

    private void ToggleNode(LeftTreeNode node)
    {
        if (node.Children.Count == 0) return;
        node.IsExpanded = !node.IsExpanded;
        if (node.Item is { } item)
        {
            if (node.IsExpanded) _collapsed.Remove(item.Id);
            else _collapsed.Add(item.Id);
        }
        Rebuild();
    }
}