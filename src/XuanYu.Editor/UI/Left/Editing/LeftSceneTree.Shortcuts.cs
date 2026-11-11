using Avalonia.Controls;
using Avalonia.Input;

namespace XuanYu.Editor.UI.Left;

internal sealed partial class LeftSceneTree
{
    private void OnTreeKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Source is TextBox)
            return;

        if (e.Key == Key.A && e.KeyModifiers == KeyModifiers.Shift)
        {
            Control target = this;
            if (_scene.SelectedId is { } id &&
                _objectRows.TryGetValue(id, out LeftTreeRow? row))
                target = row;
            target.ContextMenu?.Open(target);
            e.Handled = true;
            return;
        }

        if (_scene.SelectedId is not { } selected ||
            !_objectRows.TryGetValue(selected, out LeftTreeRow? item))
            return;

        if (e.Key == Key.F2 && e.KeyModifiers == KeyModifiers.None)
        {
            item.BeginRename(name => _scene.Rename(selected, name));
            e.Handled = true;
        }
        else if (e.Key == Key.Delete && e.KeyModifiers == KeyModifiers.None)
        {
            _scene.Delete(selected);
            e.Handled = true;
        }
    }
}