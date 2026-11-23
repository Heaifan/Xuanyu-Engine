using Avalonia;
using Avalonia.Media;

namespace XuanYu.Editor.UI;

internal sealed partial class GeoTerrainMiniMap
{
    private static readonly IBrush[] Palette =
    [
        Brush("#DEE4EA"), Brush("#65AFCF"), Brush("#5CBB6F"),
        Brush("#6BC36D"), Brush("#7CCC70"), Brush("#90D272"),
        Brush("#A8D96D"), Brush("#C4DC79"), Brush("#D5DF83"),
        Brush("#E5D47B"), Brush("#EDCF79"), Brush("#D5B07A"),
        Brush("#BF9369"), Brush("#A9A5A0")
    ];
    private static readonly IBrush Frame = Brush("#F7FAFD");
    private static readonly Pen Border = new(Brush("#A3B3C3"), 1);
    private static readonly Pen Direction = new(Brush("#D44432"), 2);
    private static readonly Pen Position = new(Brushes.White, 1.5);
    private static IBrush Brush(string hex) =>
        new SolidColorBrush(Color.Parse(hex));

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (_map is not { } map) return;
        context.DrawRectangle(Frame, Border, new Rect(Bounds.Size));
        Rect area = MapArea(map.South, map.North, map.West, map.East);
        double cellWidth = area.Width / map.Columns;
        double cellHeight = area.Height / map.Rows;
        for (int y = 0; y < map.Rows; y++)
            for (int x = 0; x < map.Columns; x++)
                context.DrawRectangle(Palette[map.Cells[y * map.Columns + x]], null,
                    new Rect(area.X + x * cellWidth, area.Y + y * cellHeight,
                        cellWidth + 0.05d, cellHeight + 0.05d));
        DrawCamera(context, area);
    }

    private Rect MapArea(double south, double north, double west, double east)
    {
        double physicalWidth = (east - west) *
            Math.Cos((south + north) * 0.5d * Math.PI / 180d);
        double physicalHeight = north - south;
        double side = Math.Min(Bounds.Width, Bounds.Height) - 8d;
        double ratio = physicalWidth / physicalHeight;
        double w = ratio > 1d ? side : side * ratio;
        double h = ratio > 1d ? side / ratio : side;
        return new Rect((Bounds.Width - w) * 0.5d,
            (Bounds.Height - h) * 0.5d, w, h);
    }

    private void DrawCamera(DrawingContext context, Rect area)
    {
        var map = _map!;
        double u = (_longitude - map.West) / (map.East - map.West);
        double v = (map.North - _latitude) / (map.North - map.South);
        if (u < 0d || u > 1d || v < 0d || v > 1d) return;
        Point center = new(area.X + u * area.Width, area.Y + v * area.Height);
        Point tip = new(center.X - 12d * Math.Sin(_yaw),
            center.Y - 12d * Math.Cos(_yaw));
        context.DrawLine(Direction, center, tip);
        context.DrawEllipse(Brushes.OrangeRed, Position, center, 4.5d, 4.5d);
    }
}