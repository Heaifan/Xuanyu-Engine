
using System.Numerics;

namespace XuanYu.World.Maps;

public sealed class TerrainMeshGeometry
{
    public Vector3[] Positions { get; }
    public Vector3[] Normals { get; }
    public int[] Indices { get; }

    public int VertexCount => Positions.Length;
    public int TriangleCount => Indices.Length / 3;

    public TerrainMeshGeometry(
        Vector3[] positions,
        Vector3[] normals,
        int[] indices)
    {
        Positions = positions;
        Normals = normals;
        Indices = indices;
    }
}
