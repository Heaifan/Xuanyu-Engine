using Xunit;
using XuanYu.Editor.Pages;

namespace XuanYu.World.Tests;

public sealed class test_B01LogBox
{
    [Fact]
    public void Records_AreBounded_AndKeepLatestMessages()
    {
        var log = new LogBox();
        for (int i = 0; i < 2100; i++) log.Write($"entry-{i}");
        Assert.Equal(LogBox.Capacity, log.Records.Count);
        Assert.Equal(LogBox.Capacity, log.Items.Count);
        Assert.EndsWith("entry-100", log.Items[0]);
        Assert.EndsWith("entry-2099", log.Items[^1]);
        Assert.Equal(101, log.Records[0].Sequence);
    }

    [Fact]
    public void StreamsAndSeverities_AreExplicit()
    {
        var log = new LogBox();
        log.Write("普通消息");
        log.WriteOutput("转码完成", "XYTerrain");
        log.Warn("空间不足", "磁盘");
        log.Error("导入失败", "地图");
        log.WriteDebug("探针", "LOD");
        Assert.Equal(LogStream.Output, log.Records[1].Stream);
        Assert.Equal("XYTerrain", log.Records[1].Source);
        Assert.Equal(LogLevel.Warning, log.Records[2].Level);
        Assert.Equal(LogLevel.Error, log.Records[3].Level);
        Assert.Equal(LogStream.Debug, log.Records[4].Stream);
    }

    [Fact]
    public void LegacyWrite_StillRaisesOneEvent()
    {
        var log = new LogBox();
        int count = 0;
        log.Updated += () => count++;
        log.Write("hello");
        Assert.Equal(1, count);
        Assert.EndsWith("hello", log.Items[0]);
        Assert.Throws<ArgumentException>(() => log.Write(" "));
    }
}