namespace Cyxor.Serialization;

partial class Serializer
{
    #region byte

    public void Serialize(Memory<byte> value) => InternalSerialize(value.Span, AutoRaw);

    public void SerializeRaw(Memory<byte> value) => InternalSerialize(value.Span, raw: true);

    public void Serialize(ReadOnlyMemory<byte> value) => InternalSerialize(value.Span, AutoRaw);

    public void SerializeRaw(ReadOnlyMemory<byte> value) => InternalSerialize(value.Span, raw: true);

    public void Serialize(Memory<byte>? value) => InternalSerializeNullableValue(value, Serialize);

    public void Serialize(ReadOnlyMemory<byte>? value) => InternalSerializeNullableValue(value, Serialize);

    #endregion byte

    #region char

    public void Serialize(Memory<char> value) => InternalSerialize(value.Span, AutoRaw);

    public void SerializeRaw(Memory<char> value) => InternalSerialize(value.Span, raw: true);

    public void Serialize(ReadOnlyMemory<char> value) => InternalSerialize(value.Span, AutoRaw);

    public void SerializeRaw(ReadOnlyMemory<char> value) => InternalSerialize(value.Span, raw: true);

    public void Serialize(Memory<char>? value) => InternalSerializeNullableValue(value, Serialize);

    public void Serialize(ReadOnlyMemory<char>? value) => InternalSerializeNullableValue(value, Serialize);

    #endregion char

    #region t

    public void Serialize<T>(Memory<T> value)
        where T : unmanaged => InternalSerializeT(value.Span, AutoRaw);

    public void SerializeRaw<T>(Memory<T> value)
        where T : unmanaged => InternalSerializeT(value.Span, raw: true);

    public void Serialize<T>(ReadOnlyMemory<T> value)
        where T : unmanaged => InternalSerializeT(value.Span, AutoRaw);

    public void SerializeRaw<T>(ReadOnlyMemory<T> value)
        where T : unmanaged => InternalSerializeT(value.Span, raw: true);

    public void Serialize<T>(Memory<T>? value)
        where T : unmanaged => InternalSerializeNullableValue(value, Serialize);

    public void Serialize<T>(ReadOnlyMemory<T>? value)
        where T : unmanaged => InternalSerializeNullableValue(value, Serialize);
    #endregion t
}
