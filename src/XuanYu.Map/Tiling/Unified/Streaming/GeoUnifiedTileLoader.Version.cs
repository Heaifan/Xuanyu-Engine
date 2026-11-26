using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace XuanYu.World.Maps;

public sealed partial class GeoUnifiedTileLoader
{
    private readonly Dictionary<(int, int), byte[]> _fingerprints = [];
    private long _generation;
    private int _versionOperations;
    internal Guid Identity { get; } = Guid.NewGuid();

    internal long Generation { get { lock (_gate) return _generation; } }

    public bool TryAdvanceVersion()
    {
        lock (_gate)
        {
            if (_reserved != 0 || _requests.Count != 0 || _queue.Count != 0 || _pins != 0 || _versionOperations != 0)
                return false;
            _fingerprints.Clear();
            _seams.Clear();
            _evictable.Clear();
            _generation++;
            return true;
        }
    }

    internal IDisposable BeginVersionOperation(long generation)
    {
        lock (_gate)
        {
            if (generation != _generation) throw new InvalidOperationException("地形数据版本已切换。");
            _versionOperations++;
            return new VersionOperation(this);
        }
    }

    private sealed class VersionOperation(GeoUnifiedTileLoader owner) : IDisposable
    {
        private GeoUnifiedTileLoader? _owner = owner;
        public void Dispose()
        {
            var current = Interlocked.Exchange(ref _owner, null);
            if (current is null) return;
            lock (current._gate) current._versionOperations--;
        }
    }

    private static byte[] Fingerprint(NasademTile tile) =>
        SHA256.HashData(MemoryMarshal.AsBytes(tile.SamplesNorthToSouth.Span));

    private void CheckVersion((int, int) key, byte[] hash)
    {
        if (_fingerprints.TryGetValue(key, out var old))
        {
            if (!old.AsSpan().SequenceEqual(hash))
                throw new InvalidDataException("高程源已变化，必须创建新的数据版本。");
        }
        else _fingerprints.Add(key, hash);
    }
}
