namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    public TerrainWorkspaceMode TerrainMode { get; private set; } =
        TerrainWorkspaceMode.Object;
    public bool IsTerrainEditMode => TerrainMode == TerrainWorkspaceMode.Edit;
    public bool CanEditLocalTerrain =>
        !IsGeoTerrainPreview && TerrainSourcePath is null && !IsSimulating;

    public void SetTerrainMode(TerrainWorkspaceMode mode)
    {
        if (mode == TerrainMode) return;
        if (mode == TerrainWorkspaceMode.Edit && !CanEditLocalTerrain)
            throw new InvalidOperationException("当前地形来源尚未接入可编辑模式。");
        TerrainMode = mode;
        Log.Write($"[E5-04B] 地形模式：{(IsTerrainEditMode ? "编辑" : "对象")}");
        Changed?.Invoke();
    }
}