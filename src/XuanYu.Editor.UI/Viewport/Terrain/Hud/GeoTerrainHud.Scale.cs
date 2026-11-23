using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.Editor.UI;

internal sealed partial class GeoTerrainHud
{
    private GeoTerrainHudScale? _lastScale;

    public void ShowScale(GeoTerrainHudScale? value)
    {
        if (_lastScale == value && _scaleText.Text is not null) return;
        _lastScale = value;
        if (value is not { } scale)
        {
            _scaleBar.IsVisible = false;
            _scaleText.Text = "比例尺 —";
            return;
        }
        _scaleBar.IsVisible = true;
        _scaleBar.Width = scale.WidthDip;
        _scaleText.Text = scale.Meters >= 1000d
            ? $"≈ {scale.Meters / 1000d:0.##} km"
            : $"≈ {scale.Meters:0.##} m";
    }
}