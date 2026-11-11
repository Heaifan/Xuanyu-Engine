
using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

internal static unsafe class VkFrameCommands
{
    public static void Record(
        Vk vk, CommandBuffer command, VkTriangleScene scene,
        uint imageIndex, Extent2D extent)
    {
        Check(vk.ResetCommandBuffer(command, 0), "重置命令缓冲区");

        var begin = new CommandBufferBeginInfo
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit
        };

        Check(vk.BeginCommandBuffer(command, &begin), "开始命令缓冲区");

        var clear = new ClearValue
        {
            Color = new ClearColorValue
            {
                Float32_0 = 0.035f,
                Float32_1 = 0.105f,
                Float32_2 = 0.220f,
                Float32_3 = 1f
            }
        };

        var pass = new RenderPassBeginInfo
        {
            SType = StructureType.RenderPassBeginInfo,
            RenderPass = scene.RenderPass,
            Framebuffer = scene.GetFramebuffer(imageIndex),
            RenderArea = new Rect2D(new Offset2D(0, 0), extent),
            ClearValueCount = 1,
            PClearValues = &clear
        };

        vk.CmdBeginRenderPass(command, &pass, SubpassContents.Inline);
        vk.CmdBindPipeline(
            command, PipelineBindPoint.Graphics, scene.Pipeline);

        Silk.NET.Vulkan.Buffer buffer = scene.VertexBuffer;
        ulong offset = 0;
        vk.CmdBindVertexBuffers(command, 0, 1, &buffer, &offset);

        float* push = stackalloc float[20]
        {
            1f, 0f, 0f, 0f,
            0f, 1f, 0f, 0f,
            0f, 0f, 1f, 0f,
            0f, 0f, 0f, 1f,
            0f, 0f, 0f, 0f
        };

        vk.CmdPushConstants(
            command, scene.Layout, ShaderStageFlags.VertexBit,
            0, 80, push);

        vk.CmdDraw(command, 3, 1, 0, 0);
        vk.CmdEndRenderPass(command);

        Check(vk.EndCommandBuffer(command), "结束命令缓冲区");
    }

    private static void Check(Result result, string action)
    {
        if (result != Result.Success)
            throw new InvalidOperationException($"{action}失败：{result}");
    }
}
