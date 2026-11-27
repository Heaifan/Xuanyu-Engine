using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class TerrainPreviewSession
{
    private const int TerrainHistoryLimit = 32;
    private readonly List<(float[] Before, float[] After)> _terrainUndo = [];
    private readonly List<(float[] Before, float[] After)> _terrainRedo = [];
    internal bool CanUndoTerrain => _terrainUndo.Count != 0;
    internal bool CanRedoTerrain => _terrainRedo.Count != 0;

    internal void RecordTerrainEdit(TerrainHeightField reference)
    {
        var before = reference.Samples.ToArray();
        var after = Map.HeightField.Samples.ToArray();
        if (before.AsSpan().SequenceEqual(after)) return;
        _terrainUndo.Add((before, after));
        if (_terrainUndo.Count > TerrainHistoryLimit) _terrainUndo.RemoveAt(0);
        _terrainRedo.Clear();
    }

    internal void UndoTerrainEdit() => Transfer(_terrainUndo, _terrainRedo, undo: true);
    internal void RedoTerrainEdit() => Transfer(_terrainRedo, _terrainUndo, undo: false);

    private void Transfer(List<(float[] Before, float[] After)> source,
        List<(float[] Before, float[] After)> destination, bool undo)
    {
        var item = source[^1];
        var values = undo ? item.Before : item.After;
        var field = Map.HeightField;
        for (int y = 0; y < field.SamplesY; y++)
        for (int x = 0; x < field.SamplesX; x++)
            field.SetHeight(x, y, values[y * field.SamplesX + x]);
        RefreshSculptMesh();
        source.RemoveAt(source.Count - 1);
        destination.Add(item);
    }
}