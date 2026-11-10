using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace XuanYu.Editor.UI;

/// <summary>
/// 侧边栏通用信息面板（用于左侧模块树与右侧 Inspector）。
/// </summary>
public sealed class SideCard : Border
{
    private readonly TextBlock _head = new()
    {
        Foreground = EditorTheme.TextMain,
        FontWeight = FontWeight.Bold,
        FontSize = 14
    };

    private readonly TextBlock _body = new()
    {
        Foreground = EditorTheme.TextSub,
        TextWrapping = TextWrapping.Wrap,
        FontSize = 13
    };

    public SideCard(string roleTag)
    {
        Background = EditorTheme.PanelBg;
        BorderBrush = EditorTheme.Border;
        BorderThickness = new Thickness(1);
        Padding = new Thickness(12);

        var tagText = new TextBlock
        {
            Text = roleTag,
            Foreground = EditorTheme.Accent,
            FontSize = 11,
            FontWeight = FontWeight.SemiBold
        };

        Child = new StackPanel
        {
            Spacing = 8,
            Children = { tagText, _head, _body }
        };
    }

    public void SetText(string title, string detail)
    {
        _head.Text = title;
        _body.Text = detail;
    }
}