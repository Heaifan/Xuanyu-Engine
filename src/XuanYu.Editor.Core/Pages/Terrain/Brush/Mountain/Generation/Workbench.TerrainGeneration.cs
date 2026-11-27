using XuanYu.Editor.TerrainEditing;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    public bool GenerateAlpineTerrain()
    {
        if (!CanPreviewTerrainBrush || !IsTerrainMountainTool) return false;
        EndTerrainSculpt();
        var session = _terrainPreview!;
        var field = session.Map.HeightField;
        var before = new TerrainHeightField(field.SamplesX, field.SamplesY,
            field.SpacingXMeters, field.SpacingYMeters);
        for (int y = 0; y < field.SamplesY; y++)
        for (int x = 0; x < field.SamplesX; x++)
            before.SetHeight(x, y, field.GetHeight(x, y));
        int changed = TerrainAlpineGenerator.Apply(field, TerrainMountainHeight, TerrainNaturalPeakSeed);
        if (changed == 0) return false;
        session.RefreshSculptMesh();
        session.RecordTerrainEdit(before);
        Log.Write($"[E5-M-R01-D02-R01] 阿尔卑斯地貌生成：{field.SamplesX}×{field.SamplesY}，" +
            $"种子={TerrainNaturalPeakSeed}，最大相对隆起={TerrainMountainHeight:0.###}m，样本修改={changed}");
        Changed?.Invoke();
        return true;
    }
}