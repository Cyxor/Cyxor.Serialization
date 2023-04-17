using System.Numerics;
using System.Runtime.CompilerServices;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        #region Deserialize nullable value types

        #region Internal

        delegate T DeserializeSignature<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        T? DeserializeNullableUnmanaged<T>(DeserializeSignature<T> deserializeDelegate)
            where T : struct => !DeserializeBoolean() ? default(T?) : deserializeDelegate();

        delegate T DeserializeSignatureLittleEndian<T>(bool littleEndian);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        T? DeserializeNullableUnmanaged<T>(bool littleEndian, DeserializeSignatureLittleEndian<T> deserializeDelegate)
            where T : struct => !DeserializeBoolean() ? default(T?) : deserializeDelegate(littleEndian);

        #endregion Internal

        #region Primitive types

        public bool? DeserializeNullableBoolean()
        {
            var value = (sbyte)DeserializeByte();
            return value == -1 ? default(bool?) : value != 0;
        }

        public char? DeserializeNullableChar() => DeserializeNullableUnmanaged(DeserializeChar);

        public float? DeserializeNullableSingle() => DeserializeNullableUnmanaged(DeserializeSingle);

        public double? DeserializeNullableDouble() => DeserializeNullableUnmanaged(DeserializeDouble);

        public byte? DeserializeNullableByte() => DeserializeNullableUnmanaged(DeserializeByte);

        public sbyte? DeserializeNullableSByte() => DeserializeNullableUnmanaged(DeserializeSByte);

        public short? DeserializeNullableInt16() => DeserializeNullableUnmanaged(DeserializeInt16);

        public short? DeserializeNullableInt16(bool littleEndian) =>
            DeserializeNullableUnmanaged(littleEndian, DeserializeInt16);

        public ushort? DeserializeNullableUInt16() => DeserializeNullableUnmanaged(DeserializeUInt16);

        public ushort? DeserializeNullableUInt16(bool littleEndian) =>
            DeserializeNullableUnmanaged(littleEndian, DeserializeUInt16);

        public int? DeserializeNullableInt32() => DeserializeNullableUnmanaged(DeserializeInt32);

        public int? DeserializeNullableUncompressedInt32() =>
            DeserializeNullableUnmanaged(DeserializeUncompressedInt32);

        public int? DeserializeNullableUncompressedInt32(bool littleEndian) =>
            DeserializeNullableUnmanaged(littleEndian, DeserializeUncompressedInt32);

        public uint? DeserializeNullableUInt32() => DeserializeNullableUnmanaged(DeserializeUInt32);

        public uint? DeserializeNullableUInt32(bool littleEndian) =>
            DeserializeNullableUnmanaged(littleEndian, DeserializeUInt32);

        public long? DeserializeNullableInt64() => DeserializeNullableUnmanaged(DeserializeInt64);

        public long? DeserializeNullableUncompressedInt64() =>
            DeserializeNullableUnmanaged(DeserializeUncompressedInt64);

        public long? DeserializeNullableUncompressedInt64(bool littleEndian) =>
            DeserializeNullableUnmanaged(littleEndian, DeserializeUncompressedInt64);

        public ulong? DeserializeNullableUInt64() => DeserializeNullableUnmanaged(DeserializeUInt64);

        public ulong? DeserializeNullableUInt64(bool littleEndian) =>
            DeserializeNullableUnmanaged(littleEndian, DeserializeUInt64);

        #endregion Primitive types

        #region Struct types

        public decimal? DeserializeNullableDecimal() => DeserializeNullableUnmanaged(DeserializeDecimal);

        public BitSerializer? DeserializeNullableBitSerializer() =>
            DeserializeNullableUnmanaged(DeserializeBitSerializer);

        public Guid? DeserializeNullableGuid() => DeserializeNullableUnmanaged(DeserializeGuid);

        public TimeSpan? DeserializeNullableTimeSpan() => DeserializeNullableUnmanaged(DeserializeTimeSpan);

        public DateTime? DeserializeNullableDateTime() => DeserializeNullableUnmanaged(DeserializeDateTime);

        public DateTimeOffset? DeserializeNullableDateTimeOffset() =>
            DeserializeNullableUnmanaged(DeserializeDateTimeOffset);

        public BigInteger? DeserializeNullableBigInteger() => DeserializeNullableUnmanaged(DeserializeBigInteger);

        public T? DeserializeNullableEnum<T>()
            where T : unmanaged, Enum => DeserializeNullableUnmanaged(DeserializeEnum<T>);

        public T? DeserializeNullableUnmanaged<T>()
            where T : unmanaged => DeserializeNullableUnmanaged(DeserializeUnmanaged<T>);

        public T? DeserializeNullableValue<T>()
            where T : struct => DeserializeNullableUnmanaged(DeserializeObject<T>);

        #endregion Struct types

        #endregion Deserialize nullable value types

        #region Try deserialize nullable value types

        #region Internal

        delegate bool TryDeserializeSignature<T>(out T value);

        bool TryDeserializeNullableUnmanaged<T>(out T? value, TryDeserializeSignature<T> tryDeserializeDelegate)
            where T : struct
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

        bool TryDeserializeNullableUnmanaged<T>(
            out T? value,
            bool littleEndian,
            TryDeserializeSignatureLittleEndian<T> tryDeserializeDelegate
        )
            where T : struct
        {
            bool result;
            value = default;

            if (result = TryDeserializeBoolean(out var notNull))
                if (notNull)
                    if (result = tryDeserializeDelegate(out var nonNullableValue, littleEndian))
                        value = nonNullableValue;

            return result;
        }

        #endregion Internal

        #region Primitive types

        public bool TryDeserializeNullableBoolean(out bool? value) =>
            TryDeserializeNullableUnmanaged(out value, TryDeserializeBoolean);

        public bool TryDeserializeNullableChar(out char? value) =>
            TryDeserializeNullableUnmanaged(out value, TryDeserializeChar);

        public bool TryDeserializeNullableSingle(out float? value) =>
            TryDeserializeNullableUnmanaged(out value, TryDeserializeSingle);

        public bool TryDeserializeNullableDouble(out double? value) =>
            TryDeserializeNullableUnmanaged(out value, TryDeserializeDouble);

        public bool TryDeserializeNullableByte(out byte? value) =>
            TryDeserializeNullableUnmanaged(out value, TryDeserializeByte);

        public bool TryDeserializeNullableSByte(out sbyte? value) =>
            TryDeserializeNullableUnmanaged(out value, TryDeserializeSByte);

        public bool TryDeserializeNullableInt16(out short? value) =>
            TryDeserializeNullableUnmanaged(out value, TryDeserializeInt16);

        public bool TryDeserializeNullableInt16(out short? value, bool littleEndian) =>
            TryDeserializeNullableUnmanaged(out value, littleEndian, TryDeserializeInt16);

        public bool TryDeserializeNullableUInt16(out ushort? value) =>
            TryDeserializeNullableUnmanaged(out value, TryDeserializeUInt16);

        public bool TryDeserializeNullableUInt16(out ushort? value, bool littleEndian) =>
            TryDeserializeNullableUnmanaged(out value, littleEndian, TryDeserializeUInt16);

        public bool TryDeserializeNullableInt32(out int? value) =>
            TryDeserializeNullableUnmanaged(out value, TryDeserializeInt32);

        public bool TryDeserializeNullableUncompressedInt32(out int? value) =>
            TryDeserializeNullableUnmanaged(out value, TryDeserializeUncompressedInt32);

        public bool TryDeserializeNullableUncompressedInt32(out int? value, bool littleEndian) =>
            TryDeserializeNullableUnmanaged(out value, littleEndian, TryDeserializeUncompressedInt32);

        public bool TryDeserializeNullableUInt32(out uint? value) =>
            TryDeserializeNullableUnmanaged(out value, TryDeserializeUInt32);

        public bool TryDeserializeNullableUInt32(out uint? value, bool littleEndian) =>
            TryDeserializeNullableUnmanaged(out value, littleEndian, TryDeserializeUInt32);

        public bool TryDeserializeNullableInt64(out long? value) =>
            TryDeserializeNullableUnmanaged(out value, TryDeserializeInt64);

        public bool TryDeserializeUncompressedNullableInt64(out long? value) =>
            TryDeserializeNullableUnmanaged(out value, TryDeserializeUncompressedInt64);

        public bool TryDeserializeUncompressedNullableInt64(out long? value, bool littleEndian) =>
            TryDeserializeNullableUnmanaged(out value, littleEndian, TryDeserializeUncompressedInt64);

        public bool TryDeserializeNullableUInt64(out ulong? value) =>
            TryDeserializeNullableUnmanaged(out value, TryDeserializeUInt64);

        public bool TryDeserializeNullableUInt64(out ulong? value, bool littleEndian) =>
            TryDeserializeNullableUnmanaged(out value, littleEndian, TryDeserializeUInt64);

        #endregion Primitive types

        #region Struct types

        public bool TryDeserializeNullableDecimal(out decimal? value) =>
            TryDeserializeNullableUnmanaged(out value, TryDeserializeDecimal);

        public bool TryDeserializeNullableBitSerializer(out BitSerializer? value) =>
            TryDeserializeNullableUnmanaged(out value, TryDeserializeBitSerializer);

        public bool TryDeserializeNullableGuid(out Guid? value) =>
            TryDeserializeNullableUnmanaged(out value, TryDeserializeGuid);

        public bool TryDeserializeNullableTimeSpan(out TimeSpan? value) =>
            TryDeserializeNullableUnmanaged(out value, TryDeserializeTimeSpan);

        public bool TryDeserializeNullableDateTime(out DateTime? value) =>
            TryDeserializeNullableUnmanaged(out value, TryDeserializeDateTime);

        public bool TryDeserializeNullableDateTimeOffset(out DateTimeOffset? value) =>
            TryDeserializeNullableUnmanaged(out value, TryDeserializeDateTimeOffset);

        public bool TryDeserializeNullableBigInteger(out BigInteger? value) =>
            TryDeserializeNullableUnmanaged(out value, TryDeserializeBigInteger);

        public bool TryDeserializeNullableEnum<T>(out T? value)
            where T : unmanaged, Enum => TryDeserializeNullableUnmanaged(out value, TryDeserializeEnum);

        public bool TryDeserializeNullableUnmanaged<T>(out T? value)
            where T : unmanaged => TryDeserializeNullableUnmanaged(out value, TryDeserializeUnmanaged);

        public bool TryDeserializeNullableValue<T>(out T? value)
            where T : struct => TryDeserializeNullableUnmanaged(out value, TryDeserializeObject);
        #endregion Struct types

        #endregion Try deserialize nullable value types
    }
}
