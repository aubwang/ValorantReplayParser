using Replay.Models.Descriptors;
using Replay.Models.Unreal;
using Replay.Unreal.Parsing;

namespace Replay.Valorant.Descriptors;

internal static class ValorantDescriptorDecoderExtensions
{
    public static FieldDescriptorBuilder Int32OrRaw(this FieldDescriptorBuilder builder) =>
        builder.Decode(ValorantPayloadDecoders.PrimitiveOrRaw(PrimitiveDecoders.Int32, "int32"));

    public static FieldDescriptorBuilder FloatOrRaw(this FieldDescriptorBuilder builder) =>
        builder.Decode(ValorantPayloadDecoders.PrimitiveOrRaw(PrimitiveDecoders.Float, "float"));

    public static FieldDescriptorBuilder DoubleOrRaw(this FieldDescriptorBuilder builder) =>
        builder.Decode(ValorantPayloadDecoders.PrimitiveOrRaw(PrimitiveDecoders.Double, "double"));

    public static FieldDescriptorBuilder BoolOrRaw(this FieldDescriptorBuilder builder) =>
        builder.Decode(ValorantPayloadDecoders.PrimitiveOrRaw(PrimitiveDecoders.Bool, "bool"));

    public static FieldDescriptorBuilder EnumByteOrRaw(this FieldDescriptorBuilder builder) =>
        builder.Decode(ValorantPayloadDecoders.PrimitiveOrRaw(PrimitiveDecoders.EnumByte, "enum byte"));

    public static FieldDescriptorBuilder ObjectNetGuidOrRaw(this FieldDescriptorBuilder builder) =>
        builder.Decode(ValorantPayloadDecoders.PrimitiveOrRaw(PrimitiveDecoders.ObjectNetGuid, "NetGUID"));

    public static FieldDescriptorBuilder FStringOrRaw(this FieldDescriptorBuilder builder) =>
        builder.Decode(ValorantPayloadDecoders.PrimitiveOrRaw(PrimitiveDecoders.FString, "FString"));

    public static FieldDescriptorBuilder FNameOrRaw(this FieldDescriptorBuilder builder) =>
        builder.Decode(ValorantPayloadDecoders.PrimitiveOrRaw(PrimitiveDecoders.FName, "FName"));

    public static FieldDescriptorBuilder FVectorOrRaw(this FieldDescriptorBuilder builder) =>
        builder.Decode(ValorantPayloadDecoders.PrimitiveOrRaw(PrimitiveDecoders.Vector, "FVector"));

    public static FieldDescriptorBuilder FRepMovement(
        this FieldDescriptorBuilder builder,
        ERotatorQuantization rotationQuantization = ERotatorQuantization.ShortComponents) =>
        builder.Decode(ValorantPayloadDecoders.PrimitiveOrRaw(
            PrimitiveDecoders.RepMovementWithRotation(rotationQuantization),
            "FRepMovement"));

    public static FieldDescriptorBuilder ByteArrayOrRaw(this FieldDescriptorBuilder builder, int maxBytes) =>
        builder.Decode(ValorantPayloadDecoders.PrimitiveOrRaw(PrimitiveDecoders.ByteArray(maxBytes), "TArray<uint8>"));
}
