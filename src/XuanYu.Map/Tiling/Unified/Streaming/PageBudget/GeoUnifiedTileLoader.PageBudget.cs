namespace XuanYu.World.Maps;

public sealed partial class GeoUnifiedTileLoader
{
    private long _ownedPageBytes;

    internal bool TryReserveOwnedPage(long bytes)
    {
        lock (_gate)
        {
            if (bytes > _budget - _reserved) return false;
            _ownedPageBytes += bytes;
            _reserved += bytes;
            return true;
        }
    }

    internal void ReleaseOwnedPage(long bytes)
    {
        lock (_gate)
        {
            if (bytes <= 0 || bytes > _ownedPageBytes) throw new ArgumentOutOfRangeException(nameof(bytes));
            _ownedPageBytes -= bytes;
            _reserved -= bytes;
            StartWaiting();
        }
    }
}
