using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace XuanYu.Editor.UI;

public sealed partial class TileMonitorWindow
{
    private readonly Button _record = IconButton("Transport/record", "开始录制");
    private readonly Button _previous = IconButton("Transport/previous", "上一帧");
    private readonly Button _next = IconButton("Transport/next", "下一帧");
    private readonly Button _clear = IconButton("Actions/clear", "清空录制");
    private readonly Button _export = IconButton("Actions/export", "导出诊断日志 JSONL");
    private readonly Slider _timeline = new()
        { Minimum = 0, Maximum = 0, TickFrequency = 1, IsSnapToTickEnabled = true };
    private readonly TextBlock _positionText = new()
        { Text = "0 / 0", FontSize = 11, MinWidth = 65 };

    private static Button IconButton(string name, string hint)
    {
        var button = new Button { Content = TileMonitorSvgIcons.Create(name),
            Padding = new Thickness(7, 3), MinWidth = 32 };
        ToolTip.SetTip(button, hint);
        return button;
    }

    private Control BuildTimelineControls()
    {
        var bar = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,Auto,Auto,Auto,Auto,*,Auto"),
            Margin = new Thickness(8, 3)
        };
        Control[] items = [_record, _previous, _next, _clear, _export, _timeline, _positionText];
        for (int i = 0; i < items.Length; i++)
        {
            Grid.SetColumn(items[i], i);
            items[i].VerticalAlignment = VerticalAlignment.Center;
            bar.Children.Add(items[i]);
        }
        _record.Click += (_, _) => ToggleRecording();
        _previous.Click += (_, _) => Seek(_cursor - 1);
        _next.Click += (_, _) => Seek(_cursor + 1);
        _clear.Click += (_, _) => { _history?.Clear(); _cursor = 0; Refresh(); };
        _export.Click += async (_, _) => await ExportLogAsync();
        _timeline.ValueChanged += (_, _) =>
        {
            if (!_syncing) Seek((int)Math.Round(_timeline.Value));
        };
        return bar;
    }

    private void ToggleRecording()
    {
        if (_history is not { } history) return;
        history.SetRecording(!history.IsRecording);
        if (history.IsRecording) _cursor = Math.Max(0, history.Count - 1);
        Refresh();
    }

    private void Seek(int index)
    {
        if (_history is not { Count: > 0 } history) return;
        history.SetRecording(false);
        _cursor = Math.Clamp(index, 0, history.Count - 1);
        Refresh();
    }

    private void SyncControls()
    {
        int count = _history?.Count ?? 0;
        _syncing = true;
        _timeline.Maximum = Math.Max(0, count - 1);
        _timeline.Value = count == 0 ? 0 : _cursor;
        _syncing = false;
        _timeline.IsEnabled = count > 0;
        _previous.IsEnabled = count > 0 && _cursor > 0;
        _next.IsEnabled = count > 0 && _cursor < count - 1;
        _clear.IsEnabled = count > 0;
        _export.IsEnabled = count > 0;
        _record.IsEnabled = _history is not null;
        bool recording = _history?.IsRecording == true;
        _record.Content = TileMonitorSvgIcons.Create(recording
            ? "Transport/pause" : "Transport/record");
        ToolTip.SetTip(_record, recording ? "暂停录制" : "开始录制");
        _positionText.Text = count > 0
            ? $"{_cursor + 1} / {count} · {(recording ? "录制中" : "已暂停")}" : "0 / 0";
    }
}