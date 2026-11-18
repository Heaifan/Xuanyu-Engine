using System.Text.RegularExpressions;

namespace XuanYu.World.Maps;

public static partial class NasademHgtFile
{
    private static NasademHgtInfo ParseInfo(long length, string name)
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

        return new NasademHgtInfo(match.Value.ToUpperInvariant(),
            latitude, longitude);
    }
}