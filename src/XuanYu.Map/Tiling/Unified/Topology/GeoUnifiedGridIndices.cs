namespace XuanYu.World.Maps;

public static class GeoUnifiedGridIndices
{
    public static int[] Create(int mask)
    {
        if ((uint)mask > 15) throw new ArgumentOutOfRangeException(nameof(mask));
        var result = new List<int>(6144);
        for (int y = 0; y < 32; y++)
            for (int x = 0; x < 32; x++)
            {
                int a = Vertex(x, y, mask), b = Vertex(x + 1, y, mask);
                int c = Vertex(x, y + 1, mask), d = Vertex(x + 1, y + 1, mask);
                Add(a, b, d); Add(a, d, c);
            }
        return result.ToArray();

        void Add(int a, int b, int c)
        {
            if (a == b || b == c || c == a) return;
            result.Add(a); result.Add(b); result.Add(c);
        }
    }

    private static int Vertex(int x, int y, int mask)
    {
        if ((x & 1) != 0 && (y == 0 && (mask & 1) != 0 ||
            y == 32 && (mask & 2) != 0)) x--;
        if ((y & 1) != 0 && (x == 0 && (mask & 4) != 0 ||
            x == 32 && (mask & 8) != 0)) y--;
        return y * 33 + x;
    }
}
