using Avalonia.Input;

namespace XuanYu.Editor.UI.Right;

internal sealed partial class RightNumberField
{
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(_surface).Properties.IsLeftButtonPressed)
            return;
        _pressed = true;
        _dragged = false;
        _startX = e.GetPosition(_surface).X;
        _startValue = _value;
        e.Pointer.Capture(_surface);
        e.Handled = true;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_pressed)
            return;
        float pixels = (float)(e.GetPosition(_surface).X - _startX);
        if (!_dragged && MathF.Abs(pixels) < 4f)
            return;
        _dragged = true;
        float multiplier = (e.KeyModifiers & KeyModifiers.Shift) != 0
            ? 10f : 1f;
        Apply(TransformNumber.Scrub(_startValue,
            _step * multiplier, pixels));
        e.Handled = true;
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_pressed)
            return;
        bool wasDrag = _dragged;
        _pressed = false;
        if (e.Pointer.Captured == _surface)
            e.Pointer.Capture(null);
        if (!wasDrag)
            BeginEdit();
        e.Handled = true;
    }
}