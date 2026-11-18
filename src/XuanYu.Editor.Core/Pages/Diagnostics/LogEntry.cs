namespace XuanYu.Editor.Pages;

public enum LogLevel { Info, Warning, Error, Debug }
public enum LogStream { Log, Output, Debug }

public sealed record LogEntry(long Sequence, DateTimeOffset Time,
    LogLevel Level, LogStream Stream, string Source, string Message)
{
    public string Display =>
        $"[{Time:HH:mm:ss}] {Level.ToString().ToUpperInvariant(),-7} {Source}  {Message}";
}