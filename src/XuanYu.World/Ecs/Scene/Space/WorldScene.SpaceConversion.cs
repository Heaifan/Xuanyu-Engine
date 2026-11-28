using XuanYu.Core.Geodesy;
using XuanYu.World.Ecs;

namespace XuanYu.World;

public sealed partial class WorldScene
{
    public WorldEntity ConvertSpace(Guid id, GeoCoordinate anchor)
    {
        var current = GetSpatialPosition(id);
        var frame = new EnuFrame(anchor);
        SpatialPosition next;
        if (current.Space == SpatialSpace.Local)
            next = SpatialPosition.FromEnu(frame, new EnuPosition(current.X, current.Y, current.Z));
        else
        {
            var enu = current.ToEnu(frame);
            next = SpatialPosition.FromLocal(enu.East, enu.North, enu.Up);
        }
        return RestoreSpacePosition(id, next);
    }

    public WorldEntity RestoreSpacePosition(Guid id, SpatialPosition position)
    {
        var handle = Require(id);
        _ecs.Set(handle, position);
        return Read(id, handle);
    }
}