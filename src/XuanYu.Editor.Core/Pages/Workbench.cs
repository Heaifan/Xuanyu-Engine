using System.Numerics;
using XuanYu.Editor.Preview;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.Render.Vulkan;
using XuanYu.Runtime;
using XuanYu.World;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    public PageId CurrentId { get; private set; }
    public PageInfo Current => PageCatalog.Get(CurrentId);
    public bool KeepViewportAlive => true;
    public PreviewScene Scene { get; }
    public WorldScene World => Scene.World;
    private WorldRenderBridge? _runtimeBridge;

    public IReadOnlyList<RenderDraw> RenderDraws =>
        _runtimeBridge is null ? Scene.Draws
        : _runtimeBridge.Snapshot(Matrix4x4.Identity);

    public IReadOnlyList<RenderDraw> ViewportDraws(uint width, uint height) =>
        ComposeEditorDraws(width, height);

    public SimulationSession Simulation { get; }
    public bool IsSimulating => Simulation.IsRunning;
    public string ViewportNote { get; private set; } =
        "Vulkan 1.3 视口常驻容器（待初始化 GPU）";
    public LogBox Log { get; } = new();
    public event Action? Changed;

    public Workbench(PageId startPage = PageId.World, bool use3D = false)
    {
        CurrentId = PageCatalog.Get(startPage).Id;
        if (use3D)
        {
            _terrainPreview = new TerrainPreviewSession(
                TerrainPreviewMap.CreateBlank(), centered: true);
            Scene = _terrainPreview.Scene;
        }
        else Scene = PreviewSeed.Create();
        Simulation = new SimulationSession(World);
        Scene.Changed += () => Changed?.Invoke();
        Scene.Restored += RebindRestoredLocalTerrain;
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
        Log.Error($"Vulkan 初始化失败：{reason}", "Vulkan");
        Changed?.Invoke();
    }

    public void SwitchTo(PageId target)
    {
        PageInfo info = PageCatalog.Get(target);
        if (CurrentId == info.Id) return;
        ResetCameraKeys();
        CurrentId = info.Id;
        Log.Write($"切换页面 -> {info.Title}");
        Changed?.Invoke();
    }
}