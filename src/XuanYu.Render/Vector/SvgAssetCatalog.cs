namespace XuanYu.Render;

public sealed record SvgAssetEntry(string Id, string Name, string SourcePath, string Category);

public sealed class SvgAssetCatalog
{
    private readonly Dictionary<string, SvgAssetEntry> _paths = new(
        OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
    private readonly Dictionary<string, SvgAssetEntry> _entries = [];
    private readonly Dictionary<string, RenderMesh> _meshes = [];
    public IReadOnlyCollection<SvgAssetEntry> Entries => _entries.Values;
    public IEnumerable<SvgAssetEntry> LoadedEntries => _entries.Values.Where(x => _meshes.ContainsKey(x.Id));
    public int LoadedCount => _meshes.Count;

    public SvgAssetEntry Register(string path)
    {
        string full = Path.GetFullPath(path);
        if (!string.Equals(Path.GetExtension(full), ".svg", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("二维资产必须为 SVG 文件。", nameof(path));
        if (_paths.TryGetValue(full, out var existing)) return existing;
        var entry = new SvgAssetEntry($"svg/{Guid.NewGuid():N}",
            Path.GetFileNameWithoutExtension(full), full, "SVG");
        _entries.Add(entry.Id, entry);
        _paths.Add(full, entry);
        return entry;
    }

    public SvgAssetEntry Get(string id) => _entries[id];

    public RenderMesh Resolve(string id)
    {
        if (_meshes.TryGetValue(id, out var mesh)) return mesh;
        using var stream = File.OpenRead(Get(id).SourcePath);
        mesh = SvgMeshReader.Read(id, stream);
        _meshes.Add(id, mesh);
        return mesh;
    }
}