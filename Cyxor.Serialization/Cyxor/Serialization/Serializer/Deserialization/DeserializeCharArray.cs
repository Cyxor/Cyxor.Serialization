using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        #region Deserialize CharArray

        #region Internal

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        char[] InternalDeserializeCharArray(Span<char> value, int count, bool raw, bool readCount, bool readOnly, bool containsNullPointer, bool utf8Encoding)
        {
            InternalDeserializeSpan(value, count, raw, readCount, readOnly, containsNullPointer, utf8Encoding, allowNullableArrayResult: false, out _, out var array, tryDeserialize: false);

            return array!;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        char[]? InternalDeserializeNullableCharArray(Span<char> value, int count, bool raw, bool readCount, bool readOnly, bool containsNullPointer, bool utf8Encoding)
        {
            InternalDeserializeSpan(value, count, raw, readCount, readOnly, containsNullPointer, utf8Encoding, allowNullableArrayResult: true, out _, out var array, tryDeserialize: false);

            return array;
        }

        #endregion Internal

        public char[] DeserializeCharArray()
            => InternalDeserializeCharArray(default, count: 0, AutoRaw, readCount: true, readOnly: false, containsNullPointer: true, utf8Encoding: true);

        public char[] DeserializeCharArray(bool utf8Encoding)
            => InternalDeserializeCharArray(default, count: 0, AutoRaw, readCount: true, readOnly: false, containsNullPointer: true, utf8Encoding);    

        public char[]? DeserializeNullableCharArray()
            => InternalDeserializeNullableCharArray(default, count: 0, AutoRaw, readCount: true, readOnly: false, containsNullPointer: true, utf8Encoding: true);

        public char[]? DeserializeNullableCharArray(bool utf8Encoding)
            => InternalDeserializeNullableCharArray(default, count: 0, AutoRaw, readCount: true, readOnly: false, containsNullPointer: true, utf8Encoding);

        public char[] DeserializeRawCharArray(bool utf8Encoding = true)
            => InternalDeserializeCharArray(default, count: 0, raw: true, readCount: true, readOnly: false, containsNullPointer: true, utf8Encoding);

        public char[]? DeserializeNullableRawCharArray(bool utf8Encoding = true)
            => InternalDeserializeNullableCharArray(default, count: 0, raw: true, readCount: true, readOnly: false, containsNullPointer: true, utf8Encoding);

        public char[] DeserializeCharArray(int charsCountToDeserialize, bool utf8Encoding = true)
            => InternalDeserializeCharArray(default, charsCountToDeserialize, raw: false, readCount: false, readOnly: false, containsNullPointer: true, utf8Encoding);

        public char[]? DeserializeNullableCharArray(int charsCountToDeserialize, bool utf8Encoding = true)
            => InternalDeserializeNullableCharArray(default, charsCountToDeserialize, raw: false, readCount: false, readOnly: false, containsNullPointer: true, utf8Encoding);

        public void DeserializeCharArray(char[] destination, int destinationOffset = 0, bool utf8Encoding = true)
            => InternalDeserializeNullableCharArray(destination.AsSpan(destinationOffset), count: 0, raw: false, readCount: true, readOnly: false, containsNullPointer: false, utf8Encoding);

        public void DeserializeCharArray(char[] destination, int destinationOffset, int charsCountToDeserialize, bool utf8Encoding = true)
            => InternalDeserializeNullableCharArray(destination.AsSpan(destinationOffset), charsCountToDeserialize, raw: false, readCount: false, readOnly: false, containsNullPointer: false, utf8Encoding);

        public void DeserializeRawCharArray(char[] destination, int destinationOffset = 0, bool utf8Encoding = true)
            => InternalDeserializeNullableCharArray(destination.AsSpan(destinationOffset), count: 0, raw: true, readCount: true, readOnly: false, containsNullPointer: false, utf8Encoding);

        public unsafe void DeserializeChars(char* destination, int destinationLength, bool utf8Encoding = true)
            => InternalDeserializeNullableCharArray(new Span<char>(destination, destinationLength), count: 0, raw: false, readCount: true, readOnly: false, containsNullPointer: destination == null, utf8Encoding);

        public unsafe void DeserializeChars(char* destination, int destinationLength, int charsCountToDeserialize, bool utf8Encoding = true)
            => InternalDeserializeNullableCharArray(new Span<char>(destination, destinationLength), charsCountToDeserialize, raw: false, readCount: false, readOnly: false, containsNullPointer: destination == null, utf8Encoding);

        #endregion Deserialize CharArray

        #region Try deserialize CharArray

        #region Internal

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool InternalTryDeserializeCharArray(Span<char> value, int count, bool raw, bool readCount, bool readOnly, bool containsNullPointer, bool utf8Encoding, out char[] result)
            => InternalDeserializeSpan(value, count, raw, readCount, readOnly, containsNullPointer, utf8Encoding,allowNullableArrayResult: false, out _, out result!, tryDeserialize: true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool InternalTryDeserializeNullableCharArray(Span<char> value, int count, bool raw, bool readCount, bool readOnly, bool containsNullPointer, bool utf8Encoding, out char[]? result)
            => InternalDeserializeSpan(value, count, raw, readCount, readOnly, containsNullPointer, utf8Encoding, allowNullableArrayResult: true, out _, out result, tryDeserialize: true);

        #endregion Internal

        public bool TryDeserializeCharArray([NotNullWhen(true)] out char[]? result, bool utf8Encoding = true)
            => InternalTryDeserializeCharArray(default, count: 0, raw: false, readCount: true, readOnly: false, containsNullPointer: true, utf8Encoding, out result);

        public bool TryDeserializeNullableCharArray(out char[]? result, bool utf8Encoding = true)
            => InternalTryDeserializeNullableCharArray(default, count: 0, raw: false, readCount: true, readOnly: false, containsNullPointer: true, utf8Encoding, out result);

        public bool TryDeserializeRawcharArray([NotNullWhen(true)] out char[]? result, bool utf8Encoding = true)
            => InternalTryDeserializeCharArray(default, count: 0, raw: true, readCount: true, readOnly: false, containsNullPointer: true, utf8Encoding, out result);

        public bool TryDeserializeNullableRawcharArray(out char[]? result, bool utf8Encoding = true)
            => InternalTryDeserializeNullableCharArray(default, count: 0, raw: true, readCount: true, readOnly: false, containsNullPointer: true, utf8Encoding, out result);

        public bool TryDeserializeCharArray([NotNullWhen(true)] out char[]? result, int charsCountToDeserialize, bool utf8Encoding = true)
            => InternalTryDeserializeCharArray(default, charsCountToDeserialize, raw: false, readCount: true, readOnly: false, containsNullPointer: true, utf8Encoding, out result);

        public bool TryDeserializeNullableCharArray(out char[]? result, int charsCountToDeserialize, bool utf8Encoding = true)
            => InternalTryDeserializeNullableCharArray(default, charsCountToDeserialize, raw: false, readCount: true, readOnly: false, containsNullPointer: true, utf8Encoding, out result);

        public bool TryDeserializeCharArray(char[] destination, int destinationOffset = 0, bool utf8Encoding = true)
            => InternalTryDeserializeNullableCharArray(destination.AsSpan(destinationOffset), count: 0, raw: false, readCount: true, readOnly: false, containsNullPointer: false, utf8Encoding, result: out _);

        public bool TryDeserializeCharArray(char[] destination, int destinationOffset, int charsCountToDeserialize, bool utf8Encoding = true)
            => InternalTryDeserializeNullableCharArray(destination.AsSpan(destinationOffset), charsCountToDeserialize, raw: false, readCount: false, readOnly: false, containsNullPointer: false, utf8Encoding, result: out _);

        public bool TryDeserializeRawcharArray(char[] destination, int destinationOffset = 0, bool utf8Encoding = true)
            => InternalTryDeserializeNullableCharArray(destination.AsSpan(destinationOffset), count: 0, raw: true, readCount: true, readOnly: false, containsNullPointer: false, utf8Encoding, result: out _);

        public unsafe bool TryDeserializeChars(char* destination, int destinationLength, bool utf8Encoding = true)
            => InternalTryDeserializeNullableCharArray(new Span<char>(destination, destinationLength), count: 0, raw: false, readCount: true, readOnly: false, containsNullPointer: destination == null, utf8Encoding, result: out _);

        public unsafe bool TryDeserializeChars(char* destination, int destinationLength, int charsCountToDeserialize, bool utf8Encoding = true)
            => InternalTryDeserializeNullableCharArray(new Span<char>(destination, destinationLength), charsCountToDeserialize, raw: false, readCount: false, readOnly: false, containsNullPointer: destination == null, utf8Encoding, result: out _);

        #endregion Try deserialize CharArray

        #region To CharArray

        public char[] ToCharArray(bool utf8Encoding = true)
        {
            var startPosition = _position;
            Position = 0;
            var chars = DeserializeRawCharArray(utf8Encoding);
            Position = _position;
            return chars;
        }

        public char[] ToCharArray(Index startIndex, bool utf8Encoding = true)
            => ToCharArray(startIndex.IsFromEnd ? _length - startIndex.Value : startIndex.Value, utf8Encoding);

        public char[] ToCharArray(int start, bool utf8Encoding = true)
        {
            if (start < 0 || start > _length)
                throw new ArgumentOutOfRangeException(nameof(start), start, $"{nameof(start)} is less than 0 or greater than {nameof(Serializer)}.{nameof(Serializer.Length)}");

            var startPosition = _position;
            Position = start;
            var chars = DeserializeCharArray(_length - start, utf8Encoding);
            Position = _position;
            return chars;
        }

        public char[] ToCharArray(Range range, bool utf8Encoding = true)
        {
            var start = range.Start.IsFromEnd ? _length - range.Start.Value : range.Start.Value;
            var length = range.End.IsFromEnd ? _length - start - range.End.Value : range.End.Value;
            
            return ToCharArray(start, length, utf8Encoding);
        }

        public char[] ToCharArray(int start, int length, bool utf8Encoding = true)
        {
            if (start < 0 || length < 0 || start + length > _length || start + length < 0)
                throw new ArgumentOutOfRangeException(nameof(start), start, $"{nameof(start)}, {nameof(length)}, or {nameof(start)} + {nameof(length)} is not in the range of {nameof(Serializer)}.");

            var startPosition = _position;
            Position = start;
            var chars = DeserializeCharArray(length, utf8Encoding);
            Position = _position;
            return chars;
        }

        #endregion To CharArray
    }
}