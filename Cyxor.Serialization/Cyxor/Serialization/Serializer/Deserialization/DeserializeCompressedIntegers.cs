using System.Runtime.CompilerServices;

namespace Cyxor.Serialization;

partial class Serializer
{
    #region CompressedIntegers

    ulong InternalDeserializeCompressedInt(int size, bool isSigned)
    {
        ulong val1 = 0;
        var val2 = 0;
        byte val3;

        var bitVal = size switch
        {
            2 => 21,
            4 => 35,
            8 => 63,
            _ => throw new ArgumentException("Invalid size", nameof(size))
        };

        while (val2 != bitVal)
        {
            val3 = DeserializeByte();
            val1 |= ((ulong)val3 & 127) << val2;
            val2 += 7;

            if ((val3 & 128) == 0)
                return !isSigned ? val1 : (ulong)((long)(val1 >> 1) ^ -(long)(val1 & 1));
        }

        throw new InvalidOperationException("Bad7BitEncodedInteger");
    }

    public short DeserializeCompressedInt16() =>
        (short)InternalDeserializeCompressedInt(sizeof(short), isSigned: true);

    public int DeserializeCompressedInt32() => (int)InternalDeserializeCompressedInt(sizeof(int), isSigned: true);

    public long DeserializeCompressedInt64() =>
        (long)InternalDeserializeCompressedInt(sizeof(long), isSigned: true);

    public ushort DeserializeCompressedUInt16() =>
        (ushort)InternalDeserializeCompressedInt(sizeof(ushort), isSigned: false);

    public uint DeserializeCompressedUInt32() =>
        (uint)InternalDeserializeCompressedInt(sizeof(uint), isSigned: false);

    public ulong DeserializeCompressedUInt64() => InternalDeserializeCompressedInt(sizeof(ulong), isSigned: false);

    #endregion CompressedIntegers

    #region CompressedIntegersTry

    bool InternalTryDeserializeCompressedInt<T>(out T value, int size, bool signed)
        where T : unmanaged
    {
        var result = false;
        var startPosition = _position;

        ulong val1 = 0;
        var val2 = 0;

        var bitVal = size switch
        {
            sizeof(short) => 21,
            sizeof(int) => 35,
            sizeof(long) => 63,
            _ => 0
        };

        while (val2 != bitVal)
        {
            if (!TryDeserializeByte(out var val3))
                break;

            val1 |= ((ulong)val3 & 127) << val2;
            val2 += 7;

            if ((val3 & 128) == 0)
            {
                if (signed)
                    val1 = (ulong)((long)(val1 >> 1) ^ -(long)(val1 & 1));

                result = true;
                break;
            }
        }

        value = Unsafe.As<ulong, T>(ref val1);
        _position = result ? _position : startPosition;
        return result;
    }

    public bool TryDeserializeCompressedInt16(out short value) =>
        InternalTryDeserializeCompressedInt(out value, sizeof(short), signed: true);

    public bool TryDeserializeCompressedInt32(out int value) =>
        InternalTryDeserializeCompressedInt(out value, sizeof(int), signed: true);

    public bool TryDeserializeCompressedInt64(out long value) =>
        InternalTryDeserializeCompressedInt(out value, sizeof(long), signed: true);

    public bool TryDeserializeCompressedUInt16(out ushort value) =>
        InternalTryDeserializeCompressedInt(out value, sizeof(ushort), signed: false);

    public bool TryDeserializeCompressedUInt32(out uint value) =>
        InternalTryDeserializeCompressedInt(out value, sizeof(uint), signed: false);

    public bool TryDeserializeCompressedUInt64(out ulong value) =>
        InternalTryDeserializeCompressedInt(out value, sizeof(ulong), signed: false);
    #endregion CompressedIntegersTry
}
