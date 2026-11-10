
using Avalonia;
using Avalonia.Controls;

namespace XuanYu.Editor.UI;

public sealed class ViewportBox : Border
{
    private readonly Win32ViewportHost _host = new();
    private string _statusNote;

    public nint ViewportHwnd => _host.Hwnd;
    public (uint Width, uint Height) PixelSize => _host.GetPixelSize();

    public event Action<nint>? HwndCreated
    {
        add => _host.HwndCreated += value;
        remove => _host.HwndCreated -= value;
    }

    public event Action? HwndDestroying
    {
        add => _host.HwndDestroying += value;
        remove => _host.HwndDestroying -= value;
    }

    public ViewportBox(string statusNote)
    {
        _statusNote = statusNote;
        Background = EditorTheme.ViewportBg;
        BorderBrush = EditorTheme.Border;
        BorderThickness = new Thickness(1);
        Margin = new Thickness(6, 0);
        Child = _host;
    }

    public void UpdateOverlay(string statusNote, string activePageTitle)
    {
        _statusNote = statusNote;
        UpdateOverlay(activePageTitle);
    }

    public void UpdateOverlay(string activePageTitle)
    {
        _host.SetPageTitle(activePageTitle);
        ToolTip.SetTip(
            this, $"{_statusNote} | 当前：{activePageTitle}");
    }
}
