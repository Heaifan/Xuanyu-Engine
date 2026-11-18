
using System.Numerics;

namespace XuanYu.World.Maps;

public readonly record struct TerrainRayHit(
    Vector3 WorldPosition,
    int TriangleIndex,
    float Distance);
