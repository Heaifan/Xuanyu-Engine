using XuanYu.Render;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe partial class VkMeshBuffer
{
    private static void CopyVertices(void* mapped, RenderMesh mesh,
        ReadOnlyMemory<float> demUv)
    {
        ReadOnlySpan<float> data = mesh.Vertices.Span;
        ReadOnlySpan<float> elevations = mesh.TerrainElevations.Span;
        ReadOnlySpan<float> shaded = mesh.TerrainShadedColors.Span;
        ReadOnlySpan<float> uv = demUv.Span;
        float* dst = (float*)mapped;
        for (int i = 0; i < mesh.VertexCount; i++)
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
}