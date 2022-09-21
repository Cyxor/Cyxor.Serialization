using System;
using System.Buffers;
using System.Text.Unicode;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        #region Internal

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void InternalSerialize(Span<byte> value, bool raw, bool containsNullPointer = false) =>
            InternalSerialize((ReadOnlySpan<byte>)value, raw, containsNullPointer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void InternalSerialize(ReadOnlySpan<byte> value, bool raw, bool containsNullPointer = false)
        {
            if (containsNullPointer)
            {
                if (!raw)
                    Serialize(NullMap);

                return;
            }

            if (value.IsEmpty)
            {
                if (!raw)
                    Serialize(EmptyMap);

                return;
            }

            var length = value.Length;

            if (!raw)
                InternalSerializeSequenceHeader(length);

            InternalEnsureSerializeCapacity(length);

            if (_stream == null)
                value.CopyTo(_memory.Span.Slice(_position, length));
            else
                _stream.Write(value);

            _position += length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void InternalSerialize(
            Span<char> value,
            bool raw,
            bool containsNullPointer = false,
            bool utf8Encoding = true
        ) => InternalSerialize((ReadOnlySpan<char>)value, raw, containsNullPointer = false, utf8Encoding);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void InternalSerialize(
            ReadOnlySpan<char> value,
            bool raw,
            bool containsNullPointer = false,
            bool utf8Encoding = true
        )
        {
            if (!utf8Encoding || containsNullPointer || value.IsEmpty)
            {
                InternalSerialize(MemoryMarshal.Cast<char, byte>(value), raw, containsNullPointer);
                return;
            }

            var count = value.Length;
            var startPosition = _position;

            if (!raw)
                InternalStartSerializeStringHeader(count);

            InternalEnsureSerializeCapacity(count);
            var serializerSpan = _memory.Span.Slice(_position, _capacity);
            var operationStatus = Utf8.FromUtf16(value, serializerSpan, out var charsRead, out var bytesWritten);

            _position += bytesWritten;

            if (_length < _position)
                _length = _position;

            var totalBytesWritten = bytesWritten;

            if (operationStatus != OperationStatus.Done && operationStatus != OperationStatus.DestinationTooSmall)
                throw new ArgumentException($"Invalid {nameof(Span<char>)} value", nameof(value));

            if (operationStatus == OperationStatus.DestinationTooSmall)
            {
                var currentLength = _length;
                var freeCapacity = MaxCapacity - _position;
                var requiredCapacity = (count - charsRead) * 2;
                requiredCapacity = freeCapacity < requiredCapacity ? freeCapacity : requiredCapacity;

                InternalEnsureSerializeCapacity(requiredCapacity);

                value = value[charsRead..];

                serializerSpan = _memory.Span.Slice(_position, _capacity);
                operationStatus = Utf8.FromUtf16(value, serializerSpan, out _, out bytesWritten);

                if (operationStatus != OperationStatus.Done)
                    throw new ArgumentException($"Invalid {nameof(Span<char>)} value", nameof(value));

                _position += bytesWritten;
                totalBytesWritten += bytesWritten;
                _length = _position > currentLength ? _position : currentLength;
            }

            if (!raw)
                InternalEndSerializeStringHeader(count, totalBytesWritten, startPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void InternalSerializeT<T>(Span<T> value, bool raw, bool containsNullPointer = false)
            where T : unmanaged => InternalSerialize(MemoryMarshal.AsBytes(value), raw, containsNullPointer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void InternalSerializeT<T>(ReadOnlySpan<T> value, bool raw, bool containsNullPointer = false)
            where T : unmanaged => InternalSerialize(MemoryMarshal.AsBytes(value), raw, containsNullPointer);

        #endregion Internal

        #region byte

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(Span<byte> value) => InternalSerialize(value, AutoRaw);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeRaw(Span<byte> value) => InternalSerialize(value, raw: true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(ReadOnlySpan<byte> value) => InternalSerialize(value, AutoRaw);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeRaw(ReadOnlySpan<byte> value) => InternalSerialize(value, raw: true);

        #endregion byte

        #region char

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(Span<char> value) => InternalSerialize(value, AutoRaw);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(Span<char> value, bool utf8Encoding) =>
            InternalSerialize(value, AutoRaw, utf8Encoding: utf8Encoding);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeRaw(Span<char> value, bool utf8Encoding = true) =>
            InternalSerialize(value, raw: true, utf8Encoding: utf8Encoding);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(ReadOnlySpan<char> value) => InternalSerialize(value, AutoRaw);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(ReadOnlySpan<char> value, bool utf8Encoding) =>
            InternalSerialize(value, AutoRaw, utf8Encoding: utf8Encoding);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeRaw(ReadOnlySpan<char> value, bool utf8Encoding = true) =>
            InternalSerialize(value, raw: true, utf8Encoding: utf8Encoding);

        #endregion char

        #region t

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize<T>(Span<T> value)
            where T : unmanaged => InternalSerializeT(value, AutoRaw);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeRaw<T>(Span<T> value)
            where T : unmanaged => InternalSerializeT(value, raw: true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize<T>(ReadOnlySpan<T> value)
            where T : unmanaged => InternalSerializeT(value, AutoRaw);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeRaw<T>(ReadOnlySpan<T> value)
            where T : unmanaged => InternalSerializeT(value, raw: true);
        #endregion t
    }
}
