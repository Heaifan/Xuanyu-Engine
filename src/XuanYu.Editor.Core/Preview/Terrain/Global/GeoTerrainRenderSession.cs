using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Camera;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private readonly GeoTerrainViewSession _stream;
    private readonly EnuFrame _anchor;
    private readonly GeoTileBounds _coverage;
    private readonly Dictionary<GeoTileId, RenderMesh?> _meshes = [];
    private bool _firstSurfaceLogged;

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
        _stream = new GeoTerrainViewSession(
            new GeoTileSelector(200_000d, 4d, maxLevel: 13, maxTiles: 64),
            new GeoTerrainTileSession(new NasademElevationSource(tile),
                cellsPerEdge: 16, cacheCapacity: 128));
        Camera = new GlobalCamera(center, 150_000d);
    }

    public IReadOnlyList<RenderDraw> Draws(uint width, uint height)
    {
        if (width < 2 || height < 2) return [];
        var view = new GeoTileView(Camera.PositionEcef, Camera.TargetEcef,
            Math.PI / 3d, (int)width, (int)height);
        GeoTerrainFrame frame = _stream.Update(view, 4, _coverage);
        SelectedCount = frame.Tiles.Count;
        PendingCount = frame.PendingCount;
        Matrix4x4 projection = ViewProjection(width, height);
        var result = new List<RenderDraw>();
        var drawn = new HashSet<GeoTileId>();
        foreach (GeoTerrainFrame.Tile tile in frame.Tiles)
        {
            if (tile.AvailableTile is not { ValidSampleCount: > 0 } heights ||
                !drawn.Add(heights.Id)) continue;
            if (!_meshes.TryGetValue(heights.Id, out RenderMesh? mesh))
            {
                mesh = GeoTerrainMeshBuilder.Build(heights, _anchor);
                _meshes.Add(heights.Id, mesh);
            }
            if (mesh is not null)
                result.Add(new RenderDraw(mesh, projection, true, false));
        }
        DrawCount = result.Count;
        if (!_firstSurfaceLogged && DrawCount > 0)
        {
            _firstSurfaceLogged = true;
            Notice?.Invoke($"G-02F 瓦片绘制首帧：{DrawCount} 片，选片 {SelectedCount}，待建 {PendingCount}");
        }
        return result;
    }
}