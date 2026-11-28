using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using XuanYu.Editor.Preview;
using XuanYu.Runtime;

namespace XuanYu.Editor.UI.Right;

internal sealed class RightRepresentationEditor : Grid
{
    private static readonly EntityRepresentationKind[] Kinds =
        [EntityRepresentationKind.Mesh, EntityRepresentationKind.Proxy, EntityRepresentationKind.None];
    private readonly PreviewScene _scene;
    private readonly Guid _id;
    private readonly ComboBox _choice = new()
    {
        MinHeight = 0, Height = 24, MinWidth = 0, FontSize = 11,
        Padding = new Thickness(5, 0), HorizontalAlignment = HorizontalAlignment.Stretch,
        Background = EditorTheme.PanelHeaderBg, BorderBrush = EditorTheme.Border,
        BorderThickness = new Thickness(1), Foreground = EditorTheme.TextMain,
        ItemsSource = new[] { "主网格", "简化代理", "无表现" }
    };

    public RightRepresentationEditor(PreviewScene scene, Guid id)
    {
        _scene = scene;
        _id = id;
        Height = 28;
        Margin = new Thickness(8, 0);
        ColumnDefinitions = new ColumnDefinitions("2*,3*");
        Children.Add(new TextBlock { Text = "表现形式", FontSize = 12,
            Foreground = EditorTheme.TextSub, VerticalAlignment = VerticalAlignment.Center });
        SetColumn(_choice, 1);
        Children.Add(_choice);
        Refresh();
        _choice.SelectionChanged += (_, _) =>
        {
            if (_choice.SelectedIndex >= 0)
                _scene.SetRepresentation(_id, Kinds[_choice.SelectedIndex]);
        };
    }

    public void Refresh()
    {
        int index = Array.IndexOf(Kinds, _scene.GetRepresentation(_id));
        if (_choice.SelectedIndex != index) _choice.SelectedIndex = index;
    }
}