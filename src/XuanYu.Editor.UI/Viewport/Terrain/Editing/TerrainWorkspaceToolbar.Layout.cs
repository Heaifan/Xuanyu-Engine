using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using XuanYu.Editor.Pages;
using SvgPath = Avalonia.Controls.Shapes.Path;

namespace XuanYu.Editor.UI;

internal sealed partial class TerrainWorkspaceToolbar
{
    private void BuildTools()
    {
        AddTool(TerrainToolGroup.Generation, "自然造山 [5]",
            "M 2 20 L 9 6 L 13 12 L 17 3 L 22 20 M 9 6 L 9 15 L 5 20 M 17 3 L 17 13 L 13 20",
            _wb.SelectTerrainMountainTool, () => _wb.IsTerrainMountainTool);
        AddTool(TerrainToolGroup.Sculpt, "抬升/降低 [1]",
            "M 2 19 L 8 8 L 12 13 L 17 4 L 22 19 Z",
            () => _wb.SelectTerrainElevationTool(false), () => !_wb.IsTerrainElevationTool &&
                !_wb.IsTerrainSmoothTool && !_wb.IsTerrainFlattenTool);
        AddTool(TerrainToolGroup.Sculpt, "平滑 [2]",
            "M 2 9 C 6 5 9 5 12 9 C 15 13 18 13 22 9 M 2 15 C 6 11 9 11 12 15 C 15 19 18 19 22 15",
            _wb.SelectTerrainSmoothTool, () => _wb.IsTerrainSmoothTool);
        AddTool(TerrainToolGroup.Sculpt, "整平 [3]",
            "M 3 7 H 21 M 3 12 H 21 M 3 17 H 21",
            _wb.SelectTerrainFlattenTool, () => _wb.IsTerrainFlattenTool);
        AddTool(TerrainToolGroup.Sculpt, "指定高程 [4/E]",
            "M 3 21 L 12 3 L 21 21 M 12 3 V 21",
            () => _wb.SelectTerrainElevationTool(true), () => _wb.IsTerrainElevationTool);
        AddTool(TerrainToolGroup.Evolution, "热力崩塌 [6]",
            "M 2 20 L 10 4 L 22 20 M 12 9 L 16 14 M 12 14 H 16 V 10 M 18 17 L 19 18",
            _wb.SelectTerrainThermalTool, () => _wb.IsTerrainThermalTool);
        AddTool(TerrainToolGroup.Evolution, "水力侵蚀 [7]",
            "M 2 21 L 10 9 L 22 21 M 10 9 L 11 14 L 9 17 L 14 21 M 5 2 L 3 6 M 12 2 L 10 6 M 19 2 L 17 6",
            _wb.SelectTerrainRainTool, () => _wb.IsTerrainRainTool);
    }

    private void AddTool(TerrainToolGroup group, string label, string path, Action action, Func<bool> selected)
    {
        var icon = new SvgPath { Width = 18, Height = 18, Stretch = Stretch.Uniform,
            Stroke = EditorTheme.TextSub, StrokeThickness = 1.7, Fill = Brushes.Transparent,
            Data = PathGeometry.Parse(path) };
        var button = new Button { Width = 32, Height = 32, Padding = new Thickness(0),
            Content = icon, Focusable = false, BorderThickness = new Thickness(0),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center };
        ToolTip.SetTip(button, label);
        button.Click += (_, _) => action();
        _tools.Children.Add(button);
        _toolButtons.Add((group, button, selected));
    }
}