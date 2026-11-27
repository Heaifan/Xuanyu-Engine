namespace XuanYu.World.Maps;

public sealed partial class GeoUnifiedLodSession
{
    private void Prune(GeoUnifiedSelection selection)
    {
        var keep = new HashSet<GeoUnifiedTile>(Coverage.Leaves);
        foreach (var tile in Coverage.Leaves)
            if (tile.Level < 15) keep.Add(tile.Parent());
        if (selection.PendingCoverage is { } pending) keep.UnionWith(pending.Leaves);
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