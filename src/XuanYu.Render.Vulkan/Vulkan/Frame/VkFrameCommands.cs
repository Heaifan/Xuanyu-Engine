using Silk.NET.Vulkan;
using XuanYu.Render;

namespace XuanYu.Render.Vulkan;

internal static unsafe class VkFrameCommands
{
    public static void Record(
        Vk vk, CommandBuffer command, VkMeshScene scene,
        uint imageIndex, Extent2D extent, IReadOnlyList<RenderDraw> draws)
    {
        scene.SyncContourGaps(draws);
        Check(vk.ResetCommandBuffer(command, 0), "重置命令缓冲区");
        var begin = new CommandBufferBeginInfo
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit
        };
        Check(vk.BeginCommandBuffer(command, &begin), "开始命令缓冲区");
        ClearValue* clears = stackalloc ClearValue[2];
        clears[0] = new ClearValue
        {
            Color = new ClearColorValue
            {
                Float32_0 = 0.035f, Float32_1 = 0.105f,
                Float32_2 = 0.220f, Float32_3 = 1f
            }
        };
        clears[1] = new ClearValue
        {
            DepthStencil = new ClearDepthStencilValue { Depth = 1f, Stencil = 0 }
        };
        var pass = new RenderPassBeginInfo
        {
            SType = StructureType.RenderPassBeginInfo,
            RenderPass = scene.RenderPass,
            Framebuffer = scene.GetFramebuffer(imageIndex),
            RenderArea = new Rect2D(new Offset2D(0, 0), extent),
            ClearValueCount = 2, PClearValues = clears
        };
        vk.CmdBeginRenderPass(command, &pass, SubpassContents.Inline);
        vk.CmdBindPipeline(command, PipelineBindPoint.Graphics, scene.Pipeline);
        foreach (RenderDraw draw in draws)
        {
            if (!draw.Visible) continue;
            DrawMesh(vk, command, scene, draw);
        }
        VkFrameSelection.Record(vk, command, scene, extent, draws);
        vk.CmdEndRenderPass(command);
        Check(vk.EndCommandBuffer(command), "结束命令缓冲区");
    }

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
        float shadeSouth = hasDem && !mask && VkMeshScene.DemHillshadeEnabled &&
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
            gaps.First, outline || mask ? 0f : gaps.Count, 0f, 0f
        };
        vk.CmdPushConstants(cmd, scene.Layout, ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit,
            0, 128, push);
        if (scene.TryGetIndexBuffer(draw.Mesh, out var index)) { vk.CmdBindIndexBuffer(cmd, index, 0, IndexType.Uint32); vk.CmdDrawIndexed(cmd, (uint)draw.Mesh.IndexCount, 1, 0, 0, 0); } else vk.CmdDraw(cmd, (uint)draw.Mesh.VertexCount, 1, 0, 0);
    }

    private static void Check(Result result, string action)
    {
        if (result != Result.Success)
            throw new InvalidOperationException($"{action}失败：{result}");
    }
}