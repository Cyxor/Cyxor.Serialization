﻿using System;
using System.Buffers;
using System.Text.Unicode;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.IO;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        #region Internal

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void InternalSerialize(ReadOnlySpan<byte> value, bool raw, bool containsNullPointer = false)
        {
            if (containsNullPointer)
            {
                if (!raw)
                    Serialize((byte)0);

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
        public void InternalSerialize(ReadOnlySpan<char> value, bool raw, bool containsNullPointer = false, bool utf8Encoding = true)
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

            int charsRead;
            int bytesWritten;
            OperationStatus operationStatus;

            if (_stream == null)
            {
                var serializerSpan = _memory.Span[_position.._capacity];
                operationStatus = Utf8.FromUtf16(value, serializerSpan, out charsRead, out bytesWritten);
            }
            else if (InternalTryGetStreamAsMemoryStreamBuffer(out var arraySegment))
            {
                var serializerSpan = arraySegment.AsSpan(arraySegment.Offset + _position, arraySegment.Count);

                operationStatus = Utf8.FromUtf16(value, serializerSpan, out charsRead, out bytesWritten);

                _stream.Position += bytesWritten;
            }
            else
            {
                using var streamWriter = new StreamWriter(_stream, System.Text.Encoding.UTF8, leaveOpen: true);

                streamWriter.Write(value);
                operationStatus = OperationStatus.Done;

                charsRead = count;
                bytesWritten = (int)(_stream.Position - startPosition);
            }

            _position += bytesWritten;

            if (_length < _position)
                _length = _position;

            var totalBytesWritten = bytesWritten;

            if (operationStatus != OperationStatus.Done && operationStatus != OperationStatus.DestinationTooSmall)
                throw new ArgumentException($"Invalid {nameof(Span<char>)} value", nameof(value));
            else if (operationStatus == OperationStatus.DestinationTooSmall)
            {
                var currentLength = _length;
                var freeCapacity = MaxCapacity - _position;
                var maxRequiredCapacity = (count - charsRead) * 2;
                maxRequiredCapacity = freeCapacity < maxRequiredCapacity ? freeCapacity : maxRequiredCapacity;

                InternalEnsureSerializeCapacity(maxRequiredCapacity);

                value = value.Slice(charsRead);

                if (_stream == null)
                {
                    var serializerSpan = _memory.Span[_position.._capacity];
                    operationStatus = Utf8.FromUtf16(value, serializerSpan, out _, out bytesWritten);
                }
                else if (InternalTryGetStreamAsMemoryStreamBuffer(out var arraySegment))
                {
                    var serializerSpan = arraySegment.AsSpan(arraySegment.Offset + _position, arraySegment.Count);

                    operationStatus = Utf8.FromUtf16(value, serializerSpan, out _, out bytesWritten);

                    _stream.Position += bytesWritten;
                }

                if (operationStatus == OperationStatus.DestinationTooSmall)
                    throw new ArgumentException($"Insufficient space in the internal buffer to serialize the provided value of {count} UTF16 chars. The buffer has reached its maximum allowed capacity of {maxRequiredCapacity} and cannot be further enlarged. The initial position of the buffer before this particular serialization operation was {startPosition}.");
                else if (operationStatus != OperationStatus.Done)
                    throw new ArgumentException($"Invalid {nameof(Span<char>)} value", nameof(value));

                _position += bytesWritten;
                totalBytesWritten += bytesWritten;
                _length = _position > currentLength ? _position : currentLength;
            }

            if (!raw)
                InternalEndSerializeStringHeader(count, totalBytesWritten, startPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void InternalSerializeGeneric<T>(ReadOnlySpan<T> value, bool raw, bool containsNullPointer = false) where T : unmanaged
            => InternalSerialize(MemoryMarshal.AsBytes(value), raw, containsNullPointer);

        #endregion Internal

        #region byte

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(Span<byte> value)
            => InternalSerialize(value, AutoRaw);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeRaw(Span<byte> value)
            => InternalSerialize(value, raw: true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(ReadOnlySpan<byte> value)
            => InternalSerialize(value, AutoRaw);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeRaw(ReadOnlySpan<byte> value)
            => InternalSerialize(value, raw: true);

        #endregion byte

        #region char

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(Span<char> value)
            => InternalSerialize(value, AutoRaw);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(Span<char> value, bool utf8Encoding)
            => InternalSerialize(value, AutoRaw, utf8Encoding: utf8Encoding);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeRaw(Span<char> value, bool utf8Encoding = true)
            => InternalSerialize(value, raw: true, utf8Encoding: utf8Encoding);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(ReadOnlySpan<char> value)
            => InternalSerialize(value, AutoRaw);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(ReadOnlySpan<char> value, bool utf8Encoding)
            => InternalSerialize(value, AutoRaw, utf8Encoding: utf8Encoding);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeRaw(ReadOnlySpan<char> value, bool utf8Encoding = true)
            => InternalSerialize(value, raw: true, utf8Encoding: utf8Encoding);

        #endregion char

        #region t

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize<T>(Span<T> value) where T : unmanaged
            => InternalSerializeGeneric<T>(value, AutoRaw);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeRaw<T>(Span<T> value) where T : unmanaged
            => InternalSerializeGeneric<T>(value, raw: true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize<T>(ReadOnlySpan<T> value) where T : unmanaged
            => InternalSerializeGeneric(value, AutoRaw);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeRaw<T>(ReadOnlySpan<T> value) where T : unmanaged
            => InternalSerializeGeneric(value, raw: true);

        #endregion t
    }
}