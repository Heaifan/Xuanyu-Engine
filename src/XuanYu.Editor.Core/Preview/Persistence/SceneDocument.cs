using XuanYu.World;

namespace XuanYu.Editor.Preview.Persistence;

public sealed record SceneDocument(int Version, SceneEntityData[] Entities,
    SceneTerrainData? LocalTerrain = null);

public sealed record SceneEntityData(WorldEntity Entity, string MeshId);

public sealed record SceneTerrainData(Guid EntityId, int SamplesX, int SamplesY,
    float SpacingX, float SpacingY, float[] Heights);