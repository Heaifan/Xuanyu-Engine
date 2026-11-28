using XuanYu.World;

namespace XuanYu.Editor.Preview;

public sealed partial class PreviewScene
{
    private readonly record struct EditChange(Action Undo, Action Redo);
    private readonly Stack<EditChange> _transformUndo = new(), _transformRedo = new();
    private (Guid Id, Transform Before)? _transformEdit;
    public bool CanUndoTransform => _transformEdit is null && _transformUndo.Count > 0;
    public bool CanRedoTransform => _transformEdit is null && _transformRedo.Count > 0;
    public event Action? TransformHistoryReplayed;

    public void BeginTransformEdit(Guid id)
    {
        if (_transformEdit is not null) throw new InvalidOperationException("已有变换操作正在进行。");
        _transformEdit = (id, World.Find(id)?.Transform ?? throw new KeyNotFoundException($"实体不存在：{id}"));
        Publish();
    }

    public void EndTransformEdit(bool commit)
    {
        if (_transformEdit is not { } edit) return;
        _transformEdit = null;
        var after = World.Find(edit.Id)!.Transform;
        if (commit) RecordTransform(edit.Id, edit.Before, after);
        else if (after != edit.Before) World.SetTransform(edit.Id, edit.Before);
        Publish();
        if (!commit && after != edit.Before) TransformHistoryReplayed?.Invoke();
    }

    private void RecordTransform(Guid id, Transform before, Transform after)
    {
        if (before != after) RecordEdit(() => World.SetTransform(id, before), () => World.SetTransform(id, after));
    }

    private void RecordEdit(Action undo, Action redo)
    {
        _transformUndo.Push(new(undo, redo));
        _transformRedo.Clear();
    }

    public void UndoTransform()
    {
        if (!CanUndoTransform) return;
        var change = _transformUndo.Pop();
        change.Undo();
        _transformRedo.Push(change);
        Publish();
        TransformHistoryReplayed?.Invoke();
    }

    public void RedoTransform()
    {
        if (!CanRedoTransform) return;
        var change = _transformRedo.Pop();
        change.Redo();
        _transformUndo.Push(change);
        Publish();
        TransformHistoryReplayed?.Invoke();
    }

    private void ClearTransformHistory()
    {
        _transformEdit = null;
        _transformUndo.Clear();
        _transformRedo.Clear();
    }
}