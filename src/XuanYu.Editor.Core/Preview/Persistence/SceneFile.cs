using System.Numerics;
using System.Text.Json;
using XuanYu.World.Ecs;

namespace XuanYu.Editor.Preview.Persistence;

public static partial class SceneFile
{
    private static readonly JsonSerializerOptions Options = new()
    {
        // Vector3 和 Quaternion 的分量是字段，默认 JSON 配置不会保存它们。
        IncludeFields = true,
        WriteIndented = true
    };

    public static void Save(PreviewScene scene, string path)
    {
        var entities = scene.World.Entities;
        if (!entities.Select(entity => entity.Id).ToHashSet().SetEquals(scene.Objects.Select(item => item.Id)))
            throw new InvalidOperationException("场景包含未登记到编辑器的实体，无法完整保存。");
        string json = JsonSerializer.Serialize(MapAssetPaths(scene.Capture(), path, saving: true), Options);
        File.WriteAllText(path, json);
    }

    public static void Load(PreviewScene scene, string path)
    {
        string json = File.ReadAllText(path);
        SceneDocument? document;
        try
        {
            document = JsonSerializer.Deserialize<SceneDocument>(json, Options);
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("场景 JSON 格式错误。", error);
        }
        Validate(document);
        scene.Restore(MapAssetPaths(document!, path, saving: false));
    }

    private static void Validate(SceneDocument? document)
    {
        if (document is null || document.Version is not (1 or 2 or 3 or 4) || document.Entities is null)
            throw new InvalidDataException("场景版本或实体列表无效。");
        SceneTerrainData? terrain = document.LocalTerrain;
        if (terrain is not null)
            ValidateTerrain(terrain, document.Entities);
        HashSet<Guid> ids = [];
        foreach (SceneEntityData? part in document.Entities)
        {
            if (part?.Entity is not { } entity ||
                entity.Id == Guid.Empty ||
                string.IsNullOrWhiteSpace(entity.Name) ||
                entity.Name != entity.Name.Trim() ||
                !ids.Add(entity.Id))
                throw new InvalidDataException("场景包含无效或重复的实体。");
            if (document.Version == 1 && part.OrganizationParentId is not null)
                throw new InvalidDataException("v1 场景不支持组织关系。");
            bool local = terrain is not null && entity.Id == terrain.EntityId &&
                part.MeshId?.StartsWith("terrain/local/", StringComparison.Ordinal) == true;
            bool meshless = document.Version >= 3 && part.MeshId is null;
            if (!local && !meshless && part.MeshId is not ("builtin/cube" or
                "demo/colored-face" or "demo/quad"))
                throw new InvalidDataException($"不支持的网格资产：{part.MeshId}");
            ValidateRepresentation(part, document.Version, local);
            if (!Valid(entity.Transform, document.Version, local))
                throw new InvalidDataException($"实体 Transform 无效：{entity.Name}");
        }
    }

    private static void ValidateTerrain(SceneTerrainData data,
        SceneEntityData[] entities)
    {
        if (data.SamplesX is < 2 or > 257 || data.SamplesY is < 2 or > 257 ||
            !float.IsFinite(data.SpacingX) || data.SpacingX <= 0 ||
            !float.IsFinite(data.SpacingY) || data.SpacingY <= 0 ||
            data.Heights is null ||
            data.Heights.Length != data.SamplesX * data.SamplesY ||
            data.Heights.Any(height => !float.IsFinite(height)) ||
            !entities.Any(part => part?.Entity?.Id == data.EntityId &&
                part.MeshId is not null &&
                part.MeshId.StartsWith("terrain/local/", StringComparison.Ordinal)))
            throw new InvalidDataException("场景中的本地 Terrain 高程数据无效。");
    }

}