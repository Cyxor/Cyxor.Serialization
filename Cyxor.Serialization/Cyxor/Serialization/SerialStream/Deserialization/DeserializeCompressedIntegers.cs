using System;

namespace Cyxor.Serialization
{
    partial class SerializationStream
    {
        #region CompressedIntegers

        public ulong DeserializeCompressedInt(int size, bool isSigned)
        {
            ulong val1 = 0;
            var val2 = 0;
            byte val3;

            var bitVal = size == sizeof(short) ? 21 : size == sizeof(int) ? 35 : size == sizeof(long) ? 63 : 0;

            if (bitVal == 0)
                throw new ArgumentException(Utilities.ResourceStrings.ExceptionMessageBufferDeserializeNumeric);

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

        public short DeserializeCompressedInt16()
            => (short)DeserializeCompressedInt(sizeof(short), isSigned: true);

        public ushort DeserializeCompressedUInt16()
            => (ushort)DeserializeCompressedInt(sizeof(ushort), isSigned: false);

        public int DeserializeCompressedInt32()
            => (int)DeserializeCompressedInt(sizeof(int), isSigned: true);

        public uint DeserializeCompressedUInt32()
            => (uint)DeserializeCompressedInt(sizeof(uint), isSigned: false);

        public long DeserializeCompressedInt64()
            => (long)DeserializeCompressedInt(sizeof(long), isSigned: true);

        public ulong DeserializeCompressedUInt64()
            => DeserializeCompressedInt(sizeof(ulong), isSigned: false);

        #endregion CompressedIntegers

        #region CompressedIntegersTry

        bool TryDeserializeCompressedInt<T>(out T value, int size, bool signed) where T : struct
        {
            var result = false;
            var startPosition = position;

            ulong val1 = 0;
            var val2 = 0;

            var bitVal = size == sizeof(short) ? 21 : size == sizeof(int) ? 35 : size == sizeof(long) ? 63 : 0;

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

            value = signed
                ? (size switch
                {
                    2 => (T)(ValueType)(short)val1,
                    4 => (T)(ValueType)(int)val1,
                    _ => (T)(ValueType)(long)val1,
                })
                : (size switch
                {
                    2 => (T)(ValueType)(ushort)val1,
                    4 => (T)(ValueType)(uint)val1,
                    _ => (T)(ValueType)val1,
                });

            position = result ? position : startPosition;
            return result;
        }

        public bool TryDeserializeCompressedUInt16(out ushort value)
            => TryDeserializeCompressedInt(out value, sizeof(ushort), signed: false);

        public bool TryDeserializeCompressedUInt32(out uint value)
            => TryDeserializeCompressedInt(out value, sizeof(uint), signed: false);

        public bool TryDeserializeCompressedUInt64(out ulong value)
            => TryDeserializeCompressedInt(out value, sizeof(ulong), signed: false);

        public bool TryDeserializeCompressedInt16(out short value)
            => TryDeserializeCompressedInt(out value, sizeof(short), signed: true);

        public bool TryDeserializeCompressedInt32(out int value)
            => TryDeserializeCompressedInt(out value, sizeof(int), signed: true);

        public bool TryDeserializeCompressedInt64(out long value)
            => TryDeserializeCompressedInt(out value, sizeof(long), signed: true);

        #endregion CompressedIntegersTry
    }
}