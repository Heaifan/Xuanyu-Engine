using XuanYu.World;

namespace XuanYu.Editor.Preview.Persistence;

public sealed record SceneDocument(int Version, SceneEntityData[] Entities);

public sealed record SceneEntityData(WorldEntity Entity, string MeshId);