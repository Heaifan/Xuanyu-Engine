using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

internal enum VkSelectionPass { Base, Mask, Outline, OverlayMask, OverlayOutline, Billboard }

internal static class VkSelectionPipelineState
{
    public static void Apply(VkSelectionPass pass,
        ref PipelineDepthStencilStateCreateInfo depth,
        ref PipelineColorBlendAttachmentState color)
    {
        if (pass == VkSelectionPass.Base) return;
        if (pass == VkSelectionPass.Billboard)
        {
            // 二维标绘覆盖地表，地球遮挡由全球表现桥判定。
            depth.DepthTestEnable = false;
            depth.DepthWriteEnable = false;
            color.BlendEnable = true;
            color.SrcColorBlendFactor = BlendFactor.SrcAlpha;
            color.DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha;
            color.ColorBlendOp = BlendOp.Add;
            color.SrcAlphaBlendFactor = BlendFactor.One;
            color.DstAlphaBlendFactor = BlendFactor.OneMinusSrcAlpha;
            color.AlphaBlendOp = BlendOp.Add;
            return;
        }
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