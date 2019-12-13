#if !NET20 && !NET35 && !NET40 && !NETSTANDARD1_0

using System;
using System.Buffers;
using System.Runtime.InteropServices;

namespace Cyxor.Extensions
{
    static class MemoryExtensions
    {
        public static Memory<TTo> Cast<TFrom, TTo>(this in Memory<TFrom> memory)
                where TFrom : unmanaged
                where TTo : unmanaged
        {
            if (typeof(TFrom) == typeof(TTo))
                return (Memory<TTo>)(object)memory;

            using var memoryManager = new MemoryManager<TFrom, TTo>(memory);

            return memoryManager.Memory;
        }

        public static ReadOnlyMemory<TTo> Cast<TFrom, TTo>(this in ReadOnlyMemory<TFrom> readOnlyMemory)
                where TFrom : unmanaged
                where TTo : unmanaged
            => MemoryMarshal.AsMemory(readOnlyMemory).Cast<TFrom, TTo>();
    }
}

#endif