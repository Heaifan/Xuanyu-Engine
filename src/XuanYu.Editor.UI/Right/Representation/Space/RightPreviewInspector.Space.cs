using System.Globalization;
using Avalonia.Controls;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview;

namespace XuanYu.Editor.UI.Right;

public sealed partial class RightPreviewInspector
{
    private bool _globalViewport;
    private TextBlock? _spaceHint;

    internal void SetViewportSpace(bool global)
    {
        if (_globalViewport == global) return;
        _globalViewport = global;
        if (_scene.Selected is { } item) RefreshSpaceHint(item);
    }

    private void RefreshSpaceHint(PreviewObject item)
    {
        if (_spaceHint is null) return;
        _spaceHint.Text = item.IsGeographic == _globalViewport ? "实体属于当前视口空间。" :
            "实体属于另一坐标空间，当前视口不显示。";
    }

    private Control CreateSpaceGroup(PreviewObject item)
    {
        var group = CreateGroup("空间转换 · ENU");
        var body = RightRepresentationEditor.FormBody();
        var hint = new TextBlock();
        RightRepresentationEditor.StyleHint(hint);
        _spaceHint = hint;
        RefreshSpaceHint(item);
        body.Children.Add(hint);
        var latitude = new TextBox();
        var longitude = new TextBox();
        var height = new TextBox();
        var button = new Button { Content = item.IsGeographic ? "转换至本地" : "转换至全球" };
        var error = new TextBlock();
        var instruction = new TextBlock { Text = "本地 XYZ 对应东、北、上；请输入同一地理原点。" };
        RightRepresentationEditor.StyleHint(error);
        RightRepresentationEditor.StyleHint(instruction);
        body.Children.Add(instruction);
        body.Children.Add(RightRepresentationEditor.Field("纬度 (°)", latitude));
        body.Children.Add(RightRepresentationEditor.Field("经度 (°)", longitude));
        body.Children.Add(RightRepresentationEditor.Field("椭球高 (m)", height));
        body.Children.Add(RightRepresentationEditor.Field("", button));
        body.Children.Add(error);
        group.AddControl(body);
        button.Click += (_, _) =>
        {
            try
            {
                double Read(TextBox field) => double.Parse(field.Text ?? "", CultureInfo.InvariantCulture);
                var anchor = new GeoCoordinate(Read(latitude), Read(longitude), Read(height));
                _scene.ConvertUnitSpace(item.Id, anchor);
            }
            catch (Exception ex) { error.Text = ex.Message; }
        };
        return group;
    }
}