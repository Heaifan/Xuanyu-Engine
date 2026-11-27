using Avalonia.Controls;
using XuanYu.Editor.UI.Top.Persistence;
using XuanYu.Editor.UI.Top.Simulation;

namespace XuanYu.Editor.UI;

internal sealed partial class TopToolRow
{
    private Button _createCube = null!, _worldGrid = null!, _sky = null!;
    private Button _start = null!, _stop = null!;
    private Button _openScene = null!, _saveScene = null!;

    private void BuildProject(SceneFileCommands scene)
    {
        AddUnavailable(_project, "新建", "打开", "保存", "导入资源", "资源管理");
        Divide(_project);
        _openScene = Add(_project, "打开场景");
        _openScene.Click += async (_, _) => await scene.OpenAsync();
        _saveScene = Add(_project, "保存场景");
        _saveScene.Click += async (_, _) => await scene.SaveAsync();
    }

    private void BuildWorld()
    {
        _createCube = Add(_world, "创建立方体", () => _wb.Scene.CreateCube());
        AddUnavailable(_world, "选择", "变换", "编组");
        Divide(_world);
        _worldGrid = Add(_world, "网格", () => _wb.SetWorldGridEnabled(!_wb.WorldGridEnabled));
        _sky = Add(_world, "天空", () => _wb.SetSkyEnabled(!_wb.SkyEnabled));
    }

    private void BuildSimulation(SimulationMenu simulation)
    {
        AddUnavailable(_simulation, "初始化");
        _start = Add(_simulation, "开始", simulation.Start);
        _stop = Add(_simulation, "停止", simulation.Stop);
        Divide(_simulation);
        AddUnavailable(_simulation, "实体", "事件", "记录", "回放");
    }

    private void BuildTools() =>
        AddUnavailable(_tools, "XYLab", "渲染", "性能", "诊断", "设置");

    private void RefreshWorkspaces()
    {
        _createCube.IsEnabled = !_wb.IsSimulating;
        _openScene.IsEnabled = !_wb.IsSimulating;
        _saveScene.IsEnabled = !_wb.IsSimulating;
        _start.IsEnabled = !_wb.IsSimulating;
        _stop.IsEnabled = _wb.IsSimulating;
        Paint(_worldGrid, _wb.WorldGridEnabled);
        Paint(_sky, _wb.SkyEnabled);
    }
}