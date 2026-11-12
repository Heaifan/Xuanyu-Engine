using XuanYu.Editor.Preview.Persistence;
using XuanYu.Render;
using XuanYu.World;

namespace XuanYu.Editor.Preview;

public sealed partial class PreviewScene
{
    internal SceneDocument Capture() => new(1,
        _objects.Select(item => new SceneEntityData(
            World.Find(item.Id)!, item.Mesh.AssetId)).ToArray());

    internal void Restore(SceneDocument document)
    {
        Dictionary<string, RenderMesh> meshes = [];
        var items = document.Entities.Select(part =>
        {
            if (!meshes.TryGetValue(part.MeshId, out RenderMesh? mesh))
            {
                mesh = PreviewSeed.CreateMesh(part.MeshId);
                meshes.Add(part.MeshId, mesh);
            }
            return (Entity: part.Entity, Mesh: mesh);
        }).ToArray();

        foreach (PreviewObject item in _objects)
            _bridge.Unbind(item.Id);
        World.Replace(items.Select(item => item.Entity).ToArray());
        SelectedId = null;
        _objects.Clear();
        foreach (var item in items)
        {
            _bridge.Bind(item.Entity.Id, item.Mesh);
            _objects.Add(new PreviewObject(World, item.Entity.Id, item.Mesh));
        }
        Publish();
    }
}