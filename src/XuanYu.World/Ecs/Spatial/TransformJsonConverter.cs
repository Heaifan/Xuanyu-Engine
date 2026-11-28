using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using XuanYu.Core.Geodesy;

namespace XuanYu.World.Ecs;

public sealed class TransformJsonConverter : JsonConverter<Transform>
{
    public override Transform Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Transform 必须为对象。");
        var data = JsonSerializer.Deserialize<Data>(ref reader, options);
        if (data.ExactPosition is not { } exact)
            return new Transform(data.Position, data.Rotation, data.Scale);
        if (!double.IsFinite(exact.X) || !double.IsFinite(exact.Y) ||
            !double.IsFinite(exact.Z))
            throw new JsonException("精确空间位置无效。");
        var spatial = exact.Space switch
        {
            SpatialSpace.Local => SpatialPosition.FromLocal(exact.X, exact.Y, exact.Z),
            SpatialSpace.Ecef => SpatialPosition.FromEcef(new EcefPosition(exact.X, exact.Y, exact.Z)),
            _ => throw new JsonException("空间坐标类型无效。")
        };
        return Transform.FromSpatial(spatial, data.Rotation, data.Scale);
    }

    public override void Write(Utf8JsonWriter writer, Transform value,
        JsonSerializerOptions options)
    {
        var spatial = value.Spatial;
        Vector3 legacy = spatial.Space == SpatialSpace.Local ? value.Position : Vector3.Zero;
        PositionData? exact = spatial == SpatialPosition.FromLocal(legacy.X, legacy.Y, legacy.Z)
            ? null : new PositionData(spatial.Space, spatial.X, spatial.Y, spatial.Z);
        JsonSerializer.Serialize(writer,
            new Data(legacy, value.Rotation, value.Scale, exact), options);
    }

    private readonly record struct Data(Vector3 Position, Quaternion Rotation,
        Vector3 Scale, [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        PositionData? ExactPosition);

    private readonly record struct PositionData(SpatialSpace Space, double X, double Y, double Z);
}