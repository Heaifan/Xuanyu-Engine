using Avalonia.Controls;
using Avalonia.Input;
using XuanYu.Editor.Camera;
using Avalonia.Threading;
using XuanYu.Editor.Pages;
using XuanYu.Runtime;

namespace XuanYu.Editor.UI.Top.Simulation;

internal sealed class SimulationMenu
{
    private readonly Workbench _wb;
    private readonly DispatcherTimer _timer;

    public SimulationMenu(Window owner, Workbench wb)
    {
        _wb = wb;
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1d / SimulationSession.TicksPerSecond)
        };
        _timer.Tick += (_, _) => wb.AdvanceSimulationTick();
        owner.Closed += (_, _) => _timer.Stop();
        owner.KeyDown += (_, e) => RouteCameraKey(e, true);
        owner.KeyUp += (_, e) => RouteCameraKey(e, false);
        owner.Deactivated += (_, _) => wb.SetGameCameraKey(GameCameraKey.None, false);
    }

    private void RouteCameraKey(KeyEventArgs e, bool down)
    {
        if (!_wb.IsSimulating || !_wb.CanNavigateRuntimeCamera || e.Source is TextBox ||
            (down && e.KeyModifiers != KeyModifiers.None)) return;
        var key = ViewportSharedHost.MapGameKey(e.Key);
        if (key == GameCameraKey.None) return;
        _wb.SetGameCameraKey(key, down);
        e.Handled = true;
    }

    public void Start()
    {
        _wb.StartSimulation();
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
        _wb.StopSimulation();
    }

    public MenuItem[] Create()
    {
        var start = new MenuItem { Header = "开始模拟" };
        var stop = new MenuItem { Header = "停止模拟" };
        start.Click += (_, _) => Start();
        stop.Click += (_, _) => Stop();
        void Refresh()
        {
            start.IsEnabled = !_wb.IsSimulating;
            stop.IsEnabled = _wb.IsSimulating;
        }
        _wb.Changed += Refresh;
        Refresh();
        return [start, stop];
    }
}