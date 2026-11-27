using System.Numerics;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Editor.TerrainEditing;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    private TerrainPreviewSession? _sculptSession;
    private TerrainSculptStroke? _sculptStroke;
    private int _sculptChanges;

    public bool TryBeginTerrainSculpt(Vector2 pixel, uint width, uint height, bool lower)
    {
        EndTerrainSculpt();
        if (!CanPreviewTerrainBrush) return false;
        var session = _terrainPreview!;
        if (!session.TryPick(pixel, width, height, out var hit)) return false;
        _sculptSession = session;
        bool peak = IsTerrainMountainTool;
        _sculptStroke = new TerrainSculptStroke(session.Map.HeightField, TerrainBrush, lower,
            IsTerrainElevationTool ? TerrainTargetElevation : null, TerrainElevationArea,
            smooth: IsTerrainSmoothTool,
            flattenHeight: IsTerrainFlattenTool ? hit.WorldPosition.Z : null,
            naturalPeakHeight: peak ? TerrainMountainHeight : null, naturalPeakSeed: TerrainNaturalPeakSeed,
            thermal: IsTerrainThermalTool, rain: IsTerrainRainTool);
        Log.Write($"[E5-05] 开始笔划：{(IsTerrainElevationTool ? "定高" :
            IsTerrainRainTool ? "水力侵蚀" : IsTerrainThermalTool ? "热力崩塌" : IsTerrainSmoothTool ? "平滑" : IsTerrainFlattenTool ? "整平" :
             peak ? "自然造山" : lower ? "降低" : "抬升")}");
        if (IsTerrainFlattenTool) Log.Write($"[E5-06B] 锁定整平高程：{hit.WorldPosition.Z:0.###} m");
        if (peak) Log.Write($"[E5-M-R01B] 自然造山：隆起={TerrainMountainHeight:0.###}m，" +
            $"半径={TerrainBrush.RadiusMeters:0.###}m，种子={TerrainNaturalPeakSeed}");
        if (IsTerrainThermalTool) Log.Write($"[E5-M03A] 独立热力崩塌：休止角=35°，半径={TerrainBrush.RadiusMeters:0.###}m，强度={TerrainBrush.Strength:P0}");
        if (IsTerrainRainTool) Log.Write($"[E5-M03B-02] 独立水力侵蚀：雨滴粒子，半径={TerrainBrush.RadiusMeters:0.###}m，强度={TerrainBrush.Strength:P0}，衰减={TerrainBrush.Falloff:P0}");
        ApplySculptChanges(_sculptStroke.MoveTo(new(hit.WorldPosition.X, hit.WorldPosition.Y)));
        return true;
    }

    public void MoveTerrainSculpt(Vector2 pixel, uint width, uint height)
    {
        if (!CheckSculptSession()) return;
        if (!_sculptSession!.TryPick(pixel, width, height, out var hit, _sculptStroke!.ReferenceSurface))
        {
            _sculptStroke.BreakPath();
            return;
        }
        ApplySculptChanges(_sculptStroke.MoveTo(new(hit.WorldPosition.X, hit.WorldPosition.Y)));
    }

    public void AdvanceTerrainSculpt(float seconds)
    {
        if (!CheckSculptSession()) return;
        ApplySculptChanges(_sculptStroke!.Advance(seconds));
    }

    public void EndTerrainSculpt()
    {
        if (_sculptStroke is not { } stroke) return;
        var session = _sculptSession!;
        if (IsTerrainMountainTool && ReferenceEquals(session, _terrainPreview))
            ApplySculptChanges(stroke.FinishMountain());
        if (IsTerrainRainTool && ReferenceEquals(session, _terrainPreview)) ApplySculptChanges(stroke.FinishRain());
        _sculptStroke = null;
        _sculptSession = null;
        if (_sculptChanges != 0 && ReferenceEquals(session, _terrainPreview))
            session.RecordTerrainEdit(stroke.ReferenceSurface);
        Log.Write($"[E5-05] 结束笔划：样本修改累计 {_sculptChanges} 次");
        _sculptChanges = 0;
        Changed?.Invoke();
    }

    private bool CheckSculptSession()
    {
        if (_sculptStroke is null) return false;
        if (CanPreviewTerrainBrush && ReferenceEquals(_terrainPreview, _sculptSession)) return true;
        EndTerrainSculpt();
        return false;
    }

    private void ApplySculptChanges(int changed)
    {
        if (changed == 0) return;
        _sculptChanges += changed;
        _sculptSession!.RefreshSculptMesh();
    }
}