using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using XuanYu.Editor.Preview;
using XuanYu.Render;

namespace XuanYu.Editor.UI.Right;

internal sealed class UnitIconSelector
{
    private readonly PreviewScene _scene;
    private readonly Action<string> _choose;
    private readonly string? _selected;
    private readonly TextBox _search = new() { PlaceholderText = "搜索单位图标…", FontSize = 12 };
    private string _category = "全部";
    private readonly WrapPanel _tiles = new();
    private readonly TextBlock _status = new() { FontSize = 11, TextWrapping = TextWrapping.Wrap };
    private readonly Flyout _flyout;
    private readonly Dictionary<string, RenderMesh> _previews = [];

    public UnitIconSelector(PreviewScene scene, string? selected, Action<string> choose)
    {
        _scene = scene; _selected = selected; _choose = choose;
        var body = new StackPanel { Width = 330, Spacing = 6, Children =
        {
            new TextBlock { Text = "选择单位图标", FontSize = 13, FontWeight = FontWeight.SemiBold },
            _search, CreateCategories(),
            new ScrollViewer { Content = _tiles, MaxHeight = 320,
                HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled },
            _status
        }};
        _flyout = new Flyout { Content = body };
        _search.TextChanged += (_, _) => Refresh();
        Refresh();
    }

    public void Show(Button target) => _flyout.ShowAt(target);

    private Control CreateCategories()
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 3 };
        foreach (string name in new[] { "全部", "陆军", "海军", "空军", "自定义" })
        {
            var tab = new Button { Content = name, FontSize = 11, Padding = new Thickness(8, 4),
                Background = name == _category ? EditorTheme.AccentSoft : EditorTheme.TabIdle };
            tab.Click += (_, _) =>
            {
                _category = name;
                foreach (var other in row.Children.OfType<Button>())
                    other.Background = ReferenceEquals(other, tab) ? EditorTheme.AccentSoft : EditorTheme.TabIdle;
                Refresh();
            };
            row.Children.Add(tab);
        }
        return row;
    }

    private void Refresh()
    {
        _tiles.Children.Clear();
        var entries = _scene.UnitIcons.Search(_category, _search.Text ?? "");
        foreach (var entry in entries) _tiles.Children.Add(CreateTile(entry));
        _status.Text = entries.Count == 0 ? "暂无匹配图标。可在高级资源中导入 SVG。" : $"{entries.Count} 项 · 点击立即应用";
    }

    private Button CreateTile(UnitIconEntry entry)
    {
        var label = new TextBlock { Text = entry.Name, FontSize = 11, TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
        var stack = new StackPanel { Spacing = 2, Children = { label } };
        var button = new Button { Width = 96, Height = 96, Margin = new Thickness(2), Padding = new Thickness(3),
            HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center,
            Background = entry.SourcePath == _selected ? EditorTheme.AccentSoft : EditorTheme.PanelBg, Content = stack };
        try
        {
            if (!_previews.TryGetValue(entry.SourcePath, out var mesh))
            {
                // 预览不占用运行时 Catalog 的解析计数，也不上传 GPU。
                using var stream = File.OpenRead(entry.SourcePath);
                mesh = SvgMeshReader.Read("preview/" + entry.Name, stream);
                _previews.Add(entry.SourcePath, mesh);
            }
            stack.Children.Insert(0, new UnitIconPreview(mesh, 60) { HorizontalAlignment = HorizontalAlignment.Center });
            button.Click += (_, _) => { _flyout.Hide(); _choose(entry.SourcePath); };
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or InvalidDataException or System.Xml.XmlException or ArgumentException)
        {
            button.IsEnabled = false;
            label.Text += "\n不可用";
            ToolTip.SetTip(button, error.Message);
        }
        return button;
    }
}