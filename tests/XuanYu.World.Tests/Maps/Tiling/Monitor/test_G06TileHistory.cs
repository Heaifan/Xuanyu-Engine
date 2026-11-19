using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G06TileHistory
{
    [Fact]
    public void Recording_OnlyCapturesActualDrawFrames()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        var session = new GeoTerrainRenderSession(hgt.Load());
        session.ToggleNative();
        var history = session.MonitorHistory;
        session.Draws(800, 600);
        Assert.Equal(0, history.Count);
        history.SetRecording(true);
        session.Draws(800, 600);
        session.Draws(800, 600);
        Assert.Equal(2, history.Count);
        Assert.Equal(history[0].Snapshot.Frame + 1, history[1].Snapshot.Frame);
        Assert.Equal(session.VisibleTileIds.Count, history[1].Snapshot.VisibleCount);
        history.SetRecording(false);
        session.Draws(800, 600);
        Assert.Equal(2, history.Count);
        history.SetRecording(true);
        session.Draws(800, 600);
        Assert.Equal(3, history.Count);
        Assert.Equal(history[1].Snapshot.Frame + 2, history[2].Snapshot.Frame);
    }

    [Fact]
    public void PausedFrame_RemainsUnchangedWhileCameraMoves()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        var session = new GeoTerrainRenderSession(hgt.Load());
        var history = session.MonitorHistory;
        history.SetRecording(true);
        session.Draws(800, 600);
        var saved = history[0];
        history.SetRecording(false);
        session.Camera.FrameTop(new GeoCoordinate(24.5d, 121.5d, 0), 1000d);
        session.Draws(800, 600);
        Assert.Same(saved, history[0]);
        Assert.NotEqual(saved.Snapshot.Frame, session.ReadMonitor().Frame);
        Assert.Equal(saved.Snapshot.VisibleCount, saved.Snapshot.Tiles.Count(t => t.Drawn));
        history.Clear();
        Assert.Equal(0, history.Count);
    }
}