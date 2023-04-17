namespace Cyxor.Serialization;

partial class Serializer
{
    #region Internal

    delegate void SerializeSignature<T>(T value)
        where T : struct;

    delegate void SerializeSignatureLittleEndian<T>(T value, bool littleEndian)
        where T : struct;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void InternalSerializeNullableValue<T>(T? value, SerializeSignature<T> serializeDelegate)
        where T : struct
    {
        if (value == null)
            Serialize(false);
        else
        {
            Serialize(true);
            serializeDelegate((T)value);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void InternalSerializeNullableValue<T>(
        T? value,
        bool littleEndian,
        SerializeSignatureLittleEndian<T> serializeDelegate
    )
        where T : struct
    {
        if (value == null)
            Serialize(false);
        else
        {
            Serialize(true);
            serializeDelegate((T)value, littleEndian);
        }
    }

    #endregion Internal

    #region Primitive types

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(bool? value) => Serialize((byte)(value == default ? -1 : value == false ? 0 : 1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(char? value) => InternalSerializeNullableValue(value, Serialize);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(float? value) => InternalSerializeNullableValue(value, Serialize);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(double? value) => InternalSerializeNullableValue(value, Serialize);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(byte? value) => InternalSerializeNullableValue(value, Serialize);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(sbyte? value) => InternalSerializeNullableValue(value, Serialize);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(short? value) => InternalSerializeNullableValue(value, Serialize);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(short? value, bool littleEndian) =>
        InternalSerializeNullableValue(value, littleEndian, Serialize);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(ushort? value) => InternalSerializeNullableValue(value, Serialize);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(ushort? value, bool littleEndian) =>
        InternalSerializeNullableValue(value, littleEndian, Serialize);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(int? value) => InternalSerializeNullableValue(value, Serialize);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SerializeUncompressedNullableInt32(int? value) =>
        InternalSerializeNullableValue(value, SerializeUncompressedInt32);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SerializeUncompressedNullableInt32(int? value, bool littleEndian) =>
        InternalSerializeNullableValue(value, littleEndian, SerializeUncompressedInt32);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(uint? value) => InternalSerializeNullableValue(value, Serialize);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(uint? value, bool littleEndian) =>
        InternalSerializeNullableValue(value, littleEndian, Serialize);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(long? value) => InternalSerializeNullableValue(value, Serialize);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SerializeUncompressedNullableInt64(long? value) =>
        InternalSerializeNullableValue(value, SerializeUncompressedInt64);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SerializeUncompressedNullableInt64(long? value, bool littleEndian) =>
        InternalSerializeNullableValue(value, littleEndian, SerializeUncompressedInt64);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(ulong? value) => InternalSerializeNullableValue(value, Serialize);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(ulong? value, bool littleEndian) =>
        InternalSerializeNullableValue(value, littleEndian, Serialize);

    #endregion Primitive types

    #region Struct types

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(decimal? value) => InternalSerializeNullableValue(value, Serialize);

    public void Serialize(BitSerializer? value) => InternalSerializeNullableValue(value, Serialize);

    public void Serialize(Guid? value) => InternalSerializeNullableValue(value, Serialize);

    public void Serialize(TimeSpan? value) => InternalSerializeNullableValue(value, Serialize);

    public void Serialize(DateTime? value) => InternalSerializeNullableValue(value, Serialize);

    public void Serialize(DateTimeOffset? value) => InternalSerializeNullableValue(value, Serialize);

    public void Serialize(BigInteger? value)
    {
        if (value == null)
            Serialize((byte)0);
        else
            Serialize(value.Value);
    }

    public void SerializeEnum<T>(T? value)
        where T : unmanaged, Enum => InternalSerializeNullableValue(value, SerializeEnum);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SerializerMethodIdentifier(SerializerMethodIdentifier.SerializeUnmanaged)]
    public unsafe void Serialize<T>(T? value)
        where T : unmanaged
    {
        Serialize(value != null);

        if (value == null) return;

        var size = sizeof(T);

        InternalEnsureSerializeCapacity(size);

        ref T refValue = ref Unsafe.AsRef((T)value);

        if (_stream == null)
            MemoryMarshal.Write(_memory.Span[_position..], ref refValue);
        else
            _stream.Write(new ReadOnlySpan<byte>(Unsafe.AsPointer(ref refValue), size));

        _position += size;

    }
    #endregion Struct types
}
