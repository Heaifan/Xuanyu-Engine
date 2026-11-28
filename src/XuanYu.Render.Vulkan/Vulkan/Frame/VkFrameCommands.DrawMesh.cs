using Silk.NET.Vulkan;
using XuanYu.Render;

namespace XuanYu.Render.Vulkan;

internal static unsafe partial class VkFrameCommands
{
    internal static void DrawMesh(Vk vk, CommandBuffer cmd, VkMeshScene scene,
        RenderDraw draw, bool outline = false, bool mask = false)
    {
        Silk.NET.Vulkan.Buffer buffer = scene.BufferFor(draw.Mesh);
        ulong offset = 0;
        vk.CmdBindVertexBuffers(cmd, 0, 1, &buffer, &offset);
        bool hasDem = scene.BindDem(cmd, draw) && !outline;
        var gaps = scene.BindContourGaps(cmd, draw);
        bool raw = hasDem && VkMeshScene.DemContourEnabled && !draw.LegacyContourOnly;
        bool water = scene.BindWater(cmd, draw) && !outline;
        float shadeSouth = hasDem && !mask && !draw.DisableTerrainHillshade && VkMeshScene.DemHillshadeEnabled &&
            !draw.Mesh.TerrainShadedColors.IsEmpty && draw.ResidentDem is { } shadeDem
            ? shadeDem.SouthLatitude + 91f : 0f;
        float dx = 0f, dy = 0f;
        if (hasDem && draw.WaterAtlas is { } atlas && draw.ResidentDem is { } dem)
        {
            dx = dem.WestLongitude - atlas.WestLongitude;
            dy = atlas.SouthLatitude - dem.SouthLatitude;
        }
        var m = draw.ModelToClip;
        float* push = stackalloc float[32]
        {
            m.M11, m.M12, m.M13, m.M14,
            m.M21, m.M22, m.M23, m.M24,
            m.M31, m.M32, m.M33, m.M34,
            m.M41, m.M42, m.M43, m.M44,
            1f, 0.722f, 0.188f, outline ? 1f : 0f,
            outline ? 0f : draw.ContourIntervalMeters, draw.ContourOpacity,
            draw.Mesh.TerrainElevations.IsEmpty ? 0f :
                (raw ? 3f : 1f) + draw.ContourIntervalBlend,
            outline ? 0f : (draw.ContourNextIntervalMeters > 0f
                ? draw.ContourNextIntervalMeters : draw.ContourIntervalMeters),
            dx, dy, water ? 1f : 0f, shadeSouth,
            gaps.First, outline || mask ? 0f : gaps.Count,
            draw.ResidentDem is null ? 1f : 0f,
            !outline && !mask && draw.DisableTerrainHillshade && !draw.Mesh.TerrainShadedColors.IsEmpty ? 1f : 0f
        };
        vk.CmdPushConstants(cmd, scene.Layout, ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit,
            0, 128, push);
        if (scene.TryGetIndexBuffer(draw.Mesh, out var index)) { vk.CmdBindIndexBuffer(cmd, index, 0, IndexType.Uint32); vk.CmdDrawIndexed(cmd, (uint)draw.Mesh.IndexCount, 1, 0, 0, 0); } else vk.CmdDraw(cmd, (uint)draw.Mesh.VertexCount, 1, 0, 0);
    }
}