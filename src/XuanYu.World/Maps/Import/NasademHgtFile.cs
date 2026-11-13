
using System.Buffers.Binary;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

namespace XuanYu.World.Maps;

public static class NasademHgtFile
{
    public static NasademTile Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        string extension = Path.GetExtension(path);

        if (extension.Equals(".hgt", StringComparison.OrdinalIgnoreCase))
        {
            using var stream = File.OpenRead(path);
            return Decode(stream, stream.Length,
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
            return Decode(stream, entries[0].Length,
                Path.GetFileNameWithoutExtension(entries[0].Name));
        }

        throw new NotSupportedException("仅支持 .hgt 或 .zip 文件。");
    }

    private static NasademTile Decode(Stream stream, long length, string name)
    {
        var match = Regex.Match(name,
            @"([NS])(\d{2})([EW])(\d{3})$",
            RegexOptions.IgnoreCase);

        if (!match.Success)
            throw new InvalidDataException("HGT 文件名缺少有效的经纬度瓦片编号。");

        int latitude = int.Parse(match.Groups[2].Value);
        int longitude = int.Parse(match.Groups[4].Value);
        if (latitude > 90 || longitude > 180 ||
            (latitude == 90 && match.Groups[1].Value.Equals(
                "N", StringComparison.OrdinalIgnoreCase)) ||
            (longitude == 180 && match.Groups[3].Value.Equals(
                "E", StringComparison.OrdinalIgnoreCase)))
            throw new InvalidDataException("HGT 瓦片经纬度超出范围。");

        if (match.Groups[1].Value.Equals("S", StringComparison.OrdinalIgnoreCase))
            latitude = -latitude;
        if (match.Groups[3].Value.Equals("W", StringComparison.OrdinalIgnoreCase))
            longitude = -longitude;

        if (length != NasademTile.ByteLength)
            throw new InvalidDataException("HGT 数据大小不是 3601×3601×2 字节。");

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

        string id = match.Value.ToUpperInvariant();
        return new NasademTile(id, latitude, longitude,
            samples, missing, min, max);
    }
}
