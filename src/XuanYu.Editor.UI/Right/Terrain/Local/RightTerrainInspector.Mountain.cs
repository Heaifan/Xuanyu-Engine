using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System.Globalization;

namespace XuanYu.Editor.UI.Right;

internal sealed partial class RightTerrainInspector
{
    private void AddMountainControls()
    {
        TextBox seed = AddNumberInput("随机种子", 0f);
        seed.Text = _wb.TerrainNaturalPeakSeed.ToString(CultureInfo.InvariantCulture);
        var seedApply = new Button { Content = "应用随机种子", FontSize = 11 };
        seedApply.Click += (_, _) =>
        {
            if (int.TryParse(seed.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            {
                _wb.SetTerrainNaturalPeakSeed(value);
                seedApply.Content = $"种子：{value}";
            }
            else seedApply.Content = "请输入 32 位整数";
        };
        _rows.Children.Add(seedApply);
        TextBox input = AddNumberInput("山体高度 (m)", _wb.TerrainMountainHeight);
        var apply = new Button { Content = "应用山体高度", FontSize = 11 };
        void Apply()
        {
            if (float.TryParse(input.Text, NumberStyles.Float, CultureInfo.InvariantCulture,
                out float height) && float.IsFinite(height) && height > 0f)
            {
                _wb.SetTerrainMountainHeight(height);
                apply.Content = $"山体高度：{height:0.###} m";
            }
            else apply.Content = "请输入大于 0 的有限高度";
        }
        apply.Click += (_, _) => Apply();
        input.KeyDown += (_, e) =>
        {
            if (e.Key != Avalonia.Input.Key.Enter) return;
            Apply(); e.Handled = true;
        };
        _rows.Children.Add(apply);
        var generate = new Button { Content = "生成阿尔卑斯地貌（全图）", FontSize = 11 };
        generate.Click += (_, _) => _wb.GenerateAlpineTerrain();
        _rows.Children.Add(generate);
        _rows.Children.Add(new TextBlock
        {
            Text = "使用已应用的种子、高度生成全图阿尔卑斯地貌；不使用半径、衰减或自动侵蚀。可撤销。",
            FontSize = 10, TextWrapping = TextWrapping.Wrap,
            Foreground = EditorTheme.TextMuted, Margin = new Thickness(6, 4, 6, 6)
        });
        _rows.Children.Add(new TextBlock
        {
            Text = "单击成峰，按住左键拖动成脉，静止不叠高。山体高度为整笔相对隆起上限；衰减越大山体越窄。侵蚀请使用独立笔刷。",
            FontSize = 10, TextWrapping = TextWrapping.Wrap,
            Foreground = EditorTheme.TextMuted, Margin = new Thickness(6, 4, 6, 6)
        });
    }
}