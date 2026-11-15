using System.Numerics;
using Silk.NET.Vulkan;
using XuanYu.Render;

namespace XuanYu.Render.Vulkan;

internal static class VkFrameSelection
{
    private static readonly (float X, float Y)[] Directions =
    [
        (-1f, 0f), (1f, 0f), (0f, -1f), (0f, 1f),
        (-0.7071f, -0.7071f), (0.7071f, -0.7071f),
        (-0.7071f, 0.7071f), (0.7071f, 0.7071f)
    ];

    public static void Record(Vk vk, CommandBuffer cmd, VkMeshScene scene,
        Extent2D extent, IReadOnlyList<RenderDraw> draws)
    {
        RenderDraw[] selected = draws.Where(d => d.Visible && d.Selected).ToArray();
        if (selected.Length == 0) return;

        vk.CmdBindPipeline(cmd, PipelineBindPoint.Graphics, scene.SelectionMaskPipeline);
        foreach (RenderDraw draw in selected)
            VkFrameCommands.DrawMesh(vk, cmd, scene, draw);

        vk.CmdBindPipeline(cmd, PipelineBindPoint.Graphics, scene.SelectionOutlinePipeline);
        foreach (RenderDraw draw in selected)
            foreach ((float x, float y) in Directions)
            {
                float dx = 4f * x / extent.Width;
                float dy = 4f * y / extent.Height;
                Matrix4x4 m = OffsetClip(draw.ModelToClip, dx, dy);
                VkFrameCommands.DrawMesh(vk, cmd, scene,
                    draw with { ModelToClip = m }, outline: true);
            }
    }

    private static Matrix4x4 OffsetClip(Matrix4x4 m, float dx, float dy)
    {
        m.M11 += dx * m.M14; m.M21 += dx * m.M24;
        m.M31 += dx * m.M34; m.M41 += dx * m.M44;
        m.M12 += dy * m.M14; m.M22 += dy * m.M24;
        m.M32 += dy * m.M34; m.M42 += dy * m.M44;
        return m;
    }
}