
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace XuanYu.Render.Vulkan;

internal static unsafe class VkSwapchainOptions
{
    public static SwapchainCreateInfoKHR Choose(
        KhrSurface api, PhysicalDevice gpu, SurfaceKHR surface,
        uint width, uint height)
    {
        Result result = api.GetPhysicalDeviceSurfaceCapabilities(
            gpu, surface, out var caps);

        if (result != Result.Success)
            throw new InvalidOperationException(
                $"读取 Surface 能力失败：{result}");

        if ((caps.SupportedUsageFlags & ImageUsageFlags.TransferDstBit) == 0)
            throw new NotSupportedException(
                "该 Surface 不支持 TransferDst 清屏。");

        uint count = 0;
        result = api.GetPhysicalDeviceSurfaceFormats(
            gpu, surface, &count, null);

        if (result != Result.Success || count == 0)
            throw new InvalidOperationException(
                $"Surface 格式不可用：{result}");

        var formats = new SurfaceFormatKHR[checked((int)count)];

        fixed (SurfaceFormatKHR* pointer = formats)
            result = api.GetPhysicalDeviceSurfaceFormats(
                gpu, surface, &count, pointer);

        if (result != Result.Success)
            throw new InvalidOperationException(
                $"读取 Surface 格式失败：{result}");

        SurfaceFormatKHR format = formats[0];

        foreach (var candidate in formats)
        {
            if (candidate.Format == Format.B8G8R8A8Srgb &&
                candidate.ColorSpace == ColorSpaceKHR.SpaceSrgbNonlinearKhr)
            {
                format = candidate;
                break;
            }
        }

        var extent = caps.CurrentExtent;

        if (extent.Width == uint.MaxValue)
        {
            extent.Width = Math.Clamp(
                width, caps.MinImageExtent.Width, caps.MaxImageExtent.Width);

            extent.Height = Math.Clamp(
                height, caps.MinImageExtent.Height, caps.MaxImageExtent.Height);
        }

        uint images = caps.MinImageCount + 1;
        if (caps.MaxImageCount > 0)
            images = Math.Min(images, caps.MaxImageCount);

        var alpha = caps.SupportedCompositeAlpha &
            CompositeAlphaFlagsKHR.OpaqueBitKhr;

        if (alpha == 0)
            alpha = caps.SupportedCompositeAlpha &
                CompositeAlphaFlagsKHR.PreMultipliedBitKhr;

        if (alpha == 0)
            alpha = caps.SupportedCompositeAlpha &
                CompositeAlphaFlagsKHR.PostMultipliedBitKhr;

        if (alpha == 0)
            alpha = CompositeAlphaFlagsKHR.InheritBitKhr;

        return new SwapchainCreateInfoKHR
        {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = surface,
            MinImageCount = images,
            ImageFormat = format.Format,
            ImageColorSpace = format.ColorSpace,
            ImageExtent = extent,
            ImageArrayLayers = 1,
            ImageUsage = ImageUsageFlags.TransferDstBit,
            ImageSharingMode = SharingMode.Exclusive,
            PreTransform = caps.CurrentTransform,
            CompositeAlpha = alpha,
            PresentMode = PresentModeKHR.FifoKhr,
            Clipped = true
        };
    }
}
