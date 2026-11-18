using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

internal enum VkSelectionPass { Base, Mask, Outline, OverlayMask, OverlayOutline }

internal static class VkSelectionPipelineState
{
    public static void Apply(VkSelectionPass pass,
        ref PipelineDepthStencilStateCreateInfo depth,
        ref PipelineColorBlendAttachmentState color)
    {
        if (pass == VkSelectionPass.Base) return;
        bool mask = pass is VkSelectionPass.Mask or VkSelectionPass.OverlayMask;
        bool overlay = pass is VkSelectionPass.OverlayMask or VkSelectionPass.OverlayOutline;
        uint bit = overlay ? 2u : 1u;
        depth.StencilTestEnable = true;
        depth.DepthTestEnable = !overlay;
        depth.DepthWriteEnable = false;
        depth.DepthCompareOp = mask ? CompareOp.Equal : CompareOp.LessOrEqual;
        // 普通物体与地图叠加使用不同 stencil 位，互不覆盖。
        var stencil = new StencilOpState
        {
            CompareOp = mask ? CompareOp.Always : CompareOp.NotEqual,
            FailOp = StencilOp.Keep,
            PassOp = mask ? StencilOp.Replace : StencilOp.Keep,
            DepthFailOp = StencilOp.Keep,
            CompareMask = bit,
            WriteMask = mask ? bit : 0u,
            Reference = bit
        };
        depth.Front = stencil;
        depth.Back = stencil;
        if (mask) color.ColorWriteMask = 0;
    }
}