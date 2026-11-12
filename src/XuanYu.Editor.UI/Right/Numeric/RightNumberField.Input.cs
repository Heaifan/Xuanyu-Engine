using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace XuanYu.Editor.UI.Right;

internal sealed partial class RightNumberField
{
    private void WireInput()
    {
        _surface.PointerPressed += OnPointerPressed;
        _surface.PointerMoved += OnPointerMoved;
        _surface.PointerReleased += OnPointerReleased;
        _surface.PointerCaptureLost += (_, _) => _pressed = false;
        _edit.GotFocus += (_, _) => _edit.SelectAll();
        _edit.LostFocus += (_, _) => FinishEdit();
        _edit.KeyDown += OnKeyDown;
        _edit.TextChanged += (_, _) => ClearError();
    }

    private void BeginEdit()
    {
        _editing = true;
        _edit.Text = TransformNumber.Display(_value);
        _surface.IsVisible = false;
        _edit.IsVisible = true;
        _edit.Focus();
        _edit.SelectAll();
    }

    private void FinishEdit()
    {
        if (!_editing)
            return;
        if (TransformNumber.TryParse(_edit.Text, out float parsed))
            Apply(parsed);
        _editing = false;
        _edit.IsVisible = false;
        _surface.IsVisible = true;
        ClearError();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            _edit.Text = TransformNumber.Display(_value);
            FinishEdit();
        }
        else if (e.Key == Key.Enter)
        {
            if (TransformNumber.TryParse(_edit.Text, out _))
                FinishEdit();
            else
            {
                _edit.BorderBrush = Brushes.IndianRed;
                ToolTip.SetTip(_edit, "请输入有效数字，例如 -2.5");
            }
        }
        else if (e.Key is Key.Up or Key.Down)
        {
            float direction = e.Key == Key.Up ? 1f : -1f;
            if ((e.KeyModifiers & KeyModifiers.Shift) != 0)
                direction *= 10;
            float start = TransformNumber.TryParse(_edit.Text, out float typed)
                ? typed : _value;
            Apply(TransformNumber.Step(start, _step, direction));
            _edit.SelectAll();
        }
        else
            return;
        e.Handled = true;
    }

    private void Apply(float next)
    {
        if (!float.IsFinite(next))
            return;
        _display.Text = TransformNumber.Display(next);
        _edit.Text = _display.Text;
        if (_value == next)
            return;
        _value = next;
        _changed(next);
    }

    private void ClearError()
    {
        _edit.BorderBrush = EditorTheme.Accent;
        ToolTip.SetTip(_edit, null);
    }
}