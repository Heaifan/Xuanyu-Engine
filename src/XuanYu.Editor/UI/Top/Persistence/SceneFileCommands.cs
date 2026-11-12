using Avalonia.Controls;
using Avalonia.Platform.Storage;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview.Persistence;

namespace XuanYu.Editor.UI.Top.Persistence;

internal sealed class SceneFileCommands(Window owner, Workbench wb)
{
    private static readonly FilePickerFileType FileType = new("玄域场景")
    {
        Patterns = ["*.xyscene"]
    };
    private string? _path;

    public Task OpenAsync() => RunAsync(OpenCoreAsync, "打开");
    public Task SaveAsync() => RunAsync(SaveCoreAsync, "保存");
    public Task SaveAsAsync() => RunAsync(SaveAsCoreAsync, "另存为");

    private async Task OpenCoreAsync()
    {
        var files = await owner.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "打开 .xyscene 场景",
                AllowMultiple = false,
                FileTypeFilter = [FileType]
            });
        if (files.Count == 0)
            return;
        string path = LocalPath(files[0]);
        SceneFile.Load(wb.Scene, path);
        _path = path;
        wb.Log.Write($"场景已加载：{path}");
    }

    private async Task SaveCoreAsync()
    {
        if (_path is null)
        {
            await SaveAsCoreAsync();
            return;
        }
        SceneFile.Save(wb.Scene, _path);
        wb.Log.Write($"场景已保存：{_path}");
    }

    private async Task SaveAsCoreAsync()
    {
        var file = await owner.StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = "保存 .xyscene 场景",
                SuggestedFileName = "Scene.xyscene",
                DefaultExtension = "xyscene",
                FileTypeChoices = [FileType],
                ShowOverwritePrompt = true
            });
        if (file is null)
            return;
        string path = LocalPath(file);
        SceneFile.Save(wb.Scene, path);
        _path = path;
        wb.Log.Write($"场景已保存：{path}");
    }

    private static string LocalPath(IStorageFile file) =>
        file.TryGetLocalPath() ??
        throw new NotSupportedException("当前阶段仅支持本地场景路径。");

    private async Task RunAsync(Func<Task> action, string operation)
    {
        try
        {
            await action();
        }
        catch (Exception error)
        {
            wb.Log.Write($"场景{operation}失败：{error.Message}");
        }
    }
}