using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        #region Deserialize ByteArray

        #region Internal

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        byte[] InternalDeserializeByteArray(Span<byte> value, int count, bool raw, bool readCount, bool readOnly, bool containsNullPointer)
        {
            InternalDeserializeSpan(value, count, raw, readCount, readOnly, containsNullPointer, allowNullableArrayResult: false, out _, out var array, tryDeserialize: false);

            return array!;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        byte[]? InternalDeserializeNullableByteArray(Span<byte> value, int count, bool raw, bool readCount, bool readOnly, bool containsNullPointer)
        {
            InternalDeserializeSpan(value, count, raw, readCount, readOnly, containsNullPointer, allowNullableArrayResult: true, out _, out var array, tryDeserialize: false);

            return array;
        }

        #endregion Internal

        public byte[] DeserializeByteArray()
            => InternalDeserializeByteArray(default, count: 0, AutoRaw, readCount: true, readOnly: false, containsNullPointer: true);

        public byte[]? DeserializeNullableByteArray()
            => InternalDeserializeNullableByteArray(default, count: 0, AutoRaw, readCount: true, readOnly: false, containsNullPointer: true);

        public byte[] DeserializeRawByteArray()
            => InternalDeserializeByteArray(default, count: 0, raw: true, readCount: true, readOnly: false, containsNullPointer: true);

        public byte[]? DeserializeNullableRawByteArray()
            => InternalDeserializeNullableByteArray(default, count: 0, raw: true, readCount: true, readOnly: false, containsNullPointer: true);

        public byte[] DeserializeByteArray(int bytesCountToDeserialize)
            => InternalDeserializeByteArray(default, bytesCountToDeserialize, raw: false, readCount: false, readOnly: false, containsNullPointer: true);

        public byte[]? DeserializeNullableByteArray(int bytesCountToDeserialize)
            => InternalDeserializeNullableByteArray(default, bytesCountToDeserialize, raw: false, readCount: false, readOnly: false, containsNullPointer: true);

        public void DeserializeByteArray(byte[] destination, int destinationOffset = 0)
            => InternalDeserializeNullableByteArray(destination.AsSpan(destinationOffset), count: 0, raw: false, readCount: true, readOnly: false, containsNullPointer: false);

        public void DeserializeByteArray(byte[] destination, int destinationOffset, int bytesCountToDeserialize)
            => InternalDeserializeNullableByteArray(destination.AsSpan(destinationOffset), bytesCountToDeserialize, raw: false, readCount: false, readOnly: false, containsNullPointer: false);

        public void DeserializeRawByteArray(byte[] destination, int destinationOffset = 0)
            => InternalDeserializeNullableByteArray(destination.AsSpan(destinationOffset), count: 0, raw: true, readCount: true, readOnly: false, containsNullPointer: false);

        public unsafe void DeserializeBytes(byte* destination, int destinationLength)
            => InternalDeserializeNullableByteArray(new Span<byte>(destination, destinationLength), count: 0, raw: false, readCount: true, readOnly: false, containsNullPointer: destination == null);

        public unsafe void DeserializeBytes(byte* destination, int destinationLength, int bytesCountToDeserialize)
            => InternalDeserializeNullableByteArray(new Span<byte>(destination, destinationLength), bytesCountToDeserialize, raw: false, readCount: false, readOnly: false, containsNullPointer: destination == null);

        #endregion Deserialize ByteArray

        #region Try deserialize ByteArray

        #region Internal

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool InternalTryDeserializeByteArray(Span<byte> value, int count, bool raw, bool readCount, bool readOnly, bool containsNullPointer, out byte[]? result)
            => InternalDeserializeSpan(value, count, raw, readCount, readOnly, containsNullPointer, allowNullableArrayResult: false, out _, out result, tryDeserialize: true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool InternalTryDeserializeNullableByteArray(Span<byte> value, int count, bool raw, bool readCount, bool readOnly, bool containsNullPointer, out byte[]? result)
            => InternalDeserializeSpan(value, count, raw, readCount, readOnly, containsNullPointer, allowNullableArrayResult: true, out _, out result, tryDeserialize: true);

        #endregion Internal

        public bool TryDeserializeByteArray([NotNullWhen(true)] out byte[]? result)
            => InternalTryDeserializeByteArray(default, count: 0, raw: false, readCount: true, readOnly: false, containsNullPointer: true, out result!);

        public bool TryDeserializeNullableByteArray(out byte[]? result)
            => InternalTryDeserializeNullableByteArray(default, count: 0, raw: false, readCount: true, readOnly: false, containsNullPointer: true, out result);

        public bool TryDeserializeRawByteArray([NotNullWhen(true)] out byte[]? result)
            => InternalTryDeserializeByteArray(default, count: 0, raw: true, readCount: true, readOnly: false, containsNullPointer: true, out result!);

        public bool TryDeserializeNullableRawByteArray(out byte[]? result)
            => InternalTryDeserializeNullableByteArray(default, count: 0, raw: true, readCount: true, readOnly: false, containsNullPointer: true, out result);

        public bool TryDeserializeByteArray([NotNullWhen(true)] out byte[]? result, int bytesCountToDeserialize)
            => InternalTryDeserializeByteArray(default, bytesCountToDeserialize, raw: false, readCount: true, readOnly: false, containsNullPointer: true, out result!);

        public bool TryDeserializeNullableByteArray(out byte[]? result, int bytesCountToDeserialize)
            => InternalTryDeserializeNullableByteArray(default, bytesCountToDeserialize, raw: false, readCount: true, readOnly: false, containsNullPointer: true, out result);

        public bool TryDeserializeByteArray(byte[] destination, int destinationOffset = 0)
            => InternalTryDeserializeNullableByteArray(destination.AsSpan(destinationOffset), count: 0, raw: false, readCount: true, readOnly: false, containsNullPointer: false, result: out _);

        public bool TryDeserializeByteArray(byte[] destination, int destinationOffset, int bytesCountToDeserialize)
            => InternalTryDeserializeNullableByteArray(destination.AsSpan(destinationOffset), bytesCountToDeserialize, raw: false, readCount: false, readOnly: false, containsNullPointer: false, result: out _);

        public bool TryDeserializeRawByteArray(byte[] destination, int destinationOffset = 0)
            => InternalTryDeserializeNullableByteArray(destination.AsSpan(destinationOffset), count: 0, raw: true, readCount: true, readOnly: false, containsNullPointer: false, result: out _);

        public unsafe bool TryDeserializeBytes(byte* destination, int destinationLength)
            => InternalTryDeserializeNullableByteArray(new Span<byte>(destination, destinationLength), count: 0, raw: false, readCount: true, readOnly: false, containsNullPointer: destination == null, result: out _);

        public unsafe bool TryDeserializeBytes(byte* destination, int destinationLength, int bytesCountToDeserialize)
            => InternalTryDeserializeNullableByteArray(new Span<byte>(destination, destinationLength), bytesCountToDeserialize, raw: false, readCount: false, readOnly: false, containsNullPointer: destination == null, result: out _);

        #endregion Try deserialize ByteArray

        #region To ByteArray

        public byte[] ToByteArray()
        {
            var startPosition = _position;
            Position = 0;
            var bytes = DeserializeRawByteArray();
            Position = _position;
            return bytes;
        }

        public byte[] ToByteArray(Index startIndex)
            => ToByteArray(startIndex.IsFromEnd ? _length - startIndex.Value : startIndex.Value);

        public byte[] ToByteArray(int start)
        {
            if (start < 0 || start > _length)
                throw new ArgumentOutOfRangeException(nameof(start), start, $"{nameof(start)} is less than 0 or greater than {nameof(Serializer)}.{nameof(Serializer.Length)}");

            var startPosition = _position;
            Position = start;
            var bytes = DeserializeByteArray(_length - start);
            Position = _position;
            return bytes;
        }

        public byte[] ToByteArray(Range range)
        {
            var start = range.Start.IsFromEnd ? _length - range.Start.Value : range.Start.Value;
            var length = range.End.IsFromEnd ? _length - start - range.End.Value : range.End.Value;
            
            return ToByteArray(start, length);
        }

        public byte[] ToByteArray(int start, int length)
        {
            if (start < 0 || length < 0 || start + length > _length || start + length < 0)
                throw new ArgumentOutOfRangeException(nameof(start), start, $"{nameof(start)}, {nameof(length)}, or {nameof(start)} + {nameof(length)} is not in the range of {nameof(Serializer)}.");

            var startPosition = _position;
            Position = start;
            var bytes = DeserializeByteArray(length);
            Position = _position;
            return bytes;
        }

        #endregion To ByteArray
    }
}