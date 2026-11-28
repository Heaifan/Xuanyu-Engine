using XuanYu.Editor.Camera;
using XuanYu.Runtime;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    private ViewportCamera? _runtimeCamera;
    private GameCameraKey _heldCameraKeys;

    public bool CanNavigateRuntimeCamera => CurrentId is PageId.World or PageId.Simulation;

    public ViewportCamera? ActiveCamera => _geoPreview is not null
        ? null
        : IsSimulating ? _runtimeCamera ?? Scene.Camera
        : CurrentId == PageId.Terrain ? TerrainScene.Camera : Scene.Camera;

    public void StartSimulation()
    {
        ViewportCamera? editorCamera = ActiveCamera;
        Simulation.Start();
        _runtimeCamera = editorCamera?.Copy();
        _geoPreview?.BeginRuntimeCamera();
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
        _geoPreview?.EndRuntimeCamera();
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
        if (!IsSimulating || !CanNavigateRuntimeCamera) return;
        if (down) _heldCameraKeys |= key;
        else _heldCameraKeys &= ~key;
    }

    private void ResetCameraKeys() => _heldCameraKeys = GameCameraKey.None;

    private void TickRuntimeCamera()
    {
        if (!CanNavigateRuntimeCamera) return;
        int forward = (Has(GameCameraKey.Forward) ? 1 : 0) -
            (Has(GameCameraKey.Backward) ? 1 : 0);
        int right = (Has(GameCameraKey.Right) ? 1 : 0) -
            (Has(GameCameraKey.Left) ? 1 : 0);
        if (forward == 0 && right == 0) return;
        if (_geoPreview is { } geo)
        {
            double meters = Math.Max(4d, geo.Camera.DistanceMeters * 0.25d);
            double delta = meters / SimulationSession.TicksPerSecond;
            if (forward != 0 && right != 0) delta /= Math.Sqrt(2d);
            geo.Camera.MoveGround(forward * delta, right * delta);
            return;
        }
        if (_runtimeCamera is null) return;
        double step = 4d / SimulationSession.TicksPerSecond;
        if (forward != 0 && right != 0) step /= Math.Sqrt(2d);
        _runtimeCamera.MoveGround(forward * step, right * step);
    }

    private bool Has(GameCameraKey key) =>
        (_heldCameraKeys & key) != 0;

    private WorldRenderBridge CreateRuntimeBridge() =>
        Scene.CreateRuntimeBridge(Simulation.RunningWorld!);
}