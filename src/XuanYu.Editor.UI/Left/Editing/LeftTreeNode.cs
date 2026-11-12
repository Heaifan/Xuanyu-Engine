using XuanYu.Editor.Preview;

namespace XuanYu.Editor.UI.Left;

internal sealed class LeftTreeNode(
    string title, string icon, PreviewObject? item = null)
{
    public string Title { get; } = title;
    public string Icon { get; } = icon;
    public PreviewObject? Item { get; } = item;
    public bool IsExpanded { get; set; } = true;
    public List<LeftTreeNode> Children { get; } = [];

    public static LeftTreeNode FromFile(string fileName) =>
        new(fileName, LeftSvgIcon.ForFileName(fileName));
}