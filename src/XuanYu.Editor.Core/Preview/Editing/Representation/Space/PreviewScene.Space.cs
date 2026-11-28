using XuanYu.Core.Geodesy;
using XuanYu.World.Ecs;

namespace XuanYu.Editor.Preview;

public sealed partial class PreviewScene
{
    public event Action? SpaceChanged;

    public void ConvertUnitSpace(Guid id, GeoCoordinate anchor)
    {
        if (_objects.First(x => x.Id == id).Mesh is not null)
            throw new InvalidOperationException("当前仅支持无网格单位的空间转换。");
        if (_transformEdit is not null) throw new InvalidOperationException("请先结束变换操作。");
        var before = World.GetSpatialPosition(id);
        var after = World.ConvertSpace(id, anchor).Transform.Spatial;
        RecordEdit(() => ApplySpace(id, before), () => ApplySpace(id, after));
        SpaceChanged?.Invoke();
        Publish();
    }

    private void ApplySpace(Guid id, SpatialPosition position)
    {
        World.RestoreSpacePosition(id, position);
        SpaceChanged?.Invoke();
    }
}