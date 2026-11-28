using Silk.NET.Vulkan;
using XuanYu.Render;
namespace XuanYu.Render.Vulkan;
internal static unsafe partial class VkFrameCommands
{
    public static void Record(
        Vk vk, CommandBuffer command, VkMeshScene scene,
        uint imageIndex, Extent2D extent, IReadOnlyList<RenderDraw> draws,
        Action<CommandBuffer>? recordTerrain = null)
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
            if (!draw.Visible || draw.IsBillboard) continue;
            DrawMesh(vk, command, scene, draw);
        }
        recordTerrain?.Invoke(command);
        bool billboardBound = false;
        foreach (RenderDraw draw in draws.Where(d => d.Visible && d.IsBillboard)
            .OrderByDescending(d => d.ModelToClip.M43 / d.ModelToClip.M44))
        {
            if (!billboardBound)
            {
                vk.CmdBindPipeline(command, PipelineBindPoint.Graphics, scene.BillboardPipeline);
                billboardBound = true;
            }
            DrawMesh(vk, command, scene, draw);
        }
        VkFrameSelection.Record(vk, command, scene, extent, draws);
        vk.CmdEndRenderPass(command);
        Check(vk.EndCommandBuffer(command), "结束命令缓冲区");
    }

    private static void Check(Result result, string action)
    {
        if (result != Result.Success)
            throw new InvalidOperationException($"{action}失败：{result}");
    }
}