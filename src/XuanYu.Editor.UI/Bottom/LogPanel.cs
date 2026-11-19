using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI;

public sealed partial class LogPanel : Border
{
    private readonly Workbench _wb;
    private readonly LogBox _log;
    private readonly ObservableCollection<string> _rows = [];
    private readonly ListBox _list;
    private readonly ContentControl _body = new();
    private readonly ScrollViewer _metricsView = new();
    private readonly ScrollViewer _debugView = new();
    private readonly SelectableTextBlock _metrics = new()
    {
        FontFamily = new FontFamily("Consolas"), FontSize = 12,
        Foreground = EditorTheme.TextMain, TextWrapping = TextWrapping.Wrap
    };
    private readonly SelectableTextBlock _debugText = new()
    {
        FontFamily = new FontFamily("Consolas"), FontSize = 12,
        Foreground = EditorTheme.TextMain, TextWrapping = TextWrapping.Wrap
    };
    private readonly TextBox _search = new()
        { PlaceholderText = "搜索当前标签", Width = 142, FontSize = 11 };
    private readonly ComboBox _level = new() { Width = 76, FontSize = 11 };
    private readonly CheckBox _follow = new()
        { Content = "跟随", IsChecked = true, FontSize = 11 };
    private readonly CheckBox _probe = new()
        { Content = "启用探针", FontSize = 12 };
    private readonly Button _fold = new() { Content = "收起", FontSize = 11 };
    private readonly Dictionary<string, Border> _tabs = [];
    private readonly DispatcherTimer _timer = new()
        { Interval = TimeSpan.FromSeconds(1) };
    private long _lastSequence;
    private bool _queued;
    private bool _folded;
    private string _activeTab = "日志";

    public event Action<bool>? FoldChanged;

    public LogPanel(Workbench wb)
    {
        _wb = wb;
        _log = wb.Log;
        Background = EditorTheme.PanelBg;
        BorderBrush = EditorTheme.Border;
        BorderThickness = new Thickness(1);
        _list = new ListBox
        {
            ItemsSource = _rows, FontFamily = new FontFamily("Consolas"),
            FontSize = 11, Background = EditorTheme.PanelBg,
            BorderThickness = new Thickness(0),
            SelectionMode = SelectionMode.Multiple
        };
        _list.Styles.Add(new Style(x => x.OfType<ListBoxItem>())
        {
            Setters =
            {
                new Setter(ListBoxItem.HeightProperty, 26d),
                new Setter(ListBoxItem.MinHeightProperty, 0d),
                new Setter(ListBoxItem.PaddingProperty, new Thickness(6, 0)),
                new Setter(ListBoxItem.MarginProperty, new Thickness(0))
            }
        });
        _list.KeyDown += OnListKeyDown;
        _metricsView.Content = _metrics;
        _metricsView.Padding = new Thickness(12, 8);
        BuildTileMonitorLayout();
        var debugContent = new StackPanel { Spacing = 10,
            Margin = new Thickness(12, 8) };
        debugContent.Children.Add(_probe);
        debugContent.Children.Add(_debugText);
        _debugView.Content = debugContent;
        Child = BuildLayout();
        _timer.Tick += (_, _) => RefreshLive();
        AttachedToVisualTree += (_, _) =>
        {
            _log.Updated += OnLogUpdated;
            _timer.Start();
            RebuildRows();
        };
        DetachedFromVisualTree += (_, _) =>
        {
            _log.Updated -= OnLogUpdated;
            _timer.Stop();
        };
        SelectTab("日志");
    }
}