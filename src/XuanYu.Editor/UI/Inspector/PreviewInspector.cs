
using Avalonia.Controls;
using Avalonia.Media;
using XuanYu.Editor.Preview;

namespace XuanYu.Editor.UI;

internal sealed class PreviewInspector : StackPanel
{
    private readonly PreviewScene _scene;
    private readonly TextBlock _name = new()
    {
        FontSize = 13,
        FontWeight = FontWeight.SemiBold
    };
    private readonly TextBlock _details = new()
    {
        FontSize = 12,
        LineHeight = 21
    };
    private readonly CheckBox _visible = new()
    {
        Content = "视口可见",
        FontSize = 12
    };

    public PreviewInspector(PreviewScene scene)
    {
        _scene = scene;
        Spacing = 10;
        _name.Foreground = EditorTheme.TextMain;
        _details.Foreground = EditorTheme.TextSub;

        Children.Add(_name);
        Children.Add(_details);
        Children.Add(_visible);

        _visible.Click += (_, _) =>
        {
            if (_scene.Selected is { } item)
                _scene.SetVisible(item.Id, _visible.IsChecked == true);
        };

        Refresh();
    }

    public void Refresh()
    {
        PreviewObject? item = _scene.Selected;

        _name.Text = item?.Name ?? "未选中对象";
        _details.Text = item is null
            ? "在 Left 中选择一个对象。"
            : $"网格：{item.Mesh.AssetId}\n" +
              $"顶点：{item.Mesh.VertexCount}\n" +
              $"三角面：{item.Mesh.TriangleCount}\n" +
              "拓扑：TriangleList";

        _visible.IsEnabled = item is not null;
        _visible.IsChecked = item?.Visible ?? false;
    }
}
