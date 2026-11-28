using System.Text.Json.Nodes;
using Xunit;
using XuanYu.Editor.Preview;
using XuanYu.Editor.Preview.Persistence;

namespace XuanYu.World.Tests;

public sealed class test_E603BOrganizationLoad
{
    [Theory]
    [InlineData("missing")]
    [InlineData("self")]
    [InlineData("cycle")]
    [InlineData("legacy")]
    public void InvalidRelation_LeavesCurrentWorldAndDrawsUntouched(string kind)
    {
        string path = Path.Combine(AppContext.BaseDirectory,
            $"XYE-E603B02-Invalid-{Guid.NewGuid():N}.xyscene");
        try
        {
            var source = PreviewSeed.Create3D();
            Guid a = Assert.Single(source.Objects).Id;
            Guid b = source.CreateCube().Id;
            source.World.SetOrganizationParent(b, a);
            SceneFile.Save(source, path);
            var json = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
            var parts = json["Entities"]!.AsArray();
            var first = parts[0]!.AsObject();
            var second = parts[1]!.AsObject();
            switch (kind)
            {
                case "missing": second["OrganizationParentId"] = Guid.NewGuid().ToString(); break;
                case "self": second["OrganizationParentId"] = b.ToString(); break;
                case "cycle": first["OrganizationParentId"] = b.ToString(); break;
                case "legacy": json["Version"] = 1; break;
            }
            File.WriteAllText(path, json.ToJsonString());
            var target = PreviewSeed.Create3D();
            Guid oldParent = Assert.Single(target.Objects).Id;
            Guid oldChild = target.CreateCube().Id;
            target.World.SetOrganizationParent(oldChild, oldParent);
            target.Select(oldChild);
            var before = target.World.Entities.ToArray();
            var objects = target.Objects.ToArray();
            int drawCount = target.Draws.Count;
            Assert.Throws<InvalidDataException>(() => SceneFile.Load(target, path));
            Assert.Equal(before, target.World.Entities.ToArray());
            Assert.Equal(objects, target.Objects.ToArray());
            Assert.Equal(oldChild, target.SelectedId);
            Assert.Equal(oldParent, target.World.GetOrganizationParent(oldChild));
            Assert.Equal(drawCount, target.Draws.Count);
        }
        finally { File.Delete(path); }
    }
}