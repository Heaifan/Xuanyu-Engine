using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Runtime;
using XuanYu.World.Maps;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportSharedHost
{
    private GeoRenderFrame? _g07EntityFrame;
    private (GeoTerrainRenderSession Source, Vector2 Pixel, uint Width, uint Height)? _g07EntityPick;

    private bool G07EntitiesReady => _g07Cpu is not null && _g07Source is not null &&
        ReferenceEquals(_g07Source, _wb.G07PreviewSource);

    internal void CreateG07CubeAtTarget()
    {
        if (!G07EntitiesReady) { _wb.Log.Warn("G07 数据尚未就绪，请在地形显示后创建。"); return; }
        var sample = _wb.CreateG07InspectionCube(_g07Cpu!, out var cube);
        if (cube is null) _wb.Log.Warn($"[E6/全球实体] 创建失败：{sample.Status}，请将相机目标移到有效 DEM 区域。");
    }

    internal void CreateG07LogicAtTarget()
    {
        if (!G07EntitiesReady) { _wb.Log.Warn("G07 数据尚未就绪，请在地形显示后创建。"); return; }
        var sample = _wb.CreateG07LogicEntity(_g07Cpu!, _g07Source!.Camera.Target, 0d, out var item);
        if (item is null) _wb.Log.Warn($"[E6/全球实体] 创建失败：{sample.Status}，请将相机目标移到有效 DEM 区域。");
        else _wb.Log.Write($"[E6/全球实体] 创建无网格实体：{item.Name}");
    }

    internal void CreateG07UnitAtTarget(string path)
    {
        if (!G07EntitiesReady) { _wb.Log.Warn("G07 数据尚未就绪，请在地形显示后创建。"); return; }
        var sample = _wb.CreateG07Unit(_g07Cpu!, _g07Source!.Camera.Target, path, out var item);
        if (item is null) _wb.Log.Warn($"[C03/二维] 创建失败：{sample.Status}，请将相机目标移到有效 DEM 区域。");
    }

    internal (double Orthometric, double? AboveGround)? ReadG07EntityHeights(Guid id) =>
        G07EntitiesReady ? _g07Source!.G07EntityHeights(_g07Cpu!,
            _wb.World.GetSpatialPosition(id).ToGeo()) : null;

    internal string? MoveG07Entity(Guid id, GeoCoordinate target, bool aboveGround)
    {
        if (!G07EntitiesReady) return "G07 数据尚未就绪。";
        var status = _wb.MoveG07Entity(_g07Cpu!, id, target, aboveGround);
        if (status != GeoSampleStatus.Valid) return $"贴地定位失败：{status}，位置未修改。";
        _wb.Log.Write("[E6/全球实体] 已应用地理位置");
        return null;
    }

    internal void QueueG07EntityPick(int x, int y)
    {
        if (!G07EntitiesReady) return;
        var (width, height) = PixelSize;
        _g07EntityPick = (_g07Source!, new Vector2(x, y), width, height);
    }

    private void DisplayG07EntityPick(GeoTerrainRenderSession source, uint width, uint height)
    {
        if (_g07EntityPick is not { } request) return;
        _g07EntityPick = null;
        if (!G07EntitiesReady || !ReferenceEquals(request.Source, source) ||
            _wb.CurrentId != PageId.World || _wb.IsSimulating ||
            _g07EntityFrame is not { } frame || request.Width != width || request.Height != height) return;
        Guid? id = GeoPreviewMeshPicker.Pick(_wb.Scene, _g07Cpu!, frame, request.Pixel, width, height);
        if (_wb.Scene.SelectedId == id) return;
        _wb.Scene.Select(id);
        _wb.Log.Write(id is null ? "视口点击空白，已取消选择" : $"视口选中：{_wb.Scene.Selected!.Name}");
    }
}