using System.Numerics;
using Xunit;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G02FPointerZoom
{
    [Fact]
    public void ZoomAtTerrain_KeepsPointUnderCursor()
    {
        using var hgt = new G02BHgtFixture("N23E120");
        var session = new GeoTerrainRenderSession(hgt.Load());
        var pixel = new Vector2(650f, 390f);
        const uint width = 1000, height = 700;
        Vector3 point = default;
        bool found = false;
        for (int i = 0; i < 96 && !found; i++)
        {
            var ready = session.Draws(width, height);
            found = ready.Count > 0 && GeoTerrainCursorPicker.TryPick(
                ready[0].ModelToClip, pixel, width, height,
                ready.Select(draw => draw.Mesh), out point);
        }
        Assert.True(found);
        double beforeDistance = session.Camera.DistanceMeters;
        session.ZoomAtCursor(1f, pixel, width, height);
        var draws = session.Draws(width, height);
        Assert.NotEmpty(draws);
        Assert.True(session.Camera.DistanceMeters < beforeDistance);

        Vector4 clip = Vector4.Transform(new Vector4(point, 1f),
            draws[0].ModelToClip);
        Assert.True(clip.W > 0);
        float actualX = (clip.X / clip.W + 1f) * width * 0.5f;
        float actualY = (clip.Y / clip.W + 1f) * height * 0.5f;
        Assert.InRange(MathF.Abs(pixel.X - actualX), 0f, 5f);
        Assert.InRange(MathF.Abs(pixel.Y - actualY), 0f, 5f);
    }

    [Fact]
    public void ZoomBeforeFirstDraw_FallsBackToCenterZoom()
    {
        using var hgt = new G02BHgtFixture("N23E120");
        var session = new GeoTerrainRenderSession(hgt.Load());
        var beforeTarget = session.Camera.Target;
        double beforeDistance = session.Camera.DistanceMeters;
        session.ZoomAtCursor(1f, new Vector2(500, 350), 1000, 700);
        Assert.True(session.Camera.DistanceMeters < beforeDistance);
        var actualTarget = session.Camera.Target;
        Assert.InRange(Math.Abs(actualTarget.LatitudeDegrees - beforeTarget.LatitudeDegrees),
            0d, 1e-10d);
        Assert.InRange(Math.Abs(actualTarget.LongitudeDegrees - beforeTarget.LongitudeDegrees),
            0d, 1e-10d);
        Assert.InRange(Math.Abs(actualTarget.HeightMeters - beforeTarget.HeightMeters),
            0d, 1e-6d);
    }
}