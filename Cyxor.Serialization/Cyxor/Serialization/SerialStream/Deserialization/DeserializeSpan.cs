#if !NET20 && !NET35 && !NET40 && !NETSTANDARD1_0

using System;

namespace Cyxor.Serialization
{
    using Extensions;

    partial class Serializer
    {
        public Span<T> DeserializeSpan<T>() where T: unmanaged
        {
            if (AutoRaw)
                return DeserializeRawSpan<T>();

            var count = DeserializeOp();

            return count == -1 ? throw new InvalidOperationException
                (Utilities.ResourceStrings.NullReferenceFoundWhenDeserializingValueType(typeof(Span<T>).Name))
                : count == 0 ? Span<T>.Empty
                : DeserializeSpan<T>(count);
        }

        public Span<T> DeserializeRawSpan<T>() where T: unmanaged
            => DeserializeSpan<T>(length - position);

        public ref Span<T> DeserializeRawSpan<T>(ref Span<T> span) where T : unmanaged
            => ref DeserializeSpan(ref span, length - position);

        public Span<T> DeserializeSpan<T>(int bytesCount) where T: unmanaged
        {
            var span = new Span<byte>(new byte[bytesCount]);
            _ = DeserializeSpan(ref span, bytesCount);
            return span.Cast<byte, T>();
        }

        public ref Span<T> DeserializeSpan<T>(ref Span<T> span) where T: unmanaged
        {
            var count = DeserializeOp();

            if (count == -1)
                throw new InvalidOperationException
                    (Utilities.ResourceStrings.NullReferenceFoundWhenDeserializingValueType(typeof(Span<T>).Name));

            return ref count == 0 ? ref span : ref DeserializeSpan(ref span, count);
        }

        public ref Span<T> DeserializeSpan<T>(ref Span<T> span, int bytesCount) where T: unmanaged
        {
            if (bytesCount < 0)
                throw new ArgumentOutOfRangeException(nameof(bytesCount), $"Parameter {nameof(bytesCount)} must be a positive value");

            if (bytesCount > span.Length)
                throw new ArgumentOutOfRangeException(nameof(bytesCount));

            if (bytesCount == 0)
                return ref span;

            EnsureCapacity(bytesCount, SerializerOperation.Deserialize);

            var bufferSpan = new Span<byte>(buffer, position, bytesCount);
            var spanOfT = bufferSpan.Cast<byte, T>();

            spanOfT.CopyTo(span);
            position += bytesCount;

            return ref span;
        }

        public bool TryDeserializeSpan<T>(out Span<T> value) where T: unmanaged
        {
            value = Span<T>.Empty;

            var currentPosition = position;

            try
            {
                value = DeserializeSpan<T>();
                return true;
            }
            catch
            {
                position = currentPosition;
                return false;
            }
        }

        public bool TryDeserializeSpan<T>(out Span<T> value, int bytesCount) where T: unmanaged
        {
            value = Span<T>.Empty;

            if (bytesCount <= 0)
                return false;

            if (length - position < bytesCount)
                return false;

            var currentPosition = position;

            try
            {
                value = DeserializeSpan<T>(bytesCount);
                return true;
            }
            catch
            {
                position = currentPosition;
                return false;
            }
        }

        public Span<T> ToSpan<T>() where T : unmanaged
        {
            position = 0;
            return DeserializeRawSpan<T>();
        }

        public ReadOnlySpan<T> DeserializeReadOnlySpan<T>() where T : unmanaged
        {
            if (AutoRaw)
                return DeserializeRawReadOnlySpan<T>();

            var count = DeserializeOp();

            return count == -1 ? throw new InvalidOperationException
                (Utilities.ResourceStrings.NullReferenceFoundWhenDeserializingValueType(typeof(ReadOnlySpan<T>).Name))
                : count == 0 ? ReadOnlySpan<T>.Empty
                : DeserializeReadOnlySpan<T>(count);
        }

        public ReadOnlySpan<T> DeserializeRawReadOnlySpan<T>() where T : unmanaged
            => DeserializeReadOnlySpan<T>(length - position);

        public ReadOnlySpan<T> DeserializeReadOnlySpan<T>(int bytesCount) where T : unmanaged
        {
            if (bytesCount < 0)
                throw new ArgumentOutOfRangeException(nameof(bytesCount), $"Parameter {nameof(bytesCount)} must be a positive value");

            if (bytesCount == 0)
                return ReadOnlySpan<T>.Empty;

            EnsureCapacity(bytesCount, SerializerOperation.Deserialize);

            var bufferReadOnlySpan = new ReadOnlySpan<byte>(buffer, position, bytesCount);

            position += bytesCount;

            return bufferReadOnlySpan.Cast<byte, T>();
        }

        public bool TryDeserializeReadOnlySpan<T>(out ReadOnlySpan<T> value) where T : unmanaged
        {
            value = ReadOnlySpan<T>.Empty;

            var currentPosition = position;

            try
            {
                value = DeserializeReadOnlySpan<T>();
                return true;
            }
            catch
            {
                position = currentPosition;
                return false;
            }
        }

        public bool TryDeserializeReadOnlySpan<T>(out ReadOnlySpan<T> value, int bytesCount) where T : unmanaged
        {
            value = ReadOnlySpan<T>.Empty;

            if (bytesCount <= 0)
                return false;

            if (length - position < bytesCount)
                return false;

            var currentPosition = position;

            try
            {
                value = DeserializeReadOnlySpan<T>(bytesCount);
                return true;
            }
            catch
            {
                position = currentPosition;
                return false;
            }
        }

        public ReadOnlySpan<T> ToReadOnlySpan<T>() where T : unmanaged
        {
            position = 0;
            return DeserializeRawReadOnlySpan<T>();
        }
    }
}

#endif