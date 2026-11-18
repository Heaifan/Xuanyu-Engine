using System.Buffers.Binary;
using System.IO.Compression;
using System.Linq;

namespace XuanYu.World.Maps;

public static partial class NasademHgtFile
{
    public static NasademTile Load(string path) =>
        string.Equals(Path.GetExtension(path), ".xyt", StringComparison.OrdinalIgnoreCase)
            ? XYTerrainReader.ReadTile(path) : Read<NasademTile>(path, Decode);

    public static NasademHgtInfo ReadInfo(string path) =>
        string.Equals(Path.GetExtension(path), ".xyt", StringComparison.OrdinalIgnoreCase)
            ? XYTerrainReader.ReadInfo(path)
            : Read(path, static (_, length, name) => ParseInfo(length, name));

    private static T Read<T>(string path, Func<Stream, long, string, T> read)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        string extension = Path.GetExtension(path);

        if (extension.Equals(".hgt", StringComparison.OrdinalIgnoreCase))
        {
            using var stream = File.OpenRead(path);
            return read(stream, stream.Length,
                Path.GetFileNameWithoutExtension(path));
        }

        if (extension.Equals(".zip", StringComparison.OrdinalIgnoreCase))
        {
            using var archive = ZipFile.OpenRead(path);
            var entries = archive.Entries.Where(entry =>
                Path.GetExtension(entry.Name).Equals(
                    ".hgt", StringComparison.OrdinalIgnoreCase)).ToArray();
            if (entries.Length != 1)
                throw new InvalidDataException(
                    "NASADEM ZIP 中必须包含且仅包含一个 HGT 文件。");

            using var stream = entries[0].Open();
            return read(stream, entries[0].Length,
                Path.GetFileNameWithoutExtension(entries[0].Name));
        }

        throw new NotSupportedException("仅支持 .hgt 或 .zip 文件。");
    }

    private static NasademTile Decode(Stream stream, long length, string name)
    {
        NasademHgtInfo info = ParseInfo(length, name);
        byte[] raw = new byte[(int)NasademTile.ByteLength];
        stream.ReadExactly(raw);

        short[] samples = new short[NasademTile.SampleCount];
        int missing = 0;
        short min = short.MaxValue;
        short max = short.MinValue;

        for (int i = 0; i < samples.Length; i++)
        {
            short value = BinaryPrimitives.ReadInt16BigEndian(
                raw.AsSpan(i * 2, 2));
            samples[i] = value;
            if (value == short.MinValue)
            {
                missing++;
                continue;
            }
            min = Math.Min(min, value);
            max = Math.Max(max, value);
        }

        if (missing == samples.Length)
            throw new InvalidDataException("HGT 不包含有效高程数据。");

        return new NasademTile(info.TileId, info.SouthLatitude,
            info.WestLongitude, samples, missing, min, max);
    }
}