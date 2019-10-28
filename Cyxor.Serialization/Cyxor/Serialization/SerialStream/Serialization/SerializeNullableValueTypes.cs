using System;

#if !NET20 && !NET35 && !NET40
using System.Reflection;
#endif

namespace Cyxor.Serialization
{
#if NET20 || NET35 || NET40
    using Extensions;
#endif

    partial class SerialStream
    {
        delegate void SerializeSignature<T>(T value) where T : struct;

        delegate void SerializeSignatureLittleEndian<T>(T value, bool littleEndian) where T : struct;

        void SerializeNullableValue<T>(T? value, SerializeSignature<T> serializeDelegate) where T : struct
        {
            if (value == null)
                Serialize(false);
            else
            {
                Serialize(true);
                serializeDelegate((T)value);
            }
        }

        void SerializeNullableValue<T>(T? value, bool littleEndian, SerializeSignatureLittleEndian<T> serializeDelegate) where T : struct
        {
            if (value == null)
                Serialize(false);
            else
            {
                Serialize(true);
                serializeDelegate((T)value, littleEndian);
            }
        }

        public void Serialize(bool? value)
            => Serialize((byte)(value == default ? 0 : value == true ? 1 : 2));

        public void Serialize(byte? value)
            => SerializeNullableValue(value, Serialize);

        public void Serialize(short? value)
            => SerializeNullableValue(value, Serialize);

        public void Serialize(short? value, bool littleEndian)
            => SerializeNullableValue(value, littleEndian, Serialize);

        public void Serialize(float? value)
            => SerializeNullableValue(value, Serialize);

        public void Serialize(double? value)
            => SerializeNullableValue(value, Serialize);

        public void Serialize(decimal? value)
            => SerializeNullableValue(value, Serialize);

        public void Serialize(sbyte? value)
            => SerializeNullableValue(value, Serialize);

        public void Serialize(ushort? value)
            => SerializeNullableValue(value, Serialize);

        public void Serialize(ushort? value, bool littleEndian)
            => SerializeNullableValue(value, littleEndian, Serialize);

        public void Serialize(uint? value)
            => SerializeNullableValue(value, Serialize);

        public void Serialize(uint? value, bool littleEndian)
            => SerializeNullableValue(value, littleEndian, Serialize);

        public void Serialize(ulong? value)
            => SerializeNullableValue(value, Serialize);

        public void Serialize(ulong? value, bool littleEndian)
            => SerializeNullableValue(value, littleEndian, Serialize);

        public void Serialize(char? value)
            => SerializeNullableValue(value, Serialize);

        public void Serialize(int? value)
            => SerializeNullableValue(value, Serialize);

        public void Serialize(int? value, bool littleEndian)
            => SerializeNullableValue(value, littleEndian, Serialize);

        public void SerializeUncompressedNullableInt32(int? value)
            => SerializeNullableValue(value, SerializeUncompressedInt32);

        public void Serialize(long? value)
            => SerializeNullableValue(value, Serialize);

        public void Serialize(long? value, bool littleEndian)
            => SerializeNullableValue(value, littleEndian, Serialize);

        public void SerializeUncompressedNullableInt64(long? value)
            => SerializeNullableValue(value, SerializeUncompressedInt64);

        public void Serialize(Guid? value)
            => SerializeNullableValue(value, Serialize);

        public void Serialize(BitSerializer? value)
            => SerializeNullableValue(value, Serialize);

        public void Serialize(TimeSpan? value)
            => SerializeNullableValue(value, Serialize);

        public void Serialize(DateTime? value)
            => SerializeNullableValue(value, Serialize);

        public void Serialize(DateTimeOffset? value)
            => SerializeNullableValue(value, Serialize);

        public void SerializeNullableEnum<T>(T? value) where T : struct, Enum
        {
            Serialize(value != null ? false : true);
            Serialize(Convert.ToInt64(value, Culture));
        }

        public void Serialize<T>(T? value) where T : struct
            => SerializeNullableValue(value, Serialize);
    }
}