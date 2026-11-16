
using XuanYu.Render;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class TerrainPreviewSession
{
    private const long CacheLimitBytes = 96L * 1024 * 1024;
    private readonly Dictionary<(Guid Id, int Step),
        (RenderMesh Mesh, long Order)> _cache = [];
    private long _cacheBytes;
    private long _nextOrder;

    private bool HasCached(Chunk chunk, int step) =>
        _cache.ContainsKey((chunk.Id, step));

    private RenderMesh? TakeCached(Chunk chunk, int step)
    {
        if (!_cache.Remove((chunk.Id, step), out var value)) return null;
        _cacheBytes -= Size(value.Mesh);
        return value.Mesh;
    }

    private void CacheMesh(Chunk chunk, int step, RenderMesh mesh)
    {
        if (step == chunk.Step || HasCached(chunk, step)) return;
        _cache.Add((chunk.Id, step), (mesh, ++_nextOrder));
        _cacheBytes += Size(mesh);
        while (_cacheBytes > CacheLimitBytes)
        {
            var oldest = _cache.MinBy(item => item.Value.Order);
            _cache.Remove(oldest.Key);
            _cacheBytes -= Size(oldest.Value.Mesh);
        }
    }

    private void ApplyMesh(Chunk chunk, int step, RenderMesh mesh)
    {
        int previousStep = chunk.Step;
        RenderMesh previousMesh = chunk.Mesh;
        Scene.ReplaceMesh(chunk.Id, mesh);
        chunk.Mesh = mesh;
        chunk.Step = step;
        CacheMesh(chunk, previousStep, previousMesh);
    }

    private static long Size(RenderMesh mesh) =>
        (long)mesh.Vertices.Length * sizeof(float);
}
