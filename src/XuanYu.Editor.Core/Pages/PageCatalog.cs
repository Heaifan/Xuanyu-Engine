namespace XuanYu.Editor.Pages;

/// <summary>
/// 五个工作区页面的只读信息表。
/// </summary>
public static class PageCatalog
{
    private static readonly IReadOnlyList<PageInfo> s_all =
    [
        new PageInfo(
            PageId.Project,
            "1. 项目 (Project)",
            "项目资产",
            "管理工程目录、terrain、materials、models 资源。",
            "资产属性",
            "查看选中资产的路径、导入参数与依赖项。"),

        new PageInfo(
            PageId.World,
            "2. 世界 (World)",
            "场景对象",
            "管理 World 拥有的实体列表与空间层级。",
            "实体属性",
            "编辑选中实体的米制空间坐标、旋转与组件。"),

        new PageInfo(
            PageId.Terrain,
            "3. 地形 (Terrain)",
            "地形数据",
            "管理 DEM 导入、地理坐标、分块、LOD 与图层。",
            "地形参数",
            "配置米制高程范围、加载阈值与垂直夸张显示。"),

        new PageInfo(
            PageId.Simulation,
            "4. 模拟 (Simulation)",
            "模拟控制",
            "管理 30Hz 步长推进、路径移动、物理与导航。",
            "模拟状态",
            "监控当前帧步数、单位速度与地面贴合高度。"),

        new PageInfo(
            PageId.Tools,
            "5. 工具 (Tools)",
            "引擎设置",
            "管理 Vulkan 物理设备、Swapchain 与渲染配置。",
            "诊断详情",
            "查看 GPU 状态、同步原语与平台运行参数。")
    ];

    public static IReadOnlyList<PageInfo> All => s_all;

    public static PageInfo Get(PageId id)
    {
        if (!Enum.IsDefined(id))
        {
            throw new ArgumentOutOfRangeException(nameof(id), id, "无效的页面标识。");
        }

        return s_all[(int)id];
    }
}