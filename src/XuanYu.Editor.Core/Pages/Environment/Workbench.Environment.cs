using System.Numerics;
using XuanYu.Editor.Preview;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    private readonly MetricGridOverlay _metricGrid = new();
    public bool SkyEnabled { get; private set; } = true;
    public bool WorldGridEnabled { get; private set; } = true;
    public bool TerrainGridEnabled { get; private set; } = true;

    public void SetSkyEnabled(bool enabled)
    {
        if (SkyEnabled == enabled) return;
        SkyEnabled = enabled;
        Log.Write($"[E5/环境] 天空：{(enabled ? "开" : "关")}");
        Changed?.Invoke();
    }

    public void SetWorldGridEnabled(bool enabled)
    {
        if (WorldGridEnabled == enabled) return;
        WorldGridEnabled = enabled;
        Log.Write($"[E5/网格] 世界网格：{(enabled ? "开" : "关")}");
        Changed?.Invoke();
    }

    public void SetTerrainGridEnabled(bool enabled)
    {
        if (TerrainGridEnabled == enabled) return;
        TerrainGridEnabled = enabled;
        Log.Write($"[E5/网格] 地形网格：{(enabled ? "开" : "关")}");
        Changed?.Invoke();
    }

    public string GridScaleText
    {
        get
        {
            double distance = _geoPreview?.Camera.DistanceMeters ??
                (ActiveCamera is { } camera
                    ? Vector3.Distance(camera.Position, camera.Target) : 100d);
            double step = _geoPreview?.G07GridStepMeters ?? GridStep.FromDistance(distance);
            string unit = step >= 1000d ? $"{step / 1000d:0.#} km" : $"{step:0.#} m";
            return CurrentId == PageId.Terrain && _geoPreview is not null
                ? $"地理方格网 · 约 {unit}" : $"三维网格 · {unit}";
        }
    }

    public void FocusGeoWorldPosition(Vector3 local) =>
        _geoPreview!.FocusWorldPosition(local);

    public Matrix4x4 EditorProjection(uint width, uint height) =>
        _geoPreview is { } geo ? geo.EditorViewProjection(width, height)
        : (CurrentId == PageId.Terrain ? TerrainScene : Scene).ViewProjection(width, height);
}