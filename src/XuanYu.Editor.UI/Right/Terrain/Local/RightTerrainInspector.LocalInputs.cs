using System.Globalization;
using Avalonia.Controls;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI.Right;

internal sealed partial class RightTerrainInspector
{
    private static bool ReadNumbers(string? x, string? y, string? s,
        out float width, out float length, out float spacing)
    {
        bool okX = float.TryParse(x, NumberStyles.Float,
            CultureInfo.InvariantCulture, out width);
        bool okY = float.TryParse(y, NumberStyles.Float,
            CultureInfo.InvariantCulture, out length);
        bool okS = float.TryParse(s, NumberStyles.Float,
            CultureInfo.InvariantCulture, out spacing);
        return okX && okY && okS;
    }

    private static string SamplePreview(string? x, string? y, string? s)
    {
        if (!ReadNumbers(x, y, s, out float width,
                out float length, out float spacing))
            return "预计采样数：—";
        try
        {
            LocalTerrainSamplePlan plan = LocalTerrainSampling.Plan(
                width, length, spacing);
            string title = plan.Automatic ? "自动采样" : "预计采样";
            return $"{title}：{plan.CellsX + 1} × {plan.CellsY + 1}，" +
                $"实际间距 {plan.SpacingX:0.###} × {plan.SpacingY:0.###}m";
        }
        catch (ArgumentOutOfRangeException error)
        {
            return error.Message;
        }
    }

    private void ApplyLocalSize(string? x, string? y, string? s,
        TextBlock status)
    {
        if (!ReadNumbers(x, y, s,
                out float width, out float length, out float spacing))
        {
            status.Text = "请输入有效数字（用 . 表示小数点）。";
            return;
        }
        try
        {
            _wb.ResizeLocalTerrain(width, length, spacing);
        }
        catch (Exception error) when (error is ArgumentException
            or InvalidOperationException)
        {
            status.Text = error.Message;
        }
    }
}