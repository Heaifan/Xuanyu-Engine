namespace XuanYu.World.Maps;

internal sealed class GeoHeightTileCache(int capacity)
{
    private readonly Dictionary<GeoTileId, GeoHeightTile> _tiles = new();
    private readonly Queue<GeoTileId> _order = new();

    public int Count => _tiles.Count;

    public bool TryGet(GeoTileId id, out GeoHeightTile? tile) =>
        _tiles.TryGetValue(id, out tile);

    public void Add(GeoHeightTile tile)
    {
        if (_tiles.ContainsKey(tile.Id)) return;
        if (_tiles.Count == capacity) _tiles.Remove(_order.Dequeue());
        _tiles.Add(tile.Id, tile);
        _order.Enqueue(tile.Id);
    }
}