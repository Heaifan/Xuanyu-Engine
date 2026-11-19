using Avalonia;
using Avalonia.Media;

namespace XuanYu.Editor.UI;

public sealed partial class TileMonitorCanvas
{
    private static readonly IBrush Cached = new SolidColorBrush(Color.Parse("#D6DCE3"));
    private static readonly IBrush Pending = new SolidColorBrush(Color.Parse("#E58B29"));
    private static readonly IBrush Eye = new SolidColorBrush(Color.Parse("#AA2ED0"));
    private static readonly IBrush Target = new SolidColorBrush(Color.Parse("#D93A52"));
    private static readonly IBrush GridLine = new SolidColorBrush(Color.Parse("#C2D0DF"));
    private static readonly IBrush[] Lod = Enumerable.Range(0, 14)
        .Select(i => (IBrush)new SolidColorBrush(Color.FromRgb(
            (byte)(64 + i * 8), (byte)(205 - i * 5), (byte)(245 - i * 6))))
        .ToArray();

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        context.FillRectangle(EditorTheme.PanelBg, new Rect(Bounds.Size));
        if (Snapshot is null) return;
        Rect area = Plot;
        context.FillRectangle(EditorTheme.ViewportBg, area);
        for (int i = 1; i < 4; i++)
        {
            double x = area.X + area.Width * i / 4d;
            double y = area.Y + area.Height * i / 4d;
            context.DrawLine(new Pen(GridLine, 0.5),
                new Point(x, area.Top), new Point(x, area.Bottom));
            context.DrawLine(new Pen(GridLine, 0.5),
                new Point(area.Left, y), new Point(area.Right, y));
        }
        foreach (var tile in Snapshot.Tiles.Where(t => InsideCoverage(t.Bounds)))
        {
            Rect rect = ProjectTile(tile.Bounds);
            if (rect.Width <= 0 || rect.Height <= 0) continue;
            if (!tile.Cached || tile.Drawn) continue;
            context.DrawRectangle(Cached, new Pen(GridLine, 0.5), rect);
        }
        foreach (var tile in Snapshot.Tiles.Where(t => t.Drawn && InsideCoverage(t.Bounds)))
        {
            Rect rect = ProjectTile(tile.Bounds);
            if (rect.Width <= 0 || rect.Height <= 0) continue;
            context.DrawRectangle(Lod[Math.Min(tile.Id.Level, 13)],
                new Pen(EditorTheme.TextSub, 0.7), rect);
        }
        foreach (var tile in Snapshot.Tiles.Where(t => t.Targeted && !t.Drawn && InsideCoverage(t.Bounds)))
            context.DrawRectangle(null, new Pen(Pending, 1.7), ProjectTile(tile.Bounds));
        if (_selectedId is { } id)
            foreach (var tile in Snapshot.Tiles.Where(t => t.Id == id && InsideCoverage(t.Bounds)))
                context.DrawRectangle(null, new Pen(Target, 2.4), ProjectTile(tile.Bounds));
        DrawCamera(context);
        context.DrawRectangle(null, new Pen(GridLine, 1), area);
    }

    private void DrawCamera(DrawingContext context)
    {
        var s = Snapshot!;
        Point eye = Project(s.Eye.LatitudeDegrees, s.Eye.LongitudeDegrees);
        Point target = Project(s.Target.LatitudeDegrees, s.Target.LongitudeDegrees);
        Vector forward = new(target.X - eye.X, target.Y - eye.Y);
        if (forward.Length < 4d)
            forward = new Vector(-Math.Sin(s.CameraYawRadians),
                -Math.Cos(s.CameraYawRadians));
        forward = forward / forward.Length;
        double dx = target.X - eye.X, dy = target.Y - eye.Y;
        double distance = Math.Clamp(Math.Sqrt(dx * dx + dy * dy), 22d, 90d);
        context.DrawLine(new Pen(Eye, 1.4), eye, target);
        foreach (double angle in new[] { -Math.PI / 6d, Math.PI / 6d })
        {
            double cos = Math.Cos(angle), sin = Math.Sin(angle);
            Vector ray = new(forward.X * cos - forward.Y * sin,
                forward.X * sin + forward.Y * cos);
            context.DrawLine(new Pen(Eye, 1), eye, eye + ray * distance);
        }
        context.DrawEllipse(Eye, new Pen(EditorTheme.PanelBg, 1),
            new Rect(eye.X - 3, eye.Y - 3, 6, 6));
        context.DrawEllipse(Target, new Pen(EditorTheme.PanelBg, 1),
            new Rect(target.X - 3, target.Y - 3, 6, 6));
    }
}