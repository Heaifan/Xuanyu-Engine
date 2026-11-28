using Xunit;
using XuanYu.Editor.Preview;

namespace XuanYu.World.Tests;

public sealed class test_E603BOrganizationOptions
{
    [Fact]
    public void OptionsExcludeSelfAndAllDescendants()
    {
        var scene = new PreviewScene();
        var root = scene.CreateCube();
        var child = scene.CreateCube();
        var leaf = scene.CreateCube();
        var other = scene.CreateCube();
        scene.SetOrganizationParent(child.Id, root.Id);
        scene.SetOrganizationParent(leaf.Id, child.Id);
        var info = scene.GetOrganizationEditingInfo(root.Id);
        Assert.Equal(new Guid?[] { null, other.Id }, info.Parents.Select(option => option.Id));
        Assert.Null(info.ParentId);
        Assert.Equal(0, info.Depth);
        Assert.Equal(1, info.DirectChildren);
        Assert.Equal(2, info.Descendants);
    }

    [Fact]
    public void ReparentUpdatesDepthAndCountsWithoutChangingTransform()
    {
        var scene = new PreviewScene();
        var a = scene.CreateCube();
        var b = scene.CreateCube();
        var c = scene.CreateCube();
        scene.SetOrganizationParent(b.Id, a.Id);
        scene.SetOrganizationParent(c.Id, b.Id);
        var before = scene.World.Find(c.Id)!.Transform;
        var info = scene.GetOrganizationEditingInfo(c.Id);
        Assert.Equal(b.Id, info.ParentId);
        Assert.Equal(2, info.Depth);
        Assert.Contains(info.Parents, option => option.Id == b.Id);
        scene.SetOrganizationParent(c.Id, a.Id);
        Assert.Equal(1, scene.GetOrganizationEditingInfo(c.Id).Depth);
        Assert.Equal(2, scene.GetOrganizationEditingInfo(a.Id).DirectChildren);
        Assert.Equal(0, scene.GetOrganizationEditingInfo(b.Id).Descendants);
        Assert.Equal(before, scene.World.Find(c.Id)!.Transform);
        Assert.Equal(c.Id, scene.SelectedId);
    }

    [Fact]
    public void CandidateNamesAndParentReflectSceneChanges()
    {
        var scene = new PreviewScene();
        var a = scene.CreateCube();
        var b = scene.CreateCube();
        scene.SetOrganizationParent(b.Id, a.Id);
        scene.Rename(a.Id, "Upper");
        var c = scene.CreateCube();
        var info = scene.GetOrganizationEditingInfo(b.Id);
        Assert.Contains(info.Parents, option => option.Id == a.Id && option.Name == "Upper");
        Assert.Contains(info.Parents, option => option.Id == c.Id);
        Assert.True(scene.Delete(a.Id));
        info = scene.GetOrganizationEditingInfo(b.Id);
        Assert.Null(info.ParentId);
        Assert.Equal(0, info.Depth);
        Assert.DoesNotContain(info.Parents, option => option.Id == a.Id);
    }
}