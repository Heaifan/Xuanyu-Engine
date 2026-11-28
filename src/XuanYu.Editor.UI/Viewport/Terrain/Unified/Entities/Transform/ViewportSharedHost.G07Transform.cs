using Avalonia.Input;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview;
using XuanYu.World.Maps;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportSharedHost
{
    internal event Action? G07TransformCommitted;

    private bool TryHandleG07TransformKey(KeyEventArgs e)
    {
        if (_gizmoDrag is not null)
        {
            if (e.Key == Key.Escape) FinishG07Drag(false);
            else if (e.Key == Key.Enter) FinishG07Drag(true);
            return true;
        }
        if (_wb.IsSimulating || _wb.CurrentId != PageId.World || !G07EntitiesReady ||
            e.KeyModifiers != KeyModifiers.None || e.Key is not (Key.G or Key.R or Key.S or Key.B) ||
            _wb.Scene.Selected is not { IsGeographic: true, Visible: true } item) return false;
        if (e.Key == Key.B) GroundSelectedG07Entity(item.Id);
        else _wb.SetG07GizmoMode(e.Key switch
        {
            Key.R => GeoGizmoMode.Rotate, Key.S => GeoGizmoMode.Scale, _ => GeoGizmoMode.Translate
        });
        return true;
    }

    private void GroundSelectedG07Entity(Guid id)
    {
        var status = _wb.GroundG07Entity(_g07Cpu!, id);
        if (status == GeoSampleStatus.Valid)
        {
            _wb.Log.Write("[E6/全球实体] B · 底部贴地已应用");
            G07TransformCommitted?.Invoke();
        }
        else _wb.Log.Warn($"[E6/全球实体] 贴地失败：{status}，位置未修改。");
    }
}