using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace XuanYu.Editor.UI.Left;

internal static class LeftMenuStyle
{
    private static readonly IBrush Surface = Brush("#FBFCFE");
    private static readonly IBrush Border = Brush("#CDD8E4");
    private static readonly IBrush Text = Brush("#26374A");
    private static readonly IBrush Muted = Brush("#75869A");
    private static readonly IBrush Hover = Brush("#E6F0FB");

    public static ContextMenu Create()
    {
        var menu = new ContextMenu
        {
            Background = Surface,
            Foreground = Text,
            BorderBrush = Border,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(1),
            MinWidth = 184,
            FontFamily = new FontFamily("Microsoft YaHei UI"),
            FontSize = 12
        };
        var resources = menu.Resources;
        resources["MenuFlyoutPresenterBackground"] = Surface;
        resources["MenuFlyoutPresenterBorderBrush"] = Border;
        resources["MenuFlyoutPresenterBorderThemeThickness"] = new Thickness(1);
        resources["MenuFlyoutPresenterThemePadding"] = new Thickness(0);
        resources["MenuFlyoutScrollerMargin"] = new Thickness(0);
        resources["MenuFlyoutSubItemPopupHorizontalOffset"] = 0d;
        resources["MenuFlyoutThemeMinHeight"] = 0d;
        resources["OverlayCornerRadius"] = new CornerRadius(4);
        resources["MenuFlyoutItemBackground"] = Brushes.Transparent;
        resources["MenuFlyoutItemForeground"] = Text;
        resources["MenuFlyoutItemBackgroundPointerOver"] = Hover;
        resources["MenuFlyoutItemForegroundPointerOver"] = Text;
        resources["MenuFlyoutItemBackgroundPressed"] = Hover;
        resources["MenuFlyoutItemForegroundPressed"] = Text;
        resources["MenuFlyoutSubItemChevron"] = Muted;
        resources["MenuFlyoutSubItemChevronPointerOver"] = Text;
        resources["MenuFlyoutSubItemChevronSubMenuOpened"] = Text;
        resources["MenuFlyoutItemKeyboardAcceleratorTextForeground"] = Muted;
        resources["MenuFlyoutItemKeyboardAcceleratorTextForegroundPointerOver"] = Text;
        return menu;
    }

    public static MenuItem Item(string title, string symbol,
        KeyGesture? shortcut = null) => new()
    {
        Header = title,
        Icon = new Avalonia.Controls.Shapes.Path
        {
            Data = StreamGeometry.Parse(symbol switch
            {
                "＋" => "M8 2 V14 M2 8 H14",
                "✎" => "M3 11 L11 3 L13 5 L5 13 L2 14 Z",
                "×" => "M3 3 L13 13 M13 3 L3 13",
                _ => "M8 1 L14 4.5 V11.5 L8 15 L2 11.5 V4.5 Z " +
                     "M2 4.5 L8 8 L14 4.5 M8 8 V15"
            }),
            Width = 16, Height = 16,
            Stroke = Muted, StrokeThickness = 1.8,
            Fill = Brushes.Transparent
        },
        InputGesture = shortcut,
        Height = 24,
        MinHeight = 0,
        Padding = new Thickness(6, 1, 5, 1),
        Foreground = Text,
        FontSize = 12,
        FontFamily = new FontFamily("Microsoft YaHei UI")
    };

    public static Separator Divider() => new()
    {
        Height = 1,
        Margin = new Thickness(8, 3),
        Background = Border
    };

    private static IBrush Brush(string value) =>
        new SolidColorBrush(Color.Parse(value));
}