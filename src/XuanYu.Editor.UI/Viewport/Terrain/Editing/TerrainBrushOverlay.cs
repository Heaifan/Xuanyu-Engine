using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using XuanYu.Editor.TerrainEditing;

namespace XuanYu.Editor.UI;

internal sealed class TerrainBrushOverlay : Control
{
    private static readonly Pen Shadow = new(Brushes.Black, 3d);
    private TerrainBrushPreview? _preview;
    private Matrix4x4 _projection;

    public TerrainBrushOverlay()
    {
        IsHitTestVisible = false;
        ClipToBounds = true;
    }

    public void SetFrame(TerrainBrushPreview? preview, Matrix4x4 projection)
    {
        if (_preview is null && preview is null) return;
        _preview = preview;
        _projection = projection;
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (_preview is not { } preview) return;
        Matrix4x4 matrix = preview.MapToScene * _projection;
        var outer = new Pen(new SolidColorBrush(Colors.White,
            0.25d + 0.35d * preview.Strength), 1.2d);
        var inner = new Pen(new SolidColorBrush(Colors.White,
            0.20d + 0.30d * preview.Strength), 1d);
        DrawOutline(context, preview.Outer, matrix, outer);
        DrawOutline(context, preview.Inner, matrix, inner);
        DrawCursor(context, preview, matrix);
        if (Project(preview.CursorMeters ?? preview.CenterMeters, matrix, out Point center))
        {
            context.DrawLine(outer, new Point(center.X - 3, center.Y),
                new Point(center.X + 3, center.Y));
            context.DrawLine(outer, new Point(center.X, center.Y - 3),
                new Point(center.X, center.Y + 3));
        }
    }

    private void DrawCursor(DrawingContext context, TerrainBrushPreview preview, Matrix4x4 matrix)
    {
        Vector3 anchor = preview.CursorMeters ?? preview.CenterMeters;
        if (preview.RadiusMeters <= 0f || !Project(anchor, matrix, out Point center)) return;
        Vector4 clip = Vector4.Transform(new Vector4(anchor, 1f), matrix);
        // 投影 Y 列长度包含真实米制到显示坐标的缩放；圆环只是辅助光标。
        double scale = new Vector3(matrix.M12, matrix.M22, matrix.M32).Length()
            * Bounds.Height / (2d * clip.W);
        var pen = new Pen(new SolidColorBrush(Colors.White, 0.9d), 1.5d);
        double r = preview.RadiusMeters * scale;
        context.DrawEllipse(null, Shadow, center, r, r);
        context.DrawEllipse(null, pen, center, r, r);
        if (preview.InnerRadiusMeters <= 0f) return;
        double inner = preview.InnerRadiusMeters * scale;
        context.DrawEllipse(null, pen, center, inner, inner);
    }

    private void DrawOutline(DrawingContext context,
        IReadOnlyList<TerrainBrushOutline.Segment> segments,
        Matrix4x4 matrix, Pen pen)
    {
        foreach (var segment in segments)
        {
            if (!Project(segment.Start, matrix, out Point a) ||
                !Project(segment.End, matrix, out Point b)) continue;
            context.DrawLine(pen, a, b);
        }
    }

    private bool Project(Vector3 position, Matrix4x4 matrix, out Point point)
    {
        point = default;
        Vector4 clip = Vector4.Transform(new Vector4(position, 1f), matrix);
        if (clip.W <= 0f || clip.Z < 0f || clip.Z > clip.W) return false;
        // Vulkan 相机投影已翻转 Y，归一化坐标直接映射至 Avalonia DIP。
        point = new Point((clip.X / clip.W + 1f) * Bounds.Width * 0.5d,
            (clip.Y / clip.W + 1f) * Bounds.Height * 0.5d);
        return true;
    }
}