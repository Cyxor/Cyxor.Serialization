namespace Cyxor.Serialization;

using Extensions;

partial class Serializer
{
    public Memory<T> DeserializeMemory<T>()
        where T : unmanaged
    {
        if (AutoRaw)
            return DeserializeRawMemory<T>();

        var count = InternalDeserializeSequenceHeader();

        return count == -1
            ? throw new InvalidOperationException(
                    Utilities.ResourceStrings.NullReferenceFoundWhenDeserializingNonNullableReference(
                        typeof(Memory<T>).Name
                    )
                )
            : count == 0 ? Memory<T>.Empty : DeserializeMemory<T>(count);
    }

    public Memory<T> DeserializeRawMemory<T>()
        where T : unmanaged => DeserializeMemory<T>(_length - _position);

    public ref Memory<T> DeserializeRawMemory<T>(ref Memory<T> memory)
        where T : unmanaged => ref DeserializeMemory(ref memory, _length - _position);

    public Memory<T> DeserializeMemory<T>(int bytesCount)
        where T : unmanaged
    {
        var memory = new Memory<byte>(new byte[bytesCount]);
        _ = DeserializeMemory(ref memory, bytesCount);
        return memory.Cast<byte, T>();
    }

    public ref Memory<T> DeserializeMemory<T>(ref Memory<T> memory)
        where T : unmanaged
    {
        var count = InternalDeserializeSequenceHeader();

        if (count == -1)
            throw new InvalidOperationException(
                Utilities.ResourceStrings.NullReferenceFoundWhenDeserializingValueType(typeof(Span<T>).Name)
            );

        return ref count == 0 ? ref memory : ref DeserializeMemory(ref memory, count);
    }

    public ref Memory<T> DeserializeMemory<T>(ref Memory<T> memory, int bytesCount)
        where T : unmanaged
    {
        if (bytesCount < 0)
            throw new ArgumentOutOfRangeException(
                nameof(bytesCount),
                $"Parameter {nameof(bytesCount)} must be a positive value"
            );

        if (bytesCount > memory.Length)
            throw new ArgumentOutOfRangeException(nameof(bytesCount));

        if (bytesCount == 0)
            return ref memory;

        InternalEnsureDeserializeCapacity(bytesCount);

        var bufferMemory = new Memory<byte>(_buffer, _position, bytesCount);
        var memoryOfT = bufferMemory.Cast<byte, T>();

        memoryOfT.CopyTo(memory);
        _position += bytesCount;

        return ref memory;
    }

    public bool TryDeserializeMemory<T>(out Memory<T> value)
        where T : unmanaged
    {
        value = Memory<T>.Empty;

        var currentPosition = _position;

        try
        {
            value = DeserializeMemory<T>();
            return true;
        }
        catch
        {
            _position = currentPosition;
            return false;
        }
    }

    public bool TryDeserializeMemory<T>(out Memory<T> value, int bytesCount)
        where T : unmanaged
    {
        value = Memory<T>.Empty;

        if (bytesCount <= 0)
            return false;

        if (_length - _position < bytesCount)
            return false;

        var currentPosition = _position;

        try
        {
            value = DeserializeMemory<T>(bytesCount);
            return true;
        }
        catch
        {
            _position = currentPosition;
            return false;
        }
    }

    public Memory<T> ToMemory<T>()
        where T : unmanaged
    {
        _position = 0;
        return DeserializeRawMemory<T>();
    }

    public ReadOnlyMemory<T> DeserializeReadOnlyMemory<T>()
        where T : unmanaged
    {
        if (AutoRaw)
            return DeserializeRawReadOnlyMemory<T>();

        var count = InternalDeserializeSequenceHeader();

        return count == -1
            ? throw new InvalidOperationException(
                    Utilities.ResourceStrings.NullReferenceFoundWhenDeserializingValueType(
                        typeof(ReadOnlyMemory<T>).Name
                    )
                )
            : count == 0 ? ReadOnlyMemory<T>.Empty : DeserializeReadOnlyMemory<T>(count);
    }

    public ReadOnlyMemory<T> DeserializeRawReadOnlyMemory<T>()
        where T : unmanaged => DeserializeReadOnlyMemory<T>(_length - _position);

    public ReadOnlyMemory<T> DeserializeReadOnlyMemory<T>(int bytesCount)
        where T : unmanaged
    {
        if (bytesCount < 0)
            throw new ArgumentOutOfRangeException(
                nameof(bytesCount),
                $"Parameter {nameof(bytesCount)} must be a positive value"
            );

        if (bytesCount == 0)
            return ReadOnlyMemory<T>.Empty;

        InternalEnsureDeserializeCapacity(bytesCount);

        var bufferReadOnlyMemory = new ReadOnlyMemory<byte>(_buffer, _position, bytesCount);

        _position += bytesCount;

        return bufferReadOnlyMemory.Cast<byte, T>();
    }

    public bool TryDeserializeReadOnlyMemory<T>(out ReadOnlyMemory<T> value)
        where T : unmanaged
    {
        value = ReadOnlyMemory<T>.Empty;

        var currentPosition = _position;

        try
        {
            value = DeserializeReadOnlyMemory<T>();
            return true;
        }
        catch
        {
            _position = currentPosition;
            return false;
        }
    }

    public bool TryDeserializeReadOnlyMemory<T>(out ReadOnlyMemory<T> value, int bytesCount)
        where T : unmanaged
    {
        value = ReadOnlyMemory<T>.Empty;

        if (bytesCount <= 0)
            return false;

        if (_length - _position < bytesCount)
            return false;

        var currentPosition = _position;

        try
        {
            value = DeserializeReadOnlyMemory<T>(bytesCount);
            return true;
        }
        catch
        {
            _position = currentPosition;
            return false;
        }
    }

    public ReadOnlyMemory<T> ToReadOnlyMemory<T>()
        where T : unmanaged
    {
        _position = 0;
        return DeserializeRawReadOnlyMemory<T>();
    }
    // TODO: NULLABLE HERE







}
