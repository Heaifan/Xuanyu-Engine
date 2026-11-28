using Xunit;
using XuanYu.Editor.Pages;

namespace XuanYu.World.Tests;

public sealed class test_WorkbenchEvents
{
    [Fact]
    public void ChangingPage_ShouldNotifyOnceAndAppendLog()
    {
        var workbench = new Workbench(PageId.Project);
        int notifications = 0;
        workbench.Changed += () => notifications++;

        workbench.SwitchTo(PageId.Terrain);

        Assert.Equal(1, notifications);
        Assert.Equal(PageId.Terrain, workbench.CurrentId);
        Assert.Equal(2, workbench.Log.Items.Count);
        Assert.Contains("切换页面", workbench.Log.Items[1]);
    }

    [Fact]
    public void SwitchingToCurrentPage_ShouldDoNothing()
    {
        var workbench = new Workbench(PageId.World);
        int notifications = 0;
        workbench.Changed += () => notifications++;

        workbench.SwitchTo(PageId.World);

        Assert.Equal(0, notifications);
        Assert.Single(workbench.Log.Items);
    }

    [Fact]
    public void WritingLog_ShouldNotifyOnce()
    {
        var log = new LogBox();
        int notifications = 0;
        log.Updated += () => notifications++;

        log.Write("  验证日志  ");

        Assert.Equal(1, notifications);
        Assert.Single(log.Items);
        Assert.EndsWith("验证日志", log.Items[0]);
    }
}