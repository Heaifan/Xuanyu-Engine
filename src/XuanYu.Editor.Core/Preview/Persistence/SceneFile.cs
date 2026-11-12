using System.Numerics;
using System.Text.Json;

namespace XuanYu.Editor.Preview.Persistence;

public static class SceneFile
{
    private static readonly JsonSerializerOptions Options = new()
    {
        // Vector3 和 Quaternion 的分量是字段，默认 JSON 配置不会保存它们。
        IncludeFields = true,
        WriteIndented = true
    };

    public static void Save(PreviewScene scene, string path)
    {
        string json = JsonSerializer.Serialize(scene.Capture(), Options);
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
        scene.Restore(document!);
    }

    private static void Validate(SceneDocument? document)
    {
        if (document is null || document.Version != 1 || document.Entities is null)
            throw new InvalidDataException("场景版本或实体列表无效。");

        HashSet<Guid> ids = [];
        foreach (SceneEntityData? part in document.Entities)
        {
            if (part?.Entity is not { } entity ||
                entity.Id == Guid.Empty ||
                string.IsNullOrWhiteSpace(entity.Name) ||
                entity.Name != entity.Name.Trim() ||
                !ids.Add(entity.Id))
                throw new InvalidDataException("场景包含无效或重复的实体。");
            if (part.MeshId is not ("builtin/cube" or
                "demo/colored-face" or "demo/quad"))
                throw new InvalidDataException($"不支持的网格资产：{part.MeshId}");
            if (!Valid(entity.Transform))
                throw new InvalidDataException($"实体 Transform 无效：{entity.Name}");
        }
    }

    private static bool Valid(XuanYu.World.Transform value) =>
        Finite(value.Position) && Finite(value.Scale) &&
        Finite(value.Rotation) && value.Rotation.LengthSquared() > 1e-12f;

    private static bool Finite(Vector3 value) =>
        float.IsFinite(value.X) && float.IsFinite(value.Y) &&
        float.IsFinite(value.Z);

    private static bool Finite(Quaternion value) =>
        float.IsFinite(value.X) && float.IsFinite(value.Y) &&
        float.IsFinite(value.Z) && float.IsFinite(value.W);
}