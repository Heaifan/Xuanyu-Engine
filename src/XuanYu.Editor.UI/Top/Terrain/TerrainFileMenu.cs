using Avalonia.Controls;
using Avalonia.Platform.Storage;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI.Top.Terrain;

internal static partial class TerrainFileMenu
{
    public static MenuItem[] Create(Window owner, Workbench wb)
    {
        var import = new MenuItem { Header = "导入 NASADEM（支持多选）…" };
        import.Click += async (_, _) => await Import(owner, wb);
        var export = new MenuItem { Header = "导出离线地形包（XYTerrain）…" };
        export.Click += async (_, _) => await ExportOffline(owner, wb);
        var open = new MenuItem { Header = "打开地图（XYMAP）" };
        open.Click += async (_, _) => await Open(owner, wb);
        var save = new MenuItem { Header = "保存地图" };
        save.Click += async (_, _) => await Save(owner, wb, false);
        var saveAs = new MenuItem { Header = "地图另存为…" };
        saveAs.Click += async (_, _) => await Save(owner, wb, true);
        return [import, export, open, save, saveAs];
    }

    public static async Task Import(Window owner, Workbench wb)
    {
        var files = await owner.StorageProvider.OpenFilePickerAsync(new()
        {
            Title = "选择 NASADEM 文件（可多选）",
            AllowMultiple = true,
            FileTypeFilter = [new FilePickerFileType("地形数据")
                { Patterns = ["*.hgt", "*.zip", "*.xyt"] }]
        });
        if (files.Count == 0) return;
        var paths = new List<string>();
        foreach (var file in files)
        {
            if (file.TryGetLocalPath() is not { } path)
            {
                wb.Log.Warn("G-05BR：请选择本地 HGT / ZIP / XYT 文件。", "地图导入");
                return;
            }
            paths.Add(path);
        }
        wb.Log.Write($"G-05BR：开始导入 {paths.Count} 张 NASADEM 文件");
        try { wb.ImportTerrainFiles(paths); }
        catch (Exception ex) { wb.Log.Error($"NASADEM 导入失败：{ex.Message}", "地图导入"); }
    }

    private static async Task Open(Window owner, Workbench wb)
    {
        var files = await owner.StorageProvider.OpenFilePickerAsync(new()
        {
            Title = "打开 XYE 地图", AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType("XYE 地图") { Patterns = ["*.xymap"] }]
        });
        string? path = files.FirstOrDefault()?.TryGetLocalPath();
        if (path is null) return;
        try { wb.OpenTerrainMap(path); }
        catch (Exception ex) { wb.Log.Error($"地图读取失败：{ex.Message}", "地图读取"); }
    }

    private static async Task Save(Window owner, Workbench wb, bool saveAs)
    {
        if (wb.TerrainMultiSourceLabel is not null)
        {
            wb.Log.Warn("多 HGT 为预览模式，暂不支持保存为单张 XYMAP。", "地图保存");
            return;
        }
        string? path = saveAs ? null : wb.TerrainMapPath;
        if (path is null)
        {
            var target = await owner.StorageProvider.SaveFilePickerAsync(new()
            {
                Title = "保存 XYE 地图",
                SuggestedFileName = wb.TerrainMap.Name,
                DefaultExtension = "xymap",
                FileTypeChoices = [new FilePickerFileType("XYE 地图")
                {
                    Patterns = ["*.xymap"]
                }]
            });
            path = target?.TryGetLocalPath();
        }
        if (path is null) return;
        try { wb.SaveTerrainMap(path); }
        catch (Exception ex) { wb.Log.Error($"地图保存失败：{ex.Message}", "地图保存"); }
    }
}