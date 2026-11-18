using Avalonia.Threading;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI;

public sealed partial class LogPanel
{
    private void OnLogUpdated()
    {
        if (_queued) return;
        _queued = true;
        Dispatcher.UIThread.Post(() =>
        {
            _queued = false;
            if (_activeTab is "日志" or "问题" or "输出") RefreshRows();
        }, DispatcherPriority.Background);
    }

    private void RebuildRows()
    {
        if (_activeTab is "性能" or "调试") return;
        _rows.Clear();
        _lastSequence = 0;
        RefreshRows();
    }

    private void RefreshRows()
    {
        if (_activeTab == "问题")
        {
            RefreshProblems();
            return;
        }
        foreach (LogEntry entry in _log.Records)
        {
            if (entry.Sequence <= _lastSequence) continue;
            _lastSequence = entry.Sequence;
            if (Matches(entry)) _rows.Add(entry.Display);
        }
        while (_rows.Count > LogBox.Capacity) _rows.RemoveAt(0);
        FollowLast();
    }

    private bool Matches(LogEntry entry)
    {
        if ((_activeTab == "日志" && entry.Stream != LogStream.Log) ||
            (_activeTab == "输出" && entry.Stream != LogStream.Output)) return false;
        if (_activeTab == "日志" && _level.SelectedIndex is > 0 and <= 4 &&
            entry.Level != (LogLevel)(_level.SelectedIndex - 1)) return false;
        string query = _search.Text?.Trim() ?? "";
        return query.Length == 0 || entry.Message.Contains(query,
            StringComparison.OrdinalIgnoreCase) || entry.Source.Contains(query,
            StringComparison.OrdinalIgnoreCase);
    }

    private void RefreshProblems()
    {
        string query = _search.Text?.Trim() ?? "";
        var groups = _log.Records
            .Where(e => e.Level is LogLevel.Error or LogLevel.Warning)
            .Where(e => query.Length == 0 || e.Message.Contains(query,
                StringComparison.OrdinalIgnoreCase) || e.Source.Contains(query,
                StringComparison.OrdinalIgnoreCase))
            .GroupBy(e => (e.Level, e.Source, e.Message))
            .OrderBy(g => g.Last().Sequence);
        _rows.Clear();
        foreach (var group in groups)
            _rows.Add($"{group.Last().Display}  ×{group.Count()}");
        FollowLast();
    }

    private void FollowLast()
    {
        if (_follow.IsChecked == true && _rows.Count > 0)
            _list.ScrollIntoView(_rows.Count - 1);
    }
}