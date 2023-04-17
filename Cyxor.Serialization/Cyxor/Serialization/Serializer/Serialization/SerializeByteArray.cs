namespace Cyxor.Serialization;

partial class Serializer
{
    #region Internal

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static unsafe int Strlen(byte* value)
    {
        var pointer = value;

        while (*pointer != 0) ++pointer;

        return (int)(pointer - value);
    }

    #endregion Internal

    public void Serialize(byte[]? value) =>
        InternalSerialize(new ReadOnlySpan<byte>(value), raw: AutoRaw, containsNullPointer: value == null);

    public void Serialize(byte[]? value, int start, int length) =>
        InternalSerialize(
            new ReadOnlySpan<byte>(value, start, length),
            raw: false,
            containsNullPointer: value == null
        );

    public void SerializeRaw(byte[]? value) =>
        InternalSerialize(new ReadOnlySpan<byte>(value), raw: true, containsNullPointer: value == null);

    public void SerializeRaw(byte[]? value, int start, int length) =>
        InternalSerialize(
            new ReadOnlySpan<byte>(value, start, length),
            raw: true,
            containsNullPointer: value == null
        );

    public unsafe void Serialize(byte* value) =>
        InternalSerialize(
            new ReadOnlySpan<byte>(value, value == null ? 0 : Strlen(value)),
            raw: AutoRaw,
            containsNullPointer: value == null
        );

    unsafe public void Serialize(byte* value, int length) =>
        InternalSerialize(new ReadOnlySpan<byte>(value, length), raw: false, containsNullPointer: value == null);

    public unsafe void SerializeRaw(byte* value) =>
        InternalSerialize(
            new ReadOnlySpan<byte>(value, value == null ? 0 : Strlen(value)),
            raw: true,
            containsNullPointer: value == null
        );

    public unsafe void SerializeRaw(byte* value, int length) =>
        InternalSerialize(new ReadOnlySpan<byte>(value, length), raw: true, containsNullPointer: value == null);
}
