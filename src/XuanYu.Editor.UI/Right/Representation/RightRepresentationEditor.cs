using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using System.Globalization;
using XuanYu.Editor.Preview;
using XuanYu.Runtime;

namespace XuanYu.Editor.UI.Right;

internal sealed partial class RightRepresentationEditor : StackPanel
{
    private readonly PreviewScene _scene;
    private readonly Guid _id;
    private EntityRepresentationKind[] _kinds = [];
    private bool _refreshing;
    private string? _shownAsset;
    private BillboardStyle? _shownStyle;
    private readonly ComboBox _choice = new();
    private readonly ComboBox _mode = new() { ItemsSource = new[] { "固定像素", "米制尺寸" } };
    private readonly TextBox _size = new() { PlaceholderText = "尺寸" };
    private readonly TextBlock _sizeLabel = new();
    private readonly Button _icon = new();
    private readonly TextBlock _asset = new() { FontSize = 11, TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _error = new() { FontSize = 11, TextWrapping = TextWrapping.Wrap };
    private readonly StackPanel _style = new() { Spacing = 2 };
    private readonly Button _clear = new() { Content = "清除 SVG" };

    public RightRepresentationEditor(PreviewScene scene, Guid id)
    {
        _scene = scene; _id = id; Spacing = 2; Margin = new Thickness(8, 3);
        BuildRows();
        _choice.SelectionChanged += (_, _) =>
        {
            if (!_refreshing && _choice.SelectedIndex >= 0)
                Run(() => _scene.SetRepresentation(_id, _kinds[_choice.SelectedIndex]));
        };
        _icon.Click += (_, _) => new UnitIconSelector(_scene, _scene.GetSymbolAsset(_id)?.SourcePath, Bind).Show(_icon);
        _mode.SelectionChanged += (_, _) =>
        {
            if (!_refreshing && _mode.SelectedIndex >= 0 && _shownStyle is not null) ApplySize();
        };
        _size.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter) { e.Handled = true; ApplySize(); }
        };
        Refresh();
    }

    public void Refresh()
    {
        _refreshing = true;
        try
        {
            var item = _scene.Objects.First(x => x.Id == _id);
            var asset = _scene.GetSymbolAsset(_id);
            List<EntityRepresentationKind> kinds = [EntityRepresentationKind.None, EntityRepresentationKind.Point];
            if (item.Mesh is not null) { kinds.Add(EntityRepresentationKind.Mesh); kinds.Add(EntityRepresentationKind.Proxy); }
            if (asset is not null) kinds.Add(EntityRepresentationKind.Symbol);
            if (!_kinds.SequenceEqual(kinds)) { _kinds = kinds.ToArray(); _choice.ItemsSource = _kinds.Select(Label).ToArray(); }
            _choice.SelectedIndex = Array.IndexOf(_kinds, _scene.GetRepresentation(_id));
            if (_icon.Content is null || _shownAsset != asset?.Id) RefreshIcon(asset);
            _asset.Text = asset is null ? "未绑定 SVG" : $"SVG：{asset.Name}";
            _clear.IsEnabled = asset is not null;
            _style.IsVisible = asset is not null;
            if (asset is null) _shownStyle = null;
            else RefreshSize(_scene.GetSymbolStyle(_id));
        }
        finally { _refreshing = false; }
    }

    private void RefreshSize(BillboardStyle style)
    {
        _mode.SelectedIndex = style.FixedPixels ? 0 : 1;
        _sizeLabel.Text = style.FixedPixels ? "尺寸 (px)" : "尺寸 (m)";
        // 与尺寸无关的场景刷新不得覆盖尚未提交的输入。
        if (_shownStyle != style) _size.Text = style.Size.ToString(CultureInfo.InvariantCulture);
        _shownStyle = style;
    }

    private void ApplySize() => Run(() =>
    {
        if (!float.TryParse(_size.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
            throw new FormatException("请输入有效尺寸，使用小数点。");
        _scene.SetSymbolStyle(_id, new(value, _mode.SelectedIndex == 0));
    });

    private static string Label(EntityRepresentationKind kind) => kind switch
    {
        EntityRepresentationKind.None => "无表现", EntityRepresentationKind.Point => "点位",
        EntityRepresentationKind.Mesh => "主网格", EntityRepresentationKind.Proxy => "简化代理", _ => "二维图形"
    };
}