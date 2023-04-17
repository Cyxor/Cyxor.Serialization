namespace Cyxor.Serialization;

partial class Serializer
{
    public char[] DeserializeChars()
    {
        if (AutoRaw)
            return DeserializeRawChars();

        var count = InternalDeserializeSequenceHeader();

        return count == -1
            ? throw new InvalidOperationException(
                    Utilities.ResourceStrings.NullReferenceFoundWhenDeserializingNonNullableReference(
                        typeof(char[]).Name
                    )
                )
            : count == 0 ? Array.Empty<char>() : DeserializeChars(count);
    }

    public char[]? DeserializeNullableChars()
    {
        if (AutoRaw)
            return DeserializeNullableRawChars();

        var count = InternalDeserializeSequenceHeader();

        return count == -1 ? default : count == 0 ? Array.Empty<char>() : DeserializeNullableChars(count);
    }

    public char[] DeserializeRawChars() => DeserializeChars(_length - _position);

    public char[]? DeserializeNullableRawChars() => DeserializeNullableChars(_length - _position);

    public char[] DeserializeChars(int byteCount)
    {
        if (byteCount == 0)
            return Array.Empty<char>();

        if (byteCount < 0)
            throw new ArgumentOutOfRangeException(
                nameof(byteCount),
                $"Parameter {nameof(byteCount)} must be a positive value"
            );

        InternalEnsureDeserializeCapacity(byteCount);

        _position += byteCount;

        return System.Text.Encoding.UTF8.GetChars(_buffer!, _position - byteCount, byteCount);
    }

    public char[]? DeserializeNullableChars(int byteCount)
    {
        if (byteCount == 0)
            return default;

        if (byteCount < 0)
            throw new ArgumentOutOfRangeException(
                nameof(byteCount),
                $"Parameter {nameof(byteCount)} must be a positive value"
            );

        InternalEnsureDeserializeCapacity(byteCount);

        _position += byteCount;

        return System.Text.Encoding.UTF8.GetChars(_buffer!, _position - byteCount, byteCount);
    }

    public int DeserializeChars(char[] chars, int offset = 0)
    {
        unsafe
        {
            fixed (char* ptr = chars) return DeserializeChars(
                ptr + offset,
                chars.Length - offset,
                0,
                zeroBytesToCopy: true
            );
        }
    }

    public int DeserializeChars(char[] chars, int offset, int byteCount)
    {
        unsafe
        {
            fixed (char* ptr = chars) return DeserializeChars(
                ptr + offset,
                chars.Length - offset,
                byteCount,
                zeroBytesToCopy: false
            );
        }
    }

    public unsafe int DeserializeChars(char* chars, int charCount) =>
        DeserializeChars(chars, charCount, 0, zeroBytesToCopy: true);

    public unsafe int DeserializeChars(char* chars, int charCount, int byteCount) =>
        DeserializeChars(chars, charCount, byteCount, zeroBytesToCopy: false);

    unsafe int DeserializeChars(char* chars, int charCount, int byteCount, bool zeroBytesToCopy)
    {
        var result = 0;

        if ((IntPtr)chars == IntPtr.Zero)
            throw new ArgumentNullException(nameof(chars));

        if (charCount < 0)
            throw new ArgumentOutOfRangeException(
                nameof(charCount),
                $"{nameof(charCount)} must be a positive value"
            );

        if (byteCount < 0)
            throw new ArgumentOutOfRangeException(
                nameof(byteCount),
                $"{nameof(byteCount)} must be a positive value"
            );

        if (byteCount == 0)
        {
            if (!zeroBytesToCopy)
                throw new ArgumentOutOfRangeException(
                    nameof(byteCount),
                    $"{nameof(byteCount)} must be greater than zero. To read the length from data use an overload."
                );

            byteCount = InternalDeserializeSequenceHeader();

            if (byteCount <= 0)
                return result;
        }

        InternalEnsureDeserializeCapacity(byteCount);

        fixed (byte* src = _buffer) result = System.Text.Encoding.UTF8.GetChars(
            src + _position,
            byteCount,
            chars,
            charCount
        );

        _position += byteCount;

        return result;
    }

    public bool TryDeserializeChars([NotNullWhen(true)] out char[]? value)
    {
        value = default;
        var currentPosition = _position;

        try
        {
            value = DeserializeNullableChars();

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

    public bool TryDeserializeNullableChars(out char[]? value)
    {
        value = default;
        var currentPosition = _position;

        try
        {
            value = DeserializeNullableChars();
            return true;
        }
        catch
        {
            _position = currentPosition;
            return false;
        }
    }

    public bool TryDeserializeChars([NotNullWhen(true)] out char[]? value, int count)
    {
        value = default;

        if (count <= 0)
            return false;

        if (_length - _position < count)
            return false;

        var currentPosition = _position;

        try
        {
            value = DeserializeChars(count);
            return true;
        }
        catch
        {
            _position = currentPosition;
            return false;
        }
    }

    public bool TryDeserializeNullableChars(out char[]? value, int count)
    {
        value = default;

        if (count <= 0)
            return false;

        if (_length - _position < count)
            return false;

        var currentPosition = _position;

        try
        {
            value = DeserializeNullableChars(count);
            return true;
        }
        catch
        {
            _position = currentPosition;
            return false;
        }
    }

    public char[] ToCharArray()
    {
        _position = 0;
        return DeserializeRawChars();
    }

    public char[]? ToNullableCharArray()
    {
        _position = 0;
        return DeserializeNullableRawChars();
    }
}
