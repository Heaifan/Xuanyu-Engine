using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.Editor.UI;

internal sealed partial class GeoTerrainHud : Border
{
    private static readonly IBrush MainInk = new SolidColorBrush(Color.Parse("#30445A"));
    private static readonly IBrush SubInk = new SolidColorBrush(Color.Parse("#62758A"));
    private readonly TextBlock _coordinates = new()
        { FontSize = 10, Foreground = MainInk };
    private readonly TextBlock _elevation = new()
        { FontSize = 10, Foreground = SubInk };
    private readonly TextBlock _scaleText = new()
        { FontSize = 10, Foreground = MainInk };
    private readonly Border _scaleBar = new()
    {
        Height = 9, BorderBrush = MainInk,
        BorderThickness = new Thickness(1, 0, 1, 2)
    };

    public GeoTerrainHud()
    {
        Width = 210;
        Padding = new Thickness(6, 4);
        CornerRadius = new CornerRadius(5);
        Background = new SolidColorBrush(Color.Parse("#F7FAFD"), 0.94);
        BorderBrush = new SolidColorBrush(Color.Parse("#C8D4E1"));
        BorderThickness = new Thickness(1);
        IsHitTestVisible = false;
        var content = new StackPanel { Spacing = 1 };
        var ruler = new StackPanel
            { Orientation = Orientation.Horizontal, Spacing = 7 };
        ruler.Children.Add(_scaleBar);
        ruler.Children.Add(_scaleText);
        content.Children.Add(_coordinates);
        content.Children.Add(_elevation);
        content.Children.Add(ruler);
        Child = content;
        ShowPoint(null);
        ShowScale(null);
    }

    public void ShowLocalPoint(Vector3? value)
    {
        _coordinates.Text = value is { } point
            ? $"X {point.X:0.##} m    Y {point.Y:0.##} m" : "X —    Y —";
        _elevation.Text = value is { } p ? $"Z {p.Z:0.##} m" : "Z —";
    }

    public void ShowPoint(GeoTerrainHudPoint? value)
    {
        if (value is not { } point)
        {
            _coordinates.Text = "纬度 —    经度 —";
            _elevation.Text = "高程 —    ·    瓦片 —";
            return;
        }
        string ns = point.Latitude < 0 ? "S" : "N";
        string ew = point.Longitude < 0 ? "W" : "E";
        _coordinates.Text = $"{Math.Abs(point.Latitude):F6}°{ns}  " +
            $"{Math.Abs(point.Longitude):F6}°{ew}";
        _elevation.Text = $"高程 {point.ElevationMeters:N1} m    ·    {point.TileId}";
    }
}