using XuanYu.World;

namespace XuanYu.Runtime;

public sealed class SimulationSession
{
    public const int TicksPerSecond = 30;
    private readonly WorldScene _source;

    public WorldScene? RunningWorld { get; private set; }
    public bool IsRunning => RunningWorld is not null;
    public long TickCount { get; private set; }
    public double ElapsedSeconds => (double)TickCount / TicksPerSecond;

    public SimulationSession(WorldScene source) => _source = source;

    public void Start()
    {
        if (IsRunning)
            throw new InvalidOperationException("模拟已经启动。");
        var snapshot = new WorldScene();
        snapshot.Replace(_source.Entities.ToArray());
        RunningWorld = snapshot;
        TickCount = 0;
    }

    public void Step()
    {
        if (!IsRunning)
            throw new InvalidOperationException("模拟尚未启动。");
        TickCount++;
    }

    public void Stop()
    {
        if (!IsRunning)
            throw new InvalidOperationException("模拟尚未启动。");
        RunningWorld = null;
    }
}