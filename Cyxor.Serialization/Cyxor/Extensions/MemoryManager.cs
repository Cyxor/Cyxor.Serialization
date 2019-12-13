#if !NET20 && !NET35 && !NET40 && !NETSTANDARD1_0

using System;
using System.Buffers;

namespace Cyxor.Extensions
{
    sealed class MemoryManager<TFrom, TTo> : MemoryManager<TTo>
        where TFrom : unmanaged
        where TTo : unmanaged
    {
        readonly Memory<TFrom> _memory;

        public MemoryManager(Memory<TFrom> memory)
        {
            _memory = memory;
        }

        public override Span<TTo> GetSpan()
            => _memory.Span.Cast<TFrom, TTo>();

        protected override void Dispose(bool disposing) { }

        public override MemoryHandle Pin(int elementIndex = 0)
            => throw new NotSupportedException();

        public override void Unpin()
            => throw new NotSupportedException();
    }
}

#endif