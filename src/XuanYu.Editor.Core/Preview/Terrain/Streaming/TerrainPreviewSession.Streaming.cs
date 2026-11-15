using System.Numerics;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class TerrainPreviewSession
{
    private sealed class Chunk(int x, int y, Guid id)
    {
        public int X { get; } = x;
        public int Y { get; } = y;
        public Guid Id { get; } = id;
        public int Step { get; set; } = 50;
    }

    private readonly List<Chunk> _chunks = [];
    private (float Min, float Max) _heightRange;

    private void CreateChunks()
    {
        TerrainHeightField field = Map.HeightField;
        ReadOnlySpan<float> heights = field.Samples;
        float min = float.PositiveInfinity, max = float.NegativeInfinity;
        foreach (float value in heights)
        {
            min = MathF.Min(min, value);
            max = MathF.Max(max, value);
        }
        _heightRange = (min, max);
        int countX = (field.SamplesX - 2) / TerrainTileMeshBuilder.TileCells + 1;
        int countY = (field.SamplesY - 2) / TerrainTileMeshBuilder.TileCells + 1;
        for (int y = 0; y < countY; y++)
            for (int x = 0; x < countX; x++)
            {
                int startX = x * TerrainTileMeshBuilder.TileCells;
                int startY = y * TerrainTileMeshBuilder.TileCells;
                var model = Matrix4x4.CreateScale(DisplayScale);
                var mesh = TerrainTileMeshBuilder.Create(field, startX, startY,
                    50, Map.WaterMask, _heightRange);
                var obj = Scene.Add($"地形 {x:D2}-{y:D2}", mesh, model);
                _chunks.Add(new Chunk(startX, startY, obj.Id));
            }
    }
}