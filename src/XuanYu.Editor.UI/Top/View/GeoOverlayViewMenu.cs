using Avalonia.Controls;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI;

internal static class GeoOverlayViewMenu
{
    public static MenuItem Create(Workbench wb)
    {
        var hud = new MenuItem
        {
            Header = "显示地理 HUD", ToggleType = MenuItemToggleType.CheckBox,
            IsChecked = wb.GeoHudVisible
        };
        hud.PropertyChanged += (_, e) =>
        {
            if (e.Property == MenuItem.IsCheckedProperty)
                wb.SetGeoHudVisible(hud.IsChecked);
        };
        var miniMap = new MenuItem
        {
            Header = "显示小地图", ToggleType = MenuItemToggleType.CheckBox,
            IsChecked = wb.GeoMiniMapVisible
        };
        miniMap.PropertyChanged += (_, e) =>
        {
            if (e.Property == MenuItem.IsCheckedProperty)
                wb.SetGeoMiniMapVisible(miniMap.IsChecked);
        };
        var sizes = new MenuItem { Header = "小地图尺寸" };
        var choices = new[] { 140, 180, 240 }.Select(size =>
        {
            var choice = new MenuItem { Header = $"{size} × {size} DIP" };
            choice.Click += (_, _) => wb.SetGeoMiniMapSize(size);
            return choice;
        }).ToArray();
        sizes.ItemsSource = choices;
        wb.Changed += () =>
        {
            hud.IsChecked = wb.GeoHudVisible;
            miniMap.IsChecked = wb.GeoMiniMapVisible;
            sizes.Header = $"小地图尺寸（{wb.GeoMiniMapSize} DIP）";
        };
        sizes.Header = $"小地图尺寸（{wb.GeoMiniMapSize} DIP）";
        return new MenuItem
        {
            Header = "地理叠层",
            ItemsSource = new[] { hud, miniMap, sizes }
        };
    }
}