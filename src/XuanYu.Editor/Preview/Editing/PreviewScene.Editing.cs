using System.Numerics;
using XuanYu.Render;

namespace XuanYu.Editor.Preview;

public sealed partial class PreviewScene
{
    private static readonly RenderMesh CubeMesh = PreviewCubeMesh.Create();

    public event Action? EntityRenamed;

    public PreviewObject CreateCube()
    {
        int number = 1;
        while (World.Entities.Any(x => x.Name == $"Cube_{number:000}"))
            number++;

        PreviewObject cube = Add($"Cube_{number:000}", CubeMesh,
            Matrix4x4.CreateTranslation((number - 1) * 1.9f, 0f, 0f));
        Select(cube.Id);
        return cube;
    }

    public void Rename(Guid id, string name)
    {
        World.Rename(id, name);
        EntityRenamed?.Invoke();
        Publish();
    }
}