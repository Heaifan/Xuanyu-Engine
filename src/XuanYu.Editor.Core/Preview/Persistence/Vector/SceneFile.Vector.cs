using System.Numerics;
using XuanYu.World.Ecs;
using XuanYu.Runtime;

namespace XuanYu.Editor.Preview.Persistence;

public static partial class SceneFile
{
    private static SceneDocument MapAssetPaths(SceneDocument document, string path, bool saving)
    {
        string folder = Path.GetDirectoryName(Path.GetFullPath(path))!;
        return document with { Entities = document.Entities.Select(part =>
            part.Representation is { SvgPath: { } asset } data
                ? part with { Representation = data with { SvgPath = saving ? Path.GetRelativePath(folder, asset)
                    : Path.GetFullPath(asset, folder) } } : part).ToArray() };
    }

    private static void ValidateRepresentation(SceneEntityData part, int version, bool terrain)
    {
        if (part.Representation is not { } data) return;
        bool supported = !terrain && (part.MeshId is null or "builtin/cube");
        bool valid = data.Kind switch
        {
            EntityRepresentationKind.None or EntityRepresentationKind.Point => true,
            EntityRepresentationKind.Symbol => data.SvgPath is not null,
            EntityRepresentationKind.Mesh or EntityRepresentationKind.Proxy => part.MeshId == "builtin/cube",
            _ => false
        };
        if (version != 4 || !supported || !valid || !float.IsFinite(data.Style.Size) || data.Style.Size <= 0 ||
            (data.SvgPath is { } path && (string.IsNullOrWhiteSpace(path) ||
                !string.Equals(Path.GetExtension(path), ".svg", StringComparison.OrdinalIgnoreCase))))
            throw new InvalidDataException("场景二维表现数据无效。");
    }

    private static bool Valid(XuanYu.World.Transform value, int version, bool terrain)
    {
        var p = value.Spatial;
        bool validSpace = p.Space == SpatialSpace.Local || (version == 4 && !terrain && p.Space == SpatialSpace.Ecef &&
            (p.X != 0 || p.Y != 0 || p.Z != 0));
        return validSpace && (p.Space != SpatialSpace.Local || Finite(value.Position)) && double.IsFinite(p.X) && double.IsFinite(p.Y) && double.IsFinite(p.Z) &&
            Finite(value.Scale) && Finite(value.Rotation) && value.Rotation.LengthSquared() > 1e-12f;
    }

    private static bool Finite(Vector3 value) => float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);
    private static bool Finite(Quaternion value) => float.IsFinite(value.X) && float.IsFinite(value.Y) &&
        float.IsFinite(value.Z) && float.IsFinite(value.W);
}