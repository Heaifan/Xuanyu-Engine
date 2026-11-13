using Silk.NET.Vulkan;
namespace XuanYu.Render.Vulkan;

internal static unsafe class VkTrianglePipelineStates
{
    public static Pipeline Create(Vk vk, Device device, RenderPass pass,
        PipelineLayout layout, Extent2D extent, ShaderModule vert, ShaderModule frag)
    {
        byte[] entry = "main\0"u8.ToArray();
        fixed (byte* name = entry)
        {
            PipelineShaderStageCreateInfo* stages = stackalloc PipelineShaderStageCreateInfo[2];
            stages[0] = new PipelineShaderStageCreateInfo
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.VertexBit, Module = vert, PName = name
            };
            stages[1] = new PipelineShaderStageCreateInfo
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.FragmentBit, Module = frag, PName = name
            };
            var binding = new VertexInputBindingDescription
            {
                Binding = 0, Stride = 28, InputRate = VertexInputRate.Vertex
            };
            VertexInputAttributeDescription* attrs = stackalloc VertexInputAttributeDescription[2];
            attrs[0] = new VertexInputAttributeDescription
            {
                Location = 0, Binding = 0, Format = Format.R32G32B32Sfloat, Offset = 0
            };
            attrs[1] = new VertexInputAttributeDescription
            {
                Location = 1, Binding = 0, Format = Format.R32G32B32A32Sfloat, Offset = 12
            };
            var input = new PipelineVertexInputStateCreateInfo
            {
                SType = StructureType.PipelineVertexInputStateCreateInfo,
                VertexBindingDescriptionCount = 1, PVertexBindingDescriptions = &binding,
                VertexAttributeDescriptionCount = 2, PVertexAttributeDescriptions = attrs
            };
            var assembly = new PipelineInputAssemblyStateCreateInfo
            {
                SType = StructureType.PipelineInputAssemblyStateCreateInfo,
                Topology = PrimitiveTopology.TriangleList
            };
            var viewport = new Viewport { X = 0, Y = 0, Width = extent.Width,
                Height = extent.Height, MinDepth = 0, MaxDepth = 1 };
            var scissor = new Rect2D(new Offset2D(0, 0), extent);
            var view = new PipelineViewportStateCreateInfo
            {
                SType = StructureType.PipelineViewportStateCreateInfo,
                ViewportCount = 1, PViewports = &viewport,
                ScissorCount = 1, PScissors = &scissor
            };
            var raster = new PipelineRasterizationStateCreateInfo
            {
                SType = StructureType.PipelineRasterizationStateCreateInfo,
                PolygonMode = PolygonMode.Fill, CullMode = CullModeFlags.None,
                FrontFace = FrontFace.CounterClockwise, LineWidth = 1
            };
            var multisample = new PipelineMultisampleStateCreateInfo
            {
                SType = StructureType.PipelineMultisampleStateCreateInfo,
                RasterizationSamples = VkMsaaSettings.Samples
            };
            var depth = new PipelineDepthStencilStateCreateInfo
            {
                SType = StructureType.PipelineDepthStencilStateCreateInfo,
                DepthTestEnable = true, DepthWriteEnable = true,
                DepthCompareOp = CompareOp.Less
            };
            var blendAttachment = new PipelineColorBlendAttachmentState
            {
                ColorWriteMask = ColorComponentFlags.RBit | ColorComponentFlags.GBit |
                    ColorComponentFlags.BBit | ColorComponentFlags.ABit
            };
            var blend = new PipelineColorBlendStateCreateInfo
            {
                SType = StructureType.PipelineColorBlendStateCreateInfo,
                AttachmentCount = 1, PAttachments = &blendAttachment
            };
            var create = new GraphicsPipelineCreateInfo
            {
                SType = StructureType.GraphicsPipelineCreateInfo,
                StageCount = 2, PStages = stages,
                PVertexInputState = &input, PInputAssemblyState = &assembly,
                PViewportState = &view, PRasterizationState = &raster,
                PMultisampleState = &multisample, PDepthStencilState = &depth,
                PColorBlendState = &blend,
                Layout = layout, RenderPass = pass, Subpass = 0
            };
            Result result = vk.CreateGraphicsPipelines(device, default, 1,
                &create, null, out var pipeline);
            if (result != Result.Success)
                throw new InvalidOperationException($"创建 Graphics Pipeline 失败：{result}");
            return pipeline;
        }
    }
}