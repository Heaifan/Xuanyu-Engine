namespace XuanYu.World.Maps;

internal sealed partial class NasademTileCache
{
    private Task<NasademTile?>? _pending;
    private (int South, int West) _pendingKey;
    public long BackgroundLoads { get; private set; }
    public bool HasPending => _pending is not null;

    public bool Request(int south, int west)
    {
        if (Find((south, west)) is not null || !_catalog.Coordinates.Contains((south, west)))
            return true;
        if (_pending is not null) return false;
        _pendingKey = (south, west);
        // 工作线程仅解码，不访问 LRU、相机或渲染状态。
        _pending = Task.Run(() => _catalog.Load(south, west));
        return false;
    }

    public void Pump()
    {
        if (_pending is { IsCompleted: true }) CompletePending();
    }

    private NasademTile? CompletePending()
    {
        var task = _pending!;
        _pending = null;
        var tile = task.GetAwaiter().GetResult();
        if (tile is not null)
        {
            Store(tile);
            BackgroundLoads++;
        }
        return tile;
    }
}