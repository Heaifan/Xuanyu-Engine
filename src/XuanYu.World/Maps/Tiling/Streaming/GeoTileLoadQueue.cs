namespace XuanYu.World.Maps;

internal sealed class GeoTileLoadQueue
{
    private readonly Queue<GeoTileId> _items = new();
    private readonly HashSet<GeoTileId> _unique = new();

    public int Count => _items.Count;

    public void Enqueue(GeoTileId id)
    {
        if (_unique.Add(id)) _items.Enqueue(id);
    }

    public GeoTileId Dequeue()
    {
        GeoTileId id = _items.Dequeue();
        _unique.Remove(id);
        return id;
    }
}