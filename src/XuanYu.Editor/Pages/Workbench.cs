using XuanYu.Render.Vulkan;

namespace XuanYu.Editor.Pages;

public sealed class Workbench
{
    public PageId CurrentId { get; private set; }

    public PageInfo Current => PageCatalog.Get(CurrentId);

    public bool KeepViewportAlive => true;

    public string ViewportNote { get; private set; } =
        "Vulkan 1.3 视口常驻容器（待初始化 GPU）";

    public LogBox Log { get; } = new();

    public event Action? Changed;

    public Workbench(PageId startPage = PageId.World)
    {
        CurrentId = PageCatalog.Get(startPage).Id;
        Log.Write($"工作台已就绪，当前页面：{Current.Title}");
    }

    public void BindGpu(GpuInfo gpu)
    {
        ViewportNote = $"GPU 已连接：{gpu.Summary}";
        Log.Write($"Vulkan 初始化成功：{gpu.Summary}");
        Changed?.Invoke();
    }

    public void ReportGpuFailure(string reason)
    {
        ViewportNote = $"GPU 初始化失败：{reason}";
        Log.Write($"Vulkan 初始化失败：{reason}");
        Changed?.Invoke();
    }

    public void SwitchTo(PageId target)
    {
        PageInfo info = PageCatalog.Get(target);

        if (CurrentId == info.Id)
            return;

        CurrentId = info.Id;
        Log.Write($"切换页面 -> {info.Title}");
        Changed?.Invoke();
    }
}