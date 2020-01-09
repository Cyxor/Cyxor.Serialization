using System;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using Cyxor.Extensions;

using System.Buffers;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        [SerializerMethodIdentifier(SerializerMethodIdentifier.SerializeUnmanaged)]
        public unsafe void SerializeUnmanaged<T>(T value) where T : unmanaged
        {
            var size = sizeof(T);

            EnsureCapacity(size, SerializerOperation.Serialize);

            Span<T> span;

            fixed (void* ptr = &buffer![position])
                span = new Span<T>(ptr, 1);

            span[0] = value;

            position += size;
        }

        unsafe void InternalSerializeUnmanaged<T>(T value, bool? littleEndian = default) where T : unmanaged
        {            
            var size = sizeof(T);

            EnsureCapacity(size, SerializerOperation.Serialize);

            Span<T> span;

            fixed (void* ptr = &buffer![position])
                span = new Span<T>(ptr, 1);

            var isLittleEndian = littleEndian ?? IsLittleEndian;
            var swap = IsLittleEndian && !isLittleEndian || !IsLittleEndian && isLittleEndian;

            span[0] = swap ? Utilities.ByteOrder.Swap(value) : value;

            position += size;
        }

        #region Numeric

        public void Serialize(bool value)
            => InternalSerializeUnmanaged(value);

        public void Serialize(byte value)
            => InternalSerializeUnmanaged(value);

        public void Serialize(char value)
            => InternalSerializeUnmanaged(value);

        public void Serialize(short value)
            => InternalSerializeUnmanaged(value);

        public void Serialize(short value, bool littleEndian)
            => InternalSerializeUnmanaged(value, littleEndian);

        public void Serialize(int value)
            => SerializeCompressedInt((uint)value);

        public void SerializeUncompressedInt32(int value)
            => InternalSerializeUnmanaged(value);

        public void SerializeUncompressedInt32(int value, bool littleEndian)
            => InternalSerializeUnmanaged(value, littleEndian);

        public void Serialize(long value)
             => SerializeCompressedInt((ulong)value);

        public void SerializeUncompressedInt64(long value)
            => InternalSerializeUnmanaged(value);

        public void SerializeUncompressedInt64(long value, bool littleEndian)
            => InternalSerializeUnmanaged(value, littleEndian);

        public void Serialize(float value)
            => InternalSerializeUnmanaged(value);

        public void Serialize(double value)
            => InternalSerializeUnmanaged(value);

        public void Serialize(decimal value)
            => InternalSerializeUnmanaged(value);

        public void Serialize(sbyte value)
            => InternalSerializeUnmanaged(value);

        public void Serialize(ushort value)
            => InternalSerializeUnmanaged(value);

        public void Serialize(ushort value, bool littleEndian)
            => InternalSerializeUnmanaged(value, littleEndian);

        public void Serialize(uint value)
            => InternalSerializeUnmanaged(value);

        public void Serialize(uint value, bool littleEndian)
            => InternalSerializeUnmanaged(value, littleEndian);

        public void Serialize(ulong value)
            => InternalSerializeUnmanaged(value);

        public void Serialize(ulong value, bool littleEndian)
            => InternalSerializeUnmanaged(value, littleEndian);

        #endregion Numeric

        #region Struct

        public void Serialize(Guid value)
            => SerializeRaw(value.ToByteArray());

        public void Serialize(BitSerializer value)
            => Serialize((long)value);

        public void Serialize(TimeSpan value)
            => Serialize(value.Ticks);

        public void Serialize(DateTime value)
            => Serialize(value.Ticks);

        public void Serialize(DateTimeOffset value)
        {
            Serialize(value.DateTime);
            Serialize(value.Offset);
        }

        public void SerializeEnum<T>(T value) where T : struct, Enum
            => Serialize(Convert.ToInt64(value, Culture));

        #endregion Struct
    }
}