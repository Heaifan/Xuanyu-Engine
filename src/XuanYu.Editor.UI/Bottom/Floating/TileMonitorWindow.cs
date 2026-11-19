using Avalonia;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Threading;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.Editor.UI;

public sealed partial class TileMonitorWindow : Window
{
    private readonly Workbench _wb;
    private readonly TileMonitorPane _pane = new();
    private readonly TextBlock _stats = new()
    {
        FontSize = 11, Foreground = EditorTheme.TextSub,
        Margin = new Thickness(12, 5), TextWrapping = TextWrapping.Wrap
    };
    private readonly DispatcherTimer _timer = new()
        { Interval = TimeSpan.FromMilliseconds(200) };
    private GeoTileMonitorHistory? _history;
    private int _cursor;
    private bool _syncing;

    public TileMonitorWindow(Workbench wb)
    {
        _wb = wb;
        Title = "XYE / 瓦片监视器 · 动态回放";
        Width = 700; Height = 700;
        MinWidth = 430; MinHeight = 430;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = EditorTheme.PanelBg;
        var root = new Grid { RowDefinitions = new RowDefinitions("38,*,Auto") };
        Control toolbar = BuildTimelineControls();
        Grid.SetRow(_pane, 1);
        Grid.SetRow(_stats, 2);
        root.Children.Add(toolbar);
        root.Children.Add(_pane);
        root.Children.Add(_stats);
        Content = root;
        _timer.Tick += (_, _) => Refresh();
        Opened += (_, _) => { Refresh(); _timer.Start(); };
        Closed += (_, _) =>
        {
            _timer.Stop();
            _history?.SetRecording(false);
        };
    }
}