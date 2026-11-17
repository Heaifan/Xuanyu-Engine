using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Camera;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private readonly GeoTerrainViewSession _stream;
    private readonly NasademElevationSource _source;
    private readonly EnuFrame _anchor;
    private readonly GeoTileBounds _coverage;
    private readonly Dictionary<GeoTileId, RenderMesh?> _meshes = [];
    private bool _firstSurfaceLogged;
    private bool _firstSafetyLogged;

    public GlobalCamera Camera { get; private set; }
    public int SelectedCount { get; private set; }
    public int DrawCount { get; private set; }
    public int PendingCount { get; private set; }
    public event Action<string>? Notice;

    public GeoTerrainRenderSession(NasademTile tile)
    {
        ArgumentNullException.ThrowIfNull(tile);
        var center = new GeoCoordinate(tile.SouthLatitude + 0.5d,
            tile.WestLongitude + 0.5d, 0d);
        _anchor = new EnuFrame(center);
        _coverage = new GeoTileBounds(tile.SouthLatitude, tile.WestLongitude,
            tile.SouthLatitude + 1d, tile.WestLongitude + 1d);
        _source = new NasademElevationSource(tile);
        _stream = new GeoTerrainViewSession(
            new GeoTileSelector(200_000d, 4d, maxLevel: 13, maxTiles: 64),
            new GeoTerrainTileSession(_source, cellsPerEdge: 16, cacheCapacity: 128));
        Camera = new GlobalCamera(center, 150_000d);
        Camera.SetTerrainCollision(SampleGroundHeight);
    }

    public IReadOnlyList<RenderDraw> Draws(uint width, uint height)
    {
        if (width < 2 || height < 2) return [];
        var view = new GeoTileView(Camera.PositionEcef, Camera.TargetEcef,
            Math.PI / 3d, (int)width, (int)height);
        GeoTerrainFrame frame = _stream.Update(view, 4, _coverage);
        SelectedCount = frame.Tiles.Count;
        UpdateVisibleCover(frame);
        StitchVisibleTiles();
        if (Camera.RestoreTerrainClearance() && !_firstSafetyLogged)
        {
            _firstSafetyLogged = true;
            Notice?.Invoke("G-03E：LOD 切换后校正相机离地高度");
        }
        PendingCount = _stream.PendingCount;

        Matrix4x4 projection = ViewProjection(width, height);
        RenderDraw[] draws = _visibleMeshes.Values.Select(mesh =>
            new RenderDraw(mesh, projection, true, false)).ToArray();
        DrawCount = draws.Length;
        if (!_firstSurfaceLogged && DrawCount > 0)
        {
            _firstSurfaceLogged = true;
            Notice?.Invoke("G-03C 地形防穿透已启用；G-03D LOD 滞回已启用");
            Notice?.Invoke($"G-02F 瓦片绘制首帧：{DrawCount} 片，选片 {SelectedCount}，待建 {PendingCount}");
        }
        return draws;
    }
}