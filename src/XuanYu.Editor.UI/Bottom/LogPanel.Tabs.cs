using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media;

namespace XuanYu.Editor.UI;

public sealed partial class LogPanel
{
    private void SelectTab(string tab)
    {
        _activeTab = tab;
        foreach (var pair in _tabs)
            pair.Value.BorderBrush = pair.Key == tab
                ? EditorTheme.Accent : Brushes.Transparent;
        bool textTab = tab is "日志" or "问题" or "输出";
        _search.IsVisible = textTab;
        _level.IsVisible = tab == "日志";
        _follow.IsVisible = textTab;
        _body.Content = tab switch
        {
            "性能" => _metricsView,
            "调试" => _debugView,
            _ => _list
        };
        if (textTab) RebuildRows();
        RefreshLive();
    }

    private void ProbeChanged(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => RefreshLive();

    private async void OnListKeyDown(object? sender, KeyEventArgs e)
    {
        if (!e.KeyModifiers.HasFlag(KeyModifiers.Control)) return;
        if (e.Key == Key.A)
        {
            _list.SelectAll();
            e.Handled = true;
            return;
        }
        if (e.Key != Key.C) return;
        int[] selected = _list.Selection.SelectedIndexes.OrderBy(i => i).ToArray();
        if (selected.Length == 0) return;
        string text = string.Join(Environment.NewLine, selected.Select(i => _rows[i]));
        e.Handled = true;
        if (TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard)
            await clipboard.SetTextAsync(text);
    }
}