namespace Cyxor.Serialization;

partial class Serializer
{
    public byte[] DeserializeBytes()
    {
        if (AutoRaw)
            return DeserializeRawBytes();

        var count = InternalDeserializeSequenceHeader();

        return count == -1
            ? throw new InvalidOperationException(
                    Utilities.ResourceStrings.NullReferenceFoundWhenDeserializingNonNullableReference(
                        typeof(byte[]).Name
                    )
                )
            : count == 0 ? Array.Empty<byte>() : DeserializeBytes(count);
    }

    public byte[]? DeserializeNullableBytes()
    {
        if (AutoRaw)
            return DeserializeNullableRawBytes();

        var count = InternalDeserializeSequenceHeader();

        return count == -1 ? default : count == 0 ? Array.Empty<byte>() : DeserializeNullableBytes(count);
    }

    public byte[] DeserializeRawBytes() => DeserializeBytes(_length - _position);

    public byte[]? DeserializeNullableRawBytes() => DeserializeNullableBytes(_length - _position);

    public byte[] DeserializeBytes(int count)
    {
        if (count == 0)
            return Array.Empty<byte>();

        if (count < 0)
            throw new ArgumentOutOfRangeException(
                nameof(count),
                $"Parameter {nameof(count)} must be a positive value"
            );

        InternalEnsureDeserializeCapacity(count);

        var value = new byte[count];
        unsafe
        {
            fixed (
                byte* src = _buffer,
                    dest = value
            ) Buffer.MemoryCopy(src + _position, dest, count, count);
        }

        _position += count;
        return value;
    }

    public byte[]? DeserializeNullableBytes(int count)
    {
        if (count == 0)
            return default;

        if (count < 0)
            throw new ArgumentOutOfRangeException(
                nameof(count),
                $"Parameter {nameof(count)} must be a positive value"
            );

        InternalEnsureDeserializeCapacity(count);

        var value = new byte[count];
        unsafe
        {
            fixed (
                byte* src = _buffer,
                    dest = value
            ) Buffer.MemoryCopy(src + _position, dest, count, count);
        }

        _position += count;
        return value;
    }

    public void DeserializeBytes(byte[] dest, int offset = 0)
    {
        unsafe
        {
            fixed (byte* ptr = dest) DeserializeBytes(ptr + offset, dest.Length - offset, 0, zeroBytesToCopy: true);
        }
    }

    public void DeserializeBytes(byte[] dest, int offset, int count)
    {
        unsafe
        {
            fixed (byte* ptr = dest) DeserializeBytes(
                ptr + offset,
                dest.Length - offset,
                count,
                zeroBytesToCopy: false
            );
        }
    }

    public unsafe void DeserializeBytes(byte* destination, int destinationSize) =>
        DeserializeBytes(destination, destinationSize, 0, zeroBytesToCopy: true);

    public unsafe void DeserializeBytes(byte* destination, int destinationSize, int bytesToCopy) =>
        DeserializeBytes(destination, destinationSize, bytesToCopy, zeroBytesToCopy: false);

    unsafe void DeserializeBytes(byte* destination, int destinationSize, int bytesToCopy, bool zeroBytesToCopy)
    {
        if ((IntPtr)destination == IntPtr.Zero)
            throw new ArgumentNullException(nameof(destination));

        if (destinationSize < 0)
            throw new ArgumentOutOfRangeException(
                nameof(destinationSize),
                $"{nameof(destinationSize)} must be a positive value"
            );

        if (bytesToCopy < 0)
            throw new ArgumentOutOfRangeException(
                nameof(bytesToCopy),
                $"{nameof(bytesToCopy)} must be a positive value"
            );

        if (bytesToCopy == 0)
        {
            if (!zeroBytesToCopy)
                throw new ArgumentOutOfRangeException(
                    nameof(bytesToCopy),
                    $"{nameof(bytesToCopy)} must be greater than zero. To read the length from data use an overload."
                );

            bytesToCopy = InternalDeserializeSequenceHeader();

            if (bytesToCopy <= 0)
                return;
        }

        if (destinationSize - bytesToCopy < 0)
            throw new ArgumentOutOfRangeException(
                nameof(bytesToCopy),
                $"{nameof(bytesToCopy)} is greater than {nameof(destinationSize)}"
            );

        InternalEnsureDeserializeCapacity(bytesToCopy);

        fixed (byte* src = _buffer) Buffer.MemoryCopy(src + _position, destination, bytesToCopy, bytesToCopy);

        _position += bytesToCopy;
    }

    public bool TryDeserializeBytes([NotNullWhen(true)] out byte[]? value)
    {
        value = default;

        var currentPosition = _position;

        try
        {
            value = DeserializeNullableBytes();

            if (value == default)
            {
                _position -= 1;
                return false;
            }

            return true;
        }
        catch
        {
            _position = currentPosition;
            return false;
        }
    }

    public bool TryDeserializeNullableBytes(out byte[]? value)
    {
        value = default;

        var currentPosition = _position;

        try
        {
            value = DeserializeNullableBytes();
            return true;
        }
        catch
        {
            _position = currentPosition;
            return false;
        }
    }

    public bool TryDeserializeBytes([NotNullWhen(true)] out byte[]? value, int count)
    {
        value = default;

        if (count <= 0)
            return false;

        if (_length - _position < count)
            return false;

        var currentPosition = _position;

        try
        {
            value = DeserializeBytes(count);
            return true;
        }
        catch
        {
            _position = currentPosition;
            return false;
        }
    }

    public bool TryDeserializeNullableBytes(out byte[]? value, int count)
    {
        value = default;

        if (count <= 0)
            return false;

        if (_length - _position < count)
            return false;

        var currentPosition = _position;

        try
        {
            value = DeserializeNullableBytes(count);
            return true;
        }
        catch
        {
            _position = currentPosition;
            return false;
        }
    }

    public byte[] ToByteArray()
    {
        _position = 0;
        return DeserializeRawBytes();
    }

    public byte[]? ToNullableBytes()
    {
        _position = 0;
        return DeserializeNullableRawBytes();
    }
}
