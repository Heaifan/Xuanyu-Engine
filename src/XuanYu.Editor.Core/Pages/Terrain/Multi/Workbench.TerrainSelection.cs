using XuanYu.World.Maps;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    public IReadOnlyList<TerrainDemEntry> TerrainDemEntries { get; private set; } = [];
    public TerrainDemEntry? SelectedTerrainDem { get; private set; }
    public NasademTileSummary? SelectedTerrainDemSummary { get; private set; }
    public NasademSourceResidency? TerrainSourceResidency => _geoPreview?.SourceResidency;
    public GeoTileBounds? TerrainGeoCoverage => _geoPreview?.Coverage;

    public void SelectTerrainDem(string tileId)
    {
        TerrainDemEntry entry = TerrainDemEntries.First(item => item.TileId == tileId);
        if (ReferenceEquals(entry, SelectedTerrainDem)) return;
        var summary = _geoPreview!.ReadSourceSummary(entry.Info.SouthLatitude, entry.Info.WestLongitude);
        SelectedTerrainDem = entry;
        SelectedTerrainDemSummary = summary;
        TerrainSelected = true;
        _geoPreview!.SetSelectedDem(entry.Bounds);
        TerrainScene.Select(null);
        Changed?.Invoke();
    }

    private void ClearTerrainDemSelection()
    {
        SelectedTerrainDem = null;
        SelectedTerrainDemSummary = null;
        _geoPreview?.SetSelectedDem(null);
    }
}