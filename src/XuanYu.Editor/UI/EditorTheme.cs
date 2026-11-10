using Avalonia.Media;

namespace XuanYu.Editor.UI;

/// <summary>
/// 编辑器深色界面配色表。
/// </summary>
public static class EditorTheme
{
    public static readonly IBrush WindowBg = new SolidColorBrush(Color.Parse("#14171A"));
    public static readonly IBrush PanelBg = new SolidColorBrush(Color.Parse("#1E2227"));
    public static readonly IBrush ViewportBg = new SolidColorBrush(Color.Parse("#0D1014"));
    public static readonly IBrush Border = new SolidColorBrush(Color.Parse("#2E343B"));
    public static readonly IBrush Accent = new SolidColorBrush(Color.Parse("#2B6CB0"));
    public static readonly IBrush TabIdle = new SolidColorBrush(Color.Parse("#252A30"));
    public static readonly IBrush TextMain = new SolidColorBrush(Color.Parse("#E2E8F0"));
    public static readonly IBrush TextSub = new SolidColorBrush(Color.Parse("#94A3B8"));
}