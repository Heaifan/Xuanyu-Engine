using System.Globalization;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using XuanYu.Editor.Preview;

namespace XuanYu.Editor.UI;

internal sealed partial class GeoGizmoOverlay : Control
{
    private static readonly IBrush[] ColorsByAxis = [Brush("#DF4141"), Brush("#31AD58"), Brush("#307CE9")];
    private static readonly IBrush Active = Brush("#E6B522");
    private static readonly Pen Shadow = new(Brush("#263645"), 5.5);
    private GeoGizmoLayout? _layout;
    private GeoGizmoHandle _active;
    public GeoGizmoOverlay() { IsHitTestVisible = false; ClipToBounds = true; }
    public void SetFrame(GeoGizmoLayout? layout, GeoGizmoHandle active)
    { _layout = layout; _active = active; InvalidateVisual(); }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (_layout is not { } layout) return;
        if (layout.Mode != GeoGizmoMode.Translate) { DrawOrientation(context, layout); return; }
        foreach (var h in layout.Handles.Where(h => h.Points.Length == 4))
        {
            var color = h.Id switch { GeoGizmoHandle.EN => Color.Parse("#D5AA2D"),
                GeoGizmoHandle.EU => Color.Parse("#BB54BE"), _ => Color.Parse("#24B6BC") };
            bool selected = h.Id == _active;
            Polygon(context, h.Points, new SolidColorBrush(color, selected ? .8 : _active == GeoGizmoHandle.None ? .24 : .10),
                new Pen(new SolidColorBrush(color), 1.4));
        }
        foreach (var h in layout.Handles.Where(h => h.Points.Length == 2))
        {
            int index = h.Id == GeoGizmoHandle.E ? 0 : h.Id == GeoGizmoHandle.N ? 1 : 2;
            using var opacity = context.PushOpacity(_active == GeoGizmoHandle.None || _active.HasFlag(h.Id) ? 1d : .3d);
            var brush = h.Id == _active ? Active : ColorsByAxis[index];
            var tip = h.Points[1];
            var direction = Vector2.Normalize(tip - layout.Center);
            var side = new Vector2(-direction.Y, direction.X) * 5.5f;
            var end = tip - direction * 12;
            context.DrawLine(Shadow, Pt(layout.Center), Pt(end));
            context.DrawLine(new Pen(brush, h.Id == _active ? 4.5 : 3.3), Pt(layout.Center), Pt(end));
            Polygon(context, [tip, end+side, end-side], brush, null);
            var text = new FormattedText(h.Id.ToString(), CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, new Typeface("Arial"), 12, brush);
            var label = tip + direction * 12;
            context.DrawText(text, new Point(label.X - text.Width/2, label.Y - text.Height/2));
        }
        if (layout.Handles.Any(h => h.Id == GeoGizmoHandle.EN))
            context.DrawEllipse(_active == GeoGizmoHandle.EN ? Active : Brushes.White,
                new Pen(Brush("#465565"), 1.5), Pt(layout.Center), 6, 6);
    }

    private static void Polygon(DrawingContext context, Vector2[] points, IBrush fill, Pen? pen)
    {
        var path = new StreamGeometry();
        using (var g = path.Open())
        {
            g.BeginFigure(Pt(points[0]), true);
            foreach (var point in points.Skip(1)) g.LineTo(Pt(point));
            g.EndFigure(true);
        }
        context.DrawGeometry(fill, pen, path);
    }
    private static Point Pt(Vector2 p) => new(p.X, p.Y);
    private static IBrush Brush(string hex) => new SolidColorBrush(Color.Parse(hex));
}