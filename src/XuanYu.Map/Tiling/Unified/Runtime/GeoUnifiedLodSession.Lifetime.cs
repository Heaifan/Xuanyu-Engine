namespace XuanYu.World.Maps;

public sealed partial class GeoUnifiedLodSession
{
    private void Prune(GeoUnifiedTile? blocked)
    {
        var keep = new HashSet<GeoUnifiedTile>(Coverage.Leaves);
        foreach (var tile in Coverage.Leaves)
            if (tile.Level < 15) keep.Add(tile.Parent());
        if (blocked is { Level: > 0 } parent)
            for (int y = 0; y < 2; y++)
                for (int x = 0; x < 2; x++) keep.Add(parent.Child(x, y));
        foreach (var key in _resources.Keys.Where(key => !keep.Contains(key.Tile)).ToArray())
        {
            var task = _resources[key];
            _resources.Remove(key);
            _retryAfter.Remove(key);
            Retire(task);
        }
    }

    private static void Retire(Task<Resource> task)
    {
        _ = task.ContinueWith(completed =>
        {
            if (completed.IsCompletedSuccessfully) completed.Result.Dispose();
            else _ = completed.Exception;
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        foreach (var task in _resources.Values) Retire(task);
        _resources.Clear();
        _retryAfter.Clear();
    }
}
