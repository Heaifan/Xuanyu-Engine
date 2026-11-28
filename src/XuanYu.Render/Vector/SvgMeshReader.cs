using System.Numerics;
using System.Xml;
using System.Xml.Linq;

namespace XuanYu.Render;

public static class SvgMeshReader
{
    private static readonly XNamespace Ns = "http://www.w3.org/2000/svg";

    public static RenderMesh Read(string id, Stream source)
    {
        using var reader = XmlReader.Create(source, new XmlReaderSettings
            { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
        var root = XDocument.Load(reader).Root;
        if (root is null || root.Name != Ns + "svg") throw new InvalidDataException("缺少 SVG 根节点。");
        var box = SvgNumbers.Parse((string?)root.Attribute("viewBox") ?? "");
        if (box.Length != 4 || box[2] <= 0 || box[3] <= 0)
            throw new InvalidDataException("SVG 需要有效的 viewBox。");
        var mesh = new SvgMeshBuilder(new Vector2(box[0] + box[2]/2, box[1] + box[3]/2),
            Math.Max(box[2], box[3]));
        Visit(root, SvgPaint.Default, mesh, Matrix3x2.Identity);
        return mesh.Build(id);
    }

    private static void Visit(XElement node, SvgPaint inherited, SvgMeshBuilder mesh, Matrix3x2 parent)
    {
        if (node.Name.Namespace != Ns) throw new InvalidDataException("不支持非 SVG 图元。");
        string name = node.Name.LocalName;
        if (name is "title" or "desc") return;
        SvgPaint paint = inherited.Apply(node);
        var transform = SvgTransform.Read((string?)node.Attribute("transform")) * parent;
        if (name is "svg" or "g")
        {
            SvgPaint.CheckAttributes(node, name == "svg" ? "viewBox width height version" : "");
            if (name == "svg" && node.Parent is not null)
                throw new InvalidDataException("暂不支持嵌套 SVG。");
            foreach (var child in node.Elements()) Visit(child, paint, mesh, transform);
            return;
        }
        var contours = SvgShapes.Read(node);
        mesh.Transform = transform;
        mesh.Opacity = paint.FillOpacity;
        if (paint.Fill is { } fill && paint.FillOpacity > 0) mesh.Fill(contours, fill, paint.EvenOdd);
        mesh.Opacity = 1;
        if (paint.Stroke is { } stroke && paint.StrokeWidth > 0)
            foreach (var contour in contours) mesh.Stroke(contour, stroke, paint.StrokeWidth);
    }
}