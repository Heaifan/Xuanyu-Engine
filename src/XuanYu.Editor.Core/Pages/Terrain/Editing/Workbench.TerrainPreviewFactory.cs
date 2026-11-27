using XuanYu.Editor.Preview;
using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    private TerrainPreviewSession CreateTerrainPreview()
    {
        var preview = new TerrainPreviewSession(
            TerrainPreviewMap.CreateBlank(), centered: true);
        BindTerrainScene(preview.Scene);
        Log.Write($"[E5-04A] 本地地形 {preview.Map.WidthMeters:0} × {preview.Map.LengthMeters:0} m");
        return preview;
    }

    private void BindTerrainScene(PreviewScene scene) =>
        scene.Changed += () => Changed?.Invoke();
}