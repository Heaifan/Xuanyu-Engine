using System.Numerics;
using XuanYu.Render;
using XuanYu.World.Ecs;

namespace XuanYu.Editor.Preview;

public sealed partial class PreviewScene
{
    private static readonly RenderMesh CubeMesh = PreviewCubeMesh.Create();

    public event Action? EntityRenamed;

    public PreviewObject CreateCube()
    {
        int number = NextCubeNumber();

        PreviewObject cube = Add($"Cube_{number:000}", CubeMesh,
            Matrix4x4.CreateTranslation((number - 1) * 1.9f, 0f, 0f));
        Select(cube.Id);
        return cube;
    }

    public PreviewObject CreateGeographicCube(SpatialPosition position)
    {
        if (position.Space != SpatialSpace.Ecef)
            throw new ArgumentException("全球立方体需要 ECEF 位置。", nameof(position));
        var entity = World.Create($"Cube_{NextCubeNumber():000}", position);
        var item = new PreviewObject(World, entity.Id, CubeMesh);
        _bridge.Bind(entity.Id, CubeMesh);
        _objects.Add(item);
        Publish();
        return item;
    }

    private int NextCubeNumber()
    {
        int number = 1;
        while (World.Entities.Any(x => x.Name == $"Cube_{number:000}")) number++;
        return number;
    }

    public void Rename(Guid id, string name)
    {
        World.Rename(id, name);
        EntityRenamed?.Invoke();
        Publish();
    }

    public void ReplaceMesh(Guid id, RenderMesh mesh)
    {
        PreviewObject item = _objects.First(x => x.Id == id);
        _bridge.Bind(id, mesh);
        item.ReplaceMesh(mesh);
    }
}