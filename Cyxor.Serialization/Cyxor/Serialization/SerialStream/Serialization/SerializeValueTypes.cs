using System;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

            fixed (void* ptr = &_buffer![_position])
                span = new Span<T>(ptr, 1);

            span[0] = value;

            _position += size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void InternalSerializeUnmanaged<T>(T value, bool? littleEndian = null) where T : unmanaged
        {
            var size = sizeof(T);

            EnsureCapacity(size, SerializerOperation.Serialize);

            // NOTE: value assigned to littleEndian is used to determine if byte order swapping is needed

            if (littleEndian != null)
                littleEndian = IsLittleEndian && !littleEndian.Value || !IsLittleEndian && littleEndian.Value;

            if (Options.ReverseByteOrder)
            {
                if (littleEndian == null)
                {
                    var tType = typeof(T);

                    if (tType == typeof(short) || tType == typeof(ushort)
                        || tType == typeof(int) || tType == typeof(uint)
                        || tType == typeof(long) || tType == typeof(ulong))
                        littleEndian = true;
                }
                else
                    littleEndian = !littleEndian;
            }

            if (littleEndian ?? false)
                value = Utilities.ByteOrder.Swap(value);

            if (_stream != null)
                _stream.Write(new ReadOnlySpan<byte>(&value, size));
            else
            {
                Span<T> span;

                fixed (void* ptr = &_buffer![_position])
                    span = new Span<T>(ptr, 1);

                span[0] = value;
            }

            _position += size;
        }

        #region Unmanaged

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(bool value)
            => InternalSerializeUnmanaged(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(byte value)
            => InternalSerializeUnmanaged(value);

        public void Serialize(char value)
            => InternalSerializeUnmanaged(value);

        public void Serialize(short value)
            => InternalSerializeUnmanaged(value);

        public void Serialize(short value, bool littleEndian)
            => InternalSerializeUnmanaged(value, littleEndian);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(int value)
            => SerializeCompressedInt((uint)value);

        public void SerializeUncompressedInt32(int value)
            => InternalSerializeUnmanaged(value);

        public void SerializeUncompressedInt32(int value, bool littleEndian)
            => InternalSerializeUnmanaged(value, littleEndian);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        #endregion Unmanaged

        #region Struct

        public void Serialize(Guid value)
        {
            const int guidSize = 16;
            EnsureCapacity(guidSize, SerializerOperation.Serialize);
            _ = value.TryWriteBytes(_buffer.AsSpan(_position, guidSize));
            _position += guidSize;
        }

        public void Serialize(BitSerializer value)
            => Serialize((long)value);

        public void Serialize(TimeSpan value)
            => Serialize(value.Ticks);

        public void Serialize(DateTime value)
            => SerializeUncompressedInt64(value.ToBinary());

        public void Serialize(DateTimeOffset value)
        {
            Serialize(value.DateTime);
            Serialize(value.Offset);
        }

        public void Serialize(BigInteger value)
        {
            // TODO: Redesign using value.TryWriteBytes
            var bytes = value.ToByteArray();
            Serialize(bytes);
        }

        public void SerializeEnum<T>(T value) where T : struct, Enum
            => Serialize(Convert.ToInt64(value, Culture));

        #endregion Struct
    }
}