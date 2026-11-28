using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using XuanYu.Render;
using XuanYu.Editor.UI.Left;

namespace XuanYu.Editor.UI.Right;

internal sealed partial class RightRepresentationEditor
{
    private Control CreateAdvanced()
    {
        var load = new Button { Content = "绑定 SVG…" };
        load.Click += async (_, _) => await LoadAsync();
        _clear.Click += (_, _) => Run(() => _scene.ClearSymbol(_id));
        var group = new RightPropertyGroup("高级资源", (key, size) => LeftSvgIcon.Create(key, size));
        var body = FormBody();
        StyleHint(_asset);
        body.Children.Add(Field("", load));
        body.Children.Add(_asset);
        body.Children.Add(Field("", _clear));
        group.AddControl(body);
        group.SetExpanded(false);
        return group;
    }

    private void RefreshIcon(SvgAssetEntry? asset)
    {
        _shownAsset = asset?.Id;
        var row = new DockPanel { LastChildFill = true };
        var arrow = new TextBlock { Text = "⌄", VerticalAlignment = VerticalAlignment.Center };
        DockPanel.SetDock(arrow, Dock.Right); row.Children.Add(arrow);
        if (asset is not null)
        {
            var preview = new UnitIconPreview(_scene.SvgAssets.Resolve(asset.Id), 22);
            DockPanel.SetDock(preview, Dock.Left); row.Children.Add(preview);
        }
        row.Children.Add(new TextBlock { Text = asset is null ? "选择单位图标" : _scene.UnitIcons.NameFor(asset.SourcePath),
            VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0),
            FontSize = 11, TextTrimming = TextTrimming.CharacterEllipsis });
        _icon.Content = row;
    }

    private void Bind(string path) => Run(() =>
    {
        var asset = _scene.BindSymbol(_id, path);
        Console.WriteLine($"[C03/二维] 已绑定 {_scene.UnitIcons.NameFor(asset.SourcePath)}，CPU缓存={_scene.SvgAssets.LoadedCount}");
    });

    private async Task LoadAsync()
    {
        try
        {
            var top = TopLevel.GetTopLevel(this);
            if (top is null) return;
            var files = await top.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                { Title = "选择 SVG 资产", AllowMultiple = false, FileTypeFilter = [new("SVG") { Patterns = ["*.svg"] }] });
            if (files.Count == 0 || _scene.SelectedId != _id) return;
            Bind(files[0].TryGetLocalPath() ?? throw new NotSupportedException("仅支持本地 SVG。"));
        }
        catch (Exception error) { _error.Text = error.Message; }
    }

    private void Run(Action action)
    {
        if (_scene.SelectedId != _id) return;
        try { action(); _error.Text = ""; }
        catch (Exception error) { _error.Text = error.Message; Console.Error.WriteLine($"[C03/二维] {error.Message}"); }
        Refresh();
    }
}