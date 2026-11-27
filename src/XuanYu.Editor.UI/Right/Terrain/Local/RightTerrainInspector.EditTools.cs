using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System.Globalization;
using XuanYu.Editor.TerrainEditing;

namespace XuanYu.Editor.UI.Right;

internal sealed partial class RightTerrainInspector
{
    private void AddEditToolSummary()
    {
        _rows.Children.Add(new Border
        {
            Background = EditorTheme.PanelHeaderBg,
            Padding = new Thickness(7, 6),
            Margin = new Thickness(0, 6, 0, 4),
            Child = new TextBlock
            {
                Text = "笔刷设置 / TERRAIN EDIT",
                FontSize = 12, FontWeight = FontWeight.SemiBold,
                Foreground = EditorTheme.TextMain
            }
        });
        AddPair("当前笔刷", _wb.IsTerrainElevationTool ? "指定高程" :
            _wb.IsTerrainSmoothTool ? "平滑笔刷" : _wb.IsTerrainFlattenTool ? "整平笔刷" :
            _wb.IsTerrainMountainTool ? "自然造山" : _wb.IsTerrainThermalTool ? "热力崩塌" :
            _wb.IsTerrainRainTool ? "水力侵蚀" : "基础雕刻");
        var settings = _wb.TerrainBrush;
        TextBox radius = AddNumberInput("半径 (m)", settings.RadiusMeters);
        TextBox? strength = _wb.IsTerrainMountainTool ? null :
            AddNumberInput("强度 (%)", settings.Strength * 100f);
        TextBox falloff = AddNumberInput("衰减 (%)", settings.Falloff * 100f);
        if (_wb.IsTerrainElevationTool) { strength!.IsEnabled = false; AddElevationControls(); }
        if (_wb.IsTerrainMountainTool) AddMountainControls();
        if (_wb.IsTerrainThermalTool) AddPair("休止角", "35°（固定）");
        if (_wb.IsTerrainRainTool) AddPair("侵蚀模型", $"雨滴粒子；3000 次/s；目标行程 {TerrainRainErosion.DefaultTravelMeters:0}m / 上限 {TerrainRainErosion.MaxMovingSteps} 步");
        var apply = new Button { Content = "应用笔刷参数", FontSize = 11 };
        var status = new TextBlock
        {
            Text = "1 雕刻 / 2 平滑 / 3 整平 / 4 定高 / E 输入 / 5 自然造山 / 6 热力崩塌 / 7 水力侵蚀；F 半径、Shift+F 强度、Alt+F 衰减；Alt+左键吸取。Ctrl+Z 撤销。整平锁定起笔高度；定高、自然造山忽略通用强度；自然造山禁用 Shift+F；定高单点固定全衰减。热力：按住左键迁移陡坡物质。水力：左键单击、长按或拖动，冲刷并沉积泥沙。",
            FontSize = 10, TextWrapping = TextWrapping.Wrap,
            Foreground = EditorTheme.TextMuted, Margin = new Thickness(6, 4, 6, 6)
        };
        apply.Click += (_, _) => ApplyBrushSettings(radius.Text,
            strength?.Text ?? (_wb.TerrainBrush.Strength * 100f).ToString(CultureInfo.InvariantCulture), falloff.Text, status);
        _rows.Children.Add(apply);
        _rows.Children.Add(status);
    }
    private void AddElevationControls()
    {
        TextBox target = AddNumberInput("目标高程 (m)", _wb.TerrainTargetElevation);
        target.Tag = "TerrainTargetElevationInput";
        var area = new CheckBox { Content = "区域定高", IsChecked = _wb.TerrainElevationArea };
        area.PropertyChanged += (_, e) =>
        {
            if (e.Property == Avalonia.Controls.Primitives.ToggleButton.IsCheckedProperty)
                _wb.SetTerrainElevation(_wb.TerrainTargetElevation, area.IsChecked == true);
        };
        var apply = new Button { Content = "应用目标高程", FontSize = 11 };
        void Apply()
        {
            if (float.TryParse(target.Text, NumberStyles.Float, CultureInfo.InvariantCulture,
                out float z) && float.IsFinite(z))
            {
                _wb.SetTerrainElevation(z, area.IsChecked == true);
                apply.Content = $"目标：{z:0.###} m";
            }
            else apply.Content = "请输入有效高程";
        }
        apply.Click += (_, _) => Apply();
        target.KeyDown += (_, e) =>
        {
            if (e.Key != Avalonia.Input.Key.Enter) return;
            Apply(); e.Handled = true;
        };
        _rows.Children.Add(area);
        _rows.Children.Add(apply);
    }

    private void ApplyBrushSettings(string? radius, string? strength,
        string? falloff, TextBlock status)
    {
        if (!ReadNumbers(radius, strength, falloff,
                out float r, out float s, out float f))
        {
            status.Text = "请输入有效数字（用 . 表示小数点）。";
            return;
        }
        try
        {
            _wb.SetTerrainBrush(new TerrainBrushSettings(r, _wb.IsTerrainMountainTool ? _wb.TerrainBrush.Strength : s / 100f, f / 100f));
            status.Text = _wb.IsTerrainMountainTool ? $"已应用：R {r:0.###}m，衰减 {f:0.##}%；山峰忽略强度。" : $"已应用：R {r:0.###}m，强度 {s:0.##}%，衰减 {f:0.##}%。";
        }
        catch (ArgumentOutOfRangeException)
        {
            status.Text = "半径必须大于 0；强度、衰减须在 0～100% 内。";
        }
    }
}