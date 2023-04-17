using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cyxor.Serialization;

partial class Serializer
{
    public Serializer() { }

    public Serializer(in SerializerOptions options)
    {
        _options = options;
    }

    public Serializer(Stream stream, in SerializerOptions options = default, bool leaveOpen = false)
        : this(options)
    {
        _stream = stream;
        _needDisposeBuffer = !leaveOpen;
    }

    public Serializer(
        in ArraySegment<byte> arraySegment,
        in SerializerOptions options = default,
        bool sharedPoolBuffer = false
    )
        : this(arraySegment.Array, arraySegment.Offset, arraySegment.Count, options, sharedPoolBuffer) { }

    public Serializer(byte[]? value, in SerializerOptions options = default, bool sharedPoolBuffer = false)
        : this(value, 0, value?.Length ?? 0, options, sharedPoolBuffer) { }

    public Serializer(
        byte[]? value,
        int start,
        int length,
        in SerializerOptions options = default,
        bool sharedPoolBuffer = false
    )
        : this(options)
    {
        ValidateOptions(in options, predefinedBuffer: true);

        if (_options.InitialCapacity != 0 || _options.InitialCapacity != length)
            throw new ArgumentException(
                $"The {nameof(SerializerOptions)}.{nameof(SerializerOptions.InitialCapacity)} field must be set to 0 (the default) or to the supplied buffer length, any other value will produce this exception."
            );

        if (_options.MaxCapacity != 0 && _options.MaxCapacity < length)
            throw new ArgumentException(
                $"The {nameof(SerializerOptions)}.{nameof(SerializerOptions.MaxCapacity)} field must be set to 0 (the default) and not shorter than the supplied buffer length."
            );

        if (_options.FixedBuffer && _options.MaxCapacity > length)
            throw new ArgumentException(
                $"The {nameof(SerializerOptions)}.{nameof(SerializerOptions.MaxCapacity)} field can't be set larger than the supplied buffer length when {nameof(SerializerOptions)}.{nameof(SerializerOptions.FixedBuffer)} field is set to true."
            );

        _capacity = length;
        _needDisposeBuffer = sharedPoolBuffer;
        _memory = new Memory<byte>(value, start, length);
    }

    void ValidateOptions(in SerializerOptions options, bool predefinedBuffer)
    {
        if (options.InitialCapacity < 0)
            throw new ArgumentException(
                $"The {nameof(SerializerOptions)}.{nameof(SerializerOptions.InitialCapacity)} field must contain a positive value."
            );

        if (options.MaxCapacity < 0)
            throw new ArgumentException(
                $"The {nameof(SerializerOptions)}.{nameof(SerializerOptions.MaxCapacity)} field must contain a positive value."
            );

        if (options.PoolThreshold < 0)
            throw new ArgumentException(
                $"The {nameof(SerializerOptions)}.{nameof(SerializerOptions.PoolThreshold)} field must contain a positive value."
            );

        if (options.InitialCapacity > options.MaxCapacity)
            throw new ArgumentException(
                $"The {nameof(SerializerOptions)}.{nameof(SerializerOptions.InitialCapacity)} field can't be set larger than  {nameof(SerializerOptions)}.{nameof(SerializerOptions.MaxCapacity)} field."
            );

        if (!predefinedBuffer && options.FixedBuffer)
            throw new ArgumentException(
                $"The {nameof(SerializerOptions)}.{nameof(SerializerOptions.FixedBuffer)} field can't be set to true without supplying a predefined buffer."
            );
    }

    public Serializer(
        in Memory<byte> value,
        IMemoryOwner<byte>? memoryOwner = null,
        in SerializerOptions options = default
    )
        : this(options)
    {
        _memory = value;
        _memoryOwner = memoryOwner;
    }

    public Serializer(
        in ReadOnlyMemory<byte> value,
        IMemoryOwner<byte>? memoryOwner = null,
        in SerializerOptions options = default
    )
    {
        _options = options == default ? new SerializerOptions(readOnly: true) : options;

        if (!options.ReadOnly)
            throw new ArgumentException(
                $"The serializer {nameof(options)} must be equal to default or the {nameof(SerializerOptions)}.{nameof(SerializerOptions.ReadOnly)} field must be set to true",
                nameof(options)
            );

        _memory = MemoryMarshal.AsMemory(value);
        _memoryOwner = memoryOwner;
    }

    //public Serializer(object value)
    //    => SerializeRaw(value);

    //public Serializer(object value, IBackingSerializer backingSerializer, object? backingSerializerOptions = default)
    //    => SerializeRaw(value, backingSerializer, backingSerializerOptions);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void InternalDisposeMemory()
    {
        if (_options.ClearMemory)
            _memory.Span.Clear();

        if (_needDisposeBuffer)
        {
            if (_stream != null)
                _stream.Dispose();
            else if (MemoryMarshal.TryGetArray<byte>(_memory, out var arraySegment))
                if (arraySegment.Array is byte[] buffer)
                    ArrayPool<byte>.Shared.Return(buffer);
        }

        _memoryOwner?.Dispose();
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
            InternalDisposeMemory();

        base.Dispose(disposing);

        _disposed = true;
    }
}
