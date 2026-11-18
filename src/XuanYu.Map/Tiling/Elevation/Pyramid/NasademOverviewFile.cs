namespace XuanYu.World.Maps;

public static class NasademOverviewFile
{
    private const int Magic = 0x31504858;
    private const int Version = 1;
    private const int HeaderBytes = 5 * sizeof(int);

    public static void Write(string path, NasademOverviewGrid grid)
    {
        ArgumentNullException.ThrowIfNull(grid);
        using var writer = new BinaryWriter(File.Create(path));
        writer.Write(Magic);
        writer.Write(Version);
        writer.Write(grid.Info.SouthLatitude);
        writer.Write(grid.Info.WestLongitude);
        writer.Write(grid.Stride);
        foreach (short value in grid.Samples) writer.Write(value);
    }

    public static NasademOverviewGrid Read(string path, NasademHgtInfo info,
        NasademOverviewLevel level)
    {
        int side = NasademOverviewLayout.Side(level);
        using var stream = File.OpenRead(path);
        if (stream.Length != HeaderBytes + (long)side * side * sizeof(short))
            throw new InvalidDataException("派生高程文件长度不正确。");
        using var reader = new BinaryReader(stream);
        if (reader.ReadInt32() != Magic || reader.ReadInt32() != Version ||
            reader.ReadInt32() != info.SouthLatitude ||
            reader.ReadInt32() != info.WestLongitude || reader.ReadInt32() != (int)level)
            throw new InvalidDataException("派生高程文件版本、坐标或级别不匹配。");
        var samples = new short[side * side];
        for (int i = 0; i < samples.Length; i++) samples[i] = reader.ReadInt16();
        return new NasademOverviewGrid(info, level, side, samples);
    }
}