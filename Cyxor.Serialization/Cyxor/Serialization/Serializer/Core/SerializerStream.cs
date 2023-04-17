using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Cyxor.Serialization;

partial class Serializer
{
    public override bool CanRead => !_disposed && (_stream?.CanRead ?? true);
    public override bool CanSeek => !_disposed && (_stream?.CanSeek ?? true);
    public override bool CanWrite => !_disposed && !_options.ReadOnly && (_stream?.CanWrite ?? true);

    public override long Position
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _stream?.Position ?? _position;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Serializer));

            if (_stream != null)
                _stream.Position = value;
            //else
            //{
            //    if (value < 0 || value > _length)
            //        throw new ArgumentOutOfRangeException(nameof(value));

            //    _position = (int)value;
            //}

            // TODO: Consider change _position type to long?
            _position = (int)value;
        }
    }

    public override long Length => _stream?.Length ?? _length;

    public override int ReadByte() => DeserializeByte();

    public override void WriteByte(byte value) => Serialize(value);

    public override int Read(byte[] buffer, int offset, int count)
    {
        count = count > _length - _position ? _length - _position : count;
        DeserializeBytes(buffer, offset, count);
        return count;
    }

    public override async Task<int> ReadAsync(
        byte[] buffer,
        int offset,
        int count,
        CancellationToken cancellationToken
    )
    {
        if (_stream != null)
        {
            var result = await _stream.ReadAsync(buffer, offset, count, cancellationToken);
            _position += result;
            return result;
        }
        else
        {
            var maxCount = _length - _position;
            count = maxCount < count ? maxCount : count;
            DeserializeBytes(buffer, offset, count);
            return count;
        }
    }

    public override int Read(Span<byte> buffer)
    {
        var initialPosition = _position;
        _ = DeserializeRawSpan(buffer);
        return _position - initialPosition;
    }

    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer,
        CancellationToken cancellationToken = default
    )
    {
        if (_stream == null)
            return DeserializeMemory(ref buffer);
        else
        {
            var result = await _stream.ReadAsync(buffer, cancellationToken);
            _position += result;
            return result;
        }
    }

    public override void Write(byte[] buffer, int offset, int count) => SerializeRaw(buffer, offset, count);

    override async public override void Write(ReadOnlySpan<byte> buffer) => SerializeRaw(buffer);

    public override void Flush() => _stream?.Flush();

    public override void SetLength(long value)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(Serializer));

        if (_stream != null)
            _stream.SetLength(value);
        else
            SetLength((int)value);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                Position = offset;
                break;
            case SeekOrigin.End:
                Position = (_stream?.Length ?? _length) - offset;
                break;
            case SeekOrigin.Current:
                Position = (_stream?.Position ?? _position) + offset;
                break;
        }

        return _stream?.Position ?? _position;
    }
}
