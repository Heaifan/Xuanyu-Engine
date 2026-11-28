using System.Numerics;
using Avalonia;
using Avalonia.Media;
using XuanYu.Editor.Preview;

namespace XuanYu.Editor.UI;

internal sealed partial class GeoGizmoOverlay
{
    private void DrawOrientation(DrawingContext context, GeoGizmoLayout layout)
    {
        foreach (var h in layout.Handles)
        {
            int index = h.Id == GeoGizmoHandle.E ? 0 : h.Id == GeoGizmoHandle.N ? 1 : 2;
            using var opacity = context.PushOpacity(_active == GeoGizmoHandle.None || _active.HasFlag(h.Id) ? 1d : .3d);
            var color = h.Id == _active ? Active : ColorsByAxis[index];
            var pen = new Pen(color, h.Id == _active ? 4.5 : 3.3);
            if (layout.Mode == GeoGizmoMode.Rotate)
            {
                var path = new StreamGeometry();
                using (var g = path.Open())
                {
                    g.BeginFigure(Pt(h.Points[0]), false);
                    foreach (var point in h.Points.Skip(1)) g.LineTo(Pt(point));
                    g.EndFigure(true);
                }
                context.DrawGeometry(null, Shadow, path);
                context.DrawGeometry(null, pen, path);
            }
            else
            {
                context.DrawLine(Shadow, Pt(layout.Center), Pt(h.Points[1]));
                context.DrawLine(pen, Pt(layout.Center), Pt(h.Points[1]));
                Square(context, h.Points[1], color);
            }
        }
        if (layout.Mode == GeoGizmoMode.Scale)
            Square(context, layout.Center, _active == GeoGizmoHandle.All ? Active : Brushes.White);
    }

    private static void Square(DrawingContext context, Vector2 p, IBrush brush) =>
        context.DrawRectangle(brush, new Pen(Brush("#465565"), 1.5), new Rect(p.X - 5, p.Y - 5, 10, 10));
}