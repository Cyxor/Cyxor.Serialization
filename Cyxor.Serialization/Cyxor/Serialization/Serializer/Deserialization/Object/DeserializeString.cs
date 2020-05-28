using System;
using System.Diagnostics.CodeAnalysis;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        public string DeserializeString()
        {
            if (AutoRaw)
                return DeserializeRawString();

            var count = InternalDeserializeSequenceHeader();

            return count == -1 ? throw new InvalidOperationException(Utilities.ResourceStrings.NullReferenceFoundWhenDeserializingNonNullableReference(typeof(string).Name))
                : count == 0 ? string.Empty
                : DeserializeString(count);
        }

        public string? DeserializeNullableString()
        {
            if (AutoRaw)
                return DeserializeNullableStringRaw();

            var count = InternalDeserializeSequenceHeader();

            return count == -1 ? default
                : count == 0 ? string.Empty
                : DeserializeNullableString(count);
        }

        public string DeserializeRawString()
            => DeserializeString(_length - _position);

        public string? DeserializeNullableStringRaw()
            => DeserializeNullableString(_length - _position);

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

            InternalEnsureDeserializeCapacity(byteCount);

            _position += byteCount;

            return System.Text.Encoding.UTF8.GetString(_buffer!, _position - byteCount, byteCount);
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

            InternalEnsureDeserializeCapacity(byteCount);

            _position += byteCount;

            return System.Text.Encoding.UTF8.GetString(_buffer!, _position - byteCount, byteCount);
            //return System.Text.Encoding.Unicode.GetString(buffer!, position - byteCount, byteCount);
        }

        public bool TryDeserializeString([NotNullWhen(true)] out string? value)
        {
            value = default;
            var currentPosition = _position;

            try
            {
                value = DeserializeNullableString();

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

        public bool TryDeserializeString([NotNullWhen(true)] out string? value, int count)
        {
            value = default;

            if (count <= 0)
                return false;

            if (_length - _position < count)
                return false;

            var currentPosition = _position;

            try
            {
                value = DeserializeString(count);
                return true;
            }
            catch
            {
                _position = currentPosition;
                return false;
            }
        }

        public bool TryDeserializeNullableString(out string? value)
        {
            value = default;
            var currentPosition = _position;

            try
            {
                value = DeserializeNullableString();
                return true;
            }
            catch
            {
                _position = currentPosition;
                return false;
            }
        }

        public bool TryDeserializeNullableString(out string? value, int count)
        {
            value = default;

            if (count <= 0)
                return false;

            if (_length - _position < count)
                return false;

            var currentPosition = _position;

            try
            {
                value = DeserializeNullableString(count);
                return true;
            }
            catch
            {
                _position = currentPosition;
                return false;
            }
        }

        public override string ToString()
        {
            _position = 0;
            return DeserializeRawString();
        }

        public string? ToNullableString()
        {
            _position = 0;
            return DeserializeNullableStringRaw();
        }
    }
}