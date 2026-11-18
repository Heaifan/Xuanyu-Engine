using Avalonia;
using Avalonia.Controls;

namespace XuanYu.Editor.UI;

public sealed partial class EditorWindow
{
    private Grid BuildWorkspace(Control viewport)
    {
        var bottom = new LogPanel(_wb);
        var workspace = new Grid
        {
            ColumnDefinitions = new("236,6,*,6,248"),
            RowDefinitions = new("*,6,180"),
            Margin = new Thickness(4)
        };
        workspace.ColumnDefinitions[0].MinWidth = 170;
        workspace.ColumnDefinitions[2].MinWidth = 320;
        workspace.ColumnDefinitions[4].MinWidth = 190;
        workspace.RowDefinitions[0].MinHeight = 160;
        workspace.RowDefinitions[2].MinHeight = 100;
        Grid.SetColumn(_leftPanel, 0); Grid.SetRowSpan(_leftPanel, 3);
        Grid.SetColumn(viewport, 2);
        Grid.SetColumn(_rightHost, 4);
        Grid.SetColumn(bottom, 2); Grid.SetRow(bottom, 2);
        Grid.SetColumnSpan(bottom, 3);
        var leftSplit = MakeSplitter(GridResizeDirection.Columns);
        var rightSplit = MakeSplitter(GridResizeDirection.Columns);
        var bottomSplit = MakeSplitter(GridResizeDirection.Rows);
        Grid.SetColumn(leftSplit, 1); Grid.SetRowSpan(leftSplit, 3);
        Grid.SetColumn(rightSplit, 3);
        Grid.SetColumn(bottomSplit, 2); Grid.SetRow(bottomSplit, 1);
        Grid.SetColumnSpan(bottomSplit, 3);
        workspace.Children.Add(_leftPanel);
        workspace.Children.Add(viewport);
        workspace.Children.Add(_rightHost);
        workspace.Children.Add(bottom);
        workspace.Children.Add(leftSplit);
        workspace.Children.Add(rightSplit);
        workspace.Children.Add(bottomSplit);
        BindBottomFold(bottom, workspace, bottomSplit);
        return workspace;
    }

    private static GridSplitter MakeSplitter(GridResizeDirection direction) => new()
    {
        ResizeDirection = direction,
        ResizeBehavior = GridResizeBehavior.PreviousAndNext,
        ShowsPreview = true,
        Background = EditorTheme.Border
    };

    private static void BindBottomFold(LogPanel bottom, Grid grid, GridSplitter splitter)
    {
        double expandedHeight = 180d;
        bottom.FoldChanged += folded =>
        {
            RowDefinition space = grid.RowDefinitions[1];
            RowDefinition panel = grid.RowDefinitions[2];
            if (folded)
            {
                expandedHeight = Math.Max(100d, panel.ActualHeight);
                panel.MinHeight = 28d;
                panel.Height = new GridLength(28d);
                space.Height = new GridLength(0d);
                splitter.IsVisible = false;
            }
            else
            {
                panel.MinHeight = 100d;
                panel.Height = new GridLength(expandedHeight);
                space.Height = new GridLength(6d);
                splitter.IsVisible = true;
            }
        };
    }
}