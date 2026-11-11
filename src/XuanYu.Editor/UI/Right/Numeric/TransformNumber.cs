using System.Globalization;

namespace XuanYu.Editor.UI.Right;

public static class TransformNumber
{
    public static bool TryParse(string? text, out float value) =>
        float.TryParse(text, NumberStyles.Float,
            CultureInfo.InvariantCulture, out value) && float.IsFinite(value);

    public static float Step(float value, float increment, float count) =>
        value + increment * count;

    public static float Scrub(float start, float step, float pixels) =>
        Step(start, step, MathF.Truncate(pixels / 5f));

    public static string Display(float value) =>
        value.ToString("0.######", CultureInfo.InvariantCulture);
}