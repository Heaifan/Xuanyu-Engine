using System.Numerics;
using XuanYu.Render;
using XuanYu.World;
using XuanYu.World.Ecs;

namespace XuanYu.Editor.Preview;

public sealed class PreviewObject
{
    private readonly WorldScene _world;

    public Guid Id { get; }
    public RenderMesh? Mesh { get; private set; }
    public string Name => _world.Find(Id)!.Name;
    public bool IsGeographic => _world.GetSpatialPosition(Id).Space == SpatialSpace.Ecef;
    public bool Visible => _world.Find(Id)!.IsVisible;
    public Matrix4x4 ModelToClip =>
        _world.Find(Id)!.Transform.ToMatrix();

    internal PreviewObject(WorldScene world, Guid id, RenderMesh? mesh)
    {
        _world = world;
        Id = id;
        Mesh = mesh;
    }

    internal void ReplaceMesh(RenderMesh mesh) => Mesh = mesh;

    public override string ToString() => Name;
}