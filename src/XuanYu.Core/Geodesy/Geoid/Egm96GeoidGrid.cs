using System.Buffers.Binary;

namespace XuanYu.Core.Geodesy;

public sealed class Egm96GeoidGrid
{
    private const int Rows = 721, Columns = 1440, HeaderBytes = 40;
    private const double Step = 0.25d;
    private readonly float[] _values;

    private Egm96GeoidGrid(float[] values) => _values = values;

    public static Egm96GeoidGrid Load(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        ValidateHeader(bytes);
        var values = new float[Rows * Columns];
        for (int i = 0; i < values.Length; i++)
        {
            float value = BinaryPrimitives.ReadSingleBigEndian(
                bytes.AsSpan(HeaderBytes + i * sizeof(float), sizeof(float)));
            if (!float.IsFinite(value) || Math.Abs(value) > 200f)
                throw new InvalidDataException("EGM96 格网存在无效起伏值。");
            values[i] = value;
        }
        return new Egm96GeoidGrid(values);
    }

    public double UndulationMeters(GeoCoordinate position)
    {
        double x = (position.LongitudeDegrees + 180d) / Step;
        double y = (position.LatitudeDegrees + 90d) / Step;
        int column = (int)x, row = (int)y;
        int west = column % Columns, east = (west + 1) % Columns;
        int north = Math.Min(row + 1, Rows - 1);
        double southHeight = Lerp(_values[row * Columns + west],
            _values[row * Columns + east], x - column);
        double northHeight = Lerp(_values[north * Columns + west],
            _values[north * Columns + east], x - column);
        return Lerp(southHeight, northHeight, y - row);
    }

    // 双线性格网在每个格元矩形上的最大值必在四角。
    // 调用者传入已裁剪纬度和展开经度；允许跨国际日期变更线。
    public double UpperBoundMeters(double south, double north, double west, double east)
    {
        if (!double.IsFinite(south) || !double.IsFinite(north) ||
            south < -90d || north > 90d || south > north ||
            !double.IsFinite(west) || !double.IsFinite(east) ||
            west < -360d || east > 360d || west > east || east - west > 360d)
            throw new ArgumentOutOfRangeException(nameof(south), "EGM96 查询范围无效。");
        double maximum = double.NegativeInfinity;
        for (double lat = south; ; lat = Math.Min(north, (Math.Floor(lat / Step) + 1d) * Step))
        {
            for (double lon = west; ; lon = Math.Min(east, (Math.Floor(lon / Step) + 1d) * Step))
            {
                maximum = Math.Max(maximum, UndulationMeters(new(lat, lon, 0d)));
                if (lon == east) break;
            }
            if (lat == north) break;
        }
        return maximum;
    }

    private static double Lerp(double a, double b, double t) => a + (b - a) * t;

    private static void ValidateHeader(byte[] bytes)
    {
        if (bytes.Length != HeaderBytes + Rows * Columns * sizeof(float) ||
            BinaryPrimitives.ReadDoubleBigEndian(bytes.AsSpan(0, 8)) != -90d ||
            BinaryPrimitives.ReadDoubleBigEndian(bytes.AsSpan(8, 8)) != -180d ||
            BinaryPrimitives.ReadDoubleBigEndian(bytes.AsSpan(16, 8)) != Step ||
            BinaryPrimitives.ReadDoubleBigEndian(bytes.AsSpan(24, 8)) != Step ||
            BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(32, 4)) != Rows ||
            BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(36, 4)) != Columns)
            throw new InvalidDataException("不是受支持的 EGM96 15 角分全球 GTX 格网。");
    }
}