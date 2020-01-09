using System;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        #region ValueTypesNullable

        delegate T DeserializeSignature<T>();

        T? DeserializeNullableNumeric<T>(DeserializeSignature<T> deserializeDelegate) where T : struct
            => !DeserializeBoolean() ? default(T?) : deserializeDelegate();

        delegate T DeserializeSignatureLittleEndian<T>(bool littleEndian);

        T? DeserializeNullableNumeric<T>(bool littleEndian, DeserializeSignatureLittleEndian<T> deserializeDelegate) where T : struct
            => !DeserializeBoolean() ? default(T?) : deserializeDelegate(littleEndian);

        public bool? DeserializeNullableBoolean()
        {
            var value = DeserializeByte();

            return value == 0 ? default
                : value == 1 ? true
                : value == 2 ? false
                : throw DataException();
        }

        public byte? DeserializeNullableByte()
            => DeserializeNullableNumeric(DeserializeByte);

        public short? DeserializeNullableInt16()
            => DeserializeNullableNumeric(DeserializeInt16);

        public short? DeserializeNullableInt16(bool littleEndian)
            => DeserializeNullableNumeric(littleEndian, DeserializeInt16);

        public float? DeserializeNullableSingle()
            => DeserializeNullableNumeric(DeserializeSingle);

        public double? DeserializeNullableDouble()
            => DeserializeNullableNumeric(DeserializeDouble);

        public decimal? DeserializeNullableDecimal()
            => DeserializeNullableNumeric(DeserializeDecimal);

        public sbyte? DeserializeNullableSByte()
            => DeserializeNullableNumeric(DeserializeSByte);

        public ushort? DeserializeNullableUInt16()
            => DeserializeNullableNumeric(DeserializeUInt16);

        public ushort? DeserializeNullableUInt16(bool littleEndian)
            => DeserializeNullableNumeric(littleEndian, DeserializeUInt16);

        public uint? DeserializeNullableUInt32()
            => DeserializeNullableNumeric(DeserializeUInt32);

        public uint? DeserializeNullableUInt32(bool littleEndian)
            => DeserializeNullableNumeric(littleEndian, DeserializeUInt32);

        public ulong? DeserializeNullableUInt64()
            => DeserializeNullableNumeric(DeserializeUInt64);

        public ulong? DeserializeNullableUInt64(bool littleEndian)
            => DeserializeNullableNumeric(littleEndian, DeserializeUInt64);

        public char? DeserializeNullableChar()
            => DeserializeNullableNumeric(DeserializeChar);

        public int? DeserializeNullableInt32()
            => DeserializeNullableNumeric(DeserializeInt32);

        public int? DeserializeNullableInt32(bool littleEndian)
            => DeserializeNullableNumeric(littleEndian, DeserializeInt32);

        public int? DeserializeUncompressedNullableInt32()
            => DeserializeNullableNumeric(DeserializeUncompressedInt32);

        public long? DeserializeNullableInt64()
            => DeserializeNullableNumeric(DeserializeInt64);

        public long? DeserializeNullableInt64(bool littleEndian)
            => DeserializeNullableNumeric(littleEndian, DeserializeInt64);

        public long? DeserializeUncompressedNullableInt64()
            => DeserializeNullableNumeric(DeserializeUncompressedInt64);

        public Guid? DeserializeNullableGuid()
            => DeserializeNullableNumeric(DeserializeGuid);

        public BitSerializer? DeserializeNullableBitSerializer()
            => DeserializeNullableNumeric(DeserializeBitSerializer);

        public TimeSpan? DeserializeNullableTimeSpan()
            => DeserializeNullableNumeric(DeserializeTimeSpan);

        public DateTime? DeserializeNullableDateTime()
            => DeserializeNullableNumeric(DeserializeDateTime);

        public DateTimeOffset? DeserializeNullableDateTimeOffset()
            => DeserializeNullableNumeric(DeserializeDateTimeOffset);

        public T? DeserializeNullableEnum<T>() where T : struct, Enum
            => DeserializeNullableNumeric(DeserializeEnum<T>);

        public T? DeserializeNullableT<T>() where T : struct
            => DeserializeNullableNumeric(DeserializeObject<T>);

        #endregion ValueTypesNullable

        #region ValueTypesNullableTry

        delegate bool TryDeserializeSignature<T>(out T value);

        bool TryDeserializeNullableNumeric<T>(out T? value, TryDeserializeSignature<T> tryDeserializeDelegate) where T : struct
        {
            bool result;
            value = default;

            if (result = TryDeserializeBoolean(out var notNull))
                if (notNull)
                    if (result = tryDeserializeDelegate(out var nonNullableValue))
                        value = nonNullableValue;

            return result;
        }

        delegate bool TryDeserializeSignatureLittleEndian<T>(out T value, bool littleEndian);

        bool TryDeserializeNullableNumeric<T>(out T? value, bool littleEndian, TryDeserializeSignatureLittleEndian<T> tryDeserializeDelegate) where T : struct
        {
            bool result;
            value = default;

            if (result = TryDeserializeBoolean(out var notNull))
                if (notNull)
                    if (result = tryDeserializeDelegate(out var nonNullableValue, littleEndian))
                        value = nonNullableValue;

            return result;
        }

        public bool TryDeserializeNullableBoolean(out bool? value)
            => TryDeserializeNullableNumeric(out value, TryDeserializeBoolean);

        public bool TryDeserializeNullableByte(out byte? value)
            => TryDeserializeNullableNumeric(out value, TryDeserializeByte);

        public bool TryDeserializeNullableInt16(out short? value)
            => TryDeserializeNullableNumeric(out value, TryDeserializeInt16);

        public bool TryDeserializeNullableInt16(out short? value, bool littleEndian)
            => TryDeserializeNullableNumeric(out value, littleEndian, TryDeserializeInt16);

        public bool TryDeserializeNullableSingle(out float? value)
            => TryDeserializeNullableNumeric(out value, TryDeserializeSingle);

        public bool TryDeserializeNullableDouble(out double? value)
            => TryDeserializeNullableNumeric(out value, TryDeserializeDouble);

        public bool TryDeserializeNullableDecimal(out decimal? value)
            => TryDeserializeNullableNumeric(out value, TryDeserializeDecimal);

        public bool TryDeserializeNullableSByte(out sbyte? value)
            => TryDeserializeNullableNumeric(out value, TryDeserializeSByte);

        public bool TryDeserializeNullableUInt16(out ushort? value)
            => TryDeserializeNullableNumeric(out value, TryDeserializeUInt16);

        public bool TryDeserializeNullableUInt16(out ushort? value, bool littleEndian)
            => TryDeserializeNullableNumeric(out value, littleEndian, TryDeserializeUInt16);

        public bool TryDeserializeNullableUInt32(out uint? value)
            => TryDeserializeNullableNumeric(out value, TryDeserializeUInt32);

        public bool TryDeserializeNullableUInt32(out uint? value, bool littleEndian)
            => TryDeserializeNullableNumeric(out value, littleEndian, TryDeserializeUInt32);

        public bool TryDeserializeNullableUInt64(out ulong? value)
            => TryDeserializeNullableNumeric(out value, TryDeserializeUInt64);

        public bool TryDeserializeNullableUInt64(out ulong? value, bool littleEndian)
            => TryDeserializeNullableNumeric(out value, littleEndian, TryDeserializeUInt64);

        public bool TryDeserializeNullableChar(out char? value)
            => TryDeserializeNullableNumeric(out value, TryDeserializeChar);

        public bool TryDeserializeNullableInt32(out int? value)
            => TryDeserializeNullableNumeric(out value, TryDeserializeInt32);

        public bool TryDeserializeNullableInt32(out int? value, bool littleEndian)
            => TryDeserializeNullableNumeric(out value, littleEndian, TryDeserializeInt32);

        public bool TryDeserializeUncompressedNullableInt32(out int? value)
            => TryDeserializeNullableNumeric(out value, TryDeserializeUncompressedInt32);

        public bool TryDeserializeNullableInt64(out long? value)
            => TryDeserializeNullableNumeric(out value, TryDeserializeInt64);

        public bool TryDeserializeNullableInt64(out long? value, bool littleEndian)
            => TryDeserializeNullableNumeric(out value, littleEndian, TryDeserializeInt64);

        public bool TryDeserializeUncompressedNullableInt64(out long? value)
            => TryDeserializeNullableNumeric(out value, TryDeserializeUncompressedInt64);

        public bool TryDeserializeNullableGuid(out Guid? value)
            => TryDeserializeNullableNumeric(out value, TryDeserializeGuid);

        public bool TryDeserializeNullableBitSerializer(out BitSerializer? value)
            => TryDeserializeNullableNumeric(out value, TryDeserializeBitSerializer);

        public bool TryDeserializeNullableTimeSpan(out TimeSpan? value)
            => TryDeserializeNullableNumeric(out value, TryDeserializeTimeSpan);

        public bool TryDeserializeNullableDateTime(out DateTime? value)
            => TryDeserializeNullableNumeric(out value, TryDeserializeDateTime);

        public bool TryDeserializeNullableDateTimeOffset(out DateTimeOffset? value)
            => TryDeserializeNullableNumeric(out value, TryDeserializeDateTimeOffset);

        public bool TryDeserializeNullableEnum<T>(out T? value) where T : struct, Enum
            => TryDeserializeNullableNumeric(out value, TryDeserializeEnum);

        #endregion ValueTypesNullableTry
    }
}