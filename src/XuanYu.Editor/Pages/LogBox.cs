namespace XuanYu.Editor.Pages;

/// <summary>
/// 编辑器底部日志盒，负责收集诊断输出。
/// </summary>
public sealed class LogBox
{
    private readonly List<string> _items = [];

    public IReadOnlyList<string> Items => _items;

    public event Action? Updated;

    public void Write(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("日志内容不能为空。", nameof(text));
        }

        _items.Add($"[{DateTime.UtcNow:HH:mm:ss}] {text.Trim()}");
        Updated?.Invoke();
    }
}