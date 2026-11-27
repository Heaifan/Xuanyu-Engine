using Avalonia.Controls;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI;

internal static class ViewportEnvironmentViewMenu
{
    public static MenuItem Create(Workbench wb)
    {
        MenuItem sky = Check("天空环境", wb.SkyEnabled, wb.SetSkyEnabled);
        MenuItem world = Check("世界：米制网格", wb.WorldGridEnabled,
            wb.SetWorldGridEnabled);
        MenuItem terrain = Check("地形：地理方格网", wb.TerrainGridEnabled,
            wb.SetTerrainGridEnabled);
        wb.Changed += () =>
        {
            sky.IsChecked = wb.SkyEnabled;
            world.IsChecked = wb.WorldGridEnabled;
            terrain.IsChecked = wb.TerrainGridEnabled;
        };
        return new MenuItem
        {
            Header = "视口环境",
            ItemsSource = new[] { sky, world, terrain }
        };
    }

    private static MenuItem Check(string title, bool enabled, Action<bool> set)
    {
        var item = new MenuItem
        {
            Header = title,
            ToggleType = MenuItemToggleType.CheckBox,
            IsChecked = enabled
        };
        item.PropertyChanged += (_, e) =>
        {
            if (e.Property == MenuItem.IsCheckedProperty)
                set(item.IsChecked);
        };
        return item;
    }
}