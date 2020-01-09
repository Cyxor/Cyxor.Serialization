using System;
using System.Diagnostics.CodeAnalysis;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        public string DeserializeString()
        {
            if (AutoRaw)
                return DeserializeStringRaw();

            var count = DeserializeOp();

            return count == -1 ? throw new InvalidOperationException(Utilities.ResourceStrings.NullReferenceFoundWhenDeserializingNonNullableReference(typeof(string).Name))
                : count == 0 ? string.Empty
                : DeserializeString(count);
        }

        public string? DeserializeNullableString()
        {
            if (AutoRaw)
                return DeserializeNullableStringRaw();

            var count = DeserializeOp();

            return count == -1 ? default
                : count == 0 ? string.Empty
                : DeserializeNullableString(count);
        }

        public string DeserializeStringRaw()
            => DeserializeString(length - position);

        public string? DeserializeNullableStringRaw()
            => DeserializeNullableString(length - position);

        /// <summary>
        /// Deserialize a string from the specified number of bytes
        /// </summary>
        /// <param name="byteCount">The number of bytes to decode</param>
        /// <returns>The deserialized string</returns>
        public string DeserializeString(int byteCount)
        {
            if (byteCount == 0)
                return string.Empty;

            if (byteCount < 0)
                throw new ArgumentOutOfRangeException(nameof(byteCount), $"Parameter {nameof(byteCount)} must be a positive value");

            EnsureCapacity(byteCount, SerializerOperation.Deserialize);

            position += byteCount;

            return Encoding.GetString(buffer!, position - byteCount, byteCount);
            //return System.Text.Encoding.Unicode.GetString(buffer!, position - byteCount, byteCount);
        }

        /// <summary>
        /// Deserialize a <c>string?</c> from the specified number of bytes
        /// </summary>
        /// <param name="byteCount">The number of bytes to decode</param>
        /// <returns>The deserialized <c>string?</c></returns>
        public string? DeserializeNullableString(int byteCount)
        {
            if (byteCount == 0)
                return default;

            if (byteCount < 0)
                throw new ArgumentOutOfRangeException(nameof(byteCount), $"Parameter {nameof(byteCount)} must be a positive value");

            EnsureCapacity(byteCount, SerializerOperation.Deserialize);

            position += byteCount;

            return Encoding.GetString(buffer!, position - byteCount, byteCount);
            //return System.Text.Encoding.Unicode.GetString(buffer!, position - byteCount, byteCount);
        }

        public bool TryDeserializeString([NotNullWhen(true)] out string? value)
        {
            value = default;
            var currentPosition = position;

            try
            {
                value = DeserializeNullableString();

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

        public bool TryDeserializeString([NotNullWhen(true)] out string? value, int count)
        {
            value = default;

            if (count <= 0)
                return false;

            if (length - position < count)
                return false;

            var currentPosition = position;

            try
            {
                value = DeserializeString(count);
                return true;
            }
            catch
            {
                position = currentPosition;
                return false;
            }
        }

        public bool TryDeserializeNullableString(out string? value)
        {
            value = default;
            var currentPosition = position;

            try
            {
                value = DeserializeNullableString();
                return true;
            }
            catch
            {
                position = currentPosition;
                return false;
            }
        }

        public bool TryDeserializeNullableString(out string? value, int count)
        {
            value = default;

            if (count <= 0)
                return false;

            if (length - position < count)
                return false;

            var currentPosition = position;

            try
            {
                value = DeserializeNullableString(count);
                return true;
            }
            catch
            {
                position = currentPosition;
                return false;
            }
        }

        public override string ToString()
        {
            position = 0;
            return DeserializeStringRaw();
        }

        public string? ToNullableString()
        {
            position = 0;
            return DeserializeNullableStringRaw();
        }
    }
}