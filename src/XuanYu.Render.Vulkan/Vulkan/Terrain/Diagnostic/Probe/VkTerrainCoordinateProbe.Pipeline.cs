using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe partial class VkTerrainCoordinateProbe
{
    private void CreatePipeline()
    {
        DescriptorSetLayoutBinding* bindings = stackalloc DescriptorSetLayoutBinding[2];
        for (uint i = 0; i < 2; i++) bindings[i] = new DescriptorSetLayoutBinding
        { Binding = i, DescriptorType = DescriptorType.StorageBuffer,
            DescriptorCount = 1, StageFlags = ShaderStageFlags.ComputeBit };
        var layoutInfo = new DescriptorSetLayoutCreateInfo
        { SType = StructureType.DescriptorSetLayoutCreateInfo, BindingCount = 2, PBindings = bindings };
        Check(_vk.CreateDescriptorSetLayout(_device, &layoutInfo, null, out _layout), "创建诊断描述符布局");
        var poolSize = new DescriptorPoolSize { Type = DescriptorType.StorageBuffer, DescriptorCount = 2 };
        var poolInfo = new DescriptorPoolCreateInfo { SType = StructureType.DescriptorPoolCreateInfo,
            MaxSets = 1, PoolSizeCount = 1, PPoolSizes = &poolSize };
        Check(_vk.CreateDescriptorPool(_device, &poolInfo, null, out _descriptorPool), "创建诊断描述符池");
        DescriptorSetLayout layout = _layout;
        var alloc = new DescriptorSetAllocateInfo { SType = StructureType.DescriptorSetAllocateInfo,
            DescriptorPool = _descriptorPool, DescriptorSetCount = 1, PSetLayouts = &layout };
        Check(_vk.AllocateDescriptorSets(_device, &alloc, out _set), "分配诊断描述符集");
        DescriptorBufferInfo* buffers = stackalloc DescriptorBufferInfo[2];
        buffers[0] = new DescriptorBufferInfo { Buffer = _input, Range = 96 + 1089 * 4 };
        buffers[1] = new DescriptorBufferInfo { Buffer = _output, Range = 1089 * 16 };
        WriteDescriptorSet* writes = stackalloc WriteDescriptorSet[2];
        for (uint i = 0; i < 2; i++) writes[i] = new WriteDescriptorSet
        { SType = StructureType.WriteDescriptorSet, DstSet = _set, DstBinding = i,
            DescriptorCount = 1, DescriptorType = DescriptorType.StorageBuffer, PBufferInfo = &buffers[i] };
        _vk.UpdateDescriptorSets(_device, 2, writes, 0, null);
        DescriptorSetLayout setLayout = _layout;
        var pl = new PipelineLayoutCreateInfo { SType = StructureType.PipelineLayoutCreateInfo,
            SetLayoutCount = 1, PSetLayouts = &setLayout };
        Check(_vk.CreatePipelineLayout(_device, &pl, null, out _pipelineLayout), "创建诊断管线布局");
        CreateComputePipeline();
    }

    private void CreateComputePipeline()
    {
        using Stream stream = typeof(VkTerrainCoordinateProbe).Assembly
            .GetManifestResourceStream("g07.terrain.probe.comp.spv")
            ?? throw new InvalidOperationException("缺少地形坐标诊断 SPIR-V。");
        byte[] code = new byte[stream.Length]; stream.ReadExactly(code);
        fixed (byte* bytes = code)
        {
            byte[] entry = "main\0"u8.ToArray();
            fixed (byte* name = entry)
            {
            var moduleInfo = new ShaderModuleCreateInfo { SType = StructureType.ShaderModuleCreateInfo,
                CodeSize = (nuint)code.Length, PCode = (uint*)bytes };
            Check(_vk.CreateShaderModule(_device, &moduleInfo, null, out var module), "创建坐标诊断 Shader");
            try
            {
                var stage = new PipelineShaderStageCreateInfo { SType = StructureType.PipelineShaderStageCreateInfo,
                    Stage = ShaderStageFlags.ComputeBit, Module = module, PName = name };
                var info = new ComputePipelineCreateInfo { SType = StructureType.ComputePipelineCreateInfo,
                    Stage = stage, Layout = _pipelineLayout };
                Check(_vk.CreateComputePipelines(_device, default, 1, &info, null, out _pipeline), "创建坐标诊断管线");
            }
            finally { _vk.DestroyShaderModule(_device, module, null); }
            }
        }
    }
}
