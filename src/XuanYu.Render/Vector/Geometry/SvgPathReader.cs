using System.Numerics;

namespace XuanYu.Render;

internal static class SvgPathReader
{
    public static SvgContour[] Read(string data)
    {
        string[] tokens = SvgNumbers.Tokenize(data);
        List<SvgContour> contours = [];
        List<Vector2> points = [];
        Vector2 current = default, start = default, control = default;
        int i = 0;
        char command = '\0', previous = '\0';
        bool begun = false;
        while (i < tokens.Length)
        {
            if (tokens[i].Length == 1 && char.IsLetter(tokens[i][0])) command = tokens[i++][0];
            bool relative = char.IsLower(command);
            char op = char.ToUpperInvariant(command);
            if (!begun && op != 'M') throw new InvalidDataException("SVG 路径需要先使用 M。");
            if (op != 'M' && op != 'Z' && points.Count == 0) { start = current; points.Add(current); }
            switch (op)
            {
                case 'M':
                    Finish(false); current = Point(); start = current; points.Add(current);
                    begun = true; command = relative ? 'l' : 'L'; break;
                case 'L': current = Point(); points.Add(current); break;
                case 'H': float x = Next(); current.X = relative ? current.X+x : x; points.Add(current); break;
                case 'V': float y = Next(); current.Y = relative ? current.Y+y : y; points.Add(current); break;
                case 'C': case 'S':
                    var a = op == 'C' ? Point() : (previous is 'C' or 'S') ? 2*current-control : current;
                    var b = Point(); var end = Point();
                    SvgCurves.Cubic(points, current, a, b, end); current = end; control = b; break;
                case 'Q': case 'T':
                    var q = op == 'Q' ? Point() : (previous is 'Q' or 'T') ? 2*current-control : current;
                    var target = Point(); SvgCurves.Quadratic(points, current, q, target);
                    current = target; control = q; break;
                case 'A':
                    float rx = Next(), ry = Next(), angle = Next();
                    bool large = Flag(), sweep = Flag(); var destination = Point();
                    SvgArc.Append(points, current, destination, rx, ry, angle, large, sweep);
                    current = destination; break;
                case 'Z':
                    current = start; Finish(true); command = '\0'; break;
                default: throw new InvalidDataException($"暂不支持 SVG 路径命令：{command}。");
            }
            previous = op;
        }
        Finish(false);
        return contours.ToArray();

        float Next() => i < tokens.Length ? SvgNumbers.Value(tokens[i++]) : throw new InvalidDataException("SVG 路径参数不足。");
        bool Flag() => Next() switch { 0 => false, 1 => true, _ => throw new InvalidDataException("SVG 圆弧标志无效。") };
        Vector2 Point() => new Vector2(Next(), Next()) + (char.IsLower(command) ? current : Vector2.Zero);
        void Finish(bool closed)
        {
            if (points.Count != 0) contours.Add(new(points.ToArray(), closed));
            points.Clear();
        }
    }
}