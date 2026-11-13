using XuanYu.Editor.Camera;
using XuanYu.Editor.Preview;
using XuanYu.Runtime;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    private ViewportCamera? _runtimeCamera;
    private GameCameraKey _heldCameraKeys;
    public ViewportCamera? ActiveCamera => _runtimeCamera ?? Scene.Camera;

    public void StartSimulation()
    {
        Simulation.Start();
        _runtimeCamera = Scene.Camera?.Copy();
        ResetCameraKeys();
        _runtimeBridge = CreateRuntimeBridge();
        Log.Write("模拟开始：已建立独立 World 副本，目标 30 Hz");
        Changed?.Invoke();
    }

    public void AdvanceSimulationTick()
    {
        Simulation.Step();
        TickRuntimeCamera();
        if (Simulation.TickCount % SimulationSession.TicksPerSecond == 0)
            Changed?.Invoke();
    }

    public void StopSimulation()
    {
        double seconds = Simulation.ElapsedSeconds;
        Simulation.Stop();
        _runtimeBridge = null;
        _runtimeCamera = null;
        ResetCameraKeys();
        Log.Write($"模拟停止：累计 {seconds:0.0} 秒，编辑场景未被替换");
        Changed?.Invoke();
    }

    public void SetGameCameraKey(GameCameraKey key, bool down)
    {
        if (key == GameCameraKey.None)
        {
            ResetCameraKeys();
            return;
        }
        if (!IsSimulating || CurrentId != PageId.World)
            return;
        if (down) _heldCameraKeys |= key;
        else _heldCameraKeys &= ~key;
    }

    private void ResetCameraKeys() => _heldCameraKeys = GameCameraKey.None;

    private void TickRuntimeCamera()
    {
        if (_runtimeCamera is null || CurrentId != PageId.World)
            return;
        int forward = (Has(GameCameraKey.Forward) ? 1 : 0) -
            (Has(GameCameraKey.Backward) ? 1 : 0);
        int right = (Has(GameCameraKey.Right) ? 1 : 0) -
            (Has(GameCameraKey.Left) ? 1 : 0);
        if (forward == 0 && right == 0)
            return;
        float step = 4f / SimulationSession.TicksPerSecond;
        if (forward != 0 && right != 0)
            step /= MathF.Sqrt(2f);
        _runtimeCamera.MoveGround(forward * step, right * step);
    }

    private bool Has(GameCameraKey key) => (_heldCameraKeys & key) != 0;

    private WorldRenderBridge CreateRuntimeBridge()
    {
        var bridge = new WorldRenderBridge(Simulation.RunningWorld!);
        foreach (PreviewObject item in Scene.Objects)
            bridge.Bind(item.Id, item.Mesh);
        return bridge;
    }
}