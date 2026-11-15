using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

internal enum VkSelectionPass { Base, Mask, Outline }

internal static class VkSelectionPipelineState
{
    public static void Apply(VkSelectionPass pass,
        ref PipelineDepthStencilStateCreateInfo depth,
        ref PipelineColorBlendAttachmentState color)
    {
        if (pass == VkSelectionPass.Base) return;
        depth.StencilTestEnable = true;
        depth.DepthWriteEnable = false;
        depth.DepthCompareOp = pass == VkSelectionPass.Mask
            ? CompareOp.Equal : CompareOp.LessOrEqual;
        var stencil = new StencilOpState
        {
            CompareOp = pass == VkSelectionPass.Mask
                ? CompareOp.Always : CompareOp.NotEqual,
            FailOp = StencilOp.Keep,
            PassOp = pass == VkSelectionPass.Mask
                ? StencilOp.Replace : StencilOp.Keep,
            DepthFailOp = StencilOp.Keep,
            CompareMask = 0xFF,
            WriteMask = pass == VkSelectionPass.Mask ? 0xFFu : 0u,
            Reference = 1
        };
        depth.Front = stencil;
        depth.Back = stencil;
        if (pass == VkSelectionPass.Mask)
            color.ColorWriteMask = 0;
    }
}