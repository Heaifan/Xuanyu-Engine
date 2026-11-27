using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview;

public sealed partial class PreviewScene
{
    internal MapDocument? LocalTerrainMap { get; private set; }
    internal Guid? LocalTerrainId { get; private set; }
    public event Action? Restored;

    internal void RegisterLocalTerrain(MapDocument? map, Guid? id)
    {
        LocalTerrainMap = map;
        LocalTerrainId = id;
    }

    private void ClearLocalTerrain(Guid id)
    {
        if (LocalTerrainId == id)
            RegisterLocalTerrain(null, null);
    }
}