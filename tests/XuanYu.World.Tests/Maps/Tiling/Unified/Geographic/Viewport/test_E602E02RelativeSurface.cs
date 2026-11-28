using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_E602E02RelativeSurface
{
    [Fact]
    public async Task RelativePickAndVisibilityUseSameDataset()
    {
        string folder = Path.Combine(Environment.CurrentDirectory, "artifacts", "tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            string path = Path.Combine(folder, "N23E120.hgt");
            using (var file = File.Create(path)) file.SetLength(NasademTile.ByteLength);
            var anchor = new EnuFrame(new GeoCoordinate(0, 0, 0));
            var geoid = Egm96GeoidGrid.Load(Environment.GetEnvironmentVariable("XYE_EGM96_GRID")!);
            using var cpu = await GeoUnifiedLodSession.CreateDatasetAsync(new NasademTileCatalog([path]),
                anchor, 512, geoid: geoid);
            var geo = new GeoCoordinate(23.5, 120.5, 0);
            var local = new EnuFrame(geo);
            var eye = anchor.ToEnu(local.ToEcef(new(0, 0, 2000)));
            var ground = anchor.ToEnu(local.OriginEcef);
            var ray = Vector3.Normalize(new Vector3((float)(ground.East - eye.East),
                (float)(ground.North - eye.North), (float)(ground.Up - eye.Up)));
            Assert.True(cpu.TryPickSurfaceHit(Vector3.Zero, ray, out var hit, eye));
            Assert.Equal(0f, hit.Elevation);
            var point = Wgs84.ToGeo(anchor.ToEcef(new EnuPosition(eye.East + hit.Position.X,
                eye.North + hit.Position.Y, eye.Up + hit.Position.Z)));
            Assert.InRange(Math.Abs(point.HeightMeters - geoid.UndulationMeters(geo)), 0, 2);
            var view = Matrix4x4.CreateLookAt(Vector3.Zero, ray, Vector3.UnitZ);
            var proj = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 3f, 1, 1, 500_000);
            proj.M22 = -proj.M22;
            var visible = cpu.CreateViewportVisibility(view * proj, eye);
            Assert.True(visible(hit.Tile));
        }
        finally { Directory.Delete(folder, true); }
    }
}