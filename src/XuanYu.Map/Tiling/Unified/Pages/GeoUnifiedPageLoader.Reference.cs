namespace XuanYu.World.Maps;

public sealed partial class GeoUnifiedPageLoader
{
    public async Task<IDisposable> PinReferenceAsync(GeoUnifiedTile tile)
    {
        using var version = cache.BeginVersionOperation();
        await _worker.WaitAsync().ConfigureAwait(false);
        try
        {
            var keys = catalog.Coordinates.Where(key => Intersects(tile, key.South, key.West)).ToArray();
            if (!loader.CanFitPage(keys.Length, 0))
                throw new InvalidOperationException("参考高程及页面超出预算。");
            if (!loader.TrimUnneeded(keys))
                throw new InvalidOperationException("参考高程仍被其他任务使用。");
            var loaded = await Task.WhenAll(keys.Select(key => loader.Request(key.South, key.West))).ConfigureAwait(false);
            loader.MarkEvictable(keys.Where((_, i) => loaded[i]));
            if (loaded.Any(ok => !ok)) throw new InvalidOperationException("参考高程未全部就绪。");
            return loader.PinResident();
        }
        finally { _worker.Release(); }
    }
}