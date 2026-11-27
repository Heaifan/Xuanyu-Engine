namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    public TerrainToolGroup ActiveTerrainToolGroup => IsTerrainMountainTool
        ? TerrainToolGroup.Generation
        : IsTerrainThermalTool || IsTerrainRainTool
            ? TerrainToolGroup.Evolution : TerrainToolGroup.Sculpt;

    public bool CanSelectTerrainToolGroup(TerrainToolGroup group) =>
        CanPreviewTerrainBrush && group is (TerrainToolGroup.Generation or
            TerrainToolGroup.Sculpt or TerrainToolGroup.Evolution);

    public void SelectTerrainToolGroup(TerrainToolGroup group)
    {
        if (!CanSelectTerrainToolGroup(group) || group == ActiveTerrainToolGroup) return;
        switch (group)
        {
            case TerrainToolGroup.Generation: SelectTerrainMountainTool(); break;
            case TerrainToolGroup.Sculpt: SelectTerrainElevationTool(false); break;
            case TerrainToolGroup.Evolution: SelectTerrainThermalTool(); break;
        }
    }
}