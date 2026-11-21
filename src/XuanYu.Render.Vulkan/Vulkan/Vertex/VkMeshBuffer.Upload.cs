using XuanYu.Render;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe partial class VkMeshBuffer
{
    private static void CopyVertices(void* mapped, RenderMesh mesh,
        ReadOnlyMemory<float> demUv)
    {
        ReadOnlySpan<float> data = mesh.Vertices.Span;
        ReadOnlySpan<float> elevations = mesh.TerrainElevations.Span;
        ReadOnlySpan<float> uv = demUv.Span;
        float* dst = (float*)mapped;
        for (int i = 0; i < mesh.VertexCount; i++)
        {
            for (int j = 0; j < 7; j++)
                dst[i * 9 + j] = data[i * 7 + j];
            if (!elevations.IsEmpty)
            {
                int triangle = i / 3 * 3;
                bool level = elevations[triangle] == elevations[triangle + 1] &&
                    elevations[triangle + 1] == elevations[triangle + 2];
                dst[i * 9 + 6] = level ? 0f : elevations[i];
            }
            dst[i * 9 + 7] = uv.IsEmpty ? 0f : uv[i * 2];
            dst[i * 9 + 8] = uv.IsEmpty ? 0f : uv[i * 2 + 1];
        }
    }
}