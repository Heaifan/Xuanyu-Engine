namespace XuanYu.World.Maps;

public sealed class NasademSampleBlock
{
    public const int CellsPerEdge = 300;
    public const int BlocksPerEdge = (NasademTile.Side - 1) / CellsPerEdge;

    private readonly NasademTile _tile;

    public string TileId => _tile.TileId;
    public int SouthLatitude => _tile.SouthLatitude;
    public int WestLongitude => _tile.WestLongitude;
    public int BlockX { get; }
    public int BlockY { get; }
    public int StartColumn => BlockX * CellsPerEdge;
    public int StartRowFromSouth => BlockY * CellsPerEdge;

    public NasademSampleBlock(NasademTile tile, int blockX, int blockY)
    {
        ArgumentNullException.ThrowIfNull(tile);
        if ((uint)blockX >= BlocksPerEdge)
            throw new ArgumentOutOfRangeException(nameof(blockX));
        if ((uint)blockY >= BlocksPerEdge)
            throw new ArgumentOutOfRangeException(nameof(blockY));

        _tile = tile;
        BlockX = blockX;
        BlockY = blockY;
    }

    public short? HeightAt(int column, int rowFromSouth)
    {
        if ((uint)column > CellsPerEdge)
            throw new ArgumentOutOfRangeException(nameof(column));
        if ((uint)rowFromSouth > CellsPerEdge)
            throw new ArgumentOutOfRangeException(nameof(rowFromSouth));
        return _tile.HeightAt(StartColumn + column,
            StartRowFromSouth + rowFromSouth);
    }
}