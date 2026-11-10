using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace XuanYu.Editor.UI;

/// <summary>
/// 中央常驻 Vulkan 1.3 视口容器。
/// </summary>
public sealed class ViewportBox : Border
{
    private readonly TextBlock _pageLabel = new()
    {
        Foreground = EditorTheme.TextSub,
        HorizontalAlignment = HorizontalAlignment.Center,
        FontSize = 13
    };

    public ViewportBox(string statusNote)
    {
        Background = EditorTheme.ViewportBg;
        BorderBrush = EditorTheme.Border;
        BorderThickness = new Thickness(1);
        Margin = new Thickness(6, 0);

        var title = new TextBlock
        {
            Text = statusNote,
            Foreground = EditorTheme.TextMain,
            FontWeight = FontWeight.Bold,
            FontSize = 15,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        Child = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 8,
            Children = { title, _pageLabel }
        };
    }

    public void UpdateOverlay(string activePageTitle)
    {
        _pageLabel.Text = $"当前工作区上下文: {activePageTitle} | 视口实例保持常驻";
    }
}