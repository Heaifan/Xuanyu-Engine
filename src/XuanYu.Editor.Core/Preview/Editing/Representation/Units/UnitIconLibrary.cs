using XuanYu.Render;

namespace XuanYu.Editor.Preview;

public sealed record UnitIconEntry(string Name, string Category, string SourcePath);

public sealed class UnitIconLibrary
{
    private readonly SvgAssetCatalog _catalog;
    private readonly UnitIconEntry[] _presets;
    private static readonly StringComparer Paths = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
    public string RecentPath { get; private set; }

    public UnitIconLibrary(SvgAssetCatalog catalog, string root)
    {
        _catalog = catalog;
        var examples = Path.GetFullPath(Path.Combine(root, "assets", "vector", "Examples"));
        _presets =
        [
            Land("步兵营", "infantry.svg"), Land("坦克营", "Army/Combat/tank.svg"),
            Land("机械化步兵营", "Army/Combat/mechanized.svg"), Land("炮兵营", "Army/Combat/artillery.svg"),
            Land("工兵营", "Army/Support/engineer.svg"), Land("侦察营", "Army/Combat/recon.svg"),
            Land("通信营", "Army/Support/signals.svg"), Land("后勤营", "Army/Support/logistics.svg"),
            Land("指挥部", "Army/Support/headquarters.svg")
        ];
        RecentPath = _presets[0].SourcePath;
        UnitIconEntry Land(string name, string path) => new(name, "陆军", Path.Combine(examples, path.Replace('/', Path.DirectorySeparatorChar)));
    }

    public IReadOnlyList<UnitIconEntry> Search(string category, string query)
    {
        var custom = _catalog.LoadedEntries.Where(x => FindPreset(x.SourcePath) is null)
            .Select(x => new UnitIconEntry(x.Name, "自定义", x.SourcePath));
        return _presets.Concat(custom).Where(x => (category == "全部" || x.Category == category) &&
            (x.Name.Contains(query.Trim(), StringComparison.OrdinalIgnoreCase) ||
             Path.GetFileNameWithoutExtension(x.SourcePath).Contains(query.Trim(), StringComparison.OrdinalIgnoreCase))).ToArray();
    }

    public string NameFor(string path) => FindPreset(Path.GetFullPath(path))?.Name ?? Path.GetFileNameWithoutExtension(path);
    internal void Remember(string path) => RecentPath = Path.GetFullPath(path);
    private UnitIconEntry? FindPreset(string path) => _presets.FirstOrDefault(x => Paths.Equals(x.SourcePath, path));
}