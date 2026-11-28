using Avalonia.Controls;
using XuanYu.Editor.Preview;
using XuanYu.Editor.UI.Top.Persistence;
using XuanYu.Editor.UI.Top.Simulation;

namespace XuanYu.Editor.UI;

internal sealed partial class TopToolRow
{
    private Button _createCube = null!, _createUnit = null!, _worldGrid = null!, _sky = null!;
    private Button _select = null!, _translate = null!, _rotate = null!, _scale = null!, _ground = null!;
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

    private void BuildWorld(Action createCube, Action? createUnit)
    {
        _createCube = Add(_world, "创建立方体", createCube);
        _createUnit = Add(_world, "创建单位", createUnit);
        _createUnit.IsVisible = createUnit is not null;
        _select = Add(_world, "选择", () => _wb.SetG07TranslateMode(false));
        _translate = Add(_world, "平移", () => _wb.SetG07TranslateMode(true));
        _rotate = Add(_world, "旋转", () => _wb.SetG07GizmoMode(GeoGizmoMode.Rotate));
        _scale = Add(_world, "缩放", () => _wb.SetG07GizmoMode(GeoGizmoMode.Scale));
        _ground = Add(_world, "贴地移动", _wb.ToggleG07GroundTranslation);
        AddUnavailable(_world, "编组");
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
        _createCube.Content = _wb.IsGeoTerrainPreview ? "创建全球立方体（200m）" : "创建立方体";
        _createCube.IsEnabled = _createUnit.IsEnabled = !_wb.IsSimulating;
        bool edit = !_wb.IsSimulating;
        bool geo = _wb.IsGeoTerrainPreview && edit;
        _select.IsEnabled = _translate.IsEnabled = _rotate.IsEnabled = _scale.IsEnabled = edit;
        _ground.IsEnabled = geo && _wb.G07TranslateMode;
        Paint(_select, edit && _wb.G07GizmoMode == GeoGizmoMode.Select);
        Paint(_translate, edit && _wb.G07TranslateMode);
        Paint(_rotate, edit && _wb.G07GizmoMode == GeoGizmoMode.Rotate);
        Paint(_scale, edit && _wb.G07GizmoMode == GeoGizmoMode.Scale);
        Paint(_ground, geo && _wb.G07TranslateMode && _wb.G07GroundTranslation);
        _openScene.IsEnabled = !_wb.IsSimulating;
        _saveScene.IsEnabled = !_wb.IsSimulating;
        _start.IsEnabled = !_wb.IsSimulating;
        _stop.IsEnabled = _wb.IsSimulating;
        Paint(_worldGrid, _wb.WorldGridEnabled);
        Paint(_sky, _wb.SkyEnabled);
    }
}