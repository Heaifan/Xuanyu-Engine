using System.Buffers.Binary;

namespace XuanYu.World.Maps;

internal static class XYTerrainDelta
{
    public static byte[] Forward(byte[] raw)
    {
        byte[] output = new byte[raw.Length];
        for (int start = 0; start < raw.Length; start += NasademTile.Side * 2)
        {
            ushort previous = 0;
            for (int col = 0; col < NasademTile.Side; col++)
            {
                int offset = start + col * 2;
                ushort current = BinaryPrimitives.ReadUInt16LittleEndian(raw.AsSpan(offset, 2));
                BinaryPrimitives.WriteUInt16LittleEndian(output.AsSpan(offset, 2),
                    unchecked((ushort)(current - previous)));
                previous = current;
            }
        }
        return output;
    }

    public static void Reverse(byte[] bytes)
    {
        for (int start = 0; start < bytes.Length; start += NasademTile.Side * 2)
        {
            ushort previous = 0;
            for (int col = 0; col < NasademTile.Side; col++)
            {
                int offset = start + col * 2;
                ushort delta = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset, 2));
                previous = unchecked((ushort)(previous + delta));
                BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(offset, 2), previous);
            }
        }
    }
}