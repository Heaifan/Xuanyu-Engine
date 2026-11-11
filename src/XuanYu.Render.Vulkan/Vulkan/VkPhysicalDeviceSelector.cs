
using System.Runtime.InteropServices;
using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

internal static unsafe class VkPhysicalDeviceSelector
{
    public static (PhysicalDevice Device, GpuInfo Info) Pick(
        Vk vk, Instance instance)
    {
        uint count = 0;
        Result result = vk.EnumeratePhysicalDevices(instance, &count, null);

        if (result != Result.Success || count == 0)
        {
            throw new InvalidOperationException($"无法枚举 Vulkan GPU：{result}");
        }

        var devices = new PhysicalDevice[checked((int)count)];
        fixed (PhysicalDevice* pointer = devices)
        {
            result = vk.EnumeratePhysicalDevices(instance, &count, pointer);
        }

        if (result != Result.Success)
        {
            throw new InvalidOperationException($"获取 Vulkan GPU 列表失败：{result}");
        }

        PhysicalDevice selected = default;
        GpuInfo? selectedInfo = null;
        foreach (PhysicalDevice device in devices)
        {
            int family = FindGraphicsFamily(vk, device);
            if (family < 0)
                continue;

            PhysicalDeviceProperties properties = default;
            vk.GetPhysicalDeviceProperties(device, &properties);
            if (properties.ApiVersion < Vk.Version13)
                continue;

            bool discrete = properties.DeviceType == PhysicalDeviceType.DiscreteGpu;
            string name = Marshal.PtrToStringUTF8((nint)properties.DeviceName) ?? "Unknown GPU";
            uint version = properties.ApiVersion;
            string api = $"{version >> 22}." +
                $"{(version >> 12) & 0x3FF}.{version & 0xFFF}";
            var info = new GpuInfo(name, (uint)family, api, discrete);

            if (selectedInfo is null || (discrete && !selectedInfo.IsDiscrete))
            {
                selected = device;
                selectedInfo = info;
            }
        }

        if (selectedInfo is null)
        {
            throw new InvalidOperationException(
                "未找到支持 Vulkan 1.3 和 Graphics Queue 的 GPU。");
        }
        return (selected, selectedInfo);
    }

    private static int FindGraphicsFamily(
        Vk vk, PhysicalDevice device)
    {
        uint count = 0;
        vk.GetPhysicalDeviceQueueFamilyProperties(device, &count, null);
        if (count == 0)
            return -1;

        var families = new QueueFamilyProperties[checked((int)count)];
        fixed (QueueFamilyProperties* pointer = families)
        {
            vk.GetPhysicalDeviceQueueFamilyProperties(
                device, &count, pointer);
        }

        for (int i = 0; i < families.Length; i++)
        {
            if (families[i].QueueCount > 0 &&
                (families[i].QueueFlags & QueueFlags.GraphicsBit) != 0)
                return i;
        }
        return -1;
    }
}
