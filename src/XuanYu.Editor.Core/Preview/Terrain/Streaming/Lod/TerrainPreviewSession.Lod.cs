using System.Numerics;
using XuanYu.Editor.Camera;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class TerrainPreviewSession
{
    private sealed record Planned(Chunk Chunk, int Needed, int Ahead,
        float Distance, float FocusDistance);
    private sealed record Built(Chunk Chunk, int Step, RenderMesh Mesh);
    private Task<Built[]>? _buildJob;

    private void UpdateChunks()
    {
        if (_chunks.Count == 0 || Scene.Camera is not { } camera) return;
        if (_buildJob is { IsCompleted: true } finished)
        {
            foreach (Built item in finished.GetAwaiter().GetResult())
                CacheMesh(item.Chunk, item.Step, item.Mesh);
            _buildJob = null;
        }

        Planned[] plans = PlanChunks(camera);
        foreach (Planned item in plans)
        {
            if (item.Chunk.Step == item.Needed) continue;
            if (TakeCached(item.Chunk, item.Needed) is { } mesh)
                ApplyMesh(item.Chunk, item.Needed, mesh);
        }
        if (_buildJob is not null) return;

        Planned[] candidates = plans.Where(item =>
                item.Chunk.Step != item.Needed &&
                !HasCached(item.Chunk, item.Needed))
            .OrderBy(item => item.Distance).Take(2).ToArray();
        bool required = candidates.Length != 0;
        if (!required && _cacheBytes < 64L * 1024 * 1024)
            candidates = plans.OrderBy(item => item.FocusDistance)
                .Take(6).Where(item =>
                    item.Ahead < item.Chunk.Step &&
                    !HasCached(item.Chunk, item.Ahead))
                .Take(2).ToArray();
        if (candidates.Length == 0) return;

        TerrainHeightField field = Map.HeightField;
        _buildJob = Task.Run(() => candidates.Select(item =>
        {
            int step = required ? item.Needed : item.Ahead;
            return new Built(item.Chunk, step, TerrainTileMeshBuilder.Create(
                field, item.Chunk.X, item.Chunk.Y, step,
                Map.WaterMask, _heightRange, _shading));
        }).ToArray());
    }

    private Planned[] PlanChunks(ViewportCamera camera)
    {
        TerrainHeightField field = Map.HeightField;
        float tileSize = TerrainTileMeshBuilder.TileCells * MathF.Max(
            field.SpacingXMeters, field.SpacingYMeters) * DisplayScale;
        Vector3 predicted = camera.Target +
            (camera.Position - camera.Target) * 0.72f;
        return _chunks.Select(chunk =>
        {
            var center = new Vector3(
                (chunk.X + 150f) * field.SpacingXMeters * DisplayScale,
                (chunk.Y + 150f) * field.SpacingYMeters * DisplayScale,
                field.GetHeight(Math.Min(chunk.X + 150, field.SamplesX - 1),
                    Math.Min(chunk.Y + 150, field.SamplesY - 1)) * DisplayScale);
            float distance = Vector3.Distance(camera.Position, center);
            int needed = Math.Min(20, SelectStep(chunk.Step, distance / tileSize));
            int ahead = Math.Min(needed, Math.Min(20,
                SelectStep(chunk.Step, Vector3.Distance(predicted, center) / tileSize)));
            return new Planned(chunk, needed, ahead, distance,
                Vector3.Distance(camera.Target, center));
        }).ToArray();
    }

    private static int SelectStep(int current, float d)
    {
        if (current == 50 && d < 5.1f) return d < 1.275f ? 1 : d < 2.55f ? 5 : 20;
        if (current == 20 && d < 2.55f) return d < 1.275f ? 1 : 5;
        if (current == 5 && d < 1.275f) return 1;
        if (current == 1 && d > 1.725f) return d > 6.9f ? 50 : d > 3.45f ? 20 : 5;
        if (current == 5 && d > 3.45f) return d > 6.9f ? 50 : 20;
        if (current == 20 && d > 6.9f) return 50;
        return current;
    }
}