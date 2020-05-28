using System;
using System.IO;
using System.Buffers;
using System.Text.Unicode;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        #region Internal

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe Span<byte> InternalDeserializeSpan(Span<byte> value, int count, bool raw, bool readCount, bool readOnly, bool containsNullPointer)
        {
            if (raw)
            {
                var valueLength = value.Length;
                var remainingLength = _length - _position;
                count = valueLength < remainingLength ? valueLength : remainingLength;
            }
            else if (readCount)
            {
                count = InternalDeserializeSequenceHeader();

                if (count < 0)
                    throw new SerializationException($"Error deserializing '{nameof(Span<byte>)}', the serializer instance may not contains a '{nameof(Span<byte>)}' serialized at position ({_position}).");
            }

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), $"Parameter '{nameof(count)}' must be a positive value.");

            if (count == 0)
                return Span<byte>.Empty;

            if (!containsNullPointer && count > value.Length)
                if (readCount)
                    throw new ArgumentException($"The supplied '{nameof(value)}.Length' of ({value.Length}) is insufficient to read the next stored {nameof(Span<byte>)} object of length ({count}).", nameof(value));
                else
                    throw new ArgumentOutOfRangeException(nameof(count), $"The supplied '{nameof(value)}.Length' of ({value.Length}) must be equal or greater than the supplied '{nameof(count)}' of ({count}).");

            InternalEnsureDeserializeCapacity(count);

            var span = containsNullPointer && !readOnly ? new Span<byte>(new byte[count]) : value;

            if (_stream == null)
            {
                if (readOnly)
                    span = _memory.Span.Slice(_position, count);
                else
                    _memory.Span.Slice(_position, count).CopyTo(span);
            }
            else
            {
                if (readOnly && _stream is MemoryStream memoryStream && memoryStream.TryGetBuffer(out var arraySegment))
                    span = arraySegment.AsSpan(arraySegment.Offset + _position, count);
                else
                {
                    if (containsNullPointer && readOnly)
                        span = new Span<byte>(new byte[count]);

                    if (count != span.Length)
                        span = span.Slice(0, count);

                    if (_stream.Read(span) != count)
                        throw new SerializationException($"Failed to read the specified '{nameof(Span<byte>)}' with count of ({count}).");
                }
            }

            _position += count;
            return span;
        }

        // TODO: Finish
        // DUDA: Es necesario el readonly aquí?
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe Span<char> InternalDeserializeSpan(Span<char> value, int count, bool raw, bool readCount, bool readOnly, bool containsNullPointer)
        {
            var bytesCount = !raw && readCount ? InternalDeserializeStringHeader() : count * sizeof(char);

            var spanBytes = InternalDeserializeSpan(MemoryMarshal.AsBytes(value), bytesCount, raw, readCount: false, readOnly: true, containsNullPointer);

            var estimatedCharsCount = !raw && readCount ? bytesCount * sizeof(char) : count;

            value = containsNullPointer ? new Span<char>(new char[estimatedCharsCount]) : value;

            var operationStatus = Utf8.ToUtf16(spanBytes, value, out var bytesRead, out var charsWritten);

            if (operationStatus != OperationStatus.Done || )
                throw new InvalidOperationException(Utilities.ResourceStrings.CyxorInternalException);
            else
                value = value.Slice(0, charsWritten);

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe Span<T> InternalDeserializeSpan<T>(Span<T> value, int count, bool raw, bool readCount, bool readOnly, bool containsNullPointer) where T : unmanaged
            => MemoryMarshal.Cast<byte, T>(InternalDeserializeSpan(MemoryMarshal.AsBytes(value), count * sizeof(T), raw, readCount, readOnly, containsNullPointer));

        #endregion Internal

        #region byte

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> DeserializeSpanByte()
            => InternalDeserializeSpan(default(Span<byte>), default, raw: AutoRaw, readCount: true, readOnly: false, containsNullPointer: true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> DeserializeRawSpanByte()
            => InternalDeserializeSpan(default(Span<byte>), default, raw: true, readCount: false, readOnly: false, containsNullPointer: true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> DeserializeSpanByte(Span<byte> value)
            => InternalDeserializeSpan(value, default, raw: false, readCount: true, readOnly: false, containsNullPointer: false);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> DeserializeRawSpanByte(Span<byte> value)
            => InternalDeserializeSpan(value, default, raw: true, readCount: false, readOnly: false, containsNullPointer: false);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> DeserializeSpanByte(int count)
            => InternalDeserializeSpan(default(Span<byte>), count, raw: false, readOnly: false, readCount: false, containsNullPointer: true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> DeserializeSpanByte(Span<byte> value, int count)
            => InternalDeserializeSpan(value, count, raw: false, readCount: false, readOnly: false, containsNullPointer: false);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> DeserializeReadOnlySpanByte()
            => InternalDeserializeSpan(default(Span<byte>), default, raw: AutoRaw, readOnly: false, readCount: true, containsNullPointer: true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> DeserializeRawReadOnlySpanByte()
            => InternalDeserializeSpan(default(Span<byte>), default, raw: true, readOnly: false, readCount: false, containsNullPointer: true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> DeserializeReadOnlySpanByte(int count)
            => InternalDeserializeSpan(default(Span<byte>), count, raw: false, readCount: false, readOnly: false, containsNullPointer: true);

        #endregion byte

        #region char

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(Span<char> value)
            => InternalSerialize(value, AutoRaw);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeRaw(Span<char> value)
            => InternalSerialize(value, raw: true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(ReadOnlySpan<char> value)
            => InternalSerialize(value, AutoRaw);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeRaw(ReadOnlySpan<char> value)
            => InternalSerialize(value, raw: true);

        #endregion char

        #region t

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize<T>(Span<T> value) where T : unmanaged
            => InternalSerialize<T>(value, AutoRaw);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeRaw<T>(Span<T> value) where T : unmanaged
            => InternalSerialize<T>(value, raw: true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize<T>(ReadOnlySpan<T> value) where T : unmanaged
            => InternalSerialize(value, AutoRaw);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeRaw<T>(ReadOnlySpan<T> value) where T : unmanaged
            => InternalSerialize(value, raw: true);

        #endregion t

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> DeserializeSpan<T>() where T : unmanaged
            => InternalDeserializeSpan<T>(default, default, raw: AutoRaw, readCount: true, readOnly: false, containsNullPointer: true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> DeserializeRawSpan<T>() where T : unmanaged
            => InternalDeserializeSpan<T>(default, default, raw: true, readCount: true, readOnly: false, containsNullPointer: true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> DeserializeSpan<T>(Span<T> value) where T : unmanaged
            => InternalDeserializeSpan(value, default, raw: false, readCount: true, readOnly: false, containsNullPointer: false);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> DeserializeRawSpan<T>(Span<T> value) where T : unmanaged
            => InternalDeserializeSpan(value, default, raw: true, readCount: false, readOnly: false, containsNullPointer: false);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> DeserializeSpan<T>(int count) where T : unmanaged
            => InternalDeserializeSpan<T>(default, count, raw: false, readCount: false, readOnly: false, containsNullPointer: true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> DeserializeSpan<T>(Span<T> value, int count) where T : unmanaged
            => InternalDeserializeSpan(value, count, raw: false, readCount: false, readOnly: false, containsNullPointer: false);





        public bool TryDeserializeSpan<T>(out Span<T> value) where T : unmanaged
        {
            value = Span<T>.Empty;

            var currentPosition = _position;

            var result = InternalTryDeserializeSequenceHeader(out var count)
                ? count == -1
                    ? false
                    : count == 0
                        ? true
                        : TryDeserializeSpan(out value, count)
                : false;

            if (!result)
            {
                if (_stream == null)
                    _position = currentPosition;
                else
                    Position = currentPosition;
            }

            return result;
        }

        public bool TryDeserializeSpan<T>(out Span<T> value, int bytesCount) where T : unmanaged
        {
            value = Span<T>.Empty;

            if (bytesCount <= 0)
                return false;

            if (!InternalTryEnsureDeserializeCapacity(bytesCount))
                return false;

            var currentPosition = _position;

            try
            {
                var span = new Span<byte>(new byte[bytesCount]);

                if (_stream == null)
                    _memory.Span.Slice(_position, bytesCount).CopyTo(span);
                else
                {
                    if (_stream.Read(span) != bytesCount)
                    {
                        Position = currentPosition;
                        return false;
                    }
                }

                _position += bytesCount;
                value = MemoryMarshal.Cast<byte, T>(span);
                return true;
            }
            catch
            {
                Position = currentPosition;
                return false;
            }
        }

        public Span<T> ToSpan<T>() where T : unmanaged
        {
            _position = 0;
            return DeserializeRawSpan<T>();
        }

        public Span<T> AsSpan<T>() where T : unmanaged
            => AsSpan<T>(0, _length);

        public Span<T> AsSpan<T>(int start) where T : unmanaged
            => AsSpan<T>(start, _length - start);

        /// <summary>
        /// Forms a span out of the current serializer starting at a specified index for a specified length.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="start">The index at which to begin this span.</param>
        /// <param name="length">The desired length for the span.</param>
        /// <returns>A span that consists of length elements from the current serializer starting at start.</returns>
        /// <exception cref="ArgumentOutOfRangeException">start or start + length is less than zero or greater than <see cref="Span{T}"/>.</exception>
        /// <exception cref="InvalidOperationException">Can't cast the current serializer as a <see cref="Span{T}"/>.</exception>
        public Span<T> AsSpan<T>(int start, int length) where T : unmanaged
        {
            if (start < 0 || start + length < 0 || start + length > _length)
                throw new ArgumentOutOfRangeException("start or start + length is less than zero or greater than System.Span`1.Length.");

            if (_stream == null)
                return MemoryMarshal.Cast<byte, T>(_memory.Span.Slice(start, length));

            if (_stream is MemoryStream memoryStream && memoryStream.TryGetBuffer(out var arraySegment))
                return MemoryMarshal.Cast<byte, T>(arraySegment.AsSpan(arraySegment.Offset + start, length));

            throw new InvalidOperationException($"Can't cast the current serializer as a {nameof(Span<T>)}");
        }

        public ReadOnlySpan<T> DeserializeReadOnlySpan<T>() where T : unmanaged
        {
            if (AutoRaw)
                return DeserializeRawReadOnlySpan<T>();

            var count = InternalDeserializeSequenceHeader();

            return count == -1 ? throw new InvalidOperationException
                (Utilities.ResourceStrings.NullReferenceFoundWhenDeserializingValueType(typeof(ReadOnlySpan<T>).Name))
                : count == 0 ? ReadOnlySpan<T>.Empty
                : DeserializeReadOnlySpan<T>(count);
        }

        public ReadOnlySpan<T> DeserializeRawReadOnlySpan<T>() where T : unmanaged
            => DeserializeReadOnlySpan<T>(_length - _position);

        public ReadOnlySpan<T> DeserializeReadOnlySpan<T>(int bytesCount) where T : unmanaged
        {
            if (bytesCount < 0)
                throw new ArgumentOutOfRangeException(nameof(bytesCount), $"Parameter {nameof(bytesCount)} must be a positive value");

            if (bytesCount == 0)
                return ReadOnlySpan<T>.Empty;

            InternalEnsureDeserializeCapacity(bytesCount);

            var bufferReadOnlySpan = new ReadOnlySpan<byte>(_buffer, _position, bytesCount);

            _position += bytesCount;

            return MemoryMarshal.Cast<byte, T>(bufferReadOnlySpan);
        }

        public bool TryDeserializeReadOnlySpan<T>(out ReadOnlySpan<T> value) where T : unmanaged
        {
            //value = ReadOnlySpan<T>.Empty;

            //var currentPosition = _position;

            //try
            //{
            //    value = DeserializeReadOnlySpan<T>();
            //    return true;
            //}
            //catch
            //{
            //    _position = currentPosition;
            //    return false;
            //}

            value = ReadOnlySpan<T>.Empty;

            var currentPosition = _position;

            var result = InternalTryDeserializeSequenceHeader(out var count)
                ? count == -1
                    ? false
                    : count == 0
                        ? true
                        : TryDeserializeReadOnlySpan(out value, count)
                : false;

            if (!result)
            {
                if (_stream != null)
                    try
                    {
                        if (_stream.CanSeek)
                            _stream.Position = currentPosition;
                    }
                    catch { }

                _position = currentPosition;
            }

            return result;
        }

        public bool TryDeserializeReadOnlySpan<T>(out ReadOnlySpan<T> value, int bytesCount) where T : unmanaged
        {
            value = ReadOnlySpan<T>.Empty;

            if (bytesCount <= 0)
                return false;

            if (!InternalTryEnsureDeserializeCapacity(bytesCount))
                return false;

            Span<byte> span;

            if (_stream == null)
            {
                span = _memory.Span.Slice(_position, bytesCount);
                _position += bytesCount;
            }
            else if (_stream is MemoryStream memoryStream && memoryStream.TryGetBuffer(out var arraySegment))
            {
                span = arraySegment.AsSpan(arraySegment.Offset, arraySegment.Count);
                Position += bytesCount;
            }
            else
                return false;

            value = MemoryMarshal.Cast<byte, T>(span);
            return true;
        }

        public ReadOnlySpan<T> ToReadOnlySpan<T>() where T : unmanaged
        {
            _position = 0;
            return DeserializeRawReadOnlySpan<T>();
        }

        public ReadOnlySpan<T> AsReadOnlySpan<T>() where T : unmanaged
            => AsSpan<T>();

        public ReadOnlySpan<T> AsReadOnlySpan<T>(int start) where T : unmanaged
            => AsSpan<T>(start);

        public ReadOnlySpan<T> AsReadOnlySpan<T>(int start, int length) where T : unmanaged
            => AsSpan<T>(start, length);
    }
}