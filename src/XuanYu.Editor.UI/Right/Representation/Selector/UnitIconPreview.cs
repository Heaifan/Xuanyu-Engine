using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using XuanYu.Render;

namespace XuanYu.Editor.UI.Right;

internal sealed class UnitIconPreview : Control
{
    private readonly List<(StreamGeometry Geometry, IBrush Brush)> _triangles = [];

    public UnitIconPreview(RenderMesh mesh, double size)
    {
        Width = Height = size;
        IsHitTestVisible = false;
        var vertices = mesh.Vertices.Span;
        for (int i = 0; i < vertices.Length; i += 21)
        {
            var geometry = new StreamGeometry();
            using (var g = geometry.Open())
            {
                g.BeginFigure(Project(vertices, i, size), true);
                g.LineTo(Project(vertices, i + 7, size));
                g.LineTo(Project(vertices, i + 14, size));
                g.EndFigure(true);
            }
            var color = Color.FromArgb(Channel(vertices[i + 6]), Channel(vertices[i + 3]), Channel(vertices[i + 4]), Channel(vertices[i + 5]));
            _triangles.Add((geometry, new SolidColorBrush(color)));
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        foreach (var (geometry, brush) in _triangles) context.DrawGeometry(brush, null, geometry);
    }

    private static Point Project(ReadOnlySpan<float> v, int i, double size) =>
        new((.5 + v[i] * .9) * size, (.5 + v[i + 1] * .9) * size);
    private static byte Channel(float value) => (byte)Math.Round(Math.Clamp(value, 0f, 1f) * 255);
}