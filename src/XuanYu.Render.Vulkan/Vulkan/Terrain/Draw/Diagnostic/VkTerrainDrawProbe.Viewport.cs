using Silk.NET.Vulkan;
using XuanYu.Render;
using static XuanYu.Render.Vulkan.VkTerrainBuffer;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe partial class VkTerrainDrawProbe
{
    public byte[] DrawViewport(VkMeshScene scene, Extent2D extent, Image image,
        VkTerrainBuffer readback, IReadOnlyList<RenderDraw> draws,
        Action<CommandBuffer> terrain)
    {
        VkFrameCommands.Record(_vk, _command, scene, 0, extent, draws, terrain);
        SubmitAndWait();

        // 仅测试读回；正式共享纹理路径不调用本方法。
        Check(_vk.ResetCommandBuffer(_command, 0), "重置 G07 视口诊断命令");
        var begin = new CommandBufferBeginInfo
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit
        };
        Check(_vk.BeginCommandBuffer(_command, &begin), "开始 G07 视口诊断读回");
        CopyToHost(image, extent, readback);
        Check(_vk.EndCommandBuffer(_command), "结束 G07 视口诊断读回");
        SubmitAndWait();
        return readback.Read();
    }
}