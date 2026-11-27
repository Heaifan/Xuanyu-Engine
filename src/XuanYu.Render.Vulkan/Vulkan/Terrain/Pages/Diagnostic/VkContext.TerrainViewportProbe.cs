using Silk.NET.Vulkan;
using XuanYu.Render;

namespace XuanYu.Render.Vulkan;

public sealed unsafe partial class VkContext
{
    public byte[] ProbeTerrainViewport(TerrainPoolUpload upload, TerrainPoolDraw draw,
        IReadOnlyList<int[]> topology, IReadOnlyList<RenderDraw> meshes)
    {
        if (!IsInitialized) throw new ObjectDisposedException(nameof(VkContext));
        if (!ShaderFloat64Enabled) throw new NotSupportedException("设备不支持 shaderFloat64。");
        ValidatePoolProbe(1, 1, topology, [new([upload], [upload.Key], [], [], [draw])]);
        var extent = new Extent2D(128, 128);
        using var image = new VkTerrainOffscreenImage(_vk, PhysicalDevice, _device, extent);
        using var scene = new VkMeshScene(_vk, PhysicalDevice, _device,
            VkTerrainOffscreenImage.ColorFormat, extent, [image.Handle], true);
        using var terrain = new VkTerrainViewportDraw(_vk, PhysicalDevice, _device,
            GraphicsQueue, Gpu.GraphicsFamilyIndex, scene, extent, 1, 1, 16 * 1024 * 1024, topology);
        using var readback = new VkTerrainBuffer(_vk, PhysicalDevice, _device,
            128 * 128 * 4, BufferUsageFlags.TransferDstBit);
        using var probe = new VkTerrainDrawProbe(_vk, _device, GraphicsQueue, Gpu.GraphicsFamilyIndex);

        terrain.Pool.Submit([upload]);
        terrain.Pool.WaitForUpload();
        terrain.Pool.Protect([draw.Key]);
        scene.SyncMeshes(meshes);
        scene.SyncDem(meshes, GraphicsQueue, Gpu.GraphicsFamilyIndex);
        scene.SyncWater(meshes, GraphicsQueue, Gpu.GraphicsFamilyIndex);
        return probe.DrawViewport(scene, extent, image.Handle, readback, meshes,
            terrain.Prepare([draw]));
    }
}