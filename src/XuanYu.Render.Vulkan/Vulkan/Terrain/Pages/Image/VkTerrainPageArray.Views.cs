using Silk.NET.Vulkan;
using static XuanYu.Render.Vulkan.VkTerrainBuffer;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe partial class VkTerrainPageArray
{
    private void CreateViews()
    {
        for (int i = 0; i < _views.Length; i++)
        {
            var view = new ImageViewCreateInfo { SType = StructureType.ImageViewCreateInfo,
                Image = _image, ViewType = ImageViewType.Type2D, Format = Format.R16Sint,
                SubresourceRange = new ImageSubresourceRange { AspectMask = ImageAspectFlags.ColorBit,
                    BaseArrayLayer = (uint)i, LevelCount = 1, LayerCount = 1 } };
            Check(_vk.CreateImageView(_device, &view, null, out _views[i]), "创建 G07 高程层视图");
        }
        var sampler = new SamplerCreateInfo { SType = StructureType.SamplerCreateInfo,
            MagFilter = Filter.Nearest, MinFilter = Filter.Nearest, MipmapMode = SamplerMipmapMode.Nearest,
            AddressModeU = SamplerAddressMode.ClampToEdge, AddressModeV = SamplerAddressMode.ClampToEdge,
            AddressModeW = SamplerAddressMode.ClampToEdge, MaxLod = 0f };
        Check(_vk.CreateSampler(_device, &sampler, null, out _sampler), "创建 G07 页池采样器");
    }
}