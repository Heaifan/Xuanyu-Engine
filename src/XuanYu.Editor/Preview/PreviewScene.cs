
using System.Collections.ObjectModel;
using System.Numerics;
using XuanYu.Render;

namespace XuanYu.Editor.Preview;

public sealed class PreviewScene
{
    private readonly ObservableCollection<PreviewObject> _objects = [];
    private RenderDraw[] _draws = [];

    public ReadOnlyObservableCollection<PreviewObject> Objects { get; }

    public IReadOnlyList<RenderDraw> Draws => _draws;

    public Guid? SelectedId { get; private set; }

    public PreviewObject? Selected =>
        _objects.FirstOrDefault(x => x.Id == SelectedId);

    public event Action? Changed;

    public PreviewScene()
    {
        Objects = new ReadOnlyObservableCollection<PreviewObject>(_objects);
    }

    public PreviewObject Add(
        string name,
        RenderMesh mesh,
        Matrix4x4 modelToClip)
    {
        var item = new PreviewObject(name, mesh, modelToClip);
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
        PreviewObject item = _objects.FirstOrDefault(x => x.Id == id)
            ?? throw new ArgumentOutOfRangeException(nameof(id));

        if (item.Visible == visible)
            return;

        item.Visible = visible;
        Publish();
    }

    private void Publish()
    {
        _draws = _objects.Select(x => new RenderDraw(
            x.Mesh,
            x.ModelToClip,
            x.Visible,
            x.Id == SelectedId)).ToArray();

        Changed?.Invoke();
    }
}
