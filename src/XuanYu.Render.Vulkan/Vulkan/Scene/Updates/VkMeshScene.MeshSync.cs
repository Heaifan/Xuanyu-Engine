using XuanYu.Render;

namespace XuanYu.Render.Vulkan;

internal sealed partial class VkMeshScene
{
    private long _localUpdates, _localWritten, _localFullBytes;
    public long VertexBufferCreateCount { get; private set; }
    public long VertexBufferUploadBytes { get; private set; }
    private static readonly bool LocalUploadProbe =
        Environment.GetEnvironmentVariable("XYE_E505_GPU") == "1";

    public void SyncMeshes(IReadOnlyList<RenderDraw> draws)
    {
        HashSet<RenderMesh> active = new(draws.Select(x => x.Mesh));
        foreach (RenderDraw draw in draws)
            if (draw.Visible && !_buffers.ContainsKey(draw.Mesh))
                TryReuseLocalBuffer(draw, active);
        foreach (RenderMesh old in _buffers.Keys.ToArray())
            if (!active.Contains(old))
            {
                _buffers[old].Dispose();
                _buffers.Remove(old);
            }
        foreach (RenderDraw draw in draws)
        {
            if (!draw.Visible) continue;
            if (_buffers.TryGetValue(draw.Mesh, out var old))
            {
                if (old.Matches(draw.DemUV)) continue;
                old.Dispose();
                _buffers.Remove(draw.Mesh);
            }
            _buffers.Add(draw.Mesh,
                new VkMeshBuffer(_vk, _gpu, _device, draw.Mesh, draw.DemUV));
            VertexBufferCreateCount++;
            VertexBufferUploadBytes += FullBytes(draw.Mesh);
            if (draw.IsBillboard)
                Console.WriteLine($"[C03/GPU] 二维缓冲区新建：{draw.Mesh.AssetId}，{draw.Mesh.VertexCount} 顶点");
            if (LocalUploadProbe && LocalTerrainVertexChanges.IsLocal(draw.Mesh))
                Console.WriteLine($"[E5/GPU] 新建本地顶点缓冲区：{draw.Mesh.VertexCount} 顶点，{FullBytes(draw.Mesh)} 字节");
        }
    }

    private void TryReuseLocalBuffer(RenderDraw draw, HashSet<RenderMesh> active)
    {
        if (!draw.DemUV.IsEmpty || !LocalTerrainVertexChanges.IsLocal(draw.Mesh)) return;
        RenderMesh? previous = null;
        foreach (var entry in _buffers)
        {
            if (active.Contains(entry.Key) || !entry.Value.HasNoDemUv ||
                !LocalTerrainVertexChanges.CanReuse(entry.Key, draw.Mesh)) continue;
            previous = entry.Key;
            break;
        }
        if (previous is null) return;
        var buffer = _buffers[previous];
        long bytes = buffer.UpdateLocalTerrain(previous, draw.Mesh);
        VertexBufferUploadBytes += bytes;
        _buffers.Remove(previous);
        _buffers.Add(draw.Mesh, buffer);
        ReportLocalUpload(draw.Mesh, bytes);
    }

    private static long FullBytes(RenderMesh mesh) =>
        (long)mesh.VertexCount * VkMeshBuffer.VertexFloatCount * sizeof(float);

    private void ReportLocalUpload(RenderMesh mesh, long bytes)
    {
        if (!LocalUploadProbe) return;
        _localUpdates++;
        _localWritten += bytes;
        _localFullBytes += FullBytes(mesh);
        if (_localUpdates != 1 && _localUpdates % 60 != 0) return;
        Console.WriteLine($"[E5/GPU] 复用次数={_localUpdates} 本次写入={bytes} 字节 " +
            $"完整={FullBytes(mesh)} 字节 累计写入={_localWritten}/{_localFullBytes} 字节 " +
            $"写入比例={(double)_localWritten / _localFullBytes:P2}");
    }
}