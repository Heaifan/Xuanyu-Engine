using System.IO.Compression;

namespace XuanYu.World.Maps;

public static partial class MapFile
{
    private static void SaveBinary(MapDocument map, string path)
    {
        TerrainHeightField field = map.HeightField;
        using var output = File.Create(path);
        output.Write(map.GeoReference is null ? "XYEMAP03"u8 : "XYEMAP04"u8);
        using var zip = new GZipStream(output, CompressionLevel.Fastest);
        using var writer = new BinaryWriter(zip);
        writer.Write(MapDocument.CurrentVersion);
        writer.Write(map.Name);
        writer.Write(field.SamplesX);
        writer.Write(field.SamplesY);
        writer.Write(field.SpacingXMeters);
        writer.Write(field.SpacingYMeters);
        foreach (float height in field.Samples)
            writer.Write(height);
        writer.Write(map.WaterMask is not null);
        if (map.WaterMask is { } water)
            writer.Write(water.Samples);
        if (map.GeoReference is { } geo)
        {
            writer.Write(geo.SouthLatitudeDegrees);
            writer.Write(geo.WestLongitudeDegrees);
            writer.Write(geo.NorthLatitudeDegrees);
            writer.Write(geo.EastLongitudeDegrees);
            writer.Write((byte)geo.VerticalDatum);
        }
    }

    private static MapDocument LoadBinary(Stream source, bool hasMask,
        bool hasGeoReference)
    {
        using var zip = new GZipStream(source, CompressionMode.Decompress);
        using var reader = new BinaryReader(zip);
        int version = reader.ReadInt32();
        string name = reader.ReadString();
        int width = reader.ReadInt32();
        int height = reader.ReadInt32();
        float spacingX = reader.ReadSingle();
        float spacingY = reader.ReadSingle();
        if (version != MapDocument.CurrentVersion)
            throw new InvalidDataException($"不支持的地图版本：{version}。");
        var field = new TerrainHeightField(width, height, spacingX, spacingY);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                field.SetHeight(x, y, reader.ReadSingle());
        TerrainWaterMask? water = null;
        if (hasMask && reader.ReadBoolean())
        {
            int size = checked(width * height);
            byte[] values = reader.ReadBytes(size);
            if (values.Length != size)
                throw new InvalidDataException("SWB 掩膜数据不完整。");
            water = new TerrainWaterMask(width, height, values);
        }
        MapGeoReference? geo = null;
        if (hasGeoReference)
            geo = new MapGeoReference(reader.ReadDouble(), reader.ReadDouble(),
                reader.ReadDouble(), reader.ReadDouble(),
                (MapVerticalDatum)reader.ReadByte());
        return new MapDocument(name, field, water, geo);
    }
}