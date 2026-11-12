using System.Numerics;
using XuanYu.Editor.Preview;
using XuanYu.Render;
using XuanYu.Render.Vulkan;
using XuanYu.Runtime;
using XuanYu.World;

namespace XuanYu.Editor.Pages;

public sealed class Workbench
{
    public PageId CurrentId { get; private set; }
    public PageInfo Current => PageCatalog.Get(CurrentId);
    public bool KeepViewportAlive => true;
    public PreviewScene Scene { get; }
    public WorldScene World => Scene.World;
    private WorldRenderBridge? _runtimeBridge;
    public IReadOnlyList<RenderDraw> RenderDraws => _runtimeBridge is null
        ? Scene.Draws : _runtimeBridge.Snapshot(Matrix4x4.Identity);
    public IReadOnlyList<RenderDraw> ViewportDraws(uint width, uint height) =>
        _runtimeBridge is null ? Scene.ViewportDraws(width, height)
        : _runtimeBridge.Snapshot(Scene.ViewProjection(width, height));
    public SimulationSession Simulation { get; }
    public bool IsSimulating => Simulation.IsRunning;

    public string ViewportNote { get; private set; } =
        "Vulkan 1.3 视口常驻容器（待初始化 GPU）";

    public LogBox Log { get; } = new();
    public event Action? Changed;

    public Workbench(PageId startPage = PageId.World, bool use3D = false)
    {
        CurrentId = PageCatalog.Get(startPage).Id;
        Scene = use3D ? PreviewSeed.Create3D() : PreviewSeed.Create();
        Simulation = new SimulationSession(World);
        Scene.Changed += () => Changed?.Invoke();
        Log.Write($"工作台已就绪，当前页面：{Current.Title}");
    }

    public void StartSimulation()
    {
        Simulation.Start();
        _runtimeBridge = CreateRuntimeBridge();
        Log.Write("模拟开始：已建立独立 World 副本，目标 30 Hz");
        Changed?.Invoke();
    }

    public void AdvanceSimulationTick()
    {
        Simulation.Step();
        if (Simulation.TickCount % SimulationSession.TicksPerSecond == 0)
            Changed?.Invoke();
    }

    public void StopSimulation()
    {
        double seconds = Simulation.ElapsedSeconds;
        Simulation.Stop();
        _runtimeBridge = null;
        Log.Write($"模拟停止：累计 {seconds:0.0} 秒，编辑场景未被替换");
        Changed?.Invoke();
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

    private WorldRenderBridge CreateRuntimeBridge()
    {
        var bridge = new WorldRenderBridge(Simulation.RunningWorld!);
        foreach (PreviewObject item in Scene.Objects)
            bridge.Bind(item.Id, item.Mesh);
        return bridge;
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