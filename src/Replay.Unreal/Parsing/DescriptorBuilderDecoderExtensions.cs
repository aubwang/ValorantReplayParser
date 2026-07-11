using Replay.Models.Descriptors;

namespace Replay.Unreal.Parsing;

public static class DescriptorBuilderDecoderExtensions
{
    public static FieldDescriptorBuilder Int32(this FieldDescriptorBuilder builder) =>
        builder.Decode(PrimitiveDecoders.Int32);

    public static FieldDescriptorBuilder UInt32(this FieldDescriptorBuilder builder) =>
        builder.Decode(PrimitiveDecoders.UInt32);

    public static FieldDescriptorBuilder Float(this FieldDescriptorBuilder builder) =>
        builder.Decode(PrimitiveDecoders.Float);

    public static FieldDescriptorBuilder Double(this FieldDescriptorBuilder builder) =>
        builder.Decode(PrimitiveDecoders.Double);

    public static FieldDescriptorBuilder Bool(this FieldDescriptorBuilder builder) =>
        builder.Decode(PrimitiveDecoders.Bool);

    public static FieldDescriptorBuilder Byte(this FieldDescriptorBuilder builder) =>
        builder.Decode(PrimitiveDecoders.Byte);

    public static FieldDescriptorBuilder EnumByte(this FieldDescriptorBuilder builder) =>
        builder.Decode(PrimitiveDecoders.EnumByte);

    public static FieldDescriptorBuilder FString(this FieldDescriptorBuilder builder) =>
        builder.Decode(PrimitiveDecoders.FString);

    public static FieldDescriptorBuilder FName(this FieldDescriptorBuilder builder) =>
        builder.Decode(PrimitiveDecoders.FName);

    public static FieldDescriptorBuilder ByteArray(this FieldDescriptorBuilder builder, int maxBytes) =>
        builder.Decode(PrimitiveDecoders.ByteArray(maxBytes));

    public static FieldDescriptorBuilder ObjectNetGuid(this FieldDescriptorBuilder builder) =>
        builder.Decode(PrimitiveDecoders.ObjectNetGuid);

    public static FieldDescriptorBuilder SerializedInt(this FieldDescriptorBuilder builder, int maxValue) =>
        builder.Decode(PrimitiveDecoders.SerializedInt(maxValue));
    
    public static FieldDescriptorBuilder Guid(this FieldDescriptorBuilder builder) =>
        builder.Decode(PrimitiveDecoders.Guid);

    public static FieldDescriptorBuilder FVector(this FieldDescriptorBuilder builder) =>
        builder.Decode(PrimitiveDecoders.Vector);
    
    public static FieldDescriptorBuilder Transform(this FieldDescriptorBuilder builder) =>
        builder.Decode(PrimitiveDecoders.Transform);
    
    public static FieldDescriptorBuilder ReplicatedMovement(this FieldDescriptorBuilder builder) =>
        builder.Decode(PrimitiveDecoders.RepMovement);

    public static FieldDescriptorBuilder FVectorNetQuantize(this FieldDescriptorBuilder builder) =>
        builder.Decode(PrimitiveDecoders.VectorNetQuantize);

    public static FieldDescriptorBuilder FVectorNetQuantize10(this FieldDescriptorBuilder builder) =>
        builder.Decode(PrimitiveDecoders.VectorNetQuantize10);

    public static FieldDescriptorBuilder FVectorNetQuantize100(this FieldDescriptorBuilder builder) =>
        builder.Decode(PrimitiveDecoders.VectorNetQuantize100);

    public static FieldDescriptorBuilder FVectorNetQuantizeNormal(this FieldDescriptorBuilder builder) =>
        builder.Decode(PrimitiveDecoders.VectorNetQuantizeNormal);

    public static FieldDescriptorBuilder RepLayoutDynamicArray<TElement>(this FieldDescriptorBuilder builder)
        where TElement : ExportGroupDescriptor<TElement>, new() =>
        builder.Decode(RepLayoutArrayDecoders.DynamicArray<TElement>());

    public static FieldDescriptorBuilder RepLayoutDynamicArray<TElement>(
        this FieldDescriptorBuilder builder,
        IFieldDecoder elementDecoder) =>
        builder.Decode(RepLayoutArrayDecoders.DynamicArray<TElement>(elementDecoder));

    public static FieldDescriptorBuilder Ignore(this FieldDescriptorBuilder builder) =>
        builder.Decode(PrimitiveDecoders.Skip);
}
