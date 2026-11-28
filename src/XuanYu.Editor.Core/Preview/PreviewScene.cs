using System.Collections.ObjectModel;
using System.Numerics;
using XuanYu.Editor.Camera;
using XuanYu.Render;
using XuanYu.Runtime;
using XuanYu.World;

namespace XuanYu.Editor.Preview;

public sealed partial class PreviewScene
{
    private readonly ObservableCollection<PreviewObject> _objects = [];
    private readonly WorldRenderBridge _bridge;
    public WorldScene World { get; } = new();
    public ViewportCamera? Camera { get; set; }
    public ReadOnlyObservableCollection<PreviewObject> Objects { get; }
    public IReadOnlyList<RenderDraw> Draws =>
        _bridge.Snapshot(Matrix4x4.Identity, SelectedId);
    public Guid? SelectedId { get; private set; }
    public PreviewObject? Selected =>
        _objects.FirstOrDefault(x => x.Id == SelectedId);
    public event Action? Changed;

    public PreviewScene()
    {
        Objects = new ReadOnlyObservableCollection<PreviewObject>(_objects);
        _bridge = new WorldRenderBridge(World);
        UnitIcons = new UnitIconLibrary(SvgAssets, Environment.CurrentDirectory);
    }

    public Matrix4x4 ViewProjection(uint width, uint height) =>
        Camera?.ViewProjection(width, height) ?? Matrix4x4.Identity;

    public IReadOnlyList<RenderDraw> ViewportDraws(uint width, uint height) =>
        RenderWithProjection(ViewProjection(width, height), width, height);

    public IReadOnlyList<RenderDraw> RenderWithProjection(Matrix4x4 projection, uint width = 0, uint height = 0) =>
        _bridge.Snapshot(projection, SelectedId, width, height);

    public IReadOnlyList<RenderDraw> RenderGeographic(GeoRenderFrame frame, uint width = 0, uint height = 0) =>
        _bridge.SnapshotGeographic(frame, SelectedId, width, height);

    public PreviewObject Add(string name, RenderMesh mesh, Matrix4x4 modelToClip)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        if (!Matrix4x4.Decompose(modelToClip, out Vector3 scale,
            out Quaternion rotation, out Vector3 position))
            throw new ArgumentException("对象矩阵无法分解。", nameof(modelToClip));
        WorldEntity entity = World.Create(name);
        World.SetTransform(entity.Id, new Transform(position, rotation, scale));
        var item = RegisterObject(entity.Id, mesh);
        Publish();
        return item;
    }

    public void Select(Guid? id)
    {
        if (id is not null && _objects.All(x => x.Id != id))
            throw new ArgumentOutOfRangeException(nameof(id));
        if (SelectedId == id) return;
        SelectedId = id;
        Publish();
    }

    public void SetVisible(Guid id, bool visible)
    {
        if (World.Find(id)?.IsVisible == visible) return;
        World.SetVisible(id, visible);
        Publish();
    }

    public void SetTransform(Guid id, Transform transform, bool recordHistory = true)
    {
        var before = World.Find(id)?.Transform ?? throw new KeyNotFoundException($"实体不存在：{id}");
        if (before == transform) return;
        var after = World.SetTransform(id, transform).Transform;
        if (recordHistory && _transformEdit?.Id != id) RecordTransform(id, before, after);
        Publish();
    }

    public bool Delete(Guid id)
    {
        if (!World.Delete(id)) return false;
        ClearTransformHistory();
        ClearLocalTerrain(id);
        _bridge.Unbind(id);
        PreviewObject? item = _objects.FirstOrDefault(x => x.Id == id);
        if (item is not null) _objects.Remove(item);
        if (SelectedId == id) SelectedId = null;
        Publish();
        return true;
    }

    private void Publish() => Changed?.Invoke();
}