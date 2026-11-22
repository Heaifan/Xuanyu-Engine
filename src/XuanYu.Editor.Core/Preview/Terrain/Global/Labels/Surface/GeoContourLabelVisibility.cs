using System.Numerics;
using XuanYu.Render;

namespace XuanYu.Editor.Preview.Terrain;

internal sealed class GeoContourLabelVisibility
{
    private Matrix4x4 _projection;
    private (RenderMesh Mesh, bool Visible)[] _draws = [];
    private readonly Dictionary<(GeoContourLabel Label, float Width, uint ViewWidth, uint ViewHeight), bool> _cache = [];

    public void Prepare(IReadOnlyList<RenderDraw> draws, Matrix4x4 projection)
    {
        bool changed = projection != _projection || draws.Count != _draws.Length;
        for (int i = 0; !changed && i < draws.Count; i++)
            changed = !ReferenceEquals(draws[i].Mesh, _draws[i].Mesh) ||
                draws[i].Visible != _draws[i].Visible;
        if (!changed) return;
        _projection = projection;
        _draws = draws.Select(d => (d.Mesh, d.Visible)).ToArray();
        _cache.Clear();
    }

    public bool IsVisible(GeoContourLabel label, float halfWidth,
        float radians, uint width, uint height)
    {
        var key = (label, halfWidth, width, height);
        if (_cache.TryGetValue(key, out bool found)) return found;
        Vector2 point = new(label.X, label.Y);
        bool visible = label.Owner is { } mesh && GeoContourLabelSpan.Fits(mesh,
            _projection, point, radians, halfWidth, width, height) &&
            GeoTerrainCursorPicker.TryPick(_projection, point, width, height,
                _draws.Where(d => d.Visible && !d.Mesh.TerrainElevations.IsEmpty)
                    .Select(d => d.Mesh), out Vector3 hit) &&
            Vector3.Distance(hit, new Vector3(label.East, label.North, label.Up)) <= 2f;
        _cache[key] = visible;
        return visible;
    }
}
