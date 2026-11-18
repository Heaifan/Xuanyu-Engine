using System.Security.Cryptography;
using System.Text;

namespace XuanYu.World.Maps;

internal sealed class NasademOverviewDiskCache(string directory)
{
    public (NasademOverviewGrid Far, NasademOverviewGrid Middle, bool Hit)
        Load(NasademCatalogEntry entry)
    {
        var source = new FileInfo(entry.Path);
        string stamp = FormattableString.Invariant(
            $"{source.Length}:{source.LastWriteTimeUtc.Ticks}");
        string folder = Folder(entry);
        string marker = Path.Combine(folder, "source.stamp");
        string far = Path.Combine(folder, $"{entry.Info.TileId}.s60.xyheight");
        string middle = Path.Combine(folder, $"{entry.Info.TileId}.s12.xyheight");
        if (File.Exists(marker) && File.ReadAllText(marker) == stamp &&
            File.Exists(far) && File.Exists(middle))
        {
            try { return ReadPair(far, middle, entry.Info, true); }
            catch (InvalidDataException) { }
        }

        // 完成标记最后写入，生成失败时不能把半成品当作有效缓存。
        Directory.CreateDirectory(folder);
        File.Delete(marker);
        var paths = NasademPyramidPreparation.Prepare(entry, folder);
        var pair = ReadPair(paths.Far, paths.Middle, entry.Info, false);
        File.WriteAllText(marker, stamp);
        return pair;
    }

    public NasademOverviewGrid ReadMiddle(NasademCatalogEntry entry) =>
        NasademOverviewFile.Read(Path.Combine(Folder(entry), $"{entry.Info.TileId}.s12.xyheight"),
            entry.Info, NasademOverviewLevel.Middle);

    private string Folder(NasademCatalogEntry entry) => Path.Combine(directory, "v1",
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(entry.Path))));

    private static (NasademOverviewGrid Far, NasademOverviewGrid Middle, bool Hit)
        ReadPair(string far, string middle, NasademHgtInfo info, bool hit) =>
        (NasademOverviewFile.Read(far, info, NasademOverviewLevel.Far),
         NasademOverviewFile.Read(middle, info, NasademOverviewLevel.Middle), hit);
}