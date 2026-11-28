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
        int number = NextNumber("Cube");
        PreviewObject cube = Add($"Cube_{number:000}", CubeMesh,
            Matrix4x4.CreateTranslation((number - 1) * 1.9f, 0f, 0f));
        Select(cube.Id);
        return cube;
    }

    public PreviewObject CreateGeographicCube(SpatialPosition position)
    {
        if (position.Space != SpatialSpace.Ecef)
            throw new ArgumentException("全球立方体需要 ECEF 位置。", nameof(position));
        var entity = World.Create($"Cube_{NextNumber("Cube"):000}", position);
        var item = RegisterObject(entity.Id, CubeMesh);
        Publish();
        return item;
    }

    public PreviewObject CreateLogicEntity(SpatialPosition position = default)
    {
        var entity = World.Create($"Entity_{NextNumber("Entity"):000}", position);
        var item = RegisterObject(entity.Id, null);
        Select(item.Id);
        return item;
    }

    private PreviewObject RegisterObject(Guid id, RenderMesh? mesh)
    {
        var item = new PreviewObject(World, id, mesh);
        if (mesh is not null) _bridge.Bind(id, mesh);
        _objects.Add(item);
        return item;
    }

    private int NextNumber(string prefix)
    {
        int number = 1;
        while (World.Entities.Any(x => x.Name == $"{prefix}_{number:000}")) number++;
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