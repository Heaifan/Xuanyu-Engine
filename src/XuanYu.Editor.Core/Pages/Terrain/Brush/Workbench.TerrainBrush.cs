using System.Numerics;
using XuanYu.Editor.TerrainEditing;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    public TerrainBrushSettings TerrainBrush { get; private set; } =
        new(10f, 0.5f, 0.5f);

    public bool CanPreviewTerrainBrush => CurrentId == PageId.Terrain &&
        IsTerrainEditMode && CanEditLocalTerrain &&
        _terrainPreview is { } session &&
        session.Scene.Objects.Any(item => item.Id == session.TerrainObjectId && item.Visible);

    public void SetTerrainBrush(TerrainBrushSettings settings)
    {
        TerrainBrush = settings;
        Log.Write($"[E5-04C] 笔刷：半径 {settings.RadiusMeters:0.###}m，" +
            $"强度 {settings.Strength:P0}，衰减 {settings.Falloff:P0}");
        // 面板主动应用不重建输入框；快捷键确认后才同步 Inspector。
    }

    public void PreviewTerrainBrush(TerrainBrushSettings settings) => TerrainBrush = settings;

    public void CommitTerrainBrushPreview()
    {
        SetTerrainBrush(TerrainBrush);
        Changed?.Invoke();
    }

    public TerrainBrushPreview? BuildTerrainBrushPreview(Vector2 pixel,
        uint width, uint height)
    {
        if (!CanPreviewTerrainBrush) return null;
        var session = _terrainPreview!;
        var reference = ReferenceEquals(session, _sculptSession)
            ? _sculptStroke?.ReferenceSurface : null;
        if (!session.TryPick(pixel, width, height, out var hit, reference)) return null;

        var field = session.Map.HeightField;
        var settings = IsTerrainElevationTool
            ? new TerrainBrushSettings(TerrainBrush.RadiusMeters, 1f,
                TerrainElevationArea ? TerrainBrush.Falloff : 1f)
            : TerrainBrush;
        var center = new Vector2(hit.WorldPosition.X, hit.WorldPosition.Y);
        float spacing = MathF.Min(field.SpacingXMeters, field.SpacingYMeters);
        float? Sample(Vector2 point) => TerrainBrushSurface.Sample(field, point);
        var outer = TerrainBrushOutline.Build(center, settings.RadiusMeters, spacing, Sample);
        bool showInner = settings.Falloff > 0f && !IsTerrainMountainTool;
        var inner = showInner
            ? TerrainBrushOutline.Build(center, settings.HalfWeightRadius, spacing, Sample)
            : Array.Empty<TerrainBrushOutline.Segment>();

        Matrix4x4 mapToScene = session.IsCentered
            ? Matrix4x4.CreateTranslation(-new Vector3(session.OriginMeters, 0f)) *
                Matrix4x4.CreateScale(session.DisplayScale)
            : Matrix4x4.Identity;
        float z = Sample(center) ?? hit.WorldPosition.Z;
        return new TerrainBrushPreview(new Vector3(center, z), outer, inner, mapToScene,
            IsTerrainMountainTool ? 1f : settings.Strength)
        {
            CursorMeters = hit.WorldPosition, RadiusMeters = settings.RadiusMeters,
            InnerRadiusMeters = showInner ? settings.HalfWeightRadius : 0f
        };
    }
}