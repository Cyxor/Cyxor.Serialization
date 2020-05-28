using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T ReverseEndianness<T>(T value) where T : unmanaged
        {
            var size = sizeof(T);

            if (size == sizeof(short))
            {
                var swapValue = BinaryPrimitives.ReverseEndianness(Unsafe.As<T, ushort>(ref value));
                return Unsafe.As<ushort, T>(ref swapValue);
            }
            else if (size == sizeof(int))
            {
                var swapValue = BinaryPrimitives.ReverseEndianness(Unsafe.As<T, uint>(ref value));
                return Unsafe.As<uint, T>(ref swapValue);
            }
            else if (size == sizeof(long))
            {
                var swapValue = BinaryPrimitives.ReverseEndianness(Unsafe.As<T, ulong>(ref value));
                return Unsafe.As<ulong, T>(ref swapValue);
            }
            else
            {
                var ptrValue = (byte*)&value;

                for (var i = 0; i < size; i++)
                {
                    var tmp = ptrValue[size - 1 - i];
                    ptrValue[size - 1 - i] = ptrValue[i];
                    ptrValue[i] = tmp;
                }

                return *(T*)ptrValue;
            }
        }
    }
}