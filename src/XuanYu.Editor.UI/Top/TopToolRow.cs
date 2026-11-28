using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using XuanYu.Editor.Pages;
using XuanYu.Editor.UI.Top.Persistence;
using XuanYu.Editor.UI.Top.Simulation;

namespace XuanYu.Editor.UI;

internal sealed partial class TopToolRow : Border
{
    private readonly Workbench _wb;
    private readonly StackPanel _project = MakePanel(), _world = MakePanel(),
        _terrain = MakePanel(), _simulation = MakePanel(), _tools = MakePanel();
    private readonly TextBlock _context = new()
    {
        FontSize = 11, Foreground = EditorTheme.TextSub,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(8, 0, 0, 0)
    };

    public TopToolRow(Workbench wb, SceneFileCommands scene, SimulationMenu simulation,
        Action createCube, Action? createUnit = null)
    {
        _wb = wb;
        Background = EditorTheme.PanelHeaderBg;
        BorderBrush = EditorTheme.Border;
        BorderThickness = new Thickness(0, 1, 0, 0);
        Padding = new Thickness(8, 3);
        BuildProject(scene);
        BuildWorld(createCube, createUnit);
        BuildTerrain();
        BuildSimulation(simulation);
        BuildTools();
        var panels = new Grid();
        foreach (var panel in new[] { _project, _world, _terrain, _simulation, _tools })
            panels.Children.Add(panel);
        var scroll = new ScrollViewer { Content = panels,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled };
        var row = new Grid { ColumnDefinitions = new("*,Auto") };
        row.Children.Add(scroll);
        Grid.SetColumn(_context, 1);
        row.Children.Add(_context);
        Child = row;
    }

    public void Refresh()
    {
        _project.IsVisible = _wb.CurrentId == PageId.Project;
        _world.IsVisible = _wb.CurrentId == PageId.World;
        _terrain.IsVisible = _wb.CurrentId == PageId.Terrain;
        _simulation.IsVisible = _wb.CurrentId == PageId.Simulation;
        _tools.IsVisible = _wb.CurrentId == PageId.Tools;
        RefreshTerrain();
        RefreshWorkspaces();
        _context.Text = _wb.CurrentId == PageId.Terrain && !_wb.CanEditLocalTerrain
            ? "地形预览 · 不可雕刻" : _wb.Current.Title;
    }

    private static StackPanel MakePanel() => new()
    {
        Orientation = Orientation.Horizontal, Spacing = 4,
        VerticalAlignment = VerticalAlignment.Center
    };

    private static Button Add(StackPanel panel, string label, Action? action = null)
    {
        var button = new Button { Content = label, FontSize = 12, Height = 28,
            Padding = new Thickness(8, 3), Focusable = false,
            BorderThickness = new Thickness(0) };
        if (action is not null) button.Click += (_, _) => action();
        button.Resources["ButtonBackgroundDisabled"] = Brushes.Transparent;
        button.Resources["ButtonBorderBrushDisabled"] = Brushes.Transparent;
        button.Resources["ButtonForegroundDisabled"] = EditorTheme.TextMuted;
        Paint(button, false);
        panel.Children.Add(button);
        return button;
    }

    private static void AddUnavailable(StackPanel panel, params string[] labels)
    {
        foreach (string label in labels)
            Add(panel, label).IsEnabled = false;
    }

    private static void Divide(StackPanel panel) => panel.Children.Add(new Border
    {
        Width = 1, Height = 20, Background = EditorTheme.Border,
        VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(3, 0)
    });

    private static void Paint(Button button, bool active)
    {
        button.Background = active ? EditorTheme.AccentSoft : EditorTheme.TabIdle;
        button.Foreground = active ? EditorTheme.Accent : EditorTheme.TextSub;
    }
}