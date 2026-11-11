using System.Collections.ObjectModel;
using System.Numerics;
using XuanYu.Editor.Camera;
using XuanYu.Render;
using XuanYu.Runtime;
using XuanYu.World;

namespace XuanYu.Editor.Preview;

public sealed class PreviewScene
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
    }

    public Matrix4x4 ViewProjection(uint width, uint height) =>
        Camera?.ViewProjection(width, height) ?? Matrix4x4.Identity;

    public IReadOnlyList<RenderDraw> ViewportDraws(uint width, uint height) =>
        _bridge.Snapshot(ViewProjection(width, height), SelectedId);

    public PreviewObject Add(string name, RenderMesh mesh, Matrix4x4 modelToClip)
    {
        if (!Matrix4x4.Decompose(modelToClip, out Vector3 scale,
            out Quaternion rotation, out Vector3 position))
            throw new ArgumentException("对象矩阵无法分解。", nameof(modelToClip));
        WorldEntity entity = World.Create(name);
        World.SetTransform(entity.Id, new Transform(position, rotation, scale));
        var item = new PreviewObject(World, entity.Id, mesh);
        _bridge.Bind(entity.Id, mesh);
        _objects.Add(item);
        Publish();
        return item;
    }

    public void Select(Guid? id)
    {
        if (id is not null && _objects.All(x => x.Id != id))
            throw new ArgumentOutOfRangeException(nameof(id));
        if (SelectedId == id)
            return;
        SelectedId = id;
        Publish();
    }

    public void SetVisible(Guid id, bool visible)
    {
        if (World.Find(id)?.IsVisible == visible)
            return;
        World.SetVisible(id, visible);
        Publish();
    }

    public void SetTransform(Guid id, Transform transform)
    {
        World.SetTransform(id, transform);
        Publish();
    }

    public bool Delete(Guid id)
    {
        if (!World.Delete(id))
            return false;
        _bridge.Unbind(id);
        PreviewObject? item = _objects.FirstOrDefault(x => x.Id == id);
        if (item is not null)
            _objects.Remove(item);
        if (SelectedId == id)
            SelectedId = null;
        Publish();
        return true;
    }

    private void Publish() => Changed?.Invoke();
}
