using Avalonia.Controls;
using Avalonia.Threading;
using XuanYu.Editor.Pages;
using XuanYu.Runtime;

namespace XuanYu.Editor.UI.Top.Simulation;

internal static class SimulationMenu
{
    public static MenuItem[] Create(Window owner, Workbench wb)
    {
        var start = new MenuItem { Header = "开始模拟" };
        var stop = new MenuItem { Header = "停止模拟" };
        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(
                1d / SimulationSession.TicksPerSecond)
        };
        timer.Tick += (_, _) => wb.AdvanceSimulationTick();
        start.Click += (_, _) =>
        {
            wb.StartSimulation();
            timer.Start();
        };
        stop.Click += (_, _) =>
        {
            timer.Stop();
            wb.StopSimulation();
        };
        owner.Closed += (_, _) => timer.Stop();

        void Refresh()
        {
            start.IsEnabled = !wb.IsSimulating;
            stop.IsEnabled = wb.IsSimulating;
        }
        wb.Changed += Refresh;
        Refresh();
        return [start, stop];
    }
}