
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace XuanYu.Editor.UI.Right;

public class RightWorkspacePanel : Border
{
    private readonly StackPanel _body = new()
    {
        Spacing = 4, HorizontalAlignment = HorizontalAlignment.Stretch
    };
    private readonly Dictionary<string, bool> _expanded = [];
    private readonly Func<string, int, Control>? _iconFactory;

    public RightWorkspacePanel(Func<string, int, Control>? iconFactory = null)
    {
        _iconFactory = iconFactory;
        Background = EditorTheme.PanelBg;
        BorderBrush = EditorTheme.Border;
        BorderThickness = new Thickness(1);
        var heading = new Border
        {
            Height = 24, Background = EditorTheme.PanelHeaderBg,
            BorderBrush = EditorTheme.Border,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(8, 0),
            Child = new TextBlock
            {
                Text = "INSPECTOR / 属性检查器", FontSize = 12,
                Foreground = EditorTheme.TextMain,
                VerticalAlignment = VerticalAlignment.Center
            }
        };
        var scroll = new ScrollViewer
        {
            Content = _body, HorizontalContentAlignment = HorizontalAlignment.Stretch,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
        var root = new DockPanel { LastChildFill = true };
        DockPanel.SetDock(heading, Dock.Top);
        root.Children.Add(heading);
        root.Children.Add(scroll);
        Child = root;
        Show(null);
    }

    public void Show(RightInspectorSnapshot? snapshot, Action<bool>? setVisible = null)
    {
        _body.Children.Clear();
        if (snapshot is null)
        {
            _body.Children.Add(new TextBlock
            {
                Text = "未选中对象", FontSize = 12,
                Foreground = EditorTheme.TextMuted,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(8, 28)
            });
            return;
        }
        _body.Children.Add(new RightObjectHeader(snapshot, _iconFactory));
        foreach (var section in snapshot.Sections)
        {
            var group = CreateGroup(section.Title);
            foreach (var field in section.Fields)
                group.AddText(field.Name, field.Value);
            _body.Children.Add(group);
        }
        if (snapshot.Visible is bool visible && setVisible is not null)
        {
            var display = CreateGroup("显示设置");
            display.AddVisibility(visible, setVisible);
            _body.Children.Add(display);
        }
    }

    private RightPropertyGroup CreateGroup(string title)
    {
        var group = new RightPropertyGroup(title, _iconFactory);
        if (_expanded.TryGetValue(title, out bool expanded))
            group.SetExpanded(expanded);
        group.ExpandedChanged += value => _expanded[title] = value;
        return group;
    }
}
