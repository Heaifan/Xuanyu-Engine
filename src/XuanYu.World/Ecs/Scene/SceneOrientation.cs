using System.Numerics;

namespace XuanYu.World.Ecs;

internal readonly record struct SceneOrientation(Quaternion Rotation, Vector3 Scale);