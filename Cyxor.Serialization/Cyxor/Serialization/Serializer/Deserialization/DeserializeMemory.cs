// Licensed under the MIT license

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        #region Memory<byte>

        #region Deserialize Memory<byte>

        private static readonly bool s_eraseMe = false;

        public Memory<byte> DeserializeMemoryByte()
            => InternalDeserializeByteArray(default, count: 0, AutoRaw, readCount: true, readOnly: false, containsNullPointer: true);

        public Memory<byte>? DeserializeNullableMemoryByte()
            => InternalDeserializeNullableByteArray(default, count: 0, AutoRaw, readCount: true, readOnly: false, containsNullPointer: true);

        public Memory<byte> DeserializeRawMemoryByte()
            => InternalDeserializeByteArray(default, count: 0, raw: true, readCount: true, readOnly: false, containsNullPointer: true);

        public Memory<byte>? DeserializeNullableRawMemoryByte()
            => InternalDeserializeNullableByteArray(default, count: 0, raw: true, readCount: true, readOnly: false, containsNullPointer: true);

        public Memory<byte> DeserializeMemoryByte(int bytesCountToDeserialize)
            => InternalDeserializeByteArray(default, bytesCountToDeserialize, raw: false, readCount: false, readOnly: false, containsNullPointer: true);

        public Memory<byte>? DeserializeNullableMemoryByte(int bytesCountToDeserialize)
            => InternalDeserializeNullableByteArray(default, bytesCountToDeserialize, raw: false, readCount: false, readOnly: false, containsNullPointer: true);

        public void DeserializeMemoryByte(Memory<byte> destination)
            => InternalDeserializeNullableByteArray(destination.Span, count: 0, raw: false, readCount: true, readOnly: false, containsNullPointer: false);

        public void DeserializeMemoryByte(Memory<byte> destination, int bytesCountToDeserialize)
            => InternalDeserializeNullableByteArray(destination.Span, bytesCountToDeserialize, raw: false, readCount: false, readOnly: false, containsNullPointer: false);

        public void DeserializeRawMemoryByte(Memory<byte> destination)
            => InternalDeserializeNullableByteArray(destination.Span, count: 0, raw: true, readCount: true, readOnly: false, containsNullPointer: false);

        #endregion Deserialize Memory<byte>

        #region Try deserialize Memory<byte>

        #region Internal

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe bool InternalTryDeserializeMemoryByte(Span<byte> value, int count, bool raw, bool readCount, bool readOnly, bool containsNullPointer, out Memory<byte>? result)
        {
            var opResult = InternalDeserializeSpan(value, count, raw, readCount, readOnly, containsNullPointer, allowNullableArrayResult: false, out _, out var byteResult, tryDeserialize: true);

            result = byteResult;
            return opResult;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe bool InternalTryDeserializeNullableMemoryByte(Span<byte> value, int count, bool raw, bool readCount, bool readOnly, bool containsNullPointer, out Memory<byte>? result)
        {
            byte[]? byteResult;

            var opResult = InternalDeserializeSpan(value, count, raw, readCount, readOnly, containsNullPointer, allowNullableArrayResult: true, out _, out byteResult, tryDeserialize: true);

            result = byteResult;
            return opResult;
        }

        #endregion Internal

        public bool TryDeserializeMemoryByte([NotNullWhen(true)] out Memory<byte>? result)
            => InternalTryDeserializeMemoryByte(default, count: 0, raw: false, readCount: true, readOnly: false, containsNullPointer: true, out result!);

        public bool TryDeserializeNullableMemoryByte(out Memory<byte>? result)
            => InternalTryDeserializeNullableMemoryByte(default, count: 0, raw: false, readCount: true, readOnly: false, containsNullPointer: true, out result);

        public bool TryDeserializeRawMemoryByte([NotNullWhen(true)] out Memory<byte>? result)
            => InternalTryDeserializeMemoryByte(default, count: 0, raw: true, readCount: true, readOnly: false, containsNullPointer: true, out result!);

        public bool TryDeserializeNullableRawMemoryByte(out Memory<byte>? result)
            => InternalTryDeserializeNullableMemoryByte(default, count: 0, raw: true, readCount: true, readOnly: false, containsNullPointer: true, out result);

        public bool TryDeserializeMemoryByte([NotNullWhen(true)] out Memory<byte>? result, int bytesCountToDeserialize)
            => InternalTryDeserializeMemoryByte(default, bytesCountToDeserialize, raw: false, readCount: true, readOnly: false, containsNullPointer: true, out result!);

        public bool TryDeserializeNullableMemoryByte(out Memory<byte>? result, int bytesCountToDeserialize)
            => InternalTryDeserializeNullableMemoryByte(default, bytesCountToDeserialize, raw: false, readCount: true, readOnly: false, containsNullPointer: true, out result);

        public bool TryDeserializeMemoryByte(Memory<byte> destination)
            => InternalTryDeserializeNullableByteArray(destination.Span, count: 0, raw: false, readCount: true, readOnly: false, containsNullPointer: false, result: out _);

        public bool TryDeserializeMemoryByte(Memory<byte> destination, int bytesCountToDeserialize)
            => InternalTryDeserializeNullableByteArray(destination.Span, bytesCountToDeserialize, raw: false, readCount: false, readOnly: false, containsNullPointer: false, result: out _);

        public bool TryDeserializeRawMemoryByte(Memory<byte> destination)
            => InternalTryDeserializeNullableByteArray(destination.Span, count: 0, raw: true, readCount: true, readOnly: false, containsNullPointer: false, result: out _);

        #endregion Try deserialize Memory<byte>

        #region To Memory<byte>

        public Memory<byte> ToMemoryByte()
            => ToByteArray();

        public Memory<byte> ToMemoryByte(Index startIndex)
            => ToByteArray(startIndex.IsFromEnd ? _length - startIndex.Value : startIndex.Value);

        public Memory<byte> ToMemoryByte(int start)
            => ToByteArray(start);

        public Memory<byte> ToMemoryByte(Range range)
            => ToByteArray(range);

        public Memory<byte> ToMemoryByte(int start, int length)
            => ToByteArray(start, length);

        #endregion To Memory<byte>

        #region As Memory<byte>

        public Memory<byte> AsMemoryByte()
        {
            if (InternalTryGetStreamAsMemoryStreamBuffer(out var arraySegment))
                return arraySegment.AsMemory();
            else if (_stream != null)
                return ToByteArray();

            return _memory;
        }

        #endregion To Memory<byte>

        #endregion Memory<byte>

        #region Char

        // TODO:

        #endregion Char

        #region T

        // TODO:

        #endregion T
    }
}
