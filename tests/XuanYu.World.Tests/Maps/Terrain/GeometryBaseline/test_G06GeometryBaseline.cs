using Xunit;
using Xunit.Abstractions;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class G06GeometryFactAttribute : FactAttribute
{
    public G06GeometryFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("XYE_G06_R04_HGT")))
            Skip = "需要显式指定真实 HGT：XYE_G06_R04_HGT";
    }
}

public sealed class test_G06GeometryBaseline(ITestOutputHelper output)
{
    [G06GeometryFact]
    public void RealHgt_ReportsVisibleMeshErrors()
    {
        string path = Environment.GetEnvironmentVariable("XYE_G06_R04_HGT")!;
        NasademTile raw = NasademHgtFile.Load(path);
        Assert.Equal(23, raw.SouthLatitude);
        Assert.Equal(120, raw.WestLongitude);
        var catalog = new NasademTileCatalog([path]);
        var session = new GeoTerrainRenderSession(catalog, 16, useOverview: true);
        var anchor = new EnuFrame(new GeoCoordinate(23.5, 120.5, 0));
        GeoTileId fine = GeographicTileScheme.Find(new GeoCoordinate(23.5, 120.85, 0), 13);
        GeoTileBounds region = GeographicTileScheme.Bounds(fine);
        var focus = new GeoCoordinate((region.SouthLatitudeDegrees + region.NorthLatitudeDegrees) / 2,
            (region.WestLongitudeDegrees + region.EastLongitudeDegrees) / 2, 0);
        var coarse = new Dictionary<int, RenderDraw>();
        session.ToggleNative();
        for (double distance = 200_000; distance >= 12_000; distance *= 0.85)
        {
            SetView(session, focus, distance);
            var draws = SettledDraws(session);
            for (int level = 10; level <= 13; level++)
            {
                if (coarse.ContainsKey(level)) continue;
                GeoTileId id = GeographicTileScheme.Find(focus, level);
                foreach (var draw in draws.Where(d => d.Mesh.AssetId == $"terrain/g02/{id}"))
                    coarse[level] = draw;
            }
            if (coarse.Count == 4) break;
        }
        SetView(session, focus, 5_000);
        var near = SettledDraws(session).Where(d => d.Mesh.AssetId == $"terrain/g02/{fine}").ToArray();
        var missing = Enumerable.Range(10, 3).Where(level => !coarse.ContainsKey(level)).ToArray();
        Assert.True(missing.Length == 0, $"未取得真实普通 LOD: {string.Join(",", missing)}");
        Assert.Single(near);
        coarse[13] = near[0];
        session.ToggleNative();
        var native = SettledDraws(session, fine).Where(d => IsNative(d, fine)).ToArray();
        Assert.Single(native);
        var samples = G06GeometryMeasure.Samples(raw, anchor, focus);
        output.WriteLine($"[G06/R04A] HGT={raw.TileId} ID={fine} 样本候选={samples.Count} " +
            $"视口=1280x720 统一投影=普通L13实拍矩阵 pitch={session.Camera.PitchRadians:F3}");
        foreach (var (level, draw) in coarse.OrderBy(p => p.Key))
            output.WriteLine(G06GeometryMeasure.Report($"L{level}", draw, near[0].ModelToClip, samples));
        output.WriteLine(G06GeometryMeasure.Report("Native", native[0], near[0].ModelToClip, samples));
        G06GeometryCompare.Run(output, catalog, raw, anchor, fine, near[0], samples);
    }

    private static void SetView(GeoTerrainRenderSession session, GeoCoordinate focus, double distance)
    {
        session.Camera.FrameTop(focus, distance);
        session.Camera.Orbit(0, -0.55);
    }

    private static IReadOnlyList<RenderDraw> SettledDraws(GeoTerrainRenderSession session, GeoTileId? native = null)
    {
        IReadOnlyList<RenderDraw> draws = [];
        for (int i = 0; i < 160; i++)
        {
            draws = session.Draws(1280, 720);
            if (session.PendingCount == 0 && (native is not { } id ||
                (draws.Any(d => IsNative(d, id)) && session.NativeDrawCount == Math.Min(12, session.VisibleTileIds.Count(t => t.Level == 13))))) return draws;
        }
        Assert.True(session.PendingCount == 0, "选片未稳定，不将残缺帧写入误差基线。");
        return draws;
    }

    private static bool IsNative(RenderDraw draw, GeoTileId id) =>
        draw.Mesh.AssetId.StartsWith("terrain/g04/", StringComparison.Ordinal) &&
        draw.Mesh.AssetId.Contains($"/{id}/", StringComparison.Ordinal);
}