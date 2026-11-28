using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using XuanYu.Editor.Preview;

namespace XuanYu.Editor.UI.Right;

internal sealed class RightOrganizationEditor : StackPanel
{
    private readonly PreviewScene _scene;
    private readonly ComboBox _parents = new()
    {
        MinHeight = 0, Height = 24, MinWidth = 0, FontSize = 11,
        Padding = new Thickness(5, 0), HorizontalAlignment = HorizontalAlignment.Stretch,
        Background = EditorTheme.PanelHeaderBg, BorderBrush = EditorTheme.Border,
        BorderThickness = new Thickness(1), Foreground = EditorTheme.TextMain
    };
    private readonly TextBlock _depth = Value(), _direct = Value(), _all = Value();
    private IReadOnlyList<OrganizationParentOption> _options = [];
    private bool _refreshing;
    public Guid EntityId { get; }

    public RightOrganizationEditor(PreviewScene scene, Guid id)
    {
        _scene = scene;
        EntityId = id;
        Spacing = 1;
        AddRow("直属上级", _parents, 28);
        AddRow("当前层级", _depth);
        AddRow("直属下属", _direct);
        AddRow("全部下属", _all);
        _parents.SelectionChanged += (_, _) => Apply();
        Refresh();
    }

    public void Refresh()
    {
        var info = _scene.GetOrganizationEditingInfo(EntityId);
        _refreshing = true;
        try
        {
            if (!_options.SequenceEqual(info.Parents))
            {
                _options = info.Parents;
                _parents.ItemsSource = _options;
            }
            var selected = _options.Single(option => option.Id == info.ParentId);
            if (!Equals(_parents.SelectedItem, selected)) _parents.SelectedItem = selected;
            _depth.Text = info.Depth == 0 ? "第 0 层（根实体）" : $"第 {info.Depth} 层";
            _direct.Text = $"{info.DirectChildren} 个实体";
            _all.Text = $"{info.Descendants} 个实体";
        }
        finally { _refreshing = false; }
    }

    private void Apply()
    {
        if (_refreshing || _parents.SelectedItem is not OrganizationParentOption parent) return;
        _scene.SetOrganizationParent(EntityId, parent.Id);
    }

    private void AddRow(string title, Control value, double height = 24)
    {
        var row = new Grid { Height = height, Margin = new Thickness(8, 0),
            ColumnDefinitions = new ColumnDefinitions("2*,3*") };
        row.Children.Add(new TextBlock { Text = title, FontSize = 12,
            Foreground = EditorTheme.TextSub, VerticalAlignment = VerticalAlignment.Center });
        Grid.SetColumn(value, 1);
        row.Children.Add(value);
        Children.Add(row);
    }

    private static TextBlock Value() => new()
    {
        FontSize = 12, Foreground = EditorTheme.TextMain,
        VerticalAlignment = VerticalAlignment.Center,
        TextTrimming = TextTrimming.CharacterEllipsis, Margin = new Thickness(6, 0, 0, 0)
    };
}