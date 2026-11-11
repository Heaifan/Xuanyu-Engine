
using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

internal static unsafe class VkLogicalDeviceCreator
{
    public static (Device Device, Queue Queue) Create(
        Vk vk, PhysicalDevice physicalDevice, uint family)
    {
        float priority = 1.0f;
        byte[] swapchainName = "VK_KHR_swapchain\0"u8.ToArray();

        fixed (byte* swapchain = swapchainName)
        {
            byte** extensions = stackalloc byte*[1] { swapchain };
            var queueInfo = new DeviceQueueCreateInfo
            {
                SType = StructureType.DeviceQueueCreateInfo,
                QueueFamilyIndex = family,
                QueueCount = 1,
                PQueuePriorities = &priority
            };

            var createInfo = new DeviceCreateInfo
            {
                SType = StructureType.DeviceCreateInfo,
                QueueCreateInfoCount = 1,
                PQueueCreateInfos = &queueInfo,
                EnabledExtensionCount = OperatingSystem.IsWindows() ? 1u : 0u,
                PpEnabledExtensionNames = OperatingSystem.IsWindows() ? extensions : null
            };

            Device device = default;
            Result result = vk.CreateDevice(
                physicalDevice, &createInfo, null, &device);

            if (result != Result.Success)
                throw new InvalidOperationException(
                    $"Vulkan 逻辑设备创建失败：{result}");

            Queue queue = default;
            vk.GetDeviceQueue(device, family, 0, &queue);
            return (device, queue);
        }
    }
}
