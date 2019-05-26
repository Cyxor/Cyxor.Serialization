using System;

namespace Cyxor.Serialization
{
    partial class SerialStream
    {
        #region ValueTypes

        T DeserializeNumeric<T>(int size, bool unsigned, bool floatingPoint = false, bool? littleEndian = default) where T : struct
        {
            EnsureCapacity(size, SerializerOperation.Deserialize);

            var value = default(ValueType);

            unsafe
            {
                fixed (byte* ptr = &buffer![position])
                {
                    position += size;

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
            => DeserializeNumeric<byte>(sizeof(bool), unsigned: true) != 0;

        public byte DeserializeByte()
            => DeserializeNumeric<byte>(sizeof(byte), unsigned: true);

        public short DeserializeInt16()
            => DeserializeNumeric<short>(sizeof(short), unsigned: false);

        public short DeserializeInt16(bool littleEndian)
            => DeserializeNumeric<short>(sizeof(short), unsigned: false, littleEndian);

        public float DeserializeSingle()
            => DeserializeNumeric<float>(sizeof(float), unsigned: false, floatingPoint: true);

        public double DeserializeDouble()
            => DeserializeNumeric<double>(sizeof(double), unsigned: false, floatingPoint: true);

        public decimal DeserializeDecimal()
            => DeserializeNumeric<decimal>(sizeof(decimal), unsigned: false, floatingPoint: true);

        public sbyte DeserializeSByte()
            => DeserializeNumeric<sbyte>(sizeof(sbyte), unsigned: false);

        public ushort DeserializeUInt16()
            => DeserializeNumeric<ushort>(sizeof(ushort), unsigned: true);

        public ushort DeserializeUInt16(bool littleEndian)
            => DeserializeNumeric<ushort>(sizeof(ushort), unsigned: true, littleEndian);

        public uint DeserializeUInt32()
            => DeserializeNumeric<uint>(sizeof(uint), unsigned: true);

        public uint DeserializeUInt32(bool littleEndian)
            => DeserializeNumeric<uint>(sizeof(uint), unsigned: true, littleEndian);

        public ulong DeserializeUInt64()
            => DeserializeNumeric<ulong>(sizeof(ulong), unsigned: true);

        public ulong DeserializeUInt64(bool littleEndian)
            => DeserializeNumeric<ulong>(sizeof(ulong), unsigned: true, littleEndian);

        public char DeserializeChar()
            => (char)DeserializeCompressedUInt16();

        public int DeserializeInt32()
            => (int)DeserializeCompressedUInt32();

        public int DeserializeInt32(bool littleEndian)
            => DeserializeNumeric<int>(sizeof(int), unsigned: false, littleEndian);

        public int DeserializeUncompressedInt32()
            => DeserializeNumeric<int>(sizeof(int), unsigned: false);

        public long DeserializeInt64()
            => (long)DeserializeCompressedUInt64();

        public long DeserializeInt64(bool littleEndian)
            => DeserializeNumeric<long>(sizeof(long), unsigned: false, littleEndian);

        public long DeserializeUncompressedInt64()
            => DeserializeNumeric<long>(sizeof(long), unsigned: false);

        public Guid DeserializeGuid()
        {
            EnsureCapacity(16, SerializerOperation.Deserialize);
            return new Guid(DeserializeBytes(count: 16)!);
        }

        public BitSerializer DeserializeBitSerializer()
            => DeserializeInt64();

        public TimeSpan DeserializeTimeSpan()
            => new TimeSpan(DeserializeInt64());

        public DateTime DeserializeDateTime()
            => new DateTime(DeserializeInt64());

        public DateTimeOffset DeserializeDateTimeOffset()
            => new DateTimeOffset(DeserializeDateTime(), DeserializeTimeSpan());

        public T DeserializeEnum<T>() where T : struct, Enum
#if !NET20 && !NET35 && !NET40 && !NET45 && !NETSTANDARD1_0 && !NETSTANDARD1_3 && !NETSTANDARD2_0
            => Enum.Parse<T>(DeserializeInt64().ToString(Culture));
#else
            => (T)Enum.Parse(typeof(T), DeserializeInt64().ToString(Culture));
#endif

        #endregion ValueTypes

        #region ValueTypesTry

        bool TryDeserializeNumeric<T>(out T value, int size, bool unsigned, bool floatingPoint = false, bool? littleEndian = default) where T : struct
        {
            var valueType = default(ValueType);

            unsafe
            {
                if (!(length - position < size))
                {
                    fixed (byte* ptr = &buffer![position])
                    {
                        position += size;

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
                    position -= sizeof(long);
            }

            return false;
        }

        public bool TryDeserializeEnum<TEnum>(out TEnum value) where TEnum : struct
        {
            value = default;

            if (!TryDeserializeInt64(out var lValue))
                return false;

#if NET20 || NET35
            var result = Enum.IsDefined(typeof(TEnum), lValue);
            value = (TEnum)Enum.ToObject(typeof(TEnum), lValue);
#else
            var result = Enum.TryParse(lValue.ToString(Culture), out value);
#endif

            if (!result)
                position -= sizeof(long);

            return result;
        }

        #endregion ValueTypesTry
    }
}