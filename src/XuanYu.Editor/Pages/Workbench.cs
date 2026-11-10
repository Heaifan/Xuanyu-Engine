namespace XuanYu.Editor.Pages;

/// <summary>
/// 编辑器主工作台状态。
/// </summary>
public sealed class Workbench
{
    public PageId CurrentId { get; private set; }

    public PageInfo Current => PageCatalog.Get(CurrentId);

    public bool KeepViewportAlive => true;

    public string ViewportNote { get; } = "Vulkan 1.3 视口常驻容器 (待接入 E1)";

    public LogBox Log { get; } = new();

    public event Action? Changed;

    public Workbench(PageId startPage = PageId.World)
    {
        CurrentId = PageCatalog.Get(startPage).Id;
        Log.Write($"工作台已就绪，当前页面：{Current.Title}");
    }

    public void SwitchTo(PageId target)
    {
        PageInfo info = PageCatalog.Get(target);
        if (CurrentId == info.Id)
        {
            return;
        }

        CurrentId = info.Id;
        Log.Write($"切换页面 -> {info.Title}");
        Changed?.Invoke();
    }
}