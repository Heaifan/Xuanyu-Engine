namespace XuanYu.Render.Vulkan;

public sealed record GpuInfo(
    string DeviceName,
    uint GraphicsFamilyIndex,
    string ApiVersionText,
    bool IsDiscrete)
{
    public string Summary =>
        $"{DeviceName} | Vulkan {ApiVersionText} | " +
        $"Graphics Queue #{GraphicsFamilyIndex} | " +
        (IsDiscrete ? "独立显卡" : "其他显卡");
}