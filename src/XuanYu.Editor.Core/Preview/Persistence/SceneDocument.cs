using System.Text.Json.Serialization;
using XuanYu.World;
using XuanYu.Runtime;

namespace XuanYu.Editor.Preview.Persistence;

public sealed record SceneDocument(int Version, SceneEntityData[] Entities,
    SceneTerrainData? LocalTerrain = null);

public sealed record SceneEntityData(WorldEntity Entity, [property: JsonRequired] string? MeshId,
    Guid? OrganizationParentId = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] SceneRepresentationData? Representation = null);

public sealed record SceneRepresentationData(EntityRepresentationKind Kind, string? SvgPath, BillboardStyle Style);

public sealed record SceneTerrainData(Guid EntityId, int SamplesX, int SamplesY,
    float SpacingX, float SpacingY, float[] Heights);