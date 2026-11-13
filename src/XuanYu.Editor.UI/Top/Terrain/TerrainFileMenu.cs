
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI.Top.Terrain;

internal static class TerrainFileMenu
{
    public static MenuItem[] Create(Window owner, Workbench wb)
    {
        var import = new MenuItem { Header = "导入 NASADEM（HGT / ZIP）" };
        import.Click += async (_, _) => await Import(owner, wb);

        var open = new MenuItem { Header = "打开地图（XYMAP）" };
        open.Click += async (_, _) => await Open(owner, wb, false);

        var save = new MenuItem { Header = "保存地图" };
        save.Click += async (_, _) => await Save(owner, wb, false);

        var saveAs = new MenuItem { Header = "地图另存为…" };
        saveAs.Click += async (_, _) => await Save(owner, wb, true);

        return [import, open, save, saveAs];
    }

    public static Task Import(Window owner, Workbench wb) =>
        Open(owner, wb, true);

    private static async Task Open(Window owner, Workbench wb, bool dem)
    {
        var type = new FilePickerFileType(
            dem ? "NASADEM 数据" : "XYE 地图")
        {
            Patterns = dem ? ["*.hgt", "*.zip"] : ["*.xymap"]
        };

        var files = await owner.StorageProvider.OpenFilePickerAsync(new()
        {
            Title = dem ? "导入 NASADEM" : "打开 XYE 地图",
            AllowMultiple = false,
            FileTypeFilter = [type]
        });

        string? path = files.FirstOrDefault()?.TryGetLocalPath();
        if (path is null) return;

        try
        {
            if (dem) wb.ImportTerrain(path);
            else wb.OpenTerrainMap(path);
        }
        catch (Exception ex)
        {
            wb.Log.Write($"E4 地图读取失败：{ex.Message}");
        }
    }

    private static async Task Save(Window owner, Workbench wb, bool saveAs)
    {
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

        try
        {
            wb.SaveTerrainMap(path);
        }
        catch (Exception ex)
        {
            wb.Log.Write($"E4 地图保存失败：{ex.Message}");
        }
    }
}
