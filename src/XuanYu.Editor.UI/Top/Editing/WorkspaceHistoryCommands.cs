using Avalonia.Controls;
using Avalonia.Input;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI;

internal sealed class WorkspaceHistoryCommands
{
    private readonly Workbench _wb;
    private readonly MenuItem _undo = new() { InputGesture = new KeyGesture(Key.Z, KeyModifiers.Control) };
    private readonly MenuItem _redo = new() { InputGesture = new KeyGesture(Key.Y, KeyModifiers.Control) };

    public WorkspaceHistoryCommands(Window owner, Workbench wb)
    {
        _wb = wb;
        _undo.Click += (_, _) => Run(false);
        _redo.Click += (_, _) => Run(true);
        owner.KeyDown += OnKeyDown;
        wb.Changed += Refresh;
        Refresh();
    }

    public MenuItem[] Create() => [_undo, _redo];

    private bool CanRun(bool redo) => !_wb.IsSimulating && (_wb.CurrentId switch
    {
        PageId.World => redo ? _wb.Scene.CanRedoTransform : _wb.Scene.CanUndoTransform,
        PageId.Terrain => redo ? _wb.CanRedoTerrain : _wb.CanUndoTerrain,
        _ => false
    });

    private void Run(bool redo)
    {
        if (!CanRun(redo)) return;
        if (_wb.CurrentId == PageId.World)
        {
            if (redo) _wb.Scene.RedoTransform(); else _wb.Scene.UndoTransform();
            _wb.Log.Write(redo ? "已重做实体编辑" : "已撤销实体编辑");
        }
        else if (redo) _wb.RedoTerrain(); else _wb.UndoTerrain();
    }

    private void Refresh()
    {
        string suffix = _wb.CurrentId == PageId.World ? "实体编辑" : _wb.CurrentId == PageId.Terrain ? "地形" : "";
        _undo.Header = "撤销" + suffix; _redo.Header = "重做" + suffix;
        _undo.IsEnabled = CanRun(false); _redo.IsEnabled = CanRun(true);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Source is TextBox || _wb.IsSimulating || _wb.CurrentId is not (PageId.World or PageId.Terrain)) return;
        bool undo = e.Key == Key.Z && e.KeyModifiers == KeyModifiers.Control;
        bool redo = (e.Key == Key.Y && e.KeyModifiers == KeyModifiers.Control) ||
            (e.Key == Key.Z && e.KeyModifiers == (KeyModifiers.Control | KeyModifiers.Shift));
        if (!undo && !redo) return;
        Run(redo);
        e.Handled = true;
    }
}