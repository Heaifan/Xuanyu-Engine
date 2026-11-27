using Avalonia.Controls;
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