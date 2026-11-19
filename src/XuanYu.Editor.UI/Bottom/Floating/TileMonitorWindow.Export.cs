using Avalonia.Controls;
using Avalonia.Platform.Storage;
using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.Editor.UI;

public sealed partial class TileMonitorWindow
{
    private async Task ExportLogAsync()
    {
        if (_history is not { Count: > 0 } history) return;
        var file = await StorageProvider.SaveFilePickerAsync(new()
        {
            Title = "导出瓦片监视诊断日志",
            SuggestedFileName = $"XYE-TileMonitor-{DateTime.Now:yyyyMMdd-HHmmss}.jsonl",
            DefaultExtension = "jsonl",
            FileTypeChoices = [new FilePickerFileType("JSONL 诊断日志")
                { Patterns = ["*.jsonl"] }]
        });
        if (file is null) return;
        GeoTileMonitorFrame[] frames = Enumerable.Range(0, history.Count)
            .Select(i => history[i]).ToArray();
        try
        {
            await using Stream stream = await file.OpenWriteAsync();
            await GeoTileMonitorLogExporter.WriteAsync(stream, frames);
            _wb.Log.Write($"G06：已导出 {frames.Length} 帧诊断日志：{file.Name}");
        }
        catch (Exception error)
        {
            _wb.Log.Error($"瓦片日志导出失败：{error.Message}", "G06");
        }
    }
}