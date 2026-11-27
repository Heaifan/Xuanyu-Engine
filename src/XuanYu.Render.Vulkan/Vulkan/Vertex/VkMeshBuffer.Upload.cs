using Silk.NET.Vulkan;
using XuanYu.Render;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe partial class VkMeshBuffer
{
    public bool HasNoDemUv => _demUv.IsEmpty;

    private static void CopyVertices(void* mapped, RenderMesh mesh,
        ReadOnlyMemory<float> demUv) =>
        CopyVertices(mapped, mesh, demUv, 0, mesh.VertexCount);

    private static void CopyVertices(void* mapped, RenderMesh mesh,
        ReadOnlyMemory<float> demUv, int first, int count)
    {
        ReadOnlySpan<float> data = mesh.Vertices.Span;
        ReadOnlySpan<float> elevations = mesh.TerrainElevations.Span;
        ReadOnlySpan<float> shaded = mesh.TerrainShadedColors.Span;
        ReadOnlySpan<float> uv = demUv.Span;
        float* dst = (float*)mapped;
        for (int i = first; i < first + count; i++)
        {
            int p = i * VertexFloatCount;
            for (int j = 0; j < 7; j++) dst[p + j] = data[i * 7 + j];
            for (int j = 0; j < 3; j++) dst[p + 9 + j] = data[i * 7 + 3 + j];
            if (!shaded.IsEmpty)
                for (int j = 0; j < 3; j++) dst[p + 3 + j] = shaded[i * 3 + j];
            if (!elevations.IsEmpty)
            {
                int triangle = i / 3 * 3;
                bool level = elevations[triangle] == elevations[triangle + 1] &&
                    elevations[triangle + 1] == elevations[triangle + 2];
                dst[p + 6] = level ? 0f : elevations[i];
            }
            dst[p + 7] = uv.IsEmpty ? 0f : uv[i * 2];
            dst[p + 8] = uv.IsEmpty ? 0f : uv[i * 2 + 1];
        }
    }
    // 两条渲染路径均在上一帧 Fence 完成后进入 SyncMeshes。
    public long UpdateLocalTerrain(RenderMesh previous, RenderMesh next)
    {
        var ranges = LocalTerrainVertexChanges.Find(previous, next);
        if (ranges.Count == 0) return 0;
        ulong bytes = (ulong)next.VertexCount * VertexFloatCount * sizeof(float);
        void* mapped = null;
        Check(_vk.MapMemory(_device, _memory, 0, bytes, 0, &mapped), "映射地形更新缓冲区");
        long written = 0;
        try
        {
            foreach (var (first, count) in ranges)
            {
                CopyVertices(mapped, next, default, first, count);
                written += (long)count * VertexFloatCount * sizeof(float);
            }
        }
        finally { _vk.UnmapMemory(_device, _memory); }
        return written;
    }
}