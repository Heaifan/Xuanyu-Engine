using Xunit;
using XuanYu.Editor.Preview;
using XuanYu.Editor.Preview.Persistence;

namespace XuanYu.World.Tests;

public sealed class test_E603BOrganizationEditing
{
    [Fact]
    public void ReparentAndDetach_NotifyOnlyOnChanges()
    {
        var scene = new PreviewScene();
        var a = scene.CreateCube();
        var b = scene.CreateCube();
        var c = scene.CreateCube();
        var original = scene.World.Find(c.Id)!.Transform;
        int structure = 0, changed = 0;
        scene.OrganizationChanged += () => structure++;
        scene.Changed += () => changed++;
        Assert.True(scene.SetOrganizationParent(c.Id, a.Id));
        Assert.False(scene.SetOrganizationParent(c.Id, a.Id));
        Assert.True(scene.SetOrganizationParent(c.Id, b.Id));
        Assert.Equal(b.Id, scene.World.GetOrganizationParent(c.Id));
        Assert.Empty(scene.World.GetOrganizationChildren(a.Id));
        Assert.True(scene.SetOrganizationParent(c.Id, null));
        Assert.Equal(3, structure);
        Assert.Equal(3, changed);
        Assert.Equal(original, scene.World.Find(c.Id)!.Transform);
        Assert.Equal(c.Id, scene.SelectedId);
        Assert.Equal(3, scene.Draws.Count);
    }

    [Fact]
    public void CycleRejected_LeavesSelectionAndNotificationsUnchanged()
    {
        var scene = new PreviewScene();
        var a = scene.CreateCube();
        var b = scene.CreateCube();
        scene.SetOrganizationParent(b.Id, a.Id);
        int notifications = 0;
        scene.OrganizationChanged += () => notifications++;
        scene.Changed += () => notifications++;
        Assert.Throws<InvalidOperationException>(() => scene.SetOrganizationParent(a.Id, b.Id));
        Assert.Equal(0, notifications);
        Assert.Null(scene.World.GetOrganizationParent(a.Id));
        Assert.Equal(a.Id, scene.World.GetOrganizationParent(b.Id));
        Assert.Equal(b.Id, scene.SelectedId);
    }

    [Fact]
    public void EditingRelationship_RoundTripsAndDeleteDoesNotCascade()
    {
        var scene = new PreviewScene();
        var a = scene.CreateCube();
        var b = scene.CreateCube();
        scene.SetOrganizationParent(b.Id, a.Id);
        string path = Path.GetTempFileName();
        try
        {
            SceneFile.Save(scene, path);
            var target = new PreviewScene();
            SceneFile.Load(target, path);
            Assert.Equal(a.Id, target.World.GetOrganizationParent(b.Id));
            Assert.True(target.Delete(a.Id));
            Assert.Equal(b.Id, Assert.Single(target.Objects).Id);
            Assert.Null(target.World.GetOrganizationParent(b.Id));
        }
        finally { File.Delete(path); }
    }
}