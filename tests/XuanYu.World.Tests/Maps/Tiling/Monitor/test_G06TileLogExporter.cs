using System.Text.Json;
using Xunit;
using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G06TileLogExporter
{
    [Fact]
    public async Task Export_ContainsRealFramesAndKeepsRecordingState()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        var session = new GeoTerrainRenderSession(hgt.Load());
        var history = session.MonitorHistory;
        history.SetRecording(true);
        session.Draws(800, 600);
        session.Draws(800, 600);
        var frames = Enumerable.Range(0, history.Count)
            .Select(i => history[i]).ToArray();
        using var stream = new MemoryStream();
        await GeoTileMonitorLogExporter.WriteAsync(stream, frames);
        stream.Position = 0;
        using var reader = new StreamReader(stream);
        string[] lines = (await reader.ReadToEndAsync())
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(3, lines.Length);
        using var header = JsonDocument.Parse(lines[0]);
        Assert.Equal("xye.tile-monitor.v1",
            header.RootElement.GetProperty("schema").GetString());
        Assert.Equal(2, header.RootElement.GetProperty("recordCount").GetInt32());
        using var first = JsonDocument.Parse(lines[1]);
        using var second = JsonDocument.Parse(lines[2]);
        Assert.Equal(frames[0].Snapshot.Frame,
            first.RootElement.GetProperty("snapshot").GetProperty("frame").GetInt64());
        Assert.True(first.RootElement.GetProperty("snapshot")
            .GetProperty("tiles").GetArrayLength() > 0);
        Assert.Equal(JsonValueKind.Null, first.RootElement.GetProperty("changes").ValueKind);
        Assert.Equal(JsonValueKind.Object, second.RootElement.GetProperty("changes").ValueKind);
        Assert.True(history.IsRecording);
        Assert.Equal(2, history.Count);
    }
}