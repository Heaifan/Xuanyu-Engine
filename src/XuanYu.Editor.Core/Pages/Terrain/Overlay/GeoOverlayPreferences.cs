namespace XuanYu.Editor.Pages;

internal sealed record GeoOverlayPreferences
{
    public bool HudVisible { get; init; } = true;
    public bool MiniMapVisible { get; init; } = true;
    public int MiniMapSize { get; init; } = 180;
}