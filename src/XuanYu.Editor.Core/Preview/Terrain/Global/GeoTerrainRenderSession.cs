using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Camera;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private readonly GeoTileSelector _selector;
    private readonly NasademElevationSource _source;
    private readonly EnuFrame _anchor;
    private readonly GeoTileBounds _coverage;
    private readonly GeoTileId _coverRoot;
    private readonly Dictionary<GeoTileId, RenderMesh?> _meshes = [];
    private bool _firstSurfaceLogged;
    private bool _firstSafetyLogged;

    public GlobalCamera Camera { get; private set; }
    public int SelectedCount { get; private set; }
    public int DrawCount { get; private set; }
    public int PendingCount { get; private set; }
    public bool BootstrapActive => PendingCount > 0;
    public event Action<string>? Notice;

    public GeoTerrainRenderSession(NasademTile tile)
    {
        ArgumentNullException.ThrowIfNull(tile);
        _nativeTile = tile;
        var center = new GeoCoordinate(tile.SouthLatitude + 0.5d,
            tile.WestLongitude + 0.5d, 0d);
        _anchor = new EnuFrame(center);
        _coverage = new GeoTileBounds(tile.SouthLatitude, tile.WestLongitude,
            tile.SouthLatitude + 1d, tile.WestLongitude + 1d);
        _source = new NasademElevationSource(tile);
        _selector = new GeoTileSelector(200_000d, 4d,
            maxLevel: 13, maxTiles: 256);
        _coverRoot = GeographicTileScheme.Find(center, 0);
        _meshes.Add(_coverRoot, BuildCoarseMesh(_coverRoot));
        Camera = new GlobalCamera(center, 150_000d);
        Camera.SetTerrainCollision(SampleGroundHeight);
    }

    public IReadOnlyList<RenderDraw> Draws(uint width, uint height)
    {
        if (width < 2 || height < 2) return [];
        var view = new GeoTileView(Camera.PositionEcef, Camera.TargetEcef,
            Math.PI / 3d, (int)width, (int)height);
        IReadOnlyList<GeoTileId> targets = _selector.Select(view, _coverage);
        SelectedCount = targets.Count;
        UpdateVisibleCover(targets);
        StitchVisibleTiles();
        UpdateNativeDetails();
        if (Camera.RestoreTerrainClearance() && !_firstSafetyLogged)
        {
            _firstSafetyLogged = true;
            Notice?.Invoke("G-04-02R：最终可见网格已用于相机离地校正");
        }
        PendingCount = targets.Count(id => !_visibleMeshes.ContainsKey(id));
        Matrix4x4 projection = ViewProjection(width, height);
        RenderDraw[] draws = _visibleMeshes.Values.Select(mesh =>
            new RenderDraw(mesh, projection, true, false)).ToArray();
        DrawCount = draws.Length;
        if (!_firstSurfaceLogged && DrawCount > 0)
        {
            _firstSurfaceLogged = true;
            Notice?.Invoke("G-04-03B-R3：完整覆盖与局部父片兜底已启用");
            Notice?.Invoke($"G-04：首帧绘制 {DrawCount} 片，选片 {SelectedCount}，待细化 {PendingCount} 片");
        }
        return draws;
    }
}