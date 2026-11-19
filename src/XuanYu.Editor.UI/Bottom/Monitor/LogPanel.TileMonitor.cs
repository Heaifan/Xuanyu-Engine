using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace XuanYu.Editor.UI;

public sealed partial class LogPanel
{
    private readonly Grid _performanceView = new()
        { RowDefinitions = new RowDefinitions("34,*") };
    private TileMonitorWindow? _tileWindow;

    private void BuildTileMonitorLayout()
    {
        var open = new Button
        {
            Content = "打开瓦片监视器 ↗", FontSize = 11,
            Padding = new Thickness(10, 3),
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(8, 2, 0, 2)
        };
        open.Click += (_, _) => OpenTileMonitor();
        _performanceView.Children.Add(open);
        Grid.SetRow(_metricsView, 1);
        _performanceView.Children.Add(_metricsView);
    }

    private void OpenTileMonitor()
    {
        if (_tileWindow is { } existing)
        {
            existing.Activate();
            return;
        }
        if (TopLevel.GetTopLevel(this) is not Window owner) return;
        var window = new TileMonitorWindow(_wb);
        _tileWindow = window;
        window.Closed += (_, _) => _tileWindow = null;
        window.Show(owner);
    }
}