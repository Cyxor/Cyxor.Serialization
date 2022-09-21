using System;
using System.Runtime.CompilerServices;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeCompressedInt(short value) =>
            SerializeCompressedInt((ulong)((value << 1) ^ (value >> 15)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeCompressedInt(int value) => SerializeCompressedInt((ulong)((value << 1) ^ (value >> 31)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeCompressedInt(long value) => SerializeCompressedInt((ulong)((value << 1) ^ (value >> 63)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeCompressedInt(ushort value) => SerializeCompressedInt((ulong)value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeCompressedInt(uint value) => SerializeCompressedInt((ulong)value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeCompressedInt(ulong value)
        {
            while (value >= 128)
            {
                Serialize((byte)(value | 128));
                value >>= 7;
            }

            Serialize((byte)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int InternalSerializeCompressedInt(ulong value, ref Span<byte> span, int spanOffset)
        {
            var position = 0;

            while (value >= 128)
            {
                span[spanOffset + position++] = (byte)(value | 128);
                value >>= 7;
            }

            span[spanOffset + position++] = (byte)value;
            return position;
        }
    }
}
