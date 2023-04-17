using System.Buffers;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Cyxor.Serialization;

partial class Serializer
{
    #region Internal

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    unsafe void InternalSerializeUnmanaged<T>(T value)
        where T : unmanaged
    {
        var size = sizeof(T);
        InternalEnsureSerializeCapacity(size);

        if (_stream == null)
            MemoryMarshal.Write(_memory.Span.Slice(_position), ref value);
        else
            _stream.Write(new ReadOnlySpan<byte>(&value, size));

        _position += size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    unsafe void InternalSerializeUnmanaged<T>(T value, bool littleEndian)
        where T : unmanaged
    {
        var size = sizeof(T);
        InternalEnsureSerializeCapacity(size);

        var reverseEndianness = IsLittleEndian && !littleEndian || !IsLittleEndian && littleEndian;

        if (reverseEndianness && !_options.ReverseEndianness || !reverseEndianness && _options.ReverseEndianness)
            value = ReverseEndianness(value);

        if (_stream == null)
            MemoryMarshal.Write(_memory.Span.Slice(_position), ref value);
        else
            _stream.Write(new ReadOnlySpan<byte>(&value, size));

        _position += size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    unsafe void InternalSerializeUnmanagedUnconstrained<T>(T value)
    {
        var size = Unsafe.SizeOf<T>();
        InternalEnsureSerializeCapacity(size);

        if (_stream == null)
        {
            var span = _memory.Span.Slice(_position);
            ref var spanRef = ref MemoryMarshal.GetReference(span);
            var ptr = Unsafe.AsPointer(ref spanRef);
            Unsafe.Copy(ptr, ref value);
        }
        else
            _stream.Write(new ReadOnlySpan<byte>(Unsafe.AsPointer(ref value), size));

        _position += size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    unsafe void InternalSerializeUnmanagedUnconstrained<T>(in T value)
    {
        var size = Unsafe.SizeOf<T>();
        InternalEnsureSerializeCapacity(size);
        ref T refValue = ref Unsafe.AsRef(value);

        if (_stream == null)
        {
            var span = _memory.Span.Slice(_position);
            ref var spanRef = ref MemoryMarshal.GetReference(span);
            var ptr = Unsafe.AsPointer(ref spanRef);
            Unsafe.Copy(ptr, ref refValue);
        }
        else
            _stream.Write(new ReadOnlySpan<byte>(Unsafe.AsPointer(ref refValue), size));

        _position += size;
    }

    #endregion Internal

    #region Primitive types

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(bool value) => Serialize((byte)(value ? 1 : 0));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(char value) => InternalSerializeUnmanaged(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(Half value) => InternalSerializeUnmanaged(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(float value) => InternalSerializeUnmanaged(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(double value) => InternalSerializeUnmanaged(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(byte value)
    {
        InternalEnsureSerializeCapacity(sizeof(byte));

        if (_stream != null)
            _stream.WriteByte(value);
        else
            _memory.Span[_position] = value;

        _position++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(sbyte value) => Serialize((byte)value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(short value)
    {
        if (!_options.ReverseEndianness)
            InternalSerializeUnmanaged(value);
        else
            InternalSerializeUnmanaged(value, IsLittleEndian);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(short value, bool littleEndian) => InternalSerializeUnmanaged(value, littleEndian);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(ushort value)
    {
        if (!_options.ReverseEndianness)
            InternalSerializeUnmanaged(value);
        else
            InternalSerializeUnmanaged(value, IsLittleEndian);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(ushort value, bool littleEndian) => InternalSerializeUnmanaged(value, littleEndian);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(int value) => SerializeCompressedInt((uint)value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SerializeUncompressedInt32(int value)
    {
        if (!_options.ReverseEndianness)
            InternalSerializeUnmanaged(value);
        else
            InternalSerializeUnmanaged(value, IsLittleEndian);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SerializeUncompressedInt32(int value, bool littleEndian) =>
        InternalSerializeUnmanaged(value, littleEndian);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(uint value)
    {
        if (!_options.ReverseEndianness)
            InternalSerializeUnmanaged(value);
        else
            InternalSerializeUnmanaged(value, IsLittleEndian);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(uint value, bool littleEndian) => InternalSerializeUnmanaged(value, littleEndian);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(long value) => SerializeCompressedInt((ulong)value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SerializeUncompressedInt64(long value)
    {
        if (!_options.ReverseEndianness)
            InternalSerializeUnmanaged(value);
        else
            InternalSerializeUnmanaged(value, IsLittleEndian);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SerializeUncompressedInt64(long value, bool littleEndian) =>
        InternalSerializeUnmanaged(value, littleEndian);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(ulong value)
    {
        if (!_options.ReverseEndianness)
            InternalSerializeUnmanaged(value);
        else
            InternalSerializeUnmanaged(value, IsLittleEndian);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(ulong value, bool littleEndian) => InternalSerializeUnmanaged(value, littleEndian);

    #endregion Primitive types

    #region Struct types

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(decimal value) => InternalSerializeUnmanaged(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(BitSerializer value) => Serialize((long)value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(Guid value)
    {
        const int size = 16;
        InternalEnsureSerializeCapacity(size);

        if (_stream == null)
        {
            if (!value.TryWriteBytes(_memory.Span.Slice(_position, size)))
                throw new SerializationException(Utilities.ResourceStrings.CyxorInternalException);
        }
        else
        {
            Span<byte> span = stackalloc byte[size];

            if (!value.TryWriteBytes(span))
                throw new SerializationException(Utilities.ResourceStrings.CyxorInternalException);

            _stream.Write(span);
        }

        _position += size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(TimeSpan value) => SerializeUncompressedInt64(value.Ticks);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(DateTime value) => SerializeUncompressedInt64(value.ToBinary());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(DateTimeOffset value)
    {
        Serialize(value.DateTime);
        Serialize(value.Offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Serialize(BigInteger value)
    {
        if (value.IsZero)
        {
            Serialize(EmptyMap);
            return;
        }

        var length = value.GetByteCount();
        InternalSerializeSequenceHeader(length);
        InternalEnsureSerializeCapacity(length);

        if (_stream == null)
        {
            if (!value.TryWriteBytes(_memory.Span.Slice(_position, length), out _))
                throw new SerializationException(Utilities.ResourceStrings.CyxorInternalException);
        }
        else
        {
            if (_stream is MemoryStream memoryStream && memoryStream.TryGetBuffer(out var arraySegment))
            {
                var span = arraySegment.AsSpan(arraySegment.Offset, length);

                if (!value.TryWriteBytes(span, out _))
                    throw new SerializationException(Utilities.ResourceStrings.CyxorInternalException);

                _stream.Position += length;
            }
            else if (length <= 1536)
            {
                Span<byte> span = stackalloc byte[length];

                if (!value.TryWriteBytes(span, out _))
                    throw new SerializationException(Utilities.ResourceStrings.CyxorInternalException);

                _stream.Write(span);
            }
            else
            {
                var buffer = ArrayPool<byte>.Shared.Rent(length);
                var span = buffer.AsSpan(0..length);

                if (!value.TryWriteBytes(span, out _))
                    throw new SerializationException(Utilities.ResourceStrings.CyxorInternalException);

                _stream.Write(span);
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        _position += length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SerializeEnum<T>(T value)
        where T : unmanaged, Enum => Serialize(Unsafe.As<T, long>(ref value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SerializerMethodIdentifier(SerializerMethodIdentifier.SerializeUnmanaged)]
    public unsafe void Serialize<T>(in T value)
        where T : unmanaged
    {
        var size = sizeof(T);

        InternalEnsureSerializeCapacity(size);

        ref T refValue = ref Unsafe.AsRef(value);

        if (_stream == null)
            MemoryMarshal.Write(_memory.Span.Slice(_position), ref refValue);
        else
            _stream.Write(new ReadOnlySpan<byte>(Unsafe.AsPointer(ref refValue), size));

        _position += size;
    }
    #endregion Struct types
}
