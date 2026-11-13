
using System.Runtime.InteropServices;
using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

internal static unsafe class VkLogicalDeviceCreator
{
    public static (Device Device, Queue Queue) Create(
        Vk vk, PhysicalDevice physicalDevice, uint family)
    {
        bool windows = OperatingSystem.IsWindows();
        bool linux = OperatingSystem.IsLinux();
        bool interop = windows && Supports(vk, physicalDevice,
            "VK_KHR_external_memory_win32", "VK_KHR_win32_keyed_mutex") ||
            linux && Supports(vk, physicalDevice,
                "VK_KHR_external_memory_fd", "VK_KHR_external_semaphore_fd");

        byte[] swapName = "VK_KHR_swapchain\0"u8.ToArray();
        byte[] winMemName = "VK_KHR_external_memory_win32\0"u8.ToArray();
        byte[] winMutexName = "VK_KHR_win32_keyed_mutex\0"u8.ToArray();
        byte[] linuxMemName = "VK_KHR_external_memory_fd\0"u8.ToArray();
        byte[] linuxSemName = "VK_KHR_external_semaphore_fd\0"u8.ToArray();
        float priority = 1.0f;

        fixed (byte* swap = swapName)
        fixed (byte* winMem = winMemName)
        fixed (byte* winMutex = winMutexName)
        fixed (byte* linuxMem = linuxMemName)
        fixed (byte* linuxSem = linuxSemName)
        {
            byte** extensions = stackalloc byte*[3];
            uint extensionCount = 0;
            if (windows || linux)
                extensions[extensionCount++] = swap;
            if (interop)
            {
                extensions[extensionCount++] = windows ? winMem : linuxMem;
                extensions[extensionCount++] = windows ? winMutex : linuxSem;
            }

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
                EnabledExtensionCount = extensionCount,
                PpEnabledExtensionNames = extensionCount == 0 ? null : extensions
            };
            Device device = default;
            Result result = vk.CreateDevice(physicalDevice, &createInfo, null, &device);
            if (result != Result.Success)
                throw new InvalidOperationException($"Vulkan 逻辑设备创建失败：{result}");
            Queue queue = default;
            vk.GetDeviceQueue(device, family, 0, &queue);
            Console.WriteLine("[XYE/E306] 外部 GPU 共享扩展：" +
                (interop ? "已启用" : "不可用，保留原生视口"));
            return (device, queue);
        }
    }

    private static bool Supports(Vk vk, PhysicalDevice gpu, string first, string second)
    {
        uint count = 0;
        Result result = vk.EnumerateDeviceExtensionProperties(
            gpu, (byte*)null, &count, null);
        if (result != Result.Success)
            throw new InvalidOperationException($"枚举设备扩展失败：{result}");

        var properties = new ExtensionProperties[checked((int)count)];
        fixed (ExtensionProperties* pointer = properties)
        {
            result = vk.EnumerateDeviceExtensionProperties(
                gpu, (byte*)null, &count, pointer);
            if (result != Result.Success)
                throw new InvalidOperationException($"读取设备扩展失败：{result}");

            bool foundFirst = false, foundSecond = false;
            for (uint i = 0; i < count; i++)
            {
                string? name = Marshal.PtrToStringUTF8((nint)pointer[i].ExtensionName);
                if (name == first) foundFirst = true;
                if (name == second) foundSecond = true;
            }
            return foundFirst && foundSecond;
        }
    }
}
