using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace XuanYu.Editor.UI.Left;

internal sealed partial class LeftTerrainPanel
{
    private static readonly IBrush SelectedBrush =
        new SolidColorBrush(Color.Parse("#E7F1FD"));
    private static readonly IBrush HoverBrush =
        new SolidColorBrush(Color.Parse("#F6F9FC"));

    private Border BuildMapRow()
    {
        var grid = new Grid { ColumnDefinitions = new("Auto,17,21,*") };
        grid.Children.Add(new LeftTreeGuide(0, [], true));
        _chevron.RenderTransformOrigin = RelativePoint.Center;
        _chevron.RenderTransform = _chevronRotation;
        _chevron.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(_chevron, 1);
        grid.Children.Add(_chevron);
        Control icon = LeftSvgIcon.Create("Terrain/terrain", 16);
        Grid.SetColumn(icon, 2);
        grid.Children.Add(icon);
        Grid.SetColumn(_mapText, 3);
        grid.Children.Add(_mapText);
        return MakeRow(grid, false, () =>
        {
            _expanded = !_expanded;
            _wb.SelectTerrain(false);
        });
    }

    private Border BuildDemRow()
    {
        var grid = new Grid { ColumnDefinitions = new("Auto,5,21,*") };
        grid.Children.Add(new LeftTreeGuide(1, [], true, isLeaf: true));
        var expander = new Border { Width = 5 };
        Grid.SetColumn(expander, 1);
        grid.Children.Add(expander);
        Control icon = LeftSvgIcon.Create("Terrain/heightfield", 16);
        Grid.SetColumn(icon, 2);
        grid.Children.Add(icon);
        Grid.SetColumn(_demText, 3);
        grid.Children.Add(_demText);
        return MakeRow(grid, true, () => _wb.SelectTerrain(true));
    }

    private Border MakeRow(Control content, bool selectDem, Action action)
    {
        var row = new Border
        {
            Height = 28, ClipToBounds = true,
            Padding = new Thickness(0),
            Background = Brushes.Transparent, Child = content,
            ContextMenu = CreateTerrainMenu()
        };
        row.PointerPressed += (_, e) =>
        {
            var point = e.GetCurrentPoint(row).Properties;
            if (point.IsRightButtonPressed)
            {
                _wb.SelectTerrain(selectDem);
                Refresh();
                return;
            }
            if (!point.IsLeftButtonPressed)
                return;
            action();
            Refresh();
            e.Handled = true;
        };
        row.PointerEntered += (_, _) => Refresh();
        row.PointerExited += (_, _) => Refresh();
        return row;
    }
}