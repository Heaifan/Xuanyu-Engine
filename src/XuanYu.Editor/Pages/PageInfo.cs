namespace XuanYu.Editor.Pages;

/// <summary>
/// 单个工作区页面的界面显示文本信息。
/// </summary>
public sealed record PageInfo(
    PageId Id,
    string Title,
    string LeftTitle,
    string LeftDetail,
    string RightTitle,
    string RightDetail);