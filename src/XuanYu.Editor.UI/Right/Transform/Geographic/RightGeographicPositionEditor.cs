using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using XuanYu.Core.Geodesy;

namespace XuanYu.Editor.UI.Right;

internal sealed class RightGeographicPositionEditor : StackPanel
{
    private readonly TextBox _latitude = new(), _longitude = new(), _height = new();
    private readonly ComboBox _mode = new() { ItemsSource = new[] { "离地高度 (m)", "海拔/正高 (m)" } };
    private readonly TextBlock _status = new() { FontSize = 11, TextWrapping = TextWrapping.Wrap };
    private readonly Func<GeoCoordinate> _read;
    private readonly Func<(double Orthometric, double? AboveGround)?> _heights;
    private readonly Func<GeoCoordinate, bool, string?> _apply;

    public RightGeographicPositionEditor(Func<GeoCoordinate> read,
        Func<(double Orthometric, double? AboveGround)?> heights,
        Func<GeoCoordinate, bool, string?> apply)
    {
        _read = read; _heights = heights; _apply = apply;
        Margin = new Thickness(8, 3); Spacing = 2;
        AddField("纬度 (°)", _latitude);
        AddField("经度 (°)", _longitude);
        AddField("高度模式", _mode);
        AddField("高度 (m)", _height);
        var applyButton = new Button { Content = "应用地理位置", FontSize = 11, Height = 24, MinHeight = 0,
            Padding = new Thickness(6, 0), Background = EditorTheme.PanelHeaderBg, Foreground = EditorTheme.TextMain };
        applyButton.Click += (_, _) => Apply();
        Children.Add(applyButton);
        Children.Add(_status);
        _mode.SelectedIndex = _heights()?.AboveGround is not null ? 0 : 1;
        _mode.SelectionChanged += (_, _) => RefreshHeight();
        RefreshPosition();
    }

    private void AddField(string label, Control input)
    {
        var row = new Grid { ColumnDefinitions = new("74,*"), Height = 25 };
        row.Children.Add(new TextBlock { Text = label, FontSize = 11, Foreground = EditorTheme.TextSub,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
        if (input is TextBox text)
        { text.FontSize = 11; text.MinHeight = 0; text.Height = 22; text.Padding = new Thickness(5, 0);
            text.Background = EditorTheme.PanelHeaderBg; text.BorderBrush = EditorTheme.Border;
            text.BorderThickness = new Thickness(1); text.TextAlignment = TextAlignment.Right; }
        else if (input is ComboBox mode) { mode.FontSize = 11; mode.MinHeight = 0; mode.Height = 23; mode.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch; mode.Padding = new Thickness(5, 0); }
        Grid.SetColumn(input, 1); row.Children.Add(input); Children.Add(row);
    }

    internal void RefreshPosition()
    {
        var geo = _read();
        _latitude.Text = Format(geo.LatitudeDegrees, 8);
        _longitude.Text = Format(geo.LongitudeDegrees, 8);
        RefreshHeight();
    }

    private void RefreshHeight()
    {
        var heights = _heights();
        double? height = _mode.SelectedIndex == 0 ? heights?.AboveGround : heights?.Orthometric;
        _height.Text = height is double value ? Format(value, 3) : "";
        _status.Text = heights is null ? "地形尚未就绪，无法转换高度。" :
            $"椭球高：{_read().HeightMeters:F3} m";
    }

    private void Apply()
    {
        var geo = _read();
        var heights = _heights();
        double? oldHeight = _mode.SelectedIndex == 0 ? heights?.AboveGround : heights?.Orthometric;
        if (!Read(_latitude.Text, geo.LatitudeDegrees, 8, out double lat) ||
            !Read(_longitude.Text, geo.LongitudeDegrees, 8, out double lon) ||
            !Read(_height.Text, oldHeight, 3, out double h))
        { _status.Text = "请输入有效数字，使用小数点。"; return; }
        try
        {
            if (lat == geo.LatitudeDegrees && lon == geo.LongitudeDegrees && h == oldHeight)
            { _status.Text += " · 未修改"; return; }
            string? error = _apply(new GeoCoordinate(lat, lon, h), _mode.SelectedIndex == 0);
            if (error is not null) { _status.Text = error; return; }
            RefreshPosition();
            _status.Text += " · 已应用";
        }
        catch (ArgumentException error) { _status.Text = error.Message; }
        catch (InvalidOperationException error) { _status.Text = error.Message; }
    }

    private static bool Read(string? text, double? original, int digits, out double value)
    {
        if (original is double exact && text == Format(exact, digits))
        { value = exact; return true; }
        return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private static string Format(double value, int digits) => value.ToString($"F{digits}", CultureInfo.InvariantCulture);
}