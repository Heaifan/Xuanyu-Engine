using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe partial class VkDemTexture
{
    private void CreateViewAndSampler()
    {
        var view = new ImageViewCreateInfo
        {
            SType = StructureType.ImageViewCreateInfo,
            Image = _image,
            ViewType = ImageViewType.Type2D,
            Format = Format.R16Sint,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = ImageAspectFlags.ColorBit,
                LevelCount = 1,
                LayerCount = 1
            }
        };
        Check(_vk.CreateImageView(_device, &view, null, out _view),
            "创建 DEM 纹理视图");
        // 整数高程不支持硬件线性过滤；后续 Shader 负责 NoData-aware 插值。
        var sampler = new SamplerCreateInfo
        {
            SType = StructureType.SamplerCreateInfo,
            MagFilter = Filter.Nearest,
            MinFilter = Filter.Nearest,
            MipmapMode = SamplerMipmapMode.Nearest,
            AddressModeU = SamplerAddressMode.ClampToEdge,
            AddressModeV = SamplerAddressMode.ClampToEdge,
            AddressModeW = SamplerAddressMode.ClampToEdge,
            MinLod = 0f,
            MaxLod = 0f
        };
        Check(_vk.CreateSampler(_device, &sampler, null, out _sampler),
            "创建 DEM 最近邻采样器");
    }
}