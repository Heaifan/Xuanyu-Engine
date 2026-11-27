namespace XuanYu.World.Maps;

public sealed partial class GeoUnifiedTileLoader
{
    private int _pins;

    public IDisposable PinResident()
    {
        lock (_gate) { _pins++; return new ResidentPin(this); }
    }

    internal IDisposable PinResident(long generation)
    {
        lock (_gate)
        {
            if (generation != _generation) throw new InvalidOperationException("地形数据版本已切换。");
            _pins++;
            return new ResidentPin(this);
        }
    }

    internal SamplePin PinSample(long generation)
    {
        lock (_gate)
        {
            if (generation != _generation) throw new InvalidOperationException("地形数据版本已切换。");
            _pins++;
            return new SamplePin(this);
        }
    }

    internal readonly ref struct SamplePin(GeoUnifiedTileLoader owner)
    {
        public void Dispose() { lock (owner._gate) owner._pins--; }
    }

    internal bool TryGet(int south, int west, out NasademTile tile)
    {
        lock (_gate)
        {
            if (_requests.TryGetValue((south, west), out var source) && source.Tile is { } found)
            { tile = found; return true; }
            tile = null!;
            return false;
        }
    }

    internal void Install(NasademTile tile)
    {
        var hash = Fingerprint(tile);
        lock (_gate)
        {
            var key = (tile.SouthLatitude, tile.WestLongitude);
            if (_requests.TryGetValue(key, out var old))
            {
                if (ReferenceEquals(old.Tile, tile)) return;
                throw new InvalidOperationException("已发布的数据不能在同一版本中替换。");
            }
            if (Reservation > _budget - _reserved) throw new InvalidOperationException("高程数据预算不足。");
            CheckVersion(key, hash);
            RecordSeams(tile);
            var source = new Entry { Tile = tile };
            source.Completion.SetResult(true);
            _requests.Add(key, source);
            _reserved += Reservation;
        }
    }

    private sealed class ResidentPin(GeoUnifiedTileLoader owner) : IDisposable
    {
        private GeoUnifiedTileLoader? _owner = owner;
        public void Dispose()
        {
            var current = Interlocked.Exchange(ref _owner, null);
            if (current is null) return;
            lock (current._gate) current._pins--;
        }
    }
}