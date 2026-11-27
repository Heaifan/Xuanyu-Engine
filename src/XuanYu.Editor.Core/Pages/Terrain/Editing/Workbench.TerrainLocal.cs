using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    public void ResizeLocalTerrain(float width, float length, float spacing)
    {
        if (!CanEditLocalTerrain)
            throw new InvalidOperationException("仅本地空白 Terrain 可调整尺寸。");

        LocalTerrainSamplePlan plan = LocalTerrainSampling.Plan(
            width, length, spacing);
        var map = TerrainPreviewMap.CreateBlank(plan.CellsX, plan.CellsY,
            plan.SpacingX, plan.SpacingY);
        var next = new TerrainPreviewSession(map, Scene,
            _terrainPreview?.TerrainObjectId, centered: true);
        _terrainPreview = next;
        TerrainSelected = true;
        Log.Write($"[E5-04A] 重建本地 Terrain：{width:0.##} × " +
            $"{length:0.##}m，采样 {plan.CellsX + 1}×{plan.CellsY + 1}，" +
            $"实际间距 {plan.SpacingX:0.###}×{plan.SpacingY:0.###}m" +
            (plan.Automatic ? $"（目标 {spacing:0.##}m，自动调整）" : ""));
        Changed?.Invoke();
    }

    private void RebindRestoredLocalTerrain()
    {
        if (Scene.LocalTerrainMap is not { } map ||
            Scene.LocalTerrainId is not Guid id)
        {
            _terrainPreview = null;
            return;
        }
        _terrainPreview = new TerrainPreviewSession(
            map, Scene, centered: true, existingId: id);
        Log.Write($"[E5-04A] 已恢复 Terrain：" +
            $"{map.WidthMeters:0.##} × {map.LengthMeters:0.##}m");
    }
}