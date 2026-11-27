using System.Numerics;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    private enum TerrainEditTool { Sculpt, Smooth, Elevation, Flatten, Mountain, Thermal, Rain }
    private TerrainEditTool _terrainEditTool;
    public bool IsTerrainElevationTool => _terrainEditTool == TerrainEditTool.Elevation;
    public bool IsTerrainSmoothTool => _terrainEditTool == TerrainEditTool.Smooth;
    public bool IsTerrainFlattenTool => _terrainEditTool == TerrainEditTool.Flatten;
    public bool IsTerrainMountainTool => _terrainEditTool == TerrainEditTool.Mountain;
    public bool IsTerrainThermalTool => _terrainEditTool == TerrainEditTool.Thermal;
    public bool IsTerrainRainTool => _terrainEditTool == TerrainEditTool.Rain;
    public float TerrainMountainHeight { get; private set; } = 30f;
    public bool TerrainElevationArea { get; private set; }
    public float TerrainTargetElevation { get; private set; }
    public bool CanUndoTerrain => CanPreviewTerrainBrush && _sculptStroke is null &&
        _terrainPreview!.CanUndoTerrain;
    public bool CanRedoTerrain => CanPreviewTerrainBrush && _sculptStroke is null &&
        _terrainPreview!.CanRedoTerrain;

    public void SelectTerrainElevationTool(bool enabled) => SelectTerrainEditTool(
        enabled ? TerrainEditTool.Elevation : TerrainEditTool.Sculpt);

    public void SelectTerrainSmoothTool() => SelectTerrainEditTool(TerrainEditTool.Smooth);

    public void SelectTerrainFlattenTool() => SelectTerrainEditTool(TerrainEditTool.Flatten);

    public void SelectTerrainMountainTool() => SelectTerrainEditTool(TerrainEditTool.Mountain);

    public void SelectTerrainThermalTool() => SelectTerrainEditTool(TerrainEditTool.Thermal);
    public void SelectTerrainRainTool() => SelectTerrainEditTool(TerrainEditTool.Rain);

    public void SetTerrainMountainHeight(float height)
    {
        if (!float.IsFinite(height) || height <= 0f)
            throw new ArgumentOutOfRangeException(nameof(height));
        TerrainMountainHeight = height;
    }

    private void SelectTerrainEditTool(TerrainEditTool tool)
    {
        if (!CanPreviewTerrainBrush || _terrainEditTool == tool) return;
        EndTerrainSculpt();
        _terrainEditTool = tool;
        Changed?.Invoke();
    }

    public void SetTerrainElevation(float target, bool area)
    {
        if (!float.IsFinite(target)) throw new ArgumentOutOfRangeException(nameof(target));
        TerrainTargetElevation = target;
        TerrainElevationArea = area;
    }

    public bool TrySampleTerrainElevation(Vector2 pixel, uint width, uint height)
    {
        if (!CanPreviewTerrainBrush || !_terrainPreview!.TryPick(pixel, width, height, out var hit))
            return false;
        SelectTerrainElevationTool(true);
        SetTerrainElevation(hit.WorldPosition.Z, TerrainElevationArea);
        Changed?.Invoke();
        return true;
    }

    public void UndoTerrain()
    {
        if (!CanUndoTerrain) return;
        _terrainPreview!.UndoTerrainEdit();
        Log.Write("[E5-05B] 撤销地形笔划");
        Changed?.Invoke();
    }

    public void RedoTerrain()
    {
        if (!CanRedoTerrain) return;
        _terrainPreview!.RedoTerrainEdit();
        Log.Write("[E5-05B] 重做地形笔划");
        Changed?.Invoke();
    }
}