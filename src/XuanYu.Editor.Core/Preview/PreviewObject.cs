using System.Numerics;
using XuanYu.Render;
using XuanYu.World;

namespace XuanYu.Editor.Preview;

public sealed class PreviewObject
{
    private readonly WorldScene _world;

    public Guid Id { get; }
    public RenderMesh Mesh { get; }

    public string Name => _world.Find(Id)!.Name;

    public bool Visible => _world.Find(Id)!.IsVisible;

    public Matrix4x4 ModelToClip =>
        _world.Find(Id)!.Transform.ToMatrix();

    internal PreviewObject(
        WorldScene world,
        Guid id,
        RenderMesh mesh)
    {
        _world = world;
        Id = id;
        Mesh = mesh;
    }

    public override string ToString() => Name;
}