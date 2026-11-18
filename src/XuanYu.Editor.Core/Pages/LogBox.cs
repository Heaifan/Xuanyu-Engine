namespace XuanYu.Editor.Pages;

public sealed class LogBox
{
    public const int Capacity = 2000;
    private readonly List<string> _items = [];
    private readonly List<LogEntry> _entries = [];
    private long _sequence;

    public IReadOnlyList<string> Items => _items;
    public IReadOnlyList<LogEntry> Records => _entries;
    public event Action? Updated;

    public void Write(string text) =>
        Add(text, LogLevel.Info, LogStream.Log, "编辑器");

    public void Warn(string text, string source = "编辑器") =>
        Add(text, LogLevel.Warning, LogStream.Log, source);

    public void Error(string text, string source = "编辑器") =>
        Add(text, LogLevel.Error, LogStream.Log, source);

    public void WriteOutput(string text, string source = "任务") =>
        Add(text, LogLevel.Info, LogStream.Output, source);

    public void WriteDebug(string text, string source = "调试") =>
        Add(text, LogLevel.Debug, LogStream.Debug, source);

    private void Add(string text, LogLevel level, LogStream stream, string source)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("日志内容不能为空。", nameof(text));
        DateTimeOffset now = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8));
        string message = text.Trim();
        _entries.Add(new LogEntry(++_sequence, now, level, stream, source, message));
        _items.Add($"[{now:HH:mm:ss}] {message}");
        if (_entries.Count > Capacity)
        {
            _entries.RemoveAt(0);
            _items.RemoveAt(0);
        }
        Updated?.Invoke();
    }
}