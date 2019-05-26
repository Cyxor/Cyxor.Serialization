#if !NET20 && !NET35 && !NET40 && !NETSTANDARD1_0

using System;

namespace Cyxor.Extensions
{
    static class SpanExtensions
    {
        public static Span<byte> ToSpanOfBytes<T>(this in Span<T> span) where T : struct
            => System.Runtime.InteropServices.MemoryMarshal.AsBytes(span);

        public static ReadOnlySpan<byte> ToReadOnlySpanOfBytes<T>(this in ReadOnlySpan<T> readOnlySpan) where T : struct
            => System.Runtime.InteropServices.MemoryMarshal.AsBytes(readOnlySpan);

        public static Span<T> ToSpanOf<T>(this in Span<byte> span) where T : struct
            => System.Runtime.InteropServices.MemoryMarshal.Cast<byte, T>(span);

        public static ReadOnlySpan<T> ToReadOnlySpanOf<T>(this in ReadOnlySpan<byte> readOnlySpan) where T : struct
            => System.Runtime.InteropServices.MemoryMarshal.Cast<byte, T>(readOnlySpan);
    }
}

#endif