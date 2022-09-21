using System;
using System.IO;
using System.Buffers;
using System.Numerics;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        #region Deserialize value types

        #region Internal

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe T InternalDeserializeUnmanaged<T>()
            where T : unmanaged
        {
            T value;
            var size = sizeof(T);
            InternalEnsureDeserializeCapacity(size);

            if (_stream == null)
                value = MemoryMarshal.Read<T>(_memory.Span.Slice(_position, size));
            else
            {
                Span<byte> span = stackalloc byte[size];

                if (_stream.Read(span) != size)
                    throw new SerializationException(Utilities.ResourceStrings.CyxorInternalException);

                value = MemoryMarshal.Read<T>(span);
            }

            _position += size;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe T InternalDeserializeUnmanaged<T>(bool littleEndian)
            where T : unmanaged
        {
            T value;
            var size = sizeof(T);
            InternalEnsureDeserializeCapacity(size);

            if (_stream == null)
                value = MemoryMarshal.Read<T>(_memory.Span.Slice(_position, size));
            else
            {
                Span<byte> span = stackalloc byte[size];

                if (_stream.Read(span) != size)
                    throw new SerializationException(Utilities.ResourceStrings.CyxorInternalException);

                value = MemoryMarshal.Read<T>(span);
            }

            var reverseEndianness = IsLittleEndian && !littleEndian || !IsLittleEndian && littleEndian;

            if (reverseEndianness && !_options.ReverseEndianness || !reverseEndianness && _options.ReverseEndianness)
                value = ReverseEndianness(value);

            _position += size;
            return value;
        }

        #endregion Internal

        #region Primitive types

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool DeserializeBoolean() => InternalDeserializeUnmanaged<byte>() != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char DeserializeChar() => InternalDeserializeUnmanaged<char>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float DeserializeSingle() => InternalDeserializeUnmanaged<float>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double DeserializeDouble() => InternalDeserializeUnmanaged<double>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte DeserializeByte()
        {
            InternalEnsureDeserializeCapacity(sizeof(byte));

            var @byte = _stream == null ? _memory.Span[_position] : (byte)_stream.ReadByte();

            _position++;
            return @byte;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte DeserializeSByte() => (sbyte)DeserializeByte();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short DeserializeInt16() =>
            !_options.ReverseEndianness
                ? InternalDeserializeUnmanaged<short>()
                : InternalDeserializeUnmanaged<short>(IsLittleEndian);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short DeserializeInt16(bool littleEndian) => InternalDeserializeUnmanaged<short>(littleEndian);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort DeserializeUInt16() =>
            !_options.ReverseEndianness
                ? InternalDeserializeUnmanaged<ushort>()
                : InternalDeserializeUnmanaged<ushort>(IsLittleEndian);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort DeserializeUInt16(bool littleEndian) => InternalDeserializeUnmanaged<ushort>(littleEndian);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int DeserializeInt32() => (int)DeserializeCompressedUInt32();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int DeserializeUncompressedInt32() =>
            !_options.ReverseEndianness
                ? InternalDeserializeUnmanaged<int>()
                : InternalDeserializeUnmanaged<int>(IsLittleEndian);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int DeserializeUncompressedInt32(bool littleEndian) => InternalDeserializeUnmanaged<int>(littleEndian);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint DeserializeUInt32() =>
            !_options.ReverseEndianness
                ? InternalDeserializeUnmanaged<uint>()
                : InternalDeserializeUnmanaged<uint>(IsLittleEndian);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint DeserializeUInt32(bool littleEndian) => InternalDeserializeUnmanaged<uint>(littleEndian);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long DeserializeInt64() => (long)DeserializeCompressedUInt64();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long DeserializeUncompressedInt64() =>
            !_options.ReverseEndianness
                ? InternalDeserializeUnmanaged<long>()
                : InternalDeserializeUnmanaged<long>(IsLittleEndian);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long DeserializeUncompressedInt64(bool littleEndian) => InternalDeserializeUnmanaged<long>(littleEndian);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong DeserializeUInt64() =>
            !_options.ReverseEndianness
                ? InternalDeserializeUnmanaged<ulong>()
                : InternalDeserializeUnmanaged<ulong>(IsLittleEndian);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong DeserializeUInt64(bool littleEndian) => InternalDeserializeUnmanaged<ulong>(littleEndian);

        #endregion Primitive types

        #region Struct types

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal DeserializeDecimal() => InternalDeserializeUnmanaged<decimal>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitSerializer DeserializeBitSerializer() => DeserializeInt64();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid DeserializeGuid()
        {
            Guid guid;
            const int size = 16;
            InternalEnsureDeserializeCapacity(size);

            if (_stream == null)
                guid = new Guid(_memory.Span.Slice(_position, size));
            else
            {
                Span<byte> span = stackalloc byte[size];

                if (_stream.Read(span) != size)
                    throw new SerializationException(Utilities.ResourceStrings.CyxorInternalException);

                guid = new Guid(span);
            }

            _position += size;
            return guid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TimeSpan DeserializeTimeSpan() => TimeSpan.FromTicks(DeserializeUncompressedInt64());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime DeserializeDateTime() => DateTime.FromBinary(DeserializeUncompressedInt64());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTimeOffset DeserializeDateTimeOffset()
        {
            var dateTime = DeserializeDateTime();
            var timeSpan = DeserializeTimeSpan();

            return new DateTimeOffset(dateTime, timeSpan);
        }

        public BigInteger DeserializeBigInteger()
        {
            BigInteger value;
            var length = InternalDeserializeSequenceHeader();

            if (length == -1)
                throw new SerializationException(
                    $"Nullable {nameof(BigInteger)} instance found when deserializing non-nullable {nameof(BigInteger)} value"
                );

            InternalEnsureDeserializeCapacity(length);

            if (length == 0)
                return BigInteger.Zero;
            else if (_stream == null)
                value = new BigInteger(_memory.Span.Slice(_position, length));
            else if (_stream is MemoryStream memoryStream && memoryStream.TryGetBuffer(out var arraySegment))
            {
                value = new BigInteger(arraySegment.AsSpan((int)memoryStream.Position, length));
                memoryStream.Position += length;
            }
            else if (length > 1536)
            {
                var buffer = ArrayPool<byte>.Shared.Rent(length);

                if (_stream.Read(buffer, 0, length) != length)
                    throw new SerializationException(Utilities.ResourceStrings.CyxorInternalException);

                value = new BigInteger(buffer.AsSpan(0, length));
                ArrayPool<byte>.Shared.Return(buffer);
            }
            else
            {
                Span<byte> span = stackalloc byte[length];

                if (_stream.Read(span) != length)
                    throw new SerializationException(Utilities.ResourceStrings.CyxorInternalException);

                value = new BigInteger(span);
            }

            _position += length;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T DeserializeEnum<T>()
            where T : unmanaged, Enum
        {
            var value = DeserializeInt64();
            return Unsafe.As<long, T>(ref value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SerializerMethodIdentifier(SerializerMethodIdentifier.DeserializeUnmanaged)]
        public T DeserializeUnmanaged<T>()
            where T : unmanaged => InternalDeserializeUnmanaged<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref readonly T DeserializeUnmanagedReference<T>()
            where T : unmanaged
        {
            var size = sizeof(T);
            InternalEnsureDeserializeCapacity(size);

            try
            {
                if (_stream == null)
                    return ref MemoryMarshal.AsRef<T>(_memory.Span.Slice(_position, size));
                else if (_stream is MemoryStream memoryStream && memoryStream.TryGetBuffer(out var arraySegment))
                {
                    var span = arraySegment.AsSpan(arraySegment.Offset, size);
                    return ref MemoryMarshal.AsRef<T>(span);
                }
                else
                {
                    Span<byte> span = new byte[size];

                    if (_stream.Read(span) != size)
                        throw new SerializationException(Utilities.ResourceStrings.CyxorInternalException);

                    return ref MemoryMarshal.AsRef<T>(span);
                }
            }

            finally
            {
                _position += size;
            }
        }

        #endregion Struct types

        #endregion Deserialize value types

        #region Try deserialize value types

        #region Internal

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe bool InternalTryDeserializeUnmanaged<T>(out T value)
            where T : unmanaged
        {
            value = default;
            var size = sizeof(T);

            if (!InternalTryEnsureDeserializeCapacity(size))
                return false;

            if (_stream == null)
                value = MemoryMarshal.Read<T>(_memory.Span.Slice(_position, size));
            else
            {
                Span<byte> span = stackalloc byte[size];

                if (_stream.Read(span) != size)
                    throw new SerializationException(Utilities.ResourceStrings.CyxorInternalException);

                value = MemoryMarshal.Read<T>(span);
            }

            _position += size;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe bool InternalTryDeserializeUnmanaged<T>(out T value, bool littleEndian)
            where T : unmanaged
        {
            value = default;
            var size = sizeof(T);

            if (!InternalTryEnsureDeserializeCapacity(size))
                return false;

            if (_stream == null)
                value = MemoryMarshal.Read<T>(_memory.Span.Slice(_position, size));
            else
            {
                Span<byte> span = stackalloc byte[size];

                if (_stream.Read(span) != size)
                    throw new SerializationException(Utilities.ResourceStrings.CyxorInternalException);

                value = MemoryMarshal.Read<T>(span);
            }

            var reverseEndianness = IsLittleEndian && !littleEndian || !IsLittleEndian && littleEndian;

            if (reverseEndianness && !_options.ReverseEndianness || !reverseEndianness && _options.ReverseEndianness)
                value = ReverseEndianness(value);

            _position += size;
            return true;
        }

        #endregion Internal

        #region Primitive types

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDeserializeBoolean(out bool value)
        {
            value = default;

            if (!TryDeserializeByte(out var tValue))
                return false;

            value = tValue != 0;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDeserializeChar(out char value) => InternalTryDeserializeUnmanaged(out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDeserializeSingle(out float value) => InternalTryDeserializeUnmanaged(out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDeserializeDouble(out double value) => InternalTryDeserializeUnmanaged(out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDeserializeByte(out byte value)
        {
            value = default;

            if (!InternalTryEnsureDeserializeCapacity(sizeof(byte)))
                return false;

            value = _stream == null ? _memory.Span[_position] : (byte)_stream.ReadByte();

            _position++;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDeserializeSByte(out sbyte value)
        {
            value = default;

            if (!TryDeserializeByte(out var tValue))
                return false;

            value = (sbyte)tValue;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDeserializeInt16(out short value) =>
            !_options.ReverseEndianness
                ? InternalTryDeserializeUnmanaged(out value)
                : InternalTryDeserializeUnmanaged(out value, IsLittleEndian);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDeserializeInt16(out short value, bool littleEndian) =>
            InternalTryDeserializeUnmanaged(out value, littleEndian);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDeserializeUInt16(out ushort value) =>
            !_options.ReverseEndianness
                ? InternalTryDeserializeUnmanaged(out value)
                : InternalTryDeserializeUnmanaged(out value, IsLittleEndian);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDeserializeUInt16(out ushort value, bool littleEndian) =>
            InternalTryDeserializeUnmanaged(out value, littleEndian);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDeserializeInt32(out int value)
        {
            value = default;

            if (!TryDeserializeCompressedUInt32(out var tValue))
                return false;

            value = (int)tValue;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDeserializeUncompressedInt32(out int value) =>
            !_options.ReverseEndianness
                ? InternalTryDeserializeUnmanaged(out value)
                : InternalTryDeserializeUnmanaged(out value, IsLittleEndian);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDeserializeUncompressedInt32(out int value, bool littleEndian) =>
            InternalTryDeserializeUnmanaged(out value, littleEndian);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDeserializeUInt32(out uint value) =>
            !_options.ReverseEndianness
                ? InternalTryDeserializeUnmanaged(out value)
                : InternalTryDeserializeUnmanaged(out value, IsLittleEndian);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDeserializeUInt32(out uint value, bool littleEndian) =>
            InternalTryDeserializeUnmanaged(out value, littleEndian);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDeserializeInt64(out long value)
        {
            value = default;

            if (!TryDeserializeCompressedUInt64(out var tValue))
                return false;

            value = (long)tValue;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDeserializeUncompressedInt64(out long value) =>
            !_options.ReverseEndianness
                ? InternalTryDeserializeUnmanaged(out value)
                : InternalTryDeserializeUnmanaged(out value, IsLittleEndian);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDeserializeUncompressedInt64(out long value, bool littleEndian) =>
            InternalTryDeserializeUnmanaged(out value, littleEndian);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDeserializeUInt64(out ulong value) =>
            !_options.ReverseEndianness
                ? InternalTryDeserializeUnmanaged(out value)
                : InternalTryDeserializeUnmanaged(out value, IsLittleEndian);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDeserializeUInt64(out ulong value, bool littleEndian) =>
            InternalTryDeserializeUnmanaged(out value, littleEndian);

        #endregion Primitive types

        #region Struct types

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDeserializeDecimal(out decimal value) => InternalTryDeserializeUnmanaged(out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDeserializeBitSerializer(out BitSerializer value)
        {
            value = default;

            if (!TryDeserializeInt64(out var tValue))
                return false;

            value = tValue;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDeserializeGuid(out Guid guid)
        {
            guid = default;
            const int size = 16;

            if (!InternalTryEnsureDeserializeCapacity(size))
                return false;

            if (_stream == null)
                guid = new Guid(_memory.Span.Slice(_position, size));
            else
            {
                Span<byte> span = stackalloc byte[size];

                if (_stream.Read(span) != size)
                    throw new SerializationException(Utilities.ResourceStrings.CyxorInternalException);

                guid = new Guid(span);
            }

            _position += size;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDeserializeTimeSpan(out TimeSpan value)
        {
            value = default;

            if (!TryDeserializeUncompressedInt64(out var tValue))
                return false;

            value = TimeSpan.FromTicks(tValue);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDeserializeDateTime(out DateTime value)
        {
            value = default;

            if (!TryDeserializeUncompressedInt64(out var tValue))
                return false;

            value = DateTime.FromBinary(tValue);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDeserializeDateTimeOffset(out DateTimeOffset value)
        {
            value = default;

            if (!TryDeserializeDateTime(out var dateTime))
                return false;

            if (!TryDeserializeTimeSpan(out var timeSpan))
                return false;

            value = new DateTimeOffset(dateTime, timeSpan);
            return true;
        }

        public bool TryDeserializeBigInteger(out BigInteger value)
        {
            value = default;
            var initialPosition = _position;

            if (
                !InternalTryDeserializeSequenceHeader(out var length)
                || length == -1
                || !InternalTryEnsureDeserializeCapacity(length)
            ) {
                if (initialPosition != _position)
                    if (_stream != null)
                        _stream.Position = initialPosition;

                _position = initialPosition;

                return false;
            }

            if (length == 0)
                value = BigInteger.Zero;
            else if (_stream == null)
                value = new BigInteger(_memory.Span.Slice(_position, length));
            else if (_stream is MemoryStream memoryStream && memoryStream.TryGetBuffer(out var arraySegment))
            {
                value = new BigInteger(arraySegment.AsSpan((int)memoryStream.Position, length));
                memoryStream.Position += length;
            }
            else if (length > 1536)
            {
                var buffer = ArrayPool<byte>.Shared.Rent(length);

                if (_stream.Read(buffer, 0, length) != length)
                    throw new SerializationException(Utilities.ResourceStrings.CyxorInternalException);

                value = new BigInteger(buffer.AsSpan(0, length));
                ArrayPool<byte>.Shared.Return(buffer);
            }
            else
            {
                Span<byte> span = stackalloc byte[length];

                if (_stream.Read(span) != length)
                    throw new SerializationException(Utilities.ResourceStrings.CyxorInternalException);

                value = new BigInteger(span);
            }

            _position += length;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDeserializeEnum<T>(out T value)
            where T : unmanaged, Enum
        {
            value = default;

            if (!TryDeserializeInt64(out var tValue))
                return false;

            value = Unsafe.As<long, T>(ref tValue);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDeserializeUnmanaged<T>(out T value)
            where T : unmanaged => InternalTryDeserializeUnmanaged<T>(out value);
        #endregion Struct types

        #endregion Try deserialize value types
    }
}
