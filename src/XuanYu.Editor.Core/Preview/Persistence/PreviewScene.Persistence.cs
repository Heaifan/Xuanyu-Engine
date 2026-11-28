using XuanYu.Editor.Preview.Persistence;
using XuanYu.Render;
using XuanYu.World;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview;

public sealed partial class PreviewScene
{
    internal SceneDocument Capture()
    {
        var entities = _objects.Select(item => new SceneEntityData(World.Find(item.Id)!, item.Mesh?.AssetId,
            World.GetOrganizationParent(item.Id), CaptureRepresentation(item))).ToArray();
        int version = entities.Any(part => part.Representation is not null || part.Entity.Transform.Spatial.Space == XuanYu.World.Ecs.SpatialSpace.Ecef)
            ? 4 : entities.Any(part => part.MeshId is null) ? 3 : 2;
        return new(version, entities, LocalTerrainId is Guid id && LocalTerrainMap is { } map
            ? SceneTerrainCodec.Capture(id, map) : null);
    }

    internal void Restore(SceneDocument document)
    {
        SceneTerrainData? data = document.LocalTerrain;
        MapDocument? localMap = data is null ? null :
            SceneTerrainCodec.RestoreMap(data,
                document.Entities.First(part => part.Entity.Id == data.EntityId)
                    .Entity.Name);
        Dictionary<string, RenderMesh> meshes = [];
        var items = document.Entities.Select(part =>
        {
            if (part.MeshId is not { } meshId)
                return (Entity: part.Entity, Mesh: (RenderMesh?)null);
            if (!meshes.TryGetValue(meshId, out RenderMesh? mesh))
            {
                mesh = data is not null && part.Entity.Id == data.EntityId
                    ? SceneTerrainCodec.RestoreMesh(meshId, localMap!)
                    : PreviewSeed.CreateMesh(meshId);
                meshes.Add(meshId, mesh);
            }
            return (Entity: part.Entity, Mesh: (RenderMesh?)mesh);
        }).ToArray();

        var representations = PrepareRepresentations(document);
        ReplaceWorld(document);
        ClearTransformHistory();
        foreach (PreviewObject item in _objects) _bridge.Unbind(item.Id);
        SelectedId = null;
        _objects.Clear();
        foreach (var item in items) RegisterObject(item.Entity.Id, item.Mesh);
        foreach (var (id, state) in representations) RestoreRepresentation(id, state);
        RegisterLocalTerrain(localMap, data?.EntityId);
        Restored?.Invoke();
        Publish();
    }

    private void ReplaceWorld(SceneDocument document)
    {
        var parents = document.Entities.Where(part => part.OrganizationParentId.HasValue)
            .ToDictionary(part => part.Entity.Id, part => part.OrganizationParentId!.Value);
        try
        {
            World.Replace(document.Entities.Select(part => part.Entity).ToArray(), parents);
        }
        catch (KeyNotFoundException error)
        {
            throw new InvalidDataException("组织关系引用了不存在的实体。", error);
        }
        catch (InvalidOperationException error)
        {
            throw new InvalidDataException("组织关系无效。", error);
        }
    }
}