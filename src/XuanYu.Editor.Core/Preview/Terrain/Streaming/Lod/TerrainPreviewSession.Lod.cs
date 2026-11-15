using System.Numerics;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class TerrainPreviewSession
{
    private sealed record Planned(Chunk Chunk, int Step, float Distance);
    private sealed record Built(Chunk Chunk, int Step, RenderMesh Mesh);
    private Task<Built[]>? _buildJob;
    private Vector3 _lastCameraPosition = new(float.NaN);
    private Vector3 _lastCameraTarget = new(float.NaN);
    private DateTime _lastMotion = DateTime.UtcNow;
    private DateTime _jobStarted;

    private void UpdateChunks()
    {
        if (_chunks.Count == 0 || Scene.Camera is not { } camera) return;
        DateTime now = DateTime.UtcNow;
        if (camera.Position != _lastCameraPosition ||
            camera.Target != _lastCameraTarget)
        {
            _lastCameraPosition = camera.Position;
            _lastCameraTarget = camera.Target;
            _lastMotion = now;
        }
        if (_buildJob is not null)
        {
            if (!_buildJob.IsCompleted) return;
            Built[] completed = _buildJob.GetAwaiter().GetResult();
            _buildJob = null;
            if (_lastMotion <= _jobStarted)
                foreach (Built item in completed)
                {
                    Scene.ReplaceMesh(item.Chunk.Id, item.Mesh);
                    item.Chunk.Step = item.Step;
                }
        }
        if (now - _lastMotion < TimeSpan.FromMilliseconds(300)) return;
        TerrainHeightField field = Map.HeightField;
        float tileSize = TerrainTileMeshBuilder.TileCells * MathF.Max(
            field.SpacingXMeters, field.SpacingYMeters) * DisplayScale;
        Planned[] pending = _chunks.Select(chunk =>
        {
            var center = new Vector3(
                (chunk.X + TerrainTileMeshBuilder.TileCells * 0.5f) *
                    field.SpacingXMeters * DisplayScale,
                (chunk.Y + TerrainTileMeshBuilder.TileCells * 0.5f) *
                    field.SpacingYMeters * DisplayScale,
                field.GetHeight(Math.Min(chunk.X + 150, field.SamplesX - 1),
                    Math.Min(chunk.Y + 150, field.SamplesY - 1)) * DisplayScale);
            float distance = Vector3.Distance(camera.Position, center);
            return new Planned(chunk,
                SelectStep(chunk.Step, distance / tileSize), distance);
        }).Where(item => item.Chunk.Step != item.Step)
          .OrderBy(item => item.Distance).Take(2).ToArray();
        if (pending.Length == 0) return;
        _jobStarted = now;
        _buildJob = Task.Run(() => pending.Select(item => new Built(
            item.Chunk, item.Step, TerrainTileMeshBuilder.Create(
                field, item.Chunk.X, item.Chunk.Y, item.Step,
                Map.WaterMask, _heightRange))).ToArray());
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