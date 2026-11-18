using Avalonia.Controls;
using Avalonia.Platform.Storage;
using XuanYu.Editor.Pages;
using XuanYu.World.Maps;

namespace XuanYu.Editor.UI.Top.Terrain;

internal static partial class TerrainFileMenu
{
    private static async Task ExportOffline(Window owner, Workbench wb)
    {
        var selected = await owner.StorageProvider.OpenFilePickerAsync(new()
        {
            Title = "选择待转换的 HGT / ZIP（支持多选）",
            AllowMultiple = true,
            FileTypeFilter = [new FilePickerFileType("原始 NASADEM")
                { Patterns = ["*.hgt", "*.zip"] }]
        });
        if (selected.Count == 0) return;
        var folders = await owner.StorageProvider.OpenFolderPickerAsync(new()
        {
            Title = "选择离线 XYTerrain 文件输出目录", AllowMultiple = false
        });
        string? folder = folders.FirstOrDefault()?.TryGetLocalPath();
        if (folder is null) return;
        foreach (var file in selected)
        {
            string? input = file.TryGetLocalPath();
            if (input is null)
            {
                wb.Log.Warn("仅支持本地 NASADEM 文件。", "XYTerrain");
                return;
            }
            string output = Path.Combine(folder,
                Path.GetFileNameWithoutExtension(input) + ".xyt");
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                var result = await Task.Run(() => XYTerrainWriter.Write(input, output));
                wb.Log.WriteOutput($"G-05D-04：{Path.GetFileName(output)}，" +
                    $"原始 {result.RawBytes / 1048576d:F1} MiB → " +
                    $"压缩 {result.PackBytes / 1048576d:F1} MiB，" +
                    $"占比 {result.Ratio:P1}，{watch.Elapsed.TotalSeconds:F1}s，" +
                    $"分块 Raw/Deflate/Brotli/Delta={result.RawBlocks}/" +
                    $"{result.DeflateBlocks}/{result.BrotliBlocks}/{result.DeltaBrotliBlocks}",
                    "XYTerrain");
            }
            catch (Exception ex)
            {
                wb.Log.Error($"XYTerrain 导出失败：{ex.Message}", "XYTerrain");
                return;
            }
        }
    }
}