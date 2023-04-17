using System.Runtime.CompilerServices;

namespace Cyxor.Serialization;

partial class Serializer
{
    #region Internal

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static unsafe int Wcslen(char* value)
    {
        var pointer = value;

        while (((uint)pointer & 3) != 0 && *pointer != 0) pointer++;

        if (*pointer != 0)
            while ((pointer[0] & pointer[1]) != 0 || pointer[0] != 0 && pointer[1] != 0) pointer += 2;

        for (; *pointer != 0; pointer++)
            ;

        return (int)(pointer - value);
    }

    #endregion Internal

    public void Serialize(char[]? value) =>
        InternalSerialize(new ReadOnlySpan<char>(value), raw: AutoRaw, containsNullPointer: value == null);

    public void Serialize(char[]? value, int start, int length) =>
        InternalSerialize(
            new ReadOnlySpan<char>(value, start, length),
            raw: false,
            containsNullPointer: value == null
        );

    public void SerializeRaw(char[]? value) =>
        InternalSerialize(new ReadOnlySpan<char>(value), raw: true, containsNullPointer: value == null);

    public void SerializeRaw(char[]? value, int start, int length) =>
        InternalSerialize(
            new ReadOnlySpan<char>(value, start, length),
            raw: true,
            containsNullPointer: value == null
        );

    public unsafe void Serialize(char* value) =>
        InternalSerialize(
            new ReadOnlySpan<char>(value, value == null ? 0 : Wcslen(value)),
            raw: AutoRaw,
            containsNullPointer: value == null
        );

    public unsafe void Serialize(char* value, int length) =>
        InternalSerialize(new ReadOnlySpan<char>(value, length), raw: false, containsNullPointer: value == null);

    public unsafe void SerializeRaw(char* value) =>
        InternalSerialize(
            new ReadOnlySpan<char>(value, value == null ? 0 : Wcslen(value)),
            raw: true,
            containsNullPointer: value == null
        );

    public unsafe void SerializeRaw(char* value, int length) =>
        InternalSerialize(new ReadOnlySpan<char>(value, length), raw: true, containsNullPointer: value == null);
}
