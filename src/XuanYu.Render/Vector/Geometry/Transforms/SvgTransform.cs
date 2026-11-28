using System.Numerics;
using System.Text.RegularExpressions;

namespace XuanYu.Render;

internal static class SvgTransform
{
    private static readonly Regex Pattern = new(@"\G\s*,?\s*([A-Za-z]+)\s*\(([^()]*)\)");

    public static Matrix3x2 Read(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return Matrix3x2.Identity;
        Matrix3x2 result = Matrix3x2.Identity;
        int offset = 0;
        while (offset < text.Length && !string.IsNullOrWhiteSpace(text[offset..]))
        {
            var match = Pattern.Match(text, offset);
            if (!match.Success) throw new InvalidDataException("无效 SVG transform。");
            float[] v = SvgNumbers.Parse(match.Groups[2].Value);
            result = Matrix(match.Groups[1].Value, v) * result;
            offset += match.Length;
        }
        return result;
    }

    private static Matrix3x2 Matrix(string name, float[] v) => (name, v.Length) switch
    {
        ("matrix", 6) => new(v[0], v[1], v[2], v[3], v[4], v[5]),
        ("translate", 1 or 2) => Matrix3x2.CreateTranslation(v[0], v.Length == 2 ? v[1] : 0),
        ("scale", 1 or 2) => Matrix3x2.CreateScale(v[0], v.Length == 2 ? v[1] : v[0]),
        ("rotate", 1) => Matrix3x2.CreateRotation(Radians(v[0])),
        ("rotate", 3) => Matrix3x2.CreateRotation(Radians(v[0]), new Vector2(v[1], v[2])),
        ("skewX", 1) => Matrix3x2.CreateSkew(Radians(v[0]), 0),
        ("skewY", 1) => Matrix3x2.CreateSkew(0, Radians(v[0])),
        _ => throw new InvalidDataException($"无效 SVG transform：{name}。")
    };

    private static float Radians(float degrees) => degrees * MathF.PI / 180;
}