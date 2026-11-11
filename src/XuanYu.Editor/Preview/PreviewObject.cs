
using System.Numerics;
using XuanYu.Render;

namespace XuanYu.Editor.Preview;

public sealed class PreviewObject
{
    public Guid Id { get; } = Guid.NewGuid();

    public string Name { get; }

    public RenderMesh Mesh { get; }

    public Matrix4x4 ModelToClip { get; }

    public bool Visible { get; internal set; } = true;

    public PreviewObject(
        string name,
        RenderMesh mesh,
        Matrix4x4 modelToClip)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "对象名称不能为空。", nameof(name));

        Name = name;
        Mesh = mesh;
        ModelToClip = modelToClip;
    }

    public override string ToString() => Name;
}
