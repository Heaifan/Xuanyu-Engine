namespace XuanYu.Editor.UI.Right;

public sealed record RightInspectorField(string Name, string Value);

public sealed record RightInspectorSection(
    string Title,
    IReadOnlyList<RightInspectorField> Fields);

public sealed record RightInspectorSnapshot(
    string Name,
    string ObjectType,
    string Context,
    string IconKey,
    IReadOnlyList<RightInspectorSection> Sections,
    bool? Visible = null);
