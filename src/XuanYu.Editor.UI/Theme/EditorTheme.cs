
using Avalonia.Media;

namespace XuanYu.Editor.UI;

public static class EditorTheme
{
    public static readonly IBrush WindowBg = Make("#F3F6FA");
    public static readonly IBrush PanelBg = Make("#FFFFFF");
    public static readonly IBrush PanelHeaderBg = Make("#F0F5FA");
    public static readonly IBrush ViewportBg = Make("#FAFCFE");
    public static readonly IBrush Border = Make("#DCE5EE");
    public static readonly IBrush Accent = Make("#2078DA");
    public static readonly IBrush AccentSoft = Make("#E7F1FD");
    public static readonly IBrush TabIdle = Make("#F3F6FA");
    public static readonly IBrush TextMain = Make("#26374A");
    public static readonly IBrush TextSub = Make("#52677D");
    public static readonly IBrush TextMuted = Make("#7A8DA2");

    private static IBrush Make(string hex)
        => new SolidColorBrush(Color.Parse(hex));
}
