using System;
using System.Buffers;
using System.Text.Unicode;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe int Wcslen(char* value)
        {
            var pointer = value;

            while (((uint)pointer & 3) != 0 && *pointer != 0)
                pointer++;

            if (*pointer != 0)
                while ((pointer[0] & pointer[1]) != 0 || pointer[0] != 0 && pointer[1] != 0)
                    pointer += 2;

            for (; *pointer != 0; pointer++)
                ;

            return (int)(pointer - value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InternalSerialize(ReadOnlySpan<char> value, bool raw, bool containsNullPointer = false)
        {
            if (containsNullPointer || value.IsEmpty)
            {
                InternalSerialize(MemoryMarshal.Cast<char, byte>(value), raw, containsNullPointer);
                return;
            }

            var count = value.Length;
            var startPosition = _position;

            if (!raw)
            {
                if (count * 2 > ushort.MaxValue)
                {
                    Serialize((byte)(EmptyMap | 127));
                    _position += 4;
                }
                else if (count * 2 > byte.MaxValue)
                {
                    Serialize((byte)(EmptyMap | 126));
                    _position += 2;
                }
                else if (count > 124)
                {
                    Serialize((byte)(EmptyMap | 125));
                    _position += 1;
                }
                else
                    Serialize((byte)(EmptyMap | count));
            }

            EnsureCapacity(count, SerializerOperation.Serialize);
            var serializerSpan = _buffer.AsSpan(_position..Capacity);
            var operationStatus = Utf8.FromUtf16(value, serializerSpan, out var charsRead, out var bytesWritten);

            _position += bytesWritten;

            if (_length < _position)
                _length = _position;

            var totalBytesWritten = bytesWritten;

            if (operationStatus != OperationStatus.Done && operationStatus != OperationStatus.DestinationTooSmall)
                throw new ArgumentException("Invalid Span<char> value", nameof(value));
            else if (operationStatus == OperationStatus.DestinationTooSmall)
            {
                var currentLength = _length;
                var freeCapacity = MaxCapacity - _position;
                var requiredCapacity = (count - charsRead) * 2;
                requiredCapacity = freeCapacity < requiredCapacity ? freeCapacity : requiredCapacity;

                EnsureCapacity(requiredCapacity, SerializerOperation.Serialize);

                value = value.Slice(charsRead);

                serializerSpan = _buffer.AsSpan(_position..Capacity);
                operationStatus = Utf8.FromUtf16(value, serializerSpan, out _, out bytesWritten);

                if (operationStatus != OperationStatus.Done)
                    throw new ArgumentException("Invalid Span<char> value", nameof(value));

                _position += bytesWritten;
                totalBytesWritten += bytesWritten;
                _length = _position > currentLength ? _position : currentLength;
            }

            if (!raw)
            {
                var endPosition = _position;
                _position = startPosition + 1;

                if (count * 2 > ushort.MaxValue)
                    SerializeUncompressedInt32(totalBytesWritten);
                else if (count * 2 > byte.MaxValue)
                    Serialize((short)totalBytesWritten);
                else if (count > 124)
                    Serialize((byte)totalBytesWritten);
                else
                {
                    _position--;

                    if (totalBytesWritten <= 124)
                        Serialize((byte)(EmptyMap | totalBytesWritten));
                    else
                    {
                        _buffer.AsSpan((startPosition + 1)..totalBytesWritten)
                            .CopyTo(_buffer.AsSpan((startPosition + 2)..totalBytesWritten));

                        _length++;
                        endPosition++;

                        Serialize((byte)(EmptyMap | 125));
                        Serialize((byte)totalBytesWritten);
                    }
                }

                _position = endPosition;
            }
        }

        public void Serialize(char[]? value)
            => InternalSerialize(new ReadOnlySpan<char>(value), raw: AutoRaw, containsNullPointer: value == null);

        public void Serialize(char[]? value, int start, int length)
            => InternalSerialize(new ReadOnlySpan<char>(value, start, length), raw: false, containsNullPointer: value == null);

        public void SerializeRaw(char[]? value)
            => InternalSerialize(new ReadOnlySpan<char>(value), raw: true, containsNullPointer: value == null);

        public void SerializeRaw(char[]? value, int start, int length)
            => InternalSerialize(new ReadOnlySpan<char>(value, start, length), raw: true, containsNullPointer: value == null);

        public unsafe void Serialize(char* value)
            => InternalSerialize(new ReadOnlySpan<char>(value, value == null ? 0 : Wcslen(value)), raw: AutoRaw, containsNullPointer: value == null);

        public unsafe void Serialize(char* value, int length)
            => InternalSerialize(new ReadOnlySpan<char>(value, length), raw: false, containsNullPointer: value == null);

        public unsafe void SerializeRaw(char* value)
            => InternalSerialize(new ReadOnlySpan<char>(value, value == null ? 0 : Wcslen(value)), raw: true, containsNullPointer: value == null);

        public unsafe void SerializeRaw(char* value, int length)
            => InternalSerialize(new ReadOnlySpan<char>(value, length), raw: true, containsNullPointer: value == null);
    }
}