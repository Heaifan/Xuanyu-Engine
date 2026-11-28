using System.Globalization;
using System.Text.RegularExpressions;

namespace XuanYu.Render;

internal static class SvgNumbers
{
    private const string Number = @"[+-]?(?:[0-9]+(?:\.[0-9]*)?|\.[0-9]+)(?:[eE][+-]?[0-9]+)?";
    private static readonly Regex Tokens = new(Number + "|[A-Za-z]|[ ,\t\r\n]+|.",
        RegexOptions.CultureInvariant);

    public static string[] Tokenize(string text) => Tokens.Matches(text)
        .Select(m => m.Value).Where(t => t.Trim(' ', ',', '\t', '\r', '\n').Length != 0).ToArray();

    public static float Value(string text)
    {
        if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value) ||
            !float.IsFinite(value)) throw new InvalidDataException($"无效 SVG 数值：{text}。");
        return value;
    }

    public static float NonNegative(string text)
    {
        float value = Value(text);
        if (value < 0) throw new InvalidDataException("SVG 尺寸不能为负。");
        return value;
    }

    public static float[] Parse(string text) => Tokenize(text).Select(Value).ToArray();
}