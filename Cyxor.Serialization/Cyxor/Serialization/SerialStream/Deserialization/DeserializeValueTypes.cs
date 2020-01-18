using System;
using System.Numerics;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        #region ValueTypes

        T InternalDeserializeUnmanaged<T>(int size, bool unsigned, bool floatingPoint = false, bool? littleEndian = default) where T : struct
        {
            EnsureCapacity(size, SerializerOperation.Deserialize);

            var value = default(ValueType);

            unsafe
            {
                fixed (byte* ptr = &_buffer![_position])
                {
                    _position += size;

                    var isLittleEndian = littleEndian ?? BitConverter.IsLittleEndian;
                    var swap = BitConverter.IsLittleEndian && !isLittleEndian || !BitConverter.IsLittleEndian && isLittleEndian;

                    if (unsigned)
                        switch (size)
                        {
                            case sizeof(byte): return (T)(value = *ptr);
                            case sizeof(ushort): return (T)(value = swap ? Utilities.ByteOrder.Swap(*(ushort*)ptr) : *(ushort*)ptr);
                            case sizeof(uint): return (T)(value = swap ? Utilities.ByteOrder.Swap(*(uint*)ptr) : *(uint*)ptr);
                            case sizeof(ulong): return (T)(value = swap ? Utilities.ByteOrder.Swap(*(ulong*)ptr) : *(ulong*)ptr);
                        }
                    else
                        switch (size)
                        {
                            case sizeof(sbyte): return (T)(value = *(sbyte*)ptr);
                            case sizeof(short): return (T)(value = swap ? Utilities.ByteOrder.Swap(*(short*)ptr) : *(short*)ptr);

                            case sizeof(int):
                            {
                                return floatingPoint
                                    ? (T)(value = swap ? Utilities.ByteOrder.Swap(*(float*)ptr) : *(float*)ptr)
                                    : (T)(value = swap ? Utilities.ByteOrder.Swap(*(int*)ptr) : *(int*)ptr);
                            }

                            case sizeof(long):
                            {
                                return floatingPoint
                                    ? (T)(value = swap ? Utilities.ByteOrder.Swap(*(double*)ptr) : *(double*)ptr)
                                    : (T)(value = swap ? Utilities.ByteOrder.Swap(*(long*)ptr) : *(long*)ptr);
                            }

                            case sizeof(decimal): return (T)(value = swap ? Utilities.ByteOrder.Swap(*(decimal*)ptr) : *(decimal*)ptr);
                        }
                }
            }

            throw new ArgumentException(Utilities.ResourceStrings.ExceptionMessageBufferDeserializeNumeric);
        }

        public bool DeserializeBoolean()
            => InternalDeserializeUnmanaged<byte>(sizeof(bool), unsigned: true) != 0;

        public byte DeserializeByte()
            => InternalDeserializeUnmanaged<byte>(sizeof(byte), unsigned: true);

        public short DeserializeInt16()
            => InternalDeserializeUnmanaged<short>(sizeof(short), unsigned: false);

        public short DeserializeInt16(bool littleEndian)
            => InternalDeserializeUnmanaged<short>(sizeof(short), unsigned: false, littleEndian);

        public float DeserializeSingle()
            => InternalDeserializeUnmanaged<float>(sizeof(float), unsigned: false, floatingPoint: true);

        public double DeserializeDouble()
            => InternalDeserializeUnmanaged<double>(sizeof(double), unsigned: false, floatingPoint: true);

        public decimal DeserializeDecimal()
            => InternalDeserializeUnmanaged<decimal>(sizeof(decimal), unsigned: false, floatingPoint: true);

        public sbyte DeserializeSByte()
            => InternalDeserializeUnmanaged<sbyte>(sizeof(sbyte), unsigned: false);

        public ushort DeserializeUInt16()
            => InternalDeserializeUnmanaged<ushort>(sizeof(ushort), unsigned: true);

        public ushort DeserializeUInt16(bool littleEndian)
            => InternalDeserializeUnmanaged<ushort>(sizeof(ushort), unsigned: true, littleEndian);

        public uint DeserializeUInt32()
            => InternalDeserializeUnmanaged<uint>(sizeof(uint), unsigned: true);

        public uint DeserializeUInt32(bool littleEndian)
            => InternalDeserializeUnmanaged<uint>(sizeof(uint), unsigned: true, littleEndian);

        public ulong DeserializeUInt64()
            => InternalDeserializeUnmanaged<ulong>(sizeof(ulong), unsigned: true);

        public ulong DeserializeUInt64(bool littleEndian)
            => InternalDeserializeUnmanaged<ulong>(sizeof(ulong), unsigned: true, littleEndian);

        public char DeserializeChar()
            => (char)DeserializeCompressedUInt16();

        public int DeserializeInt32()
            => (int)DeserializeCompressedUInt32();
            //=> InternalDeserializeUnmanaged<int>(sizeof(int), unsigned: false);

        public int DeserializeInt32(bool littleEndian)
            => InternalDeserializeUnmanaged<int>(sizeof(int), unsigned: false, littleEndian);

        public int DeserializeUncompressedInt32()
            => InternalDeserializeUnmanaged<int>(sizeof(int), unsigned: false);

        public long DeserializeInt64()
            => (long)DeserializeCompressedUInt64();

        public long DeserializeInt64(bool littleEndian)
            => InternalDeserializeUnmanaged<long>(sizeof(long), unsigned: false, littleEndian);

        public long DeserializeUncompressedInt64()
            => InternalDeserializeUnmanaged<long>(sizeof(long), unsigned: false);

        public Guid DeserializeGuid()
        {
            const int guidSize = 16;
            EnsureCapacity(guidSize, SerializerOperation.Deserialize);
            var value = new Guid(_buffer.AsSpan(_position, guidSize));
            _position += guidSize;
            return value;
        }

        public BitSerializer DeserializeBitSerializer()
            => DeserializeInt64();

        public TimeSpan DeserializeTimeSpan()
            => TimeSpan.FromTicks(DeserializeInt64());

        public DateTime DeserializeDateTime()
            => DateTime.FromBinary(DeserializeUncompressedInt64());

        public DateTimeOffset DeserializeDateTimeOffset()
            => new DateTimeOffset(DeserializeDateTime(), DeserializeTimeSpan());

        public BigInteger DeserializeBigInteger()
        {
            // TODO: Redesign using new BigInteger(ReadOnlySpan<byte> value)
            var bytes = DeserializeBytes();
            return new BigInteger(bytes);
        }

        public T DeserializeEnum<T>() where T : struct, Enum
            => Enum.Parse<T>(DeserializeInt64().ToString(Culture));

        [SerializerMethodIdentifier(SerializerMethodIdentifier.DeserializeUnmanaged)]
        public unsafe T DeserializeUnmanaged<T>() where T : unmanaged
            => InternalDeserializeUnmanaged<T>(sizeof(T), unsigned: false);

        #endregion ValueTypes

        #region ValueTypesTry

        bool TryDeserializeNumeric<T>(out T value, int size, bool unsigned, bool floatingPoint = false, bool? littleEndian = default) where T : struct
        {
            var valueType = default(ValueType);

            unsafe
            {
                if (!(_length - _position < size))
                {
                    fixed (byte* ptr = &_buffer![_position])
                    {
                        _position += size;

                        var isLittleEndian = littleEndian ?? BitConverter.IsLittleEndian;
                        var swap = BitConverter.IsLittleEndian && !isLittleEndian || !BitConverter.IsLittleEndian && isLittleEndian;

                        valueType = unsigned
                            ? (size switch
                            {
                                sizeof(byte) => *(byte*)ptr,
                                sizeof(ushort) => swap ? Utilities.ByteOrder.Swap(*(ushort*)ptr) : *(ushort*)ptr,
                                sizeof(uint) => swap ? Utilities.ByteOrder.Swap(*(uint*)ptr) : *(uint*)ptr,
                                sizeof(ulong) => swap ? Utilities.ByteOrder.Swap(*(ulong*)ptr) : *(ulong*)ptr,
                                _ => throw new InvalidOperationException(Utilities.ResourceStrings.CyxorInternalException)
                            })
                            : (size switch
                            {
                                sizeof(sbyte) => *(sbyte*)ptr,
                                sizeof(short) => swap ? Utilities.ByteOrder.Swap(*(short*)ptr) : *(short*)ptr,
                                sizeof(int) => floatingPoint
                                    ? swap ? Utilities.ByteOrder.Swap(*(float*)ptr) : *(float*)ptr
                                    : (ValueType)(swap ? Utilities.ByteOrder.Swap(*(int*)ptr) : *(int*)ptr),
                                sizeof(long) => floatingPoint
                                        ? swap ? Utilities.ByteOrder.Swap(*(double*)ptr) : *(double*)ptr
                                        : (ValueType)(swap ? Utilities.ByteOrder.Swap(*(long*)ptr) : *(long*)ptr),
                                sizeof(decimal) => swap ? Utilities.ByteOrder.Swap(*(decimal*)ptr) : *(decimal*)ptr,
                                _ => throw new InvalidOperationException(Utilities.ResourceStrings.CyxorInternalException)
                            });
                    }
                }
            }

            value = valueType != default ? (T)valueType : default;
            return valueType != default;
        }

        public bool TryDeserializeByte(out byte value)
            => TryDeserializeNumeric(out value, sizeof(byte), unsigned: true);

        public bool TryDeserializeInt16(out short value)
            => TryDeserializeNumeric(out value, sizeof(short), unsigned: false);

        public bool TryDeserializeInt16(out short value, bool littleEndian)
            => TryDeserializeNumeric(out value, sizeof(short), unsigned: false, littleEndian);

        public bool TryDeserializeSingle(out float value)
            => TryDeserializeNumeric(out value, sizeof(float), unsigned: false);

        public bool TryDeserializeDouble(out double value)
            => TryDeserializeNumeric(out value, sizeof(double), unsigned: false);

        public bool TryDeserializeDecimal(out decimal value)
            => TryDeserializeNumeric(out value, sizeof(decimal), unsigned: false);

        public bool TryDeserializeSByte(out sbyte value)
            => TryDeserializeNumeric(out value, sizeof(sbyte), unsigned: false);

        public bool TryDeserializeUInt16(out ushort value)
            => TryDeserializeNumeric(out value, sizeof(ushort), unsigned: true);

        public bool TryDeserializeUInt16(out ushort value, bool littleEndian)
            => TryDeserializeNumeric(out value, sizeof(ushort), unsigned: true, littleEndian);

        public bool TryDeserializeUInt32(out uint value)
            => TryDeserializeNumeric(out value, sizeof(uint), unsigned: true);

        public bool TryDeserializeUInt32(out uint value, bool littleEndian)
            => TryDeserializeNumeric(out value, sizeof(uint), unsigned: true, littleEndian);

        public bool TryDeserializeUInt64(out ulong value)
            => TryDeserializeNumeric(out value, sizeof(ulong), unsigned: true);

        public bool TryDeserializeUInt64(out ulong value, bool littleEndian)
            => TryDeserializeNumeric(out value, sizeof(ulong), unsigned: true, littleEndian);

        public bool TryDeserializeBoolean(out bool value)
        {
            value = default;

            if (!TryDeserializeByte(out var bValue))
                return false;

            value = bValue != 0;
            return true;
        }

        public bool TryDeserializeChar(out char value)
        {
            value = default;

            if (!TryDeserializeCompressedUInt16(out var uvalue))
                return false;

            value = (char)uvalue;
            return true;
        }

        public bool TryDeserializeInt32(out int value)
        {
            value = default;

            if (!TryDeserializeCompressedUInt32(out var uvalue))
                return false;

            value = (int)uvalue;
            return true;
        }

        public bool TryDeserializeInt32(out int value, bool littleEndian)
            => TryDeserializeNumeric(out value, sizeof(int), unsigned: false, littleEndian);

        public bool TryDeserializeUncompressedInt32(out int value)
            => TryDeserializeNumeric(out value, sizeof(int), unsigned: false);

        public bool TryDeserializeInt64(out long value)
        {
            value = default;

            if (!TryDeserializeCompressedUInt64(out var uvalue))
                return false;

            value = (long)uvalue;
            return true;
        }

        public bool TryDeserializeInt64(out long value, bool littleEndian)
            => TryDeserializeNumeric(out value, sizeof(long), unsigned: false, littleEndian);

        public bool TryDeserializeUncompressedInt64(out long value)
            => TryDeserializeNumeric(out value, sizeof(long), unsigned: false);

        public bool TryDeserializeGuid(out Guid value)
        {
            value = Guid.Empty;

            if (!TryDeserializeBytes(out var bytes, count: 16))
                return false;

            value = new Guid(bytes);
            return true;
        }

        public bool TryDeserializeBitSerializer(out BitSerializer value)
        {
            value = default;

            if (!TryDeserializeInt64(out var lValue))
                return false;

            value = new BitSerializer(lValue);
            return true;
        }

        public bool TryDeserializeTimeSpan(out TimeSpan value)
        {
            value = default;

            if (!TryDeserializeInt64(out var lValue))
                return false;

            value = new TimeSpan(lValue);
            return true;
        }

        public bool TryDeserializeDateTime(out DateTime value)
        {
            value = default;

            if (!TryDeserializeInt64(out var lValue))
                return false;

            value = new DateTime(lValue);
            return true;
        }

        public bool TryDeserializeDateTimeOffset(out DateTimeOffset value)
        {
            value = default;

            if (TryDeserializeDateTime(out var dateTime))
            {
                if (TryDeserializeTimeSpan(out var timeSpan))
                {
                    value = new DateTimeOffset(dateTime, timeSpan);
                    return true;
                }
                else
                    _position -= sizeof(long);
            }

            return false;
        }

        public bool TryDeserializeEnum<TEnum>(out TEnum value) where TEnum : struct, Enum
        {
            value = default;

            if (!TryDeserializeInt64(out var lValue))
                return false;

            var result = Enum.TryParse(lValue.ToString(Culture), out value);

            if (!result)
                _position -= sizeof(long);

            return result;
        }

        #endregion ValueTypesTry
    }
}