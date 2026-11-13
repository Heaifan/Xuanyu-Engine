using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace XuanYu.Editor.UI.Left;

internal sealed class LeftTreeGuide : Control
{
    private const int Indent = 5;
    private static readonly Pen Line = new(
        new SolidColorBrush(Color.Parse("#CFDAE6")), 1);
    private readonly bool[] _continuations;
    private readonly int _depth;
    private readonly bool _isLast;

    public LeftTreeGuide(int depth, bool[] continuations, bool isLast,
        bool isLeaf = false)
    {
        _depth = depth;
        _continuations = continuations;
        _isLast = isLast;
        // 叶节点箭头列由 17 缩至 5，差额给引导线，维持每级 5 DIP 的图标位移。
        Width = depth * Indent + 12 + (isLeaf ? 12 : 0);
        Height = 28;
        IsHitTestVisible = false;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        for (int i = 0; i < _continuations.Length; i++)
        {
            if (!_continuations[i])
                continue;
            double x = i * Indent + 8.5;
            context.DrawLine(Line, new Point(x, 0), new Point(x, 28));
        }
        if (_depth == 0)
            return;
        double elbow = (_depth - 1) * Indent + 8.5;
        context.DrawLine(Line, new Point(elbow, 0),
            new Point(elbow, _isLast ? 14 : 28));
        context.DrawLine(Line, new Point(elbow, 14),
            new Point(Width, 14));
    }
}