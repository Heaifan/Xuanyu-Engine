using Avalonia.Controls;
using Avalonia.Media;
using SvgPath = Avalonia.Controls.Shapes.Path;

namespace XuanYu.Editor.UI;

internal static class ViewportNavIcons
{
    public static Control Create(string name)
    {
        string data = name switch
        {
            "zoom" => "M 4,10 A 6,6 0 1 0 16,10 A 6,6 0 1 0 4,10 M 10,7 L 10,13 M 7,10 L 13,10 M 15,15 L 21,21",
            "pan" => "M 7,14 V 7 Q 7,5 9,5 Q 11,5 11,7 V 4 Q 11,2 13,2 Q 15,2 15,4 V 9 V 6 Q 15,4 17,4 Q 19,4 19,6 V 12 L 20,11 Q 22,10 23,12 L 20,18 Q 18,22 14,22 H 10 Q 7,22 5,19 L 2,15 Q 1,13 3,12 Q 5,11 7,14 Z",
            "camera" => "M 3,8 L 13,8 L 13,17 L 3,17 Z M 13,10 L 20,6 L 20,19 L 13,15 M 6,5 L 10,5",
            "projection" => "M 3,8 L 11,4 L 20,8 L 20,17 L 11,21 L 3,17 Z M 11,4 L 11,21 M 3,8 L 11,12 L 20,8",
            _ => throw new ArgumentOutOfRangeException(nameof(name))
        };
        return new SvgPath
        {
            Data = StreamGeometry.Parse(data), Width = 18, Height = 18,
            Stroke = new SolidColorBrush(Color.Parse("#566A7E")),
            StrokeThickness = 1.7, StrokeLineCap = PenLineCap.Round,
            StrokeJoin = PenLineJoin.Round, Fill = Brushes.Transparent,
            Stretch = Stretch.Uniform, IsHitTestVisible = false
        };
    }
}