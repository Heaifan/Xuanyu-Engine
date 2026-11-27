using XuanYu.Editor.Core.Preview.Terrain.Unified;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render.Vulkan;
using System.Numerics;
using Avalonia.Controls;
using XuanYu.Editor.Pages;
using System.Diagnostics;
using XuanYu.World.Maps;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportSharedHost
{
    private const int G07Capacity = 512, G07UploadLimit = 16;
    private readonly bool _g07Enabled =
        Environment.GetEnvironmentVariable("XYE_G07_VIEWPORT") == "1";
    private GeoTerrainRenderSession? _g07Source;
    private GeoUnifiedLodSession? _g07Cpu;
    private GeoUnifiedGpuSession? _g07Gpu;
    private VkTerrainPagePool? _g07Pool;
    private VkSharedColorProbe? _g07Probe;
    private int[][]? _g07Topology;
    private long _g07Frames;
    private Matrix4x4 _g07HudProjection;

    private async Task<string> DrawG07FrameAsync(VkSharedColorProbe probe,
        GeoTerrainRenderSession source, uint width, uint height)
    {
        if (!ReferenceEquals(source, _g07Source))
        {
            if (!_vk.ShaderFloat64Enabled)
                throw new NotSupportedException("G07 需要 Vulkan shaderFloat64。");
            ReleaseG07Session();
            _wb.Log.Write($"[G07/视口] 准备完整数据集：HGT={source.SourceCount}");
            string? path = _wb.TerrainSourcePath;
            _g07Cpu = await Task.Run(() => source.LoadG07DatasetAsync(path, G07Capacity));
            _g07Source = source;
            _wb.Log.Write($"[G07/视口] 根覆盖={_g07Cpu.Coverage.Leaves.Count} {_g07Cpu.PreparationProfile} 粗层误差=后台抽样估计");
        }
        if (source.EnsureG07CameraClearance(_g07Cpu!))
            _wb.Log.Write($"[G07/防穿透] 已提交地表离地校正，距离={source.Camera.DistanceMeters:F2}m");
        var eye = source.G07Eye;
        var mvp = source.G07Projection(width, height);
        var origin = source.G07Origin;
        bool geographic = _wb.CurrentId == PageId.Terrain;
        bool grid = geographic ? _wb.TerrainGridEnabled : _wb.WorldGridEnabled;
        var contour = source.ContourStyle;
        var style = new TerrainVisualStyle(GeoTerrainRenderSession.G07ElevationPalette, _wb.TerrainHillshadeEnabled)
        {
            Grid = grid ? source.G07GridStyle(geographic, height) : null,
            Contours = _wb.TerrainContoursEnabled ? new Vector4(contour.IntervalMeters, contour.Opacity,
                contour.NextIntervalMeters, contour.IntervalBlend) : Vector4.Zero,
            Selection = source.G07Selection
        };
        int interval = _wb.TerrainContoursEnabled && _wb.TerrainContourLabelsEnabled &&
            source.Camera.DistanceMeters <= 20_000d ? (contour.IntervalBlend >= 0.5f
                ? contour.NextIntervalMeters : contour.IntervalMeters) : 0;
        float scale = (float)(TopLevel.GetTopLevel(this)?.RenderScaling ?? 1d);
        long environmentStart = Stopwatch.GetTimestamp();
        var environment = _wb.G07EnvironmentDraws(source, _g07Cpu!, width, height);
        double environmentMs = Stopwatch.GetElapsedTime(environmentStart).TotalMilliseconds;
        var result = await Task.Run(() =>
        {
            if (!ReferenceEquals(probe, _g07Probe))
            {
                probe.ResetTerrainPool();
                var topology = _g07Topology ??=
                    Enumerable.Range(0, 16).Select(GeoUnifiedGridIndices.Create).ToArray();
                var pool = probe.GetOrCreateTerrainPool(width, height, G07Capacity,
                    G07UploadLimit, 16 * 1024 * 1024, topology, source.G07WaterAtlas);
                _g07Gpu = new GeoUnifiedGpuSession(_g07Cpu!, pool, origin);
                _g07Pool = pool;
                _g07Probe = probe;
            }
            return DrawG07Coverage(probe, _g07Gpu!, _g07Pool!, eye, mvp, width, height, environment, style, source, _g07Cpu!, interval, scale);
        });
        _g07HudProjection = mvp;
        _screenLabels = result.Labels;
        _worldLabels = [];
        _lastContourLabelsAt = 0;
        if (++_g07Frames == 1 || result.Submitted > 0 || result.LabelMs >= 5d || _g07Frames % 60 == 0 || Environment.GetEnvironmentVariable("XYE_G06_PERF") == "1")
            _wb.Log.Write($"[G07/视口] 帧={_g07Frames} 路径=GPU HGT={source.SourceCount} " +
                $"LOD={result.Levels} 绘制={result.Drawn} 驻留={result.Resident} 待发布={result.Pending} " +
                $"上传={result.TotalPages} 高程显存={result.Bytes}B 本帧={result.Ms:F1}ms " +
                $"切换等待={result.Transition} 容量阻塞={result.Blocked} CPU像素读回=0 " +
                $"指标={_g07Cpu!.Diagnostic ?? "正常"} 环境={environmentMs:F1}ms {result.Profile}");
        return result.Profile;
    }

    private void ReleaseG07Session()
    {
        _g07Cpu?.Dispose();
        _g07Cpu = null; _g07Gpu = null; _g07Pool = null;
        _g07Source = null; _g07Probe = null;
        _g07Frames = 0;
        _g07LabelCache.Clear();
    }
}