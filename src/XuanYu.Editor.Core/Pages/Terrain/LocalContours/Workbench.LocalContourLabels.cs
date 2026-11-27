using System.Numerics;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    private bool _localContourLabelsEnabled = true;
    private readonly GeoContourLabelVisibility _localLabelVisibility = new();
    private (RenderMesh Mesh, Matrix4x4 Projection, int Interval, uint Width, uint Height)? _localLabelKey;
    private GeoContourLabel[] _localLabelCandidates = [];

    private int FindLocalLabelDraw(IReadOnlyList<RenderDraw> draws)
    {
        if ((IsSimulating && CurrentId != PageId.Terrain) || !HasLocalTerrainContours ||
            !_localContoursEnabled || !_localContourLabelsEnabled || _sculptStroke is not null)
            return -1;
        var session = _terrainPreview!;
        var item = session.Scene.Objects.FirstOrDefault(o => o.Id == session.TerrainObjectId);
        if (item is null || !item.Visible) return -1;
        for (int i = 0; i < draws.Count; i++)
            if (ReferenceEquals(draws[i].Mesh, item.Mesh) && draws[i].Visible &&
                draws[i].ContourOpacity > 0f && draws[i].ContourIntervalMeters > 0f) return i;
        return -1;
    }

    private static int LabelInterval(RenderDraw draw) => (int)(draw.ContourIntervalBlend >= 0.5f
        && draw.ContourNextIntervalMeters > 0f ? draw.ContourNextIntervalMeters : draw.ContourIntervalMeters);

    private GeoContourLabel[] BuildLocalContourLabels(IReadOnlyList<RenderDraw> draws,
        uint width, uint height)
    {
        int index = FindLocalLabelDraw(draws);
        if (index < 0 || _terrainPreview!.Geometry is not { } geometry) return [];
        var draw = draws[index];
        int interval = LabelInterval(draw);
        var key = (draw.Mesh, draw.ModelToClip, interval, width, height);
        if (_localLabelKey != key)
        {
            _localLabelCandidates = LocalContourLabelCandidates.Build(draw, geometry,
                interval, width, height);
            _localLabelKey = key;
        }
        return _localLabelCandidates;
    }

    private RenderDraw[] AttachLocalContourGaps(IReadOnlyList<RenderDraw> draws,
        IReadOnlyList<GeoContourLabel> anchors, uint width, uint height, float scale,
        out GeoContourLabel[] visible, Func<int, float>? measure)
    {
        var result = draws.ToArray();
        visible = [];
        int index = FindLocalLabelDraw(draws);
        if (index < 0 || anchors.Count == 0) return result;
        var draw = draws[index];
        int major = LabelInterval(draw) * 5;
        var bound = ContourLabelGapLayout.Attach([draw], anchors, draw.ModelToClip,
            _localLabelVisibility, (_, label) => label.ElevationMeters % major == 0,
            width, height, scale, out visible, measure);
        result[index] = bound[0];
        return result;
    }
}