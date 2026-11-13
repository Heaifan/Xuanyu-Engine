using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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
        var content = new Grid { ColumnDefinitions = new("14,18,*") };
        _chevron.RenderTransformOrigin = RelativePoint.Center;
        _chevron.RenderTransform = _chevronRotation;
        _chevron.VerticalAlignment = VerticalAlignment.Center;
        content.Children.Add(_chevron);
        Control icon = LeftSvgIcon.Create("Terrain/terrain", 16);
        Grid.SetColumn(icon, 1);
        content.Children.Add(icon);
        Grid.SetColumn(_mapText, 2);
        content.Children.Add(_mapText);
        return MakeRow(content, 0, false, () =>
        {
            _expanded = !_expanded;
            _wb.SelectTerrain(false);
            Refresh();
        });
    }

    private Border BuildDemRow()
    {
        var content = new Grid { ColumnDefinitions = new("18,*") };
        content.Children.Add(LeftSvgIcon.Create("Terrain/heightfield", 16));
        Grid.SetColumn(_demText, 1);
        content.Children.Add(_demText);
        return MakeRow(content, 26, true, () => _wb.SelectTerrain(true));
    }

    private Border MakeRow(Control content, double indent,
        bool selectDem, Action action)
    {
        var row = new Border
        {
            Height = 28, ClipToBounds = true,
            Padding = new Thickness(indent, 0, 2, 0),
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
