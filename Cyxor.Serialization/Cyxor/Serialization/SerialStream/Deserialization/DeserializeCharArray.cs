using System;
using System.Diagnostics.CodeAnalysis;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        public char[] DeserializeChars()
        {
            if (AutoRaw)
                return DeserializeRawChars();

            var count = DeserializeOp();

            return count == -1 ? throw new InvalidOperationException(Utilities.ResourceStrings.NullReferenceFoundWhenDeserializingNonNullableReference(typeof(char[]).Name))
                : count == 0 ? Utilities.Array.Empty<char>()
                : DeserializeChars(count);
        }

        public char[]? DeserializeNullableChars()
        {
            if (AutoRaw)
                return DeserializeNullableRawChars();

            var count = DeserializeOp();

            return count == -1 ? default
                : count == 0 ? Utilities.Array.Empty<char>()
                : DeserializeNullableChars(count);
        }

        public char[] DeserializeRawChars()
            => DeserializeChars(length - position);

        public char[]? DeserializeNullableRawChars()
            => DeserializeNullableChars(length - position);

        public char[] DeserializeChars(int byteCount)
        {
            if (byteCount == 0)
                return Utilities.Array.Empty<char>();

            if (byteCount < 0)
                throw new ArgumentOutOfRangeException(nameof(byteCount), $"Parameter {nameof(byteCount)} must be a positive value");

            EnsureCapacity(byteCount, SerializerOperation.Deserialize);

            position += byteCount;

            return Encoding.GetChars(buffer!, position - byteCount, byteCount);
        }

        public char[]? DeserializeNullableChars(int byteCount)
        {
            if (byteCount == 0)
                return default;

            if (byteCount < 0)
                throw new ArgumentOutOfRangeException(nameof(byteCount), $"Parameter {nameof(byteCount)} must be a positive value");

            EnsureCapacity(byteCount, SerializerOperation.Deserialize);

            position += byteCount;

            return Encoding.GetChars(buffer!, position - byteCount, byteCount);
        }

        public int DeserializeChars(char[] chars, int offset = 0)
        {
            unsafe
            {
                fixed (char* ptr = chars)
                    return DeserializeChars(ptr + offset, chars.Length - offset, 0, zeroBytesToCopy: true);
            }
        }

        public int DeserializeChars(char[] chars, int offset, int byteCount)
        {
            unsafe
            {
                fixed (char* ptr = chars)
                    return DeserializeChars(ptr + offset, chars.Length - offset, byteCount, zeroBytesToCopy: false);
            }
        }

        public unsafe int DeserializeChars(char* chars, int charCount)
            => DeserializeChars(chars, charCount, 0, zeroBytesToCopy: true);

        public unsafe int DeserializeChars(char* chars, int charCount, int byteCount)
            => DeserializeChars(chars, charCount, byteCount, zeroBytesToCopy: false);

        unsafe int DeserializeChars(char* chars, int charCount, int byteCount, bool zeroBytesToCopy)
        {
            var result = 0;

            if ((IntPtr)chars == IntPtr.Zero)
                throw new ArgumentNullException(nameof(chars));

            if (charCount < 0)
                throw new ArgumentOutOfRangeException(nameof(charCount), $"{nameof(charCount)} must be a positive value");

            if (byteCount < 0)
                throw new ArgumentOutOfRangeException(nameof(byteCount), $"{nameof(byteCount)} must be a positive value");

            if (byteCount == 0)
            {
                if (!zeroBytesToCopy)
                    throw new ArgumentOutOfRangeException(nameof(byteCount), $"{nameof(byteCount)} must be greater than zero. To read the length from data use an overload.");

                byteCount = DeserializeOp();

                if (byteCount <= 0)
                    return result;
            }

            EnsureCapacity(byteCount, SerializerOperation.Deserialize);

#if !NETSTANDARD1_0
            fixed (byte* src = buffer)
                result = Encoding.GetChars(src + position, byteCount, chars, charCount);
#else
            var charArray = new char[charCount];
            result = Encoding.GetChars(buffer, position, byteCount, charArray, 0);

            fixed (char* charPtr = charArray)
                Utilities.Memory.Wstrcpy(charPtr, chars, charCount);
#endif

            position += byteCount;

            return result;
        }

        public bool TryDeserializeChars([NotNullWhen(true)] out char[]? value)
        {
            value = default;
            var currentPosition = position;

            try
            {
                value = DeserializeNullableChars();

                if (value == default)
                {
                    position -= 1;
                    return false;
                }

                return true;
            }
            catch
            {
                position = currentPosition;
                return false;
            }
        }

        public bool TryDeserializeNullableChars(out char[]? value)
        {
            value = default;
            var currentPosition = position;

            try
            {
                value = DeserializeNullableChars();
                return true;
            }
            catch
            {
                position = currentPosition;
                return false;
            }
        }

        public bool TryDeserializeChars([NotNullWhen(true)] out char[]? value, int count)
        {
            value = default;

            if (count <= 0)
                return false;

            if (length - position < count)
                return false;

            var currentPosition = position;

            try
            {
                value = DeserializeChars(count);
                return true;
            }
            catch
            {
                position = currentPosition;
                return false;
            }
        }

        public bool TryDeserializeNullableChars(out char[]? value, int count)
        {
            value = default;

            if (count <= 0)
                return false;

            if (length - position < count)
                return false;

            var currentPosition = position;

            try
            {
                value = DeserializeNullableChars(count);
                return true;
            }
            catch
            {
                position = currentPosition;
                return false;
            }
        }

        public char[] ToCharArray()
        {
            position = 0;
            return DeserializeRawChars();
        }

        public char[]? ToNullableCharArray()
        {
            position = 0;
            return DeserializeNullableRawChars();
        }
    }
}