#if !NET20 && !NET35 && !NET40 && !NETSTANDARD1_0

using System;
using System.Runtime.InteropServices;

namespace Cyxor.Extensions
{
    static class SpanExtensions
    {
        public static Span<TTo> Cast<TFrom, TTo>(this in Span<TFrom> span)
            where TFrom : unmanaged
            where TTo : unmanaged
            => MemoryMarshal.Cast<TFrom, TTo>(span);

        public static ReadOnlySpan<TTo> Cast<TFrom, TTo>(this in ReadOnlySpan<TFrom> readOnlySpan)
            where TFrom : unmanaged
            where TTo : unmanaged
            => MemoryMarshal.Cast<TFrom, TTo>(readOnlySpan);
    }
}

#endif