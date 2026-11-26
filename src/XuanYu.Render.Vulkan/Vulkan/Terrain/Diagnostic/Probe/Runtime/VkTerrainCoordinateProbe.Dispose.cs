using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe partial class VkTerrainCoordinateProbe
{
    public void Dispose()
    {
        if (_submitted) { Fence fence = _fence; _vk.WaitForFences(_device, 1, &fence, true, ulong.MaxValue); }
        if (_fence.Handle != 0) _vk.DestroyFence(_device, _fence, null);
        if (_commandPool.Handle != 0) _vk.DestroyCommandPool(_device, _commandPool, null);
        if (_pipeline.Handle != 0) _vk.DestroyPipeline(_device, _pipeline, null);
        if (_pipelineLayout.Handle != 0) _vk.DestroyPipelineLayout(_device, _pipelineLayout, null);
        if (_descriptorPool.Handle != 0) _vk.DestroyDescriptorPool(_device, _descriptorPool, null);
        if (_layout.Handle != 0) _vk.DestroyDescriptorSetLayout(_device, _layout, null);
        DestroyBuffer(ref _input, ref _inputMemory);
        DestroyBuffer(ref _output, ref _outputMemory);
    }

    private void DestroyBuffer(ref Silk.NET.Vulkan.Buffer buffer, ref DeviceMemory memory)
    {
        if (buffer.Handle != 0) _vk.DestroyBuffer(_device, buffer, null);
        if (memory.Handle != 0) _vk.FreeMemory(_device, memory, null);
        buffer = default; memory = default;
    }
}
