using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.Editor.UI;

internal sealed class GeoContourLabelCanvas : Canvas
{
    private static readonly IBrush Ink = new SolidColorBrush(Color.Parse("#505D4C"));
    private readonly List<TextBlock> _texts = [];
    private readonly TextBlock _measure = new() { FontSize = 10 };
    private readonly Dictionary<int, float> _widths = [];

    public GeoContourLabelCanvas()
    {
        IsHitTestVisible = false;
        ClipToBounds = true;
    }

    public float MeasureWidth(int elevation)
    {
        if (_widths.TryGetValue(elevation, out float width)) return width;
        _measure.Text = $"{elevation:N0}m";
        _measure.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        width = (float)_measure.DesiredSize.Width;
        _widths[elevation] = width;
        return width;
    }

    public void Show(IReadOnlyList<GeoContourLabel> labels, double scale)
    {
        while (_texts.Count < labels.Count)
        {
            var text = new TextBlock
            {
                Width = 64, Height = 16, FontSize = 10,
                Foreground = Ink, TextAlignment = TextAlignment.Center,
                IsHitTestVisible = false, IsVisible = false,
                RenderTransformOrigin = new RelativePoint(
                    0.5, 0.5, RelativeUnit.Relative)
            };
            _texts.Add(text);
            Children.Add(text);
        }
        for (int i = 0; i < _texts.Count; i++)
        {
            TextBlock text = _texts[i];
            text.IsVisible = i < labels.Count;
            if (!text.IsVisible) continue;
            GeoContourLabel label = labels[i];
            text.Text = $"{label.ElevationMeters:N0}m";
            text.RenderTransform = new RotateTransform(label.AngleDegrees);
            SetLeft(text, label.X / scale - 32);
            SetTop(text, label.Y / scale - 8);
        }
    }
}