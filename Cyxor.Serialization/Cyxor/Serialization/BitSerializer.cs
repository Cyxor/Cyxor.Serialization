namespace Cyxor.Serialization;

public struct BitSerializer : IComparable, IComparable<BitSerializer>, IEquatable<BitSerializer>
{
    long _bits;
    public const int MaxCapacity = 64;
    static readonly long[] s_mask = new long[MaxCapacity];

    static BitSerializer()
    {
        s_mask[0] = 1;

        for (var i = 0; i < MaxCapacity - 1; i++)
            s_mask[i + 1] = s_mask[i] * 2;
    }

    public BitSerializer(long value)
    {
        _bits = value;
    }

    public static implicit operator long(BitSerializer value) => value._bits;

    public long ToInt64() => _bits;

    public static implicit operator int(BitSerializer value) => (int)value._bits;

    public int ToInt32() => (int)_bits;

    public static implicit operator byte(BitSerializer value) => (byte)value._bits;

    public byte ToByte() => (byte)_bits;

    public static implicit operator short(BitSerializer value) => (short)value._bits;

    public int ToInt16() => (short)_bits;

    public static implicit operator BitSerializer(int value) => new(value);

    public static BitSerializer FromInt32(int value) => new(value);

    public static implicit operator BitSerializer(byte value) => new(value);

    public static BitSerializer FromByte(byte value) => new(value);

    public static implicit operator BitSerializer(long value) => new(value);

    public static BitSerializer FromInt64(long value) => new(value);

    public static implicit operator BitSerializer(short value) => new(value);

    public static BitSerializer FromInt16(short value) => new(value);

    public override int GetHashCode() => HashCode.Combine(_bits);

    public static bool operator ==(BitSerializer value1, BitSerializer value2) => value1._bits == value2._bits;

    public static bool operator !=(BitSerializer value1, BitSerializer value2) => value1._bits != value2._bits;

    public bool Equals(BitSerializer obj) => _bits == obj._bits;

    public int CompareTo(BitSerializer value) => _bits.CompareTo(value._bits);

    public override bool Equals(object? obj) => obj is BitSerializer serializer && _bits == serializer._bits;

    public int CompareTo(object? value)
    {
        if (value == null)
            return 1;

        if (value is BitSerializer bitSerializer)
            return _bits.CompareTo(bitSerializer._bits);

        throw new ArgumentException($"Argument must be a {nameof(BitSerializer)}.", nameof(value));
    }

    public bool this[int index]
    {
        get => (_bits & s_mask[index]) == s_mask[index];
        set =>
            _bits = value
                ? _bits | s_mask[index]
                : (_bits & s_mask[index]) == s_mask[index] ? _bits ^ s_mask[index] : _bits;
    }

    public int Count
    {
        get
        {
            var count = 1;
            var value = _bits;

            while ((value >>= 1) != 0) count++;

            return count;
        }
    }

    public int Serialize(long value, int offset)
    {
        if (offset < 0)
            throw new ArgumentOutOfRangeException(
                nameof(offset),
                Utilities.ResourceStrings.ExceptionNegativeNumber
            );

        var count = Utilities.Bits.Required(value);

        if (MaxCapacity - offset < count)
            throw new ArgumentException("The values provided exceed the capacity of the BitBuffer.");

        var valueBits = (BitSerializer)value;

        for (var i = 0; i < count; i++)
            this[i + offset] = valueBits[i];

        return count;
    }

    public long Deserialize(int offset, int count)
    {
        if (offset == 0 && count == 0)
            return _bits;

        if (offset < 0)
            throw new ArgumentOutOfRangeException(
                nameof(offset),
                Utilities.ResourceStrings.ExceptionNegativeNumber
            );

        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), Utilities.ResourceStrings.ExceptionNegativeNumber);

        if (MaxCapacity - offset < count)
            throw new ArgumentException("The values provided exceed the maximum capacity of the BitSerializer.");

        var result = 0;

        for (var i = 0; i < count; i++)
            result += this[i + offset] == true ? (int)(1 * Math.Pow(2, i)) : 0;

        return result;
    }

    public override string ToString()
    {
        var bitCount = Count;
        var bitString = bitCount == 1 ? "bit" : "bits";

        return $"{_bits} [{bitCount}{bitString}] {{{Convert.ToString(_bits, 2)}}}";
    }

    public static bool operator <(BitSerializer left, BitSerializer right) => left.CompareTo(right) < 0;

    public static bool operator <=(BitSerializer left, BitSerializer right) => left.CompareTo(right) <= 0;

    public static bool operator >(BitSerializer left, BitSerializer right) => left.CompareTo(right) > 0;

    public static bool operator >=(BitSerializer left, BitSerializer right) => left.CompareTo(right) >= 0;
}
