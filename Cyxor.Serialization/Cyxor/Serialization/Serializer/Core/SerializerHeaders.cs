using System;
using System.Runtime.CompilerServices;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        const byte ObjectLengthMap = 0b_00_111111; // 63
        const byte SequenceLengthMap = 0b_0_1111111; // 127

        const byte NullMap = 0b_00_000000; // 0
        const byte EmptyMap = 0b_10_000000; // 128
        const byte PartialMap = 0b_01_000000; // 64
        const byte CircularMap = 0b_11_000000; // 192

        #region Internal

        #region Sequence/Object

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void InternalSerializeSequenceHeader(int count)
        {
            if (count < SequenceLengthMap)
                Serialize((byte)count);
            else
                Serialize(count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int InternalDeserializeSequenceHeader()
        {
            var op = DeserializeByte();

            if (op == NullMap)
                return -1;
            else if (op == EmptyMap)
                return 0;
            else if ((op & EmptyMap) != EmptyMap)
                return op & SequenceLengthMap;
            else
            {
                var val1 = (uint)op & 127;
                var val2 = 7;

                while (val2 != 35)
                {
                    op = DeserializeByte();
                    val1 |= ((uint)op & 127) << val2;
                    val2 += 7;

                    if ((op & 128) == 0)
                        return (int)(val1 >> 1) ^ -(int)(val1 & 1);
                }

                throw new InvalidOperationException("Invalid deserialize sequence header");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool InternalTryDeserializeSequenceHeader(out int value)
        {
            var startPosition = _stream != null ? _position : Position;

            if (TryDeserializeByte(out var op))
            {
                if (op == 0)
                    value = -1;
                else if (op == EmptyMap)
                    value = 0;
                else if ((op & EmptyMap) != EmptyMap)
                    value = op & SequenceLengthMap;
                else
                {
                    var val1 = (uint)op & 127;
                    var val2 = 7;

                    while (val2 != 35)
                    {
                        if (!TryDeserializeByte(out op))
                            break;

                        val1 |= ((uint)op & 127) << val2;
                        val2 += 7;

                        if ((op & 128) == 0)
                        {
                            value = (int)(val1 >> 1) ^ -(int)(val1 & 1);
                            return true;
                        }
                    }

                    goto False;
                }

                return true;
            }

        False:
            {
                if (_stream == null)
                    _position = (int)startPosition;
                else
                    Position = startPosition;

                value = -2;
                return false;
            }
        }

        #endregion Sequence/Object

        #region String (Utf16)

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void InternalStartSerializeStringHeader(int count)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void InternalEndSerializeStringHeader(int count, int totalBytesWritten, int startPosition)
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
                    _memory.Span.Slice(startPosition + 1, totalBytesWritten)
                        .CopyTo(_memory.Span.Slice(startPosition + 2, totalBytesWritten));

                    _length++;
                    endPosition++;

                    Serialize((byte)(EmptyMap | 125));
                    Serialize((byte)totalBytesWritten);
                }
            }

            _position = endPosition;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int InternalDeserializeStringHeader()
        {
            var count = DeserializeByte();

            return count == (EmptyMap | 127)
                ? DeserializeUncompressedInt32()
                : count == (EmptyMap | 126)
                        ? DeserializeInt16()
                        : count == (EmptyMap | 125) ? DeserializeByte() : count & SequenceLengthMap;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool InternalTryDeserializeStringHeader(out int length)
        {
            bool result;

            if (!(result = TryDeserializeByte(out var count)))
                length = -2;
            else
            {
                if (count == (EmptyMap | 127))
                    result = TryDeserializeUncompressedInt32(out length);
                else if (count == (EmptyMap | 126))
                {
                    result = TryDeserializeInt16(out var sLength);
                    length = sLength;
                }
                else if (count == (EmptyMap | 125))
                {
                    result = TryDeserializeByte(out var bLength);
                    length = bLength;
                }
                else
                    length = count & SequenceLengthMap;
            }

            return result;
        }

        #endregion String (Utf16)

        #endregion Internal

        /// <summary>
        /// Reads the next object length in bytes and advances the internal position consequently.
        /// It only works correctly if the object was serialized with the length prefixed.
        /// By default unmanaged types like primitive types and custom objects (classes and structs) does not use length prefixes.
        /// On the contrary, byte sequences, arrays, collections and strings when not serialized as raw objects does prefix the length.
        /// Since string objects use a different mechanism for prefixing its length you should use <see cref="ReadNextStringLength"/>.
        /// </summary>
        /// <returns>
        /// <list type="bullet">
        /// <listheader>One of the following values:</listheader>
        /// <item>-1 if the object is null</item>
        /// <item> 0 if the object is empty</item>
        /// <item> n where 'n' represents the object length in bytes</item>
        /// </list>
        /// </returns>
        /// <seealso cref="PeekNextObjectLength(bool)"/>
        public int ReadNextObjectLength() => InternalDeserializeSequenceHeader();

        public int ReadNextStringLength() => InternalDeserializeStringHeader();

        public bool TryReadNextObjectLength(out int length) => InternalTryDeserializeSequenceHeader(out length);

        public bool TryReadNextStringLength(out int length) => InternalTryDeserializeStringHeader(out length);

        /// <summary>
        /// Peeks the next object length in bytes without advancing the internal position.
        /// It only works correctly if the object was serialized with the length prefixed.
        /// By default unmanaged types like primitive types and custom objects (classes and structs) does not use length prefixes.
        /// On the contrary, byte sequences, arrays, collections and strings when not serialized as raw objects does prefix the length.
        /// Since string objects use a different mechanism for prefixing its length you should indicate if the object is a string.
        /// </summary>
        /// <returns>
        /// <list type="bullet">
        /// <listheader>One of the following values:</listheader>
        /// <item>-1 if the object is null</item>
        /// <item> 0 if the object is empty</item>
        /// <item> n where 'n' represents the object length in bytes</item>
        /// </list>
        /// </returns>
        /// <seealso cref="ReadNextObjectLength(bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int PeekNextObjectLength()
        {
            if (_stream != null && !_stream.CanSeek)
                throw new InvalidOperationException("The backing stream buffer doesn't support seeking.");

            var initialPosition = _position;
            var length = ReadNextObjectLength();

            if (_stream == null)
                _position = initialPosition;
            else
                Position = initialPosition;

            return length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int PeekNextStringLength()
        {
            if (_stream != null && !_stream.CanSeek)
                throw new InvalidOperationException("The backing stream buffer doesn't support seeking.");

            var initialPosition = _position;
            var length = ReadNextStringLength();

            if (_stream == null)
                _position = initialPosition;
            else
                Position = initialPosition;

            return length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeekNextObjectLength(out int length)
        {
            if (_stream != null && !_stream.CanSeek)
            {
                length = -2;
                return false;
            }

            var initialPosition = _position;

            if (TryReadNextObjectLength(out length))
            {
                if (_stream == null)
                    _position = initialPosition;
                else
                    Position = initialPosition;

                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeekNextStringLength(out int length)
        {
            if (_stream != null && !_stream.CanSeek)
            {
                length = -2;
                return false;
            }

            var initialPosition = _position;

            if (TryPeekNextStringLength(out length))
            {
                if (_stream == null)
                    _position = initialPosition;
                else
                    Position = initialPosition;

                return true;
            }

            return false;
        }
    }
}
